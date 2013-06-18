namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    internal static class ConditionHelper
    {
        private static Hashtable cloneableOrNullRulesResources = new Hashtable();
        private static Hashtable uncloneableRulesResources = new Hashtable();

        internal static object CloneObject(object original)
        {
            if (original == null)
            {
                return null;
            }
            if (original.GetType().IsValueType)
            {
                return original;
            }
            ICloneable cloneable = original as ICloneable;
            if (cloneable == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Messages.NotCloneable, new object[] { original.GetType().FullName }));
            }
            return cloneable.Clone();
        }

        internal static void CloneUserData(CodeObject original, CodeObject result)
        {
            foreach (object obj2 in original.UserData.Keys)
            {
                object key = CloneObject(obj2);
                object obj4 = CloneObject(original.UserData[obj2]);
                result.UserData.Add(key, obj4);
            }
        }

        internal static void Flush_Rules_DT(IServiceProvider serviceProvider, Activity activity)
        {
            RuleDefinitions definitions = (RuleDefinitions) activity.GetValue(RuleDefinitions.RuleDefinitionsProperty);
            if (definitions != null)
            {
                WorkflowDesignerLoader service = (WorkflowDesignerLoader) serviceProvider.GetService(typeof(WorkflowDesignerLoader));
                if (service != null)
                {
                    string filePath = string.Empty;
                    if (!string.IsNullOrEmpty(service.FileName))
                    {
                        filePath = Path.Combine(Path.GetDirectoryName(service.FileName), Path.GetFileNameWithoutExtension(service.FileName));
                    }
                    filePath = filePath + ".rules";
                    using (TextWriter writer = service.GetFileWriter(filePath))
                    {
                        if (writer != null)
                        {
                            using (XmlWriter writer2 = System.Workflow.Activities.Common.Helpers.CreateXmlWriter(writer))
                            {
                                DesignerSerializationManager serializationManager = new DesignerSerializationManager(serviceProvider);
                                using (serializationManager.CreateSession())
                                {
                                    new WorkflowMarkupSerializer().Serialize(serializationManager, writer2, definitions);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static Type GetContextType(ITypeProvider typeProvider, Activity currentActivity)
        {
            Type type = null;
            string str = string.Empty;
            Activity declaringActivity = null;
            if (System.Workflow.Activities.Common.Helpers.IsActivityLocked(currentActivity))
            {
                declaringActivity = System.Workflow.Activities.Common.Helpers.GetDeclaringActivity(currentActivity);
            }
            else
            {
                declaringActivity = System.Workflow.Activities.Common.Helpers.GetRootActivity(currentActivity);
            }
            if (declaringActivity != null)
            {
                str = declaringActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                if (!string.IsNullOrEmpty(str))
                {
                    type = typeProvider.GetType(str, false);
                }
                if (type == null)
                {
                    type = typeProvider.GetType(declaringActivity.GetType().FullName);
                }
                if (type == null)
                {
                    type = declaringActivity.GetType();
                }
            }
            return type;
        }

        internal static RuleDefinitions GetRuleDefinitionsFromManifest(Type workflowType)
        {
            if (workflowType == null)
            {
                throw new ArgumentNullException("workflowType");
            }
            RuleDefinitions definitions = null;
            if (cloneableOrNullRulesResources.ContainsKey(workflowType))
            {
                definitions = (RuleDefinitions) cloneableOrNullRulesResources[workflowType];
                if (definitions != null)
                {
                    definitions = definitions.Clone();
                }
                return definitions;
            }
            string name = workflowType.Name + ".rules";
            Stream manifestResourceStream = workflowType.Module.Assembly.GetManifestResourceStream(workflowType, name);
            if (manifestResourceStream == null)
            {
                manifestResourceStream = workflowType.Module.Assembly.GetManifestResourceStream(name);
            }
            if (manifestResourceStream != null)
            {
                using (StreamReader reader = new StreamReader(manifestResourceStream))
                {
                    using (XmlReader reader2 = XmlReader.Create(reader))
                    {
                        definitions = new WorkflowMarkupSerializer().Deserialize(reader2) as RuleDefinitions;
                    }
                }
            }
            if (!uncloneableRulesResources.ContainsKey(workflowType))
            {
                try
                {
                    RuleDefinitions definitions2 = definitions;
                    if (definitions != null)
                    {
                        definitions = definitions.Clone();
                    }
                    lock (cloneableOrNullRulesResources)
                    {
                        cloneableOrNullRulesResources[workflowType] = definitions2;
                    }
                }
                catch (Exception)
                {
                    lock (uncloneableRulesResources)
                    {
                        uncloneableRulesResources[workflowType] = null;
                    }
                }
            }
            return definitions;
        }

        internal static bool IsNonNullableValueType(Type type)
        {
            return ((type.IsValueType && !type.IsGenericType) && (type != typeof(string)));
        }

        internal static bool IsNullableValueType(Type type)
        {
            return ((type.IsValueType && type.IsGenericType) && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static RuleDefinitions Load_Rules_DT(IServiceProvider serviceProvider, DependencyObject activity)
        {
            RuleDefinitions definitions = (RuleDefinitions) activity.GetValue(RuleDefinitions.RuleDefinitionsProperty);
            if (definitions == null)
            {
                WorkflowDesignerLoader service = (WorkflowDesignerLoader) serviceProvider.GetService(typeof(WorkflowDesignerLoader));
                if (service != null)
                {
                    string filePath = string.Empty;
                    if (!string.IsNullOrEmpty(service.FileName))
                    {
                        filePath = Path.Combine(Path.GetDirectoryName(service.FileName), Path.GetFileNameWithoutExtension(service.FileName));
                    }
                    filePath = filePath + ".rules";
                    try
                    {
                        using (TextReader reader = service.GetFileReader(filePath))
                        {
                            if (reader == null)
                            {
                                definitions = new RuleDefinitions();
                            }
                            else
                            {
                                using (XmlReader reader2 = XmlReader.Create(reader))
                                {
                                    definitions = new WorkflowMarkupSerializer().Deserialize(reader2) as RuleDefinitions;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        definitions = new RuleDefinitions();
                    }
                }
                activity.SetValue(RuleDefinitions.RuleDefinitionsProperty, definitions);
            }
            return definitions;
        }

        internal static RuleDefinitions Load_Rules_RT(Activity declaringActivity)
        {
            RuleDefinitions ruleDefinitionsFromManifest = declaringActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (ruleDefinitionsFromManifest == null)
            {
                ruleDefinitionsFromManifest = GetRuleDefinitionsFromManifest(declaringActivity.GetType());
                if (ruleDefinitionsFromManifest != null)
                {
                    declaringActivity.SetValue(RuleDefinitions.RuleDefinitionsProperty, ruleDefinitionsFromManifest);
                }
            }
            return ruleDefinitionsFromManifest;
        }
    }
}

