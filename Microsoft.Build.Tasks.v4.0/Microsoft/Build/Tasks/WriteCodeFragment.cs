namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    public class WriteCodeFragment : TaskExtension
    {
        public override bool Execute()
        {
            string str;
            if (string.IsNullOrEmpty(this.Language))
            {
                base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "Language", "WriteCodeFragment" });
                return false;
            }
            if ((this.OutputFile == null) && (this.OutputDirectory == null))
            {
                base.Log.LogErrorWithCodeFromResources("WriteCodeFragment.MustSpecifyLocation", new object[0]);
                return false;
            }
            string contents = this.GenerateCode(out str);
            if (base.Log.HasLoggedErrors)
            {
                return false;
            }
            if (contents.Length == 0)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "WriteCodeFragment.NoWorkToDo", new object[0]);
                this.OutputFile = null;
                return true;
            }
            try
            {
                if (((this.OutputFile != null) && (this.OutputDirectory != null)) && !Path.IsPathRooted(this.OutputFile.ItemSpec))
                {
                    this.OutputFile = new TaskItem(Path.Combine(this.OutputDirectory.ItemSpec, this.OutputFile.ItemSpec));
                }
                this.OutputFile = this.OutputFile ?? new TaskItem(Microsoft.Build.Shared.FileUtilities.GetTemporaryFile(this.OutputDirectory.ItemSpec, str));
                File.WriteAllText(this.OutputFile.ItemSpec, contents);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("WriteCodeFragment.CouldNotWriteOutput", new object[] { (this.OutputFile == null) ? string.Empty : this.OutputFile.ItemSpec, exception.Message });
                return false;
            }
            base.Log.LogMessageFromResources(MessageImportance.Low, "WriteCodeFragment.GeneratedFile", new object[] { this.OutputFile.ItemSpec });
            return !base.Log.HasLoggedErrors;
        }

        private string GenerateCode(out string extension)
        {
            CodeDomProvider provider;
            extension = null;
            bool flag = false;
            try
            {
                provider = CodeDomProvider.CreateProvider(this.Language);
            }
            catch (ConfigurationException exception)
            {
                base.Log.LogErrorWithCodeFromResources("WriteCodeFragment.CouldNotCreateProvider", new object[] { this.Language, exception.Message });
                return null;
            }
            catch (SecurityException exception2)
            {
                base.Log.LogErrorWithCodeFromResources("WriteCodeFragment.CouldNotCreateProvider", new object[] { this.Language, exception2.Message });
                return null;
            }
            extension = provider.FileExtension;
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace namespace2 = new CodeNamespace();
            compileUnit.Namespaces.Add(namespace2);
            string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("WriteCodeFragment.Comment", new object[] { DateTime.Now.ToString() });
            namespace2.Comments.Add(new CodeCommentStatement(text));
            if (this.AssemblyAttributes == null)
            {
                return string.Empty;
            }
            namespace2.Imports.Add(new CodeNamespaceImport("System"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            foreach (ITaskItem item in this.AssemblyAttributes)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(item.ItemSpec));
                IDictionary dictionary = item.CloneCustomMetadata();
                List<CodeAttributeArgument> list = new List<CodeAttributeArgument>(new CodeAttributeArgument[dictionary.Count + 1]);
                List<CodeAttributeArgument> list2 = new List<CodeAttributeArgument>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    string key = (string) entry.Key;
                    string str3 = (string) entry.Value;
                    if (key.StartsWith("_Parameter", StringComparison.OrdinalIgnoreCase))
                    {
                        int num;
                        if (!int.TryParse(key.Substring("_Parameter".Length), out num))
                        {
                            base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { key, "WriteCodeFragment" });
                            return null;
                        }
                        if ((num > list.Count) || (num < 1))
                        {
                            base.Log.LogErrorWithCodeFromResources("WriteCodeFragment.SkippedNumberedParameter", new object[] { num });
                            return null;
                        }
                        list[num - 1] = new CodeAttributeArgument(string.Empty, new CodePrimitiveExpression(str3));
                    }
                    else
                    {
                        list2.Add(new CodeAttributeArgument(key, new CodePrimitiveExpression(str3)));
                    }
                }
                bool flag2 = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        if (flag2)
                        {
                            base.Log.LogErrorWithCodeFromResources("WriteCodeFragment.SkippedNumberedParameter", new object[] { i + 1 });
                            return null;
                        }
                        declaration.Arguments.Add(list[i]);
                    }
                }
                foreach (CodeAttributeArgument argument in list2)
                {
                    declaration.Arguments.Add(argument);
                }
                compileUnit.AssemblyCustomAttributes.Add(declaration);
                flag = true;
            }
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb, CultureInfo.CurrentCulture))
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
            }
            string str4 = sb.ToString();
            if (!flag)
            {
                return string.Empty;
            }
            return str4;
        }

        public ITaskItem[] AssemblyAttributes { get; set; }

        [Required]
        public string Language { get; set; }

        public ITaskItem OutputDirectory { get; set; }

        [Output]
        public ITaskItem OutputFile { get; set; }
    }
}

