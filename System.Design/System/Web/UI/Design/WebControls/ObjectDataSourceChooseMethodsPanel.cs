namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal sealed class ObjectDataSourceChooseMethodsPanel : WizardPanel
    {
        private ObjectDataSourceMethodEditor _deleteObjectDataSourceMethodEditor;
        private TabPage _deleteTabPage;
        private ObjectDataSourceMethodEditor _insertObjectDataSourceMethodEditor;
        private TabPage _insertTabPage;
        private TabControl _methodsTabControl;
        private ObjectDataSource _objectDataSource;
        private ObjectDataSourceDesigner _objectDataSourceDesigner;
        private ObjectDataSourceMethodEditor _selectObjectDataSourceMethodEditor;
        private TabPage _selectTabPage;
        private ObjectDataSourceMethodEditor _updateObjectDataSourceMethodEditor;
        private TabPage _updateTabPage;

        public ObjectDataSourceChooseMethodsPanel(ObjectDataSourceDesigner objectDataSourceDesigner)
        {
            this._objectDataSourceDesigner = objectDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
            this._objectDataSource = (ObjectDataSource) this._objectDataSourceDesigner.Component;
        }

        private static MethodInfo[] GetMethods(System.Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            List<MethodInfo> list = new List<MethodInfo>();
            foreach (MethodInfo info in methods)
            {
                if (((info.GetBaseDefinition().DeclaringType != typeof(object)) & !info.IsSpecialName) && !info.IsAbstract)
                {
                    list.Add(info);
                }
            }
            return list.ToArray();
        }

        private void InitializeComponent()
        {
            this._methodsTabControl = new TabControl();
            this._selectTabPage = new TabPage();
            this._selectObjectDataSourceMethodEditor = new ObjectDataSourceMethodEditor();
            this._updateTabPage = new TabPage();
            this._updateObjectDataSourceMethodEditor = new ObjectDataSourceMethodEditor();
            this._insertTabPage = new TabPage();
            this._insertObjectDataSourceMethodEditor = new ObjectDataSourceMethodEditor();
            this._deleteTabPage = new TabPage();
            this._deleteObjectDataSourceMethodEditor = new ObjectDataSourceMethodEditor();
            this._methodsTabControl.SuspendLayout();
            this._selectTabPage.SuspendLayout();
            this._updateTabPage.SuspendLayout();
            this._insertTabPage.SuspendLayout();
            this._deleteTabPage.SuspendLayout();
            base.SuspendLayout();
            this._methodsTabControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._methodsTabControl.Controls.Add(this._selectTabPage);
            this._methodsTabControl.Controls.Add(this._updateTabPage);
            this._methodsTabControl.Controls.Add(this._insertTabPage);
            this._methodsTabControl.Controls.Add(this._deleteTabPage);
            this._methodsTabControl.Location = new Point(0, 0);
            this._methodsTabControl.Name = "_methodsTabControl";
            this._methodsTabControl.SelectedIndex = 0;
            this._methodsTabControl.ShowToolTips = true;
            this._methodsTabControl.Size = new Size(0x220, 0x112);
            this._methodsTabControl.TabIndex = 0;
            this._selectTabPage.Controls.Add(this._selectObjectDataSourceMethodEditor);
            this._selectTabPage.Location = new Point(4, 0x16);
            this._selectTabPage.Name = "_selectTabPage";
            this._selectTabPage.Size = new Size(0x218, 0xf8);
            this._selectTabPage.TabIndex = 10;
            this._selectTabPage.Text = "SELECT";
            this._selectObjectDataSourceMethodEditor.Dock = DockStyle.Fill;
            this._selectObjectDataSourceMethodEditor.Location = new Point(0, 0);
            this._selectObjectDataSourceMethodEditor.Name = "_selectObjectDataSourceMethodEditor";
            this._selectObjectDataSourceMethodEditor.TabIndex = 0;
            this._selectObjectDataSourceMethodEditor.MethodChanged += new EventHandler(this.OnSelectMethodChanged);
            this._updateTabPage.Controls.Add(this._updateObjectDataSourceMethodEditor);
            this._updateTabPage.Location = new Point(4, 0x16);
            this._updateTabPage.Name = "_updateTabPage";
            this._updateTabPage.Size = new Size(0x218, 0xf8);
            this._updateTabPage.TabIndex = 20;
            this._updateTabPage.Text = "UPDATE";
            this._updateObjectDataSourceMethodEditor.Dock = DockStyle.Fill;
            this._updateObjectDataSourceMethodEditor.Location = new Point(0, 0);
            this._updateObjectDataSourceMethodEditor.Name = "_updateObjectDataSourceMethodEditor";
            this._updateObjectDataSourceMethodEditor.TabIndex = 0;
            this._insertTabPage.Controls.Add(this._insertObjectDataSourceMethodEditor);
            this._insertTabPage.Location = new Point(4, 0x16);
            this._insertTabPage.Name = "_insertTabPage";
            this._insertTabPage.Size = new Size(0x218, 0xf8);
            this._insertTabPage.TabIndex = 30;
            this._insertTabPage.Text = "INSERT";
            this._insertObjectDataSourceMethodEditor.Dock = DockStyle.Fill;
            this._insertObjectDataSourceMethodEditor.Location = new Point(0, 0);
            this._insertObjectDataSourceMethodEditor.Name = "_insertObjectDataSourceMethodEditor";
            this._insertObjectDataSourceMethodEditor.TabIndex = 0;
            this._deleteTabPage.Controls.Add(this._deleteObjectDataSourceMethodEditor);
            this._deleteTabPage.Location = new Point(4, 0x16);
            this._deleteTabPage.Name = "_deleteTabPage";
            this._deleteTabPage.Size = new Size(0x218, 0xf8);
            this._deleteTabPage.TabIndex = 40;
            this._deleteTabPage.Text = "DELETE";
            this._deleteObjectDataSourceMethodEditor.Dock = DockStyle.Fill;
            this._deleteObjectDataSourceMethodEditor.Location = new Point(0, 0);
            this._deleteObjectDataSourceMethodEditor.Name = "_deleteObjectDataSourceMethodEditor";
            this._deleteObjectDataSourceMethodEditor.TabIndex = 0;
            base.Controls.Add(this._methodsTabControl);
            base.Name = "ObjectDataSourceChooseMethodsPanel";
            base.Size = new Size(0x220, 0x112);
            this._methodsTabControl.ResumeLayout(false);
            this._selectTabPage.ResumeLayout(false);
            this._updateTabPage.ResumeLayout(false);
            this._insertTabPage.ResumeLayout(false);
            this._deleteTabPage.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            base.Caption = System.Design.SR.GetString("ObjectDataSourceChooseMethodsPanel_PanelCaption");
        }

        protected internal override void OnComplete()
        {
            PropertyDescriptor descriptor;
            MethodInfo deleteMethodInfo = this.DeleteMethodInfo;
            string str = (deleteMethodInfo == null) ? string.Empty : deleteMethodInfo.Name;
            if (this._objectDataSource.DeleteMethod != str)
            {
                descriptor = TypeDescriptor.GetProperties(this._objectDataSource)["DeleteMethod"];
                descriptor.ResetValue(this._objectDataSource);
                descriptor.SetValue(this._objectDataSource, str);
            }
            deleteMethodInfo = this.InsertMethodInfo;
            str = (deleteMethodInfo == null) ? string.Empty : deleteMethodInfo.Name;
            if (this._objectDataSource.InsertMethod != str)
            {
                descriptor = TypeDescriptor.GetProperties(this._objectDataSource)["InsertMethod"];
                descriptor.ResetValue(this._objectDataSource);
                descriptor.SetValue(this._objectDataSource, str);
            }
            deleteMethodInfo = this.SelectMethodInfo;
            str = (deleteMethodInfo == null) ? string.Empty : deleteMethodInfo.Name;
            if (this._objectDataSource.SelectMethod != str)
            {
                descriptor = TypeDescriptor.GetProperties(this._objectDataSource)["SelectMethod"];
                descriptor.ResetValue(this._objectDataSource);
                descriptor.SetValue(this._objectDataSource, str);
            }
            deleteMethodInfo = this.UpdateMethodInfo;
            str = (deleteMethodInfo == null) ? string.Empty : deleteMethodInfo.Name;
            if (this._objectDataSource.UpdateMethod != str)
            {
                descriptor = TypeDescriptor.GetProperties(this._objectDataSource)["UpdateMethod"];
                descriptor.ResetValue(this._objectDataSource);
                descriptor.SetValue(this._objectDataSource, str);
            }
            this._objectDataSource.SelectParameters.Clear();
            deleteMethodInfo = this.SelectMethodInfo;
            try
            {
                IDataSourceSchema schema = new TypeSchema(deleteMethodInfo.ReturnType);
                if (schema != null)
                {
                    IDataSourceViewSchema[] views = schema.GetViews();
                    if ((views != null) && (views.Length > 0))
                    {
                        views[0].GetFields();
                    }
                }
            }
            catch (Exception)
            {
            }
            ObjectDataSourceDesigner.MergeParameters(this._objectDataSource.DeleteParameters, this.DeleteMethodInfo, this.DeleteMethodDataObjectType);
            ObjectDataSourceDesigner.MergeParameters(this._objectDataSource.InsertParameters, this.InsertMethodInfo, this.InsertMethodDataObjectType);
            ObjectDataSourceDesigner.MergeParameters(this._objectDataSource.UpdateParameters, this.UpdateMethodInfo, this.UpdateMethodDataObjectType);
            string fullName = string.Empty;
            if (this.DeleteMethodDataObjectType != null)
            {
                fullName = this.DeleteMethodDataObjectType.FullName;
            }
            else if (this.InsertMethodDataObjectType != null)
            {
                fullName = this.InsertMethodDataObjectType.FullName;
            }
            else if (this.UpdateMethodDataObjectType != null)
            {
                fullName = this.UpdateMethodDataObjectType.FullName;
            }
            if (this._objectDataSource.DataObjectTypeName != fullName)
            {
                descriptor = TypeDescriptor.GetProperties(this._objectDataSource)["DataObjectTypeName"];
                descriptor.ResetValue(this._objectDataSource);
                descriptor.SetValue(this._objectDataSource, fullName);
            }
            if (deleteMethodInfo != null)
            {
                this._objectDataSourceDesigner.RefreshSchema(deleteMethodInfo.ReflectedType, deleteMethodInfo.Name, deleteMethodInfo.ReturnType, true);
            }
        }

        public override bool OnNext()
        {
            List<System.Type> list = new List<System.Type>();
            System.Type deleteMethodDataObjectType = this.DeleteMethodDataObjectType;
            if (deleteMethodDataObjectType != null)
            {
                list.Add(deleteMethodDataObjectType);
            }
            System.Type insertMethodDataObjectType = this.InsertMethodDataObjectType;
            if (insertMethodDataObjectType != null)
            {
                list.Add(insertMethodDataObjectType);
            }
            System.Type updateMethodDataObjectType = this.UpdateMethodDataObjectType;
            if (updateMethodDataObjectType != null)
            {
                list.Add(updateMethodDataObjectType);
            }
            if (list.Count > 1)
            {
                System.Type type4 = list[0];
                for (int i = 1; i < list.Count; i++)
                {
                    if (type4 != list[i])
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("ObjectDataSourceChooseMethodsPanel_IncompatibleDataObjectTypes"));
                        return false;
                    }
                }
            }
            MethodInfo selectMethodInfo = this.SelectMethodInfo;
            if (selectMethodInfo == null)
            {
                return false;
            }
            if (selectMethodInfo.GetParameters().Length > 0)
            {
                ObjectDataSourceConfigureParametersPanel nextPanel = base.NextPanel as ObjectDataSourceConfigureParametersPanel;
                if (nextPanel == null)
                {
                    nextPanel = ((ObjectDataSourceWizardForm) base.ParentWizard).GetParametersPanel();
                    base.NextPanel = nextPanel;
                    nextPanel.InitializeParameters(this._objectDataSource.SelectParameters);
                }
                nextPanel.SetMethod(this.SelectMethodInfo);
            }
            return true;
        }

        public override void OnPrevious()
        {
        }

        private void OnSelectMethodChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledState();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                this.UpdateEnabledState();
            }
        }

        public void SetType(System.Type type)
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                MethodInfo[] methods = GetMethods(type);
                this._methodsTabControl.SelectedIndex = 0;
                System.Type dataObjectType = ObjectDataSourceDesigner.GetType(base.ServiceProvider, this._objectDataSource.DataObjectTypeName, true);
                this._selectObjectDataSourceMethodEditor.SetMethodInformation(methods, this._objectDataSource.SelectMethod, this._objectDataSource.SelectParameters, DataObjectMethodType.Select, dataObjectType);
                this._insertObjectDataSourceMethodEditor.SetMethodInformation(methods, this._objectDataSource.InsertMethod, this._objectDataSource.InsertParameters, DataObjectMethodType.Insert, dataObjectType);
                this._updateObjectDataSourceMethodEditor.SetMethodInformation(methods, this._objectDataSource.UpdateMethod, this._objectDataSource.UpdateParameters, DataObjectMethodType.Update, dataObjectType);
                this._deleteObjectDataSourceMethodEditor.SetMethodInformation(methods, this._objectDataSource.DeleteMethod, this._objectDataSource.DeleteParameters, DataObjectMethodType.Delete, dataObjectType);
            }
            finally
            {
                Cursor.Current = current;
            }
            this.UpdateEnabledState();
        }

        private void UpdateEnabledState()
        {
            MethodInfo selectMethodInfo = this.SelectMethodInfo;
            if (selectMethodInfo != null)
            {
                bool flag = selectMethodInfo.GetParameters().Length > 0;
                base.ParentWizard.NextButton.Enabled = flag;
                base.ParentWizard.FinishButton.Enabled = !flag;
            }
            else
            {
                base.ParentWizard.NextButton.Enabled = false;
                base.ParentWizard.FinishButton.Enabled = false;
            }
        }

        private System.Type DeleteMethodDataObjectType
        {
            get
            {
                return this._deleteObjectDataSourceMethodEditor.DataObjectType;
            }
        }

        private MethodInfo DeleteMethodInfo
        {
            get
            {
                return this._deleteObjectDataSourceMethodEditor.MethodInfo;
            }
        }

        private System.Type InsertMethodDataObjectType
        {
            get
            {
                return this._insertObjectDataSourceMethodEditor.DataObjectType;
            }
        }

        private MethodInfo InsertMethodInfo
        {
            get
            {
                return this._insertObjectDataSourceMethodEditor.MethodInfo;
            }
        }

        private MethodInfo SelectMethodInfo
        {
            get
            {
                return this._selectObjectDataSourceMethodEditor.MethodInfo;
            }
        }

        private System.Type UpdateMethodDataObjectType
        {
            get
            {
                return this._updateObjectDataSourceMethodEditor.DataObjectType;
            }
        }

        private MethodInfo UpdateMethodInfo
        {
            get
            {
                return this._updateObjectDataSourceMethodEditor.MethodInfo;
            }
        }
    }
}

