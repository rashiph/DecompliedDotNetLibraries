namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Security.Cryptography;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    internal static class WorkflowMarkupSerializationHelpers
    {
        internal static string[] standardNamespaces = new string[] { "System", "System.Collections", "System.ComponentModel", "System.ComponentModel.Design", "System.Collections.Generic", "System.Workflow.ComponentModel", "System.Workflow.Runtime", "System.Workflow.Activities" };

        internal static void FixStandardNamespacesAndRootNamespace(CodeNamespaceCollection codeNamespaces, string rootNS, SupportedLanguages language)
        {
            if (language == SupportedLanguages.VB)
            {
                foreach (CodeNamespace namespace2 in codeNamespaces)
                {
                    if (namespace2.Name == rootNS)
                    {
                        namespace2.Name = string.Empty;
                        namespace2.UserData.Add("TruncatedNamespace", null);
                    }
                    else if (namespace2.Name.StartsWith(rootNS + ".", StringComparison.Ordinal))
                    {
                        namespace2.Name = namespace2.Name.Substring(rootNS.Length + 1);
                        namespace2.UserData.Add("TruncatedNamespace", null);
                    }
                }
            }
            foreach (CodeNamespace namespace3 in codeNamespaces)
            {
                Hashtable hashtable = new Hashtable();
                foreach (CodeNamespaceImport import in namespace3.Imports)
                {
                    hashtable.Add(import.Namespace, import);
                }
                foreach (string str in standardNamespaces)
                {
                    if (!hashtable.Contains(str))
                    {
                        namespace3.Imports.Add(new CodeNamespaceImport(str));
                    }
                }
            }
        }

        internal static CodeNamespaceCollection GenerateCodeFromXomlDocument(Activity rootActivity, string filePath, string rootNamespace, SupportedLanguages language, IServiceProvider serviceProvider)
        {
            CodeNamespaceCollection namespaces = new CodeNamespaceCollection();
            CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(language);
            string str = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
            CodeTypeDeclaration declaration = null;
            if ((codeDomProvider != null) && !string.IsNullOrEmpty(str))
            {
                string str2;
                string str3;
                Helpers.GetNamespaceAndClassName(str, out str2, out str3);
                if (codeDomProvider.IsValidIdentifier(str3))
                {
                    DesignerSerializationManager manager = new DesignerSerializationManager(serviceProvider);
                    using (manager.CreateSession())
                    {
                        ActivityCodeDomSerializationManager manager2 = new ActivityCodeDomSerializationManager(manager);
                        TypeCodeDomSerializer serializer = manager2.GetSerializer(rootActivity.GetType(), typeof(TypeCodeDomSerializer)) as TypeCodeDomSerializer;
                        bool flag = true;
                        ArrayList list = new ArrayList();
                        list.Add(rootActivity);
                        if (rootActivity is CompositeActivity)
                        {
                            foreach (Activity activity in Helpers.GetNestedActivities((CompositeActivity) rootActivity))
                            {
                                if (!Helpers.IsActivityLocked(activity))
                                {
                                    if (codeDomProvider.IsValidIdentifier(manager2.GetName(activity)))
                                    {
                                        list.Insert(0, activity);
                                    }
                                    else
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            DummySite site = new DummySite();
                            foreach (Activity activity2 in list)
                            {
                                activity2.Site = site;
                            }
                            rootActivity.Site = site;
                            declaration = serializer.Serialize(manager2, rootActivity, list);
                            declaration.IsPartial = true;
                            if ((filePath != null) && (filePath.Length > 0))
                            {
                                MD5 md = new MD5CryptoServiceProvider();
                                byte[] buffer = null;
                                using (StreamReader reader = new StreamReader(filePath))
                                {
                                    buffer = md.ComputeHash(reader.BaseStream);
                                }
                                string str4 = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}", new object[] { buffer[0].ToString("X2", CultureInfo.InvariantCulture), buffer[1].ToString("X2", CultureInfo.InvariantCulture), buffer[2].ToString("X2", CultureInfo.InvariantCulture), buffer[3].ToString("X2", CultureInfo.InvariantCulture), buffer[4].ToString("X2", CultureInfo.InvariantCulture), buffer[5].ToString("X2", CultureInfo.InvariantCulture), buffer[6].ToString("X2", CultureInfo.InvariantCulture), buffer[7].ToString("X2", CultureInfo.InvariantCulture), buffer[8].ToString("X2", CultureInfo.InvariantCulture), buffer[9].ToString("X2", CultureInfo.InvariantCulture), buffer[10].ToString("X2", CultureInfo.InvariantCulture), buffer[11].ToString("X2", CultureInfo.InvariantCulture), buffer[12].ToString("X2", CultureInfo.InvariantCulture), buffer[13].ToString("X2", CultureInfo.InvariantCulture), buffer[14].ToString("X2", CultureInfo.InvariantCulture), buffer[15].ToString("X2", CultureInfo.InvariantCulture) });
                                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(WorkflowMarkupSourceAttribute).FullName);
                                declaration2.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(filePath)));
                                declaration2.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(str4)));
                                declaration.CustomAttributes.Add(declaration2);
                            }
                            CodeNamespace namespace2 = new CodeNamespace(str2);
                            namespace2.Types.Add(declaration);
                            namespaces.Add(namespace2);
                        }
                    }
                }
            }
            if (declaration != null)
            {
                Queue queue = new Queue(new object[] { rootActivity });
                while (queue.Count > 0)
                {
                    Activity activity3 = (Activity) queue.Dequeue();
                    if (!Helpers.IsActivityLocked(activity3))
                    {
                        Queue queue2 = new Queue(new object[] { activity3 });
                        while (queue2.Count > 0)
                        {
                            Activity activity4 = (Activity) queue2.Dequeue();
                            if (activity4 is CompositeActivity)
                            {
                                foreach (Activity activity5 in ((CompositeActivity) activity4).Activities)
                                {
                                    queue2.Enqueue(activity5);
                                }
                            }
                            CodeTypeMemberCollection members = activity4.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
                            if (members != null)
                            {
                                foreach (CodeSnippetTypeMember member in members)
                                {
                                    declaration.Members.Add(member);
                                }
                            }
                        }
                    }
                }
                if (language == SupportedLanguages.CSharp)
                {
                    declaration.LinePragma = new CodeLinePragma((string) rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int) rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                }
                CodeConstructor constructor = null;
                CodeMemberMethod method = null;
                foreach (CodeTypeMember member2 in declaration.Members)
                {
                    if ((constructor == null) && (member2 is CodeConstructor))
                    {
                        constructor = member2 as CodeConstructor;
                    }
                    if (((method == null) && (member2 is CodeMemberMethod)) && member2.Name.Equals("InitializeComponent", StringComparison.Ordinal))
                    {
                        method = member2 as CodeMemberMethod;
                    }
                    if ((constructor != null) && (method != null))
                    {
                        break;
                    }
                }
                if (constructor != null)
                {
                    constructor.LinePragma = new CodeLinePragma((string) rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int) rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                }
                if ((method != null) && (language == SupportedLanguages.CSharp))
                {
                    method.LinePragma = new CodeLinePragma((string) rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int) rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                }
            }
            List<string> list2 = rootActivity.GetValue(WorkflowMarkupSerializer.ClrNamespacesProperty) as List<string>;
            if (list2 != null)
            {
                foreach (CodeNamespace namespace3 in namespaces)
                {
                    foreach (string str5 in list2)
                    {
                        if (!string.IsNullOrEmpty(str5))
                        {
                            CodeNamespaceImport import = new CodeNamespaceImport(str5) {
                                LinePragma = new CodeLinePragma((string) rootActivity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int) rootActivity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1))
                            };
                            namespace3.Imports.Add(import);
                        }
                    }
                }
            }
            return namespaces;
        }

        internal static string GetEventHandlerName(object owner, string eventName)
        {
            string str = null;
            DependencyObject obj2 = owner as DependencyObject;
            if ((!string.IsNullOrEmpty(eventName) && (owner != null)) && ((obj2 != null) && (obj2.GetValue(WorkflowMarkupSerializer.EventsProperty) != null)))
            {
                Hashtable hashtable = obj2.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                if ((hashtable != null) && hashtable.ContainsKey(eventName))
                {
                    str = hashtable[eventName] as string;
                }
            }
            return str;
        }

        public static Activity LoadXomlDocument(WorkflowMarkupSerializationManager xomlSerializationManager, XmlReader textReader, string fileName)
        {
            if (xomlSerializationManager == null)
            {
                throw new ArgumentNullException("xomlSerializationManager");
            }
            Activity activity = null;
            try
            {
                xomlSerializationManager.Context.Push(fileName);
                activity = new WorkflowMarkupSerializer().Deserialize(xomlSerializationManager, textReader) as Activity;
            }
            finally
            {
                xomlSerializationManager.Context.Pop();
            }
            return activity;
        }

        internal static void ProcessDefTag(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, Activity activity, bool newSegment, string fileName)
        {
            ResourceManager manager = new ResourceManager("System.Workflow.ComponentModel.StringResources", typeof(ActivityBind).Assembly);
            if (reader.NodeType == XmlNodeType.Attribute)
            {
                string str;
                if (((str = reader.LocalName) != null) && (str == "Class"))
                {
                    activity.SetValue(WorkflowMarkupSerializer.XClassProperty, reader.Value);
                }
                else
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(string.Format(CultureInfo.CurrentCulture, manager.GetString("UnknownDefinitionTag"), new object[] { "x", reader.LocalName, "http://schemas.microsoft.com/winfx/2006/xaml" }), (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1, (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1));
                }
            }
            else
            {
                bool flag = false;
                bool isEmptyElement = reader.IsEmptyElement;
                int depth = reader.Depth;
                do
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            string localName = reader.LocalName;
                            if (localName != null)
                            {
                                if (localName == "Code")
                                {
                                    if (isEmptyElement)
                                    {
                                        flag = true;
                                    }
                                    break;
                                }
                                bool flag1 = localName == "Constructor";
                            }
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(string.Format(CultureInfo.CurrentCulture, manager.GetString("UnknownDefinitionTag"), new object[] { "x", reader.LocalName, "http://schemas.microsoft.com/winfx/2006/xaml" }), (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1, (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1));
                            return;
                        }
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        {
                            int num2 = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1;
                            int num3 = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1;
                            CodeSnippetTypeMember member = new CodeSnippetTypeMember(reader.Value) {
                                LinePragma = new CodeLinePragma(fileName, Math.Max(num2 - 1, 1))
                            };
                            member.UserData[UserDataKeys.CodeSegment_New] = newSegment;
                            member.UserData[UserDataKeys.CodeSegment_ColumnNumber] = (num3 + reader.Name.Length) - 1;
                            CodeTypeMemberCollection members = activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
                            if (members == null)
                            {
                                members = new CodeTypeMemberCollection();
                                activity.SetValue(WorkflowMarkupSerializer.XCodeProperty, members);
                            }
                            members.Add(member);
                            break;
                        }
                        case XmlNodeType.EndElement:
                            if (reader.Depth == depth)
                            {
                                flag = true;
                            }
                            break;
                    }
                }
                while (!flag && reader.Read());
            }
        }

        internal static void ReapplyRootNamespace(CodeNamespaceCollection codeNamespaces, string rootNS, SupportedLanguages language)
        {
            if (language == SupportedLanguages.VB)
            {
                foreach (CodeNamespace namespace2 in codeNamespaces)
                {
                    if (namespace2.UserData.Contains("TruncatedNamespace"))
                    {
                        if ((namespace2.Name == null) || (namespace2.Name.Length == 0))
                        {
                            namespace2.Name = rootNS;
                        }
                        else if (namespace2.Name.StartsWith(rootNS + ".", StringComparison.Ordinal))
                        {
                            namespace2.Name = rootNS + "." + namespace2.Name;
                        }
                        namespace2.UserData.Remove("TruncatedNamespace");
                    }
                }
            }
        }

        internal static void SetEventHandlerName(object owner, string eventName, string value)
        {
            DependencyObject obj2 = owner as DependencyObject;
            if ((!string.IsNullOrEmpty(eventName) && (owner != null)) && (obj2 != null))
            {
                if (obj2.GetValue(WorkflowMarkupSerializer.EventsProperty) == null)
                {
                    obj2.SetValue(WorkflowMarkupSerializer.EventsProperty, new Hashtable());
                }
                Hashtable hashtable = obj2.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                hashtable[eventName] = value;
            }
        }

        private class DummySite : ISite, IServiceProvider
        {
            public object GetService(Type type)
            {
                return null;
            }

            public IComponent Component
            {
                get
                {
                    return null;
                }
            }

            public IContainer Container
            {
                get
                {
                    return null;
                }
            }

            public bool DesignMode
            {
                get
                {
                    return true;
                }
            }

            public string Name
            {
                get
                {
                    return string.Empty;
                }
                set
                {
                }
            }
        }
    }
}

