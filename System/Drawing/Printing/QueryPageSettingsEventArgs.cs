namespace System.Drawing.Printing
{
    using System;

    public class QueryPageSettingsEventArgs : PrintEventArgs
    {
        private System.Drawing.Printing.PageSettings pageSettings;

        public QueryPageSettingsEventArgs(System.Drawing.Printing.PageSettings pageSettings)
        {
            this.pageSettings = pageSettings;
        }

        public System.Drawing.Printing.PageSettings PageSettings
        {
            get
            {
                return this.pageSettings;
            }
            set
            {
                if (value == null)
                {
                    value = new System.Drawing.Printing.PageSettings();
                }
                this.pageSettings = value;
            }
        }
    }
}

