namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public sealed class ToolStripManager
    {
        [ThreadStatic]
        private static System.Type currentRendererType;
        private static Font defaultFont;
        [ThreadStatic]
        private static ToolStripRenderer defaultRenderer;
        [ThreadStatic]
        private static bool initialized;
        private static object internalSyncObject = new object();
        internal static System.Type ProfessionalRendererType = typeof(ToolStripProfessionalRenderer);
        private const int staticEventCount = 1;
        private const int staticEventDefaultRendererChanged = 0;
        [ThreadStatic]
        private static Delegate[] staticEventHandlers;
        internal static System.Type SystemRendererType = typeof(ToolStripSystemRenderer);
        [ThreadStatic]
        private static System.Windows.Forms.ClientUtils.WeakRefCollection toolStripPanelWeakArrayList;
        [ThreadStatic]
        private static System.Windows.Forms.ClientUtils.WeakRefCollection toolStripWeakArrayList;
        private static bool visualStylesEnabledIfPossible = true;

        public static  event EventHandler RendererChanged
        {
            add
            {
                AddEventHandler(0, value);
            }
            remove
            {
                RemoveEventHandler(0, value);
            }
        }

        static ToolStripManager()
        {
            SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(ToolStripManager.OnUserPreferenceChanging);
        }

        private ToolStripManager()
        {
        }

        private static void AddEventHandler(int key, Delegate value)
        {
            lock (internalSyncObject)
            {
                if (staticEventHandlers == null)
                {
                    staticEventHandlers = new Delegate[1];
                }
                staticEventHandlers[key] = Delegate.Combine(staticEventHandlers[key], value);
            }
        }

        private static bool CanChangeSelection(ToolStrip start, ToolStrip toolStrip)
        {
            if ((toolStrip != null) && ((((!toolStrip.TabStop && toolStrip.Enabled) && (toolStrip.Visible && !toolStrip.IsDisposed)) && (!toolStrip.Disposing && !toolStrip.IsDropDown)) && IsOnSameWindow(start, toolStrip)))
            {
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if (item.CanSelect)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ChangeSelection(ToolStrip start, ToolStrip toolStrip)
        {
            if ((toolStrip == null) || (start == null))
            {
                return false;
            }
            if (start == toolStrip)
            {
                return false;
            }
            if (ModalMenuFilter.InMenuMode)
            {
                if (ModalMenuFilter.GetActiveToolStrip() == start)
                {
                    ModalMenuFilter.RemoveActiveToolStrip(start);
                    start.NotifySelectionChange(null);
                }
                ModalMenuFilter.SetActiveToolStrip(toolStrip);
            }
            else
            {
                toolStrip.FocusInternal();
            }
            start.SnapFocusChange(toolStrip);
            toolStrip.SelectNextToolStripItem(null, toolStrip.RightToLeft != RightToLeft.Yes);
            return true;
        }

        internal static ToolStripRenderer CreateRenderer(ToolStripManagerRenderMode renderMode)
        {
            switch (renderMode)
            {
                case ToolStripManagerRenderMode.System:
                    return new ToolStripSystemRenderer(true);

                case ToolStripManagerRenderMode.Professional:
                    return new ToolStripProfessionalRenderer(true);
            }
            return new ToolStripSystemRenderer(true);
        }

        internal static ToolStripRenderer CreateRenderer(ToolStripRenderMode renderMode)
        {
            switch (renderMode)
            {
                case ToolStripRenderMode.System:
                    return new ToolStripSystemRenderer(true);

                case ToolStripRenderMode.Professional:
                    return new ToolStripProfessionalRenderer(true);
            }
            return new ToolStripSystemRenderer(true);
        }

        private static ToolStripItem FindMatch(ToolStripItem source, ToolStripItemCollection destinationItems)
        {
            ToolStripItem item = null;
            if (source != null)
            {
                for (int i = 0; i < destinationItems.Count; i++)
                {
                    ToolStripItem item2 = destinationItems[i];
                    if (WindowsFormsUtils.SafeCompareStrings(source.Text, item2.Text, true))
                    {
                        item = item2;
                        break;
                    }
                }
                if (((item == null) && (source.MergeIndex > -1)) && (source.MergeIndex < destinationItems.Count))
                {
                    item = destinationItems[source.MergeIndex];
                }
            }
            return item;
        }

        internal static ArrayList FindMergeableToolStrips(ContainerControl container)
        {
            ArrayList list = new ArrayList();
            if (container != null)
            {
                for (int i = 0; i < ToolStrips.Count; i++)
                {
                    ToolStrip strip = (ToolStrip) ToolStrips[i];
                    if (((strip != null) && strip.AllowMerge) && (container == strip.FindFormInternal()))
                    {
                        list.Add(strip);
                    }
                }
            }
            list.Sort(new ToolStripCustomIComparer());
            return list;
        }

        [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
        public static ToolStrip FindToolStrip(string toolStripName)
        {
            for (int i = 0; i < ToolStrips.Count; i++)
            {
                if ((ToolStrips[i] != null) && string.Equals(((ToolStrip) ToolStrips[i]).Name, toolStripName, StringComparison.Ordinal))
                {
                    return (ToolStrip) ToolStrips[i];
                }
            }
            return null;
        }

        [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
        internal static ToolStrip FindToolStrip(Form owningForm, string toolStripName)
        {
            ToolStrip strip = null;
            for (int i = 0; i < ToolStrips.Count; i++)
            {
                if ((ToolStrips[i] != null) && string.Equals(((ToolStrip) ToolStrips[i]).Name, toolStripName, StringComparison.Ordinal))
                {
                    strip = (ToolStrip) ToolStrips[i];
                    if (strip.FindForm() == owningForm)
                    {
                        return strip;
                    }
                }
            }
            return strip;
        }

        private static Delegate GetEventHandler(int key)
        {
            lock (internalSyncObject)
            {
                if (staticEventHandlers == null)
                {
                    return null;
                }
                return staticEventHandlers[key];
            }
        }

        private static MenuStrip GetFirstMenuStripRecursive(Control.ControlCollection controlsToLookIn)
        {
            try
            {
                for (int i = 0; i < controlsToLookIn.Count; i++)
                {
                    if ((controlsToLookIn[i] != null) && (controlsToLookIn[i] is MenuStrip))
                    {
                        return (controlsToLookIn[i] as MenuStrip);
                    }
                }
                for (int j = 0; j < controlsToLookIn.Count; j++)
                {
                    if (((controlsToLookIn[j] != null) && (controlsToLookIn[j].Controls != null)) && (controlsToLookIn[j].Controls.Count > 0))
                    {
                        MenuStrip firstMenuStripRecursive = GetFirstMenuStripRecursive(controlsToLookIn[j].Controls);
                        if (firstMenuStripRecursive != null)
                        {
                            return firstMenuStripRecursive;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            return null;
        }

        internal static MenuStrip GetMainMenuStrip(Control control)
        {
            if (control == null)
            {
                return null;
            }
            Form form = control.FindFormInternal();
            if ((form != null) && (form.MainMenuStrip != null))
            {
                return form.MainMenuStrip;
            }
            return GetFirstMenuStripRecursive(control.Controls);
        }

        private static void InitalizeThread()
        {
            if (!initialized)
            {
                initialized = true;
                currentRendererType = ProfessionalRendererType;
            }
        }

        internal static bool IsMenuKey(Keys keyData)
        {
            Keys keys = keyData & Keys.KeyCode;
            if (Keys.Menu != keys)
            {
                return (Keys.F10 == keys);
            }
            return true;
        }

        private static bool IsOnSameWindow(Control control1, Control control2)
        {
            return (WindowsFormsUtils.GetRootHWnd(control1).Handle == WindowsFormsUtils.GetRootHWnd(control2).Handle);
        }

        public static bool IsShortcutDefined(Keys shortcut)
        {
            for (int i = 0; i < ToolStrips.Count; i++)
            {
                ToolStrip strip = ToolStrips[i] as ToolStrip;
                if ((strip != null) && strip.Shortcuts.Contains(shortcut))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSpecialMDIStrip(ToolStrip toolStrip)
        {
            return ((toolStrip is MdiControlStrip) || (toolStrip is MdiWindowListStrip));
        }

        internal static bool IsThreadUsingToolStrips()
        {
            return ((toolStripWeakArrayList != null) && (toolStripWeakArrayList.Count > 0));
        }

        public static bool IsValidShortcut(Keys shortcut)
        {
            Keys keys = shortcut & Keys.KeyCode;
            Keys keys2 = shortcut & ~Keys.KeyCode;
            if (shortcut == Keys.None)
            {
                return false;
            }
            switch (keys)
            {
                case Keys.Delete:
                case Keys.Insert:
                    return true;
            }
            if ((keys < Keys.F1) || (keys > Keys.F24))
            {
                if ((keys == Keys.None) || (keys2 == Keys.None))
                {
                    return false;
                }
                switch (keys)
                {
                    case Keys.ShiftKey:
                    case Keys.ControlKey:
                    case Keys.Menu:
                        return false;
                }
                if (keys2 == Keys.Shift)
                {
                    return false;
                }
            }
            return true;
        }

        public static void LoadSettings(Form targetForm)
        {
            if (targetForm == null)
            {
                throw new ArgumentNullException("targetForm");
            }
            LoadSettings(targetForm, targetForm.GetType().FullName);
        }

        public static void LoadSettings(Form targetForm, string key)
        {
            if (targetForm == null)
            {
                throw new ArgumentNullException("targetForm");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            new ToolStripSettingsManager(targetForm, key).Load();
        }

        public static bool Merge(ToolStrip sourceToolStrip, string targetName)
        {
            if (sourceToolStrip == null)
            {
                throw new ArgumentNullException("sourceToolStrip");
            }
            if (targetName == null)
            {
                throw new ArgumentNullException("targetName");
            }
            ToolStrip targetToolStrip = FindToolStrip(targetName);
            if (targetToolStrip == null)
            {
                return false;
            }
            return Merge(sourceToolStrip, targetToolStrip);
        }

        public static bool Merge(ToolStrip sourceToolStrip, ToolStrip targetToolStrip)
        {
            if (sourceToolStrip == null)
            {
                throw new ArgumentNullException("sourceToolStrip");
            }
            if (targetToolStrip == null)
            {
                throw new ArgumentNullException("targetToolStrip");
            }
            if (targetToolStrip == sourceToolStrip)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolStripMergeImpossibleIdentical"));
            }
            bool flag = IsSpecialMDIStrip(sourceToolStrip) || ((sourceToolStrip.AllowMerge && targetToolStrip.AllowMerge) && (sourceToolStrip.GetType().IsAssignableFrom(targetToolStrip.GetType()) || targetToolStrip.GetType().IsAssignableFrom(sourceToolStrip.GetType())));
            MergeHistory history = null;
            if (flag)
            {
                history = new MergeHistory(sourceToolStrip);
                int count = sourceToolStrip.Items.Count;
                if (count > 0)
                {
                    sourceToolStrip.SuspendLayout();
                    targetToolStrip.SuspendLayout();
                    try
                    {
                        int num2 = count;
                        int num3 = 0;
                        int num4 = 0;
                        while (num3 < count)
                        {
                            ToolStripItem source = sourceToolStrip.Items[num4];
                            MergeRecursive(source, targetToolStrip.Items, history.MergeHistoryItemsStack);
                            int num5 = num2 - sourceToolStrip.Items.Count;
                            num4 = (num5 > 0) ? num4 : (num4 + 1);
                            num2 = sourceToolStrip.Items.Count;
                            num3++;
                        }
                    }
                    finally
                    {
                        sourceToolStrip.ResumeLayout();
                        targetToolStrip.ResumeLayout();
                    }
                    if (history.MergeHistoryItemsStack.Count > 0)
                    {
                        targetToolStrip.MergeHistoryStack.Push(history);
                    }
                }
            }
            bool flag2 = false;
            if ((history != null) && (history.MergeHistoryItemsStack.Count > 0))
            {
                flag2 = true;
            }
            return flag2;
        }

        private static void MergeRecursive(ToolStripItem source, ToolStripItemCollection destinationItems, Stack<MergeHistoryItem> history)
        {
            MergeHistoryItem item;
            ToolStripItem item2;
            switch (source.MergeAction)
            {
                case MergeAction.Append:
                {
                    item = new MergeHistoryItem(MergeAction.Remove) {
                        PreviousIndexCollection = source.Owner.Items,
                        PreviousIndex = item.PreviousIndexCollection.IndexOf(source),
                        TargetItem = source
                    };
                    int num8 = destinationItems.Add(source);
                    item.Index = num8;
                    item.IndexCollection = destinationItems;
                    history.Push(item);
                    return;
                }
                case MergeAction.Insert:
                    if (source.MergeIndex > -1)
                    {
                        item = new MergeHistoryItem(MergeAction.Remove) {
                            PreviousIndexCollection = source.Owner.Items,
                            PreviousIndex = item.PreviousIndexCollection.IndexOf(source),
                            TargetItem = source
                        };
                        int num7 = Math.Min(destinationItems.Count, source.MergeIndex);
                        destinationItems.Insert(num7, source);
                        item.IndexCollection = destinationItems;
                        item.Index = num7;
                        history.Push(item);
                    }
                    return;

                case MergeAction.Replace:
                case MergeAction.Remove:
                case MergeAction.MatchOnly:
                    item2 = FindMatch(source, destinationItems);
                    if (item2 != null)
                    {
                        switch (source.MergeAction)
                        {
                            case MergeAction.MatchOnly:
                            {
                                ToolStripDropDownItem item3 = item2 as ToolStripDropDownItem;
                                ToolStripDropDownItem item4 = source as ToolStripDropDownItem;
                                if (((item3 == null) || (item4 == null)) || (item4.DropDownItems.Count == 0))
                                {
                                    return;
                                }
                                int count = item4.DropDownItems.Count;
                                if (count <= 0)
                                {
                                    return;
                                }
                                int num2 = count;
                                item4.DropDown.SuspendLayout();
                                try
                                {
                                    int num3 = 0;
                                    int num4 = 0;
                                    while (num3 < count)
                                    {
                                        MergeRecursive(item4.DropDownItems[num4], item3.DropDownItems, history);
                                        int num5 = num2 - item4.DropDownItems.Count;
                                        num4 = (num5 > 0) ? num4 : (num4 + 1);
                                        num2 = item4.DropDownItems.Count;
                                        num3++;
                                    }
                                    return;
                                }
                                finally
                                {
                                    item4.DropDown.ResumeLayout();
                                }
                                goto Label_0108;
                            }
                        }
                    }
                    return;

                default:
                    return;
            }
        Label_0108:
            item = new MergeHistoryItem(MergeAction.Insert);
            item.TargetItem = item2;
            int index = destinationItems.IndexOf(item2);
            destinationItems.RemoveAt(index);
            item.Index = index;
            item.IndexCollection = destinationItems;
            item.TargetItem = item2;
            history.Push(item);
            if (source.MergeAction == MergeAction.Replace)
            {
                item = new MergeHistoryItem(MergeAction.Remove) {
                    PreviousIndexCollection = source.Owner.Items,
                    PreviousIndex = item.PreviousIndexCollection.IndexOf(source),
                    TargetItem = source
                };
                destinationItems.Insert(index, source);
                item.Index = index;
                item.IndexCollection = destinationItems;
                history.Push(item);
            }
        }

        internal static void NotifyMenuModeChange(bool invalidateText, bool activationChange)
        {
            bool flag = false;
            for (int i = 0; i < ToolStrips.Count; i++)
            {
                ToolStrip strip = ToolStrips[i] as ToolStrip;
                if (strip == null)
                {
                    flag = true;
                }
                else
                {
                    if (invalidateText)
                    {
                        strip.InvalidateTextItems();
                    }
                    if (activationChange)
                    {
                        strip.KeyboardActive = false;
                    }
                }
            }
            if (flag)
            {
                PruneToolStripList();
            }
        }

        private static void OnUserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Window)
            {
                lock (internalSyncObject)
                {
                    defaultFont = null;
                }
            }
        }

        internal static bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if (IsValidShortcut(keyData))
            {
                return ProcessShortcut(ref m, keyData);
            }
            if (m.Msg == 260)
            {
                ModalMenuFilter.ProcessMenuKeyDown(ref m);
            }
            return false;
        }

        internal static bool ProcessMenuKey(ref Message m)
        {
            if (IsThreadUsingToolStrips())
            {
                Keys lParam = (Keys) ((int) m.LParam);
                Control control = Control.FromHandleInternal(m.HWnd);
                Control wrapper = null;
                MenuStrip mainMenuStrip = null;
                if (control != null)
                {
                    wrapper = control.TopLevelControlInternal;
                    if ((wrapper != null) && (System.Windows.Forms.UnsafeNativeMethods.GetMenu(new HandleRef(wrapper, wrapper.Handle)) == IntPtr.Zero))
                    {
                        mainMenuStrip = GetMainMenuStrip(wrapper);
                    }
                }
                if (((ushort) lParam) == 0x20)
                {
                    ModalMenuFilter.MenuKeyToggle = false;
                }
                else if (((ushort) lParam) == 0x2d)
                {
                    Form form = wrapper as Form;
                    if (((form != null) && form.IsMdiChild) && (form.WindowState == FormWindowState.Maximized))
                    {
                        ModalMenuFilter.MenuKeyToggle = false;
                    }
                }
                else
                {
                    if ((System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x10) < 0) && (lParam == Keys.None))
                    {
                        return ModalMenuFilter.InMenuMode;
                    }
                    if ((mainMenuStrip != null) && !ModalMenuFilter.MenuKeyToggle)
                    {
                        HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(mainMenuStrip);
                        IntPtr foregroundWindow = System.Windows.Forms.UnsafeNativeMethods.GetForegroundWindow();
                        if (rootHWnd.Handle == foregroundWindow)
                        {
                            return mainMenuStrip.OnMenuKey();
                        }
                    }
                    else if (mainMenuStrip != null)
                    {
                        ModalMenuFilter.MenuKeyToggle = false;
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool ProcessShortcut(ref Message m, Keys shortcut)
        {
            if (!IsThreadUsingToolStrips())
            {
                return false;
            }
            Control control = Control.FromChildHandleInternal(m.HWnd);
            Control parentInternal = control;
            if ((parentInternal == null) || !IsValidShortcut(shortcut))
            {
                return false;
            }
            do
            {
                if ((parentInternal.ContextMenuStrip != null) && parentInternal.ContextMenuStrip.Shortcuts.ContainsKey(shortcut))
                {
                    ToolStripMenuItem item = parentInternal.ContextMenuStrip.Shortcuts[shortcut] as ToolStripMenuItem;
                    if (item.ProcessCmdKey(ref m, shortcut))
                    {
                        return true;
                    }
                }
                parentInternal = parentInternal.ParentInternal;
            }
            while (parentInternal != null);
            if (parentInternal != null)
            {
                control = parentInternal;
            }
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < ToolStrips.Count; i++)
            {
                ToolStrip strip = ToolStrips[i] as ToolStrip;
                bool flag3 = false;
                bool isAssignedToDropDownItem = false;
                if (strip == null)
                {
                    flag2 = true;
                }
                else
                {
                    if (((control == null) || (strip != control.ContextMenuStrip)) && strip.Shortcuts.ContainsKey(shortcut))
                    {
                        if (strip.IsDropDown)
                        {
                            ToolStripDropDown down = strip as ToolStripDropDown;
                            ContextMenuStrip firstDropDown = down.GetFirstDropDown() as ContextMenuStrip;
                            if (firstDropDown != null)
                            {
                                isAssignedToDropDownItem = firstDropDown.IsAssignedToDropDownItem;
                                if (!isAssignedToDropDownItem)
                                {
                                    if (firstDropDown != control.ContextMenuStrip)
                                    {
                                        goto Label_01D2;
                                    }
                                    flag3 = true;
                                }
                            }
                        }
                        bool flag5 = false;
                        if (!flag3)
                        {
                            ToolStrip toplevelOwnerToolStrip = strip.GetToplevelOwnerToolStrip();
                            if ((toplevelOwnerToolStrip != null) && (control != null))
                            {
                                HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(toplevelOwnerToolStrip);
                                HandleRef ref3 = WindowsFormsUtils.GetRootHWnd(control);
                                flag5 = rootHWnd.Handle == ref3.Handle;
                                if (flag5)
                                {
                                    Form form = Control.FromHandleInternal(ref3.Handle) as Form;
                                    if ((form != null) && form.IsMdiContainer)
                                    {
                                        Form form2 = toplevelOwnerToolStrip.FindFormInternal();
                                        if ((form2 != form) && (form2 != null))
                                        {
                                            flag5 = form2 == form.ActiveMdiChildInternal;
                                        }
                                    }
                                }
                            }
                        }
                        if ((flag3 || flag5) || isAssignedToDropDownItem)
                        {
                            ToolStripMenuItem item2 = strip.Shortcuts[shortcut] as ToolStripMenuItem;
                            if ((item2 != null) && item2.ProcessCmdKey(ref m, shortcut))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                Label_01D2:;
                }
            }
            if (flag2)
            {
                PruneToolStripList();
            }
            return flag;
        }

        internal static void PruneToolStripList()
        {
            if ((toolStripWeakArrayList != null) && (toolStripWeakArrayList.Count > 0))
            {
                for (int i = toolStripWeakArrayList.Count - 1; i >= 0; i--)
                {
                    if (toolStripWeakArrayList[i] == null)
                    {
                        toolStripWeakArrayList.RemoveAt(i);
                    }
                }
            }
        }

        private static void RemoveEventHandler(int key, Delegate value)
        {
            lock (internalSyncObject)
            {
                if (staticEventHandlers != null)
                {
                    staticEventHandlers[key] = Delegate.Remove(staticEventHandlers[key], value);
                }
            }
        }

        public static bool RevertMerge(string targetName)
        {
            ToolStrip targetToolStrip = FindToolStrip(targetName);
            if (targetToolStrip == null)
            {
                return false;
            }
            return RevertMerge(targetToolStrip);
        }

        public static bool RevertMerge(ToolStrip targetToolStrip)
        {
            return RevertMergeInternal(targetToolStrip, null, false);
        }

        public static bool RevertMerge(ToolStrip targetToolStrip, ToolStrip sourceToolStrip)
        {
            if (sourceToolStrip == null)
            {
                throw new ArgumentNullException("sourceToolStrip");
            }
            return RevertMergeInternal(targetToolStrip, sourceToolStrip, false);
        }

        internal static bool RevertMergeInternal(ToolStrip targetToolStrip, ToolStrip sourceToolStrip, bool revertMDIControls)
        {
            bool flag = false;
            if (targetToolStrip == null)
            {
                throw new ArgumentNullException("targetToolStrip");
            }
            if (targetToolStrip == sourceToolStrip)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolStripMergeImpossibleIdentical"));
            }
            bool flag2 = false;
            if (sourceToolStrip != null)
            {
                foreach (MergeHistory history in targetToolStrip.MergeHistoryStack)
                {
                    flag2 = history.MergedToolStrip == sourceToolStrip;
                    if (flag2)
                    {
                        break;
                    }
                }
                if (!flag2)
                {
                    return false;
                }
            }
            if (sourceToolStrip != null)
            {
                sourceToolStrip.SuspendLayout();
            }
            targetToolStrip.SuspendLayout();
            try
            {
                Stack<ToolStrip> stack = new Stack<ToolStrip>();
                flag2 = false;
                while ((targetToolStrip.MergeHistoryStack.Count > 0) && !flag2)
                {
                    flag = true;
                    MergeHistory history2 = targetToolStrip.MergeHistoryStack.Pop();
                    if (history2.MergedToolStrip == sourceToolStrip)
                    {
                        flag2 = true;
                    }
                    else if (!revertMDIControls && (sourceToolStrip == null))
                    {
                        if (IsSpecialMDIStrip(history2.MergedToolStrip))
                        {
                            stack.Push(history2.MergedToolStrip);
                        }
                    }
                    else
                    {
                        stack.Push(history2.MergedToolStrip);
                    }
                    while (history2.MergeHistoryItemsStack.Count > 0)
                    {
                        MergeHistoryItem item = history2.MergeHistoryItemsStack.Pop();
                        switch (item.MergeAction)
                        {
                            case MergeAction.Insert:
                                item.IndexCollection.Insert(Math.Min(item.Index, item.IndexCollection.Count), item.TargetItem);
                                break;

                            case MergeAction.Remove:
                                item.IndexCollection.Remove(item.TargetItem);
                                item.PreviousIndexCollection.Insert(Math.Min(item.PreviousIndex, item.PreviousIndexCollection.Count), item.TargetItem);
                                break;
                        }
                    }
                }
                while (stack.Count > 0)
                {
                    Merge(stack.Pop(), targetToolStrip);
                }
            }
            finally
            {
                if (sourceToolStrip != null)
                {
                    sourceToolStrip.ResumeLayout();
                }
                targetToolStrip.ResumeLayout();
            }
            return flag;
        }

        public static void SaveSettings(Form sourceForm)
        {
            if (sourceForm == null)
            {
                throw new ArgumentNullException("sourceForm");
            }
            SaveSettings(sourceForm, sourceForm.GetType().FullName);
        }

        public static void SaveSettings(Form sourceForm, string key)
        {
            if (sourceForm == null)
            {
                throw new ArgumentNullException("sourceForm");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            new ToolStripSettingsManager(sourceForm, key).Save();
        }

        internal static bool SelectNextToolStrip(ToolStrip start, bool forward)
        {
            if ((start == null) || (start.ParentInternal == null))
            {
                return false;
            }
            ToolStrip toolStrip = null;
            ToolStrip strip2 = null;
            int tabIndex = start.TabIndex;
            int index = ToolStrips.IndexOf(start);
            int count = ToolStrips.Count;
            for (int i = 0; i < count; i++)
            {
                index = forward ? ((index + 1) % count) : (((index + count) - 1) % count);
                ToolStrip strip3 = ToolStrips[index] as ToolStrip;
                if ((strip3 != null) && (strip3 != start))
                {
                    int num5 = strip3.TabIndex;
                    if (forward)
                    {
                        if ((num5 >= tabIndex) && CanChangeSelection(start, strip3))
                        {
                            if (strip2 == null)
                            {
                                strip2 = strip3;
                            }
                            else if (strip3.TabIndex < strip2.TabIndex)
                            {
                                strip2 = strip3;
                            }
                        }
                        else if (((toolStrip == null) || (strip3.TabIndex < toolStrip.TabIndex)) && CanChangeSelection(start, strip3))
                        {
                            toolStrip = strip3;
                        }
                    }
                    else if ((num5 <= tabIndex) && CanChangeSelection(start, strip3))
                    {
                        if (strip2 == null)
                        {
                            strip2 = strip3;
                        }
                        else if (strip3.TabIndex > strip2.TabIndex)
                        {
                            strip2 = strip3;
                        }
                    }
                    else if (((toolStrip == null) || (strip3.TabIndex > toolStrip.TabIndex)) && CanChangeSelection(start, strip3))
                    {
                        toolStrip = strip3;
                    }
                    if ((strip2 != null) && (Math.Abs((int) (strip2.TabIndex - tabIndex)) <= 1))
                    {
                        break;
                    }
                }
            }
            if (strip2 != null)
            {
                return ChangeSelection(start, strip2);
            }
            return ((toolStrip != null) && ChangeSelection(start, toolStrip));
        }

        internal static ToolStripPanel ToolStripPanelFromPoint(Control draggedControl, Point screenLocation)
        {
            if (toolStripPanelWeakArrayList != null)
            {
                ISupportToolStripPanel panel = draggedControl as ISupportToolStripPanel;
                bool isCurrentlyDragging = panel.IsCurrentlyDragging;
                for (int i = 0; i < toolStripPanelWeakArrayList.Count; i++)
                {
                    ToolStripPanel panel2 = toolStripPanelWeakArrayList[i] as ToolStripPanel;
                    if (((panel2 != null) && panel2.IsHandleCreated) && (panel2.Visible && panel2.DragBounds.Contains(panel2.PointToClient(screenLocation))))
                    {
                        if (!isCurrentlyDragging)
                        {
                            return panel2;
                        }
                        if (IsOnSameWindow(draggedControl, panel2))
                        {
                            return panel2;
                        }
                    }
                }
            }
            return null;
        }

        private static System.Type CurrentRendererType
        {
            get
            {
                InitalizeThread();
                return currentRendererType;
            }
            set
            {
                currentRendererType = value;
            }
        }

        internal static Font DefaultFont
        {
            get
            {
                Font menuFont = null;
                Font defaultFont = ToolStripManager.defaultFont;
                if (defaultFont == null)
                {
                    lock (internalSyncObject)
                    {
                        defaultFont = ToolStripManager.defaultFont;
                        if (defaultFont != null)
                        {
                            return defaultFont;
                        }
                        menuFont = SystemFonts.MenuFont;
                        if (menuFont == null)
                        {
                            menuFont = Control.DefaultFont;
                        }
                        if (menuFont == null)
                        {
                            return defaultFont;
                        }
                        if (menuFont.Unit != GraphicsUnit.Point)
                        {
                            ToolStripManager.defaultFont = ControlPaint.FontInPoints(menuFont);
                            defaultFont = ToolStripManager.defaultFont;
                            menuFont.Dispose();
                            return defaultFont;
                        }
                        ToolStripManager.defaultFont = menuFont;
                        return ToolStripManager.defaultFont;
                    }
                }
                return defaultFont;
            }
        }

        private static System.Type DefaultRendererType
        {
            get
            {
                return ProfessionalRendererType;
            }
        }

        public static ToolStripRenderer Renderer
        {
            get
            {
                if (defaultRenderer == null)
                {
                    defaultRenderer = CreateRenderer(RenderMode);
                }
                return defaultRenderer;
            }
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            set
            {
                if (defaultRenderer != value)
                {
                    CurrentRendererType = (value == null) ? DefaultRendererType : value.GetType();
                    defaultRenderer = value;
                    EventHandler eventHandler = (EventHandler) GetEventHandler(0);
                    if (eventHandler != null)
                    {
                        eventHandler(null, EventArgs.Empty);
                    }
                }
            }
        }

        public static ToolStripManagerRenderMode RenderMode
        {
            get
            {
                System.Type currentRendererType = CurrentRendererType;
                if ((defaultRenderer == null) || defaultRenderer.IsAutoGenerated)
                {
                    if (currentRendererType == ProfessionalRendererType)
                    {
                        return ToolStripManagerRenderMode.Professional;
                    }
                    if (currentRendererType == SystemRendererType)
                    {
                        return ToolStripManagerRenderMode.System;
                    }
                }
                return ToolStripManagerRenderMode.Custom;
            }
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripManagerRenderMode));
                }
                switch (value)
                {
                    case ToolStripManagerRenderMode.Custom:
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripRenderModeUseRendererPropertyInstead"));

                    case ToolStripManagerRenderMode.System:
                    case ToolStripManagerRenderMode.Professional:
                        Renderer = CreateRenderer(value);
                        return;
                }
            }
        }

        internal static bool ShowMenuFocusCues
        {
            get
            {
                if (!DisplayInformation.MenuAccessKeysUnderlined)
                {
                    return ModalMenuFilter.Instance.ShowUnderlines;
                }
                return true;
            }
        }

        internal static System.Windows.Forms.ClientUtils.WeakRefCollection ToolStripPanels
        {
            get
            {
                if (toolStripPanelWeakArrayList == null)
                {
                    toolStripPanelWeakArrayList = new System.Windows.Forms.ClientUtils.WeakRefCollection();
                }
                return toolStripPanelWeakArrayList;
            }
        }

        internal static System.Windows.Forms.ClientUtils.WeakRefCollection ToolStrips
        {
            get
            {
                if (toolStripWeakArrayList == null)
                {
                    toolStripWeakArrayList = new System.Windows.Forms.ClientUtils.WeakRefCollection();
                }
                return toolStripWeakArrayList;
            }
        }

        public static bool VisualStylesEnabled
        {
            get
            {
                return (visualStylesEnabledIfPossible && Application.RenderWithVisualStyles);
            }
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            set
            {
                bool visualStylesEnabled = VisualStylesEnabled;
                visualStylesEnabledIfPossible = value;
                if (visualStylesEnabled != VisualStylesEnabled)
                {
                    EventHandler eventHandler = (EventHandler) GetEventHandler(0);
                    if (eventHandler != null)
                    {
                        eventHandler(null, EventArgs.Empty);
                    }
                }
            }
        }

        internal class ModalMenuFilter : IMessageModifyAndFilter, IMessageFilter
        {
            private HandleRef _activeHwnd = System.Windows.Forms.NativeMethods.NullHandleRef;
            private bool _caretHidden;
            private Timer _ensureMessageProcessingTimer;
            private bool _inMenuMode;
            private List<ToolStrip> _inputFilterQueue;
            [ThreadStatic]
            private static ToolStripManager.ModalMenuFilter _instance;
            private HandleRef _lastActiveWindow = System.Windows.Forms.NativeMethods.NullHandleRef;
            private bool _showUnderlines;
            private bool _suspendMenuMode;
            private ToolStrip _toplevelToolStrip;
            private bool menuKeyToggle;
            private const int MESSAGE_PROCESSING_INTERVAL = 500;
            private HostedWindowsFormsMessageHook messageHook;

            private ModalMenuFilter()
            {
            }

            internal static void CloseActiveDropDown(ToolStripDropDown activeToolStripDropDown, ToolStripDropDownCloseReason reason)
            {
                activeToolStripDropDown.SetCloseReason(reason);
                activeToolStripDropDown.Visible = false;
                if (GetActiveToolStrip() == null)
                {
                    ExitMenuMode();
                    if (activeToolStripDropDown.OwnerItem != null)
                    {
                        activeToolStripDropDown.OwnerItem.Unselect();
                    }
                }
            }

            private void EnterMenuModeCore()
            {
                if (!InMenuMode)
                {
                    IntPtr activeWindow = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
                    if (activeWindow != IntPtr.Zero)
                    {
                        this.ActiveHwndInternal = new HandleRef(this, activeWindow);
                    }
                    Application.ThreadContext.FromCurrent().AddMessageFilter(this);
                    Application.ThreadContext.FromCurrent().TrackInput(true);
                    if (!Application.ThreadContext.FromCurrent().GetMessageLoop(true))
                    {
                        this.MessageHook.HookMessages = true;
                    }
                    this._inMenuMode = true;
                    this.ProcessMessages(true);
                }
            }

            internal static void ExitMenuMode()
            {
                Instance.ExitMenuModeCore();
            }

            private void ExitMenuModeCore()
            {
                this.ProcessMessages(false);
                if (InMenuMode)
                {
                    try
                    {
                        if (this.messageHook != null)
                        {
                            this.messageHook.HookMessages = false;
                        }
                        Application.ThreadContext.FromCurrent().RemoveMessageFilter(this);
                        Application.ThreadContext.FromCurrent().TrackInput(false);
                        if (ActiveHwnd.Handle != IntPtr.Zero)
                        {
                            Control control = Control.FromHandleInternal(ActiveHwnd.Handle);
                            if (control != null)
                            {
                                control.HandleCreated -= new EventHandler(this.OnActiveHwndHandleCreated);
                            }
                            this.ActiveHwndInternal = System.Windows.Forms.NativeMethods.NullHandleRef;
                        }
                        if (this._inputFilterQueue != null)
                        {
                            this._inputFilterQueue.Clear();
                        }
                        if (this._caretHidden)
                        {
                            this._caretHidden = false;
                            System.Windows.Forms.SafeNativeMethods.ShowCaret(System.Windows.Forms.NativeMethods.NullHandleRef);
                        }
                    }
                    finally
                    {
                        this._inMenuMode = false;
                        bool invalidateText = this._showUnderlines;
                        this._showUnderlines = false;
                        ToolStripManager.NotifyMenuModeChange(invalidateText, true);
                    }
                }
            }

            internal static ToolStrip GetActiveToolStrip()
            {
                return Instance.GetActiveToolStripInternal();
            }

            internal ToolStrip GetActiveToolStripInternal()
            {
                if ((this._inputFilterQueue != null) && (this._inputFilterQueue.Count > 0))
                {
                    return this._inputFilterQueue[this._inputFilterQueue.Count - 1];
                }
                return null;
            }

            private ToolStrip GetCurrentToplevelToolStrip()
            {
                if (this._toplevelToolStrip == null)
                {
                    ToolStrip activeToolStripInternal = this.GetActiveToolStripInternal();
                    if (activeToolStripInternal != null)
                    {
                        this._toplevelToolStrip = activeToolStripInternal.GetToplevelOwnerToolStrip();
                    }
                }
                return this._toplevelToolStrip;
            }

            private static bool IsChildOrSameWindow(HandleRef hwndParent, HandleRef hwndChild)
            {
                return ((hwndParent.Handle == hwndChild.Handle) || System.Windows.Forms.UnsafeNativeMethods.IsChild(hwndParent, hwndChild));
            }

            private static bool IsKeyOrMouseMessage(Message m)
            {
                bool flag = false;
                if ((m.Msg >= 0x200) && (m.Msg <= 0x20a))
                {
                    return true;
                }
                if ((m.Msg >= 0xa1) && (m.Msg <= 0xa9))
                {
                    return true;
                }
                if ((m.Msg >= 0x100) && (m.Msg <= 0x108))
                {
                    flag = true;
                }
                return flag;
            }

            private void OnActiveHwndHandleCreated(object sender, EventArgs e)
            {
                Control control = sender as Control;
                this.ActiveHwndInternal = new HandleRef(this, control.Handle);
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (!this._suspendMenuMode)
                {
                    ToolStrip activeToolStrip = GetActiveToolStrip();
                    if (activeToolStrip == null)
                    {
                        return false;
                    }
                    if (activeToolStrip.IsDisposed)
                    {
                        this.RemoveActiveToolStripCore(activeToolStrip);
                        return false;
                    }
                    HandleRef hwndChild = new HandleRef(activeToolStrip, activeToolStrip.Handle);
                    HandleRef hwndParent = new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow());
                    if (hwndParent.Handle != this._lastActiveWindow.Handle)
                    {
                        if (hwndParent.Handle == IntPtr.Zero)
                        {
                            this.ProcessActivationChange();
                        }
                        else if ((!(Control.FromChildHandleInternal(hwndParent.Handle) is ToolStripDropDown) && !IsChildOrSameWindow(hwndParent, hwndChild)) && !IsChildOrSameWindow(hwndParent, ActiveHwnd))
                        {
                            this.ProcessActivationChange();
                        }
                    }
                    this._lastActiveWindow = hwndParent;
                    if (!IsKeyOrMouseMessage(m))
                    {
                        return false;
                    }
                    switch (m.Msg)
                    {
                        case 160:
                        case 0x200:
                        {
                            Control control = Control.FromChildHandleInternal(m.HWnd);
                            if (((control != null) && (control.TopLevelControlInternal is ToolStripDropDown)) || IsChildOrSameWindow(hwndChild, new HandleRef(null, m.HWnd)))
                            {
                                break;
                            }
                            ToolStrip currentToplevelToolStrip = this.GetCurrentToplevelToolStrip();
                            if ((currentToplevelToolStrip != null) && IsChildOrSameWindow(new HandleRef(currentToplevelToolStrip, currentToplevelToolStrip.Handle), new HandleRef(null, m.HWnd)))
                            {
                                return false;
                            }
                            if (!IsChildOrSameWindow(ActiveHwnd, new HandleRef(null, m.HWnd)))
                            {
                                return false;
                            }
                            return true;
                        }
                        case 0xa1:
                        case 0xa4:
                        case 0xa7:
                            this.ProcessMouseButtonPressed(IntPtr.Zero, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam));
                            break;

                        case 0x100:
                        case 0x101:
                        case 0x102:
                        case 0x103:
                        case 260:
                        case 0x105:
                        case 0x106:
                        case 0x107:
                            if (!activeToolStrip.ContainsFocus)
                            {
                                m.HWnd = activeToolStrip.Handle;
                            }
                            break;

                        case 0x201:
                        case 0x204:
                        case 0x207:
                            this.ProcessMouseButtonPressed(m.HWnd, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam));
                            break;
                    }
                }
                return false;
            }

            private bool ProcessActivationChange()
            {
                int count = this._inputFilterQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    ToolStripDropDown activeToolStripInternal = this.GetActiveToolStripInternal() as ToolStripDropDown;
                    if ((activeToolStripInternal != null) && activeToolStripInternal.AutoClose)
                    {
                        activeToolStripInternal.Visible = false;
                    }
                }
                this.ExitMenuModeCore();
                return true;
            }

            internal static void ProcessMenuKeyDown(ref Message m)
            {
                Keys wParam = (Keys) ((int) m.WParam);
                ToolStrip strip = Control.FromHandleInternal(m.HWnd) as ToolStrip;
                if (((strip == null) || strip.IsDropDown) && ToolStripManager.IsMenuKey(wParam))
                {
                    if (!InMenuMode && MenuKeyToggle)
                    {
                        MenuKeyToggle = false;
                    }
                    else if (!MenuKeyToggle)
                    {
                        Instance.ShowUnderlines = true;
                    }
                }
            }

            private void ProcessMessages(bool process)
            {
                if (process)
                {
                    if (this._ensureMessageProcessingTimer == null)
                    {
                        this._ensureMessageProcessingTimer = new Timer();
                    }
                    this._ensureMessageProcessingTimer.Interval = 500;
                    this._ensureMessageProcessingTimer.Enabled = true;
                }
                else if (this._ensureMessageProcessingTimer != null)
                {
                    this._ensureMessageProcessingTimer.Enabled = false;
                    this._ensureMessageProcessingTimer.Dispose();
                    this._ensureMessageProcessingTimer = null;
                }
            }

            private void ProcessMouseButtonPressed(IntPtr hwndMouseMessageIsFrom, int x, int y)
            {
                int count = this._inputFilterQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    ToolStrip activeToolStripInternal = this.GetActiveToolStripInternal();
                    if (activeToolStripInternal == null)
                    {
                        break;
                    }
                    System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT {
                        x = x,
                        y = y
                    };
                    System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(activeToolStripInternal, hwndMouseMessageIsFrom), new HandleRef(activeToolStripInternal, activeToolStripInternal.Handle), pt, 1);
                    if (activeToolStripInternal.ClientRectangle.Contains(pt.x, pt.y))
                    {
                        break;
                    }
                    ToolStripDropDown activeToolStripDropDown = activeToolStripInternal as ToolStripDropDown;
                    if (activeToolStripDropDown != null)
                    {
                        if (((activeToolStripDropDown.OwnerToolStrip == null) || (activeToolStripDropDown.OwnerToolStrip.Handle != hwndMouseMessageIsFrom)) || ((activeToolStripDropDown.OwnerDropDownItem == null) || !activeToolStripDropDown.OwnerDropDownItem.DropDownButtonArea.Contains(x, y)))
                        {
                            CloseActiveDropDown(activeToolStripDropDown, ToolStripDropDownCloseReason.AppClicked);
                        }
                    }
                    else
                    {
                        activeToolStripInternal.NotifySelectionChange(null);
                        this.ExitMenuModeCore();
                    }
                }
            }

            internal static void RemoveActiveToolStrip(ToolStrip toolStrip)
            {
                Instance.RemoveActiveToolStripCore(toolStrip);
            }

            private void RemoveActiveToolStripCore(ToolStrip toolStrip)
            {
                this._toplevelToolStrip = null;
                if (this._inputFilterQueue != null)
                {
                    this._inputFilterQueue.Remove(toolStrip);
                }
            }

            internal static void ResumeMenuMode()
            {
                Instance._suspendMenuMode = false;
            }

            internal static void SetActiveToolStrip(ToolStrip toolStrip)
            {
                Instance.SetActiveToolStripCore(toolStrip);
            }

            internal static void SetActiveToolStrip(ToolStrip toolStrip, bool menuKeyPressed)
            {
                if (!InMenuMode && menuKeyPressed)
                {
                    Instance.ShowUnderlines = true;
                }
                Instance.SetActiveToolStripCore(toolStrip);
            }

            private void SetActiveToolStripCore(ToolStrip toolStrip)
            {
                if (toolStrip != null)
                {
                    if (toolStrip.IsDropDown)
                    {
                        ToolStripDropDown down = toolStrip as ToolStripDropDown;
                        if (!down.AutoClose)
                        {
                            IntPtr activeWindow = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
                            if (activeWindow != IntPtr.Zero)
                            {
                                this.ActiveHwndInternal = new HandleRef(this, activeWindow);
                            }
                            return;
                        }
                    }
                    toolStrip.KeyboardActive = true;
                    if (this._inputFilterQueue == null)
                    {
                        this._inputFilterQueue = new List<ToolStrip>();
                    }
                    else
                    {
                        ToolStrip activeToolStripInternal = this.GetActiveToolStripInternal();
                        if (activeToolStripInternal != null)
                        {
                            if (!activeToolStripInternal.IsDropDown)
                            {
                                this._inputFilterQueue.Remove(activeToolStripInternal);
                            }
                            else if (toolStrip.IsDropDown && (ToolStripDropDown.GetFirstDropDown(toolStrip) != ToolStripDropDown.GetFirstDropDown(activeToolStripInternal)))
                            {
                                this._inputFilterQueue.Remove(activeToolStripInternal);
                                (activeToolStripInternal as ToolStripDropDown).DismissAll();
                            }
                        }
                    }
                    this._toplevelToolStrip = null;
                    if (!this._inputFilterQueue.Contains(toolStrip))
                    {
                        this._inputFilterQueue.Add(toolStrip);
                    }
                    if (!InMenuMode && (this._inputFilterQueue.Count > 0))
                    {
                        this.EnterMenuModeCore();
                    }
                    if ((!this._caretHidden && toolStrip.IsDropDown) && InMenuMode)
                    {
                        this._caretHidden = true;
                        System.Windows.Forms.SafeNativeMethods.HideCaret(System.Windows.Forms.NativeMethods.NullHandleRef);
                    }
                }
            }

            internal static void SuspendMenuMode()
            {
                Instance._suspendMenuMode = true;
            }

            internal static HandleRef ActiveHwnd
            {
                get
                {
                    return Instance.ActiveHwndInternal;
                }
            }

            private HandleRef ActiveHwndInternal
            {
                get
                {
                    return this._activeHwnd;
                }
                set
                {
                    if (this._activeHwnd.Handle != value.Handle)
                    {
                        Control control = null;
                        if (this._activeHwnd.Handle != IntPtr.Zero)
                        {
                            control = Control.FromHandleInternal(this._activeHwnd.Handle);
                            if (control != null)
                            {
                                control.HandleCreated -= new EventHandler(this.OnActiveHwndHandleCreated);
                            }
                        }
                        this._activeHwnd = value;
                        control = Control.FromHandleInternal(this._activeHwnd.Handle);
                        if (control != null)
                        {
                            control.HandleCreated += new EventHandler(this.OnActiveHwndHandleCreated);
                        }
                    }
                }
            }

            internal static bool InMenuMode
            {
                get
                {
                    return Instance._inMenuMode;
                }
            }

            internal static ToolStripManager.ModalMenuFilter Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new ToolStripManager.ModalMenuFilter();
                    }
                    return _instance;
                }
            }

            internal static bool MenuKeyToggle
            {
                get
                {
                    return Instance.menuKeyToggle;
                }
                set
                {
                    if (Instance.menuKeyToggle != value)
                    {
                        Instance.menuKeyToggle = value;
                    }
                }
            }

            private HostedWindowsFormsMessageHook MessageHook
            {
                get
                {
                    if (this.messageHook == null)
                    {
                        this.messageHook = new HostedWindowsFormsMessageHook();
                    }
                    return this.messageHook;
                }
            }

            public bool ShowUnderlines
            {
                get
                {
                    return this._showUnderlines;
                }
                set
                {
                    if (this._showUnderlines != value)
                    {
                        this._showUnderlines = value;
                        ToolStripManager.NotifyMenuModeChange(true, false);
                    }
                }
            }

            private class HostedWindowsFormsMessageHook
            {
                private System.Windows.Forms.NativeMethods.HookProc hookProc;
                private bool isHooked;
                private IntPtr messageHookHandle = IntPtr.Zero;

                private void InstallMessageHook()
                {
                    lock (this)
                    {
                        if (this.messageHookHandle == IntPtr.Zero)
                        {
                            this.hookProc = new System.Windows.Forms.NativeMethods.HookProc(this.MessageHookProc);
                            this.messageHookHandle = System.Windows.Forms.UnsafeNativeMethods.SetWindowsHookEx(3, this.hookProc, new HandleRef(null, IntPtr.Zero), System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId());
                            if (this.messageHookHandle != IntPtr.Zero)
                            {
                                this.isHooked = true;
                            }
                        }
                    }
                }

                private unsafe IntPtr MessageHookProc(int nCode, IntPtr wparam, IntPtr lparam)
                {
                    if (((nCode == 0) && this.isHooked) && (((int) wparam) == 1))
                    {
                        System.Windows.Forms.NativeMethods.MSG* msgPtr = (System.Windows.Forms.NativeMethods.MSG*) lparam;
                        if ((msgPtr != null) && Application.ThreadContext.FromCurrent().PreTranslateMessage(ref (System.Windows.Forms.NativeMethods.MSG) ref msgPtr))
                        {
                            msgPtr->message = 0;
                        }
                    }
                    return System.Windows.Forms.UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, this.messageHookHandle), nCode, wparam, lparam);
                }

                private void UninstallMessageHook()
                {
                    lock (this)
                    {
                        if (this.messageHookHandle != IntPtr.Zero)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, this.messageHookHandle));
                            this.hookProc = null;
                            this.messageHookHandle = IntPtr.Zero;
                            this.isHooked = false;
                        }
                    }
                }

                public bool HookMessages
                {
                    get
                    {
                        return (this.messageHookHandle != IntPtr.Zero);
                    }
                    set
                    {
                        if (value)
                        {
                            this.InstallMessageHook();
                        }
                        else
                        {
                            this.UninstallMessageHook();
                        }
                    }
                }
            }
        }
    }
}

