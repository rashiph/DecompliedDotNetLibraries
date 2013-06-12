namespace System.Linq.Expressions
{
    using System;
    using System.Linq.Expressions.Compiler;

    internal sealed class SymbolDocumentWithGuids : SymbolDocumentInfo
    {
        private readonly Guid _documentType;
        private readonly Guid _language;
        private readonly Guid _vendor;

        internal SymbolDocumentWithGuids(string fileName, ref Guid language) : base(fileName)
        {
            this._language = language;
            this._documentType = SymbolGuids.DocumentType_Text;
        }

        internal SymbolDocumentWithGuids(string fileName, ref Guid language, ref Guid vendor) : base(fileName)
        {
            this._language = language;
            this._vendor = vendor;
            this._documentType = SymbolGuids.DocumentType_Text;
        }

        internal SymbolDocumentWithGuids(string fileName, ref Guid language, ref Guid vendor, ref Guid documentType) : base(fileName)
        {
            this._language = language;
            this._vendor = vendor;
            this._documentType = documentType;
        }

        public override Guid DocumentType
        {
            get
            {
                return this._documentType;
            }
        }

        public override Guid Language
        {
            get
            {
                return this._language;
            }
        }

        public override Guid LanguageVendor
        {
            get
            {
                return this._vendor;
            }
        }
    }
}

