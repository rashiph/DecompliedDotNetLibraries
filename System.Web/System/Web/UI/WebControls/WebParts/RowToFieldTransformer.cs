namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [WebPartTransformer(typeof(IWebPartRow), typeof(IWebPartField))]
    public sealed class RowToFieldTransformer : WebPartTransformer, IWebPartField
    {
        private FieldCallback _callback;
        private string _fieldName;
        private IWebPartRow _provider;

        public override Control CreateConfigurationControl()
        {
            return new RowToFieldConfigurationWizard(this);
        }

        private void GetRowData(object rowData)
        {
            object fieldValue = null;
            if (rowData != null)
            {
                PropertyDescriptor schema = ((IWebPartField) this).Schema;
                if (schema != null)
                {
                    fieldValue = schema.GetValue(rowData);
                }
            }
            this._callback(fieldValue);
        }

        protected internal override void LoadConfigurationState(object savedState)
        {
            this._fieldName = (string) savedState;
        }

        protected internal override object SaveConfigurationState()
        {
            return this._fieldName;
        }

        void IWebPartField.GetFieldValue(FieldCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
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

        public override object Transform(object providerData)
        {
            this._provider = (IWebPartRow) providerData;
            return this;
        }

        public string FieldName
        {
            get
            {
                if (this._fieldName == null)
                {
                    return string.Empty;
                }
                return this._fieldName;
            }
            set
            {
                this._fieldName = value;
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

        PropertyDescriptor IWebPartField.Schema
        {
            get
            {
                PropertyDescriptorCollection providerSchema = this.ProviderSchema;
                if (providerSchema == null)
                {
                    return null;
                }
                return providerSchema.Find(this.FieldName, true);
            }
        }

        private sealed class RowToFieldConfigurationWizard : TransformerConfigurationWizardBase
        {
            private DropDownList _fieldName;
            private RowToFieldTransformer _owner;
            private const string fieldNameID = "FieldName";

            public RowToFieldConfigurationWizard(RowToFieldTransformer owner)
            {
                this._owner = owner;
            }

            protected override void CreateWizardSteps()
            {
                WizardStep wizardStep = new WizardStep();
                this._fieldName = new DropDownList();
                this._fieldName.ID = "FieldName";
                if (base.OldProviderNames != null)
                {
                    for (int i = 0; i < (base.OldProviderNames.Length / 2); i++)
                    {
                        ListItem item = new ListItem(base.OldProviderNames[2 * i], base.OldProviderNames[(2 * i) + 1]);
                        if (string.Equals(item.Value, this._owner.FieldName, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Selected = true;
                        }
                        this._fieldName.Items.Add(item);
                    }
                }
                else
                {
                    this._fieldName.Items.Add(new ListItem(System.Web.SR.GetString("RowToFieldTransformer_NoProviderSchema")));
                    this._fieldName.Enabled = false;
                }
                Label child = new Label {
                    Text = System.Web.SR.GetString("RowToFieldTransformer_FieldName"),
                    AssociatedControlID = this._fieldName.ID
                };
                wizardStep.Controls.Add(child);
                wizardStep.Controls.Add(new LiteralControl(" "));
                wizardStep.Controls.Add(this._fieldName);
                this.WizardSteps.Add(wizardStep);
            }

            protected override void OnFinishButtonClick(WizardNavigationEventArgs e)
            {
                string selectedValue = null;
                if (this._fieldName.Enabled)
                {
                    selectedValue = this._fieldName.SelectedValue;
                }
                this._owner.FieldName = selectedValue;
                base.OnFinishButtonClick(e);
            }

            protected override PropertyDescriptorCollection ConsumerSchema
            {
                get
                {
                    return null;
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

