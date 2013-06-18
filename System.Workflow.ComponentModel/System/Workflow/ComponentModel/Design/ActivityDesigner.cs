namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [SRCategory("ActivityDesigners", "System.Workflow.ComponentModel.Design.DesignerResources"), ActivityDesignerTheme(typeof(ActivityDesignerTheme)), DesignerSerializer(typeof(ActivityDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer)), ToolboxItemFilter("Microsoft.Workflow.VSDesigner", ToolboxItemFilterType.Require), ToolboxItemFilter("System.Workflow.ComponentModel.Design.ActivitySet", ToolboxItemFilterType.Custom)]
    public class ActivityDesigner : IDesignerFilter, IToolboxUser, IPersistUIState, IWorkflowDesignerMessageSink, IWorkflowRootDesigner, IRootDesigner, IDesigner, IDisposable, IConnectableDesigner
    {
        private ActivityDesignerAccessibleObject accessibilityObject;
        private System.Workflow.ComponentModel.Activity activity;
        private List<DesignerAction> designerActions;
        private ActivityDesignerVerbCollection designerVerbs;
        private DrawingStates drawingState;
        private const uint FrameworkVersion_3_0 = 0x30000;
        private const uint FrameworkVersion_3_5 = 0x30005;
        private System.Drawing.Image image;
        private CompositeActivityDesigner invokingDesigner;
        private bool isVisible = true;
        private Point location = Point.Empty;
        private const int MaximumCharsPerLine = 8;
        private const int MaximumDescriptionLength = 80;
        private const int MaximumIdentifierLength = 0x19;
        private const int MaximumTextLines = 2;
        private string rulesText;
        private System.Drawing.Size size = System.Drawing.Size.Empty;
        private SmartTag smartTag = new SmartTag();
        private bool smartTagVisible;
        private string text = string.Empty;
        private System.Drawing.Size textSize = System.Drawing.Size.Empty;
        private WorkflowView workflowView;

        public virtual bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivityDesigner");
            }
            return true;
        }

        protected virtual bool CanConnect(ConnectionPoint source, ConnectionPoint target)
        {
            return true;
        }

        internal static ActivityDesigner CreateDesigner(IServiceProvider serviceProvider, System.Workflow.ComponentModel.Activity activity)
        {
            IDesigner designer = null;
            System.Type type = GetDesignerType(serviceProvider, activity.GetType(), typeof(IDesigner));
            if (type == null)
            {
                type = GetDesignerType(serviceProvider, activity.GetType(), null);
            }
            if (type != null)
            {
                try
                {
                    designer = Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null) as IDesigner;
                    designer.Initialize(activity);
                }
                catch
                {
                }
            }
            return (designer as ActivityDesigner);
        }

        internal static ActivityDesigner CreateTransientDesigner(System.Workflow.ComponentModel.Activity activity)
        {
            ActivityDesigner designer = new ActivityDesigner();
            ActivityDesignerTheme designerTheme = designer.DesignerTheme;
            using (Bitmap bitmap = new Bitmap(designerTheme.Size.Width, designerTheme.Size.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    designer.Image = ActivityToolboxItem.GetToolboxImage(activity.GetType());
                    designer.Location = new Point(-1, -1);
                    designer.Location = Point.Empty;
                    designer.Size = designer.OnLayoutSize(new ActivityDesignerLayoutEventArgs(graphics, designer.DesignerTheme));
                }
            }
            return designer;
        }

        protected virtual WorkflowView CreateView(ViewTechnology viewTechnology)
        {
            return new WorkflowView(this.Activity.Site) { ShowToolContainer = true };
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.IsRootDesigner)
            {
                if (this.workflowView != null)
                {
                    this.workflowView.Dispose();
                    this.workflowView = null;
                }
                IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (((host != null) && (this.InvokingDesigner == null)) && (this.Activity == host.RootComponent))
                {
                    host.LoadComplete -= new EventHandler(this.OnLoadComplete);
                }
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRenamed);
                }
            }
        }

        protected virtual void DoDefaultAction()
        {
            if (!this.IsLocked)
            {
                DefaultEventAttribute attribute = TypeDescriptor.GetAttributes(this.Activity)[typeof(DefaultEventAttribute)] as DefaultEventAttribute;
                if (((attribute != null) && (attribute.Name != null)) && (attribute.Name.Length != 0))
                {
                    ActivityBindPropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.Activity)[attribute.Name] as ActivityBindPropertyDescriptor;
                    if ((descriptor != null) && !(descriptor.GetValue(this.Activity) is ActivityBind))
                    {
                        IEventBindingService service = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                        if (service != null)
                        {
                            EventDescriptor e = service.GetEvent(descriptor.RealPropertyDescriptor);
                            if (e != null)
                            {
                                string str = descriptor.RealPropertyDescriptor.GetValue(this.Activity) as string;
                                if (string.IsNullOrEmpty(str))
                                {
                                    str = DesignerHelpers.CreateUniqueMethodName(this.Activity, e.Name, e.EventType);
                                }
                                descriptor.SetValue(this.Activity, str);
                                service.ShowCode(this.Activity, e);
                            }
                        }
                    }
                }
            }
        }

        public void EnsureVisible()
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.EnsureVisible(this.Activity);
            }
        }

        ~ActivityDesigner()
        {
            this.Dispose(false);
        }

        internal static string GetActivityDescription(System.Type activityType)
        {
            if (activityType == null)
            {
                return null;
            }
            object[] customAttributes = activityType.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((customAttributes != null) && (customAttributes.GetLength(0) == 0))
            {
                customAttributes = activityType.GetCustomAttributes(typeof(DescriptionAttribute), true);
            }
            DescriptionAttribute attribute = ((customAttributes != null) && (customAttributes.GetLength(0) > 0)) ? (customAttributes[0] as DescriptionAttribute) : null;
            if (attribute == null)
            {
                return string.Empty;
            }
            return attribute.Description;
        }

        public virtual ReadOnlyCollection<ConnectionPoint> GetConnectionPoints(DesignerEdges edges)
        {
            List<ConnectionPoint> list = new List<ConnectionPoint>();
            if ((edges & DesignerEdges.Left) > DesignerEdges.None)
            {
                for (int i = 0; i < this.GetConnections(DesignerEdges.Left).Count; i++)
                {
                    list.Add(new ConnectionPoint(this, DesignerEdges.Left, i));
                }
            }
            if ((edges & DesignerEdges.Right) > DesignerEdges.None)
            {
                for (int j = 0; j < this.GetConnections(DesignerEdges.Right).Count; j++)
                {
                    list.Add(new ConnectionPoint(this, DesignerEdges.Right, j));
                }
            }
            if ((edges & DesignerEdges.Top) > DesignerEdges.None)
            {
                for (int k = 0; k < this.GetConnections(DesignerEdges.Top).Count; k++)
                {
                    list.Add(new ConnectionPoint(this, DesignerEdges.Top, k));
                }
            }
            if ((edges & DesignerEdges.Bottom) > DesignerEdges.None)
            {
                for (int m = 0; m < this.GetConnections(DesignerEdges.Bottom).Count; m++)
                {
                    list.Add(new ConnectionPoint(this, DesignerEdges.Bottom, m));
                }
            }
            return list.AsReadOnly();
        }

        protected internal virtual ReadOnlyCollection<Point> GetConnections(DesignerEdges edges)
        {
            Rectangle bounds = this.Bounds;
            List<Point> list = new List<Point>();
            if ((edges & DesignerEdges.Left) > DesignerEdges.None)
            {
                list.Add(new Point(bounds.Left, bounds.Top + (bounds.Height / 2)));
            }
            if ((edges & DesignerEdges.Top) > DesignerEdges.None)
            {
                list.Add(new Point(bounds.Left + (bounds.Width / 2), bounds.Top));
            }
            if ((edges & DesignerEdges.Right) > DesignerEdges.None)
            {
                list.Add(new Point(bounds.Right, bounds.Top + (bounds.Height / 2)));
            }
            if ((edges & DesignerEdges.Bottom) > DesignerEdges.None)
            {
                list.Add(new Point(bounds.Left + (bounds.Width / 2), bounds.Bottom));
            }
            return list.AsReadOnly();
        }

        internal static ActivityDesigner GetDesigner(System.Workflow.ComponentModel.Activity activity)
        {
            ActivityDesigner designer = null;
            if ((activity != null) && (activity.Site != null))
            {
                IDesignerHost service = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    designer = service.GetDesigner(activity) as ActivityDesigner;
                }
            }
            return designer;
        }

        internal static System.Type GetDesignerType(IServiceProvider serviceProvider, System.Type activityType, System.Type designerBaseType)
        {
            System.Type type = null;
            foreach (Attribute attribute in TypeDescriptor.GetAttributes(activityType))
            {
                DesignerAttribute attribute2 = attribute as DesignerAttribute;
                if ((attribute2 != null) && ((designerBaseType == null) || (attribute2.DesignerBaseTypeName == designerBaseType.AssemblyQualifiedName)))
                {
                    int index = attribute2.DesignerTypeName.IndexOf(',');
                    string name = (index >= 0) ? attribute2.DesignerTypeName.Substring(0, index) : attribute2.DesignerTypeName;
                    type = activityType.Assembly.GetType(name);
                    if ((type == null) && (serviceProvider != null))
                    {
                        ITypeResolutionService service = serviceProvider.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
                        type = (service != null) ? service.GetType(attribute2.DesignerTypeName) : null;
                    }
                    if (type == null)
                    {
                        type = System.Type.GetType(attribute2.DesignerTypeName);
                    }
                    return type;
                }
            }
            return type;
        }

        internal static CompositeActivityDesigner GetParentDesigner(object obj)
        {
            CompositeActivityDesigner parentDesigner = null;
            if (obj is System.Workflow.ComponentModel.Design.HitTestInfo)
            {
                return (((System.Workflow.ComponentModel.Design.HitTestInfo) obj).AssociatedDesigner as CompositeActivityDesigner);
            }
            if (obj is System.Workflow.ComponentModel.Activity)
            {
                ActivityDesigner designer2 = GetDesigner(obj as System.Workflow.ComponentModel.Activity);
                if (designer2 != null)
                {
                    parentDesigner = designer2.ParentDesigner;
                }
            }
            return parentDesigner;
        }

        public System.Drawing.Image GetPreviewImage(Graphics compatibleGraphics)
        {
            if (compatibleGraphics == null)
            {
                throw new ArgumentNullException("compatibleGraphics");
            }
            if (this.Activity.Site == null)
            {
                ((IWorkflowDesignerMessageSink) this).OnLayoutSize(compatibleGraphics);
                ((IWorkflowDesignerMessageSink) this).OnLayoutPosition(compatibleGraphics);
            }
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Bitmap image = new Bitmap(this.Size.Width + (4 * ambientTheme.Margin.Width), this.Size.Height + (4 * ambientTheme.Margin.Height), PixelFormat.Format32bppArgb);
            GlyphManager manager = ((this.Activity != null) && (this.Activity.Site != null)) ? (this.GetService(typeof(IDesignerGlyphProviderService)) as GlyphManager) : null;
            using (Graphics graphics = Graphics.FromImage(image))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(0, 0xff, 0, 0xff)))
                {
                    graphics.FillRectangle(brush, 0, 0, image.Width, image.Height);
                    graphics.TranslateTransform((float) (-this.Location.X + (2 * ambientTheme.Margin.Width)), (float) (-this.Location.Y + (2 * ambientTheme.Margin.Height)));
                    Rectangle bounds = this.Bounds;
                    Rectangle viewPort = new Rectangle(bounds.Location, new System.Drawing.Size(bounds.Width + 1, bounds.Height + 1));
                    Queue<ActivityDesigner> queue = new Queue<ActivityDesigner>();
                    queue.Enqueue(this);
                    while (queue.Count > 0)
                    {
                        ActivityDesigner designer = queue.Dequeue();
                        designer.OnPaint(new ActivityDesignerPaintEventArgs(graphics, designer.Bounds, viewPort, designer.DesignerTheme));
                        ActivityDesignerGlyphCollection glyphs = (manager != null) ? manager.GetDesignerGlyphs(designer) : this.Glyphs;
                        foreach (DesignerGlyph glyph in glyphs)
                        {
                            if (!(glyph is SelectionGlyph))
                            {
                                glyph.Draw(graphics, designer);
                            }
                        }
                        CompositeActivityDesigner designer2 = designer as CompositeActivityDesigner;
                        if (designer2 != null)
                        {
                            foreach (ActivityDesigner designer3 in designer2.ContainedDesigners)
                            {
                                if (((designer3 != null) && designer2.Expanded) && designer2.IsContainedDesignerVisible(designer3))
                                {
                                    queue.Enqueue(designer3);
                                }
                            }
                        }
                    }
                }
            }
            return image;
        }

        public static ActivityDesigner GetRootDesigner(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            IDesignerHost service = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service == null)
            {
                return null;
            }
            return GetDesigner(service.RootComponent as System.Workflow.ComponentModel.Activity);
        }

        internal static ActivityDesigner GetSafeRootDesigner(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return null;
            }
            return GetRootDesigner(serviceProvider);
        }

        protected object GetService(System.Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if ((this.activity != null) && (this.activity.Site != null))
            {
                return this.activity.Site.GetService(serviceType);
            }
            return null;
        }

        private bool GetVisible()
        {
            System.Workflow.ComponentModel.Activity activity = this.Activity;
            if (activity != null)
            {
                while (activity != null)
                {
                    ActivityDesigner containedDesigner = GetDesigner(activity);
                    if (containedDesigner != null)
                    {
                        CompositeActivityDesigner parentDesigner = containedDesigner.ParentDesigner;
                        if ((parentDesigner == null) && (containedDesigner != null))
                        {
                            return true;
                        }
                        if (((parentDesigner == null) || !parentDesigner.Expanded) || !parentDesigner.IsContainedDesignerVisible(containedDesigner))
                        {
                            return false;
                        }
                        activity = parentDesigner.Activity;
                    }
                    else
                    {
                        activity = null;
                    }
                }
                return true;
            }
            return false;
        }

        public virtual System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
        {
            System.Workflow.ComponentModel.Design.HitTestInfo nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
            if ((this.ParentDesigner is FreeformActivityDesigner) || ((this.ParentDesigner == null) && (this is FreeformActivityDesigner)))
            {
                ReadOnlyCollection<ConnectionPoint> connectionPoints = this.GetConnectionPoints(DesignerEdges.All);
                for (int i = 0; i < connectionPoints.Count; i++)
                {
                    if (connectionPoints[i].Bounds.Contains(point))
                    {
                        nowhere = new ConnectionPointHitTestInfo(connectionPoints[i]);
                        break;
                    }
                }
            }
            Rectangle bounds = this.Bounds;
            if (bounds.Contains(point) && (nowhere == System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere))
            {
                HitTestLocations location = bounds.Contains(point) ? HitTestLocations.Designer : HitTestLocations.None;
                Rectangle rectangle2 = new Rectangle(bounds.Left, bounds.Top, bounds.Left - bounds.Left, bounds.Height);
                location |= rectangle2.Contains(point) ? HitTestLocations.Left : location;
                rectangle2 = new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height - bounds.Height);
                location |= rectangle2.Contains(point) ? HitTestLocations.Top : location;
                rectangle2 = new Rectangle(bounds.Right, bounds.Top, bounds.Width - bounds.Width, bounds.Height);
                location |= rectangle2.Contains(point) ? HitTestLocations.Right : location;
                rectangle2 = new Rectangle(bounds.Left, bounds.Bottom, bounds.Width, bounds.Bottom - bounds.Bottom);
                location |= rectangle2.Contains(point) ? HitTestLocations.Bottom : location;
                nowhere = new System.Workflow.ComponentModel.Design.HitTestInfo(this, location);
            }
            return nowhere;
        }

        protected virtual void Initialize(System.Workflow.ComponentModel.Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (this.IsRootDesigner)
            {
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((host != null) && (this.InvokingDesigner == null))
                {
                    host.LoadComplete += new EventHandler(this.OnLoadComplete);
                }
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRenamed);
                }
            }
            this.Text = !string.IsNullOrEmpty(activity.Name) ? activity.Name : activity.GetType().Name;
            this.Image = this.StockImage;
            this.RefreshDesignerVerbs();
            if (this.IsLocked)
            {
                DesignerHelpers.MakePropertiesReadOnly(activity.Site, activity);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Rectangle InternalRectangleToScreen(Rectangle rectangle)
        {
            return this.RectangleToScreen(rectangle);
        }

        public void Invalidate()
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.InvalidateLogicalRectangle(this.Bounds);
                GlyphManager service = this.GetService(typeof(IDesignerGlyphProviderService)) as GlyphManager;
                if (service != null)
                {
                    foreach (DesignerGlyph glyph in service.GetDesignerGlyphs(this))
                    {
                        parentView.InvalidateLogicalRectangle(glyph.GetBounds(this, false));
                    }
                }
            }
        }

        public void Invalidate(Rectangle rectangle)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                rectangle = Rectangle.Intersect(this.Bounds, rectangle);
                parentView.InvalidateLogicalRectangle(rectangle);
            }
        }

        public static bool IsCommentedActivity(System.Workflow.ComponentModel.Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            bool flag = false;
            for (CompositeActivity activity2 = activity.Parent; (activity2 != null) && !flag; activity2 = activity2.Parent)
            {
                flag = (activity2 != null) && !activity2.Enabled;
            }
            return flag;
        }

        protected virtual bool IsSupportedActivityType(System.Type activityType)
        {
            return true;
        }

        protected virtual void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
        }

        protected virtual void OnActivityChanged(ActivityChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (((e.Member != null) && (e.Member.Name != null)) && e.Member.Name.Equals("Name"))
            {
                this.Text = this.Activity.Name;
            }
            if (!(e.OldValue is ActivityCollectionChangeEventArgs))
            {
                this.RefreshDesignerVerbs();
            }
            IUIService service = this.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                service.SetUIDirty();
            }
            this.rulesText = null;
        }

        protected virtual void OnBeginResizing(ActivityDesignerResizeEventArgs e)
        {
        }

        private void OnBindProperty(object sender, EventArgs e)
        {
            IExtendedUIService service = this.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (service != null)
            {
                BindUITypeEditor.EditValue(service.GetSelectedPropertyContext());
            }
        }

        private void OnBindPropertyStatusUpdate(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb != null)
            {
                bool flag = false;
                string str = null;
                IExtendedUIService service = this.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
                if (service != null)
                {
                    ITypeDescriptorContext selectedPropertyContext = service.GetSelectedPropertyContext();
                    flag = ((selectedPropertyContext != null) && ActivityBindPropertyDescriptor.IsBindableProperty(selectedPropertyContext.PropertyDescriptor)) && !selectedPropertyContext.PropertyDescriptor.IsReadOnly;
                    str = (selectedPropertyContext != null) ? selectedPropertyContext.PropertyDescriptor.Name : null;
                }
                verb.Properties["Text"] = ((str != null) && verb.Enabled) ? string.Format(CultureInfo.CurrentCulture, DR.GetString("BindSelectedPropertyFormat", new object[0]), new object[] { str }) : DR.GetString("BindSelectedProperty", new object[0]);
                verb.Enabled = flag && !this.IsLocked;
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if ((((e.Component != null) && (e.OldValue is ActivityBind)) && !(e.NewValue is ActivityBind)) || (!(e.OldValue is ActivityBind) && (e.NewValue is ActivityBind)))
            {
                TypeDescriptor.Refresh(e.Component);
            }
            IReferenceService service = this.GetService(typeof(IReferenceService)) as IReferenceService;
            System.Workflow.ComponentModel.Activity activity = (service != null) ? (service.GetComponent(e.Component) as System.Workflow.ComponentModel.Activity) : (e.Component as System.Workflow.ComponentModel.Activity);
            if (activity != null)
            {
                ActivityDesigner designer = GetDesigner(activity);
                if (designer != null)
                {
                    designer.OnActivityChanged(new ActivityChangedEventArgs(activity, e.Member, e.OldValue, e.NewValue));
                }
            }
        }

        private void OnComponentRenamed(object sender, ComponentRenameEventArgs e)
        {
            if (GetDesigner(e.Component as System.Workflow.ComponentModel.Activity) != null)
            {
                this.Text = this.Activity.Name;
            }
        }

        protected virtual void OnConnected(ConnectionPoint source, ConnectionPoint target)
        {
        }

        protected virtual void OnDragDrop(ActivityDragEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnDragEnter(ActivityDragEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnDragLeave()
        {
        }

        protected virtual void OnDragOver(ActivityDragEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected virtual void OnEndResizing()
        {
            this.PerformLayout();
        }

        protected internal virtual void OnExecuteDesignerAction(DesignerAction designerAction)
        {
            if (designerAction == null)
            {
                throw new ArgumentNullException("designerAction");
            }
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                service.SetSelectedComponents(new object[] { this.Activity }, SelectionTypes.Replace);
            }
            string propertyName = designerAction.PropertyName;
            if ((propertyName != null) && (propertyName.Length > 0))
            {
                IExtendedUIService service2 = this.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
                if (service2 != null)
                {
                    service2.NavigateToProperty(propertyName);
                }
            }
        }

        private void OnFirstIdle(object sender, EventArgs e)
        {
            WorkflowView view = ((IRootDesigner) this).GetView(ViewTechnology.Default) as WorkflowView;
            if (view != null)
            {
                view.Idle -= new EventHandler(this.OnFirstIdle);
            }
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if ((service != null) && (service.SelectionCount == 0))
            {
                service.SetSelectedComponents(new object[] { this.Activity }, SelectionTypes.Replace);
            }
            DesignerHelpers.RefreshDesignerActions(this.Activity.Site);
            this.Invalidate();
        }

        private void OnGenerateEventHandler(object sender, EventArgs e)
        {
            if ((sender is DesignerVerb) && (Helpers.GetRootActivity(this.Activity) != null))
            {
                PropertyDescriptor property = null;
                foreach (PropertyDescriptor descriptor2 in this.EventHandlerProperties)
                {
                    System.Type delegateType = PropertyDescriptorUtils.GetBaseType(descriptor2, this.Activity, this.Activity.Site);
                    if (delegateType != null)
                    {
                        object obj2 = descriptor2.GetValue(this.Activity);
                        if (!(obj2 is string) || string.IsNullOrEmpty((string) obj2))
                        {
                            obj2 = DesignerHelpers.CreateUniqueMethodName(this.Activity, descriptor2.Name, delegateType);
                        }
                        descriptor2.SetValue(this.Activity, obj2);
                        property = descriptor2;
                    }
                }
                IEventBindingService service = this.GetService(typeof(IEventBindingService)) as IEventBindingService;
                if (service != null)
                {
                    if (property is DynamicPropertyDescriptor)
                    {
                        property = ((DynamicPropertyDescriptor) property).RealPropertyDescriptor;
                    }
                    EventDescriptor descriptor3 = service.GetEvent(property);
                    if (descriptor3 != null)
                    {
                        service.ShowCode(this.Activity, descriptor3);
                    }
                    else
                    {
                        service.ShowCode();
                    }
                }
            }
        }

        private void OnGenerateEventHandlerStatusUpdate(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb != null)
            {
                bool flag = false;
                foreach (PropertyDescriptor descriptor in this.EventHandlerProperties)
                {
                    if (descriptor.GetValue(this.Activity) == null)
                    {
                        flag = true;
                        break;
                    }
                }
                verb.Enabled = flag;
            }
        }

        protected virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual System.Drawing.Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            this.isVisible = this.GetVisible();
            if (!string.IsNullOrEmpty(this.Text))
            {
                System.Drawing.Size size = ActivityDesignerPaint.MeasureString(e.Graphics, e.DesignerTheme.BoldFont, this.Text, StringAlignment.Center, System.Drawing.Size.Empty);
                System.Drawing.Size size2 = size;
                size2.Width /= this.Text.Length;
                size2.Width += ((size2.Width % this.Text.Length) > 0) ? 1 : 0;
                size2.Width *= Math.Min(this.Text.Length, 7);
                this.textSize.Width = this.MinimumSize.Width - (2 * e.AmbientTheme.Margin.Width);
                if (this.Image != null)
                {
                    this.textSize.Width -= e.DesignerTheme.ImageSize.Width + e.AmbientTheme.Margin.Width;
                }
                this.textSize.Width = Math.Min(this.textSize.Width, size.Width);
                this.textSize.Width = Math.Max(this.textSize.Width, size2.Width);
                this.textSize.Height = size2.Height;
                int num = size.Width / this.textSize.Width;
                num += ((size.Width % this.textSize.Width) > 0) ? 1 : 0;
                num = Math.Min(num, 2);
                this.textSize.Height *= num;
            }
            else
            {
                this.textSize = System.Drawing.Size.Empty;
            }
            System.Drawing.Size empty = System.Drawing.Size.Empty;
            empty.Width = ((2 * e.AmbientTheme.Margin.Width) + ((this.Image != null) ? (e.DesignerTheme.ImageSize.Width + e.AmbientTheme.Margin.Width) : 0)) + this.textSize.Width;
            empty.Height = (e.AmbientTheme.Margin.Height + Math.Max(e.DesignerTheme.ImageSize.Height, this.textSize.Height)) + e.AmbientTheme.Margin.Height;
            return empty;
        }

        private void OnLoadComplete(object sender, EventArgs e)
        {
            WorkflowView view = ((IRootDesigner) this).GetView(ViewTechnology.Default) as WorkflowView;
            if (view != null)
            {
                view.Idle += new EventHandler(this.OnFirstIdle);
            }
        }

        protected virtual void OnMouseCaptureChanged()
        {
        }

        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnMouseDragBegin(Point initialDragPoint, MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnMouseDragEnd()
        {
        }

        protected virtual void OnMouseDragMove(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.IsVisible)
            {
                if (this.ShowSmartTag)
                {
                    this.SmartTagVisible = true;
                }
                this.ShowInfoTip(this.InfoTipTitle, this.InfoTipText);
            }
        }

        protected virtual void OnMouseHover(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.IsVisible)
            {
                this.ShowInfoTip(this.InfoTipTitle, this.InfoTipText);
            }
        }

        protected virtual void OnMouseLeave()
        {
            if (this.ShowSmartTag)
            {
                this.SmartTagVisible = false;
            }
            this.ShowInfoTip(string.Empty);
        }

        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.IsVisible)
            {
                this.ShowInfoTip(this.InfoTipTitle, this.InfoTipText);
            }
        }

        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        private void OnMoveBranch(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                CompositeActivityDesigner.MoveDesigners(this, (bool) verb.Properties[DesignerUserDataKeys.MoveBranchKey]);
            }
        }

        protected virtual void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ActivityDesignerPaint.DrawDesignerBackground(e.Graphics, this);
            if (!string.IsNullOrEmpty(this.Text) && !this.TextRectangle.Size.IsEmpty)
            {
                Font font = this.SmartTagVisible ? e.DesignerTheme.BoldFont : e.DesignerTheme.Font;
                ActivityDesignerPaint.DrawText(e.Graphics, font, this.Text, this.TextRectangle, StringAlignment.Near, e.AmbientTheme.TextQuality, e.DesignerTheme.ForegroundBrush);
            }
            if ((this.Image != null) && !this.ImageRectangle.Size.IsEmpty)
            {
                ActivityDesignerPaint.DrawImage(e.Graphics, this.Image, this.ImageRectangle, DesignerContentAlignment.Fill);
            }
        }

        protected virtual void OnProcessMessage(Message message)
        {
        }

        private void OnPromoteBindings(object sender, EventArgs e)
        {
            if (sender is DesignerVerb)
            {
                IServiceProvider service = this.GetService(typeof(DesignSurface)) as IServiceProvider;
                List<CustomProperty> customProperties = CustomActivityDesignerHelper.GetCustomProperties(service);
                if (customProperties != null)
                {
                    System.Type customActivityType = CustomActivityDesignerHelper.GetCustomActivityType(service);
                    List<string> list2 = new List<string>();
                    foreach (MemberInfo info in customActivityType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                    {
                        if (!list2.Contains(info.Name))
                        {
                            list2.Add(info.Name);
                        }
                    }
                    PropertyDescriptor[] bindableProperties = this.BindableProperties;
                    Dictionary<PropertyDescriptor, ActivityBind> dictionary = new Dictionary<PropertyDescriptor, ActivityBind>();
                    foreach (PropertyDescriptor descriptor in bindableProperties)
                    {
                        if (!(descriptor.GetValue(this.Activity) is ActivityBind))
                        {
                            CustomProperty item = CustomProperty.CreateCustomProperty(this.Activity.Site, DesignerHelpers.GenerateUniqueIdentifier(this.Activity.Site, this.Activity.Name + "_" + descriptor.Name, list2.ToArray()), descriptor, this.Activity);
                            customProperties.Add(item);
                            list2.Add(item.Name);
                            dictionary.Add(descriptor, new ActivityBind(ActivityBind.GetRelativePathExpression(Helpers.GetRootActivity(this.Activity), this.Activity), item.Name));
                        }
                    }
                    CustomActivityDesignerHelper.SetCustomProperties(customProperties, service);
                    foreach (PropertyDescriptor descriptor2 in dictionary.Keys)
                    {
                        descriptor2.SetValue(this.Activity, dictionary[descriptor2]);
                    }
                }
            }
        }

        private void OnPromoteBindingsStatusUpdate(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb != null)
            {
                bool flag = false;
                foreach (PropertyDescriptor descriptor in this.BindableProperties)
                {
                    if (!(descriptor.GetValue(this.Activity) is ActivityBind))
                    {
                        flag = true;
                        break;
                    }
                }
                verb.Enabled = flag;
            }
        }

        protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected virtual void OnResizing(ActivityDesignerResizeEventArgs e)
        {
            FreeformActivityDesigner.SetDesignerBounds(this, e.Bounds);
        }

        protected virtual void OnScroll(ScrollBar sender, int value)
        {
        }

        protected virtual void OnShowSmartTagVerbs(Point smartTagPoint)
        {
            ActivityDesignerVerb[] array = null;
            this.SmartTagVerbs.CopyTo(array, 0);
            DesignerHelpers.ShowDesignerVerbs(this, this.PointToScreen(smartTagPoint), array);
        }

        protected virtual void OnSmartTagVisibilityChanged(bool visible)
        {
            Rectangle bounds = this.smartTag.GetBounds(this, true);
            Rectangle textRectangle = this.TextRectangle;
            if (!textRectangle.Size.IsEmpty)
            {
                bounds = Rectangle.Union(textRectangle, bounds);
            }
            this.Invalidate(bounds);
        }

        private void OnStatusMoveBranch(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool flag = false;
                CompositeActivityDesigner parentDesigner = this.ParentDesigner;
                if (!this.IsLocked && (parentDesigner != null))
                {
                    List<System.Workflow.ComponentModel.Activity> list = new List<System.Workflow.ComponentModel.Activity>();
                    foreach (System.Workflow.ComponentModel.Activity activity in ((CompositeActivity) parentDesigner.Activity).Activities)
                    {
                        if (!Helpers.IsAlternateFlowActivity(activity))
                        {
                            list.Add(activity);
                        }
                    }
                    bool flag2 = (bool) verb.Properties[DesignerUserDataKeys.MoveBranchKey];
                    int index = list.IndexOf(this.Activity);
                    int num2 = (index > 0) ? (index - 1) : -1;
                    flag = (index >= 0) && (((flag2 && (index > 0)) && ((index - num2) > 0)) || (!flag2 && (index < (list.Count - 1))));
                }
                verb.Visible = (parentDesigner is ParallelActivityDesigner) || ((parentDesigner is ActivityPreviewDesigner) && !Helpers.IsAlternateFlowActivity(this.Activity));
                verb.Enabled = flag;
            }
        }

        protected virtual void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            if (newTheme == null)
            {
                throw new ArgumentNullException("newTheme");
            }
            this.Image = this.StockImage;
        }

        protected void PerformLayout()
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.PerformLayout(false);
            }
        }

        protected Point PointToLogical(Point point)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                return parentView.ScreenPointToLogical(point);
            }
            return point;
        }

        protected Point PointToScreen(Point point)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                return parentView.LogicalPointToScreen(point);
            }
            return point;
        }

        protected virtual void PostFilterAttributes(IDictionary attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
        }

        protected virtual void PostFilterEvents(IDictionary events)
        {
            if (events == null)
            {
                throw new ArgumentNullException("events");
            }
        }

        protected virtual void PostFilterProperties(IDictionary properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            StringCollection strings = new StringCollection();
            foreach (DictionaryEntry entry in properties)
            {
                PropertyDescriptor descriptor = entry.Value as PropertyDescriptor;
                ExtenderProvidedPropertyAttribute attribute = descriptor.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
                if (((attribute != null) && (attribute.Provider != null)) && !attribute.Provider.CanExtend(this.Activity))
                {
                    strings.Add(entry.Key as string);
                }
            }
            foreach (string str in strings)
            {
                properties.Remove(str);
            }
            PropertyDescriptorFilter.FilterProperties(this.Activity.Site, this.Activity, properties);
        }

        protected virtual void PreFilterAttributes(IDictionary attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
        }

        protected virtual void PreFilterEvents(IDictionary events)
        {
            if (events == null)
            {
                throw new ArgumentNullException("events");
            }
        }

        protected virtual void PreFilterProperties(IDictionary properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
        }

        protected Rectangle RectangleToLogical(Rectangle rectangle)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                return new Rectangle(this.PointToLogical(rectangle.Location), parentView.ClientSizeToLogical(rectangle.Size));
            }
            return rectangle;
        }

        protected Rectangle RectangleToScreen(Rectangle rectangle)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                return new Rectangle(this.PointToScreen(rectangle.Location), parentView.LogicalSizeToClient(rectangle.Size));
            }
            return rectangle;
        }

        protected internal virtual void RefreshDesignerActions()
        {
            this.designerActions = null;
        }

        protected void RefreshDesignerVerbs()
        {
            if ((this.Activity != null) && (this.Activity.Site != null))
            {
                DesignerVerbCollection verbs = ((IDesigner) this).Verbs;
                if (verbs != null)
                {
                    foreach (DesignerVerb verb in verbs)
                    {
                        int oleStatus = verb.OleStatus;
                    }
                }
            }
        }

        protected virtual void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
        }

        protected void ShowInfoTip(string infoTip)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.ShowInfoTip(infoTip);
            }
        }

        protected void ShowInfoTip(string title, string infoTip)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.ShowInfoTip(title, infoTip);
            }
        }

        protected void ShowInPlaceTip(string infoTip, Rectangle rectangle)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.ShowInPlaceToolTip(infoTip, parentView.LogicalRectangleToClient(rectangle));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesigner.DoDefaultAction()
        {
            this.DoDefaultAction();
        }

        void IDesigner.Initialize(IComponent component)
        {
            this.activity = component as System.Workflow.ComponentModel.Activity;
            if (this.activity == null)
            {
                throw new ArgumentException(DR.GetString("Error_InvalidActivity", new object[0]), "component");
            }
            this.Initialize(this.activity);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesignerFilter.PostFilterAttributes(IDictionary attributes)
        {
            this.PostFilterAttributes(attributes);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesignerFilter.PostFilterEvents(IDictionary events)
        {
            this.PostFilterEvents(events);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesignerFilter.PostFilterProperties(IDictionary properties)
        {
            this.PostFilterProperties(properties);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesignerFilter.PreFilterAttributes(IDictionary attributes)
        {
            this.PreFilterAttributes(attributes);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesignerFilter.PreFilterEvents(IDictionary events)
        {
            this.PreFilterEvents(events);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDesignerFilter.PreFilterProperties(IDictionary properties)
        {
            this.PreFilterProperties(properties);
        }

        object IRootDesigner.GetView(ViewTechnology technology)
        {
            DesignSurface service = this.GetService(typeof(DesignSurface)) as DesignSurface;
            IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (((this.workflowView == null) && (service != null)) && ((host != null) && (host.RootComponent == this.Activity)))
            {
                this.workflowView = this.CreateView(technology);
            }
            return this.workflowView;
        }

        bool IToolboxUser.GetToolSupported(ToolboxItem toolboxItem)
        {
            bool flag = true;
            IExtendedUIService2 service = this.GetService(typeof(IExtendedUIService2)) as IExtendedUIService2;
            if (service != null)
            {
                long targetFrameworkVersion = service.GetTargetFrameworkVersion();
                if (targetFrameworkVersion != 0L)
                {
                    if (targetFrameworkVersion < 0x30000L)
                    {
                        return false;
                    }
                    if ((targetFrameworkVersion < 0x30005L) && (string.Equals(toolboxItem.TypeName, "System.Workflow.Activities.ReceiveActivity") || string.Equals(toolboxItem.TypeName, "System.Workflow.Activities.SendActivity")))
                    {
                        return false;
                    }
                }
            }
            ITypeProvider provider = this.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (provider != null)
            {
                System.Type activityType = null;
                if (provider.LocalAssembly != null)
                {
                    activityType = provider.LocalAssembly.GetType(toolboxItem.TypeName, false);
                }
                if (activityType == null)
                {
                    try
                    {
                        activityType = System.Type.GetType(toolboxItem.TypeName + ", " + toolboxItem.AssemblyName);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    catch (FileLoadException)
                    {
                    }
                }
                if (activityType == null)
                {
                    return flag;
                }
                IWorkflowRootDesigner safeRootDesigner = GetSafeRootDesigner(this.Activity.Site);
                if (safeRootDesigner != null)
                {
                    if (!safeRootDesigner.IsSupportedActivityType(activityType))
                    {
                        return false;
                    }
                    if ((safeRootDesigner.InvokingDesigner != null) && (safeRootDesigner.InvokingDesigner.Activity != null))
                    {
                        safeRootDesigner = GetSafeRootDesigner(safeRootDesigner.InvokingDesigner.Activity.Site);
                        if ((safeRootDesigner != null) && !safeRootDesigner.IsSupportedActivityType(activityType))
                        {
                            return false;
                        }
                    }
                }
                if (toolboxItem is ActivityToolboxItem)
                {
                    return flag;
                }
                object[] customAttributes = activityType.GetCustomAttributes(typeof(ToolboxItemAttribute), false);
                if (customAttributes.Length <= 0)
                {
                    return flag;
                }
                flag = false;
                foreach (Attribute attribute in customAttributes)
                {
                    ToolboxItemAttribute attribute2 = attribute as ToolboxItemAttribute;
                    if ((attribute2 != null) && typeof(ActivityToolboxItem).IsAssignableFrom(attribute2.ToolboxItemType))
                    {
                        return true;
                    }
                }
            }
            return flag;
        }

        void IToolboxUser.ToolPicked(ToolboxItem toolboxItem)
        {
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            this.GetService(typeof(IToolboxService));
            if ((toolboxItem != null) && (service != null))
            {
                object primarySelection = service.PrimarySelection;
                if ((primarySelection is System.Workflow.ComponentModel.Design.HitTestInfo) || (primarySelection is CompositeActivity))
                {
                    System.Workflow.ComponentModel.Design.HitTestInfo insertLocation = null;
                    CompositeActivity activity = null;
                    if (primarySelection is System.Workflow.ComponentModel.Design.HitTestInfo)
                    {
                        insertLocation = (System.Workflow.ComponentModel.Design.HitTestInfo) primarySelection;
                        activity = insertLocation.AssociatedDesigner.Activity as CompositeActivity;
                    }
                    else if (primarySelection is CompositeActivity)
                    {
                        activity = (CompositeActivity) primarySelection;
                        insertLocation = new System.Workflow.ComponentModel.Design.HitTestInfo(GetDesigner(activity), HitTestLocations.Designer);
                    }
                    CompositeActivityDesigner compositeActivityDesigner = GetDesigner(activity) as CompositeActivityDesigner;
                    if (compositeActivityDesigner != null)
                    {
                        System.Workflow.ComponentModel.Activity[] collection = CompositeActivityDesigner.DeserializeActivitiesFromToolboxItem(this.Activity.Site, toolboxItem, false);
                        if ((collection.Length != 0) && compositeActivityDesigner.CanInsertActivities(insertLocation, new List<System.Workflow.ComponentModel.Activity>(collection).AsReadOnly()))
                        {
                            try
                            {
                                collection = CompositeActivityDesigner.DeserializeActivitiesFromToolboxItem(this.Activity.Site, toolboxItem, true);
                                if (collection.Length > 0)
                                {
                                    CompositeActivityDesigner.InsertActivities(compositeActivityDesigner, insertLocation, new List<System.Workflow.ComponentModel.Activity>(collection).AsReadOnly(), SR.GetString("PastingActivities"));
                                    service.SetSelectedComponents(collection, SelectionTypes.Replace);
                                    this.ParentView.EnsureVisible(collection[0]);
                                }
                            }
                            catch (CheckoutException exception)
                            {
                                if (exception != CheckoutException.Canceled)
                                {
                                    throw new Exception(DR.GetString("ActivityInsertError", new object[0]) + "\n" + exception.Message, exception);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        bool IConnectableDesigner.CanConnect(ConnectionPoint source, ConnectionPoint target)
        {
            return this.CanConnect(source, target);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IConnectableDesigner.OnConnected(ConnectionPoint source, ConnectionPoint target)
        {
            this.OnConnected(source, target);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IPersistUIState.LoadViewState(BinaryReader reader)
        {
            this.LoadViewState(reader);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IPersistUIState.SaveViewState(BinaryWriter writer)
        {
            this.SaveViewState(writer);
        }

        void IWorkflowDesignerMessageSink.OnBeginResizing(DesignerEdges sizingEdge)
        {
            try
            {
                this.OnBeginResizing(new ActivityDesignerResizeEventArgs(sizingEdge, this.Bounds));
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.OnDragDrop(DragEventArgs e)
        {
            try
            {
                this.OnDragDrop(e as ActivityDragEventArgs);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragEnter(DragEventArgs e)
        {
            try
            {
                this.OnDragEnter(e as ActivityDragEventArgs);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragLeave()
        {
            try
            {
                this.OnDragLeave();
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragOver(DragEventArgs e)
        {
            try
            {
                this.OnDragOver(e as ActivityDragEventArgs);
            }
            catch
            {
            }
            return true;
        }

        void IWorkflowDesignerMessageSink.OnEndResizing()
        {
            try
            {
                this.OnEndResizing();
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            try
            {
                this.OnGiveFeedback(e);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnKeyDown(KeyEventArgs e)
        {
            try
            {
                this.OnKeyDown(e);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnKeyUp(KeyEventArgs e)
        {
            try
            {
                this.OnKeyUp(e);
            }
            catch
            {
            }
            return true;
        }

        void IWorkflowDesignerMessageSink.OnLayout(LayoutEventArgs layoutEventArgs)
        {
        }

        void IWorkflowDesignerMessageSink.OnLayoutPosition(Graphics graphics)
        {
            try
            {
                this.OnLayoutPosition(new ActivityDesignerLayoutEventArgs(graphics, this.DesignerTheme));
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnLayoutSize(Graphics graphics)
        {
            try
            {
                this.Size = this.OnLayoutSize(new ActivityDesignerLayoutEventArgs(graphics, this.DesignerTheme));
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.OnMouseCaptureChanged()
        {
            try
            {
                this.OnMouseCaptureChanged();
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDoubleClick(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseDoubleClick(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDown(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragBegin(Point initialPoint, MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseDragBegin(initialPoint, new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragEnd()
        {
            try
            {
                this.OnMouseDragEnd();
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragMove(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseDragMove(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseEnter(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseEnter(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseHover(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseHover(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseLeave()
        {
            try
            {
                this.OnMouseLeave();
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseMove(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseUp(MouseEventArgs e)
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                try
                {
                    Point point = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    this.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseWheel(MouseEventArgs e)
        {
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnPaint(PaintEventArgs e, Rectangle viewPort)
        {
            try
            {
                Rectangle bounds = this.Bounds;
                if (this.IsVisible && viewPort.IntersectsWith(bounds))
                {
                    GlyphManager service = this.GetService(typeof(IDesignerGlyphProviderService)) as GlyphManager;
                    bounds.Width++;
                    bounds.Height++;
                    using (GraphicsPath path = ActivityDesignerPaint.GetDesignerPath(this, Point.Empty, new System.Drawing.Size(this.DesignerTheme.BorderWidth, this.DesignerTheme.BorderWidth), DesignerEdges.All, false))
                    {
                        using (Region region = new Region(path))
                        {
                            Region clip = e.Graphics.Clip;
                            region.Intersect(clip);
                            region.Intersect(viewPort);
                            bool flag = false;
                            try
                            {
                                ActivityDesignerPaintEventArgs args = new ActivityDesignerPaintEventArgs(e.Graphics, bounds, viewPort, this.DesignerTheme);
                                e.Graphics.Clip = region;
                                this.OnPaint(args);
                                e.Graphics.Clip = clip;
                                flag = true;
                                if (service != null)
                                {
                                    service.DrawDesignerGlyphs(args, this);
                                }
                                this.DrawingState &= ~DrawingStates.InvalidDraw;
                            }
                            catch
                            {
                                this.DrawingState |= DrawingStates.InvalidDraw;
                            }
                            finally
                            {
                                if (!flag)
                                {
                                    e.Graphics.Clip = clip;
                                }
                                if (this.DrawingState != DrawingStates.Valid)
                                {
                                    ActivityDesignerPaint.DrawInvalidDesignerIndicator(e.Graphics, this);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort)
        {
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            try
            {
                this.OnQueryContinueDrag(e);
            }
            catch
            {
            }
            return true;
        }

        void IWorkflowDesignerMessageSink.OnResizing(DesignerEdges sizingEdge, Rectangle bounds)
        {
            try
            {
                this.OnResizing(new ActivityDesignerResizeEventArgs(sizingEdge, bounds));
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.OnScroll(ScrollBar sender, int value)
        {
            try
            {
                this.OnScroll(sender, value);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnShowContextMenu(Point screenMenuPoint)
        {
            return true;
        }

        void IWorkflowDesignerMessageSink.OnThemeChange()
        {
            try
            {
                this.OnThemeChange(this.DesignerTheme);
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.ProcessMessage(Message message)
        {
            try
            {
                this.OnProcessMessage(message);
            }
            catch
            {
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        bool IWorkflowRootDesigner.IsSupportedActivityType(System.Type activityType)
        {
            return this.IsSupportedActivityType(activityType);
        }

        public virtual AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new ActivityDesignerAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activity;
            }
        }

        private PropertyDescriptor[] BindableProperties
        {
            get
            {
                List<PropertyDescriptor> list = new List<PropertyDescriptor>();
                if (!Helpers.IsActivityLocked(this.Activity))
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.Activity, new Attribute[] { new BrowsableAttribute(true) });
                    if (properties != null)
                    {
                        foreach (PropertyDescriptor descriptor in properties)
                        {
                            if (descriptor.Converter is ActivityBindTypeConverter)
                            {
                                list.Add(descriptor);
                            }
                        }
                    }
                }
                return list.ToArray();
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(this.Location, this.Size);
            }
        }

        protected internal virtual ReadOnlyCollection<DesignerAction> DesignerActions
        {
            get
            {
                if (this.designerActions == null)
                {
                    this.designerActions = new List<DesignerAction>();
                    System.Workflow.ComponentModel.Activity activity = this.Activity;
                    if (activity != null)
                    {
                        bool flag = IsCommentedActivity(activity);
                        WorkflowDesignerLoader service = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                        bool flag2 = (service != null) && service.InDebugMode;
                        if (((activity.Enabled && !flag) && (!this.IsLocked && (activity.Site != null))) && !flag2)
                        {
                            ValidationErrorCollection errors = new ValidationErrorCollection();
                            try
                            {
                                ValidationManager serviceProvider = new ValidationManager(this.Activity.Site, false);
                                using (WorkflowCompilationContext.CreateScope(serviceProvider))
                                {
                                    Helpers.GetRootActivity(this.Activity);
                                    foreach (Validator validator in serviceProvider.GetValidators(activity.GetType()))
                                    {
                                        errors.AddRange(validator.Validate(serviceProvider, activity));
                                    }
                                }
                            }
                            catch
                            {
                            }
                            if (errors.Count > 0)
                            {
                                for (int i = 0; i < errors.Count; i++)
                                {
                                    ValidationError error = errors[i];
                                    if ((error != null) && !error.IsWarning)
                                    {
                                        DesignerAction item = new DesignerAction(this, i, error.ErrorText, AmbientTheme.ConfigErrorImage) {
                                            PropertyName = error.PropertyName
                                        };
                                        foreach (DictionaryEntry entry in error.UserData)
                                        {
                                            item.UserData[entry.Key] = entry.Value;
                                        }
                                        this.designerActions.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
                return this.designerActions.AsReadOnly();
            }
        }

        internal SmartTag DesignerSmartTag
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.smartTag;
            }
        }

        public ActivityDesignerTheme DesignerTheme
        {
            get
            {
                return WorkflowTheme.CurrentTheme.GetDesignerTheme(this);
            }
        }

        internal DrawingStates DrawingState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.drawingState;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.drawingState = value;
            }
        }

        protected internal virtual bool EnableVisualResizing
        {
            get
            {
                return false;
            }
        }

        private PropertyDescriptor[] EventHandlerProperties
        {
            get
            {
                List<PropertyDescriptor> list = new List<PropertyDescriptor>();
                if (this.Activity.Site != null)
                {
                    foreach (PropertyDescriptor descriptor in PropertyDescriptorFilter.GetPropertiesForEvents(this.Activity.Site, this.Activity))
                    {
                        list.Add(descriptor);
                    }
                }
                return list.ToArray();
            }
        }

        protected internal virtual ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                if (this.IsSelected)
                {
                    if (this.IsPrimarySelection)
                    {
                        glyphs.Add(PrimarySelectionGlyph.Default);
                    }
                    else
                    {
                        glyphs.Add(NonPrimarySelectionGlyph.Default);
                    }
                }
                bool flag = IsCommentedActivity(this.Activity);
                WorkflowDesignerLoader service = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                bool flag2 = (service != null) && service.InDebugMode;
                if (((WorkflowTheme.CurrentTheme.AmbientTheme.ShowConfigErrors && this.Activity.Enabled) && (!flag && !flag2)) && (this.DesignerActions.Count > 0))
                {
                    glyphs.Add(ConfigErrorGlyph.Default);
                }
                if (!this.Activity.Enabled && !flag)
                {
                    glyphs.Add(CommentGlyph.Default);
                }
                if (Helpers.IsActivityLocked(this.Activity))
                {
                    glyphs.Add(LockedActivityGlyph.Default);
                }
                if (this.SmartTagVisible && this.ShowSmartTag)
                {
                    glyphs.Add(this.smartTag);
                }
                return glyphs;
            }
        }

        public virtual System.Drawing.Image Image
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.image;
            }
            protected set
            {
                this.image = value;
                this.PerformLayout();
            }
        }

        protected virtual Rectangle ImageRectangle
        {
            get
            {
                if (this.Image == null)
                {
                    return Rectangle.Empty;
                }
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                ActivityDesignerTheme designerTheme = this.DesignerTheme;
                Rectangle bounds = this.Bounds;
                Rectangle empty = Rectangle.Empty;
                empty.X = bounds.Left + WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Width;
                empty.Y = bounds.Top + ((bounds.Height - this.DesignerTheme.ImageSize.Height) / 2);
                empty.Size = designerTheme.ImageSize;
                return empty;
            }
        }

        private string InfoTipText
        {
            get
            {
                string str = !string.IsNullOrEmpty(this.Activity.Description) ? this.Activity.Description : GetActivityDescription(this.Activity.GetType());
                str = (str.Length > 80) ? (str.Substring(0, 80) + "...") : str;
                if (this.RulesText.Length > 0)
                {
                    str = str + "\n\n" + this.RulesText;
                }
                return str;
            }
        }

        private string InfoTipTitle
        {
            get
            {
                if (this.Activity.Parent == null)
                {
                    return this.Activity.GetType().Name;
                }
                string str2 = (this.Activity.Name.Length > 0x19) ? (this.Activity.Name.Substring(0, 0x19) + "...") : this.Activity.Name;
                return DR.GetString("InfoTipTitle", new object[] { this.Activity.GetType().Name, str2 });
            }
        }

        protected virtual CompositeActivityDesigner InvokingDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.invokingDesigner;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.invokingDesigner = value;
            }
        }

        public bool IsLocked
        {
            get
            {
                if (Helpers.IsActivityLocked(this.Activity))
                {
                    return true;
                }
                if (this.DrawingState != DrawingStates.Valid)
                {
                    return true;
                }
                WorkflowDesignerLoader service = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if ((service != null) && service.InDebugMode)
                {
                    return true;
                }
                IWorkflowRootDesigner safeRootDesigner = GetSafeRootDesigner(this.Activity.Site);
                return ((safeRootDesigner != null) && (safeRootDesigner.InvokingDesigner != null));
            }
        }

        public bool IsPrimarySelection
        {
            get
            {
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                return ((service != null) && (service.PrimarySelection == this.Activity));
            }
        }

        public bool IsRootDesigner
        {
            get
            {
                bool flag = false;
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    flag = service.RootComponent == this.Activity;
                }
                return flag;
            }
        }

        public bool IsSelected
        {
            get
            {
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                return ((service != null) && service.GetComponentSelected(this.Activity));
            }
        }

        public virtual bool IsVisible
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isVisible;
            }
        }

        public virtual Point Location
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.location;
            }
            set
            {
                if (this.ParentDesigner is FreeformActivityDesigner)
                {
                    value = DesignerHelpers.SnapToGrid(value);
                }
                if (this.location != value)
                {
                    this.location = value;
                }
            }
        }

        protected virtual ReadOnlyCollection<WorkflowDesignerMessageFilter> MessageFilters
        {
            get
            {
                return new List<WorkflowDesignerMessageFilter> { new ConnectionManager(), new ResizingMessageFilter(), new DynamicActionMessageFilter(), new AutoScrollingMessageFilter(), new AutoExpandingMessageFilter(), new DragSelectionMessageFilter(), new FreeFormDragDropManager() }.AsReadOnly();
            }
        }

        public virtual System.Drawing.Size MinimumSize
        {
            get
            {
                return this.DesignerTheme.Size;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        internal string Name
        {
            get
            {
                if (this.Activity == null)
                {
                    return null;
                }
                return this.Activity.Name;
            }
            set
            {
            }
        }

        public CompositeActivityDesigner ParentDesigner
        {
            get
            {
                CompositeActivityDesigner designer = null;
                IWorkflowRootDesigner designer2 = this;
                if ((designer2 != null) && this.IsRootDesigner)
                {
                    return designer2.InvokingDesigner;
                }
                if ((this.Activity != null) && (this.Activity.Parent != null))
                {
                    designer = GetDesigner(this.Activity.Parent) as CompositeActivityDesigner;
                }
                return designer;
            }
        }

        protected internal WorkflowView ParentView
        {
            get
            {
                return (this.GetService(typeof(WorkflowView)) as WorkflowView);
            }
        }

        private string RulesText
        {
            get
            {
                if (this.rulesText == null)
                {
                    this.rulesText = string.Empty;
                    IDictionary<string, string> declarativeRules = DesignerHelpers.GetDeclarativeRules(this.Activity);
                    if (declarativeRules.Count > 0)
                    {
                        this.rulesText = DR.GetString("Rules", new object[0]);
                        int num = 0x13b;
                        foreach (KeyValuePair<string, string> pair in declarativeRules)
                        {
                            this.rulesText = this.rulesText + "\n";
                            string key = pair.Key;
                            key = (key.Length > 0x19) ? (key.Substring(0, 0x19) + "...") : key;
                            string str2 = pair.Value;
                            str2 = (str2.Length > 80) ? (str2.Substring(0, 80) + "...") : str2;
                            if (str2.Length == 0)
                            {
                                str2 = DR.GetString("Empty", new object[0]);
                            }
                            this.rulesText = this.rulesText + string.Format(CultureInfo.CurrentCulture, "{0}: {1}", new object[] { key, str2 });
                            if (this.rulesText.Length > num)
                            {
                                break;
                            }
                        }
                        if (this.rulesText.Length > num)
                        {
                            this.rulesText = this.rulesText + "\n\n" + DR.GetString("More", new object[0]);
                        }
                    }
                }
                return this.rulesText;
            }
        }

        protected virtual bool ShowSmartTag
        {
            get
            {
                return false;
            }
        }

        public virtual System.Drawing.Size Size
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.size;
            }
            set
            {
                value.Width = Math.Max(value.Width, this.MinimumSize.Width);
                value.Height = Math.Max(value.Height, this.MinimumSize.Height);
                if (this.size != value)
                {
                    this.size = value;
                }
            }
        }

        protected virtual Rectangle SmartTagRectangle
        {
            get
            {
                Rectangle empty = Rectangle.Empty;
                Rectangle imageRectangle = this.ImageRectangle;
                if (!imageRectangle.Size.IsEmpty)
                {
                    empty = imageRectangle;
                }
                return empty;
            }
        }

        protected virtual ReadOnlyCollection<ActivityDesignerVerb> SmartTagVerbs
        {
            get
            {
                return new List<ActivityDesignerVerb>().AsReadOnly();
            }
        }

        internal virtual bool SmartTagVisible
        {
            get
            {
                return ((this.ShowSmartTag && (this.smartTag.ActiveDesigner != null)) || this.smartTagVisible);
            }
            set
            {
                if (this.smartTagVisible != value)
                {
                    this.smartTagVisible = value;
                    this.OnSmartTagVisibilityChanged(this.smartTagVisible);
                }
            }
        }

        internal System.Drawing.Image StockImage
        {
            get
            {
                if (this.Activity == null)
                {
                    return null;
                }
                System.Drawing.Image designerImage = this.DesignerTheme.DesignerImage;
                if (designerImage == null)
                {
                    designerImage = ActivityToolboxItem.GetToolboxImage(this.Activity.GetType());
                }
                return designerImage;
            }
        }

        internal virtual WorkflowLayout SupportedLayout
        {
            get
            {
                return new ActivityRootLayout(this.Activity.Site);
            }
        }

        internal virtual bool SupportsLayoutPersistence
        {
            get
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    foreach (IComponent component in service.Container.Components)
                    {
                        System.Workflow.ComponentModel.Activity activity = component as System.Workflow.ComponentModel.Activity;
                        if ((activity != null) && (GetDesigner(activity) is FreeformActivityDesigner))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        IComponent IDesigner.Component
        {
            get
            {
                return this.activity;
            }
        }

        DesignerVerbCollection IDesigner.Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(this.Verbs);
                IDesignerVerbProviderService service = this.GetService(typeof(IDesignerVerbProviderService)) as IDesignerVerbProviderService;
                if (service != null)
                {
                    foreach (IDesignerVerbProvider provider in service.VerbProviders)
                    {
                        verbs.AddRange(provider.GetVerbs(this));
                    }
                }
                return verbs.SafeCollection;
            }
        }

        ViewTechnology[] IRootDesigner.SupportedTechnologies
        {
            get
            {
                return new ViewTechnology[] { ViewTechnology.Default };
            }
        }

        CompositeActivityDesigner IWorkflowRootDesigner.InvokingDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.InvokingDesigner;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.InvokingDesigner = value;
            }
        }

        ReadOnlyCollection<WorkflowDesignerMessageFilter> IWorkflowRootDesigner.MessageFilters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.MessageFilters;
            }
        }

        bool IWorkflowRootDesigner.SupportsLayoutPersistence
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.SupportsLayoutPersistence;
            }
        }

        public virtual string Text
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.text;
            }
            protected set
            {
                if (((value != null) && (value.Length != 0)) && (this.text != value))
                {
                    this.text = value;
                    this.PerformLayout();
                }
            }
        }

        protected virtual Rectangle TextRectangle
        {
            get
            {
                if (string.IsNullOrEmpty(this.Text))
                {
                    return Rectangle.Empty;
                }
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                ActivityDesignerTheme designerTheme = this.DesignerTheme;
                Rectangle bounds = this.Bounds;
                Rectangle empty = Rectangle.Empty;
                empty.X = bounds.Left + ambientTheme.Margin.Width;
                empty.X += (this.Image != null) ? (designerTheme.ImageSize.Width + ambientTheme.Margin.Width) : 0;
                empty.Y = bounds.Top + ((bounds.Height - this.textSize.Height) / 2);
                empty.Size = this.textSize;
                return empty;
            }
        }

        protected virtual ActivityDesignerVerbCollection Verbs
        {
            get
            {
                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new ActivityDesignerVerbCollection();
                    if (!this.IsLocked)
                    {
                        this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString("GenerateEventHandlers", new object[0]), new EventHandler(this.OnGenerateEventHandler), new EventHandler(this.OnGenerateEventHandlerStatusUpdate)));
                    }
                    this.GetService(typeof(WorkflowDesignerLoader));
                    if (this.Activity.Parent != null)
                    {
                        this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString("PromoteBindings", new object[0]), new EventHandler(this.OnPromoteBindings), new EventHandler(this.OnPromoteBindingsStatusUpdate)));
                    }
                    this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString("BindSelectedProperty", new object[0]), new EventHandler(this.OnBindProperty), new EventHandler(this.OnBindPropertyStatusUpdate)));
                    ActivityDesignerVerb verb = new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString("MoveLeftDesc", new object[0]), new EventHandler(this.OnMoveBranch), new EventHandler(this.OnStatusMoveBranch));
                    verb.Properties[DesignerUserDataKeys.MoveBranchKey] = true;
                    this.designerVerbs.Add(verb);
                    verb = new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString("MoveRightDesc", new object[0]), new EventHandler(this.OnMoveBranch), new EventHandler(this.OnStatusMoveBranch));
                    verb.Properties[DesignerUserDataKeys.MoveBranchKey] = false;
                    this.designerVerbs.Add(verb);
                    foreach (ActivityDesignerVerb verb2 in this.SmartTagVerbs)
                    {
                        this.designerVerbs.Add(verb2);
                    }
                }
                return this.designerVerbs;
            }
        }

        [Flags]
        internal enum DrawingStates
        {
            InvalidDraw = 4,
            InvalidPosition = 1,
            InvalidSize = 2,
            Valid = 0
        }

        internal sealed class SmartTag : DesignerGlyph
        {
            private ActivityDesigner activeDesigner;
            internal const int DefaultHeight = 2;
            private static Image defaultImage = DR.GetImage("SmartTag");

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle empty = Rectangle.Empty;
                Rectangle smartTagRectangle = designer.SmartTagRectangle;
                if (!smartTagRectangle.IsEmpty)
                {
                    Size glyphSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
                    Size size = smartTagRectangle.Size;
                    Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                    empty.X = smartTagRectangle.Left - (margin.Width / 2);
                    empty.Y = smartTagRectangle.Top - (margin.Height / 2);
                    empty.Width = (size.Width + (glyphSize.Width / 2)) + (3 * margin.Width);
                    empty.Height = size.Height + margin.Height;
                }
                return empty;
            }

            protected override void OnActivate(ActivityDesigner designer)
            {
                if (designer.SmartTagVerbs.Count > 0)
                {
                    this.activeDesigner = designer;
                    Rectangle bounds = this.GetBounds(designer, true);
                    this.activeDesigner.OnShowSmartTagVerbs(new Point(bounds.Left, bounds.Bottom + 1));
                    this.activeDesigner = null;
                }
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                Rectangle bounds = this.GetBounds(designer, true);
                bool flag = false;
                if ((Form.ActiveForm != null) && Form.ActiveForm.GetType().FullName.Equals(typeof(ItemPalette).FullName + "+Palette", StringComparison.Ordinal))
                {
                    flag = Form.ActiveForm.Location == designer.PointToScreen(new Point(bounds.Left, bounds.Bottom));
                }
                if (!activated)
                {
                    if (this.activeDesigner != null)
                    {
                        activated = true;
                    }
                    else if ((Form.ActiveForm != null) && Form.ActiveForm.GetType().FullName.Equals(typeof(ItemPalette).FullName + "+Palette", StringComparison.Ordinal))
                    {
                        activated = flag;
                    }
                }
                graphics.FillRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.BackgroundBrush, bounds);
                using (Brush brush = new SolidBrush(Color.FromArgb(50, WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor)))
                {
                    graphics.FillRectangle(brush, bounds);
                }
                graphics.DrawRectangle(SystemPens.ControlDarkDark, bounds);
                Image image = designer.Image;
                image = (designer.Image == null) ? defaultImage : image;
                Size glyphSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
                Size size = designer.SmartTagRectangle.Size;
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle destination = bounds;
                destination.X += margin.Width / 2;
                destination.Y += margin.Height / 2;
                destination.Size = size;
                ActivityDesignerPaint.DrawImage(graphics, image, destination, DesignerContentAlignment.Center);
                Rectangle rectangle3 = bounds;
                rectangle3.X += size.Width + ((3 * margin.Width) / 2);
                rectangle3.Y += margin.Height / 2;
                rectangle3.Width = glyphSize.Width / 2;
                rectangle3.Height -= glyphSize.Height / 4;
                using (GraphicsPath path = ActivityDesignerPaint.GetScrollIndicatorPath(rectangle3, ScrollButton.Down))
                {
                    graphics.FillPath(Brushes.Black, path);
                    graphics.DrawPath(Pens.Black, path);
                }
            }

            internal ActivityDesigner ActiveDesigner
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.activeDesigner;
                }
            }

            public override bool CanBeActivated
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

