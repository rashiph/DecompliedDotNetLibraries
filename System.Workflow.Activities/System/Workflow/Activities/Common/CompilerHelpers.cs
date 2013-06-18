namespace System.Workflow.Activities.Common
{
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;

    internal static class CompilerHelpers
    {
        private const string CompilerVersionKeyword = "CompilerVersion";
        private static Dictionary<Type, Dictionary<string, CodeDomProvider>> providers = null;
        private static object providersLock = new object();

        internal static CodeDomProvider CreateCodeProviderInstance(Type type)
        {
            return CreateCodeProviderInstance(type, string.Empty);
        }

        internal static CodeDomProvider CreateCodeProviderInstance(Type type, string compilerVersion)
        {
            if (string.IsNullOrEmpty(compilerVersion))
            {
                if (type == typeof(CSharpCodeProvider))
                {
                    return new CSharpCodeProvider();
                }
                if (type == typeof(VBCodeProvider))
                {
                    return new VBCodeProvider();
                }
                return (CodeDomProvider) Activator.CreateInstance(type);
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("CompilerVersion", compilerVersion);
            return (CodeDomProvider) Activator.CreateInstance(type, new object[] { dictionary });
        }

        internal static CodeDomProvider GetCodeDomProvider(System.Workflow.Activities.Common.SupportedLanguages language)
        {
            return GetCodeDomProvider(language, string.Empty);
        }

        internal static CodeDomProvider GetCodeDomProvider(System.Workflow.Activities.Common.SupportedLanguages language, string compilerVersion)
        {
            if (language == System.Workflow.Activities.Common.SupportedLanguages.CSharp)
            {
                return GetCodeProviderInstance(typeof(CSharpCodeProvider), compilerVersion);
            }
            return GetCodeProviderInstance(typeof(VBCodeProvider), compilerVersion);
        }

        private static CodeDomProvider GetCodeProviderInstance(Type type, string compilerVersion)
        {
            CodeDomProvider provider;
            lock (providersLock)
            {
                Dictionary<string, CodeDomProvider> dictionary;
                if (providers == null)
                {
                    providers = new Dictionary<Type, Dictionary<string, CodeDomProvider>>();
                }
                if (!providers.TryGetValue(type, out dictionary))
                {
                    dictionary = new Dictionary<string, CodeDomProvider>();
                    providers.Add(type, dictionary);
                }
                if (!dictionary.TryGetValue(compilerVersion, out provider))
                {
                    provider = CreateCodeProviderInstance(type, compilerVersion);
                    dictionary.Add(compilerVersion, provider);
                }
            }
            return provider;
        }

        internal static System.Workflow.Activities.Common.SupportedLanguages GetSupportedLanguage(IServiceProvider serviceProvider)
        {
            System.Workflow.Activities.Common.SupportedLanguages cSharp = System.Workflow.Activities.Common.SupportedLanguages.CSharp;
            IWorkflowCompilerOptionsService service = serviceProvider.GetService(typeof(IWorkflowCompilerOptionsService)) as IWorkflowCompilerOptionsService;
            if (service != null)
            {
                cSharp = GetSupportedLanguage(service.Language);
            }
            return cSharp;
        }

        internal static System.Workflow.Activities.Common.SupportedLanguages GetSupportedLanguage(string language)
        {
            System.Workflow.Activities.Common.SupportedLanguages cSharp = System.Workflow.Activities.Common.SupportedLanguages.CSharp;
            if (string.IsNullOrEmpty(language) || ((string.Compare(language, "VB", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(language, "VisualBasic", StringComparison.OrdinalIgnoreCase) != 0)))
            {
                return cSharp;
            }
            return System.Workflow.Activities.Common.SupportedLanguages.VB;
        }
    }
}

