namespace System.Configuration
{
    using System;

    internal class DefinitionUpdate : Update
    {
        private System.Configuration.SectionRecord _sectionRecord;

        internal DefinitionUpdate(string configKey, bool moved, string updatedXml, System.Configuration.SectionRecord sectionRecord) : base(configKey, moved, updatedXml)
        {
            this._sectionRecord = sectionRecord;
        }

        internal System.Configuration.SectionRecord SectionRecord
        {
            get
            {
                return this._sectionRecord;
            }
        }
    }
}

