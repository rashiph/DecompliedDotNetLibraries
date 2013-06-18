namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;

    public class WorkflowOutline : UserControl
    {
        private Hashtable activityToNodeMapping = new Hashtable();
        private bool isDirty;
        private bool needsExpandAll = true;
        private IServiceProvider serviceProvider;
        private System.Windows.Forms.TreeView treeView;

        protected internal event TreeViewCancelEventHandler Expanding
        {
            add
            {
                this.treeView.BeforeExpand += value;
            }
            remove
            {
                this.treeView.BeforeExpand -= value;
            }
        }

        public WorkflowOutline(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (!(this.GetService(typeof(IDesignerHost)) is IDesignerHost))
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            DesignSurface surface = this.GetService(typeof(DesignSurface)) as DesignSurface;
            if (surface == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(DesignSurface).FullName }));
            }
            surface.Loaded += new LoadedEventHandler(this.OnSurfaceLoaded);
            IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
            }
            WorkflowTheme.ThemeChanged += new EventHandler(this.OnThemeChanged);
            ISelectionService service2 = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service2 != null)
            {
                service2.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            IUIService service3 = this.GetService(typeof(IUIService)) as IUIService;
            if (service3 != null)
            {
                this.Font = (Font) service3.Styles["DialogFont"];
            }
            this.treeView = new System.Windows.Forms.TreeView();
            this.treeView.Dock = DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.AfterSelect += new TreeViewEventHandler(this.OnTreeViewAfterSelect);
            this.treeView.MouseDown += new MouseEventHandler(this.OnTreeViewMouseDown);
            this.treeView.Font = this.Font;
            this.treeView.ItemHeight = Math.Max(this.treeView.ItemHeight, 0x12);
            base.Controls.Add(this.treeView);
        }

        protected virtual WorkflowOutlineNode CreateNewNode(Activity activity)
        {
            return new WorkflowOutlineNode(activity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.IsDirty = false;
                WorkflowTheme.ThemeChanged -= new EventHandler(this.OnThemeChanged);
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                }
                ISelectionService service2 = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service2 != null)
                {
                    service2.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                DesignSurface surface = this.GetService(typeof(DesignSurface)) as DesignSurface;
                if (surface != null)
                {
                    surface.Loaded -= new LoadedEventHandler(this.OnSurfaceLoaded);
                }
                this.serviceProvider = null;
            }
            base.Dispose(disposing);
        }

        protected WorkflowOutlineNode GetNode(Activity activity)
        {
            return (this.activityToNodeMapping[activity] as WorkflowOutlineNode);
        }

        protected override object GetService(System.Type serviceType)
        {
            if (this.serviceProvider != null)
            {
                return this.serviceProvider.GetService(serviceType);
            }
            return base.GetService(serviceType);
        }

        private void InsertDocOutlineNode(WorkflowOutlineNode parentNode, Activity activity, int childIndex, bool addNestedActivities)
        {
            if (!this.activityToNodeMapping.Contains(activity))
            {
                WorkflowOutlineNode nodeToUpdate = this.CreateNewNode(activity);
                if (nodeToUpdate != null)
                {
                    this.RefreshNode(nodeToUpdate, false);
                    this.activityToNodeMapping.Add(activity, nodeToUpdate);
                    if (addNestedActivities && (activity is CompositeActivity))
                    {
                        foreach (Activity activity2 in ((CompositeActivity) activity).Activities)
                        {
                            this.InsertDocOutlineNode(nodeToUpdate, activity2, nodeToUpdate.Nodes.Count, addNestedActivities);
                        }
                    }
                    if (parentNode != null)
                    {
                        parentNode.Nodes.Insert(childIndex, nodeToUpdate);
                    }
                    else
                    {
                        this.treeView.Nodes.Add(nodeToUpdate);
                    }
                    this.OnNodeAdded(nodeToUpdate);
                }
            }
        }

        protected virtual void OnBeginUpdate()
        {
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.OldValue is ActivityCollectionChangeEventArgs)
            {
                this.IsDirty = true;
            }
            else if ((e.Member != null) && (e.Component is Activity))
            {
                WorkflowOutlineNode nodeToUpdate = this.activityToNodeMapping[e.Component] as WorkflowOutlineNode;
                if ((nodeToUpdate != null) && string.Equals(e.Member.Name, "Enabled", StringComparison.Ordinal))
                {
                    this.RefreshNode(nodeToUpdate, true);
                }
            }
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if (e.Component is Activity)
            {
                WorkflowOutlineNode node = this.activityToNodeMapping[e.Component] as WorkflowOutlineNode;
                if (node != null)
                {
                    node.OnActivityRename(e.NewName);
                }
            }
        }

        protected virtual void OnEndUpdate()
        {
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (this.IsDirty && this.treeView.Visible)
            {
                this.ReloadWorkflowOutline();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ReloadWorkflowOutline();
        }

        protected virtual void OnNodeAdded(WorkflowOutlineNode node)
        {
        }

        protected virtual void OnNodeSelected(WorkflowOutlineNode node)
        {
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if (((service != null) && (node != null)) && (service.PrimarySelection != node.Activity))
            {
                WorkflowView view = this.GetService(typeof(WorkflowView)) as WorkflowView;
                if (view != null)
                {
                    view.EnsureVisible(node.Activity);
                }
                service.SetSelectedComponents(new object[] { node.Activity }, SelectionTypes.Replace);
            }
        }

        protected virtual void OnRefreshNode(WorkflowOutlineNode node)
        {
            if (node != null)
            {
                Activity activity = node.Activity;
                if (activity != null)
                {
                    int num = (this.treeView.ImageList != null) ? this.treeView.ImageList.Images.IndexOfKey(activity.GetType().FullName) : -1;
                    if (num == -1)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                        if (designer != null)
                        {
                            Bitmap stockImage = designer.StockImage as Bitmap;
                            if (stockImage != null)
                            {
                                if (this.treeView.ImageList == null)
                                {
                                    this.treeView.ImageList = new ImageList();
                                    this.treeView.ImageList.ColorDepth = ColorDepth.Depth32Bit;
                                }
                                this.treeView.ImageList.Images.Add(activity.GetType().FullName, stockImage);
                                num = this.treeView.ImageList.Images.Count - 1;
                            }
                        }
                    }
                    node.ImageIndex = node.SelectedImageIndex = num;
                    node.RefreshNode();
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if ((service != null) && (service.PrimarySelection != null))
            {
                this.treeView.SelectedNode = this.activityToNodeMapping[service.PrimarySelection] as WorkflowOutlineNode;
                if (this.treeView.SelectedNode != null)
                {
                    this.treeView.SelectedNode.EnsureVisible();
                }
            }
        }

        private void OnSurfaceLoaded(object sender, LoadedEventArgs e)
        {
            this.ReloadWorkflowOutline();
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (this.treeView.Nodes.Count > 0)
            {
                this.treeView.ImageList.Images.Clear();
                this.RefreshWorkflowOutline();
            }
        }

        private void OnTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            WorkflowOutlineNode node = e.Node as WorkflowOutlineNode;
            this.OnNodeSelected(node);
        }

        private void OnTreeViewMouseDown(object sender, MouseEventArgs e)
        {
            if (this.treeView.GetNodeAt(e.Location) != null)
            {
                this.treeView.SelectedNode = this.treeView.GetNodeAt(e.Location);
            }
        }

        protected void RefreshNode(WorkflowOutlineNode nodeToUpdate, bool refreshChildNodes)
        {
            this.treeView.BeginUpdate();
            Queue<WorkflowOutlineNode> queue = new Queue<WorkflowOutlineNode>();
            queue.Enqueue(nodeToUpdate);
            while (queue.Count > 0)
            {
                WorkflowOutlineNode node = queue.Dequeue();
                this.OnRefreshNode(node);
                if (refreshChildNodes)
                {
                    foreach (TreeNode node2 in node.Nodes)
                    {
                        WorkflowOutlineNode item = node2 as WorkflowOutlineNode;
                        if ((item != null) && (item.Activity != null))
                        {
                            queue.Enqueue(item);
                        }
                    }
                }
                this.treeView.EndUpdate();
            }
        }

        public void RefreshWorkflowOutline()
        {
            if (this.treeView.Nodes.Count > 0)
            {
                this.RefreshNode(this.treeView.Nodes[0] as WorkflowOutlineNode, true);
            }
        }

        public void ReloadWorkflowOutline()
        {
            this.OnBeginUpdate();
            this.treeView.BeginUpdate();
            try
            {
                this.treeView.Nodes.Clear();
                this.activityToNodeMapping.Clear();
                IRootDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(this.serviceProvider);
                if (((safeRootDesigner != null) && (safeRootDesigner.Component != null)) && (safeRootDesigner.Component is Activity))
                {
                    this.InsertDocOutlineNode(null, safeRootDesigner.Component as Activity, 0, true);
                }
                if (this.NeedsExpandAll)
                {
                    this.treeView.ExpandAll();
                }
            }
            finally
            {
                this.treeView.EndUpdate();
            }
            this.IsDirty = false;
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if ((service != null) && (service.PrimarySelection != null))
            {
                this.treeView.SelectedNode = this.activityToNodeMapping[service.PrimarySelection] as WorkflowOutlineNode;
                if (this.treeView.SelectedNode != null)
                {
                    this.treeView.SelectedNode.EnsureVisible();
                }
            }
            this.OnEndUpdate();
        }

        private bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
            set
            {
                if (this.isDirty != value)
                {
                    this.isDirty = value;
                    if (value)
                    {
                        Application.Idle += new EventHandler(this.OnIdle);
                    }
                    else
                    {
                        Application.Idle -= new EventHandler(this.OnIdle);
                    }
                }
            }
        }

        protected internal bool NeedsExpandAll
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.needsExpandAll;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.needsExpandAll = value;
            }
        }

        protected internal TreeNode RootNode
        {
            get
            {
                if (this.treeView.Nodes.Count > 0)
                {
                    return this.treeView.Nodes[0];
                }
                return null;
            }
        }

        protected internal System.Windows.Forms.TreeView TreeView
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.treeView;
            }
        }
    }
}

