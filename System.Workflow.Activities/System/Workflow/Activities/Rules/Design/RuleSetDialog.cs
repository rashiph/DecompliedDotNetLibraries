namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    public class RuleSetDialog : System.Windows.Forms.Form
    {
        private CheckBox activeCheckBox;
        private ColumnHeader activeColumnHeader;
        private Button buttonCancel;
        private Button buttonOK;
        private ComboBox chainingBehaviourComboBox;
        private Label chainingLabel;
        private IContainer components;
        private ErrorProvider conditionErrorProvider;
        private Label conditionLabel;
        private IntellisenseTextBox conditionTextBox;
        private ToolStripButton deleteToolStripButton;
        private System.Workflow.Activities.Rules.RuleSet dialogRuleSet;
        private ErrorProvider elseErrorProvider;
        private Label elseLabel;
        private IntellisenseTextBox elseTextBox;
        private Label headerTextLabel;
        private ImageList imageList;
        private ColumnHeader nameColumnHeader;
        private Label nameLabel;
        private TextBox nameTextBox;
        private ToolStripButton newRuleToolStripButton;
        private const int numCols = 4;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private Panel panel1;
        private PictureBox pictureBoxHeader;
        private ColumnHeader priorityColumnHeader;
        private Label priorityLabel;
        private TextBox priorityTextBox;
        private ComboBox reevaluationComboBox;
        private ColumnHeader reevaluationCountColumnHeader;
        private Label reevaluationLabel;
        private GroupBox ruleGroupBox;
        private Parser ruleParser;
        private ColumnHeader rulePreviewColumnHeader;
        private GroupBox rulesGroupBox;
        private ListView rulesListView;
        private ToolStrip rulesToolStrip;
        private IServiceProvider serviceProvider;
        private bool[] sortOrder;
        private ErrorProvider thenErrorProvider;
        private Label thenLabel;
        private IntellisenseTextBox thenTextBox;
        private ToolStripSeparator toolStripSeparator1;

        public RuleSetDialog(Activity activity, System.Workflow.Activities.Rules.RuleSet ruleSet)
        {
            ITypeProvider service;
            this.sortOrder = new bool[4];
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            this.InitializeDialog(ruleSet);
            this.serviceProvider = activity.Site;
            if (this.serviceProvider != null)
            {
                service = (ITypeProvider) this.serviceProvider.GetService(typeof(ITypeProvider));
                if (service == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(ITypeProvider).FullName }));
                }
                WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if (loader != null)
                {
                    loader.Flush();
                }
            }
            else
            {
                TypeProvider provider2 = new TypeProvider(null);
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    provider2.AddAssembly(assembly);
                }
                service = provider2;
            }
            RuleValidation validation = new RuleValidation(activity, service, false);
            this.ruleParser = new Parser(validation);
        }

        public RuleSetDialog(System.Type activityType, ITypeProvider typeProvider, System.Workflow.Activities.Rules.RuleSet ruleSet)
        {
            this.sortOrder = new bool[4];
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }
            this.InitializeDialog(ruleSet);
            RuleValidation validation = new RuleValidation(activityType, typeProvider);
            this.ruleParser = new Parser(validation);
        }

        private void activeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                tag.Active = this.activeCheckBox.Checked;
                this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
            }
        }

        private ListViewItem AddNewItem(Rule rule)
        {
            ListViewItem item = new ListViewItem(new string[] { rule.Name, string.Empty, string.Empty, string.Empty, string.Empty });
            this.rulesListView.Items.Add(item);
            item.Tag = rule;
            this.UpdateItem(item, rule);
            return item;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.conditionTextBox.Validating -= new CancelEventHandler(this.conditionTextBox_Validating);
            this.thenTextBox.Validating -= new CancelEventHandler(this.thenTextBox_Validating);
            this.elseTextBox.Validating -= new CancelEventHandler(this.elseTextBox_Validating);
        }

        private void chainingBehaviourComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dialogRuleSet.ChainingBehavior = (RuleChainingBehavior) this.chainingBehaviourComboBox.SelectedIndex;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void conditionTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count != 0)
            {
                try
                {
                    Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                    RuleCondition condition = this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                    tag.Condition = condition;
                    if (!string.IsNullOrEmpty(this.conditionTextBox.Text))
                    {
                        this.conditionTextBox.Text = condition.ToString().Replace("\n", "\r\n");
                    }
                    this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
                    this.conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                }
                catch (Exception exception)
                {
                    this.conditionErrorProvider.SetError(this.conditionTextBox, exception.Message);
                    System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_ConditionParser + "\n" + exception.Message, this.Text, this.serviceProvider);
                    e.Cancel = true;
                }
            }
        }

        private string CreateNewName()
        {
            string newRuleName = Messages.NewRuleName;
            int num = 1;
            while (true)
            {
                string name = newRuleName + num.ToString(CultureInfo.InvariantCulture);
                if (this.IsUniqueIdentifier(name))
                {
                    return name;
                }
                num++;
            }
        }

        private void deleteToolStripButton_Click(object sender, EventArgs e)
        {
            IntellisenseTextBox activeControl = base.ActiveControl as IntellisenseTextBox;
            if (activeControl != null)
            {
                activeControl.HideIntellisenceDropDown();
            }
            MessageBoxOptions options = 0;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            }
            if (MessageBox.Show(this, Messages.RuleConfirmDeleteMessageText, Messages.DeleteRule, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, options) == DialogResult.OK)
            {
                Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                int index = this.rulesListView.SelectedIndices[0];
                this.dialogRuleSet.Rules.Remove(tag);
                this.rulesListView.Items.RemoveAt(index);
                if (this.rulesListView.Items.Count > 0)
                {
                    int num2 = Math.Min(index, this.rulesListView.Items.Count - 1);
                    this.rulesListView.Items[num2].Selected = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void elseTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count != 0)
            {
                try
                {
                    Rule tag = (Rule) this.rulesListView.SelectedItems[0].Tag;
                    List<RuleAction> actions = this.ruleParser.ParseStatementList(this.elseTextBox.Text);
                    this.elseTextBox.Text = GetActionsString(actions);
                    tag.ElseActions.Clear();
                    foreach (RuleAction action in actions)
                    {
                        tag.ElseActions.Add(action);
                    }
                    this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
                    this.elseErrorProvider.SetError(this.elseTextBox, string.Empty);
                }
                catch (Exception exception)
                {
                    this.elseErrorProvider.SetError(this.elseTextBox, exception.Message);
                    System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_ActionsParser + "\n" + exception.Message, this.Text, this.serviceProvider);
                    e.Cancel = true;
                }
            }
        }

        private static string GetActionsString(IList<RuleAction> actions)
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }
            bool flag = true;
            StringBuilder builder = new StringBuilder();
            foreach (RuleAction action in actions)
            {
                if (!flag)
                {
                    builder.Append("\r\n");
                }
                else
                {
                    flag = false;
                }
                builder.Append(action.ToString());
            }
            return builder.ToString();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(RuleSetDialog));
            this.nameColumnHeader = new ColumnHeader();
            this.rulesListView = new ListView();
            this.priorityColumnHeader = new ColumnHeader();
            this.reevaluationCountColumnHeader = new ColumnHeader();
            this.activeColumnHeader = new ColumnHeader();
            this.rulePreviewColumnHeader = new ColumnHeader();
            this.rulesGroupBox = new GroupBox();
            this.panel1 = new Panel();
            this.chainingLabel = new Label();
            this.chainingBehaviourComboBox = new ComboBox();
            this.rulesToolStrip = new ToolStrip();
            this.imageList = new ImageList(this.components);
            this.newRuleToolStripButton = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.deleteToolStripButton = new ToolStripButton();
            this.buttonOK = new Button();
            this.ruleGroupBox = new GroupBox();
            this.reevaluationComboBox = new ComboBox();
            this.elseTextBox = new IntellisenseTextBox();
            this.elseLabel = new Label();
            this.thenTextBox = new IntellisenseTextBox();
            this.thenLabel = new Label();
            this.conditionTextBox = new IntellisenseTextBox();
            this.conditionLabel = new Label();
            this.nameTextBox = new TextBox();
            this.nameLabel = new Label();
            this.activeCheckBox = new CheckBox();
            this.reevaluationLabel = new Label();
            this.priorityTextBox = new TextBox();
            this.priorityLabel = new Label();
            this.buttonCancel = new Button();
            this.headerTextLabel = new Label();
            this.pictureBoxHeader = new PictureBox();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.conditionErrorProvider = new ErrorProvider(this.components);
            this.thenErrorProvider = new ErrorProvider(this.components);
            this.elseErrorProvider = new ErrorProvider(this.components);
            this.rulesGroupBox.SuspendLayout();
            this.panel1.SuspendLayout();
            this.rulesToolStrip.SuspendLayout();
            this.ruleGroupBox.SuspendLayout();
            ((ISupportInitialize) this.pictureBoxHeader).BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.conditionErrorProvider).BeginInit();
            ((ISupportInitialize) this.thenErrorProvider).BeginInit();
            ((ISupportInitialize) this.elseErrorProvider).BeginInit();
            base.SuspendLayout();
            this.nameColumnHeader.Name = "nameColumnHeader";
            manager.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
            this.rulesListView.Columns.AddRange(new ColumnHeader[] { this.nameColumnHeader, this.priorityColumnHeader, this.reevaluationCountColumnHeader, this.activeColumnHeader, this.rulePreviewColumnHeader });
            manager.ApplyResources(this.rulesListView, "rulesListView");
            this.rulesListView.FullRowSelect = true;
            this.rulesListView.HideSelection = false;
            this.rulesListView.MultiSelect = false;
            this.rulesListView.Name = "rulesListView";
            this.rulesListView.UseCompatibleStateImageBehavior = false;
            this.rulesListView.View = View.Details;
            this.rulesListView.SelectedIndexChanged += new EventHandler(this.rulesListView_SelectedIndexChanged);
            this.rulesListView.ColumnClick += new ColumnClickEventHandler(this.rulesListView_ColumnClick);
            manager.ApplyResources(this.priorityColumnHeader, "priorityColumnHeader");
            manager.ApplyResources(this.reevaluationCountColumnHeader, "reevaluationCountColumnHeader");
            manager.ApplyResources(this.activeColumnHeader, "activeColumnHeader");
            manager.ApplyResources(this.rulePreviewColumnHeader, "rulePreviewColumnHeader");
            this.rulesGroupBox.Controls.Add(this.panel1);
            manager.ApplyResources(this.rulesGroupBox, "rulesGroupBox");
            this.rulesGroupBox.Name = "rulesGroupBox";
            this.rulesGroupBox.TabStop = false;
            this.panel1.Controls.Add(this.chainingLabel);
            this.panel1.Controls.Add(this.chainingBehaviourComboBox);
            this.panel1.Controls.Add(this.rulesToolStrip);
            this.panel1.Controls.Add(this.rulesListView);
            manager.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            manager.ApplyResources(this.chainingLabel, "chainingLabel");
            this.chainingLabel.Name = "chainingLabel";
            this.chainingBehaviourComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.chainingBehaviourComboBox.FormattingEnabled = true;
            manager.ApplyResources(this.chainingBehaviourComboBox, "chainingBehaviourComboBox");
            this.chainingBehaviourComboBox.Name = "chainingBehaviourComboBox";
            this.chainingBehaviourComboBox.SelectedIndexChanged += new EventHandler(this.chainingBehaviourComboBox_SelectedIndexChanged);
            this.rulesToolStrip.BackColor = SystemColors.Control;
            this.rulesToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.rulesToolStrip.ImageList = this.imageList;
            this.rulesToolStrip.Items.AddRange(new ToolStripItem[] { this.newRuleToolStripButton, this.toolStripSeparator1, this.deleteToolStripButton });
            manager.ApplyResources(this.rulesToolStrip, "rulesToolStrip");
            this.rulesToolStrip.Name = "rulesToolStrip";
            this.rulesToolStrip.RenderMode = ToolStripRenderMode.System;
            this.rulesToolStrip.TabStop = true;
            this.imageList.ImageStream = (ImageListStreamer) manager.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.Transparent;
            this.imageList.Images.SetKeyName(0, "NewRule.bmp");
            this.imageList.Images.SetKeyName(1, "RenameRule.bmp");
            this.imageList.Images.SetKeyName(2, "Delete.bmp");
            manager.ApplyResources(this.newRuleToolStripButton, "newRuleToolStripButton");
            this.newRuleToolStripButton.Name = "newRuleToolStripButton";
            this.newRuleToolStripButton.Click += new EventHandler(this.newRuleToolStripButton_Click);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            manager.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            manager.ApplyResources(this.deleteToolStripButton, "deleteToolStripButton");
            this.deleteToolStripButton.Name = "deleteToolStripButton";
            this.deleteToolStripButton.Click += new EventHandler(this.deleteToolStripButton_Click);
            manager.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            this.ruleGroupBox.Controls.Add(this.reevaluationComboBox);
            this.ruleGroupBox.Controls.Add(this.elseTextBox);
            this.ruleGroupBox.Controls.Add(this.elseLabel);
            this.ruleGroupBox.Controls.Add(this.thenTextBox);
            this.ruleGroupBox.Controls.Add(this.thenLabel);
            this.ruleGroupBox.Controls.Add(this.conditionTextBox);
            this.ruleGroupBox.Controls.Add(this.conditionLabel);
            this.ruleGroupBox.Controls.Add(this.nameTextBox);
            this.ruleGroupBox.Controls.Add(this.nameLabel);
            this.ruleGroupBox.Controls.Add(this.activeCheckBox);
            this.ruleGroupBox.Controls.Add(this.reevaluationLabel);
            this.ruleGroupBox.Controls.Add(this.priorityTextBox);
            this.ruleGroupBox.Controls.Add(this.priorityLabel);
            manager.ApplyResources(this.ruleGroupBox, "ruleGroupBox");
            this.ruleGroupBox.Name = "ruleGroupBox";
            this.ruleGroupBox.TabStop = false;
            this.reevaluationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.reevaluationComboBox.FormattingEnabled = true;
            manager.ApplyResources(this.reevaluationComboBox, "reevaluationComboBox");
            this.reevaluationComboBox.Name = "reevaluationComboBox";
            this.reevaluationComboBox.SelectedIndexChanged += new EventHandler(this.reevaluationComboBox_SelectedIndexChanged);
            this.elseTextBox.AcceptsReturn = true;
            manager.ApplyResources(this.elseTextBox, "elseTextBox");
            this.elseTextBox.Name = "elseTextBox";
            this.elseTextBox.Validating += new CancelEventHandler(this.elseTextBox_Validating);
            manager.ApplyResources(this.elseLabel, "elseLabel");
            this.elseLabel.Name = "elseLabel";
            this.thenTextBox.AcceptsReturn = true;
            manager.ApplyResources(this.thenTextBox, "thenTextBox");
            this.thenTextBox.Name = "thenTextBox";
            this.thenTextBox.Validating += new CancelEventHandler(this.thenTextBox_Validating);
            manager.ApplyResources(this.thenLabel, "thenLabel");
            this.thenLabel.Name = "thenLabel";
            this.conditionTextBox.AcceptsReturn = true;
            manager.ApplyResources(this.conditionTextBox, "conditionTextBox");
            this.conditionTextBox.Name = "conditionTextBox";
            this.conditionTextBox.Validating += new CancelEventHandler(this.conditionTextBox_Validating);
            manager.ApplyResources(this.conditionLabel, "conditionLabel");
            this.conditionLabel.Name = "conditionLabel";
            manager.ApplyResources(this.nameTextBox, "nameTextBox");
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Validating += new CancelEventHandler(this.nameTextBox_Validating);
            manager.ApplyResources(this.nameLabel, "nameLabel");
            this.nameLabel.Name = "nameLabel";
            manager.ApplyResources(this.activeCheckBox, "activeCheckBox");
            this.activeCheckBox.Name = "activeCheckBox";
            this.activeCheckBox.CheckedChanged += new EventHandler(this.activeCheckBox_CheckedChanged);
            manager.ApplyResources(this.reevaluationLabel, "reevaluationLabel");
            this.reevaluationLabel.Name = "reevaluationLabel";
            manager.ApplyResources(this.priorityTextBox, "priorityTextBox");
            this.priorityTextBox.Name = "priorityTextBox";
            this.priorityTextBox.Validating += new CancelEventHandler(this.priorityTextBox_Validating);
            manager.ApplyResources(this.priorityLabel, "priorityLabel");
            this.priorityLabel.Name = "priorityLabel";
            manager.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
            manager.ApplyResources(this.headerTextLabel, "headerTextLabel");
            this.headerTextLabel.Name = "headerTextLabel";
            manager.ApplyResources(this.pictureBoxHeader, "pictureBoxHeader");
            this.pictureBoxHeader.Name = "pictureBoxHeader";
            this.pictureBoxHeader.TabStop = false;
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.CausesValidation = false;
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonOK, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonCancel, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.conditionErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.conditionErrorProvider.ContainerControl = this;
            this.thenErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.thenErrorProvider.ContainerControl = this;
            this.elseErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.elseErrorProvider.ContainerControl = this;
            base.AcceptButton = this.buttonOK;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.buttonCancel;
            base.Controls.Add(this.ruleGroupBox);
            base.Controls.Add(this.headerTextLabel);
            base.Controls.Add(this.pictureBoxHeader);
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.rulesGroupBox);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "RuleSetDialog";
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            this.rulesGroupBox.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.rulesToolStrip.ResumeLayout(false);
            this.rulesToolStrip.PerformLayout();
            this.ruleGroupBox.ResumeLayout(false);
            this.ruleGroupBox.PerformLayout();
            ((ISupportInitialize) this.pictureBoxHeader).EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.conditionErrorProvider).EndInit();
            ((ISupportInitialize) this.thenErrorProvider).EndInit();
            ((ISupportInitialize) this.elseErrorProvider).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeDialog(System.Workflow.Activities.Rules.RuleSet ruleSet)
        {
            this.InitializeComponent();
            this.conditionTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(this.PopulateAutoCompleteList);
            this.thenTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(this.PopulateAutoCompleteList);
            this.elseTextBox.PopulateAutoCompleteList += new EventHandler<AutoCompletionEventArgs>(this.PopulateAutoCompleteList);
            this.conditionTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(this.PopulateAutoCompleteList);
            this.thenTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(this.PopulateAutoCompleteList);
            this.elseTextBox.PopulateToolTipList += new EventHandler<AutoCompletionEventArgs>(this.PopulateAutoCompleteList);
            this.reevaluationComboBox.Items.Add(Messages.ReevaluationNever);
            this.reevaluationComboBox.Items.Add(Messages.ReevaluationAlways);
            this.chainingBehaviourComboBox.Items.Add(Messages.Sequential);
            this.chainingBehaviourComboBox.Items.Add(Messages.ExplicitUpdateOnly);
            this.chainingBehaviourComboBox.Items.Add(Messages.FullChaining);
            base.HelpRequested += new HelpEventHandler(this.OnHelpRequested);
            base.HelpButtonClicked += new CancelEventHandler(this.OnHelpClicked);
            if (ruleSet != null)
            {
                this.dialogRuleSet = ruleSet.Clone();
            }
            else
            {
                this.dialogRuleSet = new System.Workflow.Activities.Rules.RuleSet();
            }
            this.chainingBehaviourComboBox.SelectedIndex = (int) this.dialogRuleSet.ChainingBehavior;
            this.rulesListView.Select();
            foreach (Rule rule in this.dialogRuleSet.Rules)
            {
                this.AddNewItem(rule);
            }
            if (this.rulesListView.Items.Count > 0)
            {
                this.rulesListView.Items[0].Selected = true;
            }
            else
            {
                this.rulesListView_SelectedIndexChanged(this, new EventArgs());
            }
        }

        private bool IsUniqueIdentifier(string name)
        {
            foreach (Rule rule in this.dialogRuleSet.Rules)
            {
                if (rule.Name == name)
                {
                    return false;
                }
            }
            return true;
        }

        private void nameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                if (this.nameTextBox.Text != tag.Name)
                {
                    string text = this.nameTextBox.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        e.Cancel = true;
                        System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_RuleNameIsEmpty, this.Text, this.serviceProvider);
                    }
                    else if (tag.Name == text)
                    {
                        this.nameTextBox.Text = text;
                    }
                    else if (!this.IsUniqueIdentifier(text))
                    {
                        e.Cancel = true;
                        System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_DuplicateRuleName, this.Text, this.serviceProvider);
                    }
                    else
                    {
                        tag.Name = text;
                        this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
                    }
                }
            }
        }

        private void newRuleToolStripButton_Click(object sender, EventArgs e)
        {
            if (this.rulesToolStrip.Focus())
            {
                Rule rule = new Rule {
                    Name = this.CreateNewName()
                };
                this.dialogRuleSet.Rules.Add(rule);
                ListViewItem item = this.AddNewItem(rule);
                item.Selected = true;
                item.Focused = true;
                int index = this.rulesListView.Items.IndexOf(item);
                this.rulesListView.EnsureVisible(index);
            }
        }

        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowHelp();
        }

        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            this.ShowHelp();
        }

        private void PopulateAutoCompleteList(object sender, AutoCompletionEventArgs e)
        {
            e.AutoCompleteValues = this.ruleParser.GetExpressionCompletions(e.Prefix);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void priorityTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                try
                {
                    tag.Priority = int.Parse(this.priorityTextBox.Text, CultureInfo.CurrentCulture);
                    this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
                }
                catch
                {
                    e.Cancel = true;
                    System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_InvalidPriority, this.Text, this.serviceProvider);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.Equals(Keys.Escape))
            {
                this.conditionTextBox.Validating -= new CancelEventHandler(this.conditionTextBox_Validating);
                this.thenTextBox.Validating -= new CancelEventHandler(this.thenTextBox_Validating);
                this.elseTextBox.Validating -= new CancelEventHandler(this.elseTextBox_Validating);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void reevaluationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                tag.ReevaluationBehavior = (RuleReevaluationBehavior) this.reevaluationComboBox.SelectedIndex;
                this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
            }
        }

        private void rulesListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < 4)
            {
                this.rulesListView.ListViewItemSorter = new ListViewItemComparer(e.Column, this.sortOrder[e.Column]);
                this.sortOrder[e.Column] = !this.sortOrder[e.Column];
            }
        }

        private void rulesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                this.nameTextBox.Enabled = true;
                this.activeCheckBox.Enabled = true;
                this.reevaluationComboBox.Enabled = true;
                this.priorityTextBox.Enabled = true;
                this.conditionTextBox.Enabled = true;
                this.thenTextBox.Enabled = true;
                this.elseTextBox.Enabled = true;
                this.nameTextBox.Text = tag.Name;
                this.activeCheckBox.Checked = tag.Active;
                this.reevaluationComboBox.SelectedIndex = (int) tag.ReevaluationBehavior;
                this.priorityTextBox.Text = tag.Priority.ToString(CultureInfo.CurrentCulture);
                this.conditionTextBox.Text = (tag.Condition != null) ? tag.Condition.ToString().Replace("\n", "\r\n") : string.Empty;
                try
                {
                    this.ruleParser.ParseCondition(this.conditionTextBox.Text);
                    this.conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                }
                catch (RuleSyntaxException exception)
                {
                    this.conditionErrorProvider.SetError(this.conditionTextBox, exception.Message);
                }
                this.thenTextBox.Text = GetActionsString(tag.ThenActions);
                try
                {
                    this.ruleParser.ParseStatementList(this.thenTextBox.Text);
                    this.thenErrorProvider.SetError(this.thenTextBox, string.Empty);
                }
                catch (RuleSyntaxException exception2)
                {
                    this.thenErrorProvider.SetError(this.thenTextBox, exception2.Message);
                }
                this.elseTextBox.Text = GetActionsString(tag.ElseActions);
                try
                {
                    this.ruleParser.ParseStatementList(this.elseTextBox.Text);
                    this.elseErrorProvider.SetError(this.elseTextBox, string.Empty);
                }
                catch (RuleSyntaxException exception3)
                {
                    this.elseErrorProvider.SetError(this.elseTextBox, exception3.Message);
                }
                this.deleteToolStripButton.Enabled = true;
            }
            else
            {
                this.nameTextBox.Text = string.Empty;
                this.activeCheckBox.Checked = false;
                this.reevaluationComboBox.Text = string.Empty;
                this.priorityTextBox.Text = string.Empty;
                this.conditionTextBox.Text = string.Empty;
                this.thenTextBox.Text = string.Empty;
                this.elseTextBox.Text = string.Empty;
                this.nameTextBox.Enabled = false;
                this.activeCheckBox.Enabled = false;
                this.reevaluationComboBox.Enabled = false;
                this.priorityTextBox.Enabled = false;
                this.conditionTextBox.Enabled = false;
                this.thenTextBox.Enabled = false;
                this.elseTextBox.Enabled = false;
                this.conditionErrorProvider.SetError(this.conditionTextBox, string.Empty);
                this.thenErrorProvider.SetError(this.thenTextBox, string.Empty);
                this.elseErrorProvider.SetError(this.elseTextBox, string.Empty);
                this.deleteToolStripButton.Enabled = false;
            }
        }

        private static void SetCaretAt(TextBoxBase textBox, int position)
        {
            textBox.Focus();
            textBox.SelectionStart = position;
            textBox.SelectionLength = 0;
            textBox.ScrollToCaret();
        }

        private void ShowHelp()
        {
            if (this.serviceProvider != null)
            {
                IHelpService service = this.serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
                if (service != null)
                {
                    service.ShowHelpFromKeyword(base.GetType().FullName + ".UI");
                }
                else
                {
                    IUIService service2 = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    if (service2 != null)
                    {
                        service2.ShowError(Messages.NoHelp);
                    }
                }
            }
            else
            {
                IUIService service3 = (IUIService) this.GetService(typeof(IUIService));
                if (service3 != null)
                {
                    service3.ShowError(Messages.NoHelp);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void thenTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count != 0)
            {
                try
                {
                    Rule tag = this.rulesListView.SelectedItems[0].Tag as Rule;
                    List<RuleAction> actions = this.ruleParser.ParseStatementList(this.thenTextBox.Text);
                    this.thenTextBox.Text = GetActionsString(actions);
                    tag.ThenActions.Clear();
                    foreach (RuleAction action in actions)
                    {
                        tag.ThenActions.Add(action);
                    }
                    this.UpdateItem(this.rulesListView.SelectedItems[0], tag);
                    this.thenErrorProvider.SetError(this.thenTextBox, string.Empty);
                }
                catch (Exception exception)
                {
                    this.thenErrorProvider.SetError(this.thenTextBox, exception.Message);
                    System.Workflow.Activities.Rules.Design.DesignerHelpers.DisplayError(Messages.Error_ActionsParser + "\n" + exception.Message, this.Text, this.serviceProvider);
                    e.Cancel = true;
                }
            }
        }

        private void UpdateItem(ListViewItem listViewItem, Rule rule)
        {
            listViewItem.SubItems[0].Text = rule.Name;
            listViewItem.SubItems[1].Text = rule.Priority.ToString(CultureInfo.CurrentCulture);
            listViewItem.SubItems[2].Text = (string) this.reevaluationComboBox.Items[(int) rule.ReevaluationBehavior];
            listViewItem.SubItems[3].Text = rule.Active.ToString(CultureInfo.CurrentCulture);
            listViewItem.SubItems[4].Text = System.Workflow.Activities.Rules.Design.DesignerHelpers.GetRulePreview(rule);
        }

        public System.Workflow.Activities.Rules.RuleSet RuleSet
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dialogRuleSet;
            }
        }

        private class ListViewItemComparer : IComparer
        {
            private bool ascending;
            private int col;

            public ListViewItemComparer(int column, bool ascending)
            {
                this.col = column;
                this.ascending = ascending;
            }

            public int Compare(object x, object y)
            {
                int num = 0;
                ListViewItem item = (ListViewItem) x;
                ListViewItem item2 = (ListViewItem) y;
                if (this.col == 1)
                {
                    int result = 0;
                    int num3 = 0;
                    int.TryParse(item.SubItems[this.col].Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out result);
                    int.TryParse(item2.SubItems[this.col].Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out num3);
                    if (result != num3)
                    {
                        num = num3 - result;
                    }
                    else
                    {
                        num = string.Compare(item.SubItems[0].Text, item2.SubItems[0].Text, StringComparison.CurrentCulture);
                    }
                }
                else
                {
                    num = string.Compare(item.SubItems[this.col].Text, item2.SubItems[this.col].Text, StringComparison.CurrentCulture);
                }
                if (!this.ascending)
                {
                    return -num;
                }
                return num;
            }
        }
    }
}

