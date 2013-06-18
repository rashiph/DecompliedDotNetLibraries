namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class RichTextBoxDesigner : TextBoxBaseDesigner
    {
        private DesignerActionListCollection _actionLists;

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            Control control = this.Control;
            if ((control != null) && (control.Handle != IntPtr.Zero))
            {
                System.Design.NativeMethods.RevokeDragDrop(control.Handle);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Text" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(RichTextBoxDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new RichTextBoxActionList(this));
                }
                return this._actionLists;
            }
        }

        private string Text
        {
            get
            {
                return this.Control.Text;
            }
            set
            {
                string text = this.Control.Text;
                if (value != null)
                {
                    value = value.Replace("\r\n", "\n");
                }
                if (text != value)
                {
                    this.Control.Text = value;
                }
            }
        }
    }
}

