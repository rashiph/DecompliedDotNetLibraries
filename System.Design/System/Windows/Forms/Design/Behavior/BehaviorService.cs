namespace System.Windows.Forms.Design.Behavior
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public sealed class BehaviorService : IDisposable
    {
        private System.Windows.Forms.Design.DesignerActionUI actionPointer;
        private BehaviorServiceAdornerCollection adorners;
        private AdornerWindow adornerWindow;
        private int adornerWindowIndex = -1;
        private ArrayList behaviorStack;
        private bool cancelDrag;
        private System.Windows.Forms.Design.Behavior.Behavior captureBehavior;
        private static TraceSwitch dragDropSwitch = new TraceSwitch("BSDRAGDROP", "Behavior service drag & drop messages");
        private Hashtable dragEnterReplies;
        private bool dragging;
        private Control dropSource;
        private Glyph hitTestedGlyph;
        private MenuCommandHandler menuCommandHandler;
        private bool queriedSnapLines;
        private IServiceProvider serviceProvider;
        private string[] testHook_RecentSnapLines;
        private const string ToolboxFormat = ".NET Toolbox Item";
        private IToolboxService toolboxSvc;
        private bool trackingMouseEvent;
        private System.Design.NativeMethods.TRACKMOUSEEVENT trackMouseEvent;
        private bool useSnapLines;
        private DragEventArgs validDragArgs;
        private static int WM_GETALLSNAPLINES;
        private static int WM_GETRECENTSNAPLINES;

        public event BehaviorDragDropEventHandler BeginDrag;

        public event BehaviorDragDropEventHandler EndDrag;

        public event EventHandler Synchronize;

        internal BehaviorService(IServiceProvider serviceProvider, Control windowFrame)
        {
            this.serviceProvider = serviceProvider;
            this.adornerWindow = new AdornerWindow(this, windowFrame);
            IOverlayService service = (IOverlayService) serviceProvider.GetService(typeof(IOverlayService));
            if (service != null)
            {
                this.adornerWindowIndex = service.PushOverlay(this.adornerWindow);
            }
            this.dragEnterReplies = new Hashtable();
            this.adorners = new BehaviorServiceAdornerCollection(this);
            this.behaviorStack = new ArrayList();
            this.hitTestedGlyph = null;
            this.validDragArgs = null;
            this.actionPointer = null;
            this.trackMouseEvent = null;
            this.trackingMouseEvent = false;
            IMenuCommandService menuService = serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((menuService != null) && (host != null))
            {
                this.menuCommandHandler = new MenuCommandHandler(this, menuService);
                host.RemoveService(typeof(IMenuCommandService));
                host.AddService(typeof(IMenuCommandService), this.menuCommandHandler);
            }
            this.useSnapLines = false;
            this.queriedSnapLines = false;
            WM_GETALLSNAPLINES = System.Design.SafeNativeMethods.RegisterWindowMessage("WM_GETALLSNAPLINES");
            WM_GETRECENTSNAPLINES = System.Design.SafeNativeMethods.RegisterWindowMessage("WM_GETRECENTSNAPLINES");
            SystemEvents.DisplaySettingsChanged += new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.InstalledFontsChanged += new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        public Point AdornerWindowPointToScreen(Point p)
        {
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(p.X, p.Y);
            System.Design.NativeMethods.MapWindowPoints(this.adornerWindow.Handle, IntPtr.Zero, pt, 1);
            return new Point(pt.x, pt.y);
        }

        public Point AdornerWindowToScreen()
        {
            Point p = new Point(0, 0);
            return this.AdornerWindowPointToScreen(p);
        }

        public Rectangle ControlRectInAdornerWindow(Control c)
        {
            if (c.Parent == null)
            {
                return Rectangle.Empty;
            }
            return new Rectangle(this.ControlToAdornerWindow(c), c.Size);
        }

        public Point ControlToAdornerWindow(Control c)
        {
            if (c.Parent == null)
            {
                return Point.Empty;
            }
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                x = c.Left,
                y = c.Top
            };
            System.Design.NativeMethods.MapWindowPoints(c.Parent.Handle, this.adornerWindow.Handle, pt, 1);
            if (c.Parent.IsMirrored)
            {
                pt.x -= c.Width;
            }
            return new Point(pt.x, pt.y);
        }

        public void Dispose()
        {
            IOverlayService service = (IOverlayService) this.serviceProvider.GetService(typeof(IOverlayService));
            if (service != null)
            {
                service.RemoveOverlay(this.adornerWindow);
            }
            if (this.dropSource != null)
            {
                this.dropSource.Dispose();
            }
            IMenuCommandService service2 = this.serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            MenuCommandHandler handler = null;
            if (service2 != null)
            {
                handler = service2 as MenuCommandHandler;
            }
            if ((handler != null) && (host != null))
            {
                IMenuCommandService menuService = handler.MenuService;
                host.RemoveService(typeof(IMenuCommandService));
                host.AddService(typeof(IMenuCommandService), menuService);
            }
            this.adornerWindow.Dispose();
            SystemEvents.DisplaySettingsChanged -= new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.InstalledFontsChanged -= new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        internal DragDropEffects DoDragDrop(DropSourceBehavior dropSourceBehavior)
        {
            this.DropSource.QueryContinueDrag += new QueryContinueDragEventHandler(dropSourceBehavior.QueryContinueDrag);
            this.DropSource.GiveFeedback += new GiveFeedbackEventHandler(dropSourceBehavior.GiveFeedback);
            DragDropEffects none = DragDropEffects.None;
            BehaviorDragDropEventArgs e = new BehaviorDragDropEventArgs(((DropSourceBehavior.BehaviorDataObject) dropSourceBehavior.DataObject).DragComponents);
            try
            {
                try
                {
                    this.OnBeginDrag(e);
                    this.dragging = true;
                    this.cancelDrag = false;
                    this.dragEnterReplies.Clear();
                    none = this.DropSource.DoDragDrop(dropSourceBehavior.DataObject, dropSourceBehavior.AllowedEffects);
                }
                finally
                {
                    this.DropSource.QueryContinueDrag -= new QueryContinueDragEventHandler(dropSourceBehavior.QueryContinueDrag);
                    this.DropSource.GiveFeedback -= new GiveFeedbackEventHandler(dropSourceBehavior.GiveFeedback);
                    this.EndDragNotification();
                    this.validDragArgs = null;
                    this.dragging = false;
                    this.cancelDrag = false;
                    this.OnEndDrag(e);
                }
                return none;
            }
            catch (CheckoutException exception)
            {
                if (exception != CheckoutException.Canceled)
                {
                    throw;
                }
                return DragDropEffects.None;
            }
            finally
            {
                if (dropSourceBehavior != null)
                {
                    dropSourceBehavior.CleanupDrag();
                }
            }
            return none;
        }

        internal void EnableAllAdorners(bool enabled)
        {
            using (BehaviorServiceAdornerCollectionEnumerator enumerator = this.Adorners.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.EnabledInternal = enabled;
                }
            }
            this.Invalidate();
        }

        internal void EndDragNotification()
        {
            this.adornerWindow.EndDragNotification();
        }

        private MenuCommand FindCommand(CommandID commandID, IMenuCommandService menuService)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior != null)
            {
                if (appropriateBehavior.DisableAllCommands)
                {
                    MenuCommand command = menuService.FindCommand(commandID);
                    if (command != null)
                    {
                        command.Enabled = false;
                    }
                    return command;
                }
                MenuCommand command2 = appropriateBehavior.FindCommand(commandID);
                if (command2 != null)
                {
                    return command2;
                }
            }
            return menuService.FindCommand(commandID);
        }

        private System.Windows.Forms.Design.Behavior.Behavior GetAppropriateBehavior(Glyph g)
        {
            if ((this.behaviorStack != null) && (this.behaviorStack.Count > 0))
            {
                return (this.behaviorStack[0] as System.Windows.Forms.Design.Behavior.Behavior);
            }
            if ((g != null) && (g.Behavior != null))
            {
                return g.Behavior;
            }
            return null;
        }

        internal Glyph[] GetIntersectingGlyphs(Glyph primaryGlyph)
        {
            if (primaryGlyph == null)
            {
                return new Glyph[0];
            }
            Rectangle bounds = primaryGlyph.Bounds;
            ArrayList list = new ArrayList();
            for (int i = this.adorners.Count - 1; i >= 0; i--)
            {
                if (this.adorners[i].Enabled)
                {
                    for (int j = 0; j < this.adorners[i].Glyphs.Count; j++)
                    {
                        Glyph glyph = this.adorners[i].Glyphs[j];
                        if (bounds.IntersectsWith(glyph.Bounds))
                        {
                            list.Add(glyph);
                        }
                    }
                }
            }
            if (list.Count == 0)
            {
                return new Glyph[0];
            }
            return (Glyph[]) list.ToArray(typeof(Glyph));
        }

        public System.Windows.Forms.Design.Behavior.Behavior GetNextBehavior(System.Windows.Forms.Design.Behavior.Behavior behavior)
        {
            if ((this.behaviorStack != null) && (this.behaviorStack.Count > 0))
            {
                int index = this.behaviorStack.IndexOf(behavior);
                if ((index != -1) && (index < (this.behaviorStack.Count - 1)))
                {
                    return (this.behaviorStack[index + 1] as System.Windows.Forms.Design.Behavior.Behavior);
                }
            }
            return null;
        }

        private void HookMouseEvent()
        {
            if (!this.trackingMouseEvent)
            {
                this.trackingMouseEvent = true;
                if (this.trackMouseEvent == null)
                {
                    this.trackMouseEvent = new System.Design.NativeMethods.TRACKMOUSEEVENT();
                    this.trackMouseEvent.dwFlags = System.Design.NativeMethods.TME_HOVER;
                    this.trackMouseEvent.hwndTrack = this.adornerWindow.Handle;
                }
                System.Design.SafeNativeMethods.TrackMouseEvent(this.trackMouseEvent);
            }
        }

        public void Invalidate()
        {
            this.adornerWindow.InvalidateAdornerWindow();
        }

        public void Invalidate(Rectangle rect)
        {
            this.adornerWindow.InvalidateAdornerWindow(rect);
        }

        public void Invalidate(Region r)
        {
            this.adornerWindow.InvalidateAdornerWindow(r);
        }

        private void InvokeMouseEnterLeave(Glyph leaveGlyph, Glyph enterGlyph)
        {
            if (leaveGlyph != null)
            {
                if ((enterGlyph != null) && leaveGlyph.Equals(enterGlyph))
                {
                    return;
                }
                if (this.validDragArgs != null)
                {
                    this.OnDragLeave(leaveGlyph, EventArgs.Empty);
                }
                else
                {
                    this.OnMouseLeave(leaveGlyph);
                }
            }
            if (enterGlyph != null)
            {
                if (this.validDragArgs != null)
                {
                    this.OnDragEnter(enterGlyph, this.validDragArgs);
                }
                else
                {
                    this.OnMouseEnter(enterGlyph);
                }
            }
        }

        public Point MapAdornerWindowPoint(IntPtr handle, Point pt)
        {
            System.Design.NativeMethods.POINT point = new System.Design.NativeMethods.POINT {
                x = pt.X,
                y = pt.Y
            };
            System.Design.NativeMethods.MapWindowPoints(handle, this.adornerWindow.Handle, point, 1);
            return new Point(point.x, point.y);
        }

        private void OnBeginDrag(BehaviorDragDropEventArgs e)
        {
            if (this.beginDragHandler != null)
            {
                this.beginDragHandler(this, e);
            }
        }

        private void OnDragDrop(DragEventArgs e)
        {
            this.validDragArgs = null;
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior != null)
            {
                appropriateBehavior.OnDragDrop(this.hitTestedGlyph, e);
            }
        }

        private void OnDragEnter(Glyph g, DragEventArgs e)
        {
            if (g == null)
            {
                g = this.hitTestedGlyph;
            }
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(g);
            if (appropriateBehavior != null)
            {
                appropriateBehavior.OnDragEnter(g, e);
                if (((g != null) && (g is ControlBodyGlyph)) && (e.Effect == DragDropEffects.None))
                {
                    this.dragEnterReplies[g] = this;
                }
            }
        }

        private void OnDragLeave(Glyph g, EventArgs e)
        {
            this.dragEnterReplies.Clear();
            if (g == null)
            {
                g = this.hitTestedGlyph;
            }
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(g);
            if (appropriateBehavior != null)
            {
                appropriateBehavior.OnDragLeave(g, e);
            }
        }

        private void OnDragOver(DragEventArgs e)
        {
            this.validDragArgs = e;
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else if ((this.hitTestedGlyph == null) || ((this.hitTestedGlyph != null) && !this.dragEnterReplies.ContainsKey(this.hitTestedGlyph)))
            {
                appropriateBehavior.OnDragOver(this.hitTestedGlyph, e);
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void OnEndDrag(BehaviorDragDropEventArgs e)
        {
            if (this.endDragHandler != null)
            {
                this.endDragHandler(this, e);
            }
        }

        private void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior != null)
            {
                appropriateBehavior.OnGiveFeedback(this.hitTestedGlyph, e);
            }
        }

        internal void OnLoseCapture()
        {
            if (this.captureBehavior != null)
            {
                System.Windows.Forms.Design.Behavior.Behavior captureBehavior = this.captureBehavior;
                this.captureBehavior = null;
                try
                {
                    captureBehavior.OnLoseCapture(this.hitTestedGlyph, EventArgs.Empty);
                }
                catch
                {
                }
            }
        }

        private bool OnMouseDoubleClick(MouseButtons button, Point mouseLoc)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseDoubleClick(this.hitTestedGlyph, button, mouseLoc);
        }

        private bool OnMouseDown(MouseButtons button, Point mouseLoc)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseDown(this.hitTestedGlyph, button, mouseLoc);
        }

        private bool OnMouseEnter(Glyph g)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(g);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseEnter(g);
        }

        private bool OnMouseHover(Point mouseLoc)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseHover(this.hitTestedGlyph, mouseLoc);
        }

        private bool OnMouseLeave(Glyph g)
        {
            this.UnHookMouseEvent();
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(g);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseLeave(g);
        }

        private bool OnMouseMove(MouseButtons button, Point mouseLoc)
        {
            this.HookMouseEvent();
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseMove(this.hitTestedGlyph, button, mouseLoc);
        }

        private bool OnMouseUp(MouseButtons button)
        {
            this.dragEnterReplies.Clear();
            this.validDragArgs = null;
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior == null)
            {
                return false;
            }
            return appropriateBehavior.OnMouseUp(this.hitTestedGlyph, button);
        }

        private void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            System.Windows.Forms.Design.Behavior.Behavior appropriateBehavior = this.GetAppropriateBehavior(this.hitTestedGlyph);
            if (appropriateBehavior != null)
            {
                appropriateBehavior.OnQueryContinueDrag(this.hitTestedGlyph, e);
            }
        }

        private void OnSystemSettingChanged(object sender, EventArgs e)
        {
            this.SyncSelection();
            DesignerUtils.SyncBrushes();
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            this.SyncSelection();
            DesignerUtils.SyncBrushes();
        }

        public System.Windows.Forms.Design.Behavior.Behavior PopBehavior(System.Windows.Forms.Design.Behavior.Behavior behavior)
        {
            if (this.behaviorStack.Count == 0)
            {
                throw new InvalidOperationException();
            }
            int index = this.behaviorStack.IndexOf(behavior);
            if (index == -1)
            {
                return null;
            }
            this.behaviorStack.RemoveAt(index);
            if (behavior == this.captureBehavior)
            {
                this.adornerWindow.Capture = false;
                if (this.captureBehavior != null)
                {
                    this.OnLoseCapture();
                }
            }
            return behavior;
        }

        internal void ProcessPaintMessage(Rectangle paintRect)
        {
            this.adornerWindow.Invalidate(paintRect);
        }

        private bool PropagateHitTest(Point pt)
        {
            for (int i = this.adorners.Count - 1; i >= 0; i--)
            {
                if (this.adorners[i].Enabled)
                {
                    for (int j = 0; j < this.adorners[i].Glyphs.Count; j++)
                    {
                        Cursor hitTest = this.adorners[i].Glyphs[j].GetHitTest(pt);
                        if (hitTest != null)
                        {
                            Glyph enterGlyph = this.adorners[i].Glyphs[j];
                            this.InvokeMouseEnterLeave(this.hitTestedGlyph, enterGlyph);
                            if (this.validDragArgs == null)
                            {
                                this.SetAppropriateCursor(hitTest);
                            }
                            this.hitTestedGlyph = enterGlyph;
                            return (this.hitTestedGlyph.Behavior is ControlDesigner.TransparentBehavior);
                        }
                    }
                }
            }
            this.InvokeMouseEnterLeave(this.hitTestedGlyph, null);
            if (this.validDragArgs == null)
            {
                Cursor cursor = Cursors.Default;
                if ((this.behaviorStack != null) && (this.behaviorStack.Count > 0))
                {
                    System.Windows.Forms.Design.Behavior.Behavior behavior = this.behaviorStack[0] as System.Windows.Forms.Design.Behavior.Behavior;
                    if (behavior != null)
                    {
                        cursor = behavior.Cursor;
                    }
                }
                this.SetAppropriateCursor(cursor);
            }
            this.hitTestedGlyph = null;
            return true;
        }

        private void PropagatePaint(PaintEventArgs pe)
        {
            for (int i = 0; i < this.adorners.Count; i++)
            {
                if (this.adorners[i].Enabled)
                {
                    for (int j = this.adorners[i].Glyphs.Count - 1; j >= 0; j--)
                    {
                        this.adorners[i].Glyphs[j].Paint(pe);
                    }
                }
            }
        }

        public void PushBehavior(System.Windows.Forms.Design.Behavior.Behavior behavior)
        {
            if (behavior == null)
            {
                throw new ArgumentNullException("behavior");
            }
            this.behaviorStack.Insert(0, behavior);
            if ((this.captureBehavior != null) && (this.captureBehavior != behavior))
            {
                this.OnLoseCapture();
            }
        }

        public void PushCaptureBehavior(System.Windows.Forms.Design.Behavior.Behavior behavior)
        {
            this.PushBehavior(behavior);
            this.captureBehavior = behavior;
            this.adornerWindow.Capture = true;
            IUIService service = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
            if (service != null)
            {
                IWin32Window dialogOwnerWindow = service.GetDialogOwnerWindow();
                if (((dialogOwnerWindow != null) && (dialogOwnerWindow.Handle != IntPtr.Zero)) && (dialogOwnerWindow.Handle != System.Design.UnsafeNativeMethods.GetActiveWindow()))
                {
                    System.Design.UnsafeNativeMethods.SetActiveWindow(new HandleRef(this, dialogOwnerWindow.Handle));
                }
            }
        }

        public Point ScreenToAdornerWindow(Point p)
        {
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                x = p.X,
                y = p.Y
            };
            System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, this.adornerWindow.Handle, pt, 1);
            return new Point(pt.x, pt.y);
        }

        private void SetAppropriateCursor(Cursor cursor)
        {
            if (cursor == Cursors.Default)
            {
                if (this.toolboxSvc == null)
                {
                    this.toolboxSvc = (IToolboxService) this.serviceProvider.GetService(typeof(IToolboxService));
                }
                if ((this.toolboxSvc != null) && this.toolboxSvc.SetCursor())
                {
                    cursor = new Cursor(System.Design.NativeMethods.GetCursor());
                }
            }
            this.adornerWindow.Cursor = cursor;
        }

        private void ShowError(Exception ex)
        {
            IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                service.ShowError(ex);
            }
        }

        internal void StartDragNotification()
        {
            this.adornerWindow.StartDragNotification();
        }

        public void SyncSelection()
        {
            if (this.synchronizeEventHandler != null)
            {
                this.synchronizeEventHandler(this, EventArgs.Empty);
            }
        }

        private void TestHook_GetAllSnapLines(ref Message m)
        {
            string text = "";
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service != null)
            {
                foreach (Component component in service.Container.Components)
                {
                    if (component is Control)
                    {
                        ControlDesigner designer = service.GetDesigner(component) as ControlDesigner;
                        if (designer != null)
                        {
                            foreach (SnapLine line in designer.SnapLines)
                            {
                                string str2 = text;
                                text = str2 + line.ToString() + "\tAssociated Control = " + designer.Control.Name + ":::";
                            }
                        }
                    }
                }
                this.TestHook_SetText(ref m, text);
            }
        }

        private void TestHook_GetRecentSnapLines(ref Message m)
        {
            string text = "";
            if (this.testHook_RecentSnapLines != null)
            {
                foreach (string str2 in this.testHook_RecentSnapLines)
                {
                    text = text + str2 + "\n";
                }
            }
            this.TestHook_SetText(ref m, text);
        }

        private void TestHook_SetText(ref Message m, string text)
        {
            if (m.LParam == IntPtr.Zero)
            {
                m.Result = (IntPtr) ((text.Length + 1) * Marshal.SystemDefaultCharSize);
            }
            else if (((int) ((long) m.WParam)) < (text.Length + 1))
            {
                m.Result = (IntPtr) (-1);
            }
            else
            {
                byte[] buffer;
                byte[] bytes;
                char[] chars = new char[1];
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    bytes = Encoding.Default.GetBytes(text);
                    buffer = Encoding.Default.GetBytes(chars);
                }
                else
                {
                    bytes = Encoding.Unicode.GetBytes(text);
                    buffer = Encoding.Unicode.GetBytes(chars);
                }
                Marshal.Copy(bytes, 0, m.LParam, bytes.Length);
                Marshal.Copy(buffer, 0, (IntPtr) (((long) m.LParam) + bytes.Length), buffer.Length);
                m.Result = (IntPtr) ((bytes.Length + buffer.Length) / Marshal.SystemDefaultCharSize);
            }
        }

        private void UnHookMouseEvent()
        {
            this.trackingMouseEvent = false;
        }

        public BehaviorServiceAdornerCollection Adorners
        {
            get
            {
                return this.adorners;
            }
        }

        internal Control AdornerWindowControl
        {
            get
            {
                return this.adornerWindow;
            }
        }

        public Graphics AdornerWindowGraphics
        {
            get
            {
                Graphics graphics = this.adornerWindow.CreateGraphics();
                graphics.Clip = new Region(this.adornerWindow.DesignerFrameDisplayRectangle);
                return graphics;
            }
        }

        internal int AdornerWindowIndex
        {
            get
            {
                return this.adornerWindowIndex;
            }
        }

        internal bool CancelDrag
        {
            get
            {
                return this.cancelDrag;
            }
            set
            {
                this.cancelDrag = value;
            }
        }

        public System.Windows.Forms.Design.Behavior.Behavior CurrentBehavior
        {
            get
            {
                if ((this.behaviorStack != null) && (this.behaviorStack.Count > 0))
                {
                    return (this.behaviorStack[0] as System.Windows.Forms.Design.Behavior.Behavior);
                }
                return null;
            }
        }

        internal System.Windows.Forms.Design.DesignerActionUI DesignerActionUI
        {
            get
            {
                return this.actionPointer;
            }
            set
            {
                this.actionPointer = value;
            }
        }

        internal bool Dragging
        {
            get
            {
                return this.dragging;
            }
        }

        private Control DropSource
        {
            get
            {
                if (this.dropSource == null)
                {
                    this.dropSource = new Control();
                }
                return this.dropSource;
            }
        }

        internal bool HasCapture
        {
            get
            {
                return (this.captureBehavior != null);
            }
        }

        internal bool IsDisposed
        {
            get
            {
                if (this.adornerWindow != null)
                {
                    return this.adornerWindow.IsDisposed;
                }
                return true;
            }
        }

        internal string[] RecentSnapLines
        {
            set
            {
                this.testHook_RecentSnapLines = value;
            }
        }

        internal bool UseSnapLines
        {
            get
            {
                if (!this.queriedSnapLines)
                {
                    this.queriedSnapLines = true;
                    this.useSnapLines = DesignerUtils.UseSnapLines(this.serviceProvider);
                }
                return this.useSnapLines;
            }
        }

        private class AdornerWindow : Control
        {
            private static List<BehaviorService.AdornerWindow> AdornerWindowList = new List<BehaviorService.AdornerWindow>();
            private BehaviorService behaviorService;
            private Control designerFrame;
            private static MouseHook mouseHook;
            private bool processingDrag;

            internal AdornerWindow(BehaviorService behaviorService, Control designerFrame)
            {
                this.behaviorService = behaviorService;
                this.designerFrame = designerFrame;
                this.Dock = DockStyle.Fill;
                this.AllowDrop = true;
                this.Text = "AdornerWindow";
                base.SetStyle(ControlStyles.Opaque, true);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.designerFrame != null))
                {
                    this.designerFrame = null;
                }
                base.Dispose(disposing);
            }

            internal void EndDragNotification()
            {
                this.ProcessingDrag = false;
            }

            internal void InvalidateAdornerWindow()
            {
                if (this.DesignerFrameValid)
                {
                    this.designerFrame.Invalidate(true);
                    this.designerFrame.Update();
                }
            }

            internal void InvalidateAdornerWindow(Rectangle rectangle)
            {
                if (this.DesignerFrameValid)
                {
                    Point autoScrollPosition = ((System.Windows.Forms.Design.DesignerFrame) this.designerFrame).AutoScrollPosition;
                    rectangle.Offset(autoScrollPosition.X, autoScrollPosition.Y);
                    this.designerFrame.Invalidate(rectangle, true);
                    this.designerFrame.Update();
                }
            }

            internal void InvalidateAdornerWindow(Region region)
            {
                if (this.DesignerFrameValid)
                {
                    Point autoScrollPosition = ((System.Windows.Forms.Design.DesignerFrame) this.designerFrame).AutoScrollPosition;
                    region.Translate(autoScrollPosition.X, autoScrollPosition.Y);
                    this.designerFrame.Invalidate(region, true);
                    this.designerFrame.Update();
                }
            }

            private static bool IsLocalDrag(DragEventArgs e)
            {
                if (e.Data is DropSourceBehavior.BehaviorDataObject)
                {
                    return true;
                }
                string[] formats = e.Data.GetFormats();
                for (int i = 0; i < formats.Length; i++)
                {
                    if ((formats[i].Length == ".NET Toolbox Item".Length) && string.Equals(".NET Toolbox Item", formats[i]))
                    {
                        return true;
                    }
                }
                return false;
            }

            protected override void OnDragDrop(DragEventArgs e)
            {
                try
                {
                    this.behaviorService.OnDragDrop(e);
                }
                finally
                {
                    this.ProcessingDrag = false;
                }
            }

            protected override void OnDragEnter(DragEventArgs e)
            {
                this.ProcessingDrag = true;
                if (!IsLocalDrag(e))
                {
                    this.behaviorService.validDragArgs = e;
                    System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT();
                    System.Design.NativeMethods.GetCursorPos(pt);
                    System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, base.Handle, pt, 1);
                    Point point2 = new Point(pt.x, pt.y);
                    this.behaviorService.PropagateHitTest(point2);
                }
                this.behaviorService.OnDragEnter(null, e);
            }

            protected override void OnDragLeave(EventArgs e)
            {
                this.behaviorService.validDragArgs = null;
                try
                {
                    this.behaviorService.OnDragLeave(null, e);
                }
                finally
                {
                    this.ProcessingDrag = false;
                }
            }

            protected override void OnDragOver(DragEventArgs e)
            {
                this.ProcessingDrag = true;
                if (!IsLocalDrag(e))
                {
                    this.behaviorService.validDragArgs = e;
                    System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT();
                    System.Design.NativeMethods.GetCursorPos(pt);
                    System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, base.Handle, pt, 1);
                    Point point2 = new Point(pt.x, pt.y);
                    this.behaviorService.PropagateHitTest(point2);
                }
                this.behaviorService.OnDragOver(e);
            }

            protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
            {
                this.behaviorService.OnGiveFeedback(e);
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                AdornerWindowList.Add(this);
                if (mouseHook == null)
                {
                    mouseHook = new MouseHook();
                }
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                AdornerWindowList.Remove(this);
                if ((AdornerWindowList.Count == 0) && (mouseHook != null))
                {
                    mouseHook.Dispose();
                    mouseHook = null;
                }
                base.OnHandleDestroyed(e);
            }

            protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
            {
                this.behaviorService.OnQueryContinueDrag(e);
            }

            internal void StartDragNotification()
            {
                this.ProcessingDrag = true;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == BehaviorService.WM_GETALLSNAPLINES)
                {
                    this.behaviorService.TestHook_GetAllSnapLines(ref m);
                }
                else if (m.Msg == BehaviorService.WM_GETRECENTSNAPLINES)
                {
                    this.behaviorService.TestHook_GetRecentSnapLines(ref m);
                }
                int msg = m.Msg;
                if (msg != 15)
                {
                    if (msg != 0x84)
                    {
                        if (msg == 0x215)
                        {
                            base.WndProc(ref m);
                            this.behaviorService.OnLoseCapture();
                            return;
                        }
                        base.WndProc(ref m);
                        return;
                    }
                }
                else
                {
                    IntPtr hrgn = System.Design.NativeMethods.CreateRectRgn(0, 0, 0, 0);
                    System.Design.NativeMethods.GetUpdateRgn(m.HWnd, hrgn, true);
                    System.Design.NativeMethods.RECT rc = new System.Design.NativeMethods.RECT();
                    System.Design.NativeMethods.GetUpdateRect(m.HWnd, ref rc, true);
                    Rectangle clipRect = new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
                    try
                    {
                        using (Region region = Region.FromHrgn(hrgn))
                        {
                            this.DefWndProc(ref m);
                            using (Graphics graphics = Graphics.FromHwnd(m.HWnd))
                            {
                                using (PaintEventArgs args = new PaintEventArgs(graphics, clipRect))
                                {
                                    graphics.Clip = region;
                                    this.behaviorService.PropagatePaint(args);
                                }
                                return;
                            }
                        }
                    }
                    finally
                    {
                        System.Design.NativeMethods.DeleteObject(hrgn);
                    }
                }
                Point pt = new Point((short) System.Design.NativeMethods.Util.LOWORD((int) ((long) m.LParam)), (short) System.Design.NativeMethods.Util.HIWORD((int) ((long) m.LParam)));
                System.Design.NativeMethods.POINT point2 = new System.Design.NativeMethods.POINT {
                    x = 0,
                    y = 0
                };
                System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, base.Handle, point2, 1);
                pt.Offset(point2.x, point2.y);
                if (this.behaviorService.PropagateHitTest(pt) && !this.ProcessingDrag)
                {
                    m.Result = (IntPtr) (-1);
                }
                else
                {
                    m.Result = (IntPtr) 1;
                }
            }

            private bool WndProcProxy(ref Message m, int x, int y)
            {
                Point pt = new Point(x, y);
                this.behaviorService.PropagateHitTest(pt);
                switch (m.Msg)
                {
                    case 0x200:
                        if (!this.behaviorService.OnMouseMove(Control.MouseButtons, pt))
                        {
                            break;
                        }
                        return false;

                    case 0x201:
                        if (!this.behaviorService.OnMouseDown(MouseButtons.Left, pt))
                        {
                            break;
                        }
                        return false;

                    case 0x202:
                        if (!this.behaviorService.OnMouseUp(MouseButtons.Left))
                        {
                            break;
                        }
                        return false;

                    case 0x203:
                        if (!this.behaviorService.OnMouseDoubleClick(MouseButtons.Left, pt))
                        {
                            break;
                        }
                        return false;

                    case 0x204:
                        if (!this.behaviorService.OnMouseDown(MouseButtons.Right, pt))
                        {
                            break;
                        }
                        return false;

                    case 0x205:
                        if (!this.behaviorService.OnMouseUp(MouseButtons.Right))
                        {
                            break;
                        }
                        return false;

                    case 0x206:
                        if (!this.behaviorService.OnMouseDoubleClick(MouseButtons.Right, pt))
                        {
                            break;
                        }
                        return false;

                    case 0x2a1:
                        if (this.behaviorService.OnMouseHover(pt))
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    createParams.Style &= -100663297;
                    createParams.ExStyle |= 0x20;
                    return createParams;
                }
            }

            internal Control DesignerFrame
            {
                get
                {
                    return this.designerFrame;
                }
            }

            internal Rectangle DesignerFrameDisplayRectangle
            {
                get
                {
                    if (this.DesignerFrameValid)
                    {
                        return ((System.Windows.Forms.Design.DesignerFrame) this.designerFrame).DisplayRectangle;
                    }
                    return Rectangle.Empty;
                }
            }

            private bool DesignerFrameValid
            {
                get
                {
                    return (((this.designerFrame != null) && !this.designerFrame.IsDisposed) && this.designerFrame.IsHandleCreated);
                }
            }

            internal bool ProcessingDrag
            {
                get
                {
                    return this.processingDrag;
                }
                set
                {
                    this.processingDrag = value;
                }
            }

            private class MouseHook
            {
                private BehaviorService.AdornerWindow currentAdornerWindow;
                private bool isHooked;
                private int lastLButtonDownTimeStamp;
                private IntPtr mouseHookHandle = IntPtr.Zero;
                private GCHandle mouseHookRoot;
                private bool processingMessage;
                private int thisProcessID;

                public MouseHook()
                {
                    this.HookMouse();
                }

                public void Dispose()
                {
                    this.UnhookMouse();
                }

                private void HookMouse()
                {
                    lock (this)
                    {
                        if ((this.mouseHookHandle == IntPtr.Zero) && (BehaviorService.AdornerWindow.AdornerWindowList.Count != 0))
                        {
                            if (this.thisProcessID == 0)
                            {
                                BehaviorService.AdornerWindow wrapper = BehaviorService.AdornerWindow.AdornerWindowList[0];
                                System.Design.UnsafeNativeMethods.GetWindowThreadProcessId(new HandleRef(wrapper, wrapper.Handle), out this.thisProcessID);
                            }
                            System.Design.UnsafeNativeMethods.HookProc proc = new System.Design.UnsafeNativeMethods.HookProc(this.MouseHookProc);
                            this.mouseHookRoot = GCHandle.Alloc(proc);
                            this.mouseHookHandle = System.Design.UnsafeNativeMethods.SetWindowsHookEx(7, proc, new HandleRef(null, IntPtr.Zero), AppDomain.GetCurrentThreadId());
                            if (this.mouseHookHandle != IntPtr.Zero)
                            {
                                this.isHooked = true;
                            }
                        }
                    }
                }

                public static int MAKELONG(int low, int high)
                {
                    return ((high << 0x10) | (low & 0xffff));
                }

                private unsafe IntPtr MouseHookProc(int nCode, IntPtr wparam, IntPtr lparam)
                {
                    if (this.isHooked && (nCode == 0))
                    {
                        System.Design.NativeMethods.MOUSEHOOKSTRUCT* mousehookstructPtr = (System.Design.NativeMethods.MOUSEHOOKSTRUCT*) lparam;
                        if (mousehookstructPtr != null)
                        {
                            try
                            {
                                if (this.ProcessMouseMessage(mousehookstructPtr->hWnd, (int) ((long) wparam), mousehookstructPtr->pt_x, mousehookstructPtr->pt_y))
                                {
                                    return (IntPtr) 1;
                                }
                            }
                            catch (Exception exception)
                            {
                                this.currentAdornerWindow.Capture = false;
                                if (exception != CheckoutException.Canceled)
                                {
                                    this.currentAdornerWindow.behaviorService.ShowError(exception);
                                }
                                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                                {
                                    throw;
                                }
                            }
                            finally
                            {
                                this.currentAdornerWindow = null;
                            }
                        }
                    }
                    return System.Design.UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, this.mouseHookHandle), nCode, wparam, lparam);
                }

                private bool ProcessMouseMessage(IntPtr hWnd, int msg, int x, int y)
                {
                    if (!this.processingMessage)
                    {
                        new NamedPermissionSet("FullTrust").Assert();
                        foreach (BehaviorService.AdornerWindow window in BehaviorService.AdornerWindow.AdornerWindowList)
                        {
                            this.currentAdornerWindow = window;
                            IntPtr handle = window.DesignerFrame.Handle;
                            if (window.ProcessingDrag || ((hWnd != handle) && System.Design.SafeNativeMethods.IsChild(new HandleRef(this, handle), new HandleRef(this, hWnd))))
                            {
                                int num;
                                System.Design.UnsafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, hWnd), out num);
                                if (num != this.thisProcessID)
                                {
                                    return false;
                                }
                                try
                                {
                                    this.processingMessage = true;
                                    System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                                        x = x,
                                        y = y
                                    };
                                    System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, window.Handle, pt, 1);
                                    Message m = Message.Create(hWnd, msg, IntPtr.Zero, (IntPtr) MAKELONG(pt.y, pt.x));
                                    if (m.Msg == 0x201)
                                    {
                                        this.lastLButtonDownTimeStamp = System.Design.UnsafeNativeMethods.GetMessageTime();
                                    }
                                    else if ((m.Msg == 0x203) && (System.Design.UnsafeNativeMethods.GetMessageTime() == this.lastLButtonDownTimeStamp))
                                    {
                                        return true;
                                    }
                                    if (!window.WndProcProxy(ref m, pt.x, pt.y))
                                    {
                                        return true;
                                    }
                                    break;
                                }
                                finally
                                {
                                    this.processingMessage = false;
                                }
                            }
                        }
                    }
                    return false;
                }

                private void UnhookMouse()
                {
                    lock (this)
                    {
                        if (this.mouseHookHandle != IntPtr.Zero)
                        {
                            System.Design.UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, this.mouseHookHandle));
                            this.mouseHookRoot.Free();
                            this.mouseHookHandle = IntPtr.Zero;
                            this.isHooked = false;
                        }
                    }
                }
            }
        }

        private class MenuCommandHandler : IMenuCommandService
        {
            private Stack<CommandID> currentCommands = new Stack<CommandID>();
            private IMenuCommandService menuService;
            private BehaviorService owner;

            public MenuCommandHandler(BehaviorService owner, IMenuCommandService menuService)
            {
                this.owner = owner;
                this.menuService = menuService;
            }

            void IMenuCommandService.AddCommand(MenuCommand command)
            {
                this.menuService.AddCommand(command);
            }

            void IMenuCommandService.AddVerb(DesignerVerb verb)
            {
                this.menuService.AddVerb(verb);
            }

            MenuCommand IMenuCommandService.FindCommand(CommandID commandID)
            {
                MenuCommand command;
                try
                {
                    if (this.currentCommands.Contains(commandID))
                    {
                        return null;
                    }
                    this.currentCommands.Push(commandID);
                    command = this.owner.FindCommand(commandID, this.menuService);
                }
                finally
                {
                    this.currentCommands.Pop();
                }
                return command;
            }

            bool IMenuCommandService.GlobalInvoke(CommandID commandID)
            {
                return this.menuService.GlobalInvoke(commandID);
            }

            void IMenuCommandService.RemoveCommand(MenuCommand command)
            {
                this.menuService.RemoveCommand(command);
            }

            void IMenuCommandService.RemoveVerb(DesignerVerb verb)
            {
                this.menuService.RemoveVerb(verb);
            }

            void IMenuCommandService.ShowContextMenu(CommandID menuID, int x, int y)
            {
                this.menuService.ShowContextMenu(menuID, x, y);
            }

            public IMenuCommandService MenuService
            {
                get
                {
                    return this.menuService;
                }
            }

            DesignerVerbCollection IMenuCommandService.Verbs
            {
                get
                {
                    return this.menuService.Verbs;
                }
            }
        }
    }
}

