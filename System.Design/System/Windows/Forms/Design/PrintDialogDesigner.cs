namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class PrintDialogDesigner : ComponentDesigner
    {
        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            PrintDialog component = base.Component as PrintDialog;
            if (component != null)
            {
                component.UseEXDialog = true;
            }
        }
    }
}

