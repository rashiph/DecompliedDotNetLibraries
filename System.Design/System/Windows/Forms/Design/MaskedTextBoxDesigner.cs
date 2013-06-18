namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Windows.Forms;

    internal class MaskedTextBoxDesigner : TextBoxBaseDesigner
    {
        private DesignerActionListCollection actions;
        private DesignerVerbCollection verbs;

        internal static MaskedTextBox GetDesignMaskedTextBox(MaskedTextBox mtb)
        {
            MaskedTextBox box = null;
            if (mtb == null)
            {
                box = new MaskedTextBox();
            }
            else
            {
                if (mtb.MaskedTextProvider == null)
                {
                    box = new MaskedTextBox {
                        Text = mtb.Text
                    };
                }
                else
                {
                    box = new MaskedTextBox(mtb.MaskedTextProvider);
                }
                box.ValidatingType = mtb.ValidatingType;
                box.BeepOnError = mtb.BeepOnError;
                box.InsertKeyMode = mtb.InsertKeyMode;
                box.RejectInputOnFirstFailure = mtb.RejectInputOnFirstFailure;
                box.CutCopyMaskFormat = mtb.CutCopyMaskFormat;
                box.Culture = mtb.Culture;
            }
            box.UseSystemPasswordChar = false;
            box.PasswordChar = '\0';
            box.ReadOnly = false;
            box.HidePromptOnLeave = false;
            return box;
        }

        internal static string GetMaskInputRejectedErrorMessage(MaskInputRejectedEventArgs e)
        {
            string str;
            switch (e.RejectionHint)
            {
                case MaskedTextResultHint.PositionOutOfRange:
                    str = System.Design.SR.GetString("MaskedTextBoxHintPositionOutOfRange");
                    break;

                case MaskedTextResultHint.NonEditPosition:
                    str = System.Design.SR.GetString("MaskedTextBoxHintNonEditPosition");
                    break;

                case MaskedTextResultHint.UnavailableEditPosition:
                    str = System.Design.SR.GetString("MaskedTextBoxHintUnavailableEditPosition");
                    break;

                case MaskedTextResultHint.PromptCharNotAllowed:
                    str = System.Design.SR.GetString("MaskedTextBoxHintPromptCharNotAllowed");
                    break;

                case MaskedTextResultHint.SignedDigitExpected:
                    str = System.Design.SR.GetString("MaskedTextBoxHintSignedDigitExpected");
                    break;

                case MaskedTextResultHint.LetterExpected:
                    str = System.Design.SR.GetString("MaskedTextBoxHintLetterExpected");
                    break;

                case MaskedTextResultHint.DigitExpected:
                    str = System.Design.SR.GetString("MaskedTextBoxHintDigitExpected");
                    break;

                case MaskedTextResultHint.AlphanumericCharacterExpected:
                    str = System.Design.SR.GetString("MaskedTextBoxHintAlphanumericCharacterExpected");
                    break;

                case MaskedTextResultHint.AsciiCharacterExpected:
                    str = System.Design.SR.GetString("MaskedTextBoxHintAsciiCharacterExpected");
                    break;

                default:
                    str = System.Design.SR.GetString("MaskedTextBoxHintInvalidInput");
                    break;
            }
            return string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("MaskedTextBoxTextEditorErrorFormatString"), new object[] { e.Position, str });
        }

        [Obsolete("This method has been deprecated. Use InitializeNewComponent instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public override void OnSetComponentDefaults()
        {
        }

        private void OnVerbSetMask(object sender, EventArgs e)
        {
            new MaskedTextBoxDesignerActionList(this).SetMask();
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Text", "PasswordChar" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(MaskedTextBoxDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this.actions == null)
                {
                    this.actions = new DesignerActionListCollection();
                    this.actions.Add(new MaskedTextBoxDesignerActionList(this));
                }
                return this.actions;
            }
        }

        private char PasswordChar
        {
            get
            {
                MaskedTextBox control = this.Control as MaskedTextBox;
                if (control.UseSystemPasswordChar)
                {
                    control.UseSystemPasswordChar = false;
                    char passwordChar = control.PasswordChar;
                    control.UseSystemPasswordChar = true;
                    return passwordChar;
                }
                return control.PasswordChar;
            }
            set
            {
                MaskedTextBox control = this.Control as MaskedTextBox;
                control.PasswordChar = value;
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                return (base.SelectionRules & ~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
            }
        }

        private string Text
        {
            get
            {
                MaskedTextBox control = this.Control as MaskedTextBox;
                if (string.IsNullOrEmpty(control.Mask))
                {
                    return control.Text;
                }
                return control.MaskedTextProvider.ToString(false, false);
            }
            set
            {
                MaskedTextBox control = this.Control as MaskedTextBox;
                if (string.IsNullOrEmpty(control.Mask))
                {
                    control.Text = value;
                }
                else
                {
                    bool resetOnSpace = control.ResetOnSpace;
                    bool resetOnPrompt = control.ResetOnPrompt;
                    bool skipLiterals = control.SkipLiterals;
                    control.ResetOnSpace = true;
                    control.ResetOnPrompt = true;
                    control.SkipLiterals = true;
                    control.Text = value;
                    control.ResetOnSpace = resetOnSpace;
                    control.ResetOnPrompt = resetOnPrompt;
                    control.SkipLiterals = skipLiterals;
                }
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this.verbs == null)
                {
                    this.verbs = new DesignerVerbCollection();
                    this.verbs.Add(new DesignerVerb(System.Design.SR.GetString("MaskedTextBoxDesignerVerbsSetMaskDesc"), new EventHandler(this.OnVerbSetMask)));
                }
                return this.verbs;
            }
        }
    }
}

