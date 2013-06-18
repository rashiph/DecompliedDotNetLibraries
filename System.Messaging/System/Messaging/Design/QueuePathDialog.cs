namespace System.Messaging.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Messaging;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public class QueuePathDialog : Form
    {
        private Button cancelButton;
        private bool closed;
        private TreeView enterprise;
        private bool exit;
        private static readonly string HELP_KEYWORD = "System.Messaging.Design.QueuePathDialog";
        private Button helpButton;
        private ImageList icons;
        private int lastPathType;
        private Hashtable machinesTable;
        private Button okButton;
        private string path;
        private ComboBox pathType;
        private Thread populateThread;
        private bool populateThreadRan;
        private static readonly string PREFIX_FORMAT_NAME = "FORMATNAME:";
        private static readonly string PREFIX_LABEL = "LABEL:";
        private IServiceProvider provider;
        private string queuePath;
        private Label referenceLabel;
        private MessageQueue selectedQueue;
        private Label selectLabel;
        private IUIService uiService;

        public QueuePathDialog(IServiceProvider provider)
        {
            this.path = string.Empty;
            this.queuePath = string.Empty;
            this.machinesTable = new Hashtable();
            this.uiService = (IUIService) provider.GetService(typeof(IUIService));
            this.provider = provider;
            this.InitializeComponent();
        }

        public QueuePathDialog(IUIService uiService)
        {
            this.path = string.Empty;
            this.queuePath = string.Empty;
            this.machinesTable = new Hashtable();
            this.uiService = uiService;
            this.InitializeComponent();
        }

        private void AfterSelect(object source, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            string[] strArray = node.FullPath.Split(new char[] { '\\' });
            if (strArray.Length == 2)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(node.Parent.Text);
                builder.Append(@"\");
                builder.Append(strArray[1]);
                this.path = builder.ToString();
                this.ChoosePath();
                this.exit = true;
            }
        }

        private void BeforeSelect(object source, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;
            string[] strArray = node.FullPath.Split(new char[] { '\\' });
            node.SelectedImageIndex = strArray.Length - 1;
            this.exit = false;
        }

        public void ChoosePath()
        {
            if ((this.path != null) && (this.path != string.Empty))
            {
                if (this.pathType.Text.CompareTo(Res.GetString("RefByPath")) == 0)
                {
                    this.queuePath = this.path;
                    this.lastPathType = this.pathType.SelectedIndex;
                }
                else if (this.pathType.Text.CompareTo(Res.GetString("RefByFormatName")) == 0)
                {
                    MessageQueue queue = new MessageQueue(this.path);
                    this.queuePath = PREFIX_FORMAT_NAME + queue.FormatName;
                    this.lastPathType = this.pathType.SelectedIndex;
                }
                else
                {
                    MessageQueue queue2 = new MessageQueue(this.path);
                    string path = PREFIX_LABEL + queue2.Label;
                    try
                    {
                        MessageQueue queue3 = new MessageQueue(path);
                        string formatName = queue3.FormatName;
                        this.queuePath = path;
                        this.lastPathType = this.pathType.SelectedIndex;
                    }
                    catch (Exception exception)
                    {
                        if ((this.queuePath != null) && (string.Compare(this.queuePath, path, true, CultureInfo.InvariantCulture) != 0))
                        {
                            this.exit = false;
                            if (this.uiService != null)
                            {
                                this.uiService.ShowError(exception.Message);
                            }
                            else
                            {
                                MessageBox.Show(exception.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                            if (this.queuePath == string.Empty)
                            {
                                this.queuePath = this.path;
                                this.lastPathType = 0;
                            }
                            this.OnSelectQueue(new MessageQueue(this.queuePath), this.lastPathType);
                        }
                    }
                }
            }
        }

        public void DoubleClicked(object source, EventArgs e)
        {
            if (this.exit)
            {
                base.Close();
                base.DialogResult = DialogResult.OK;
            }
        }

        private void IndexChanged(object source, EventArgs e)
        {
            this.ChoosePath();
        }

        private void InitializeComponent()
        {
            ResourceManager manager = new ResourceManager(typeof(QueuePathDialog));
            this.icons = new ImageList();
            this.okButton = new Button();
            this.pathType = new ComboBox();
            this.enterprise = new TreeView();
            this.helpButton = new Button();
            this.selectLabel = new Label();
            this.referenceLabel = new Label();
            this.cancelButton = new Button();
            this.okButton.Location = (Point) manager.GetObject("okButton.Location");
            this.okButton.Size = (Size) manager.GetObject("okButton.Size");
            this.okButton.TabIndex = (int) manager.GetObject("okButton.TabIndex");
            this.okButton.Text = manager.GetString("okButton.Text");
            this.okButton.DialogResult = DialogResult.OK;
            this.pathType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.pathType.DropDownWidth = 0x108;
            this.pathType.Items.Add(Res.GetString("RefByPath"));
            this.pathType.Items.Add(Res.GetString("RefByFormatName"));
            this.pathType.Items.Add(Res.GetString("RefByLabel"));
            this.pathType.SelectedIndex = 0;
            this.pathType.Location = (Point) manager.GetObject("pathType.Location");
            this.pathType.Size = (Size) manager.GetObject("pathType.Size");
            this.pathType.TabIndex = (int) manager.GetObject("pathType.TabIndex");
            this.pathType.SelectedIndexChanged += new EventHandler(this.IndexChanged);
            this.enterprise.HideSelection = false;
            this.enterprise.ImageIndex = -1;
            this.enterprise.Location = (Point) manager.GetObject("enterprise.Location");
            this.enterprise.Nodes.AddRange(new TreeNode[] { new TreeNode(Res.GetString("PleaseWait")) });
            this.enterprise.SelectedImageIndex = -1;
            this.enterprise.Size = (Size) manager.GetObject("enterprise.Size");
            this.enterprise.Sorted = true;
            this.enterprise.TabIndex = (int) manager.GetObject("enterprise.TabIndex");
            this.enterprise.AfterSelect += new TreeViewEventHandler(this.AfterSelect);
            this.enterprise.BeforeSelect += new TreeViewCancelEventHandler(this.BeforeSelect);
            this.enterprise.DoubleClick += new EventHandler(this.DoubleClicked);
            this.enterprise.ImageList = this.icons;
            this.helpButton.Location = (Point) manager.GetObject("helpButton.Location");
            this.helpButton.Size = (Size) manager.GetObject("helpButton.Size");
            this.helpButton.TabIndex = (int) manager.GetObject("helpButton.TabIndex");
            this.helpButton.Text = manager.GetString("helpButton.Text");
            this.helpButton.Click += new EventHandler(this.OnClickHelpButton);
            this.icons.Images.Add(new Bitmap(typeof(MessageQueue), "Machine.bmp"));
            this.icons.Images.Add(new Bitmap(typeof(MessageQueue), "PublicQueue.bmp"));
            this.selectLabel.Location = (Point) manager.GetObject("selectLabel.Location");
            this.selectLabel.Size = (Size) manager.GetObject("selectLabel.Size");
            this.selectLabel.TabIndex = (int) manager.GetObject("selectLabel.TabIndex");
            this.selectLabel.Text = manager.GetString("selectLabel.Text");
            this.referenceLabel.Location = (Point) manager.GetObject("referenceLabel.Location");
            this.referenceLabel.Size = (Size) manager.GetObject("referenceLabel.Size");
            this.referenceLabel.TabIndex = (int) manager.GetObject("referenceLabel.TabIndex");
            this.referenceLabel.Text = manager.GetString("referenceLabel.Text");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Location = (Point) manager.GetObject("cancelButton.Location");
            this.cancelButton.Size = (Size) manager.GetObject("cancelButton.Size");
            this.cancelButton.TabIndex = (int) manager.GetObject("cancelButton.TabIndex");
            this.cancelButton.Text = manager.GetString("cancelButton.Text");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            base.HelpRequested += new HelpEventHandler(this.OnHelpRequested);
            base.AcceptButton = this.okButton;
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoScaleDimensions = new SizeF(6f, 14f);
            base.CancelButton = this.cancelButton;
            base.ClientSize = (Size) manager.GetObject("$this.ClientSize");
            base.Controls.AddRange(new Control[] { this.helpButton, this.cancelButton, this.okButton, this.pathType, this.referenceLabel, this.enterprise, this.selectLabel });
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "Win32Form1";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = manager.GetString("$this.Text");
            this.lastPathType = 0;
            base.Icon = null;
        }

        private void OnClickHelpButton(object source, EventArgs e)
        {
            if (this.provider != null)
            {
                IHelpService service = (IHelpService) this.provider.GetService(typeof(IHelpService));
                if (service != null)
                {
                    service.ShowHelpFromKeyword(HELP_KEYWORD);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.closed = true;
            if (this.populateThread != null)
            {
                this.populateThread.Abort();
            }
            base.OnFormClosing(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!this.populateThreadRan)
            {
                this.populateThreadRan = true;
                this.populateThread = new Thread(new ThreadStart(this.PopulateThread));
                this.populateThread.Start();
            }
            base.OnHandleCreated(e);
        }

        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            this.OnClickHelpButton(null, null);
        }

        private void OnPopulateTreeview(MessageQueue[] queues)
        {
            if ((queues != null) && (queues.Length != 0))
            {
                if (this.machinesTable.Count == 0)
                {
                    this.enterprise.Nodes.Clear();
                }
                for (int i = 0; i < queues.Length; i++)
                {
                    if (queues[i] != null)
                    {
                        string machineName = queues[i].MachineName;
                        TreeNode node = null;
                        if (this.machinesTable.ContainsKey(machineName))
                        {
                            node = (TreeNode) this.machinesTable[machineName];
                        }
                        else
                        {
                            node = this.enterprise.Nodes.Add(machineName);
                            this.machinesTable[machineName] = node;
                        }
                        node.Nodes.Add(queues[i].QueueName).ImageIndex = 1;
                    }
                }
            }
        }

        private void OnSelectQueue(MessageQueue queue, int pathTypeIndex)
        {
            try
            {
                this.pathType.SelectedIndex = pathTypeIndex;
                string machineName = queue.MachineName;
                string queueName = queue.QueueName;
                TreeNodeCollection nodes = this.enterprise.Nodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    TreeNode node = nodes[i];
                    if (string.Compare(machineName, node.Text, true, CultureInfo.InvariantCulture) == 0)
                    {
                        node.Expand();
                        Application.DoEvents();
                        TreeNodeCollection nodes2 = node.Nodes;
                        for (int j = 0; j < nodes2.Count; j++)
                        {
                            TreeNode node2 = nodes2[j];
                            if ((node2.Text != null) && (string.Compare(queueName, node2.Text, true, CultureInfo.InvariantCulture) == 0))
                            {
                                this.enterprise.SelectedNode = node2;
                                break;
                            }
                        }
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        private void OnShowError()
        {
            if (this.uiService != null)
            {
                this.uiService.ShowError(Res.GetString("QueueNetworkProblems"));
            }
            else
            {
                MessageBox.Show(Res.GetString("QueueNetworkProblems"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void PopulateThread()
        {
            try
            {
                IEnumerator messageQueueEnumerator = MessageQueue.GetMessageQueueEnumerator();
                bool flag = true;
                while (flag)
                {
                    MessageQueue[] queueArray = new MessageQueue[100];
                    for (int i = 0; i < queueArray.Length; i++)
                    {
                        if (messageQueueEnumerator.MoveNext())
                        {
                            queueArray[i] = (MessageQueue) messageQueueEnumerator.Current;
                        }
                        else
                        {
                            queueArray[i] = null;
                            flag = false;
                        }
                    }
                    base.BeginInvoke(new FinishPopulateDelegate(this.OnPopulateTreeview), new object[] { queueArray });
                }
            }
            catch
            {
                if (!this.closed)
                {
                    base.BeginInvoke(new ShowErrorDelegate(this.OnShowError), null);
                }
            }
            if (!this.closed)
            {
                base.BeginInvoke(new SelectQueueDelegate(this.OnSelectQueue), new object[] { this.selectedQueue, 0 });
            }
        }

        public void SelectQueue(MessageQueue queue)
        {
            this.selectedQueue = queue;
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.queuePath;
            }
        }

        private delegate void FinishPopulateDelegate(MessageQueue[] queues);

        private delegate void SelectQueueDelegate(MessageQueue queue, int pathTypeIndex);

        private delegate void ShowErrorDelegate();
    }
}

