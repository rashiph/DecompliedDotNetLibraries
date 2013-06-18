namespace System.Workflow.ComponentModel.Design
{
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class IdentifierCreationService : IIdentifierCreationService
    {
        private WorkflowDesignerLoader loader;
        private CodeDomProvider provider;
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal IdentifierCreationService(IServiceProvider serviceProvider, WorkflowDesignerLoader loader)
        {
            this.serviceProvider = serviceProvider;
            this.loader = loader;
        }

        private Type GetRootActivityType(IServiceProvider serviceProvider)
        {
            IDesignerHost service = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            string rootComponentClassName = service.RootComponentClassName;
            if (string.IsNullOrEmpty(rootComponentClassName))
            {
                return null;
            }
            ITypeProvider provider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (provider == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            return provider.GetType(rootComponentClassName, false);
        }

        private static bool IsPreBuiltActivity(Activity activity)
        {
            for (CompositeActivity activity2 = activity.Parent; activity2 != null; activity2 = activity2.Parent)
            {
                if (Helpers.IsCustomActivity(activity2))
                {
                    return true;
                }
            }
            return false;
        }

        void IIdentifierCreationService.EnsureUniqueIdentifiers(CompositeActivity parentActivity, ICollection childActivities)
        {
            if (parentActivity == null)
            {
                throw new ArgumentNullException("parentActivity");
            }
            if (childActivities == null)
            {
                throw new ArgumentNullException("childActivities");
            }
            ArrayList list = new ArrayList();
            Queue queue = new Queue(childActivities);
            while (queue.Count > 0)
            {
                Activity activity = (Activity) queue.Dequeue();
                if (activity is CompositeActivity)
                {
                    foreach (Activity activity2 in ((CompositeActivity) activity).Activities)
                    {
                        queue.Enqueue(activity2);
                    }
                }
                if ((activity.Site == null) && !IsPreBuiltActivity(activity))
                {
                    list.Add(activity);
                }
            }
            CompositeActivity rootActivity = Helpers.GetRootActivity(parentActivity) as CompositeActivity;
            StringDictionary dictionary = new StringDictionary();
            Type rootActivityType = this.GetRootActivityType(this.serviceProvider);
            if (rootActivity != null)
            {
                foreach (string str in Helpers.GetIdentifiersInCompositeActivity(rootActivity))
                {
                    dictionary[str] = str;
                }
            }
            if (rootActivityType != null)
            {
                foreach (MemberInfo info in rootActivityType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    Type c = null;
                    if (info is FieldInfo)
                    {
                        c = ((FieldInfo) info).FieldType;
                    }
                    if ((c == null) || !typeof(Activity).IsAssignableFrom(c))
                    {
                        dictionary[info.Name] = info.Name;
                    }
                }
            }
            foreach (Activity activity4 in list)
            {
                int num = 0;
                string baseIdentifier = Helpers.GetBaseIdentifier(activity4);
                string key = null;
                if (string.IsNullOrEmpty(activity4.Name) || string.Equals(activity4.Name, activity4.GetType().Name, StringComparison.Ordinal))
                {
                    key = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { baseIdentifier, ++num });
                }
                else
                {
                    key = activity4.Name;
                }
                while (dictionary.ContainsKey(key))
                {
                    key = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { baseIdentifier, ++num });
                    if (this.Provider != null)
                    {
                        key = this.Provider.CreateValidIdentifier(key);
                    }
                }
                dictionary[key] = key;
                activity4.Name = key;
            }
        }

        void IIdentifierCreationService.ValidateIdentifier(Activity activity, string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!activity.Name.ToLowerInvariant().Equals(identifier.ToLowerInvariant()))
            {
                if (this.Provider != null)
                {
                    SupportedLanguages supportedLanguage = CompilerHelpers.GetSupportedLanguage(this.serviceProvider);
                    if ((((supportedLanguage == SupportedLanguages.CSharp) && identifier.StartsWith("@", StringComparison.Ordinal)) || (((supportedLanguage == SupportedLanguages.VB) && identifier.StartsWith("[", StringComparison.Ordinal)) && identifier.EndsWith("]", StringComparison.Ordinal))) || !this.Provider.IsValidIdentifier(identifier))
                    {
                        throw new Exception(SR.GetString("Error_InvalidLanguageIdentifier", new object[] { identifier }));
                    }
                }
                StringDictionary dictionary = new StringDictionary();
                CompositeActivity rootActivity = Helpers.GetRootActivity(activity) as CompositeActivity;
                if (rootActivity != null)
                {
                    foreach (string str in Helpers.GetIdentifiersInCompositeActivity(rootActivity))
                    {
                        dictionary[str] = str;
                    }
                }
                Type rootActivityType = this.GetRootActivityType(this.serviceProvider);
                if (rootActivityType != null)
                {
                    foreach (MemberInfo info in rootActivityType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                    {
                        Type c = null;
                        if (info is FieldInfo)
                        {
                            c = ((FieldInfo) info).FieldType;
                        }
                        if ((c == null) || !typeof(Activity).IsAssignableFrom(c))
                        {
                            dictionary[info.Name] = info.Name;
                        }
                    }
                }
                if (dictionary.ContainsKey(identifier))
                {
                    throw new ArgumentException(SR.GetString("DuplicateActivityIdentifier", new object[] { identifier }));
                }
            }
        }

        internal CodeDomProvider Provider
        {
            get
            {
                if (this.provider == null)
                {
                    if (CompilerHelpers.GetSupportedLanguage(this.serviceProvider) == SupportedLanguages.CSharp)
                    {
                        this.provider = CompilerHelpers.CreateCodeProviderInstance(typeof(CSharpCodeProvider));
                    }
                    else
                    {
                        this.provider = CompilerHelpers.CreateCodeProviderInstance(typeof(VBCodeProvider));
                    }
                }
                return this.provider;
            }
        }
    }
}

