namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class MaskedTextBoxDesignerActionList : DesignerActionList
    {
        private ITypeDiscoveryService discoverySvc;
        private IHelpService helpService;
        private MaskedTextBox maskedTextBox;
        private IUIService uiSvc;

        public MaskedTextBoxDesignerActionList(MaskedTextBoxDesigner designer) : base(designer.Component)
        {
            this.maskedTextBox = (MaskedTextBox) designer.Component;
            this.discoverySvc = base.GetService(typeof(ITypeDiscoveryService)) as ITypeDiscoveryService;
            this.uiSvc = base.GetService(typeof(IUIService)) as IUIService;
            this.helpService = base.GetService(typeof(IHelpService)) as IHelpService;
            if (this.discoverySvc != null)
            {
                IUIService uiSvc = this.uiSvc;
            }
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "SetMask", System.Design.SR.GetString("MaskedTextBoxDesignerVerbsSetMaskDesc")));
            return items;
        }

        public void SetMask()
        {
            string str = MaskPropertyEditor.EditMask(this.discoverySvc, this.uiSvc, this.maskedTextBox, this.helpService);
            if (str != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.maskedTextBox)["Mask"];
                if (descriptor != null)
                {
                    descriptor.SetValue(this.maskedTextBox, str);
                }
            }
        }
    }
}

