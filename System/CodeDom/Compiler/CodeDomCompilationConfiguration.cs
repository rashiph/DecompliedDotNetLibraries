namespace System.CodeDom.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Xml;

    internal class CodeDomCompilationConfiguration
    {
        internal ArrayList _allCompilerInfo;
        internal Hashtable _compilerExtensions;
        internal Hashtable _compilerLanguages;
        private static CodeDomCompilationConfiguration defaultInstance = new CodeDomCompilationConfiguration();
        private static readonly char[] s_fieldSeparators = new char[] { ';' };
        internal const string sectionName = "system.codedom";

        internal CodeDomCompilationConfiguration()
        {
            this._compilerLanguages = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this._compilerExtensions = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this._allCompilerInfo = new ArrayList();
            CompilerParameters compilerParams = new CompilerParameters {
                WarningLevel = 4
            };
            string codeDomProviderTypeName = "Microsoft.CSharp.CSharpCodeProvider, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            CompilerInfo compilerInfo = new CompilerInfo(compilerParams, codeDomProviderTypeName) {
                _compilerLanguages = new string[] { "c#", "cs", "csharp" },
                _compilerExtensions = new string[] { ".cs", "cs" },
                _providerOptions = new Dictionary<string, string>()
            };
            compilerInfo._providerOptions["CompilerVersion"] = "v4.0";
            this.AddCompilerInfo(compilerInfo);
            compilerParams = new CompilerParameters {
                WarningLevel = 4
            };
            codeDomProviderTypeName = "Microsoft.VisualBasic.VBCodeProvider, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            compilerInfo = new CompilerInfo(compilerParams, codeDomProviderTypeName) {
                _compilerLanguages = new string[] { "vb", "vbs", "visualbasic", "vbscript" },
                _compilerExtensions = new string[] { ".vb", "vb" },
                _providerOptions = new Dictionary<string, string>()
            };
            compilerInfo._providerOptions["CompilerVersion"] = "v4.0";
            this.AddCompilerInfo(compilerInfo);
            compilerParams = new CompilerParameters {
                WarningLevel = 4
            };
            codeDomProviderTypeName = "Microsoft.JScript.JScriptCodeProvider, Microsoft.JScript, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            compilerInfo = new CompilerInfo(compilerParams, codeDomProviderTypeName) {
                _compilerLanguages = new string[] { "js", "jscript", "javascript" },
                _compilerExtensions = new string[] { ".js", "js" },
                _providerOptions = new Dictionary<string, string>()
            };
            this.AddCompilerInfo(compilerInfo);
            compilerParams = new CompilerParameters {
                WarningLevel = 4
            };
            codeDomProviderTypeName = "Microsoft.VJSharp.VJSharpCodeProvider, VJSharpCodeProvider, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            compilerInfo = new CompilerInfo(compilerParams, codeDomProviderTypeName) {
                _compilerLanguages = new string[] { "vj#", "vjs", "vjsharp" },
                _compilerExtensions = new string[] { ".jsl", "jsl", ".java", "java" },
                _providerOptions = new Dictionary<string, string>()
            };
            this.AddCompilerInfo(compilerInfo);
            compilerParams = new CompilerParameters {
                WarningLevel = 4
            };
            codeDomProviderTypeName = "Microsoft.VisualC.CppCodeProvider, CppCodeProvider, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            compilerInfo = new CompilerInfo(compilerParams, codeDomProviderTypeName) {
                _compilerLanguages = new string[] { "c++", "mc", "cpp" },
                _compilerExtensions = new string[] { ".h", "h" },
                _providerOptions = new Dictionary<string, string>()
            };
            this.AddCompilerInfo(compilerInfo);
        }

        private CodeDomCompilationConfiguration(CodeDomCompilationConfiguration original)
        {
            if (original._compilerLanguages != null)
            {
                this._compilerLanguages = (Hashtable) original._compilerLanguages.Clone();
            }
            if (original._compilerExtensions != null)
            {
                this._compilerExtensions = (Hashtable) original._compilerExtensions.Clone();
            }
            if (original._allCompilerInfo != null)
            {
                this._allCompilerInfo = (ArrayList) original._allCompilerInfo.Clone();
            }
        }

        private void AddCompilerInfo(CompilerInfo compilerInfo)
        {
            foreach (string str in compilerInfo._compilerLanguages)
            {
                this._compilerLanguages[str] = compilerInfo;
            }
            foreach (string str2 in compilerInfo._compilerExtensions)
            {
                this._compilerExtensions[str2] = compilerInfo;
            }
            this._allCompilerInfo.Add(compilerInfo);
        }

        private CompilerInfo FindExistingCompilerInfo(string[] languageList, string[] extensionList)
        {
            foreach (CompilerInfo info2 in this._allCompilerInfo)
            {
                if ((info2._compilerExtensions.Length != extensionList.Length) || (info2._compilerLanguages.Length != languageList.Length))
                {
                    continue;
                }
                bool flag = false;
                for (int i = 0; i < info2._compilerExtensions.Length; i++)
                {
                    if (info2._compilerExtensions[i] != extensionList[i])
                    {
                        flag = true;
                        break;
                    }
                }
                for (int j = 0; j < info2._compilerLanguages.Length; j++)
                {
                    if (info2._compilerLanguages[j] != languageList[j])
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return info2;
                }
            }
            return null;
        }

        private void RemoveUnmapped()
        {
            for (int i = 0; i < this._allCompilerInfo.Count; i++)
            {
                ((CompilerInfo) this._allCompilerInfo[i])._mapped = false;
            }
            foreach (CompilerInfo info in this._compilerLanguages.Values)
            {
                info._mapped = true;
            }
            foreach (CompilerInfo info2 in this._compilerExtensions.Values)
            {
                info2._mapped = true;
            }
            for (int j = this._allCompilerInfo.Count - 1; j >= 0; j--)
            {
                if (!((CompilerInfo) this._allCompilerInfo[j])._mapped)
                {
                    this._allCompilerInfo.RemoveAt(j);
                }
            }
        }

        internal static CodeDomCompilationConfiguration Default
        {
            get
            {
                return defaultInstance;
            }
        }

        internal class SectionHandler
        {
            private SectionHandler()
            {
            }

            internal static object CreateStatic(object inheritedObject, XmlNode node)
            {
                CodeDomCompilationConfiguration configuration2;
                CodeDomCompilationConfiguration original = (CodeDomCompilationConfiguration) inheritedObject;
                if (original == null)
                {
                    configuration2 = new CodeDomCompilationConfiguration();
                }
                else
                {
                    configuration2 = new CodeDomCompilationConfiguration(original);
                }
                System.CodeDom.Compiler.HandlerBase.CheckForUnrecognizedAttributes(node);
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    if (!System.CodeDom.Compiler.HandlerBase.IsIgnorableAlsoCheckForNonElement(node2))
                    {
                        if (node2.Name == "compilers")
                        {
                            ProcessCompilersElement(configuration2, node2);
                        }
                        else
                        {
                            System.CodeDom.Compiler.HandlerBase.ThrowUnrecognizedElement(node2);
                        }
                    }
                }
                return configuration2;
            }

            private static IDictionary<string, string> GetProviderOptions(XmlNode compilerNode)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                foreach (XmlNode node in compilerNode)
                {
                    if (node.Name != "providerOption")
                    {
                        System.CodeDom.Compiler.HandlerBase.ThrowUnrecognizedElement(node);
                    }
                    string val = null;
                    string str2 = null;
                    System.CodeDom.Compiler.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node, "name", ref val);
                    System.CodeDom.Compiler.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node, "value", ref str2);
                    System.CodeDom.Compiler.HandlerBase.CheckForUnrecognizedAttributes(node);
                    System.CodeDom.Compiler.HandlerBase.CheckForChildNodes(node);
                    dictionary[val] = str2;
                }
                return dictionary;
            }

            private static void ProcessCompilersElement(CodeDomCompilationConfiguration result, XmlNode node)
            {
                System.CodeDom.Compiler.HandlerBase.CheckForUnrecognizedAttributes(node);
                string filename = ConfigurationErrorsException.GetFilename(node);
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    int lineNumber = ConfigurationErrorsException.GetLineNumber(node2);
                    if (!System.CodeDom.Compiler.HandlerBase.IsIgnorableAlsoCheckForNonElement(node2))
                    {
                        if (node2.Name != "compiler")
                        {
                            System.CodeDom.Compiler.HandlerBase.ThrowUnrecognizedElement(node2);
                        }
                        string val = string.Empty;
                        System.CodeDom.Compiler.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "language", ref val);
                        string str3 = string.Empty;
                        System.CodeDom.Compiler.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "extension", ref str3);
                        string str4 = null;
                        System.CodeDom.Compiler.HandlerBase.GetAndRemoveStringAttribute(node2, "type", ref str4);
                        CompilerParameters compilerParams = new CompilerParameters();
                        int num2 = 0;
                        if (System.CodeDom.Compiler.HandlerBase.GetAndRemoveNonNegativeIntegerAttribute(node2, "warningLevel", ref num2) != null)
                        {
                            compilerParams.WarningLevel = num2;
                            compilerParams.TreatWarningsAsErrors = num2 > 0;
                        }
                        string str5 = null;
                        if (System.CodeDom.Compiler.HandlerBase.GetAndRemoveStringAttribute(node2, "compilerOptions", ref str5) != null)
                        {
                            compilerParams.CompilerOptions = str5;
                        }
                        IDictionary<string, string> providerOptions = GetProviderOptions(node2);
                        System.CodeDom.Compiler.HandlerBase.CheckForUnrecognizedAttributes(node2);
                        string[] languageList = val.Split(CodeDomCompilationConfiguration.s_fieldSeparators);
                        string[] extensionList = str3.Split(CodeDomCompilationConfiguration.s_fieldSeparators);
                        for (int i = 0; i < languageList.Length; i++)
                        {
                            languageList[i] = languageList[i].Trim();
                        }
                        for (int j = 0; j < extensionList.Length; j++)
                        {
                            extensionList[j] = extensionList[j].Trim();
                        }
                        foreach (string str6 in languageList)
                        {
                            if (str6.Length == 0)
                            {
                                throw new ConfigurationErrorsException(System.SR.GetString("Language_Names_Cannot_Be_Empty"));
                            }
                        }
                        foreach (string str7 in extensionList)
                        {
                            if ((str7.Length == 0) || (str7[0] != '.'))
                            {
                                throw new ConfigurationErrorsException(System.SR.GetString("Extension_Names_Cannot_Be_Empty_Or_Non_Period_Based"));
                            }
                        }
                        CompilerInfo compilerInfo = null;
                        if (str4 != null)
                        {
                            compilerInfo = new CompilerInfo(compilerParams, str4);
                        }
                        else
                        {
                            compilerInfo = result.FindExistingCompilerInfo(languageList, extensionList);
                            if (compilerInfo == null)
                            {
                                throw new ConfigurationErrorsException();
                            }
                        }
                        compilerInfo.configFileName = filename;
                        compilerInfo.configFileLineNumber = lineNumber;
                        if (str4 != null)
                        {
                            compilerInfo._compilerLanguages = languageList;
                            compilerInfo._compilerExtensions = extensionList;
                            compilerInfo._providerOptions = providerOptions;
                            result.AddCompilerInfo(compilerInfo);
                        }
                        else
                        {
                            foreach (KeyValuePair<string, string> pair in providerOptions)
                            {
                                compilerInfo._providerOptions[pair.Key] = pair.Value;
                            }
                        }
                    }
                }
                result.RemoveUnmapped();
            }
        }
    }
}

