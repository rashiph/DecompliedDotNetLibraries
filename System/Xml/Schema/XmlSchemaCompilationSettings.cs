namespace System.Xml.Schema
{
    using System;

    public sealed class XmlSchemaCompilationSettings
    {
        private bool enableUpaCheck = true;

        public bool EnableUpaCheck
        {
            get
            {
                return this.enableUpaCheck;
            }
            set
            {
                this.enableUpaCheck = value;
            }
        }
    }
}

