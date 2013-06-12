namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI.WebControls;

    internal abstract class TransformerConfigurationWizardBase : Wizard, ITransformerConfigurationControl
    {
        private string[] _oldConsumerNames;
        private string[] _oldProviderNames;
        private const int baseIndex = 0;
        private const int controlStateArrayLength = 3;
        private static readonly object EventCancelled = new object();
        private static readonly object EventSucceeded = new object();
        private const int oldConsumerNamesIndex = 2;
        private const int oldProviderNamesIndex = 1;

        public event EventHandler Cancelled
        {
            add
            {
                base.Events.AddHandler(EventCancelled, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCancelled, value);
            }
        }

        public event EventHandler Succeeded
        {
            add
            {
                base.Events.AddHandler(EventSucceeded, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSucceeded, value);
            }
        }

        protected TransformerConfigurationWizardBase()
        {
        }

        private string[] ConvertSchemaToArray(PropertyDescriptorCollection schema)
        {
            string[] strArray = null;
            if ((schema != null) && (schema.Count > 0))
            {
                strArray = new string[schema.Count * 2];
                for (int i = 0; i < schema.Count; i++)
                {
                    PropertyDescriptor descriptor = schema[i];
                    if (descriptor != null)
                    {
                        strArray[2 * i] = descriptor.DisplayName;
                        strArray[(2 * i) + 1] = descriptor.Name;
                    }
                }
            }
            return strArray;
        }

        protected abstract void CreateWizardSteps();
        protected internal override void LoadControlState(object savedState)
        {
            if (savedState == null)
            {
                this.CreateWizardSteps();
                base.LoadControlState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 3)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_ControlState"));
                }
                if (objArray[1] != null)
                {
                    this.OldProviderNames = (string[]) objArray[1];
                }
                if (objArray[2] != null)
                {
                    this.OldConsumerNames = (string[]) objArray[2];
                }
                this.CreateWizardSteps();
                base.LoadControlState(objArray[0]);
            }
        }

        protected override void OnCancelButtonClick(EventArgs e)
        {
            this.OnCancelled(EventArgs.Empty);
            base.OnCancelButtonClick(e);
        }

        private void OnCancelled(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCancelled];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFinishButtonClick(WizardNavigationEventArgs e)
        {
            this.OnSucceeded(EventArgs.Empty);
            base.OnFinishButtonClick(e);
        }

        protected internal override void OnInit(EventArgs e)
        {
            this.DisplayCancelButton = true;
            this.DisplaySideBar = false;
            if (this.Page != null)
            {
                this.Page.RegisterRequiresControlState(this);
                this.Page.PreRenderComplete += new EventHandler(this.OnPagePreRenderComplete);
            }
            base.OnInit(e);
        }

        private void OnPagePreRenderComplete(object sender, EventArgs e)
        {
            string[] arrA = this.ConvertSchemaToArray(this.ProviderSchema);
            string[] strArray2 = this.ConvertSchemaToArray(this.ConsumerSchema);
            if ((this.StringArraysDifferent(arrA, this.OldProviderNames) || this.StringArraysDifferent(strArray2, this.OldConsumerNames)) || (this.WizardSteps.Count == 0))
            {
                this.OldProviderNames = arrA;
                this.OldConsumerNames = strArray2;
                this.WizardSteps.Clear();
                base.ClearChildState();
                this.CreateWizardSteps();
                this.ActiveStepIndex = 0;
            }
        }

        private void OnSucceeded(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSucceeded];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override object SaveControlState()
        {
            object[] objArray = new object[] { base.SaveControlState(), this.OldProviderNames, this.OldConsumerNames };
            for (int i = 0; i < 3; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        private bool StringArraysDifferent(string[] arrA, string[] arrB)
        {
            int num = (arrA == null) ? 0 : arrA.Length;
            int num2 = (arrB == null) ? 0 : arrB.Length;
            if (num != num2)
            {
                return true;
            }
            for (int i = 0; i < num2; i++)
            {
                if (arrA[i] != arrB[i])
                {
                    return true;
                }
            }
            return false;
        }

        protected abstract PropertyDescriptorCollection ConsumerSchema { get; }

        protected string[] OldConsumerNames
        {
            get
            {
                return this._oldConsumerNames;
            }
            set
            {
                this._oldConsumerNames = value;
            }
        }

        protected string[] OldProviderNames
        {
            get
            {
                return this._oldProviderNames;
            }
            set
            {
                this._oldProviderNames = value;
            }
        }

        protected abstract PropertyDescriptorCollection ProviderSchema { get; }
    }
}

