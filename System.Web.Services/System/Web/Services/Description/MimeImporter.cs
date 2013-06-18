namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;

    internal abstract class MimeImporter
    {
        private HttpProtocolImporter protocol;

        protected MimeImporter()
        {
        }

        internal virtual void AddClassMetadata(CodeTypeDeclaration codeClass)
        {
        }

        internal virtual void GenerateCode(MimeReturn[] importedReturns, MimeParameterCollection[] importedParameters)
        {
        }

        internal abstract MimeParameterCollection ImportParameters();
        internal abstract MimeReturn ImportReturn();

        internal HttpProtocolImporter ImportContext
        {
            get
            {
                return this.protocol;
            }
            set
            {
                this.protocol = value;
            }
        }
    }
}

