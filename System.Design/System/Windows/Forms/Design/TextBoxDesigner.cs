namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class TextBoxDesigner : TextBoxBaseDesigner
    {
        private DesignerActionListCollection _actionLists;
        private char passwordChar;

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "PasswordChar" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(TextBoxDesigner), oldPropertyDescriptor, attributes);
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
                    this._actionLists.Add(new TextBoxActionList(this));
                }
                return this._actionLists;
            }
        }

        private char PasswordChar
        {
            get
            {
                TextBox control = this.Control as TextBox;
                if (control.UseSystemPasswordChar)
                {
                    return this.passwordChar;
                }
                return control.PasswordChar;
            }
            set
            {
                TextBox control = this.Control as TextBox;
                this.passwordChar = value;
                control.PasswordChar = value;
            }
        }
    }
}

