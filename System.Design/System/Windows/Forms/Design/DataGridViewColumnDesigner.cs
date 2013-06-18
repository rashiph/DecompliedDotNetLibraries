namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class DataGridViewColumnDesigner : ComponentDesigner
    {
        private FilterCutCopyPasteDeleteBehavior behavior;
        private bool behaviorPushed;
        private BehaviorService behaviorService;
        private const int DATAGRIDVIEWCOLUMN_defaultWidth = 100;
        private bool initializing;
        private DataGridView liveDataGridView;
        private ISelectionService selectionService;
        private bool userAddedColumn;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.PopBehavior();
                if (this.selectionService != null)
                {
                    this.selectionService.SelectionChanged -= new EventHandler(this.selectionService_SelectionChanged);
                }
                this.selectionService = null;
                this.behaviorService = null;
            }
        }

        public override void Initialize(IComponent component)
        {
            this.initializing = true;
            base.Initialize(component);
            if (component.Site != null)
            {
                this.selectionService = this.GetService(typeof(ISelectionService)) as ISelectionService;
                this.behaviorService = this.GetService(typeof(BehaviorService)) as BehaviorService;
                if ((this.behaviorService != null) && (this.selectionService != null))
                {
                    this.behavior = new FilterCutCopyPasteDeleteBehavior(true, this.behaviorService);
                    this.UpdateBehavior();
                    this.selectionService.SelectionChanged += new EventHandler(this.selectionService_SelectionChanged);
                }
            }
            this.initializing = false;
        }

        private void PopBehavior()
        {
            if (this.behaviorPushed)
            {
                try
                {
                    this.behaviorService.PopBehavior(this.behavior);
                }
                finally
                {
                    this.behaviorPushed = false;
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["Width"];
            if (oldPropertyDescriptor != null)
            {
                properties["Width"] = TypeDescriptor.CreateProperty(typeof(DataGridViewColumnDesigner), oldPropertyDescriptor, new Attribute[0]);
            }
            oldPropertyDescriptor = (PropertyDescriptor) properties["Name"];
            if (oldPropertyDescriptor != null)
            {
                if (base.Component.Site == null)
                {
                    properties["Name"] = TypeDescriptor.CreateProperty(typeof(DataGridViewColumnDesigner), oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.Yes, CategoryAttribute.Design, new DescriptionAttribute(System.Design.SR.GetString("DesignerPropName")), new ParenthesizePropertyNameAttribute(true) });
                }
                else
                {
                    properties["Name"] = TypeDescriptor.CreateProperty(typeof(DataGridViewColumnDesigner), oldPropertyDescriptor, new Attribute[] { new ParenthesizePropertyNameAttribute(true) });
                }
            }
            properties["UserAddedColumn"] = TypeDescriptor.CreateProperty(typeof(DataGridViewColumnDesigner), "UserAddedColumn", typeof(bool), new Attribute[] { new DefaultValueAttribute(false), BrowsableAttribute.No, DesignOnlyAttribute.Yes });
        }

        private void PushBehavior()
        {
            if (!this.behaviorPushed)
            {
                try
                {
                    this.behaviorService.PushBehavior(this.behavior);
                }
                finally
                {
                    this.behaviorPushed = true;
                }
            }
        }

        private void selectionService_SelectionChanged(object sender, EventArgs e)
        {
            this.UpdateBehavior();
        }

        private bool ShouldSerializeName()
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                return false;
            }
            if (!this.initializing)
            {
                return base.ShadowProperties.ShouldSerializeValue("Name", null);
            }
            return (base.Component != service.RootComponent);
        }

        private bool ShouldSerializeWidth()
        {
            DataGridViewColumn component = (DataGridViewColumn) base.Component;
            return ((component.InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.Fill) && (component.Width != 100));
        }

        private void UpdateBehavior()
        {
            if (this.selectionService != null)
            {
                if ((this.selectionService.PrimarySelection != null) && base.Component.Equals(this.selectionService.PrimarySelection))
                {
                    this.PushBehavior();
                }
                else
                {
                    this.PopBehavior();
                }
            }
        }

        public DataGridView LiveDataGridView
        {
            set
            {
                this.liveDataGridView = value;
            }
        }

        private string Name
        {
            get
            {
                DataGridViewColumn component = (DataGridViewColumn) base.Component;
                if (component.Site != null)
                {
                    return component.Site.Name;
                }
                return component.Name;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                DataGridViewColumn component = (DataGridViewColumn) base.Component;
                if ((component != null) && (string.Compare(value, component.Name, false, CultureInfo.InvariantCulture) != 0))
                {
                    DataGridView dataGridView = component.DataGridView;
                    IDesignerHost service = null;
                    IContainer container = null;
                    INameCreationService nameCreationService = null;
                    if ((dataGridView != null) && (dataGridView.Site != null))
                    {
                        service = dataGridView.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        nameCreationService = dataGridView.Site.GetService(typeof(INameCreationService)) as INameCreationService;
                    }
                    if (service != null)
                    {
                        container = service.Container;
                    }
                    string errorString = string.Empty;
                    if ((dataGridView != null) && !DataGridViewAddColumnDialog.ValidName(value, dataGridView.Columns, container, nameCreationService, (this.liveDataGridView != null) ? this.liveDataGridView.Columns : null, true, out errorString))
                    {
                        if ((dataGridView != null) && (dataGridView.Site != null))
                        {
                            IUIService uiService = (IUIService) dataGridView.Site.GetService(typeof(IUIService));
                            DataGridViewDesigner.ShowErrorDialog(uiService, errorString, this.liveDataGridView);
                        }
                    }
                    else
                    {
                        if (((service == null) || ((service != null) && !service.Loading)) && (base.Component.Site != null))
                        {
                            base.Component.Site.Name = value;
                        }
                        component.Name = value;
                    }
                }
            }
        }

        private bool UserAddedColumn
        {
            get
            {
                return this.userAddedColumn;
            }
            set
            {
                this.userAddedColumn = value;
            }
        }

        private int Width
        {
            get
            {
                DataGridViewColumn component = (DataGridViewColumn) base.Component;
                return component.Width;
            }
            set
            {
                DataGridViewColumn component = (DataGridViewColumn) base.Component;
                value = Math.Max(component.MinimumWidth, value);
                component.Width = value;
            }
        }

        public class FilterCutCopyPasteDeleteBehavior : System.Windows.Forms.Design.Behavior.Behavior
        {
            public FilterCutCopyPasteDeleteBehavior(bool callParentBehavior, BehaviorService behaviorService) : base(callParentBehavior, behaviorService)
            {
            }

            public override MenuCommand FindCommand(CommandID commandId)
            {
                if ((commandId.ID == StandardCommands.Copy.ID) && (commandId.Guid == StandardCommands.Copy.Guid))
                {
                    return new MenuCommand(new EventHandler(this.handler), StandardCommands.Copy) { Enabled = false };
                }
                if ((commandId.ID == StandardCommands.Paste.ID) && (commandId.Guid == StandardCommands.Paste.Guid))
                {
                    return new MenuCommand(new EventHandler(this.handler), StandardCommands.Paste) { Enabled = false };
                }
                if ((commandId.ID == StandardCommands.Delete.ID) && (commandId.Guid == StandardCommands.Delete.Guid))
                {
                    return new MenuCommand(new EventHandler(this.handler), StandardCommands.Delete) { Enabled = false };
                }
                if ((commandId.ID == StandardCommands.Cut.ID) && (commandId.Guid == StandardCommands.Cut.Guid))
                {
                    return new MenuCommand(new EventHandler(this.handler), StandardCommands.Cut) { Enabled = false };
                }
                return base.FindCommand(commandId);
            }

            private void handler(object sender, EventArgs e)
            {
            }
        }
    }
}

