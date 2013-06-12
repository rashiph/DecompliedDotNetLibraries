namespace System.Web.UI.WebControls
{
    using System;

    public class WizardNavigationEventArgs : EventArgs
    {
        private bool _cancel;
        private int _currentStepIndex;
        private int _nextStepIndex;

        public WizardNavigationEventArgs(int currentStepIndex, int nextStepIndex)
        {
            this._currentStepIndex = currentStepIndex;
            this._nextStepIndex = nextStepIndex;
        }

        internal void SetNextStepIndex(int nextStepIndex)
        {
            this._nextStepIndex = nextStepIndex;
        }

        public bool Cancel
        {
            get
            {
                return this._cancel;
            }
            set
            {
                this._cancel = value;
            }
        }

        public int CurrentStepIndex
        {
            get
            {
                return this._currentStepIndex;
            }
        }

        public int NextStepIndex
        {
            get
            {
                return this._nextStepIndex;
            }
        }
    }
}

