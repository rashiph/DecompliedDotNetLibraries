namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class ActivityBindForm : Form
    {
        private string ActivityBindDialogTitleFormat;
        private ActivityBind binding;
        private System.Windows.Forms.TabControl bindTabControl;
        private System.Type boundType;
        private TableLayoutPanel buttonTableLayoutPanel;
        private Button cancelButton;
        private IContainer components;
        private ITypeDescriptorContext context;
        private RadioButton createField;
        private bool createNew;
        private string CreateNewMemberHelpFormat;
        private bool createNewProperty;
        private RadioButton createProperty;
        private string DescriptionFormat;
        private Panel dummyPanel;
        private string EditIndex;
        private TabPage existingMemberPage;
        private GroupBox groupBox1;
        private TextBox helpTextBox;
        private string IncorrectIndexChange;
        private Label memberNameLabel;
        private TextBox memberNameTextBox;
        private const string MemberTypeFormat = "MemberType#{0}";
        private ImageList memberTypes;
        private TextBox newMemberHelpTextBox;
        private string newMemberName = string.Empty;
        private TabPage newMemberPage;
        private Button OKButton;
        private string PleaseSelectActivityProperty;
        private string PleaseSelectCorrectActivityProperty;
        private List<CustomProperty> properties;
        private string PropertyAssignableFormat;
        private IServiceProvider serviceProvider;
        private ActivityBindFormWorkflowOutline workflowOutline;

        public ActivityBindForm(IServiceProvider serviceProvider, ITypeDescriptorContext context)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
            this.InitializeComponent();
            this.createProperty.Checked = true;
            this.helpTextBox.Multiline = true;
            IUIService service = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
            if (service != null)
            {
                this.Font = (Font) service.Styles["DialogFont"];
            }
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ActivityBindForm));
            this.ActivityBindDialogTitleFormat = manager.GetString("ActivityBindDialogTitleFormat");
            this.PropertyAssignableFormat = manager.GetString("PropertyAssignableFormat");
            this.DescriptionFormat = manager.GetString("DescriptionFormat");
            this.EditIndex = manager.GetString("EditIndex");
            this.PleaseSelectCorrectActivityProperty = manager.GetString("PleaseSelectCorrectActivityProperty");
            this.PleaseSelectActivityProperty = manager.GetString("PleaseSelectActivityProperty");
            this.IncorrectIndexChange = manager.GetString("IncorrectIndexChange");
            this.CreateNewMemberHelpFormat = manager.GetString("CreateNewMemberHelpFormat");
            this.memberTypes = new ImageList();
            this.memberTypes.ImageStream = (ImageListStreamer) manager.GetObject("memberTypes.ImageStream");
            this.memberTypes.TransparentColor = AmbientTheme.TransparentColor;
            this.properties = CustomActivityDesignerHelper.GetCustomProperties(context);
        }

        private void ActivityBindForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.GetHelp();
        }

        private void ActivityBindForm_Load(object sender, EventArgs e)
        {
            this.Text = string.Format(CultureInfo.CurrentCulture, this.ActivityBindDialogTitleFormat, new object[] { this.context.PropertyDescriptor.Name });
            if (this.context.PropertyDescriptor is DynamicPropertyDescriptor)
            {
                this.boundType = PropertyDescriptorUtils.GetBaseType(this.context.PropertyDescriptor, PropertyDescriptorUtils.GetComponent(this.context), this.serviceProvider);
            }
            if (this.boundType != null)
            {
                ITypeProvider provider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (provider != null)
                {
                    System.Type type = provider.GetType(this.boundType.FullName, false);
                    this.boundType = (type != null) ? type : this.boundType;
                }
            }
            this.workflowOutline = new ActivityBindFormWorkflowOutline(this.serviceProvider, this);
            this.dummyPanel.BorderStyle = BorderStyle.None;
            this.dummyPanel.SuspendLayout();
            this.dummyPanel.Controls.Add(this.workflowOutline);
            this.workflowOutline.Location = new Point(3, 3);
            this.workflowOutline.Size = new Size(0xc7, 0x15f);
            this.workflowOutline.Dock = DockStyle.Fill;
            this.dummyPanel.ResumeLayout(false);
            this.workflowOutline.AddMemberKindImages(this.memberTypes);
            this.workflowOutline.ReloadWorkflowOutline();
            this.workflowOutline.ExpandRootNode();
            Activity component = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
            if (component == null)
            {
                IReferenceService service = this.context.GetService(typeof(IReferenceService)) as IReferenceService;
                if (service != null)
                {
                    component = service.GetComponent(this.context.Instance) as Activity;
                }
            }
            ActivityBind bind = this.context.PropertyDescriptor.GetValue(this.context.Instance) as ActivityBind;
            if ((component != null) && (bind != null))
            {
                Activity activity = Helpers.ParseActivity(Helpers.GetRootActivity(component), bind.Name);
                if (activity != null)
                {
                    this.workflowOutline.SelectActivity(activity, this.ParseStringPath(this.GetActivityType(activity), bind.Path));
                }
            }
            if (this.properties != null)
            {
                List<string> list = new List<string>();
                foreach (CustomProperty property in this.properties)
                {
                    list.Add(property.Name);
                }
                this.memberNameTextBox.Text = DesignerHelpers.GenerateUniqueIdentifier(this.serviceProvider, component.Name + "_" + this.context.PropertyDescriptor.Name, list.ToArray());
            }
            this.newMemberHelpTextBox.Lines = string.Format(CultureInfo.CurrentCulture, this.CreateNewMemberHelpFormat, new object[] { this.GetSimpleTypeFullName(this.boundType) }).Split(new char[] { '\n' });
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
        }

        private string ConstructIndexString(MethodInfo getterMethod)
        {
            StringBuilder builder = new StringBuilder();
            ParameterInfo[] parameters = getterMethod.GetParameters();
            if ((parameters != null) && (parameters.Length > 0))
            {
                builder.Append("[");
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo info = parameters[i];
                    string indexerString = this.GetIndexerString(info.ParameterType);
                    if (indexerString == null)
                    {
                        return null;
                    }
                    builder.Append(indexerString);
                    if (i < (parameters.Length - 1))
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("]");
            }
            return builder.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Type GetActivityType(Activity activity)
        {
            System.Type type = null;
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (((service != null) && (loader != null)) && (activity.Parent == null))
            {
                ITypeProvider provider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (provider != null)
                {
                    type = provider.GetType(service.RootComponentClassName, false);
                }
            }
            if (type == null)
            {
                type = activity.GetType();
            }
            return type;
        }

        private PathInfo[] GetArraySubProperties(System.Type propertyType, string currentPath)
        {
            List<PathInfo> list = new List<PathInfo>();
            if (propertyType != typeof(string))
            {
                List<MethodInfo> list2 = new List<MethodInfo>();
                MemberInfo[] defaultMembers = null;
                try
                {
                    defaultMembers = propertyType.GetDefaultMembers();
                }
                catch (NotImplementedException)
                {
                }
                catch (ArgumentException)
                {
                }
                if ((defaultMembers != null) && (defaultMembers.Length > 0))
                {
                    foreach (MemberInfo info in defaultMembers)
                    {
                        if (info is PropertyInfo)
                        {
                            list2.Add((info as PropertyInfo).GetGetMethod());
                        }
                    }
                }
                if (propertyType.IsArray)
                {
                    MemberInfo[] member = propertyType.GetMember("Get");
                    if ((member != null) && (member.Length > 0))
                    {
                        foreach (MemberInfo info2 in member)
                        {
                            if (info2 is MethodInfo)
                            {
                                list2.Add(info2 as MethodInfo);
                            }
                        }
                    }
                }
                foreach (MethodInfo info3 in list2)
                {
                    string str = this.ConstructIndexString(info3);
                    if (str != null)
                    {
                        list.Add(new PathInfo(currentPath + str, info3, info3.ReturnType));
                    }
                }
            }
            return list.ToArray();
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(ActivityBindForm).FullName + ".UI");
        }

        private string GetIndexerString(System.Type indexType)
        {
            object obj2 = null;
            if (IsTypePrimitive(indexType))
            {
                try
                {
                    obj2 = Activator.CreateInstance(indexType);
                }
                catch
                {
                    obj2 = null;
                }
            }
            else if (indexType == typeof(string))
            {
                obj2 = "\"<name>\"";
            }
            if (obj2 == null)
            {
                return null;
            }
            return obj2.ToString();
        }

        private string GetMemberDescription(MemberInfo member)
        {
            object[] customAttributes = member.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length <= 0))
            {
                return string.Empty;
            }
            DescriptionAttribute attribute = customAttributes[0] as DescriptionAttribute;
            if (attribute == null)
            {
                return string.Empty;
            }
            return attribute.Description;
        }

        private PropertyInfo[] GetProperties(System.Type type)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            list.AddRange(type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
            if (type.IsInterface)
            {
                foreach (System.Type type2 in type.GetInterfaces())
                {
                    list.AddRange(type2.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance));
                }
            }
            return list.ToArray();
        }

        private string GetSimpleTypeFullName(System.Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(type.FullName);
            Stack<System.Type> stack = new Stack<System.Type>();
            stack.Push(type);
            while (stack.Count > 0)
            {
                type = stack.Pop();
                while (type.IsArray)
                {
                    type = type.GetElementType();
                }
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    foreach (System.Type type2 in type.GetGenericArguments())
                    {
                        builder.Replace("[" + type2.AssemblyQualifiedName + "]", this.GetSimpleTypeFullName(type2));
                        stack.Push(type2);
                    }
                }
            }
            return builder.ToString();
        }

        private PathInfo[] GetSubPropertiesOnType(System.Type typeToGetPropertiesOn, string currentPath)
        {
            List<PathInfo> list = new List<PathInfo>();
            if ((typeToGetPropertiesOn != typeof(string)) && (!TypeProvider.IsAssignable(typeof(Delegate), typeToGetPropertiesOn) || this.boundType.IsSubclassOf(typeof(Delegate))))
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? string.Empty : (currentPath + ".");
                ITypeProvider service = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                foreach (PropertyInfo info in this.GetProperties(typeToGetPropertiesOn))
                {
                    MethodInfo getMethod = info.GetGetMethod();
                    System.Type memberType = BindHelpers.GetMemberType(info);
                    if (memberType != null)
                    {
                        if (service != null)
                        {
                            System.Type type = service.GetType(memberType.FullName, false);
                            memberType = (type != null) ? type : memberType;
                        }
                        if (((this.IsPropertyBrowsable(info) && (getMethod != null)) && (memberType != null)) && ((!IsTypePrimitive(memberType) || TypeProvider.IsAssignable(this.boundType, memberType)) && (!(this.boundType != typeof(object)) || !(memberType == typeof(object)))))
                        {
                            string name = info.Name;
                            name = currentPath + name + this.ConstructIndexString(getMethod);
                            list.Add(new PathInfo(name, info, memberType));
                            list.AddRange(this.GetArraySubProperties(memberType, name));
                        }
                    }
                }
                foreach (FieldInfo info3 in typeToGetPropertiesOn.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    System.Type fromType = BindHelpers.GetMemberType(info3);
                    if ((fromType != null) && !TypeProvider.IsAssignable(typeof(DependencyProperty), fromType))
                    {
                        if (service != null)
                        {
                            System.Type type4 = service.GetType(fromType.FullName, false);
                            fromType = (type4 != null) ? type4 : fromType;
                        }
                        if (((this.IsPropertyBrowsable(info3) && (fromType != null)) && (!IsTypePrimitive(fromType) || TypeProvider.IsAssignable(this.boundType, fromType))) && ((!(this.boundType != typeof(object)) || !(fromType == typeof(object))) && (TypeProvider.IsAssignable(typeof(Delegate), this.boundType) || !TypeProvider.IsAssignable(typeof(Delegate), fromType))))
                        {
                            string path = currentPath + info3.Name;
                            list.Add(new PathInfo(path, info3, BindHelpers.GetMemberType(info3)));
                            list.AddRange(this.GetArraySubProperties(fromType, path));
                        }
                    }
                }
                if (this.boundType.IsSubclassOf(typeof(Delegate)))
                {
                    foreach (EventInfo info4 in typeToGetPropertiesOn.GetEvents(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                    {
                        System.Type type5 = BindHelpers.GetMemberType(info4);
                        if (type5 != null)
                        {
                            if (service != null)
                            {
                                System.Type type6 = service.GetType(type5.FullName, false);
                                type5 = (type6 != null) ? type6 : type5;
                            }
                            if ((this.IsPropertyBrowsable(info4) && (type5 != null)) && TypeProvider.IsAssignable(this.boundType, type5))
                            {
                                list.Add(new PathInfo(currentPath + info4.Name, info4, type5));
                            }
                        }
                    }
                }
            }
            return list.ToArray();
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ActivityBindForm));
            this.dummyPanel = new Panel();
            this.cancelButton = new Button();
            this.OKButton = new Button();
            this.buttonTableLayoutPanel = new TableLayoutPanel();
            this.helpTextBox = new TextBox();
            this.createField = new RadioButton();
            this.createProperty = new RadioButton();
            this.groupBox1 = new GroupBox();
            this.bindTabControl = new System.Windows.Forms.TabControl();
            this.existingMemberPage = new TabPage();
            this.newMemberPage = new TabPage();
            this.newMemberHelpTextBox = new TextBox();
            this.memberNameLabel = new Label();
            this.memberNameTextBox = new TextBox();
            this.buttonTableLayoutPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.bindTabControl.SuspendLayout();
            this.existingMemberPage.SuspendLayout();
            this.newMemberPage.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.dummyPanel, "dummyPanel");
            this.dummyPanel.BorderStyle = BorderStyle.FixedSingle;
            this.dummyPanel.Name = "dummyPanel";
            this.cancelButton.DialogResult = DialogResult.Cancel;
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
            manager.ApplyResources(this.OKButton, "OKButton");
            this.OKButton.Name = "OKButton";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new EventHandler(this.OKButton_Click);
            manager.ApplyResources(this.buttonTableLayoutPanel, "buttonTableLayoutPanel");
            this.buttonTableLayoutPanel.Controls.Add(this.OKButton, 0, 0);
            this.buttonTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.buttonTableLayoutPanel.Name = "buttonTableLayoutPanel";
            manager.ApplyResources(this.helpTextBox, "helpTextBox");
            this.helpTextBox.Name = "helpTextBox";
            this.helpTextBox.ReadOnly = true;
            manager.ApplyResources(this.createField, "createField");
            this.createField.Checked = true;
            this.createField.Name = "createField";
            this.createField.TabStop = true;
            this.createField.UseVisualStyleBackColor = true;
            manager.ApplyResources(this.createProperty, "createProperty");
            this.createProperty.Name = "createProperty";
            this.createProperty.UseVisualStyleBackColor = true;
            manager.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.createField);
            this.groupBox1.Controls.Add(this.createProperty);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            manager.ApplyResources(this.bindTabControl, "bindTabControl");
            this.bindTabControl.Controls.Add(this.existingMemberPage);
            this.bindTabControl.Controls.Add(this.newMemberPage);
            this.bindTabControl.Name = "bindTabControl";
            this.bindTabControl.SelectedIndex = 0;
            this.existingMemberPage.Controls.Add(this.dummyPanel);
            this.existingMemberPage.Controls.Add(this.helpTextBox);
            manager.ApplyResources(this.existingMemberPage, "existingMemberPage");
            this.existingMemberPage.Name = "existingMemberPage";
            this.existingMemberPage.UseVisualStyleBackColor = true;
            this.newMemberPage.Controls.Add(this.memberNameLabel);
            this.newMemberPage.Controls.Add(this.memberNameTextBox);
            this.newMemberPage.Controls.Add(this.groupBox1);
            this.newMemberPage.Controls.Add(this.newMemberHelpTextBox);
            manager.ApplyResources(this.newMemberPage, "newMemberPage");
            this.newMemberPage.Name = "newMemberPage";
            this.newMemberPage.UseVisualStyleBackColor = true;
            manager.ApplyResources(this.newMemberHelpTextBox, "newMemberHelpTextBox");
            this.newMemberHelpTextBox.Name = "newMemberHelpTextBox";
            this.newMemberHelpTextBox.ReadOnly = true;
            manager.ApplyResources(this.memberNameLabel, "memberNameLabel");
            this.memberNameLabel.Name = "memberNameLabel";
            manager.ApplyResources(this.memberNameTextBox, "memberNameTextBox");
            this.memberNameTextBox.Name = "memberNameTextBox";
            base.AcceptButton = this.OKButton;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.cancelButton;
            base.Controls.Add(this.bindTabControl);
            base.Controls.Add(this.buttonTableLayoutPanel);
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ActivityBindForm";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Show;
            base.HelpButtonClicked += new CancelEventHandler(this.ActivityBindForm_HelpButtonClicked);
            base.Load += new EventHandler(this.ActivityBindForm_Load);
            this.buttonTableLayoutPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.bindTabControl.ResumeLayout(false);
            this.existingMemberPage.ResumeLayout(false);
            this.existingMemberPage.PerformLayout();
            this.newMemberPage.ResumeLayout(false);
            this.newMemberPage.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool IsPropertyBrowsable(MemberInfo property)
        {
            object[] customAttributes = property.GetCustomAttributes(typeof(BrowsableAttribute), false);
            if (customAttributes.Length > 0)
            {
                BrowsableAttribute attribute = customAttributes[0] as BrowsableAttribute;
                if (attribute != null)
                {
                    return attribute.Browsable;
                }
                AttributeInfoAttribute attribute2 = customAttributes[0] as AttributeInfoAttribute;
                if (attribute2 != null)
                {
                    ReadOnlyCollection<object> argumentValues = attribute2.AttributeInfo.ArgumentValues;
                    if (argumentValues.Count > 0)
                    {
                        return Convert.ToBoolean(argumentValues[0], CultureInfo.InvariantCulture);
                    }
                }
            }
            return true;
        }

        private static bool IsTypePrimitive(System.Type type)
        {
            if (((!type.IsPrimitive && !type.IsEnum) && (!(type == typeof(Guid)) && !(type == typeof(IntPtr)))) && (!(type == typeof(string)) && !(type == typeof(DateTime))))
            {
                return (type == typeof(TimeSpan));
            }
            return true;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.None;
            this.createNew = this.bindTabControl.SelectedIndex != this.bindTabControl.TabPages.IndexOf(this.existingMemberPage);
            if (this.createNew)
            {
                this.createNewProperty = this.createProperty.Checked;
                this.newMemberName = this.memberNameTextBox.Text;
                base.DialogResult = this.ValidateNewMemberBind(this.newMemberName);
            }
            else
            {
                base.DialogResult = this.ValidateExistingPropertyBind();
            }
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            e.Handled = true;
            this.GetHelp();
        }

        private List<PathInfo> ParseStringPath(System.Type activityType, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            List<PathInfo> pathInfoList = new List<PathInfo>();
            PathWalker walker = new PathWalker();
            PathMemberInfoEventArgs finalEventArgs = null;
            PathErrorInfoEventArgs errorEventArgs = null;
            walker.MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                finalEventArgs = eventArgs;
                pathInfoList.Add(new PathInfo(eventArgs.Path, eventArgs.MemberInfo, BindHelpers.GetMemberType(eventArgs.MemberInfo)));
            });
            walker.PathErrorFound = (EventHandler<PathErrorInfoEventArgs>) Delegate.Combine(walker.PathErrorFound, delegate (object sender, PathErrorInfoEventArgs eventArgs) {
                errorEventArgs = eventArgs;
            });
            walker.TryWalkPropertyPath(activityType, path);
            return pathInfoList;
        }

        private List<PathInfo> PopulateAutoCompleteList(Activity activity, PathInfo path)
        {
            List<PathInfo> list = new List<PathInfo>();
            System.Type activityType = this.GetActivityType(activity);
            PathInfo[] collection = (activityType != null) ? this.ProcessPaths(activityType, path) : null;
            if (collection != null)
            {
                list.AddRange(collection);
            }
            return list;
        }

        private PathInfo[] ProcessPaths(System.Type activityType, PathInfo topProperty)
        {
            List<PathInfo> list = new List<PathInfo>();
            if (topProperty == null)
            {
                list.AddRange(this.GetSubPropertiesOnType(activityType, string.Empty));
            }
            else
            {
                list.AddRange(this.GetSubPropertiesOnType(topProperty.PropertyType, topProperty.Path));
            }
            return list.ToArray();
        }

        private void SelectedActivityChanged(Activity activity, PathInfo memberPathInfo, string path)
        {
            string str = string.Empty;
            string simpleTypeFullName = this.GetSimpleTypeFullName(this.boundType);
            if (memberPathInfo != null)
            {
                if ((path == null) || (path.Length == 0))
                {
                    str = string.Format(CultureInfo.CurrentCulture, this.PleaseSelectActivityProperty, new object[] { simpleTypeFullName });
                }
                else
                {
                    MemberActivityBindTreeNode.MemberName(memberPathInfo.Path);
                    string str3 = this.GetSimpleTypeFullName(memberPathInfo.PropertyType);
                    string memberDescription = this.GetMemberDescription(memberPathInfo.MemberInfo);
                    if (TypeProvider.IsAssignable(this.boundType, memberPathInfo.PropertyType))
                    {
                        str = string.Format(CultureInfo.CurrentCulture, this.PropertyAssignableFormat, new object[] { str3, simpleTypeFullName }) + ((memberDescription.Length > 0) ? string.Format(CultureInfo.CurrentCulture, this.DescriptionFormat, new object[] { memberDescription }) : string.Empty);
                    }
                    else
                    {
                        str = string.Format(CultureInfo.CurrentCulture, this.PleaseSelectCorrectActivityProperty, new object[] { simpleTypeFullName, str3 });
                    }
                    str = str + ((MemberActivityBindTreeNode.MemberName(path).IndexOfAny(new char[] { '[', ']' }) != -1) ? this.EditIndex : string.Empty);
                }
            }
            else
            {
                str = string.Format(CultureInfo.CurrentCulture, this.PleaseSelectActivityProperty, new object[] { simpleTypeFullName });
            }
            this.helpTextBox.Lines = str.Split(new char[] { '\n' });
        }

        private DialogResult ValidateExistingPropertyBind()
        {
            ValidationErrorCollection errors;
            Activity selectedActivity = this.workflowOutline.SelectedActivity;
            PathInfo selectedMember = this.workflowOutline.SelectedMember;
            string propertyPath = this.workflowOutline.PropertyPath;
            if ((selectedActivity == null) || (selectedMember == null))
            {
                string message = SR.GetString("Error_BindDialogNoValidPropertySelected", new object[] { this.GetSimpleTypeFullName(this.boundType) });
                DesignerHelpers.ShowError(this.serviceProvider, message);
                return DialogResult.None;
            }
            System.Type propertyType = selectedMember.PropertyType;
            ITypeProvider service = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if ((service != null) && (propertyType != null))
            {
                System.Type type = service.GetType(propertyType.FullName, false);
                propertyType = (type != null) ? type : propertyType;
            }
            if ((this.boundType != propertyType) && !TypeProvider.IsAssignable(this.boundType, propertyType))
            {
                string str3 = SR.GetString("Error_BindDialogWrongPropertyType", new object[] { this.GetSimpleTypeFullName(propertyType), this.GetSimpleTypeFullName(this.boundType) });
                DesignerHelpers.ShowError(this.serviceProvider, str3);
                return DialogResult.None;
            }
            Activity component = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
            string name = this.context.PropertyDescriptor.Name;
            if (((component == selectedActivity) && (selectedMember != null)) && selectedMember.Path.Equals(name, StringComparison.Ordinal))
            {
                DesignerHelpers.ShowError(this.serviceProvider, SR.GetString("Error_BindDialogCanNotBindToItself"));
                return DialogResult.None;
            }
            if ((selectedActivity == null) || (selectedMember == null))
            {
                return DialogResult.None;
            }
            ActivityBind bind = new ActivityBind(selectedActivity.QualifiedName, propertyPath);
            ValidationManager serviceProvider = new ValidationManager(this.serviceProvider);
            PropertyValidationContext propertyValidationContext = new PropertyValidationContext(this.context.Instance, DependencyProperty.FromName(this.context.PropertyDescriptor.Name, this.context.Instance.GetType()));
            serviceProvider.Context.Append(this.context.Instance);
            using (WorkflowCompilationContext.CreateScope(serviceProvider))
            {
                errors = ValidationHelpers.ValidateProperty(serviceProvider, component, bind, propertyValidationContext);
            }
            if (((errors != null) && (errors.Count > 0)) && errors.HasErrors)
            {
                string str5 = string.Empty;
                for (int i = 0; i < errors.Count; i++)
                {
                    ValidationError error = errors[i];
                    str5 = str5 + error.ErrorText + ((i == (errors.Count - 1)) ? string.Empty : "; ");
                }
                str5 = SR.GetString("Error_BindDialogBindNotValid") + str5;
                DesignerHelpers.ShowError(this.serviceProvider, str5);
                return DialogResult.None;
            }
            this.binding = bind;
            return DialogResult.OK;
        }

        private DialogResult ValidateNewMemberBind(string newMemberName)
        {
            Activity component = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
            if (component == null)
            {
                IReferenceService service = this.context.GetService(typeof(IReferenceService)) as IReferenceService;
                if (service != null)
                {
                    component = service.GetComponent(this.context.Instance) as Activity;
                }
            }
            string message = null;
            try
            {
                ValidationHelpers.ValidateIdentifier(this.context, newMemberName);
            }
            catch
            {
                message = SR.GetString("Error_InvalidLanguageIdentifier", new object[] { newMemberName });
            }
            System.Type customActivityType = CustomActivityDesignerHelper.GetCustomActivityType(this.context);
            SupportedLanguages supportedLanguage = CompilerHelpers.GetSupportedLanguage(this.context);
            foreach (MemberInfo info in customActivityType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (string.Compare(info.Name, newMemberName, supportedLanguage == SupportedLanguages.VB, CultureInfo.InvariantCulture) == 0)
                {
                    message = SR.GetString("Failure_FieldAlreadyExist");
                    break;
                }
            }
            if ((message == null) && (string.Compare(customActivityType.Name, newMemberName, supportedLanguage == SupportedLanguages.VB, CultureInfo.InvariantCulture) == 0))
            {
                message = SR.GetString("Failure_FieldAlreadyExist");
            }
            if (message == null)
            {
                ActivityBind bind = new ActivityBind(ActivityBind.GetRelativePathExpression(Helpers.GetRootActivity(component), component), newMemberName);
                if (!(this.context.GetService(typeof(IDesignerHost)) is IDesignerHost))
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
                }
                this.binding = bind;
            }
            else
            {
                DesignerHelpers.ShowError(this.context, message);
            }
            if (message != null)
            {
                return DialogResult.None;
            }
            return DialogResult.OK;
        }

        public ActivityBind Binding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.binding;
            }
        }

        public bool CreateNew
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.createNew;
            }
        }

        public bool CreateNewProperty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.createNewProperty;
            }
        }

        public string NewMemberName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newMemberName;
            }
        }

        private class ActivityBindFormWorkflowOutline : WorkflowOutline
        {
            private ActivityBindForm parent;
            private Activity selectedActivity;
            private ActivityBindForm.PathInfo selectedPathInfo;

            public ActivityBindFormWorkflowOutline(IServiceProvider serviceProvider, ActivityBindForm parent) : base(serviceProvider)
            {
                this.parent = parent;
                base.NeedsExpandAll = false;
                base.Expanding += new TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
                base.TreeView.BeforeLabelEdit += new NodeLabelEditEventHandler(this.TreeView_BeforeLabelEdit);
                base.TreeView.AfterLabelEdit += new NodeLabelEditEventHandler(this.TreeView_AfterLabelEdit);
                base.TreeView.LabelEdit = true;
                base.TreeView.KeyDown += new KeyEventHandler(this.TreeView_KeyDown);
            }

            public void AddMemberKindImages(ImageList memberTypes)
            {
                for (int i = 0; i < memberTypes.Images.Count; i++)
                {
                    Image image = memberTypes.Images[i];
                    base.TreeView.ImageList.Images.Add(string.Format(CultureInfo.InvariantCulture, "MemberType#{0}", new object[] { i }), image);
                }
            }

            private ActivityBindForm.MemberActivityBindTreeNode CreateMemberNode(Activity activity, ActivityBindForm.PathInfo pathInfo)
            {
                ActivityBindForm.MemberActivityBindTreeNode node = new ActivityBindForm.MemberActivityBindTreeNode(activity, pathInfo);
                if (node.MayHaveChildNodes)
                {
                    node.Nodes.Add(new ActivityBindForm.DummyActivityBindTreeNode(activity));
                }
                return node;
            }

            protected override WorkflowOutlineNode CreateNewNode(Activity activity)
            {
                if (!activity.Enabled || Helpers.IsActivityLocked(activity))
                {
                    return null;
                }
                ActivityBindForm.ActivityBindTreeNode node = new ActivityBindForm.ActivityBindTreeNode(activity);
                node.Nodes.Add(new ActivityBindForm.DummyActivityBindTreeNode(activity));
                return node;
            }

            public void ExpandRootNode()
            {
                TreeNode rootNode = base.RootNode;
                if (rootNode != null)
                {
                    rootNode.Collapse();
                    rootNode.Expand();
                }
            }

            private string GetMemberNameFromIndexerName(string fullName)
            {
                int index = fullName.IndexOf('[');
                if (index != -1)
                {
                    fullName = fullName.Substring(0, index);
                }
                return fullName;
            }

            private bool IsSamePropertyIndexer(MemberInfo member1, MemberInfo member2)
            {
                if ((member1 == null) || (member2 == null))
                {
                    return false;
                }
                PropertyInfo info = member1 as PropertyInfo;
                PropertyInfo info2 = member2 as PropertyInfo;
                MethodInfo info3 = member1 as MethodInfo;
                MethodInfo info4 = member2 as MethodInfo;
                ParameterInfo[] infoArray = (info != null) ? info.GetIndexParameters() : ((info3 != null) ? info3.GetParameters() : null);
                ParameterInfo[] infoArray2 = (info2 != null) ? info2.GetIndexParameters() : ((info4 != null) ? info4.GetParameters() : null);
                if (((infoArray == null) || (infoArray.Length == 0)) || (((infoArray2 == null) || (infoArray2.Length == 0)) || (infoArray.Length != infoArray2.Length)))
                {
                    return false;
                }
                for (int i = 0; i < infoArray.Length; i++)
                {
                    if (infoArray[i].ParameterType != infoArray2[i].ParameterType)
                    {
                        return false;
                    }
                }
                return true;
            }

            protected override void OnNodeSelected(WorkflowOutlineNode node)
            {
                this.selectedActivity = (node != null) ? node.Activity : null;
                ActivityBindForm.MemberActivityBindTreeNode node2 = node as ActivityBindForm.MemberActivityBindTreeNode;
                this.selectedPathInfo = (node2 != null) ? node2.PathInfo : null;
                string propertyPath = this.PropertyPath;
                this.parent.SelectedActivityChanged(this.selectedActivity, this.selectedPathInfo, propertyPath);
            }

            protected override void OnRefreshNode(WorkflowOutlineNode node)
            {
                if (node.Activity != null)
                {
                    ActivityBindForm.MemberActivityBindTreeNode node2 = node as ActivityBindForm.MemberActivityBindTreeNode;
                    if (node2 != null)
                    {
                        node.RefreshNode();
                        int num = (int) (node2.MemberKind + ((ActivityBindForm.BindMemberKind) ((int) node2.MemberAccessKind)));
                        node.ImageIndex = node.SelectedImageIndex = base.TreeView.ImageList.Images.IndexOfKey(string.Format(CultureInfo.InvariantCulture, "MemberType#{0}", new object[] { num }));
                    }
                    else
                    {
                        base.OnRefreshNode(node);
                    }
                }
            }

            public void SelectActivity(Activity activity, List<ActivityBindForm.PathInfo> pathInfoList)
            {
                WorkflowOutlineNode parent = base.GetNode(activity);
                if (parent != null)
                {
                    parent.Expand();
                    if ((pathInfoList != null) && (pathInfoList.Count > 0))
                    {
                        for (int i = 0; i < pathInfoList.Count; i++)
                        {
                            ActivityBindForm.PathInfo info = pathInfoList[i];
                            ActivityBindForm.MemberActivityBindTreeNode node2 = null;
                            int index = info.Path.IndexOf('[');
                            if (index != -1)
                            {
                                string str = info.Path.Substring(0, index);
                                if ((parent.Text.Equals(str, StringComparison.Ordinal) && (i > 0)) && pathInfoList[i - 1].Path.Equals(str, StringComparison.Ordinal))
                                {
                                    parent = parent.Parent as WorkflowOutlineNode;
                                }
                            }
                            foreach (TreeNode node3 in parent.Nodes)
                            {
                                ActivityBindForm.MemberActivityBindTreeNode node4 = node3 as ActivityBindForm.MemberActivityBindTreeNode;
                                if ((node4 != null) && node4.PathInfo.Equals(info))
                                {
                                    node2 = node4;
                                    break;
                                }
                                if (((node4 != null) && node4.Text.Contains("[")) && info.Path.Contains("["))
                                {
                                    string memberNameFromIndexerName = this.GetMemberNameFromIndexerName(info.Path);
                                    string b = this.GetMemberNameFromIndexerName(node4.Text);
                                    if (string.Equals(memberNameFromIndexerName, b, StringComparison.Ordinal) && this.IsSamePropertyIndexer(info.MemberInfo, node4.PathInfo.MemberInfo))
                                    {
                                        node2 = node4;
                                        node4.PathInfo = info;
                                        node4.Text = ActivityBindForm.MemberActivityBindTreeNode.MemberName(info.Path);
                                        break;
                                    }
                                }
                            }
                            if (node2 == null)
                            {
                                break;
                            }
                            parent = node2;
                            parent.Expand();
                        }
                    }
                    base.TreeView.SelectedNode = parent;
                    Timer timer = new Timer();
                    timer.Tick += new EventHandler(this.timer_Tick);
                    timer.Interval = 50;
                    timer.Start();
                }
            }

            private void timer_Tick(object sender, EventArgs e)
            {
                Timer timer = sender as Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -= new EventHandler(this.timer_Tick);
                }
                if (base.TreeView.SelectedNode != null)
                {
                    base.TreeView.SelectedNode.EnsureVisible();
                }
                base.Focus();
            }

            private void TreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
            {
                string text = e.Node.Text;
                string label = e.Label;
                if ((text == null) || (label == null))
                {
                    e.CancelEdit = true;
                }
                else
                {
                    ActivityBindForm.MemberActivityBindTreeNode node = e.Node as ActivityBindForm.MemberActivityBindTreeNode;
                    bool flag = false;
                    if ((label.IndexOf("[", StringComparison.Ordinal) == -1) || !label.EndsWith("]", StringComparison.Ordinal))
                    {
                        flag = true;
                    }
                    else
                    {
                        string str3 = text.Substring(0, text.IndexOf("[", StringComparison.Ordinal));
                        string str4 = label.Substring(0, label.IndexOf("[", StringComparison.Ordinal));
                        flag = !str3.Equals(str4, StringComparison.Ordinal);
                    }
                    if (!flag)
                    {
                        ActivityBindForm.ActivityBindTreeNode parent = node.Parent as ActivityBindForm.ActivityBindTreeNode;
                        ActivityBindForm.MemberActivityBindTreeNode node3 = parent as ActivityBindForm.MemberActivityBindTreeNode;
                        System.Type activityType = (node3 != null) ? node3.PathInfo.PropertyType : this.parent.GetActivityType(parent.Activity);
                        List<ActivityBindForm.PathInfo> list = this.parent.ParseStringPath(activityType, label);
                        if ((list == null) || (list.Count == 0))
                        {
                            flag = true;
                        }
                        else
                        {
                            ActivityBindForm.PathInfo info = list[list.Count - 1];
                            if (info.Path.Equals(label, StringComparison.Ordinal))
                            {
                                node.PathInfo = info;
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        DesignerHelpers.ShowError(this.parent.serviceProvider, string.Format(CultureInfo.CurrentCulture, this.parent.IncorrectIndexChange, new object[] { label }));
                        e.CancelEdit = true;
                    }
                }
            }

            private void TreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
            {
                ActivityBindForm.MemberActivityBindTreeNode node = e.Node as ActivityBindForm.MemberActivityBindTreeNode;
                e.CancelEdit = ((node == null) || !node.Text.Contains("[")) || !node.Text.Contains("]");
            }

            private void TreeView_KeyDown(object sender, KeyEventArgs e)
            {
                if ((e.KeyCode == Keys.F2) && (base.TreeView.SelectedNode != null))
                {
                    base.TreeView.SelectedNode.BeginEdit();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }

            private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
            {
                ActivityBindForm.ActivityBindTreeNode node = e.Node as ActivityBindForm.ActivityBindTreeNode;
                if (((node != null) && (node.Nodes.Count > 0)) && (node.Nodes[0] is ActivityBindForm.DummyActivityBindTreeNode))
                {
                    ActivityBindForm.MemberActivityBindTreeNode node2 = node as ActivityBindForm.MemberActivityBindTreeNode;
                    List<ActivityBindForm.PathInfo> list = this.parent.PopulateAutoCompleteList(node.Activity, (node2 != null) ? node2.PathInfo : null);
                    List<TreeNode> list2 = new List<TreeNode>();
                    foreach (ActivityBindForm.PathInfo info in list)
                    {
                        ActivityBindForm.MemberActivityBindTreeNode nodeToUpdate = this.CreateMemberNode(node.Activity, info);
                        if (nodeToUpdate != null)
                        {
                            base.RefreshNode(nodeToUpdate, false);
                            list2.Add(nodeToUpdate);
                        }
                    }
                    base.TreeView.BeginUpdate();
                    try
                    {
                        node.Nodes.RemoveAt(0);
                        e.Node.Nodes.AddRange(list2.ToArray());
                    }
                    finally
                    {
                        base.TreeView.EndUpdate();
                    }
                }
            }

            public string PropertyPath
            {
                get
                {
                    ActivityBindForm.MemberActivityBindTreeNode selectedNode = base.TreeView.SelectedNode as ActivityBindForm.MemberActivityBindTreeNode;
                    string str = string.Empty;
                    while (selectedNode != null)
                    {
                        str = (str.Length == 0) ? selectedNode.Text : (selectedNode.Text + "." + str);
                        selectedNode = selectedNode.Parent as ActivityBindForm.MemberActivityBindTreeNode;
                    }
                    return str;
                }
            }

            public Activity SelectedActivity
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.selectedActivity;
                }
            }

            public ActivityBindForm.PathInfo SelectedMember
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.selectedPathInfo;
                }
            }
        }

        private class ActivityBindTreeNode : WorkflowOutlineNode
        {
            public ActivityBindTreeNode(Activity activity) : base(activity)
            {
            }
        }

        internal enum BindMemberAccessKind
        {
            Public,
            Internal,
            Protected,
            Private
        }

        internal enum BindMemberKind
        {
            Constant = 8,
            Delegate = 0x10,
            Event = 12,
            Field = 0,
            Index = 20,
            Property = 4
        }

        private class DummyActivityBindTreeNode : WorkflowOutlineNode
        {
            public DummyActivityBindTreeNode(Activity activity) : base(activity)
            {
            }
        }

        private class MemberActivityBindTreeNode : ActivityBindForm.ActivityBindTreeNode
        {
            private ActivityBindForm.BindMemberAccessKind accessKind;
            private ActivityBindForm.BindMemberKind kind;
            private System.Workflow.ComponentModel.Design.ActivityBindForm.PathInfo pathInfo;

            public MemberActivityBindTreeNode(Activity activity, System.Workflow.ComponentModel.Design.ActivityBindForm.PathInfo pathInfo) : base(activity)
            {
                this.kind = ActivityBindForm.BindMemberKind.Property;
                this.pathInfo = pathInfo;
                string str = MemberName(this.PathInfo.Path);
                if (this.pathInfo.MemberInfo is EventInfo)
                {
                    this.kind = ActivityBindForm.BindMemberKind.Event;
                    this.accessKind = ActivityBindForm.BindMemberAccessKind.Public;
                }
                else if (this.pathInfo.MemberInfo is FieldInfo)
                {
                    FieldInfo memberInfo = this.pathInfo.MemberInfo as FieldInfo;
                    if (((memberInfo.Attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope) && ((memberInfo.Attributes & FieldAttributes.Literal) != FieldAttributes.PrivateScope))
                    {
                        this.kind = ActivityBindForm.BindMemberKind.Constant;
                    }
                    else if (TypeProvider.IsAssignable(typeof(Delegate), memberInfo.FieldType))
                    {
                        this.kind = ActivityBindForm.BindMemberKind.Delegate;
                    }
                    else
                    {
                        this.kind = ActivityBindForm.BindMemberKind.Field;
                    }
                    this.accessKind = memberInfo.IsPublic ? ActivityBindForm.BindMemberAccessKind.Public : (memberInfo.IsFamily ? ActivityBindForm.BindMemberAccessKind.Internal : (memberInfo.IsPrivate ? ActivityBindForm.BindMemberAccessKind.Private : ActivityBindForm.BindMemberAccessKind.Protected));
                }
                else if (this.pathInfo.MemberInfo is PropertyInfo)
                {
                    this.kind = ActivityBindForm.BindMemberKind.Property;
                    MemberInfo info1 = this.pathInfo.MemberInfo;
                    this.accessKind = ActivityBindForm.BindMemberAccessKind.Public;
                }
                else if (str.IndexOfAny("[]".ToCharArray()) != -1)
                {
                    this.kind = ActivityBindForm.BindMemberKind.Index;
                    this.accessKind = ActivityBindForm.BindMemberAccessKind.Public;
                }
                else
                {
                    this.kind = ActivityBindForm.BindMemberKind.Property;
                    this.accessKind = ActivityBindForm.BindMemberAccessKind.Public;
                }
            }

            internal static string MemberName(string path)
            {
                string str = path;
                int num = str.LastIndexOf('.');
                return (((num != -1) && ((num + 1) < str.Length)) ? str.Substring(num + 1) : str);
            }

            public override void RefreshNode()
            {
                base.RefreshNode();
                base.Text = MemberName(this.PathInfo.Path);
                base.ForeColor = Color.DarkBlue;
            }

            public bool MayHaveChildNodes
            {
                get
                {
                    System.Type type = (this.pathInfo != null) ? this.pathInfo.PropertyType : null;
                    if (type == null)
                    {
                        return false;
                    }
                    return ((!ActivityBindForm.IsTypePrimitive(type) && !TypeProvider.IsAssignable(typeof(Delegate), type)) && !(type == typeof(object)));
                }
            }

            public ActivityBindForm.BindMemberAccessKind MemberAccessKind
            {
                get
                {
                    return this.accessKind;
                }
            }

            public ActivityBindForm.BindMemberKind MemberKind
            {
                get
                {
                    return this.kind;
                }
            }

            public System.Workflow.ComponentModel.Design.ActivityBindForm.PathInfo PathInfo
            {
                get
                {
                    return this.pathInfo;
                }
                set
                {
                    this.pathInfo = value;
                }
            }
        }

        private class PathInfo
        {
            private System.Reflection.MemberInfo memberInfo;
            private string path;
            private System.Type propertyType;

            public PathInfo(string path, System.Reflection.MemberInfo memberInfo, System.Type propertyType)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentNullException("path");
                }
                if (propertyType == null)
                {
                    throw new ArgumentNullException("propertyType");
                }
                if (memberInfo == null)
                {
                    throw new ArgumentNullException("memberInfo");
                }
                this.path = path;
                this.propertyType = propertyType;
                this.memberInfo = memberInfo;
            }

            public override bool Equals(object obj)
            {
                ActivityBindForm.PathInfo info = obj as ActivityBindForm.PathInfo;
                if (info == null)
                {
                    return false;
                }
                return this.path.Equals(info.path, StringComparison.Ordinal);
            }

            public override int GetHashCode()
            {
                return this.path.GetHashCode();
            }

            public override string ToString()
            {
                return this.path;
            }

            public System.Reflection.MemberInfo MemberInfo
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.memberInfo;
                }
            }

            public string Path
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.path;
                }
            }

            public System.Type PropertyType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.propertyType;
                }
            }
        }
    }
}

