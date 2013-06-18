namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.ComponentModel.Design;

    internal sealed class DesignerActionKeyboardBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private DesignerActionUIService daUISvc;
        private IMenuCommandService menuService;
        private DesignerActionPanel panel;
        private static readonly Guid VSStandardCommandSet97 = new Guid("{5efc7975-14bc-11cf-9b2b-00aa00573819}");

        public DesignerActionKeyboardBehavior(DesignerActionPanel panel, IServiceProvider serviceProvider, BehaviorService behaviorService) : base(true, behaviorService)
        {
            this.panel = panel;
            if (serviceProvider != null)
            {
                this.menuService = serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
                this.daUISvc = serviceProvider.GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
            }
        }

        public override MenuCommand FindCommand(CommandID commandId)
        {
            if ((this.panel != null) && (this.menuService != null))
            {
                foreach (CommandID did in this.panel.FilteredCommandIDs)
                {
                    if (did.Equals(commandId))
                    {
                        return new MenuCommand(delegate {
                        }, commandId) { Enabled = false };
                    }
                }
                if (((this.daUISvc != null) && (commandId.Guid == VSStandardCommandSet97)) && (commandId.ID == 0x464))
                {
                    this.daUISvc.HideUI(null);
                }
            }
            return base.FindCommand(commandId);
        }
    }
}

