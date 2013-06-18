namespace System.Web.UI.Design.Util
{
    using System;

    internal class WizardPanelChangingEventArgs : EventArgs
    {
        private WizardPanel _currentPanel;

        public WizardPanelChangingEventArgs(WizardPanel currentPanel)
        {
            this._currentPanel = currentPanel;
        }

        public WizardPanel CurrentPanel
        {
            get
            {
                return this._currentPanel;
            }
        }
    }
}

