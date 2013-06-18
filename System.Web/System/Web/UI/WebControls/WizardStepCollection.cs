namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    public sealed class WizardStepCollection : IList, ICollection, IEnumerable
    {
        private Wizard _wizard;

        internal WizardStepCollection(Wizard wizard)
        {
            this._wizard = wizard;
            wizard.TemplatedSteps.Clear();
        }

        public void Add(WizardStepBase wizardStep)
        {
            if (wizardStep == null)
            {
                throw new ArgumentNullException("wizardStep");
            }
            wizardStep.PreventAutoID();
            RemoveIfAlreadyExistsInWizard(wizardStep);
            wizardStep.Owner = this._wizard;
            this.Views.Add(wizardStep);
            this.AddTemplatedWizardStep(wizardStep);
            this.NotifyWizardStepsChanged();
        }

        public void AddAt(int index, WizardStepBase wizardStep)
        {
            if (wizardStep == null)
            {
                throw new ArgumentNullException("wizardStep");
            }
            RemoveIfAlreadyExistsInWizard(wizardStep);
            wizardStep.PreventAutoID();
            wizardStep.Owner = this._wizard;
            this.Views.AddAt(index, wizardStep);
            this.AddTemplatedWizardStep(wizardStep);
            this.NotifyWizardStepsChanged();
        }

        private void AddTemplatedWizardStep(WizardStepBase wizardStep)
        {
            TemplatedWizardStep item = wizardStep as TemplatedWizardStep;
            if (item != null)
            {
                this._wizard.TemplatedSteps.Add(item);
                this._wizard.RegisterCustomNavigationContainers(item);
            }
        }

        public void Clear()
        {
            this.Views.Clear();
            this._wizard.TemplatedSteps.Clear();
            this.NotifyWizardStepsChanged();
        }

        public bool Contains(WizardStepBase wizardStep)
        {
            if (wizardStep == null)
            {
                throw new ArgumentNullException("wizardStep");
            }
            return this.Views.Contains(wizardStep);
        }

        public void CopyTo(WizardStepBase[] array, int index)
        {
            this.Views.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.Views.GetEnumerator();
        }

        private static WizardStepBase GetStepAndVerify(object value)
        {
            WizardStepBase base2 = value as WizardStepBase;
            if (base2 == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Wizard_WizardStepOnly"));
            }
            return base2;
        }

        public int IndexOf(WizardStepBase wizardStep)
        {
            if (wizardStep == null)
            {
                throw new ArgumentNullException("wizardStep");
            }
            return this.Views.IndexOf(wizardStep);
        }

        public void Insert(int index, WizardStepBase wizardStep)
        {
            this.AddAt(index, wizardStep);
        }

        internal void NotifyWizardStepsChanged()
        {
            this._wizard.OnWizardStepsChanged();
        }

        public void Remove(WizardStepBase wizardStep)
        {
            if (wizardStep == null)
            {
                throw new ArgumentNullException("wizardStep");
            }
            this.Views.Remove(wizardStep);
            wizardStep.Owner = null;
            TemplatedWizardStep item = wizardStep as TemplatedWizardStep;
            if (item != null)
            {
                this._wizard.TemplatedSteps.Remove(item);
            }
            this.NotifyWizardStepsChanged();
        }

        public void RemoveAt(int index)
        {
            WizardStepBase base2 = this.Views[index] as WizardStepBase;
            if (base2 != null)
            {
                base2.Owner = null;
                TemplatedWizardStep item = base2 as TemplatedWizardStep;
                if (item != null)
                {
                    this._wizard.TemplatedSteps.Remove(item);
                }
            }
            this.Views.RemoveAt(index);
            this.NotifyWizardStepsChanged();
        }

        private static void RemoveIfAlreadyExistsInWizard(WizardStepBase wizardStep)
        {
            if (wizardStep.Owner != null)
            {
                wizardStep.Owner.WizardSteps.Remove(wizardStep);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.Views.CopyTo(array, index);
        }

        int IList.Add(object value)
        {
            WizardStepBase stepAndVerify = GetStepAndVerify(value);
            stepAndVerify.PreventAutoID();
            this.Add(stepAndVerify);
            return this.IndexOf(stepAndVerify);
        }

        bool IList.Contains(object value)
        {
            return this.Contains(GetStepAndVerify(value));
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(GetStepAndVerify(value));
        }

        void IList.Insert(int index, object value)
        {
            this.AddAt(index, GetStepAndVerify(value));
        }

        void IList.Remove(object value)
        {
            this.Remove(GetStepAndVerify(value));
        }

        public int Count
        {
            get
            {
                return this.Views.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.Views.IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public WizardStepBase this[int index]
        {
            get
            {
                return (WizardStepBase) this.Views[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.Views[index];
            }
            set
            {
                this.RemoveAt(index);
                this.AddAt(index, GetStepAndVerify(value));
            }
        }

        private ViewCollection Views
        {
            get
            {
                return this._wizard.MultiView.Views;
            }
        }
    }
}

