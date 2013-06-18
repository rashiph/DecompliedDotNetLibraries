namespace System.ServiceModel.Channels
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Web.Services.Description;
    using System.Xml.Serialization;

    public class XmlSerializerImportOptions
    {
        private string clrNamespace;
        private System.CodeDom.CodeCompileUnit codeCompileUnit;
        private CodeDomProvider codeProvider;
        private static CodeGenerationOptions defaultCodeGenerationOptions = (CodeGenerationOptions.GenerateOrder | CodeGenerationOptions.GenerateProperties);
        private System.Web.Services.Description.WebReferenceOptions webReferenceOptions;

        public XmlSerializerImportOptions() : this(new System.CodeDom.CodeCompileUnit())
        {
        }

        public XmlSerializerImportOptions(System.CodeDom.CodeCompileUnit codeCompileUnit)
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        public string ClrNamespace
        {
            get
            {
                return this.clrNamespace;
            }
            set
            {
                this.clrNamespace = value;
            }
        }

        public System.CodeDom.CodeCompileUnit CodeCompileUnit
        {
            get
            {
                if (this.codeCompileUnit == null)
                {
                    this.codeCompileUnit = new System.CodeDom.CodeCompileUnit();
                }
                return this.codeCompileUnit;
            }
        }

        public CodeDomProvider CodeProvider
        {
            get
            {
                if (this.codeProvider == null)
                {
                    this.codeProvider = CodeDomProvider.CreateProvider("C#");
                }
                return this.codeProvider;
            }
            set
            {
                this.codeProvider = value;
            }
        }

        public System.Web.Services.Description.WebReferenceOptions WebReferenceOptions
        {
            get
            {
                if (this.webReferenceOptions == null)
                {
                    this.webReferenceOptions = new System.Web.Services.Description.WebReferenceOptions();
                    this.webReferenceOptions.CodeGenerationOptions = defaultCodeGenerationOptions;
                }
                return this.webReferenceOptions;
            }
            set
            {
                this.webReferenceOptions = value;
            }
        }
    }
}

