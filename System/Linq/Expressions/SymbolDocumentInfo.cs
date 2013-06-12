namespace System.Linq.Expressions
{
    using System;
    using System.Dynamic.Utils;
    using System.Linq.Expressions.Compiler;

    public class SymbolDocumentInfo
    {
        private readonly string _fileName;

        internal SymbolDocumentInfo(string fileName)
        {
            ContractUtils.RequiresNotNull(fileName, "fileName");
            this._fileName = fileName;
        }

        public virtual Guid DocumentType
        {
            get
            {
                return SymbolGuids.DocumentType_Text;
            }
        }

        public string FileName
        {
            get
            {
                return this._fileName;
            }
        }

        public virtual Guid Language
        {
            get
            {
                return Guid.Empty;
            }
        }

        public virtual Guid LanguageVendor
        {
            get
            {
                return Guid.Empty;
            }
        }
    }
}

