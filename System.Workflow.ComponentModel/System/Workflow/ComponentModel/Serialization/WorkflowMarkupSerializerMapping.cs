namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    internal sealed class WorkflowMarkupSerializerMapping
    {
        private static readonly WorkflowMarkupSerializerMapping Activities;
        private string clrns;
        private static readonly WorkflowMarkupSerializerMapping ComponentModel;
        private static readonly WorkflowMarkupSerializerMapping ComponentModelDesign;
        private string prefix;
        private static readonly WorkflowMarkupSerializerMapping Rules;
        private static readonly WorkflowMarkupSerializerMapping Serialization;
        private string targetAssemblyName;
        private string unifiedAssemblyName;
        private static readonly List<WorkflowMarkupSerializerMapping> wellKnownMappings;
        private static readonly Dictionary<string, Type> wellKnownTypes = new Dictionary<string, Type>();
        private string xmlns;

        static WorkflowMarkupSerializerMapping()
        {
            wellKnownTypes.Add(typeof(ThrowActivity).Name, typeof(ThrowActivity));
            wellKnownTypes.Add(typeof(ThrowDesigner).Name, typeof(ThrowDesigner));
            wellKnownTypes.Add(typeof(SuspendActivity).Name, typeof(SuspendActivity));
            wellKnownTypes.Add(typeof(SuspendDesigner).Name, typeof(SuspendDesigner));
            wellKnownTypes.Add(typeof(CancellationHandlerActivity).Name, typeof(CancellationHandlerActivity));
            wellKnownTypes.Add(typeof(CancellationHandlerActivityDesigner).Name, typeof(CancellationHandlerActivityDesigner));
            wellKnownTypes.Add(typeof(CompensateActivity).Name, typeof(CompensateActivity));
            wellKnownTypes.Add(typeof(CompensateDesigner).Name, typeof(CompensateDesigner));
            wellKnownTypes.Add(typeof(CompensationHandlerActivity).Name, typeof(CompensationHandlerActivity));
            wellKnownTypes.Add(typeof(CompensationHandlerActivityDesigner).Name, typeof(CompensationHandlerActivityDesigner));
            wellKnownTypes.Add(typeof(FaultHandlerActivity).Name, typeof(FaultHandlerActivity));
            wellKnownTypes.Add(typeof(FaultHandlerActivityDesigner).Name, typeof(FaultHandlerActivityDesigner));
            wellKnownTypes.Add(typeof(FaultHandlersActivity).Name, typeof(FaultHandlersActivity));
            wellKnownTypes.Add(typeof(FaultHandlersActivityDesigner).Name, typeof(FaultHandlersActivityDesigner));
            wellKnownTypes.Add(typeof(SynchronizationScopeActivity).Name, typeof(SynchronizationScopeActivity));
            wellKnownTypes.Add(typeof(SequenceDesigner).Name, typeof(SequenceDesigner));
            wellKnownTypes.Add(typeof(TransactionScopeActivity).Name, typeof(TransactionScopeActivity));
            wellKnownTypes.Add(typeof(TransactionScopeActivityDesigner).Name, typeof(TransactionScopeActivityDesigner));
            wellKnownTypes.Add(typeof(PropertySegment).Name, typeof(PropertySegment));
            wellKnownTypes.Add(typeof(CompensatableTransactionScopeActivity).Name, typeof(CompensatableTransactionScopeActivity));
            wellKnownTypes.Add(typeof(ActivityDesigner).Name, typeof(ActivityDesigner));
            wellKnownMappings = new List<WorkflowMarkupSerializerMapping>();
            Activities = new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Activities", "System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            wellKnownMappings.Add(Activities);
            ComponentModel = new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.ComponentModel", Assembly.GetExecutingAssembly().FullName);
            wellKnownMappings.Add(ComponentModel);
            Serialization = new WorkflowMarkupSerializerMapping("x", "http://schemas.microsoft.com/winfx/2006/xaml", "System.Workflow.ComponentModel.Serialization", Assembly.GetExecutingAssembly().FullName);
            wellKnownMappings.Add(Serialization);
            Rules = new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Activities.Rules", "System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            wellKnownMappings.Add(Rules);
            ComponentModelDesign = new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.ComponentModel.Design", Assembly.GetExecutingAssembly().FullName);
            wellKnownMappings.Add(ComponentModelDesign);
            wellKnownMappings.Add(new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime", "System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL"));
            wellKnownMappings.Add(new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.ComponentModel.Compiler", Assembly.GetExecutingAssembly().FullName));
            wellKnownMappings.Add(new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Activities.Rules.Design", "System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            wellKnownMappings.Add(new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime.Configuration", "System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL"));
            wellKnownMappings.Add(new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime.Hosting", "System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL"));
            wellKnownMappings.Add(new WorkflowMarkupSerializerMapping("wf", "http://schemas.microsoft.com/winfx/2006/xaml/workflow", "System.Workflow.Runtime.Tracking", "System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL"));
        }

        public WorkflowMarkupSerializerMapping(string prefix, string xmlNamespace, string clrNamespace, string assemblyName)
        {
            this.xmlns = string.Empty;
            this.clrns = string.Empty;
            this.targetAssemblyName = string.Empty;
            this.prefix = string.Empty;
            this.unifiedAssemblyName = string.Empty;
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (clrNamespace == null)
            {
                throw new ArgumentNullException("clrNamespace");
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            this.prefix = prefix;
            this.xmlns = xmlNamespace;
            this.clrns = clrNamespace;
            this.targetAssemblyName = assemblyName;
            this.unifiedAssemblyName = assemblyName;
        }

        public WorkflowMarkupSerializerMapping(string prefix, string xmlNamespace, string clrNamespace, string targetAssemblyName, string unifiedAssemblyName)
        {
            this.xmlns = string.Empty;
            this.clrns = string.Empty;
            this.targetAssemblyName = string.Empty;
            this.prefix = string.Empty;
            this.unifiedAssemblyName = string.Empty;
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (clrNamespace == null)
            {
                throw new ArgumentNullException("clrNamespace");
            }
            if (targetAssemblyName == null)
            {
                throw new ArgumentNullException("targetAssemblyName");
            }
            if (unifiedAssemblyName == null)
            {
                throw new ArgumentNullException("unifiedAssemblyName");
            }
            this.prefix = prefix;
            this.xmlns = xmlNamespace;
            this.clrns = clrNamespace;
            this.targetAssemblyName = targetAssemblyName;
            this.unifiedAssemblyName = unifiedAssemblyName;
        }

        public override bool Equals(object value)
        {
            WorkflowMarkupSerializerMapping mapping = value as WorkflowMarkupSerializerMapping;
            if (mapping == null)
            {
                return false;
            }
            return (((this.clrns == mapping.clrns) && (this.targetAssemblyName == mapping.targetAssemblyName)) && (this.unifiedAssemblyName == mapping.unifiedAssemblyName));
        }

        private static string GetAssemblyName(Type type, WorkflowMarkupSerializationManager manager)
        {
            TypeProvider service = manager.GetService(typeof(ITypeProvider)) as TypeProvider;
            if (service != null)
            {
                return service.GetAssemblyName(type);
            }
            if (type.Assembly == null)
            {
                return string.Empty;
            }
            return type.Assembly.FullName;
        }

        private static string GetFormatedXmlNamespace(string clrNamespace, string assemblyName)
        {
            string str = "clr-namespace:";
            str = str + (string.IsNullOrEmpty(clrNamespace) ? "{Global}" : clrNamespace);
            if (!string.IsNullOrEmpty(assemblyName))
            {
                str = str + ";Assembly=" + assemblyName;
            }
            return str;
        }

        public override int GetHashCode()
        {
            return (this.ClrNamespace.GetHashCode() ^ this.unifiedAssemblyName.GetHashCode());
        }

        internal static void GetMappingFromType(WorkflowMarkupSerializationManager manager, Type type, out WorkflowMarkupSerializerMapping matchingMapping, out IList<WorkflowMarkupSerializerMapping> collectedMappings)
        {
            matchingMapping = null;
            collectedMappings = new List<WorkflowMarkupSerializerMapping>();
            string clrNamespace = (type.Namespace != null) ? type.Namespace : string.Empty;
            string xmlNamespace = string.Empty;
            string assemblyName = string.Empty;
            string prefix = string.Empty;
            assemblyName = GetAssemblyName(type, manager);
            if (type.Assembly.FullName.Equals("System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL", StringComparison.Ordinal))
            {
                xmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/workflow";
                prefix = "wf";
            }
            if (type.Assembly.FullName.Equals("System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.Ordinal))
            {
                xmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/workflow";
                prefix = "wf";
            }
            else if (type.Assembly == Assembly.GetExecutingAssembly())
            {
                xmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/workflow";
                prefix = "wf";
            }
            if (xmlNamespace.Length == 0)
            {
                foreach (XmlnsDefinitionAttribute attribute in type.Assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true))
                {
                    xmlNamespace = attribute.XmlNamespace;
                    assemblyName = attribute.AssemblyName;
                    if (type.Assembly == manager.LocalAssembly)
                    {
                        assemblyName = string.Empty;
                    }
                    else if (string.IsNullOrEmpty(assemblyName))
                    {
                        assemblyName = GetAssemblyName(type, manager);
                    }
                    if (string.IsNullOrEmpty(xmlNamespace))
                    {
                        xmlNamespace = GetFormatedXmlNamespace(clrNamespace, assemblyName);
                    }
                    prefix = GetPrefix(manager, type.Assembly, xmlNamespace);
                    WorkflowMarkupSerializerMapping item = new WorkflowMarkupSerializerMapping(prefix, xmlNamespace, clrNamespace, assemblyName, type.Assembly.FullName);
                    if (attribute.ClrNamespace.Equals(clrNamespace, StringComparison.Ordinal) && (matchingMapping == null))
                    {
                        matchingMapping = item;
                    }
                    else
                    {
                        collectedMappings.Add(item);
                    }
                }
            }
            if (matchingMapping == null)
            {
                if (type.Assembly == manager.LocalAssembly)
                {
                    assemblyName = string.Empty;
                }
                else if (string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = GetAssemblyName(type, manager);
                }
                xmlNamespace = GetFormatedXmlNamespace(clrNamespace, assemblyName);
                if (string.IsNullOrEmpty(prefix))
                {
                    prefix = GetPrefix(manager, type.Assembly, xmlNamespace);
                }
                matchingMapping = new WorkflowMarkupSerializerMapping(prefix, xmlNamespace, clrNamespace, assemblyName, type.Assembly.FullName);
            }
        }

        internal static void GetMappingsFromXmlNamespace(WorkflowMarkupSerializationManager serializationManager, string xmlNamespace, out IList<WorkflowMarkupSerializerMapping> matchingMappings, out IList<WorkflowMarkupSerializerMapping> collectedMappings)
        {
            matchingMappings = new List<WorkflowMarkupSerializerMapping>();
            collectedMappings = new List<WorkflowMarkupSerializerMapping>();
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader != null)
            {
                if (xmlNamespace.StartsWith("clr-namespace:", StringComparison.OrdinalIgnoreCase))
                {
                    bool flag = false;
                    string clrNamespace = xmlNamespace.Substring("clr-namespace:".Length).Trim();
                    string assemblyName = string.Empty;
                    int index = clrNamespace.IndexOf(';');
                    if (index != -1)
                    {
                        assemblyName = ((index + 1) < clrNamespace.Length) ? clrNamespace.Substring(index + 1).Trim() : string.Empty;
                        clrNamespace = clrNamespace.Substring(0, index).Trim();
                        if (!assemblyName.StartsWith("Assembly=", StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                        }
                        assemblyName = assemblyName.Substring("Assembly=".Length);
                    }
                    if (!flag)
                    {
                        if (clrNamespace.Equals("{Global}", StringComparison.OrdinalIgnoreCase))
                        {
                            clrNamespace = string.Empty;
                        }
                        matchingMappings.Add(new WorkflowMarkupSerializerMapping(reader.Prefix, xmlNamespace, clrNamespace, assemblyName));
                    }
                }
                else
                {
                    List<Assembly> list = new List<Assembly>();
                    if (serializationManager.LocalAssembly != null)
                    {
                        list.Add(serializationManager.LocalAssembly);
                    }
                    ITypeProvider service = serializationManager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (service != null)
                    {
                        list.AddRange(service.ReferencedAssemblies);
                    }
                    foreach (Assembly assembly in list)
                    {
                        object[] customAttributes = assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true);
                        if (customAttributes != null)
                        {
                            foreach (XmlnsDefinitionAttribute attribute in customAttributes)
                            {
                                string fullName = string.Empty;
                                if (serializationManager.LocalAssembly != assembly)
                                {
                                    if ((attribute.AssemblyName != null) && (attribute.AssemblyName.Trim().Length > 0))
                                    {
                                        fullName = attribute.AssemblyName;
                                    }
                                    else
                                    {
                                        fullName = assembly.FullName;
                                    }
                                }
                                if (attribute.XmlNamespace.Equals(xmlNamespace, StringComparison.Ordinal))
                                {
                                    matchingMappings.Add(new WorkflowMarkupSerializerMapping(reader.Prefix, xmlNamespace, attribute.ClrNamespace, fullName));
                                }
                                else
                                {
                                    collectedMappings.Add(new WorkflowMarkupSerializerMapping(reader.Prefix, xmlNamespace, attribute.ClrNamespace, fullName));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string GetPrefix(WorkflowMarkupSerializationManager manager, Assembly assembly, string xmlNamespace)
        {
            string prefix = string.Empty;
            object[] customAttributes = assembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), true);
            if (customAttributes != null)
            {
                foreach (XmlnsPrefixAttribute attribute in customAttributes)
                {
                    if (attribute.XmlNamespace.Equals(xmlNamespace, StringComparison.Ordinal))
                    {
                        prefix = attribute.Prefix;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(prefix) || !IsNamespacePrefixUnique(prefix, manager.PrefixBasedMappings.Keys))
            {
                string str2 = string.IsNullOrEmpty(prefix) ? "ns" : prefix;
                int num = 0;
                prefix = str2 + string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { num++ });
                while (!IsNamespacePrefixUnique(prefix, manager.PrefixBasedMappings.Keys))
                {
                    prefix = str2 + string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { num++ });
                }
            }
            return prefix;
        }

        private static bool IsNamespacePrefixUnique(string prefix, ICollection existingPrefixes)
        {
            foreach (string str in existingPrefixes)
            {
                if (str.Equals(prefix, StringComparison.Ordinal))
                {
                    return false;
                }
            }
            return true;
        }

        internal static Type ResolveWellKnownTypes(WorkflowMarkupSerializationManager manager, string xmlns, string typeName)
        {
            Type type = null;
            List<WorkflowMarkupSerializerMapping> list = new List<WorkflowMarkupSerializerMapping>();
            if (xmlns.Equals("http://schemas.microsoft.com/winfx/2006/xaml/workflow", StringComparison.Ordinal))
            {
                if (!wellKnownTypes.TryGetValue(typeName, out type))
                {
                    if (typeName.EndsWith("Activity", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(Activities);
                        list.Add(ComponentModel);
                    }
                    if (typeName.EndsWith("Designer", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(Activities);
                        list.Add(ComponentModel);
                        list.Add(ComponentModelDesign);
                    }
                    else if (typeName.EndsWith("Theme", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(ComponentModelDesign);
                        list.Add(Activities);
                    }
                    else if (typeName.StartsWith("Rule", StringComparison.OrdinalIgnoreCase) || typeName.EndsWith("Action", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(Rules);
                    }
                }
            }
            else if (xmlns.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.Ordinal))
            {
                list.Add(Serialization);
            }
            if (type == null)
            {
                foreach (WorkflowMarkupSerializerMapping mapping in list)
                {
                    string str = mapping.ClrNamespace + "." + typeName + ", " + mapping.AssemblyName;
                    type = manager.GetType(str);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return type;
        }

        public string AssemblyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetAssemblyName;
            }
        }

        public string ClrNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clrns;
            }
        }

        public string Prefix
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.prefix;
            }
        }

        internal static IList<WorkflowMarkupSerializerMapping> WellKnownMappings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return wellKnownMappings;
            }
        }

        public string XmlNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlns;
            }
        }
    }
}

