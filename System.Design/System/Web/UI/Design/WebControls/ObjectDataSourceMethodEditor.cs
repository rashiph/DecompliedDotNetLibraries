namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Text;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal sealed class ObjectDataSourceMethodEditor : UserControl
    {
        private System.Windows.Forms.Label _helpLabel;
        private AutoSizeComboBox _methodComboBox;
        private System.Windows.Forms.Label _methodLabel;
        private System.Windows.Forms.Label _signatureLabel;
        private System.Windows.Forms.TextBox _signatureTextBox;
        private static readonly object EventMethodChanged = new object();

        public event EventHandler MethodChanged
        {
            add
            {
                base.Events.AddHandler(EventMethodChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMethodChanged, value);
            }
        }

        public ObjectDataSourceMethodEditor()
        {
            this.InitializeComponent();
            this.InitializeUI();
        }

        private static void AppendGenericArguments(System.Type[] args, StringBuilder sb)
        {
            if (args.Length > 0)
            {
                sb.Append("<");
                for (int i = 0; i < args.Length; i++)
                {
                    AppendTypeName(args[i], false, sb);
                    if ((i + 1) < args.Length)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(">");
            }
        }

        internal static void AppendTypeName(System.Type t, bool topLevelFullName, StringBuilder sb)
        {
            string s = topLevelFullName ? t.FullName : t.Name;
            if (t.IsGenericType)
            {
                int index = s.IndexOf("`", StringComparison.Ordinal);
                if (index == -1)
                {
                    index = s.Length;
                }
                sb.Append(s.Substring(0, index));
                AppendGenericArguments(t.GetGenericArguments(), sb);
                if (index < s.Length)
                {
                    index++;
                    while ((index < s.Length) && char.IsNumber(s, index))
                    {
                        index++;
                    }
                    sb.Append(s.Substring(index));
                }
            }
            else
            {
                sb.Append(s);
            }
        }

        private bool FilterMethod(System.Reflection.MethodInfo methodInfo, DataObjectMethodType methodType)
        {
            if (methodType == DataObjectMethodType.Select)
            {
                if (methodInfo.ReturnType == typeof(void))
                {
                    return false;
                }
            }
            else
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if ((parameters == null) || (parameters.Length == 0))
                {
                    return false;
                }
            }
            return true;
        }

        internal static string GetMethodSignature(System.Reflection.MethodInfo mi)
        {
            if (mi == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder(0x80);
            sb.Append(mi.Name);
            AppendGenericArguments(mi.GetGenericArguments(), sb);
            sb.Append("(");
            ParameterInfo[] parameters = mi.GetParameters();
            foreach (ParameterInfo info in parameters)
            {
                AppendTypeName(info.ParameterType, false, sb);
                sb.Append(" " + info.Name);
                if ((info.Position + 1) < parameters.Length)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            if (mi.ReturnType != typeof(void))
            {
                StringBuilder builder2 = new StringBuilder();
                AppendTypeName(mi.ReturnType, false, builder2);
                return System.Design.SR.GetString("ObjectDataSourceMethodEditor_SignatureFormat", new object[] { sb, builder2 });
            }
            return sb.ToString();
        }

        private void InitializeComponent()
        {
            this._helpLabel = new System.Windows.Forms.Label();
            this._methodLabel = new System.Windows.Forms.Label();
            this._signatureLabel = new System.Windows.Forms.Label();
            this._methodComboBox = new AutoSizeComboBox();
            this._signatureTextBox = new System.Windows.Forms.TextBox();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 12);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x1e7, 80);
            this._helpLabel.TabIndex = 10;
            this._methodLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._methodLabel.Location = new Point(12, 0x62);
            this._methodLabel.Name = "_methodLabel";
            this._methodLabel.Size = new Size(0x1e7, 0x10);
            this._methodLabel.TabIndex = 20;
            this._methodComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._methodComboBox.Location = new Point(12, 0x74);
            this._methodComboBox.Name = "_methodComboBox";
            this._methodComboBox.Size = new Size(300, 0x15);
            this._methodComboBox.Sorted = true;
            this._methodComboBox.TabIndex = 30;
            this._methodComboBox.SelectedIndexChanged += new EventHandler(this.OnMethodComboBoxSelectedIndexChanged);
            this._signatureLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._signatureLabel.Location = new Point(12, 0x91);
            this._signatureLabel.Name = "_signatureLabel";
            this._signatureLabel.Size = new Size(0x1e7, 0x10);
            this._signatureLabel.TabIndex = 40;
            this._signatureTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._signatureTextBox.BackColor = SystemColors.Control;
            this._signatureTextBox.Location = new Point(12, 0xa3);
            this._signatureTextBox.Multiline = true;
            this._signatureTextBox.Name = "_signatureTextBox";
            this._signatureTextBox.ReadOnly = true;
            this._signatureTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._signatureTextBox.Size = new Size(0x1e7, 0x30);
            this._signatureTextBox.TabIndex = 50;
            this._signatureTextBox.Text = "";
            base.Controls.Add(this._signatureTextBox);
            base.Controls.Add(this._methodComboBox);
            base.Controls.Add(this._signatureLabel);
            base.Controls.Add(this._methodLabel);
            base.Controls.Add(this._helpLabel);
            base.Name = "ObjectDataSourceMethodEditor";
            base.Size = new Size(0x1ff, 220);
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._methodLabel.Text = System.Design.SR.GetString("ObjectDataSourceMethodEditor_MethodLabel");
            this._signatureLabel.Text = System.Design.SR.GetString("ObjectDataSource_General_MethodSignatureLabel");
        }

        private static bool IsPrimitiveType(System.Type t)
        {
            System.Type underlyingType = Nullable.GetUnderlyingType(t);
            if (underlyingType != null)
            {
                t = underlyingType;
            }
            if ((!t.IsPrimitive && !(t == typeof(string))) && (!(t == typeof(DateTime)) && !(t == typeof(decimal))))
            {
                return (t == typeof(object));
            }
            return true;
        }

        private void OnMethodChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventMethodChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMethodComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnMethodChanged(EventArgs.Empty);
            this._signatureTextBox.Text = GetMethodSignature(this.MethodInfo);
        }

        public void SetMethodInformation(System.Reflection.MethodInfo[] methods, string selectedMethodName, ParameterCollection selectedParameters, DataObjectMethodType methodType, System.Type dataObjectType)
        {
            try
            {
                this._signatureTextBox.Text = string.Empty;
                switch (methodType)
                {
                    case DataObjectMethodType.Select:
                        this._helpLabel.Text = System.Design.SR.GetString("ObjectDataSourceMethodEditor_SelectHelpLabel");
                        break;

                    case DataObjectMethodType.Update:
                        this._helpLabel.Text = System.Design.SR.GetString("ObjectDataSourceMethodEditor_UpdateHelpLabel");
                        break;

                    case DataObjectMethodType.Insert:
                        this._helpLabel.Text = System.Design.SR.GetString("ObjectDataSourceMethodEditor_InsertHelpLabel");
                        break;

                    case DataObjectMethodType.Delete:
                        this._helpLabel.Text = System.Design.SR.GetString("ObjectDataSourceMethodEditor_DeleteHelpLabel");
                        break;
                }
                this._methodComboBox.BeginUpdate();
                this._methodComboBox.Items.Clear();
                MethodItem item = null;
                bool flag = false;
                foreach (System.Reflection.MethodInfo info in methods)
                {
                    if (this.FilterMethod(info, methodType))
                    {
                        bool flag2 = false;
                        DataObjectMethodAttribute attribute = Attribute.GetCustomAttribute(info, typeof(DataObjectMethodAttribute), true) as DataObjectMethodAttribute;
                        if ((attribute != null) && (attribute.MethodType == methodType))
                        {
                            if (!flag)
                            {
                                this._methodComboBox.Items.Clear();
                            }
                            flag = true;
                            flag2 = true;
                        }
                        else if (!flag)
                        {
                            flag2 = true;
                        }
                        bool flag3 = ObjectDataSourceDesigner.IsMatchingMethod(info, selectedMethodName, selectedParameters, dataObjectType);
                        if (flag2 || flag3)
                        {
                            MethodItem item2 = new MethodItem(info);
                            this._methodComboBox.Items.Add(item2);
                            if (flag3)
                            {
                                item = item2;
                            }
                            else if (((attribute != null) && (attribute.MethodType == methodType)) && (attribute.IsDefault && (selectedMethodName.Length == 0)))
                            {
                                item = item2;
                            }
                        }
                    }
                }
                if (methodType != DataObjectMethodType.Select)
                {
                    this._methodComboBox.Items.Insert(0, new MethodItem(null));
                }
                this._methodComboBox.InvalidateDropDownWidth();
                this._methodComboBox.SelectedItem = item;
            }
            finally
            {
                this._methodComboBox.EndUpdate();
            }
        }

        public System.Type DataObjectType
        {
            get
            {
                System.Reflection.MethodInfo methodInfo = this.MethodInfo;
                if (methodInfo == null)
                {
                    return null;
                }
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length != 1)
                {
                    return null;
                }
                System.Type parameterType = parameters[0].ParameterType;
                if (IsPrimitiveType(parameterType))
                {
                    return null;
                }
                return parameterType;
            }
        }

        public System.Reflection.MethodInfo MethodInfo
        {
            get
            {
                MethodItem selectedItem = this._methodComboBox.SelectedItem as MethodItem;
                if (selectedItem == null)
                {
                    return null;
                }
                return selectedItem.MethodInfo;
            }
        }

        private sealed class MethodItem
        {
            private System.Reflection.MethodInfo _methodInfo;

            public MethodItem(System.Reflection.MethodInfo methodInfo)
            {
                this._methodInfo = methodInfo;
            }

            public override string ToString()
            {
                if (this._methodInfo == null)
                {
                    return System.Design.SR.GetString("ObjectDataSourceMethodEditor_NoMethod");
                }
                return ObjectDataSourceMethodEditor.GetMethodSignature(this._methodInfo);
            }

            public System.Reflection.MethodInfo MethodInfo
            {
                get
                {
                    return this._methodInfo;
                }
            }
        }
    }
}

