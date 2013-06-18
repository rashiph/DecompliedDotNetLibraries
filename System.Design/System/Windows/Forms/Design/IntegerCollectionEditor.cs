namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal class IntegerCollectionEditor : CollectionEditor
    {
        public IntegerCollectionEditor(System.Type type) : base(type)
        {
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            return new IntegerCollectionForm(this);
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.ComponentModel.IntegerCollectionEditor";
            }
        }

        private class IntegerCollectionForm : CollectionEditor.CollectionForm
        {
            private Button cancelButton;
            private IntegerCollectionEditor editor;
            private Button helpButton;
            private Label instruction;
            private Button okButton;
            private TextBox textEntry;

            public IntegerCollectionForm(CollectionEditor editor) : base(editor)
            {
                this.instruction = new Label();
                this.textEntry = new TextBox();
                this.okButton = new Button();
                this.cancelButton = new Button();
                this.helpButton = new Button();
                this.editor = (IntegerCollectionEditor) editor;
                this.InitializeComponent();
            }

            private void Edit1_keyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.cancelButton.PerformClick();
                    e.Handled = true;
                }
            }

            private void Form_HelpRequested(object sender, HelpEventArgs e)
            {
                this.editor.ShowHelp();
            }

            private void HelpButton_click(object sender, EventArgs e)
            {
                this.editor.ShowHelp();
            }

            private void InitializeComponent()
            {
                this.instruction.Location = new Point(4, 7);
                this.instruction.Size = new Size(0x1a6, 14);
                this.instruction.TabIndex = 0;
                this.instruction.TabStop = false;
                this.instruction.Text = System.Design.SR.GetString("IntegerCollectionEditorInstruction");
                this.textEntry.Location = new Point(4, 0x16);
                this.textEntry.Size = new Size(0x1a6, 0xf4);
                this.textEntry.TabIndex = 0;
                this.textEntry.Text = "";
                this.textEntry.AcceptsTab = false;
                this.textEntry.AcceptsReturn = true;
                this.textEntry.AutoSize = false;
                this.textEntry.Multiline = true;
                this.textEntry.ScrollBars = ScrollBars.Both;
                this.textEntry.WordWrap = false;
                this.textEntry.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                this.textEntry.KeyDown += new KeyEventHandler(this.Edit1_keyDown);
                this.okButton.Location = new Point(0xb9, 0x112);
                this.okButton.Size = new Size(0x4b, 0x17);
                this.okButton.TabIndex = 1;
                this.okButton.Text = System.Design.SR.GetString("IntegerCollectionEditorOKCaption");
                this.okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this.okButton.DialogResult = DialogResult.OK;
                this.okButton.Click += new EventHandler(this.OKButton_click);
                this.cancelButton.Location = new Point(0x108, 0x112);
                this.cancelButton.Size = new Size(0x4b, 0x17);
                this.cancelButton.TabIndex = 2;
                this.cancelButton.Text = System.Design.SR.GetString("IntegerCollectionEditorCancelCaption");
                this.cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this.cancelButton.DialogResult = DialogResult.Cancel;
                this.helpButton.Location = new Point(0x157, 0x112);
                this.helpButton.Size = new Size(0x4b, 0x17);
                this.helpButton.TabIndex = 3;
                this.helpButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this.helpButton.Text = System.Design.SR.GetString("IntegerCollectionEditorHelpCaption");
                base.Location = new Point(7, 7);
                this.Text = System.Design.SR.GetString("IntegerCollectionEditorTitle");
                base.AcceptButton = this.okButton;
                base.AutoScaleMode = AutoScaleMode.Font;
                base.AutoScaleDimensions = new SizeF(6f, 13f);
                base.CancelButton = this.cancelButton;
                base.ClientSize = new Size(0x1ad, 0x133);
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                base.ControlBox = false;
                base.ShowInTaskbar = false;
                base.StartPosition = FormStartPosition.CenterScreen;
                this.MinimumSize = new Size(300, 200);
                this.helpButton.Click += new EventHandler(this.HelpButton_click);
                base.HelpRequested += new HelpEventHandler(this.Form_HelpRequested);
                base.Controls.Clear();
                base.Controls.AddRange(new Control[] { this.instruction, this.textEntry, this.okButton, this.cancelButton, this.helpButton });
            }

            private void OKButton_click(object sender, EventArgs e)
            {
                char[] separator = new char[] { '\n' };
                char[] trimChars = new char[] { '\r' };
                string[] strArray = this.textEntry.Text.Split(separator);
                object[] items = base.Items;
                int length = strArray.Length;
                if ((strArray.Length > 0) && (strArray[strArray.Length - 1].Length == 0))
                {
                    length--;
                }
                int[] numArray = new int[length];
                for (int i = 0; i < length; i++)
                {
                    strArray[i] = strArray[i].Trim(trimChars);
                    try
                    {
                        numArray[i] = int.Parse(strArray[i], CultureInfo.CurrentCulture);
                    }
                    catch (Exception exception)
                    {
                        this.DisplayError(exception);
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
                bool flag = true;
                if (length == items.Length)
                {
                    int index = 0;
                    while (index < length)
                    {
                        if (!numArray[index].Equals((int) items[index]))
                        {
                            break;
                        }
                        index++;
                    }
                    if (index == length)
                    {
                        flag = false;
                    }
                }
                if (!flag)
                {
                    base.DialogResult = DialogResult.Cancel;
                }
                else
                {
                    object[] objArray2 = new object[length];
                    for (int j = 0; j < length; j++)
                    {
                        objArray2[j] = numArray[j];
                    }
                    base.Items = objArray2;
                }
            }

            protected override void OnEditValueChanged()
            {
                object[] items = base.Items;
                string str = string.Empty;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] is int)
                    {
                        str = str + ((int) items[i]).ToString(CultureInfo.CurrentCulture);
                        if (i != (items.Length - 1))
                        {
                            str = str + "\r\n";
                        }
                    }
                }
                this.textEntry.Text = str;
            }
        }
    }
}

