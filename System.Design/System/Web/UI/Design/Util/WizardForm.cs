namespace System.Web.UI.Design.Util
{
    using System;
    using System.Collections.Generic;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class WizardForm : TaskFormBase
    {
        private Button _cancelButton;
        private Label _dummyLabel1;
        private Label _dummyLabel2;
        private Label _dummyLabel3;
        private Button _finishButton;
        private WizardPanel _initialPanel;
        private Button _nextButton;
        private Stack<WizardPanel> _panelHistory;
        private Button _previousButton;
        private TableLayoutPanel _wizardButtonsTableLayoutPanel;

        public WizardForm(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._panelHistory = new Stack<WizardPanel>();
            this.InitializeComponent();
            this.InitializeUI();
        }

        private void InitializeComponent()
        {
            this._wizardButtonsTableLayoutPanel = new TableLayoutPanel();
            this._previousButton = new Button();
            this._nextButton = new Button();
            this._dummyLabel2 = new Label();
            this._finishButton = new Button();
            this._dummyLabel3 = new Label();
            this._cancelButton = new Button();
            this._dummyLabel1 = new Label();
            this._wizardButtonsTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            this._wizardButtonsTableLayoutPanel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._wizardButtonsTableLayoutPanel.AutoSize = true;
            this._wizardButtonsTableLayoutPanel.ColumnCount = 7;
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 3f));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 7f));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 7f));
            this._wizardButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._previousButton);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._dummyLabel1);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._nextButton);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._dummyLabel2);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._finishButton);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._dummyLabel3);
            this._wizardButtonsTableLayoutPanel.Controls.Add(this._cancelButton);
            this._wizardButtonsTableLayoutPanel.Location = new Point(0xef, 0x180);
            this._wizardButtonsTableLayoutPanel.Name = "_wizardButtonsTableLayoutPanel";
            this._wizardButtonsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._wizardButtonsTableLayoutPanel.Size = new Size(0x13d, 0x17);
            this._wizardButtonsTableLayoutPanel.TabIndex = 100;
            this._previousButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._previousButton.AutoSize = true;
            this._previousButton.Enabled = false;
            this._previousButton.Location = new Point(0, 0);
            this._previousButton.Margin = new Padding(0);
            this._previousButton.MinimumSize = new Size(0x4b, 0x17);
            this._previousButton.Name = "_previousButton";
            this._previousButton.TabIndex = 10;
            this._previousButton.Click += new EventHandler(this.OnPreviousButtonClick);
            this._dummyLabel1.Location = new Point(0x4b, 0);
            this._dummyLabel1.Margin = new Padding(0);
            this._dummyLabel1.Name = "_dummyLabel1";
            this._dummyLabel1.Size = new Size(3, 0);
            this._dummyLabel1.TabIndex = 20;
            this._nextButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._nextButton.AutoSize = true;
            this._nextButton.Location = new Point(0x4e, 0);
            this._nextButton.Margin = new Padding(0);
            this._nextButton.MinimumSize = new Size(0x4b, 0x17);
            this._nextButton.Name = "_nextButton";
            this._nextButton.TabIndex = 30;
            this._nextButton.Click += new EventHandler(this.OnNextButtonClick);
            this._dummyLabel2.Location = new Point(0x99, 0);
            this._dummyLabel2.Margin = new Padding(0);
            this._dummyLabel2.Name = "_dummyLabel2";
            this._dummyLabel2.Size = new Size(7, 0);
            this._dummyLabel2.TabIndex = 40;
            this._finishButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._finishButton.AutoSize = true;
            this._finishButton.Enabled = false;
            this._finishButton.Location = new Point(160, 0);
            this._finishButton.Margin = new Padding(0);
            this._finishButton.MinimumSize = new Size(0x4b, 0x17);
            this._finishButton.Name = "_finishButton";
            this._finishButton.TabIndex = 50;
            this._finishButton.Click += new EventHandler(this.OnFinishButtonClick);
            this._dummyLabel3.Location = new Point(0xeb, 0);
            this._dummyLabel3.Margin = new Padding(0);
            this._dummyLabel3.Name = "_dummyLabel3";
            this._dummyLabel3.Size = new Size(7, 0);
            this._dummyLabel3.TabIndex = 60;
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._cancelButton.AutoSize = true;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0xf2, 0);
            this._cancelButton.Margin = new Padding(0);
            this._cancelButton.MinimumSize = new Size(0x4b, 0x17);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 70;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._nextButton;
            base.CancelButton = this._cancelButton;
            base.Controls.Add(this._wizardButtonsTableLayoutPanel);
            this.MinimumSize = new Size(580, 450);
            base.SizeGripStyle = SizeGripStyle.Show;
            this._wizardButtonsTableLayoutPanel.ResumeLayout(false);
            this._wizardButtonsTableLayoutPanel.PerformLayout();
            base.InitializeForm();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._cancelButton.Text = System.Design.SR.GetString("Wizard_CancelButton");
            this._nextButton.Text = System.Design.SR.GetString("Wizard_NextButton");
            this._previousButton.Text = System.Design.SR.GetString("Wizard_PreviousButton");
            this._finishButton.Text = System.Design.SR.GetString("Wizard_FinishButton");
        }

        public void NextPanel()
        {
            WizardPanel currentPanel = this._panelHistory.Peek();
            if (currentPanel.OnNext())
            {
                currentPanel.Hide();
                WizardPanel nextPanel = currentPanel.NextPanel;
                if (nextPanel != null)
                {
                    this.RegisterPanel(nextPanel);
                    this._panelHistory.Push(nextPanel);
                    this.OnPanelChanging(new WizardPanelChangingEventArgs(currentPanel));
                    this.ShowPanel(nextPanel);
                }
            }
        }

        protected virtual void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected virtual void OnFinishButtonClick(object sender, EventArgs e)
        {
            if (this._panelHistory.Peek().OnNext())
            {
                WizardPanel[] array = this._panelHistory.ToArray();
                Array.Reverse(array);
                foreach (WizardPanel panel2 in array)
                {
                    panel2.OnComplete();
                }
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        protected override void OnInitialActivated(EventArgs e)
        {
            base.OnInitialActivated(e);
            if (this._initialPanel != null)
            {
                this.RegisterPanel(this._initialPanel);
                this._panelHistory.Push(this._initialPanel);
                this.ShowPanel(this._initialPanel);
            }
        }

        protected virtual void OnNextButtonClick(object sender, EventArgs e)
        {
            this.NextPanel();
        }

        protected virtual void OnPanelChanging(WizardPanelChangingEventArgs e)
        {
        }

        protected virtual void OnPreviousButtonClick(object sender, EventArgs e)
        {
            this.PreviousPanel();
        }

        public void PreviousPanel()
        {
            if (this._panelHistory.Count > 1)
            {
                WizardPanel currentPanel = this._panelHistory.Pop();
                WizardPanel panel = this._panelHistory.Peek();
                currentPanel.OnPrevious();
                currentPanel.Hide();
                this.OnPanelChanging(new WizardPanelChangingEventArgs(currentPanel));
                this.ShowPanel(panel);
            }
        }

        internal void RegisterPanel(WizardPanel panel)
        {
            if (!base.TaskPanel.Controls.Contains(panel))
            {
                panel.Dock = DockStyle.Fill;
                panel.SetParentWizard(this);
                panel.Hide();
                base.TaskPanel.Controls.Add(panel);
            }
        }

        protected void SetPanels(WizardPanel[] panels)
        {
            if ((panels != null) && (panels.Length > 0))
            {
                this.RegisterPanel(panels[0]);
                this._initialPanel = panels[0];
                for (int i = 0; i < (panels.Length - 1); i++)
                {
                    this.RegisterPanel(panels[i + 1]);
                    panels[i].NextPanel = panels[i + 1];
                }
            }
        }

        private void ShowPanel(WizardPanel panel)
        {
            if (this._panelHistory.Count == 1)
            {
                this.PreviousButton.Enabled = false;
            }
            else
            {
                this.PreviousButton.Enabled = true;
            }
            if (panel.NextPanel == null)
            {
                this.NextButton.Enabled = false;
            }
            else
            {
                this.NextButton.Enabled = true;
            }
            panel.Show();
            base.AccessibleDescription = panel.Caption;
            base.CaptionLabel.Text = panel.Caption;
            if (base.IsHandleCreated)
            {
                base.Invalidate();
            }
            panel.Focus();
        }

        public Button FinishButton
        {
            get
            {
                return this._finishButton;
            }
        }

        public Button NextButton
        {
            get
            {
                return this._nextButton;
            }
        }

        public Button PreviousButton
        {
            get
            {
                return this._previousButton;
            }
        }
    }
}

