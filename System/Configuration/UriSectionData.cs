namespace System.Configuration
{
    using System;
    using System.Collections.Generic;

    internal sealed class UriSectionData
    {
        private UriIdnScope? idnScope;
        private bool? iriParsing;
        private Dictionary<string, SchemeSettingInternal> schemeSettings = new Dictionary<string, SchemeSettingInternal>();

        public UriIdnScope? IdnScope
        {
            get
            {
                return this.idnScope;
            }
            set
            {
                this.idnScope = value;
            }
        }

        public bool? IriParsing
        {
            get
            {
                return this.iriParsing;
            }
            set
            {
                this.iriParsing = value;
            }
        }

        public Dictionary<string, SchemeSettingInternal> SchemeSettings
        {
            get
            {
                return this.schemeSettings;
            }
        }
    }
}

