namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.Activities.Rules;

    internal class IntellisenseTextBox : TextBox
    {
        private ImageList autoCompletionImageList;
        private IContainer components;
        private ListView listBoxAutoComplete = new ListView();
        private int oldSelectionStart;
        private ToolTip toolTip;

        public event EventHandler<AutoCompletionEventArgs> PopulateAutoCompleteList;

        public event EventHandler<AutoCompletionEventArgs> PopulateToolTipList;

        public IntellisenseTextBox()
        {
            this.InitializeComponent();
            base.AcceptsReturn = true;
            this.listBoxAutoComplete.FullRowSelect = true;
            this.listBoxAutoComplete.MultiSelect = false;
            this.listBoxAutoComplete.SmallImageList = this.autoCompletionImageList;
            this.listBoxAutoComplete.LargeImageList = this.autoCompletionImageList;
            this.listBoxAutoComplete.View = View.Details;
            this.listBoxAutoComplete.HeaderStyle = ColumnHeaderStyle.None;
            this.listBoxAutoComplete.Columns.Add(Messages.No, this.listBoxAutoComplete.Size.Width);
            this.listBoxAutoComplete.CausesValidation = false;
            this.listBoxAutoComplete.Sorting = SortOrder.Ascending;
            this.listBoxAutoComplete.Visible = false;
            base.KeyPress += new KeyPressEventHandler(this.IntellisenseTextBox_KeyPress);
            base.HandleCreated += new EventHandler(this.IntellisenseTextBox_HandleCreated);
        }

        private static void AppendParameterInfo(StringBuilder toolTipText, ParameterInfo parameterInfo, bool isLastParameter)
        {
            System.Type parameterType = parameterInfo.ParameterType;
            if (parameterType != null)
            {
                if (parameterType.IsByRef)
                {
                    if (parameterInfo.IsOut)
                    {
                        toolTipText.Append("out ");
                    }
                    else
                    {
                        toolTipText.Append("ref ");
                    }
                    parameterType = parameterType.GetElementType();
                }
                else if (isLastParameter && parameterType.IsArray)
                {
                    object[] customAttributes = parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        toolTipText.Append("params ");
                    }
                }
                toolTipText.Append(RuleDecompiler.DecompileType(parameterType));
                toolTipText.Append(" ");
            }
            toolTipText.Append(parameterInfo.Name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        internal void HideIntellisenceDropDown()
        {
            this.listBoxAutoComplete.Hide();
            this.toolTip.Hide(this);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(IntellisenseTextBox));
            this.autoCompletionImageList = new ImageList(this.components);
            this.toolTip = new ToolTip(this.components);
            base.SuspendLayout();
            this.autoCompletionImageList.ImageStream = (ImageListStreamer) manager.GetObject("autoCompletionImageList.ImageStream");
            this.autoCompletionImageList.TransparentColor = Color.Magenta;
            this.autoCompletionImageList.Images.SetKeyName(0, "");
            this.autoCompletionImageList.Images.SetKeyName(1, "");
            this.autoCompletionImageList.Images.SetKeyName(2, "");
            this.autoCompletionImageList.Images.SetKeyName(3, "");
            this.autoCompletionImageList.Images.SetKeyName(4, "");
            this.autoCompletionImageList.Images.SetKeyName(5, "");
            this.autoCompletionImageList.Images.SetKeyName(6, "");
            this.autoCompletionImageList.Images.SetKeyName(7, "");
            this.autoCompletionImageList.Images.SetKeyName(8, "");
            this.autoCompletionImageList.Images.SetKeyName(9, "");
            this.autoCompletionImageList.Images.SetKeyName(10, "");
            this.autoCompletionImageList.Images.SetKeyName(11, "");
            this.autoCompletionImageList.Images.SetKeyName(12, "");
            this.autoCompletionImageList.Images.SetKeyName(13, "");
            this.autoCompletionImageList.Images.SetKeyName(14, "Keyword.bmp");
            this.autoCompletionImageList.Images.SetKeyName(15, "MethodExtension.bmp");
            this.toolTip.AutomaticDelay = 0;
            this.toolTip.UseAnimation = false;
            base.Enter += new EventHandler(this.IntellisenseTextBox_Enter);
            base.MouseClick += new MouseEventHandler(this.IntellisenseTextBox_MouseClick);
            base.Leave += new EventHandler(this.IntellisenseTextBox_Leave);
            base.KeyDown += new KeyEventHandler(this.IntellisenseTextBox_KeyDown);
            base.ResumeLayout(false);
        }

        private void IntellisenseTextBox_Enter(object sender, EventArgs e)
        {
            if (this.oldSelectionStart >= 0)
            {
                base.SelectionStart = this.oldSelectionStart;
            }
        }

        private void IntellisenseTextBox_HandleCreated(object sender, EventArgs e)
        {
            if (base.TopLevelControl != null)
            {
                base.TopLevelControl.Controls.Add(this.listBoxAutoComplete);
                this.listBoxAutoComplete.DoubleClick += new EventHandler(this.listBoxAutoComplete_DoubleClick);
                this.listBoxAutoComplete.SelectedIndexChanged += new EventHandler(this.listBoxAutoComplete_SelectedIndexChanged);
                this.listBoxAutoComplete.Enter += new EventHandler(this.listBoxAutoComplete_Enter);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void IntellisenseTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string text = this.Text;
            int selectionStart = base.SelectionStart;
            int selectionLength = this.SelectionLength;
            StringBuilder builder = new StringBuilder(text.Substring(selectionStart, selectionLength));
            StringBuilder builder2 = new StringBuilder(text.Substring(0, selectionStart));
            builder2.Append(text.Substring(selectionStart + selectionLength));
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "KeyCode:{0}, KeyData:{1}, KeyValue:{2}", new object[] { e.KeyCode, e.KeyData, e.KeyValue }));
            this.toolTip.Hide(this);
            if (e.KeyData == (Keys.Control | Keys.Space))
            {
                if (!this.listBoxAutoComplete.Visible)
                {
                    this.UpdateIntellisenceDropDown(this.Text.Substring(0, selectionStart - this.CurrentPrefix.Length));
                    this.ShowIntellisenceDropDown(selectionStart);
                    this.UpdateAutoCompleteSelection(this.CurrentPrefix);
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (this.Text.Length > 0)
                {
                    if ((builder.Length == 0) && (selectionStart > 0))
                    {
                        builder.Append(builder2[selectionStart - 1]);
                        builder2.Length--;
                    }
                    if (this.CurrentPrefix.Length <= 1)
                    {
                        this.HideIntellisenceDropDown();
                    }
                    if (builder.ToString().IndexOfAny(". ()[]\t\n".ToCharArray()) >= 0)
                    {
                        this.HideIntellisenceDropDown();
                    }
                    else if (this.listBoxAutoComplete.Visible)
                    {
                        this.UpdateAutoCompleteSelection(this.CurrentPrefix.Substring(0, this.CurrentPrefix.Length - 1));
                    }
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (this.listBoxAutoComplete.Visible)
                {
                    if ((this.listBoxAutoComplete.SelectedIndices.Count > 0) && (this.listBoxAutoComplete.SelectedIndices[0] > 0))
                    {
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0] - 1].Selected = true;
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0]].Focused = true;
                    }
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (this.listBoxAutoComplete.Visible)
                {
                    if (this.listBoxAutoComplete.SelectedIndices.Count == 0)
                    {
                        if (this.listBoxAutoComplete.Items.Count > 0)
                        {
                            this.listBoxAutoComplete.Items[0].Selected = true;
                            this.listBoxAutoComplete.Items[0].Focused = true;
                        }
                    }
                    else if (this.listBoxAutoComplete.SelectedIndices[0] < (this.listBoxAutoComplete.Items.Count - 1))
                    {
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0] + 1].Selected = true;
                        this.listBoxAutoComplete.Items[this.listBoxAutoComplete.SelectedIndices[0]].Focused = true;
                    }
                    e.Handled = true;
                }
            }
            else if ((((e.KeyCode != Keys.ShiftKey) && (e.KeyCode != Keys.ControlKey)) && (e.KeyCode != Keys.OemPeriod)) && (((((e.KeyValue < 0x30) || ((e.KeyValue >= 0x3a) && (e.KeyValue <= 0x40))) || ((e.KeyValue >= 0x5b) && (e.KeyValue <= 0x60))) || (e.KeyValue > 0x7a)) && ((e.KeyData != (Keys.Shift | Keys.OemMinus)) && this.listBoxAutoComplete.Visible)))
            {
                if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Space))
                {
                    this.SelectItem();
                    e.Handled = true;
                }
                this.HideIntellisenceDropDown();
            }
        }

        private void IntellisenseTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            string text = this.Text;
            int selectionStart = base.SelectionStart;
            int selectionLength = this.SelectionLength;
            StringBuilder builder = new StringBuilder(text.Substring(0, selectionStart));
            builder.Append(text.Substring(selectionStart + selectionLength));
            char keyChar = e.KeyChar;
            switch (keyChar)
            {
                case '.':
                    if (this.listBoxAutoComplete.Visible)
                    {
                        this.SelectItem();
                        this.HideIntellisenceDropDown();
                        this.IntellisenseTextBox_KeyPress(sender, e);
                        return;
                    }
                    builder.Insert(selectionStart, '.');
                    this.UpdateIntellisenceDropDown(builder.ToString().Substring(0, selectionStart + 1));
                    this.ShowIntellisenceDropDown(selectionStart);
                    this.IntellisenseTextBox_KeyDown(sender, new KeyEventArgs(Keys.Down));
                    return;

                case '(':
                    if (this.listBoxAutoComplete.Visible)
                    {
                        this.SelectItem();
                        this.HideIntellisenceDropDown();
                        this.IntellisenseTextBox_KeyPress(sender, e);
                        return;
                    }
                    builder.Insert(selectionStart, '(');
                    this.ShowToolTip(selectionStart, builder.ToString().Substring(0, selectionStart + 1));
                    return;
            }
            if ((!this.listBoxAutoComplete.Visible && (this.CurrentPrefix.Length == 0)) && (((keyChar == '_') || char.IsLetter(keyChar)) || (char.GetUnicodeCategory(keyChar) == UnicodeCategory.LetterNumber)))
            {
                builder.Insert(selectionStart, keyChar);
                this.UpdateIntellisenceDropDown(builder.ToString().Substring(0, selectionStart + 1));
                this.ShowIntellisenceDropDown(selectionStart);
                if (this.listBoxAutoComplete.Visible)
                {
                    this.IntellisenseTextBox_KeyDown(sender, new KeyEventArgs(Keys.Down));
                }
            }
            else if (this.listBoxAutoComplete.Visible)
            {
                builder.Insert(selectionStart, keyChar);
                this.UpdateAutoCompleteSelection(this.CurrentPrefix + keyChar);
            }
        }

        private void IntellisenseTextBox_Leave(object sender, EventArgs e)
        {
            this.oldSelectionStart = base.SelectionStart;
            this.toolTip.Hide(this);
            if (!this.listBoxAutoComplete.Focused && !this.Focused)
            {
                this.listBoxAutoComplete.Visible = false;
            }
        }

        private void IntellisenseTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            this.HideIntellisenceDropDown();
        }

        private void listBoxAutoComplete_DoubleClick(object sender, EventArgs e)
        {
            if (this.listBoxAutoComplete.SelectedItems.Count == 1)
            {
                this.SelectItem();
                this.HideIntellisenceDropDown();
            }
        }

        private void listBoxAutoComplete_Enter(object sender, EventArgs e)
        {
            base.CausesValidation = false;
            base.Focus();
            base.CausesValidation = true;
        }

        private void listBoxAutoComplete_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listBoxAutoComplete.Items)
            {
                if (item.Selected)
                {
                    item.ForeColor = SystemColors.HighlightText;
                    item.BackColor = SystemColors.Highlight;
                    item.EnsureVisible();
                }
                else
                {
                    item.ForeColor = SystemColors.ControlText;
                    item.BackColor = SystemColors.Window;
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void PopulateListBox(ICollection list)
        {
            this.listBoxAutoComplete.Items.Clear();
            if ((list != null) && (list.Count > 0))
            {
                foreach (object obj2 in list)
                {
                    ListViewItem item = null;
                    if (obj2 is string)
                    {
                        item = new ListViewItem(obj2 as string) {
                            ImageIndex = 0
                        };
                    }
                    else if (obj2 is IntellisenseKeyword)
                    {
                        item = new ListViewItem(((IntellisenseKeyword) obj2).Name) {
                            ImageIndex = 14
                        };
                    }
                    else if (obj2 is MemberInfo)
                    {
                        item = new ListViewItem(((MemberInfo) obj2).Name);
                        if (obj2 is PropertyInfo)
                        {
                            MethodInfo getMethod = ((PropertyInfo) obj2).GetGetMethod(true);
                            if (getMethod == null)
                            {
                                getMethod = ((PropertyInfo) obj2).GetSetMethod(true);
                            }
                            if (getMethod.IsPublic)
                            {
                                item.ImageIndex = 6;
                            }
                            else if (getMethod.IsPrivate)
                            {
                                item.ImageIndex = 7;
                            }
                            else if ((getMethod.IsFamily || getMethod.IsFamilyAndAssembly) || getMethod.IsFamilyOrAssembly)
                            {
                                item.ImageIndex = 9;
                            }
                            else
                            {
                                item.ImageIndex = 8;
                            }
                        }
                        else if (obj2 is FieldInfo)
                        {
                            FieldInfo info2 = (FieldInfo) obj2;
                            if (info2.IsPublic)
                            {
                                item.ImageIndex = 10;
                            }
                            else if (info2.IsPrivate)
                            {
                                item.ImageIndex = 11;
                            }
                            else if ((info2.IsFamily || info2.IsFamilyAndAssembly) || info2.IsFamilyOrAssembly)
                            {
                                item.ImageIndex = 13;
                            }
                            else
                            {
                                item.ImageIndex = 12;
                            }
                        }
                        else if (obj2 is ExtensionMethodInfo)
                        {
                            item.ImageIndex = 15;
                        }
                        else if (obj2 is MethodInfo)
                        {
                            MethodInfo info3 = (MethodInfo) obj2;
                            if (info3.IsPublic)
                            {
                                item.ImageIndex = 2;
                            }
                            else if (info3.IsPrivate)
                            {
                                item.ImageIndex = 3;
                            }
                            else if ((info3.IsFamily || info3.IsFamilyAndAssembly) || info3.IsFamilyOrAssembly)
                            {
                                item.ImageIndex = 5;
                            }
                            else
                            {
                                item.ImageIndex = 4;
                            }
                        }
                        else if (obj2 is System.Type)
                        {
                            item.ImageIndex = 1;
                        }
                    }
                    this.listBoxAutoComplete.Items.Add(item);
                }
            }
            this.listBoxAutoComplete.Sort();
            if (this.listBoxAutoComplete.Items.Count > 0)
            {
                this.listBoxAutoComplete.Columns[0].Width = -2;
                this.listBoxAutoComplete.Size = new Size(this.listBoxAutoComplete.Items[0].Bounds.Width + 30, 0x48);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.listBoxAutoComplete.Visible)
            {
                switch (keyData)
                {
                    case Keys.Tab:
                    case Keys.Enter:
                        this.SelectItem();
                        this.HideIntellisenceDropDown();
                        return true;

                    case Keys.Escape:
                        this.HideIntellisenceDropDown();
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SelectItem()
        {
            if (this.listBoxAutoComplete.SelectedItems.Count > 0)
            {
                int selectionStart = base.SelectionStart;
                int length = selectionStart - this.CurrentPrefix.Length;
                int startIndex = selectionStart;
                if (startIndex >= this.Text.Length)
                {
                    startIndex = this.Text.Length;
                }
                string str = this.Text.Substring(0, length);
                string text = this.listBoxAutoComplete.SelectedItems[0].Text;
                string str3 = this.Text.Substring(startIndex, this.Text.Length - startIndex);
                this.Text = str + text + str3;
                base.SelectionStart = str.Length + text.Length;
                base.ScrollToCaret();
                this.oldSelectionStart = base.SelectionStart;
            }
        }

        private void ShowIntellisenceDropDown(int charIndex)
        {
            if (this.listBoxAutoComplete.Items.Count > 0)
            {
                Point positionFromCharIndex = this.GetPositionFromCharIndex(charIndex - 1);
                positionFromCharIndex.Y += ((int) Math.Ceiling((double) this.Font.GetHeight())) + 2;
                positionFromCharIndex.X -= 6;
                if ((charIndex > 0) && (this.Text[charIndex - 1] == '\n'))
                {
                    positionFromCharIndex.Y += (int) Math.Ceiling((double) this.Font.GetHeight());
                    positionFromCharIndex.X = this.GetPositionFromCharIndex(0).X - 6;
                }
                Point point2 = base.TopLevelControl.PointToScreen(new Point(0, 0));
                Point location = base.PointToScreen(positionFromCharIndex);
                location.Offset(-point2.X, -point2.Y);
                Size size = (base.TopLevelControl is System.Windows.Forms.Form) ? ((System.Windows.Forms.Form) base.TopLevelControl).ClientSize : base.TopLevelControl.Size;
                Rectangle rectangle = new Rectangle(location, this.listBoxAutoComplete.Size);
                if (rectangle.Right > size.Width)
                {
                    if (this.listBoxAutoComplete.Size.Width > size.Width)
                    {
                        this.listBoxAutoComplete.Size = new Size(size.Width, this.listBoxAutoComplete.Height);
                    }
                    location = new Point(size.Width - this.listBoxAutoComplete.Size.Width, location.Y);
                }
                if (rectangle.Bottom > size.Height)
                {
                    this.listBoxAutoComplete.Size = new Size(this.listBoxAutoComplete.Width, size.Height - rectangle.Top);
                }
                this.listBoxAutoComplete.Location = location;
                this.listBoxAutoComplete.BringToFront();
                this.listBoxAutoComplete.Show();
            }
        }

        private void ShowToolTip(int charIndex, string prefix)
        {
            Point positionFromCharIndex = this.GetPositionFromCharIndex(charIndex - 1);
            positionFromCharIndex.Y += ((int) Math.Ceiling((double) this.Font.GetHeight())) + 2;
            positionFromCharIndex.X -= 6;
            AutoCompletionEventArgs e = new AutoCompletionEventArgs {
                Prefix = prefix
            };
            if (this.PopulateToolTipList != null)
            {
                this.PopulateToolTipList(this, e);
                if (e.AutoCompleteValues != null)
                {
                    StringBuilder toolTipText = new StringBuilder();
                    bool flag = true;
                    foreach (MemberInfo info in e.AutoCompleteValues)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            toolTipText.Append("\n");
                        }
                        ParameterInfo[] parameters = null;
                        MethodInfo info2 = info as MethodInfo;
                        if (info2 != null)
                        {
                            toolTipText.Append(RuleDecompiler.DecompileType(info2.ReturnType));
                            toolTipText.Append(" ");
                            toolTipText.Append(info2.Name);
                            toolTipText.Append("(");
                            parameters = info2.GetParameters();
                        }
                        else
                        {
                            ConstructorInfo info3 = (ConstructorInfo) info;
                            toolTipText.Append(RuleDecompiler.DecompileType(info3.DeclaringType));
                            toolTipText.Append("(");
                            parameters = info3.GetParameters();
                        }
                        if ((parameters != null) && (parameters.Length > 0))
                        {
                            int num = parameters.Length - 1;
                            AppendParameterInfo(toolTipText, parameters[0], 0 == num);
                            for (int i = 1; i < parameters.Length; i++)
                            {
                                toolTipText.Append(", ");
                                AppendParameterInfo(toolTipText, parameters[i], i == num);
                            }
                        }
                        toolTipText.Append(")");
                    }
                    this.toolTip.Show(toolTipText.ToString(), this, positionFromCharIndex);
                }
            }
        }

        private void UpdateAutoCompleteSelection(string currentValue)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(currentValue.Trim()) && (this.listBoxAutoComplete.Items.Count > 0))
            {
                flag = true;
                this.listBoxAutoComplete.Items[0].Selected = true;
                this.listBoxAutoComplete.Items[0].Focused = true;
            }
            else
            {
                for (int i = 0; i < this.listBoxAutoComplete.Items.Count; i++)
                {
                    if (this.listBoxAutoComplete.Items[i].Text.StartsWith(currentValue, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                        this.listBoxAutoComplete.Items[i].Selected = true;
                        this.listBoxAutoComplete.Items[i].Focused = true;
                        break;
                    }
                }
            }
            if (!flag && (this.listBoxAutoComplete.SelectedItems.Count == 1))
            {
                this.listBoxAutoComplete.SelectedItems[0].Selected = false;
            }
        }

        private void UpdateIntellisenceDropDown(string text)
        {
            AutoCompletionEventArgs e = new AutoCompletionEventArgs {
                Prefix = text
            };
            if (this.PopulateAutoCompleteList != null)
            {
                this.PopulateAutoCompleteList(this, e);
            }
            this.PopulateListBox(e.AutoCompleteValues);
        }

        private string CurrentPrefix
        {
            get
            {
                string str = this.Text.Substring(0, base.SelectionStart);
                int num = str.LastIndexOfAny(" .()[]\t\r\n".ToCharArray());
                if (num >= 0)
                {
                    return str.Substring(num + 1);
                }
                return str;
            }
        }

        private enum memberIcons
        {
            Default,
            Type,
            PublicMethod,
            PrivateMethod,
            InternalMethod,
            ProtectedMethod,
            PublicProperty,
            PrivateProperty,
            InternalProperty,
            ProtectedProperty,
            PublicField,
            PrivateField,
            InternalField,
            ProtectedField,
            Keyword,
            ExtensionMethod
        }
    }
}

