namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal class MaskDesignerDialog : Form
    {
        private Button btnCancel;
        private Button btnOK;
        private CheckBox checkBoxUseValidatingType;
        private IContainer components;
        private MaskDescriptor customMaskDescriptor;
        private ColumnHeader dataFormatHeader;
        private ErrorProvider errorProvider;
        private IHelpService helpService;
        private Label lblHeader;
        private Label lblMask;
        private Label lblTryIt;
        private ListView listViewCannedMasks;
        private SortOrder listViewSortOrder = SortOrder.Ascending;
        private ColumnHeader maskDescriptionHeader;
        private List<MaskDescriptor> maskDescriptors = new List<MaskDescriptor>();
        private MaskedTextBox maskedTextBox;
        private TableLayoutPanel maskTryItTable;
        private System.Type mtpValidatingType;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private TextBox txtBoxMask;
        private ColumnHeader validatingTypeHeader;

        public MaskDesignerDialog(MaskedTextBox instance, IHelpService helpService)
        {
            if (instance == null)
            {
                this.maskedTextBox = new MaskedTextBox();
            }
            else
            {
                this.maskedTextBox = MaskedTextBoxDesigner.GetDesignMaskedTextBox(instance);
            }
            this.helpService = helpService;
            this.InitializeComponent();
            DesignerUtils.ApplyListViewThemeStyles(this.listViewCannedMasks);
            base.SuspendLayout();
            this.txtBoxMask.Text = this.maskedTextBox.Mask;
            this.AddDefaultMaskDescriptors(this.maskedTextBox.Culture);
            this.maskDescriptionHeader.Text = System.Design.SR.GetString("MaskDesignerDialogMaskDescription");
            this.maskDescriptionHeader.Width = this.listViewCannedMasks.Width / 3;
            this.dataFormatHeader.Text = System.Design.SR.GetString("MaskDesignerDialogDataFormat");
            this.dataFormatHeader.Width = this.listViewCannedMasks.Width / 3;
            this.validatingTypeHeader.Text = System.Design.SR.GetString("MaskDesignerDialogValidatingType");
            this.validatingTypeHeader.Width = ((this.listViewCannedMasks.Width / 3) - SystemInformation.VerticalScrollBarWidth) - 4;
            base.ResumeLayout(false);
            this.HookEvents();
        }

        private void AddDefaultMaskDescriptors(CultureInfo culture)
        {
            this.customMaskDescriptor = new MaskDescriptorTemplate(null, System.Design.SR.GetString("MaskDesignerDialogCustomEntry"), null, null, null, true);
            List<MaskDescriptor> localizedMaskDescriptors = MaskDescriptorTemplate.GetLocalizedMaskDescriptors(culture);
            this.InsertMaskDescriptor(0, this.customMaskDescriptor, false);
            foreach (MaskDescriptor descriptor in localizedMaskDescriptors)
            {
                this.InsertMaskDescriptor(0, descriptor);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.checkBoxUseValidatingType.Checked)
            {
                this.mtpValidatingType = this.maskedTextBox.ValidatingType;
            }
            else
            {
                this.mtpValidatingType = null;
            }
        }

        private bool ContainsMaskDescriptor(MaskDescriptor maskDescriptor)
        {
            foreach (MaskDescriptor descriptor in this.maskDescriptors)
            {
                if (maskDescriptor.Equals(descriptor) || (maskDescriptor.Name.Trim() == descriptor.Name.Trim()))
                {
                    return true;
                }
            }
            return false;
        }

        public void DiscoverMaskDescriptors(ITypeDiscoveryService discoveryService)
        {
            if (discoveryService != null)
            {
                foreach (System.Type type in DesignerUtils.FilterGenericTypes(discoveryService.GetTypes(typeof(MaskDescriptor), false)))
                {
                    if (!type.IsAbstract && type.IsPublic)
                    {
                        try
                        {
                            MaskDescriptor maskDescriptor = (MaskDescriptor) Activator.CreateInstance(type);
                            this.InsertMaskDescriptor(0, maskDescriptor);
                        }
                        catch (Exception exception)
                        {
                            if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        private int GetMaskDescriptorIndex(MaskDescriptor maskDescriptor)
        {
            for (int i = 0; i < this.maskDescriptors.Count; i++)
            {
                MaskDescriptor descriptor = this.maskDescriptors[i];
                if (descriptor == maskDescriptor)
                {
                    return i;
                }
            }
            return -1;
        }

        private void HookEvents()
        {
            this.listViewCannedMasks.SelectedIndexChanged += new EventHandler(this.listViewCannedMasks_SelectedIndexChanged);
            this.listViewCannedMasks.ColumnClick += new ColumnClickEventHandler(this.listViewCannedMasks_ColumnClick);
            this.listViewCannedMasks.Enter += new EventHandler(this.listViewCannedMasks_Enter);
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.txtBoxMask.TextChanged += new EventHandler(this.txtBoxMask_TextChanged);
            this.txtBoxMask.Validating += new CancelEventHandler(this.txtBoxMask_Validating);
            this.maskedTextBox.KeyDown += new KeyEventHandler(this.maskedTextBox_KeyDown);
            this.maskedTextBox.MaskInputRejected += new MaskInputRejectedEventHandler(this.maskedTextBox_MaskInputRejected);
            base.Load += new EventHandler(this.MaskDesignerDialog_Load);
            base.HelpButtonClicked += new CancelEventHandler(this.MaskDesignerDialog_HelpButtonClicked);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MaskDesignerDialog));
            this.lblHeader = new Label();
            this.listViewCannedMasks = new ListView();
            this.maskDescriptionHeader = new ColumnHeader(manager.GetString("listViewCannedMasks.Columns"));
            this.dataFormatHeader = new ColumnHeader(manager.GetString("listViewCannedMasks.Columns1"));
            this.validatingTypeHeader = new ColumnHeader(manager.GetString("listViewCannedMasks.Columns2"));
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.checkBoxUseValidatingType = new CheckBox();
            this.maskTryItTable = new TableLayoutPanel();
            this.lblMask = new Label();
            this.txtBoxMask = new TextBox();
            this.lblTryIt = new Label();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.errorProvider = new ErrorProvider(this.components);
            this.maskTryItTable.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            this.okCancelTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.errorProvider).BeginInit();
            base.SuspendLayout();
            manager.ApplyResources(this.maskedTextBox, "maskedTextBox");
            this.maskedTextBox.Margin = new Padding(3, 3, 0x12, 0);
            this.maskedTextBox.Name = "maskedTextBox";
            manager.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.Margin = new Padding(0, 0, 0, 3);
            this.lblHeader.Name = "lblHeader";
            manager.ApplyResources(this.listViewCannedMasks, "listViewCannedMasks");
            this.listViewCannedMasks.Columns.AddRange(new ColumnHeader[] { this.maskDescriptionHeader, this.dataFormatHeader, this.validatingTypeHeader });
            this.listViewCannedMasks.FullRowSelect = true;
            this.listViewCannedMasks.HideSelection = false;
            this.listViewCannedMasks.Margin = new Padding(0, 3, 0, 3);
            this.listViewCannedMasks.MultiSelect = false;
            this.listViewCannedMasks.Name = "listViewCannedMasks";
            this.listViewCannedMasks.Sorting = SortOrder.None;
            this.listViewCannedMasks.View = View.Details;
            manager.ApplyResources(this.maskDescriptionHeader, "maskDescriptionHeader");
            manager.ApplyResources(this.dataFormatHeader, "dataFormatHeader");
            manager.ApplyResources(this.validatingTypeHeader, "validatingTypeHeader");
            manager.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Margin = new Padding(0, 0, 3, 0);
            this.btnOK.MinimumSize = new Size(0x4b, 0x17);
            this.btnOK.Name = "btnOK";
            this.btnOK.Padding = new Padding(10, 0, 10, 0);
            manager.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Margin = new Padding(3, 0, 0, 0);
            this.btnCancel.MinimumSize = new Size(0x4b, 0x17);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new Padding(10, 0, 10, 0);
            manager.ApplyResources(this.checkBoxUseValidatingType, "checkBoxUseValidatingType");
            this.checkBoxUseValidatingType.Checked = true;
            this.checkBoxUseValidatingType.CheckState = CheckState.Checked;
            this.checkBoxUseValidatingType.Margin = new Padding(0, 0, 0, 3);
            this.checkBoxUseValidatingType.Name = "checkBoxUseValidatingType";
            manager.ApplyResources(this.maskTryItTable, "maskTryItTable");
            this.maskTryItTable.ColumnStyles.Add(new ColumnStyle());
            this.maskTryItTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.maskTryItTable.ColumnStyles.Add(new ColumnStyle());
            this.maskTryItTable.Controls.Add(this.checkBoxUseValidatingType, 2, 0);
            this.maskTryItTable.Controls.Add(this.lblMask, 0, 0);
            this.maskTryItTable.Controls.Add(this.txtBoxMask, 1, 0);
            this.maskTryItTable.Controls.Add(this.lblTryIt, 0, 1);
            this.maskTryItTable.Controls.Add(this.maskedTextBox, 1, 1);
            this.maskTryItTable.Margin = new Padding(0, 3, 0, 3);
            this.maskTryItTable.Name = "maskTryItTable";
            this.maskTryItTable.RowStyles.Add(new RowStyle());
            this.maskTryItTable.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.lblMask, "lblMask");
            this.lblMask.Margin = new Padding(0, 0, 3, 3);
            this.lblMask.Name = "lblMask";
            manager.ApplyResources(this.txtBoxMask, "txtBoxMask");
            this.txtBoxMask.Margin = new Padding(3, 0, 0x12, 3);
            this.txtBoxMask.Name = "txtBoxMask";
            manager.ApplyResources(this.lblTryIt, "lblTryIt");
            this.lblTryIt.Margin = new Padding(0, 3, 3, 0);
            this.lblTryIt.Name = "lblTryIt";
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.overarchingTableLayoutPanel.Controls.Add(this.maskTryItTable, 0, 3);
            this.overarchingTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 4);
            this.overarchingTableLayoutPanel.Controls.Add(this.lblHeader, 0, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.listViewCannedMasks, 0, 2);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.Controls.Add(this.btnCancel, 1, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.btnOK, 0, 0);
            this.okCancelTableLayoutPanel.Margin = new Padding(0, 6, 0, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            manager.ApplyResources(this, "$this");
            base.AcceptButton = this.btnOK;
            base.CancelButton = this.btnCancel;
            base.AutoScaleMode = AutoScaleMode.Font;
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "MaskDesignerDialog";
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            this.maskTryItTable.ResumeLayout(false);
            this.maskTryItTable.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.errorProvider).EndInit();
            base.ResumeLayout(false);
        }

        private void InsertMaskDescriptor(int index, MaskDescriptor maskDescriptor)
        {
            this.InsertMaskDescriptor(index, maskDescriptor, true);
        }

        private void InsertMaskDescriptor(int index, MaskDescriptor maskDescriptor, bool validateDescriptor)
        {
            string str;
            if ((!validateDescriptor || MaskDescriptor.IsValidMaskDescriptor(maskDescriptor, out str)) && !this.ContainsMaskDescriptor(maskDescriptor))
            {
                this.maskDescriptors.Insert(index, maskDescriptor);
            }
        }

        private void listViewCannedMasks_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (this.listViewSortOrder)
            {
                case SortOrder.None:
                case SortOrder.Descending:
                    this.listViewSortOrder = SortOrder.Ascending;
                    break;

                case SortOrder.Ascending:
                    this.listViewSortOrder = SortOrder.Descending;
                    break;
            }
            this.UpdateSortedListView((MaskDescriptorComparer.SortType) e.Column);
        }

        private void listViewCannedMasks_Enter(object sender, EventArgs e)
        {
            if ((this.listViewCannedMasks.FocusedItem == null) && (this.listViewCannedMasks.Items.Count > 0))
            {
                this.listViewCannedMasks.Items[0].Focused = true;
            }
        }

        private void listViewCannedMasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listViewCannedMasks.SelectedItems.Count != 0)
            {
                int num = this.listViewCannedMasks.SelectedIndices[0];
                MaskDescriptor descriptor = this.maskDescriptors[num];
                if (descriptor != this.customMaskDescriptor)
                {
                    this.txtBoxMask.Text = descriptor.Mask;
                    this.maskedTextBox.Mask = descriptor.Mask;
                    this.maskedTextBox.ValidatingType = descriptor.ValidatingType;
                }
                else
                {
                    this.maskedTextBox.ValidatingType = null;
                }
            }
        }

        private void MaskDesignerDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowHelp();
        }

        private void MaskDesignerDialog_Load(object sender, EventArgs e)
        {
            this.UpdateSortedListView(MaskDescriptorComparer.SortType.ByName);
            this.SelectMtbMaskDescriptor();
            this.btnCancel.Select();
        }

        private void maskedTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            this.errorProvider.Clear();
        }

        private void maskedTextBox_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            this.errorProvider.SetError(this.maskedTextBox, MaskedTextBoxDesigner.GetMaskInputRejectedErrorMessage(e));
        }

        private void RemoveMaskDescriptor(MaskDescriptor maskDescriptor)
        {
            int maskDescriptorIndex = this.GetMaskDescriptorIndex(maskDescriptor);
            if (maskDescriptorIndex >= 0)
            {
                this.maskDescriptors.RemoveAt(maskDescriptorIndex);
            }
        }

        private void SelectMtbMaskDescriptor()
        {
            int maskDexIndex = -1;
            if (!string.IsNullOrEmpty(this.maskedTextBox.Mask))
            {
                for (int i = 0; i < this.maskDescriptors.Count; i++)
                {
                    MaskDescriptor descriptor = this.maskDescriptors[i];
                    if ((descriptor.Mask == this.maskedTextBox.Mask) && (descriptor.ValidatingType == this.maskedTextBox.ValidatingType))
                    {
                        maskDexIndex = i;
                        break;
                    }
                }
            }
            if (maskDexIndex == -1)
            {
                maskDexIndex = this.GetMaskDescriptorIndex(this.customMaskDescriptor);
            }
            if (maskDexIndex != -1)
            {
                this.SetSelectedMaskDescriptor(maskDexIndex);
            }
        }

        private void SetSelectedMaskDescriptor(int maskDexIndex)
        {
            if ((maskDexIndex >= 0) && (this.listViewCannedMasks.Items.Count > maskDexIndex))
            {
                this.listViewCannedMasks.Items[maskDexIndex].Selected = true;
                this.listViewCannedMasks.FocusedItem = this.listViewCannedMasks.Items[maskDexIndex];
                this.listViewCannedMasks.EnsureVisible(maskDexIndex);
            }
        }

        private void SetSelectedMaskDescriptor(MaskDescriptor maskDex)
        {
            int maskDescriptorIndex = this.GetMaskDescriptorIndex(maskDex);
            this.SetSelectedMaskDescriptor(maskDescriptorIndex);
        }

        private void ShowHelp()
        {
            if (this.helpService != null)
            {
                this.helpService.ShowHelpFromKeyword(this.HelpTopic);
            }
        }

        private void txtBoxMask_TextChanged(object sender, EventArgs e)
        {
            MaskDescriptor descriptor = null;
            if (this.listViewCannedMasks.SelectedItems.Count != 0)
            {
                int num = this.listViewCannedMasks.SelectedIndices[0];
                descriptor = this.maskDescriptors[num];
            }
            if ((descriptor == null) || ((descriptor != this.customMaskDescriptor) && (descriptor.Mask != this.txtBoxMask.Text)))
            {
                this.SetSelectedMaskDescriptor(this.customMaskDescriptor);
            }
        }

        private void txtBoxMask_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                this.maskedTextBox.Mask = this.txtBoxMask.Text;
            }
            catch (ArgumentException)
            {
            }
        }

        private void UpdateSortedListView(MaskDescriptorComparer.SortType sortType)
        {
            if (this.listViewCannedMasks.IsHandleCreated)
            {
                MaskDescriptor maskDex = null;
                if (this.listViewCannedMasks.SelectedItems.Count > 0)
                {
                    int num = this.listViewCannedMasks.SelectedIndices[0];
                    maskDex = this.maskDescriptors[num];
                }
                this.maskDescriptors.RemoveAt(this.maskDescriptors.Count - 1);
                this.maskDescriptors.Sort(new MaskDescriptorComparer(sortType, this.listViewSortOrder));
                System.Design.UnsafeNativeMethods.SendMessage(this.listViewCannedMasks.Handle, 11, false, 0);
                try
                {
                    this.listViewCannedMasks.Items.Clear();
                    string str = System.Design.SR.GetString("MaskDescriptorValidatingTypeNone");
                    foreach (MaskDescriptor descriptor2 in this.maskDescriptors)
                    {
                        string str2 = (descriptor2.ValidatingType != null) ? descriptor2.ValidatingType.Name : str;
                        MaskedTextProvider provider = new MaskedTextProvider(descriptor2.Mask, descriptor2.Culture);
                        provider.Add(descriptor2.Sample);
                        string str3 = provider.ToString(false, true);
                        this.listViewCannedMasks.Items.Add(new ListViewItem(new string[] { descriptor2.Name, str3, str2 }));
                    }
                    this.maskDescriptors.Add(this.customMaskDescriptor);
                    this.listViewCannedMasks.Items.Add(new ListViewItem(new string[] { this.customMaskDescriptor.Name, "", str }));
                    if (maskDex != null)
                    {
                        this.SetSelectedMaskDescriptor(maskDex);
                    }
                }
                finally
                {
                    System.Design.UnsafeNativeMethods.SendMessage(this.listViewCannedMasks.Handle, 11, true, 0);
                    this.listViewCannedMasks.Invalidate();
                }
            }
        }

        private string HelpTopic
        {
            get
            {
                return "net.ComponentModel.MaskPropertyEditor";
            }
        }

        public string Mask
        {
            get
            {
                return this.maskedTextBox.Mask;
            }
        }

        public IEnumerator MaskDescriptors
        {
            get
            {
                return this.maskDescriptors.GetEnumerator();
            }
        }

        public System.Type ValidatingType
        {
            get
            {
                return this.mtpValidatingType;
            }
        }
    }
}

