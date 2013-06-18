namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public class ParameterEditorUserControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Panel _addButtonPanel;
        private System.Windows.Forms.Button _addParameterButton;
        private AdvancedParameterEditor _advancedParameterEditor;
        private System.Web.UI.Control _control;
        private ControlParameterEditor _controlParameterEditor;
        private CookieParameterEditor _cookieParameterEditor;
        private System.Windows.Forms.Button _deleteParameterButton;
        private System.Windows.Forms.Panel _editorPanel;
        private FormParameterEditor _formParameterEditor;
        private int _ignoreParameterChangesCount;
        private bool _inAdvancedMode;
        private System.Windows.Forms.Button _moveDownButton;
        private System.Windows.Forms.Button _moveUpButton;
        private ColumnHeader _nameColumnHeader;
        private ParameterEditor _parameterEditor;
        private System.Windows.Forms.Label _parametersLabel;
        private ListView _parametersListView;
        private AutoSizeComboBox _parameterTypeComboBox;
        private ListDictionary _parameterTypes;
        private ProfileParameterEditor _profileParameterEditor;
        private System.ComponentModel.TypeDescriptionProvider _provider;
        private QueryStringParameterEditor _queryStringParameterEditor;
        private RouteParameterEditor _routeParameterEditor;
        private IServiceProvider _serviceProvider;
        private SessionParameterEditor _sessionParameterEditor;
        private System.Windows.Forms.Label _sourceLabel;
        private StaticParameterEditor _staticParameterEditor;
        private ColumnHeader _valueColumnHeader;
        private static readonly object EventParametersChanged = new object();

        public event EventHandler ParametersChanged
        {
            add
            {
                base.Events.AddHandler(EventParametersChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventParametersChanged, value);
            }
        }

        public ParameterEditorUserControl(IServiceProvider serviceProvider) : this(serviceProvider, null, null)
        {
        }

        internal ParameterEditorUserControl(IServiceProvider serviceProvider, System.Web.UI.Control control) : this(serviceProvider, control, null)
        {
        }

        internal ParameterEditorUserControl(IServiceProvider serviceProvider, System.Web.UI.Control control, System.ComponentModel.TypeDescriptionProvider provider)
        {
            this._serviceProvider = serviceProvider;
            this._control = control;
            this._provider = provider;
            this.InitializeComponent();
            this.InitializeUI();
            this.InitializeParameterEditors();
            this._parameterTypes = this.CreateParameterList();
            foreach (DictionaryEntry entry in this._parameterTypes)
            {
                this._parameterTypeComboBox.Items.Add(entry.Value);
            }
            this._parameterTypeComboBox.InvalidateDropDownWidth();
            this.UpdateUI(false);
        }

        private void AddParameter(Parameter parameter)
        {
            try
            {
                this.IgnoreParameterChanges(true);
                ParameterListViewItem item = new ParameterListViewItem(parameter);
                this._parametersListView.BeginUpdate();
                try
                {
                    this._parametersListView.Items.Add(item);
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    this._parametersListView.Focus();
                }
                finally
                {
                    this._parametersListView.EndUpdate();
                }
                item.Refresh();
                item.BeginEdit();
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        public void AddParameters(Parameter[] parameters)
        {
            try
            {
                this.IgnoreParameterChanges(true);
                this._parametersListView.BeginUpdate();
                ArrayList list = new ArrayList();
                try
                {
                    foreach (Parameter parameter in parameters)
                    {
                        ParameterListViewItem item = new ParameterListViewItem(parameter);
                        this._parametersListView.Items.Add(item);
                        list.Add(item);
                    }
                    if (this._parametersListView.Items.Count > 0)
                    {
                        this._parametersListView.Items[0].Selected = true;
                        this._parametersListView.Items[0].Focused = true;
                        this._parametersListView.Items[0].EnsureVisible();
                    }
                    this._parametersListView.Focus();
                }
                finally
                {
                    this._parametersListView.EndUpdate();
                }
                foreach (ParameterListViewItem item2 in list)
                {
                    item2.Refresh();
                }
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        public void ClearParameters()
        {
            try
            {
                this.IgnoreParameterChanges(true);
                this._parametersListView.Items.Clear();
                this.UpdateUI(false);
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        internal ListDictionary CreateParameterList()
        {
            ListDictionary dictionary = new ListDictionary();
            dictionary.Add(typeof(Parameter), "None");
            dictionary.Add(typeof(CookieParameter), "Cookie");
            dictionary.Add(typeof(ControlParameter), "Control");
            dictionary.Add(typeof(FormParameter), "Form");
            dictionary.Add(typeof(ProfileParameter), "Profile");
            dictionary.Add(typeof(QueryStringParameter), "QueryString");
            dictionary.Add(typeof(SessionParameter), "Session");
            System.ComponentModel.TypeDescriptionProvider typeDescriptionProvider = this.TypeDescriptionProvider;
            if ((typeDescriptionProvider == null) || typeDescriptionProvider.IsSupportedType(typeof(RouteParameter)))
            {
                dictionary.Add(typeof(RouteParameter), "RouteData");
            }
            return dictionary;
        }

        internal static string GetControlDefaultValuePropertyName(string controlID, IServiceProvider serviceProvider, System.Web.UI.Control control)
        {
            System.Web.UI.Control control2 = ControlHelper.FindControl(serviceProvider, control, controlID);
            if (control2 != null)
            {
                return GetDefaultValuePropertyName(control2);
            }
            return string.Empty;
        }

        private static string GetDefaultValuePropertyName(System.Web.UI.Control control)
        {
            ControlValuePropertyAttribute attribute = (ControlValuePropertyAttribute) TypeDescriptor.GetAttributes(control)[typeof(ControlValuePropertyAttribute)];
            if ((attribute != null) && !string.IsNullOrEmpty(attribute.Name))
            {
                return attribute.Name;
            }
            return string.Empty;
        }

        internal static string GetParameterExpression(IServiceProvider serviceProvider, Parameter p, System.Web.UI.Control control, out bool isHelperText)
        {
            if (p.GetType() == typeof(ControlParameter))
            {
                ControlParameter parameter = (ControlParameter) p;
                if (parameter.ControlID.Length == 0)
                {
                    isHelperText = true;
                    return System.Design.SR.GetString("ParameterEditorUserControl_ControlParameterExpressionUnknown");
                }
                string propertyName = parameter.PropertyName;
                if (propertyName.Length == 0)
                {
                    propertyName = GetControlDefaultValuePropertyName(parameter.ControlID, serviceProvider, control);
                }
                if (propertyName.Length > 0)
                {
                    isHelperText = false;
                    return (parameter.ControlID + "." + propertyName);
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_ControlParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(FormParameter))
            {
                FormParameter parameter2 = (FormParameter) p;
                if (parameter2.FormField.Length > 0)
                {
                    isHelperText = false;
                    return string.Format(CultureInfo.InvariantCulture, "Request.Form(\"{0}\")", new object[] { parameter2.FormField });
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_FormParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(QueryStringParameter))
            {
                QueryStringParameter parameter3 = (QueryStringParameter) p;
                if (parameter3.QueryStringField.Length > 0)
                {
                    isHelperText = false;
                    return string.Format(CultureInfo.InvariantCulture, "Request.QueryString(\"{0}\")", new object[] { parameter3.QueryStringField });
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_QueryStringParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(RouteParameter))
            {
                RouteParameter parameter4 = (RouteParameter) p;
                if (parameter4.RouteKey.Length > 0)
                {
                    isHelperText = false;
                    return string.Format(CultureInfo.InvariantCulture, "Page.RouteData(\"{0}\")", new object[] { parameter4.RouteKey });
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_RouteParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(CookieParameter))
            {
                CookieParameter parameter5 = (CookieParameter) p;
                if (parameter5.CookieName.Length > 0)
                {
                    isHelperText = false;
                    return string.Format(CultureInfo.InvariantCulture, "Request.Cookies(\"{0}\").Value", new object[] { parameter5.CookieName });
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_CookieParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(SessionParameter))
            {
                SessionParameter parameter6 = (SessionParameter) p;
                if (parameter6.SessionField.Length > 0)
                {
                    isHelperText = false;
                    return string.Format(CultureInfo.InvariantCulture, "Session(\"{0}\")", new object[] { parameter6.SessionField });
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_SessionParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(ProfileParameter))
            {
                ProfileParameter parameter7 = (ProfileParameter) p;
                if (parameter7.PropertyName.Length > 0)
                {
                    isHelperText = false;
                    return string.Format(CultureInfo.InvariantCulture, "Profile(\"{0}\")", new object[] { parameter7.PropertyName });
                }
                isHelperText = true;
                return System.Design.SR.GetString("ParameterEditorUserControl_ProfileParameterExpressionUnknown");
            }
            if (p.GetType() == typeof(Parameter))
            {
                Parameter parameter8 = p;
                if (parameter8.DefaultValue == null)
                {
                    isHelperText = false;
                    return string.Empty;
                }
                isHelperText = false;
                return parameter8.DefaultValue;
            }
            isHelperText = true;
            return p.GetType().Name;
        }

        public Parameter[] GetParameters()
        {
            ArrayList list = new ArrayList();
            foreach (ParameterListViewItem item in this._parametersListView.Items)
            {
                if (item.Parameter != null)
                {
                    list.Add(item.Parameter);
                }
            }
            return (Parameter[]) list.ToArray(typeof(Parameter));
        }

        private void IgnoreParameterChanges(bool ignoreChanges)
        {
            this._ignoreParameterChangesCount += ignoreChanges ? 1 : -1;
            if (this._ignoreParameterChangesCount == 0)
            {
                this.UpdateUI(false);
            }
        }

        private void InitializeComponent()
        {
            this._addButtonPanel = new System.Windows.Forms.Panel();
            this._addParameterButton = new System.Windows.Forms.Button();
            this._parametersLabel = new System.Windows.Forms.Label();
            this._sourceLabel = new System.Windows.Forms.Label();
            this._parametersListView = new ListView();
            this._nameColumnHeader = new ColumnHeader("");
            this._valueColumnHeader = new ColumnHeader("");
            this._parameterTypeComboBox = new AutoSizeComboBox();
            this._moveUpButton = new System.Windows.Forms.Button();
            this._moveDownButton = new System.Windows.Forms.Button();
            this._deleteParameterButton = new System.Windows.Forms.Button();
            this._editorPanel = new System.Windows.Forms.Panel();
            this._addButtonPanel.SuspendLayout();
            base.SuspendLayout();
            this._parametersLabel.Location = new Point(0, 0);
            this._parametersLabel.Name = "_parametersLabel";
            this._parametersLabel.Size = new Size(0xfc, 0x10);
            this._parametersLabel.TabIndex = 10;
            this._parametersListView.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._parametersListView.Columns.AddRange(new ColumnHeader[] { this._nameColumnHeader, this._valueColumnHeader });
            this._parametersListView.FullRowSelect = true;
            this._parametersListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this._parametersListView.HideSelection = false;
            this._parametersListView.LabelEdit = true;
            this._parametersListView.Location = new Point(0, 0x12);
            this._parametersListView.MultiSelect = false;
            this._parametersListView.Name = "_parametersListView";
            this._parametersListView.Size = new Size(0xfc, 0xe0);
            this._parametersListView.TabIndex = 20;
            this._parametersListView.View = System.Windows.Forms.View.Details;
            this._parametersListView.SelectedIndexChanged += new EventHandler(this.OnParametersListViewSelectedIndexChanged);
            this._parametersListView.AfterLabelEdit += new LabelEditEventHandler(this.OnParametersListViewAfterLabelEdit);
            this._nameColumnHeader.Width = 0x55;
            this._valueColumnHeader.Width = 0x86;
            this._addButtonPanel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._addButtonPanel.Controls.Add(this._addParameterButton);
            this._addButtonPanel.Location = new Point(0, 0xf8);
            this._addButtonPanel.Name = "_addButtonPanel";
            this._addButtonPanel.Size = new Size(0xfc, 30);
            this._addButtonPanel.TabIndex = 30;
            this._addParameterButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._addParameterButton.AutoSize = true;
            this._addParameterButton.Location = new Point(0x7c, 0);
            this._addParameterButton.Name = "_addParameterButton";
            this._addParameterButton.Size = new Size(0x80, 0x17);
            this._addParameterButton.TabIndex = 10;
            this._addParameterButton.Click += new EventHandler(this.OnAddParameterButtonClick);
            this._moveUpButton.Location = new Point(0x102, 0x12);
            this._moveUpButton.Name = "_moveUpButton";
            this._moveUpButton.Size = new Size(0x1a, 0x17);
            this._moveUpButton.TabIndex = 40;
            this._moveUpButton.Click += new EventHandler(this.OnMoveUpButtonClick);
            this._moveDownButton.Location = new Point(0x102, 0x2a);
            this._moveDownButton.Name = "_moveDownButton";
            this._moveDownButton.Size = new Size(0x1a, 0x17);
            this._moveDownButton.TabIndex = 50;
            this._moveDownButton.Click += new EventHandler(this.OnMoveDownButtonClick);
            this._deleteParameterButton.Location = new Point(0x102, 0x47);
            this._deleteParameterButton.Name = "_deleteParameterButton";
            this._deleteParameterButton.Size = new Size(0x1a, 0x17);
            this._deleteParameterButton.TabIndex = 60;
            this._deleteParameterButton.Click += new EventHandler(this.OnDeleteParameterButtonClick);
            this._sourceLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sourceLabel.Location = new Point(0x124, 0);
            this._sourceLabel.Name = "_sourceLabel";
            this._sourceLabel.Size = new Size(300, 0x10);
            this._sourceLabel.TabIndex = 70;
            this._parameterTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._parameterTypeComboBox.Location = new Point(0x124, 0x12);
            this._parameterTypeComboBox.Name = "_parameterTypeComboBox";
            this._parameterTypeComboBox.Size = new Size(0xa3, 0x15);
            this._parameterTypeComboBox.TabIndex = 80;
            this._parameterTypeComboBox.SelectedIndexChanged += new EventHandler(this.OnParameterTypeComboBoxSelectedIndexChanged);
            this._editorPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._editorPanel.Location = new Point(0x124, 0x2f);
            this._editorPanel.Name = "_editorPanel";
            this._editorPanel.Size = new Size(0x134, 0xeb);
            this._editorPanel.TabIndex = 90;
            base.Controls.Add(this._editorPanel);
            base.Controls.Add(this._addButtonPanel);
            base.Controls.Add(this._deleteParameterButton);
            base.Controls.Add(this._moveDownButton);
            base.Controls.Add(this._moveUpButton);
            base.Controls.Add(this._parameterTypeComboBox);
            base.Controls.Add(this._parametersListView);
            base.Controls.Add(this._sourceLabel);
            base.Controls.Add(this._parametersLabel);
            this.MinimumSize = new Size(460, 0x7e);
            base.Name = "ParameterEditorUserControl";
            base.Size = new Size(600, 280);
            this._addButtonPanel.ResumeLayout(false);
            this._addButtonPanel.PerformLayout();
            base.ResumeLayout(false);
        }

        private void InitializeParameterEditors()
        {
            this._advancedParameterEditor = new AdvancedParameterEditor(this._serviceProvider, this._control);
            this._advancedParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._advancedParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._advancedParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._advancedParameterEditor);
            this._staticParameterEditor = new StaticParameterEditor(this._serviceProvider);
            this._staticParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._staticParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._staticParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._staticParameterEditor);
            this._controlParameterEditor = new ControlParameterEditor(this._serviceProvider, this._control);
            this._controlParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._controlParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._controlParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._controlParameterEditor);
            this._formParameterEditor = new FormParameterEditor(this._serviceProvider);
            this._formParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._formParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._formParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._formParameterEditor);
            this._queryStringParameterEditor = new QueryStringParameterEditor(this._serviceProvider);
            this._queryStringParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._queryStringParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._queryStringParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._queryStringParameterEditor);
            this._routeParameterEditor = new RouteParameterEditor(this._serviceProvider);
            this._routeParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._routeParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._routeParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._routeParameterEditor);
            this._cookieParameterEditor = new CookieParameterEditor(this._serviceProvider);
            this._cookieParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._cookieParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._cookieParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._cookieParameterEditor);
            this._sessionParameterEditor = new SessionParameterEditor(this._serviceProvider);
            this._sessionParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._sessionParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._sessionParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._sessionParameterEditor);
            this._profileParameterEditor = new ProfileParameterEditor(this._serviceProvider);
            this._profileParameterEditor.RequestModeChange += new EventHandler(this.ToggleAdvancedMode);
            this._profileParameterEditor.ParameterChanged += new EventHandler(this.OnParametersChanged);
            this._profileParameterEditor.Visible = false;
            this._editorPanel.Controls.Add(this._profileParameterEditor);
        }

        private void InitializeUI()
        {
            this._parametersLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParametersLabel");
            this._nameColumnHeader.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterNameColumnHeader");
            this._valueColumnHeader.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterValueColumnHeader");
            this._addParameterButton.Text = System.Design.SR.GetString("ParameterEditorUserControl_AddButton");
            this._sourceLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_SourceLabel");
            Bitmap bitmap = new Icon(typeof(ParameterEditorUserControl), "SortUp.ico").ToBitmap();
            bitmap.MakeTransparent();
            this._moveUpButton.Image = bitmap;
            Bitmap bitmap2 = new Icon(typeof(ParameterEditorUserControl), "SortDown.ico").ToBitmap();
            bitmap2.MakeTransparent();
            this._moveDownButton.Image = bitmap2;
            Bitmap bitmap3 = new Icon(typeof(ParameterEditorUserControl), "Delete.ico").ToBitmap();
            bitmap3.MakeTransparent();
            this._deleteParameterButton.Image = bitmap3;
            this._moveUpButton.AccessibleName = System.Design.SR.GetString("ParameterEditorUserControl_MoveParameterUp");
            this._moveDownButton.AccessibleName = System.Design.SR.GetString("ParameterEditorUserControl_MoveParameterDown");
            this._deleteParameterButton.AccessibleName = System.Design.SR.GetString("ParameterEditorUserControl_DeleteParameter");
        }

        private void OnAddParameterButtonClick(object sender, EventArgs e)
        {
            this.AddParameter(new Parameter("newparameter"));
        }

        private void OnDeleteParameterButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.IgnoreParameterChanges(true);
                if (this._parametersListView.SelectedItems.Count == 0)
                {
                    return;
                }
                int index = this._parametersListView.SelectedIndices[0];
                this._parametersListView.BeginUpdate();
                try
                {
                    this._parametersListView.Items.RemoveAt(index);
                    if (index < this._parametersListView.Items.Count)
                    {
                        this._parametersListView.Items[index].Selected = true;
                        this._parametersListView.Items[index].Focused = true;
                        this._parametersListView.Items[index].EnsureVisible();
                        this._parametersListView.Focus();
                    }
                    else if (this._parametersListView.Items.Count > 0)
                    {
                        index = this._parametersListView.Items.Count - 1;
                        this._parametersListView.Items[index].Selected = true;
                        this._parametersListView.Items[index].Focused = true;
                        this._parametersListView.Items[index].EnsureVisible();
                        this._parametersListView.Focus();
                    }
                }
                finally
                {
                    this._parametersListView.EndUpdate();
                }
                this.UpdateUI(false);
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        private void OnMoveDownButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.IgnoreParameterChanges(true);
                if (this._parametersListView.SelectedItems.Count == 0)
                {
                    return;
                }
                int num = this._parametersListView.SelectedIndices[0];
                if (num == (this._parametersListView.Items.Count - 1))
                {
                    return;
                }
                this._parametersListView.BeginUpdate();
                try
                {
                    ListViewItem item = this._parametersListView.Items[num];
                    item.Remove();
                    this._parametersListView.Items.Insert(num + 1, item);
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    this._parametersListView.Focus();
                }
                finally
                {
                    this._parametersListView.EndUpdate();
                }
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        private void OnMoveUpButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.IgnoreParameterChanges(true);
                if (this._parametersListView.SelectedItems.Count == 0)
                {
                    return;
                }
                int num = this._parametersListView.SelectedIndices[0];
                if (num == 0)
                {
                    return;
                }
                this._parametersListView.BeginUpdate();
                try
                {
                    ListViewItem item = this._parametersListView.Items[num];
                    item.Remove();
                    this._parametersListView.Items.Insert(num - 1, item);
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    this._parametersListView.Focus();
                }
                finally
                {
                    this._parametersListView.EndUpdate();
                }
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        protected virtual void OnParametersChanged(object sender, EventArgs e)
        {
            if (this._ignoreParameterChangesCount <= 0)
            {
                EventHandler handler = base.Events[EventParametersChanged] as EventHandler;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private void OnParametersListViewAfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if ((e.Label == null) || (e.Label.Trim().Length == 0))
            {
                e.CancelEdit = true;
            }
            else
            {
                ParameterListViewItem item = (ParameterListViewItem) this._parametersListView.Items[e.Item];
                item.ParameterName = e.Label;
                this.UpdateUI(false);
            }
        }

        private void OnParametersListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateUI(false);
        }

        private void OnParameterTypeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.IgnoreParameterChanges(true);
                if (this._parametersListView.SelectedItems.Count == 0)
                {
                    return;
                }
                ParameterListViewItem parameterItem = (ParameterListViewItem) this._parametersListView.SelectedItems[0];
                string selectedItem = (string) this._parameterTypeComboBox.SelectedItem;
                System.Type key = null;
                foreach (DictionaryEntry entry in this._parameterTypes)
                {
                    if (((string) entry.Value) == selectedItem)
                    {
                        key = (System.Type) entry.Key;
                    }
                }
                if ((key != null) && ((parameterItem.Parameter == null) || (parameterItem.Parameter.GetType() != key)))
                {
                    parameterItem.Parameter = (Parameter) Activator.CreateInstance(key);
                    parameterItem.Refresh();
                }
                this.SetActiveEditParameterItem(parameterItem, false);
            }
            finally
            {
                this.IgnoreParameterChanges(false);
            }
            this.OnParametersChanged(this, EventArgs.Empty);
        }

        private void SetActiveEditParameterItem(ParameterListViewItem parameterItem, bool allowFocusChange)
        {
            if (parameterItem == null)
            {
                if (this._parameterEditor != null)
                {
                    this._parameterEditor.Visible = false;
                    this._parameterEditor = null;
                }
            }
            else
            {
                ParameterEditor editor = null;
                if (this._inAdvancedMode)
                {
                    editor = this._advancedParameterEditor;
                }
                else if (parameterItem.Parameter != null)
                {
                    if (parameterItem.Parameter.GetType() == typeof(Parameter))
                    {
                        editor = this._staticParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(ControlParameter))
                    {
                        editor = this._controlParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(FormParameter))
                    {
                        editor = this._formParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(QueryStringParameter))
                    {
                        editor = this._queryStringParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(CookieParameter))
                    {
                        editor = this._cookieParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(SessionParameter))
                    {
                        editor = this._sessionParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(ProfileParameter))
                    {
                        editor = this._profileParameterEditor;
                    }
                    else if (parameterItem.Parameter.GetType() == typeof(RouteParameter))
                    {
                        editor = this._routeParameterEditor;
                    }
                }
                if (this._parameterEditor != editor)
                {
                    if (this._parameterEditor != null)
                    {
                        this._parameterEditor.Visible = false;
                    }
                    this._parameterEditor = editor;
                }
                if (this._parameterEditor != null)
                {
                    this._parameterEditor.InitializeParameter(parameterItem);
                    this._parameterEditor.Visible = true;
                    if (allowFocusChange)
                    {
                        this._parameterEditor.SetDefaultFocus();
                    }
                }
            }
        }

        public void SetAllowCollectionChanges(bool allowChanges)
        {
            this._moveUpButton.Visible = allowChanges;
            this._moveDownButton.Visible = allowChanges;
            this._deleteParameterButton.Visible = allowChanges;
            this._addParameterButton.Visible = allowChanges;
        }

        private void ToggleAdvancedMode(object sender, EventArgs e)
        {
            this._inAdvancedMode = !this._inAdvancedMode;
            this.UpdateUI(true);
        }

        private void UpdateUI(bool allowFocusChange)
        {
            if (this._parametersListView.SelectedItems.Count > 0)
            {
                ParameterListViewItem parameterItem = (ParameterListViewItem) this._parametersListView.SelectedItems[0];
                this._deleteParameterButton.Enabled = true;
                this._moveUpButton.Enabled = this._parametersListView.SelectedIndices[0] > 0;
                this._moveDownButton.Enabled = this._parametersListView.SelectedIndices[0] < (this._parametersListView.Items.Count - 1);
                this._sourceLabel.Enabled = true;
                this._parameterTypeComboBox.Enabled = true;
                this._editorPanel.Enabled = true;
                if (parameterItem.Parameter == null)
                {
                    this._parameterTypeComboBox.SelectedIndex = -1;
                }
                else
                {
                    System.Type type = parameterItem.Parameter.GetType();
                    object obj2 = this._parameterTypes[type];
                    if (obj2 != null)
                    {
                        this._parameterTypeComboBox.SelectedItem = obj2;
                    }
                    else
                    {
                        this._parameterTypeComboBox.SelectedIndex = -1;
                    }
                }
                this.SetActiveEditParameterItem(parameterItem, allowFocusChange);
            }
            else
            {
                this._deleteParameterButton.Enabled = false;
                this._moveUpButton.Enabled = false;
                this._moveDownButton.Enabled = false;
                this._sourceLabel.Enabled = false;
                this._parameterTypeComboBox.Enabled = false;
                this._parameterTypeComboBox.SelectedIndex = -1;
                this._editorPanel.Enabled = false;
                this.SetActiveEditParameterItem(null, false);
            }
        }

        public bool ParametersConfigured
        {
            get
            {
                foreach (ParameterListViewItem item in this._parametersListView.Items)
                {
                    if ((item != null) && !item.IsConfigured)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public System.ComponentModel.TypeDescriptionProvider TypeDescriptionProvider
        {
            get
            {
                if (this._provider != null)
                {
                    return this._provider;
                }
                if (this._control != null)
                {
                    return TypeDescriptor.GetProvider(this._control);
                }
                if (this._serviceProvider != null)
                {
                    TypeDescriptionProviderService service = this._serviceProvider.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                    if (service != null)
                    {
                        return service.GetProvider(null);
                    }
                }
                return null;
            }
        }

        private sealed class AdvancedParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _advancedlabel;
            private System.Web.UI.Control _control;
            private LinkLabel _hideAdvancedLinkLabel;
            private PropertyGrid _parameterPropertyGrid;

            public AdvancedParameterEditor(IServiceProvider serviceProvider, System.Web.UI.Control control) : base(serviceProvider)
            {
                this._control = control;
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._advancedlabel = new System.Windows.Forms.Label();
                this._parameterPropertyGrid = new VsPropertyGrid(base.ServiceProvider);
                this._hideAdvancedLinkLabel = new LinkLabel();
                this._advancedlabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._advancedlabel.Location = new Point(0, 0);
                this._advancedlabel.Size = new Size(400, 0x10);
                this._advancedlabel.TabIndex = 10;
                this._advancedlabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_AdvancedProperties");
                this._parameterPropertyGrid.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                this._parameterPropertyGrid.CommandsVisibleIfAvailable = true;
                this._parameterPropertyGrid.LargeButtons = false;
                this._parameterPropertyGrid.LineColor = SystemColors.ScrollBar;
                this._parameterPropertyGrid.Location = new Point(0, 0x12);
                this._parameterPropertyGrid.PropertySort = PropertySort.Alphabetical;
                this._parameterPropertyGrid.Site = new ParameterEditorUserControl.PropertyGridSite(base.ServiceProvider, this._parameterPropertyGrid);
                this._parameterPropertyGrid.Size = new Size(400, 0x164);
                this._parameterPropertyGrid.TabIndex = 20;
                this._parameterPropertyGrid.ToolbarVisible = false;
                this._parameterPropertyGrid.ViewBackColor = SystemColors.Window;
                this._parameterPropertyGrid.ViewForeColor = SystemColors.WindowText;
                this._parameterPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnParameterPropertyGridPropertyValueChanged);
                this._hideAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
                this._hideAdvancedLinkLabel.Location = new Point(0, 0x180);
                this._hideAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._hideAdvancedLinkLabel.TabIndex = 30;
                this._hideAdvancedLinkLabel.TabStop = true;
                this._hideAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_HideAdvancedPropertiesLabel");
                this._hideAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._hideAdvancedLinkLabel.Text.Length));
                this._hideAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnHideAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._advancedlabel);
                base.Controls.Add(this._parameterPropertyGrid);
                base.Controls.Add(this._hideAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._parameterPropertyGrid.SelectedObject = base.ParameterItem.Parameter;
            }

            private void OnHideAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            private void OnParameterPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
            {
                if (e.ChangedItem.PropertyDescriptor.Name == "ControlID")
                {
                    ControlParameter parameter = base.ParameterItem.Parameter as ControlParameter;
                    if (((parameter != null) && (parameter.PropertyName.Length == 0)) && (parameter.ControlID != ((string) e.OldValue)))
                    {
                        parameter.PropertyName = ParameterEditorUserControl.GetControlDefaultValuePropertyName(parameter.ControlID, base.ServiceProvider, this._control);
                    }
                }
                base.OnParameterChanged();
            }

            public override void SetDefaultFocus()
            {
                this._parameterPropertyGrid.Focus();
            }
        }

        internal sealed class ControlItem
        {
            private string _controlID;
            private string _propertyName;

            public ControlItem(string controlID, string propertyName)
            {
                this._controlID = controlID;
                this._propertyName = propertyName;
            }

            public static ParameterEditorUserControl.ControlItem[] GetControlItems(IDesignerHost host, System.Web.UI.Control control)
            {
                IList<IComponent> allComponents = ControlHelper.GetAllComponents(control, new ControlHelper.IsValidComponentDelegate(ParameterEditorUserControl.ControlItem.IsValidComponent));
                List<ParameterEditorUserControl.ControlItem> list2 = new List<ParameterEditorUserControl.ControlItem>();
                foreach (System.Web.UI.Control control2 in allComponents)
                {
                    string defaultValuePropertyName = ParameterEditorUserControl.GetDefaultValuePropertyName(control2);
                    if (!string.IsNullOrEmpty(defaultValuePropertyName))
                    {
                        list2.Add(new ParameterEditorUserControl.ControlItem(control2.ID, defaultValuePropertyName));
                    }
                }
                return list2.ToArray();
            }

            private static bool IsValidComponent(IComponent component)
            {
                System.Web.UI.Control control = component as System.Web.UI.Control;
                if (control == null)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(control.ID))
                {
                    return false;
                }
                return true;
            }

            public override string ToString()
            {
                return this._controlID;
            }

            public string ControlID
            {
                get
                {
                    return this._controlID;
                }
            }

            public string PropertyName
            {
                get
                {
                    return this._propertyName;
                }
            }
        }

        private sealed class ControlParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Web.UI.Control _control;
            private AutoSizeComboBox _controlIDComboBox;
            private System.Windows.Forms.Label _controlIDLabel;
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public ControlParameterEditor(IServiceProvider serviceProvider, System.Web.UI.Control control) : base(serviceProvider)
            {
                this._control = control;
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._controlIDLabel = new System.Windows.Forms.Label();
                this._controlIDComboBox = new AutoSizeComboBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._controlIDLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._controlIDLabel.Location = new Point(0, 0);
                this._controlIDLabel.Size = new Size(400, 0x10);
                this._controlIDLabel.TabIndex = 10;
                this._controlIDLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ControlParameterControlID");
                this._controlIDComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._controlIDComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                this._controlIDComboBox.Location = new Point(0, 0x12);
                this._controlIDComboBox.Size = new Size(400, 0x15);
                this._controlIDComboBox.Sorted = true;
                this._controlIDComboBox.TabIndex = 20;
                this._controlIDComboBox.SelectedIndexChanged += new EventHandler(this.OnControlIDComboBoxSelectedIndexChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2d);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3f);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x57);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._controlIDLabel);
                base.Controls.Add(this._controlIDComboBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                string controlID = ((ControlParameter) base.ParameterItem.Parameter).ControlID;
                string propertyName = ((ControlParameter) base.ParameterItem.Parameter).PropertyName;
                this._controlIDComboBox.Items.Clear();
                ParameterEditorUserControl.ControlItem item = null;
                if (base.ServiceProvider != null)
                {
                    IDesignerHost service = (IDesignerHost) base.ServiceProvider.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        foreach (ParameterEditorUserControl.ControlItem item2 in ParameterEditorUserControl.ControlItem.GetControlItems(service, this._control))
                        {
                            this._controlIDComboBox.Items.Add(item2);
                            if ((item2.ControlID == controlID) && (item2.PropertyName == propertyName))
                            {
                                item = item2;
                            }
                        }
                    }
                }
                if ((item == null) && (controlID.Length > 0))
                {
                    ParameterEditorUserControl.ControlItem item3 = new ParameterEditorUserControl.ControlItem(controlID, propertyName);
                    this._controlIDComboBox.Items.Insert(0, item3);
                    item = item3;
                }
                this._controlIDComboBox.InvalidateDropDownWidth();
                this._controlIDComboBox.SelectedItem = item;
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
            }

            private void OnControlIDComboBoxSelectedIndexChanged(object s, EventArgs e)
            {
                ParameterEditorUserControl.ControlItem selectedItem = this._controlIDComboBox.SelectedItem as ParameterEditorUserControl.ControlItem;
                ControlParameter parameter = (ControlParameter) base.ParameterItem.Parameter;
                if (selectedItem == null)
                {
                    parameter.ControlID = string.Empty;
                    parameter.PropertyName = string.Empty;
                }
                else
                {
                    parameter.ControlID = selectedItem.ControlID;
                    parameter.PropertyName = selectedItem.PropertyName;
                }
                base.OnParameterChanged();
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._controlIDComboBox.Focus();
            }
        }

        private sealed class CookieParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _cookieNameLabel;
            private System.Windows.Forms.TextBox _cookieNameTextBox;
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public CookieParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._cookieNameLabel = new System.Windows.Forms.Label();
                this._cookieNameTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._cookieNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._cookieNameLabel.Location = new Point(0, 0);
                this._cookieNameLabel.Size = new Size(400, 0x10);
                this._cookieNameLabel.TabIndex = 10;
                this._cookieNameLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_CookieParameterCookieName");
                this._cookieNameTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._cookieNameTextBox.Location = new Point(0, 0x12);
                this._cookieNameTextBox.Size = new Size(400, 20);
                this._cookieNameTextBox.TabIndex = 20;
                this._cookieNameTextBox.TextChanged += new EventHandler(this.OnCookieNameTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2c);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3e);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x56);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._cookieNameLabel);
                base.Controls.Add(this._cookieNameTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
                this._cookieNameTextBox.Text = ((CookieParameter) base.ParameterItem.Parameter).CookieName;
            }

            private void OnCookieNameTextBoxTextChanged(object s, EventArgs e)
            {
                if (((CookieParameter) base.ParameterItem.Parameter).CookieName != this._cookieNameTextBox.Text)
                {
                    ((CookieParameter) base.ParameterItem.Parameter).CookieName = this._cookieNameTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._cookieNameTextBox.Focus();
            }
        }

        private sealed class FormParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Windows.Forms.Label _formFieldLabel;
            private System.Windows.Forms.TextBox _formFieldTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public FormParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._formFieldLabel = new System.Windows.Forms.Label();
                this._formFieldTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._formFieldLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._formFieldLabel.Location = new Point(0, 0);
                this._formFieldLabel.Size = new Size(400, 0x10);
                this._formFieldLabel.TabIndex = 10;
                this._formFieldLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_FormParameterFormField");
                this._formFieldTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._formFieldTextBox.Location = new Point(0, 0x12);
                this._formFieldTextBox.Size = new Size(400, 20);
                this._formFieldTextBox.TabIndex = 20;
                this._formFieldTextBox.TextChanged += new EventHandler(this.OnFormFieldTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2c);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3e);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x56);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._formFieldLabel);
                base.Controls.Add(this._formFieldTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
                this._formFieldTextBox.Text = ((FormParameter) base.ParameterItem.Parameter).FormField;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnFormFieldTextBoxTextChanged(object s, EventArgs e)
            {
                if (((FormParameter) base.ParameterItem.Parameter).FormField != this._formFieldTextBox.Text)
                {
                    ((FormParameter) base.ParameterItem.Parameter).FormField = this._formFieldTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._formFieldTextBox.Focus();
            }
        }

        private abstract class ParameterEditor : System.Windows.Forms.Panel
        {
            private ParameterEditorUserControl.ParameterListViewItem _parameterItem;
            private IServiceProvider _serviceProvider;
            private static readonly object EventParameterChanged = new object();
            private static readonly object EventRequestModeChange = new object();

            public event EventHandler ParameterChanged
            {
                add
                {
                    base.Events.AddHandler(EventParameterChanged, value);
                }
                remove
                {
                    base.Events.RemoveHandler(EventParameterChanged, value);
                }
            }

            public event EventHandler RequestModeChange
            {
                add
                {
                    base.Events.AddHandler(EventRequestModeChange, value);
                }
                remove
                {
                    base.Events.RemoveHandler(EventRequestModeChange, value);
                }
            }

            protected ParameterEditor(IServiceProvider serviceProvider)
            {
                this._serviceProvider = serviceProvider;
            }

            public virtual void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                this._parameterItem = parameterItem;
            }

            protected void OnParameterChanged()
            {
                this.ParameterItem.Refresh();
                EventHandler handler = base.Events[EventParameterChanged] as EventHandler;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            protected void OnRequestModeChange()
            {
                EventHandler handler = base.Events[EventRequestModeChange] as EventHandler;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public virtual void SetDefaultFocus()
            {
            }

            protected ParameterEditorUserControl.ParameterListViewItem ParameterItem
            {
                get
                {
                    return this._parameterItem;
                }
            }

            protected IServiceProvider ServiceProvider
            {
                get
                {
                    return this._serviceProvider;
                }
            }
        }

        private class ParameterListViewItem : ListViewItem
        {
            private bool _isConfigured;
            private System.Web.UI.WebControls.Parameter _parameter;

            public ParameterListViewItem(System.Web.UI.WebControls.Parameter parameter)
            {
                this._parameter = parameter;
                this._isConfigured = true;
            }

            public void Refresh()
            {
                bool flag;
                base.SubItems.Clear();
                base.Text = this.ParameterName;
                base.UseItemStyleForSubItems = false;
                ListView listView = base.ListView;
                IServiceProvider serviceProvider = null;
                System.Web.UI.Control control = null;
                if (listView != null)
                {
                    ParameterEditorUserControl parent = (ParameterEditorUserControl) listView.Parent;
                    serviceProvider = parent._serviceProvider;
                    control = parent._control;
                }
                string str = ParameterEditorUserControl.GetParameterExpression(serviceProvider, this._parameter, control, out flag);
                this._isConfigured = !flag;
                ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem {
                    Text = str
                };
                if (flag)
                {
                    item.ForeColor = SystemColors.GrayText;
                }
                base.SubItems.Add(item);
            }

            public System.Data.DbType DbType
            {
                get
                {
                    return this._parameter.DbType;
                }
                set
                {
                    this._parameter.DbType = value;
                }
            }

            public bool IsConfigured
            {
                get
                {
                    return this._isConfigured;
                }
            }

            public System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
                set
                {
                    string defaultValue = this._parameter.DefaultValue;
                    ParameterDirection direction = this._parameter.Direction;
                    string name = this._parameter.Name;
                    bool convertEmptyStringToNull = this._parameter.ConvertEmptyStringToNull;
                    int size = this._parameter.Size;
                    TypeCode code = this._parameter.Type;
                    System.Data.DbType dbType = this._parameter.DbType;
                    this._parameter = value;
                    this._parameter.DefaultValue = defaultValue;
                    this._parameter.Direction = direction;
                    this._parameter.Name = name;
                    this._parameter.ConvertEmptyStringToNull = convertEmptyStringToNull;
                    this._parameter.Size = size;
                    this._parameter.Type = code;
                    this._parameter.DbType = dbType;
                }
            }

            public string ParameterName
            {
                get
                {
                    return this._parameter.Name;
                }
                set
                {
                    this._parameter.Name = value;
                }
            }

            public TypeCode ParameterType
            {
                get
                {
                    return this._parameter.Type;
                }
                set
                {
                    this._parameter.Type = value;
                }
            }
        }

        private sealed class ProfileParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Windows.Forms.Label _propertyNameLabel;
            private System.Windows.Forms.TextBox _propertyNameTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public ProfileParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._propertyNameLabel = new System.Windows.Forms.Label();
                this._propertyNameTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._propertyNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._propertyNameLabel.Location = new Point(0, 0);
                this._propertyNameLabel.Size = new Size(400, 0x10);
                this._propertyNameLabel.TabIndex = 10;
                this._propertyNameLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ProfilePropertyName");
                this._propertyNameTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._propertyNameTextBox.Location = new Point(0, 0x12);
                this._propertyNameTextBox.Size = new Size(400, 20);
                this._propertyNameTextBox.TabIndex = 20;
                this._propertyNameTextBox.TextChanged += new EventHandler(this.OnPropertyNameTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2c);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3e);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x56);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._propertyNameLabel);
                base.Controls.Add(this._propertyNameTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
                this._propertyNameTextBox.Text = ((ProfileParameter) base.ParameterItem.Parameter).PropertyName;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnPropertyNameTextBoxTextChanged(object s, EventArgs e)
            {
                if (((ProfileParameter) base.ParameterItem.Parameter).PropertyName != this._propertyNameTextBox.Text)
                {
                    ((ProfileParameter) base.ParameterItem.Parameter).PropertyName = this._propertyNameTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._propertyNameTextBox.Focus();
            }
        }

        private class PropertyGridSite : ISite, IServiceProvider
        {
            private IComponent _comp;
            private bool _inGetService;
            private IServiceProvider _sp;

            public PropertyGridSite(IServiceProvider sp, IComponent comp)
            {
                this._sp = sp;
                this._comp = comp;
            }

            public object GetService(System.Type t)
            {
                if (!this._inGetService && (this._sp != null))
                {
                    try
                    {
                        this._inGetService = true;
                        return this._sp.GetService(t);
                    }
                    finally
                    {
                        this._inGetService = false;
                    }
                }
                return null;
            }

            public IComponent Component
            {
                get
                {
                    return this._comp;
                }
            }

            public IContainer Container
            {
                get
                {
                    return null;
                }
            }

            public bool DesignMode
            {
                get
                {
                    return false;
                }
            }

            public string Name
            {
                get
                {
                    return null;
                }
                set
                {
                }
            }
        }

        private sealed class QueryStringParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Windows.Forms.Label _queryStringFieldLabel;
            private System.Windows.Forms.TextBox _queryStringFieldTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public QueryStringParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._queryStringFieldLabel = new System.Windows.Forms.Label();
                this._queryStringFieldTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._queryStringFieldLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._queryStringFieldLabel.Location = new Point(0, 0);
                this._queryStringFieldLabel.Size = new Size(400, 0x10);
                this._queryStringFieldLabel.TabIndex = 10;
                this._queryStringFieldLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_QueryStringParameterQueryStringField");
                this._queryStringFieldTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._queryStringFieldTextBox.Location = new Point(0, 0x12);
                this._queryStringFieldTextBox.Size = new Size(400, 20);
                this._queryStringFieldTextBox.TabIndex = 20;
                this._queryStringFieldTextBox.TextChanged += new EventHandler(this.OnQueryStringFieldTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2c);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3e);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x56);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._queryStringFieldLabel);
                base.Controls.Add(this._queryStringFieldTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
                this._queryStringFieldTextBox.Text = ((QueryStringParameter) base.ParameterItem.Parameter).QueryStringField;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnQueryStringFieldTextBoxTextChanged(object s, EventArgs e)
            {
                if (((QueryStringParameter) base.ParameterItem.Parameter).QueryStringField != this._queryStringFieldTextBox.Text)
                {
                    ((QueryStringParameter) base.ParameterItem.Parameter).QueryStringField = this._queryStringFieldTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._queryStringFieldTextBox.Focus();
            }
        }

        private sealed class RouteParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Windows.Forms.Label _routeKeyLabel;
            private System.Windows.Forms.TextBox _routeKeyTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public RouteParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._routeKeyLabel = new System.Windows.Forms.Label();
                this._routeKeyTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._routeKeyLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._routeKeyLabel.Location = new Point(0, 0);
                this._routeKeyLabel.Size = new Size(400, 0x10);
                this._routeKeyLabel.TabIndex = 10;
                this._routeKeyLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_RouteParameterRouteKey");
                this._routeKeyTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._routeKeyTextBox.Location = new Point(0, 0x12);
                this._routeKeyTextBox.Size = new Size(400, 20);
                this._routeKeyTextBox.TabIndex = 20;
                this._routeKeyTextBox.TextChanged += new EventHandler(this.OnRouteKeyTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2c);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3e);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x56);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._routeKeyLabel);
                base.Controls.Add(this._routeKeyTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
                this._routeKeyTextBox.Text = ((RouteParameter) base.ParameterItem.Parameter).RouteKey;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnRouteKeyTextBoxTextChanged(object s, EventArgs e)
            {
                if (((RouteParameter) base.ParameterItem.Parameter).RouteKey != this._routeKeyTextBox.Text)
                {
                    ((RouteParameter) base.ParameterItem.Parameter).RouteKey = this._routeKeyTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._routeKeyTextBox.Focus();
            }
        }

        private sealed class SessionParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Windows.Forms.Label _sessionFieldLabel;
            private System.Windows.Forms.TextBox _sessionFieldTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public SessionParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._sessionFieldLabel = new System.Windows.Forms.Label();
                this._sessionFieldTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._sessionFieldLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._sessionFieldLabel.Location = new Point(0, 0);
                this._sessionFieldLabel.Size = new Size(400, 0x10);
                this._sessionFieldLabel.TabIndex = 10;
                this._sessionFieldLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_SessionParameterSessionField");
                this._sessionFieldTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._sessionFieldTextBox.Location = new Point(0, 0x12);
                this._sessionFieldTextBox.Size = new Size(400, 20);
                this._sessionFieldTextBox.TabIndex = 20;
                this._sessionFieldTextBox.TextChanged += new EventHandler(this.OnSessionFieldTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x2c);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x3e);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x56);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 50;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._sessionFieldLabel);
                base.Controls.Add(this._sessionFieldTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
                this._sessionFieldTextBox.Text = ((SessionParameter) base.ParameterItem.Parameter).SessionField;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnSessionFieldTextBoxTextChanged(object s, EventArgs e)
            {
                if (((SessionParameter) base.ParameterItem.Parameter).SessionField != this._sessionFieldTextBox.Text)
                {
                    ((SessionParameter) base.ParameterItem.Parameter).SessionField = this._sessionFieldTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._sessionFieldTextBox.Focus();
            }
        }

        private sealed class StaticParameterEditor : ParameterEditorUserControl.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private LinkLabel _showAdvancedLinkLabel;

            public StaticParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(400, 400);
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._showAdvancedLinkLabel = new LinkLabel();
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0);
                this._defaultValueLabel.Size = new Size(400, 0x10);
                this._defaultValueLabel.TabIndex = 10;
                this._defaultValueLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ParameterDefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x12);
                this._defaultValueTextBox.Size = new Size(400, 20);
                this._defaultValueTextBox.TabIndex = 20;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                this._showAdvancedLinkLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._showAdvancedLinkLabel.Location = new Point(0, 0x2a);
                this._showAdvancedLinkLabel.Size = new Size(400, 0x10);
                this._showAdvancedLinkLabel.TabIndex = 30;
                this._showAdvancedLinkLabel.TabStop = true;
                this._showAdvancedLinkLabel.Text = System.Design.SR.GetString("ParameterEditorUserControl_ShowAdvancedProperties");
                this._showAdvancedLinkLabel.Links.Add(new LinkLabel.Link(0, this._showAdvancedLinkLabel.Text.Length));
                this._showAdvancedLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnShowAdvancedLinkLabelLinkClicked);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                base.Controls.Add(this._showAdvancedLinkLabel);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void InitializeParameter(ParameterEditorUserControl.ParameterListViewItem parameterItem)
            {
                base.InitializeParameter(parameterItem);
                this._defaultValueTextBox.Text = base.ParameterItem.Parameter.DefaultValue;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                if (base.ParameterItem.Parameter.DefaultValue != this._defaultValueTextBox.Text)
                {
                    base.ParameterItem.Parameter.DefaultValue = this._defaultValueTextBox.Text;
                    base.OnParameterChanged();
                }
            }

            private void OnShowAdvancedLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.OnRequestModeChange();
            }

            public override void SetDefaultFocus()
            {
                this._defaultValueTextBox.Focus();
            }
        }
    }
}

