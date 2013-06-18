namespace System.Web.UI.Design.Util
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class WizardPanel : UserControl
    {
        private string _caption;
        private bool _needsToInvalidate;
        private WizardPanel _nextPanel;
        private WizardForm _parentWizard;

        protected internal virtual void OnComplete()
        {
        }

        public virtual bool OnNext()
        {
            return true;
        }

        public virtual void OnPrevious()
        {
        }

        internal void SetParentWizard(WizardForm parent)
        {
            this._parentWizard = parent;
            if ((this._parentWizard != null) && this._needsToInvalidate)
            {
                this._parentWizard.Invalidate();
                this._needsToInvalidate = false;
            }
        }

        public string Caption
        {
            get
            {
                if (this._caption == null)
                {
                    return string.Empty;
                }
                return this._caption;
            }
            set
            {
                this._caption = value;
                if (this._parentWizard != null)
                {
                    this._parentWizard.Invalidate();
                }
                else
                {
                    this._needsToInvalidate = true;
                }
            }
        }

        public WizardPanel NextPanel
        {
            get
            {
                return this._nextPanel;
            }
            set
            {
                this._nextPanel = value;
                if (this._parentWizard != null)
                {
                    this._parentWizard.RegisterPanel(this._nextPanel);
                }
            }
        }

        [Browsable(false)]
        public WizardForm ParentWizard
        {
            get
            {
                return this._parentWizard;
            }
        }

        protected IServiceProvider ServiceProvider
        {
            get
            {
                return this.ParentWizard.ServiceProvider;
            }
        }
    }
}

