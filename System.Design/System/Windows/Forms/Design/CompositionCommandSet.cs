namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class CompositionCommandSet : CommandSet
    {
        private CommandSet.CommandSetItem[] commandSet;
        private Control compositionUI;

        public CompositionCommandSet(Control compositionUI, ISite site) : base(site)
        {
            this.compositionUI = compositionUI;
            this.commandSet = new CommandSet.CommandSetItem[] { new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySelect), MenuCommands.KeySelectNext), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySelect), MenuCommands.KeySelectPrevious) };
            if (base.MenuService != null)
            {
                for (int i = 0; i < this.commandSet.Length; i++)
                {
                    base.MenuService.AddCommand(this.commandSet[i]);
                }
            }
        }

        public override void Dispose()
        {
            if (base.MenuService != null)
            {
                for (int i = 0; i < this.commandSet.Length; i++)
                {
                    base.MenuService.RemoveCommand(this.commandSet[i]);
                    this.commandSet[i].Dispose();
                }
            }
            base.Dispose();
        }

        protected override bool OnKeyCancel(object sender)
        {
            if (base.OnKeyCancel(sender))
            {
                return false;
            }
            ISelectionService selectionService = base.SelectionService;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((selectionService != null) && (service != null))
            {
                IComponent rootComponent = service.RootComponent;
                selectionService.SetSelectedComponents(new object[] { rootComponent }, SelectionTypes.Replace);
            }
            return true;
        }

        protected void OnKeySelect(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            bool backwards = command.CommandID.Equals(MenuCommands.KeySelectPrevious);
            this.RotateTabSelection(backwards);
        }

        protected override void OnUpdateCommandStatus()
        {
            for (int i = 0; i < this.commandSet.Length; i++)
            {
                this.commandSet[i].UpdateStatus();
            }
            base.OnUpdateCommandStatus();
        }

        private void RotateTabSelection(bool backwards)
        {
            ComponentTray.TrayControl control2 = null;
            ISelectionService selectionService = base.SelectionService;
            if (selectionService != null)
            {
                IComponent component;
                Control control;
                IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                if (primarySelection != null)
                {
                    component = primarySelection;
                }
                else
                {
                    component = null;
                    foreach (object obj2 in selectionService.GetSelectedComponents())
                    {
                        IComponent component3 = obj2 as IComponent;
                        if (component3 != null)
                        {
                            component = component3;
                            break;
                        }
                    }
                }
                if (component != null)
                {
                    control = ComponentTray.TrayControl.FromComponent(component);
                }
                else
                {
                    control = null;
                }
                if (control != null)
                {
                    for (int i = 1; i < this.compositionUI.Controls.Count; i++)
                    {
                        if (this.compositionUI.Controls[i] == control)
                        {
                            int num2 = i + 1;
                            if (num2 >= this.compositionUI.Controls.Count)
                            {
                                num2 = 1;
                            }
                            ComponentTray.TrayControl control3 = this.compositionUI.Controls[num2] as ComponentTray.TrayControl;
                            if (control3 != null)
                            {
                                control2 = control3;
                                break;
                            }
                        }
                    }
                }
                else if (this.compositionUI.Controls.Count > 1)
                {
                    ComponentTray.TrayControl control4 = this.compositionUI.Controls[1] as ComponentTray.TrayControl;
                    if (control4 != null)
                    {
                        control2 = control4;
                    }
                }
                if (control2 != null)
                {
                    selectionService.SetSelectedComponents(new object[] { control2.Component }, SelectionTypes.Replace);
                }
            }
        }
    }
}

