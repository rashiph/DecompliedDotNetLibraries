namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class StringCollectionEditor : CollectionEditor
    {
        public StringCollectionEditor(System.Type type) : base(type)
        {
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            return new StringCollectionForm(this);
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.ComponentModel.StringCollectionEditor";
            }
        }

        private class StringCollectionForm : CollectionEditor.CollectionForm
        {
            private Button cancelButton;
            private StringCollectionEditor editor;
            private Label instruction;
            private Button okButton;
            private TableLayoutPanel okCancelTableLayoutPanel;
            private TextBox textEntry;

            public StringCollectionForm(CollectionEditor editor) : base(editor)
            {
                this.editor = (StringCollectionEditor) editor;
                this.InitializeComponent();
                this.HookEvents();
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

            private void HookEvents()
            {
                this.textEntry.KeyDown += new KeyEventHandler(this.Edit1_keyDown);
                this.okButton.Click += new EventHandler(this.OKButton_click);
                base.HelpButtonClicked += new CancelEventHandler(this.StringCollectionEditor_HelpButtonClicked);
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(StringCollectionEditor));
                this.instruction = new Label();
                this.textEntry = new TextBox();
                this.okButton = new Button();
                this.cancelButton = new Button();
                this.okCancelTableLayoutPanel = new TableLayoutPanel();
                this.okCancelTableLayoutPanel.SuspendLayout();
                base.SuspendLayout();
                manager.ApplyResources(this.instruction, "instruction");
                this.instruction.Margin = new Padding(3, 1, 3, 0);
                this.instruction.Name = "instruction";
                this.textEntry.AcceptsTab = true;
                this.textEntry.AcceptsReturn = true;
                manager.ApplyResources(this.textEntry, "textEntry");
                this.textEntry.Name = "textEntry";
                manager.ApplyResources(this.okButton, "okButton");
                this.okButton.DialogResult = DialogResult.OK;
                this.okButton.Margin = new Padding(0, 0, 3, 0);
                this.okButton.Name = "okButton";
                manager.ApplyResources(this.cancelButton, "cancelButton");
                this.cancelButton.DialogResult = DialogResult.Cancel;
                this.cancelButton.Margin = new Padding(3, 0, 0, 0);
                this.cancelButton.Name = "cancelButton";
                manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
                this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
                this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
                this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this, "$this");
                base.AutoScaleMode = AutoScaleMode.Font;
                base.Controls.Add(this.okCancelTableLayoutPanel);
                base.Controls.Add(this.instruction);
                base.Controls.Add(this.textEntry);
                base.HelpButton = true;
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                base.Name = "StringCollectionEditor";
                base.ShowIcon = false;
                base.ShowInTaskbar = false;
                this.okCancelTableLayoutPanel.ResumeLayout(false);
                this.okCancelTableLayoutPanel.PerformLayout();
                base.HelpRequested += new HelpEventHandler(this.Form_HelpRequested);
                base.ResumeLayout(false);
                base.PerformLayout();
            }

            private void OKButton_click(object sender, EventArgs e)
            {
                char[] separator = new char[] { '\n' };
                char[] trimChars = new char[] { '\r' };
                string[] strArray = this.textEntry.Text.Split(separator);
                object[] items = base.Items;
                int length = strArray.Length;
                for (int i = 0; i < length; i++)
                {
                    strArray[i] = strArray[i].Trim(trimChars);
                }
                bool flag = true;
                if (length == items.Length)
                {
                    int index = 0;
                    while (index < length)
                    {
                        if (!strArray[index].Equals((string) items[index]))
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
                    if ((strArray.Length > 0) && (strArray[strArray.Length - 1].Length == 0))
                    {
                        length--;
                    }
                    object[] objArray2 = new object[length];
                    for (int j = 0; j < length; j++)
                    {
                        objArray2[j] = strArray[j];
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
                    if (items[i] is string)
                    {
                        str = str + ((string) items[i]);
                        if (i != (items.Length - 1))
                        {
                            str = str + "\r\n";
                        }
                    }
                }
                this.textEntry.Text = str;
            }

            private void StringCollectionEditor_HelpButtonClicked(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                this.editor.ShowHelp();
            }
        }
    }
}

