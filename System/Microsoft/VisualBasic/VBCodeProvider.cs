namespace Microsoft.VisualBasic
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class VBCodeProvider : CodeDomProvider
    {
        private VBCodeGenerator generator;

        public VBCodeProvider()
        {
            this.generator = new VBCodeGenerator();
        }

        public VBCodeProvider(IDictionary<string, string> providerOptions)
        {
            if (providerOptions == null)
            {
                throw new ArgumentNullException("providerOptions");
            }
            this.generator = new VBCodeGenerator(providerOptions);
        }

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeCompiler CreateCompiler()
        {
            return this.generator;
        }

        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeGenerator CreateGenerator()
        {
            return this.generator;
        }

        public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            this.generator.GenerateCodeFromMember(member, writer, options);
        }

        public override TypeConverter GetConverter(Type type)
        {
            if (type == typeof(MemberAttributes))
            {
                return VBMemberAttributeConverter.Default;
            }
            if (type == typeof(TypeAttributes))
            {
                return VBTypeAttributeConverter.Default;
            }
            return base.GetConverter(type);
        }

        public override string FileExtension
        {
            get
            {
                return "vb";
            }
        }

        public override System.CodeDom.Compiler.LanguageOptions LanguageOptions
        {
            get
            {
                return System.CodeDom.Compiler.LanguageOptions.CaseInsensitive;
            }
        }
    }
}

