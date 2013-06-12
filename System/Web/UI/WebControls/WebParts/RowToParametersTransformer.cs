namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [WebPartTransformer(typeof(IWebPartRow), typeof(IWebPartParameters))]
    public sealed class RowToParametersTransformer : WebPartTransformer, IWebPartParameters
    {
        private ParametersCallback _callback;
        private string[] _consumerFieldNames;
        private PropertyDescriptorCollection _consumerSchema;
        private IWebPartRow _provider;
        private string[] _providerFieldNames;

        private void CheckFieldNamesLength()
        {
            int num = (this._consumerFieldNames != null) ? this._consumerFieldNames.Length : 0;
            int num2 = (this._providerFieldNames != null) ? this._providerFieldNames.Length : 0;
            if (num != num2)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("RowToParametersTransformer_DifferentFieldNamesLength"));
            }
        }

        public override Control CreateConfigurationControl()
        {
            return new RowToParametersConfigurationWizard(this);
        }

        private void GetRowData(object rowData)
        {
            IDictionary parametersData = null;
            if (rowData != null)
            {
                PropertyDescriptorCollection schema = ((IWebPartParameters) this).Schema;
                parametersData = new HybridDictionary(schema.Count);
                if (schema.Count > 0)
                {
                    PropertyDescriptorCollection selectedProviderSchema = this.SelectedProviderSchema;
                    if (((selectedProviderSchema != null) && (selectedProviderSchema.Count > 0)) && (selectedProviderSchema.Count == schema.Count))
                    {
                        for (int i = 0; i < selectedProviderSchema.Count; i++)
                        {
                            PropertyDescriptor descriptor = selectedProviderSchema[i];
                            PropertyDescriptor descriptor2 = schema[i];
                            parametersData[descriptor2.Name] = descriptor.GetValue(rowData);
                        }
                    }
                }
            }
            this._callback(parametersData);
        }

        protected internal override void LoadConfigurationState(object savedState)
        {
            if (savedState != null)
            {
                string[] strArray = (string[]) savedState;
                int length = strArray.Length;
                if ((length % 2) != 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("RowToParametersTransformer_DifferentFieldNamesLength"));
                }
                int num2 = length / 2;
                this._consumerFieldNames = new string[num2];
                this._providerFieldNames = new string[num2];
                for (int i = 0; i < num2; i++)
                {
                    this._consumerFieldNames[i] = strArray[2 * i];
                    this._providerFieldNames[i] = strArray[(2 * i) + 1];
                }
            }
        }

        protected internal override object SaveConfigurationState()
        {
            this.CheckFieldNamesLength();
            int num = (this._consumerFieldNames != null) ? this._consumerFieldNames.Length : 0;
            if (num <= 0)
            {
                return null;
            }
            string[] strArray = new string[num * 2];
            for (int i = 0; i < num; i++)
            {
                strArray[2 * i] = this._consumerFieldNames[i];
                strArray[(2 * i) + 1] = this._providerFieldNames[i];
            }
            return strArray;
        }

        void IWebPartParameters.GetParametersData(ParametersCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            this.CheckFieldNamesLength();
            if (this._provider != null)
            {
                this._callback = callback;
                this._provider.GetRowData(new RowCallback(this.GetRowData));
            }
            else
            {
                callback(null);
            }
        }

        void IWebPartParameters.SetConsumerSchema(PropertyDescriptorCollection schema)
        {
            this._consumerSchema = schema;
        }

        public override object Transform(object providerData)
        {
            this._provider = (IWebPartRow) providerData;
            return this;
        }

        [TypeConverter(typeof(StringArrayConverter))]
        public string[] ConsumerFieldNames
        {
            get
            {
                if (this._consumerFieldNames == null)
                {
                    return new string[0];
                }
                return (string[]) this._consumerFieldNames.Clone();
            }
            set
            {
                this._consumerFieldNames = (value != null) ? ((string[]) value.Clone()) : null;
            }
        }

        private PropertyDescriptorCollection ConsumerSchema
        {
            get
            {
                return this._consumerSchema;
            }
        }

        [TypeConverter(typeof(StringArrayConverter))]
        public string[] ProviderFieldNames
        {
            get
            {
                if (this._providerFieldNames == null)
                {
                    return new string[0];
                }
                return (string[]) this._providerFieldNames.Clone();
            }
            set
            {
                this._providerFieldNames = (value != null) ? ((string[]) value.Clone()) : null;
            }
        }

        private PropertyDescriptorCollection ProviderSchema
        {
            get
            {
                if (this._provider == null)
                {
                    return null;
                }
                return this._provider.Schema;
            }
        }

        private PropertyDescriptorCollection SelectedProviderSchema
        {
            get
            {
                PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
                PropertyDescriptorCollection providerSchema = this.ProviderSchema;
                if (((providerSchema != null) && (this._providerFieldNames != null)) && (this._providerFieldNames.Length > 0))
                {
                    foreach (string str in this._providerFieldNames)
                    {
                        PropertyDescriptor descriptor = providerSchema.Find(str, true);
                        if (descriptor == null)
                        {
                            return new PropertyDescriptorCollection(null);
                        }
                        descriptors.Add(descriptor);
                    }
                }
                return descriptors;
            }
        }

        PropertyDescriptorCollection IWebPartParameters.Schema
        {
            get
            {
                this.CheckFieldNamesLength();
                PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
                if (((this._consumerSchema != null) && (this._consumerFieldNames != null)) && (this._consumerFieldNames.Length > 0))
                {
                    foreach (string str in this._consumerFieldNames)
                    {
                        PropertyDescriptor descriptor = this._consumerSchema.Find(str, true);
                        if (descriptor == null)
                        {
                            return new PropertyDescriptorCollection(null);
                        }
                        descriptors.Add(descriptor);
                    }
                }
                return descriptors;
            }
        }

        private sealed class RowToParametersConfigurationWizard : TransformerConfigurationWizardBase
        {
            private DropDownList[] _consumerFieldNames;
            private RowToParametersTransformer _owner;
            private const string consumerFieldNameID = "ConsumerFieldName";

            public RowToParametersConfigurationWizard(RowToParametersTransformer owner)
            {
                this._owner = owner;
            }

            protected override void CreateWizardSteps()
            {
                int num = (base.OldProviderNames != null) ? base.OldProviderNames.Length : 0;
                if (num > 0)
                {
                    this._consumerFieldNames = new DropDownList[num / 2];
                    ListItem[] itemArray = null;
                    int num2 = (base.OldConsumerNames != null) ? base.OldConsumerNames.Length : 0;
                    if (num2 > 0)
                    {
                        itemArray = new ListItem[num2 / 2];
                        for (int j = 0; j < (num2 / 2); j++)
                        {
                            itemArray[j] = new ListItem(base.OldConsumerNames[2 * j], base.OldConsumerNames[(2 * j) + 1]);
                        }
                    }
                    for (int i = 0; i < (num / 2); i++)
                    {
                        WizardStep wizardStep = new WizardStep();
                        wizardStep.Controls.Add(new LiteralControl(System.Web.SR.GetString("RowToParametersTransformer_ProviderFieldName") + " "));
                        Label child = new Label {
                            Text = HttpUtility.HtmlEncode(base.OldProviderNames[2 * i])
                        };
                        child.Font.Bold = true;
                        wizardStep.Controls.Add(child);
                        wizardStep.Controls.Add(new LiteralControl("<br />"));
                        DropDownList list = new DropDownList {
                            ID = "ConsumerFieldName" + i
                        };
                        if (itemArray != null)
                        {
                            list.Items.Add(new ListItem());
                            string[] strArray = this._owner._providerFieldNames;
                            string[] strArray2 = this._owner._consumerFieldNames;
                            string b = base.OldProviderNames[(2 * i) + 1];
                            string str2 = null;
                            if (strArray != null)
                            {
                                for (int k = 0; k < strArray.Length; k++)
                                {
                                    if ((string.Equals(strArray[k], b, StringComparison.OrdinalIgnoreCase) && (strArray2 != null)) && (strArray2.Length > k))
                                    {
                                        str2 = strArray2[k];
                                        break;
                                    }
                                }
                            }
                            foreach (ListItem item in itemArray)
                            {
                                ListItem item2 = new ListItem(item.Text, item.Value);
                                if (string.Equals(item2.Value, str2, StringComparison.OrdinalIgnoreCase))
                                {
                                    item2.Selected = true;
                                }
                                list.Items.Add(item2);
                            }
                        }
                        else
                        {
                            list.Items.Add(new ListItem(System.Web.SR.GetString("RowToParametersTransformer_NoConsumerSchema")));
                            list.Enabled = false;
                        }
                        this._consumerFieldNames[i] = list;
                        Label label2 = new Label {
                            Text = System.Web.SR.GetString("RowToParametersTransformer_ConsumerFieldName"),
                            AssociatedControlID = list.ID
                        };
                        wizardStep.Controls.Add(label2);
                        wizardStep.Controls.Add(new LiteralControl(" "));
                        wizardStep.Controls.Add(list);
                        this.WizardSteps.Add(wizardStep);
                    }
                }
                else
                {
                    WizardStep step2 = new WizardStep();
                    step2.Controls.Add(new LiteralControl(System.Web.SR.GetString("RowToParametersTransformer_NoProviderSchema")));
                    this.WizardSteps.Add(step2);
                }
            }

            protected override void OnFinishButtonClick(WizardNavigationEventArgs e)
            {
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                int num = (base.OldProviderNames != null) ? base.OldProviderNames.Length : 0;
                if (num > 0)
                {
                    for (int i = 0; i < this._consumerFieldNames.Length; i++)
                    {
                        DropDownList list3 = this._consumerFieldNames[i];
                        if (list3.Enabled)
                        {
                            string selectedValue = list3.SelectedValue;
                            if (!string.IsNullOrEmpty(selectedValue))
                            {
                                list.Add(base.OldProviderNames[(2 * i) + 1]);
                                list2.Add(selectedValue);
                            }
                        }
                    }
                }
                this._owner.ConsumerFieldNames = (string[]) list2.ToArray(typeof(string));
                this._owner.ProviderFieldNames = (string[]) list.ToArray(typeof(string));
                base.OnFinishButtonClick(e);
            }

            protected override PropertyDescriptorCollection ConsumerSchema
            {
                get
                {
                    return this._owner.ConsumerSchema;
                }
            }

            protected override PropertyDescriptorCollection ProviderSchema
            {
                get
                {
                    return this._owner.ProviderSchema;
                }
            }
        }
    }
}

