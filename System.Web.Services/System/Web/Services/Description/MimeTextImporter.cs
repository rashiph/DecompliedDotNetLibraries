namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    internal class MimeTextImporter : MimeImporter
    {
        private string methodName;

        internal override void GenerateCode(MimeReturn[] importedReturns, MimeParameterCollection[] importedParameters)
        {
            for (int i = 0; i < importedReturns.Length; i++)
            {
                if (importedReturns[i] is MimeTextReturn)
                {
                    this.GenerateCode((MimeTextReturn) importedReturns[i], base.ImportContext.ServiceImporter.CodeGenerationOptions);
                }
            }
        }

        private void GenerateCode(MimeTextReturn importedReturn, CodeGenerationOptions options)
        {
            this.GenerateCode(importedReturn.TypeName, importedReturn.TextBinding.Matches, options);
        }

        private void GenerateCode(string typeName, MimeTextMatchCollection matches, CodeGenerationOptions options)
        {
            CodeIdentifiers identifiers = new CodeIdentifiers();
            CodeTypeDeclaration codeClass = WebCodeGenerator.AddClass(base.ImportContext.CodeNamespace, typeName, string.Empty, new string[0], null, CodeFlags.IsPublic, base.ImportContext.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.PartialTypes));
            string[] strArray = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                string fullName;
                MimeTextMatch match = matches[i];
                string memberName = identifiers.AddUnique(CodeIdentifier.MakeValid((match.Name.Length == 0) ? (this.methodName + "Match") : match.Name), match);
                CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
                if (match.Pattern.Length == 0)
                {
                    throw new ArgumentException(Res.GetString("WebTextMatchMissingPattern"));
                }
                CodeExpression expression = new CodePrimitiveExpression(match.Pattern);
                int index = 0;
                if (match.Group != 1)
                {
                    index++;
                }
                if (match.Capture != 0)
                {
                    index++;
                }
                if (match.IgnoreCase)
                {
                    index++;
                }
                if ((match.Repeats != 1) && (match.Repeats != 0x7fffffff))
                {
                    index++;
                }
                CodeExpression[] propValues = new CodeExpression[index];
                string[] propNames = new string[propValues.Length];
                index = 0;
                if (match.Group != 1)
                {
                    propValues[index] = new CodePrimitiveExpression(match.Group);
                    propNames[index] = "Group";
                    index++;
                }
                if (match.Capture != 0)
                {
                    propValues[index] = new CodePrimitiveExpression(match.Capture);
                    propNames[index] = "Capture";
                    index++;
                }
                if (match.IgnoreCase)
                {
                    propValues[index] = new CodePrimitiveExpression(match.IgnoreCase);
                    propNames[index] = "IgnoreCase";
                    index++;
                }
                if ((match.Repeats != 1) && (match.Repeats != 0x7fffffff))
                {
                    propValues[index] = new CodePrimitiveExpression(match.Repeats);
                    propNames[index] = "MaxRepeats";
                    index++;
                }
                WebCodeGenerator.AddCustomAttribute(metadata, typeof(MatchAttribute), new CodeExpression[] { expression }, propNames, propValues);
                if (match.Matches.Count > 0)
                {
                    fullName = base.ImportContext.ClassNames.AddUnique(CodeIdentifier.MakeValid((match.Type.Length == 0) ? memberName : match.Type), match);
                    strArray[i] = fullName;
                }
                else
                {
                    fullName = typeof(string).FullName;
                }
                if (match.Repeats != 1)
                {
                    fullName = fullName + "[]";
                }
                CodeTypeMember member = WebCodeGenerator.AddMember(codeClass, fullName, memberName, null, metadata, CodeFlags.IsPublic, options);
                if ((match.Matches.Count == 0) && (match.Type.Length > 0))
                {
                    HttpProtocolImporter importContext = base.ImportContext;
                    importContext.Warnings |= ServiceDescriptionImportWarnings.OptionalExtensionsIgnored;
                    ProtocolImporter.AddWarningComment(member.Comments, Res.GetString("WebTextMatchIgnoredTypeWarning"));
                }
            }
            for (int j = 0; j < strArray.Length; j++)
            {
                string str3 = strArray[j];
                if (str3 != null)
                {
                    this.GenerateCode(str3, matches[j].Matches, options);
                }
            }
        }

        internal override MimeParameterCollection ImportParameters()
        {
            return null;
        }

        internal override MimeReturn ImportReturn()
        {
            MimeTextBinding binding = (MimeTextBinding) base.ImportContext.OperationBinding.Output.Extensions.Find(typeof(MimeTextBinding));
            if (binding == null)
            {
                return null;
            }
            if (binding.Matches.Count == 0)
            {
                base.ImportContext.UnsupportedOperationBindingWarning(Res.GetString("MissingMatchElement0"));
                return null;
            }
            this.methodName = CodeIdentifier.MakeValid(base.ImportContext.OperationBinding.Name);
            return new MimeTextReturn { TypeName = base.ImportContext.ClassNames.AddUnique(this.methodName + "Matches", binding), TextBinding = binding, ReaderType = typeof(TextReturnReader) };
        }
    }
}

