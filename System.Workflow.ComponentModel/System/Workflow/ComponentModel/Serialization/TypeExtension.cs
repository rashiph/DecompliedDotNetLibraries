namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Xml;

    [DesignerSerializer(typeof(TypeExtensionSerializer), typeof(WorkflowMarkupSerializer))]
    internal sealed class TypeExtension : MarkupExtension
    {
        private System.Type type;
        private string typeName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TypeExtension()
        {
        }

        public TypeExtension(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("typeName");
            }
            this.typeName = type;
        }

        public TypeExtension(System.Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.type = type;
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            if (this.type != null)
            {
                return this.type;
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (this.typeName == null)
            {
                throw new InvalidOperationException("typename");
            }
            WorkflowMarkupSerializationManager manager = provider as WorkflowMarkupSerializationManager;
            if (manager == null)
            {
                throw new ArgumentNullException("provider");
            }
            XmlReader reader = manager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader != null)
            {
                string name = this.typeName.Trim();
                string prefix = string.Empty;
                int index = name.IndexOf(':');
                if (index >= 0)
                {
                    prefix = name.Substring(0, index);
                    name = name.Substring(index + 1);
                    this.type = manager.GetType(new XmlQualifiedName(name, reader.LookupNamespace(prefix)));
                    if (this.type != null)
                    {
                        return this.type;
                    }
                    List<WorkflowMarkupSerializerMapping> list = null;
                    if ((manager.XmlNamespaceBasedMappings.TryGetValue(reader.LookupNamespace(prefix), out list) && (list != null)) && (list.Count > 0))
                    {
                        return (list[0].ClrNamespace + "." + name);
                    }
                    return name;
                }
                this.type = manager.GetType(new XmlQualifiedName(name, reader.LookupNamespace(string.Empty)));
                if (this.type == null)
                {
                    ITypeProvider service = provider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (service != null)
                    {
                        this.type = service.GetType(name);
                    }
                    if ((this.type == null) && (manager.GetService(typeof(ITypeResolutionService)) == null))
                    {
                        this.type = manager.SerializationManager.GetType(name);
                    }
                }
                if (this.type != null)
                {
                    return this.type;
                }
            }
            return this.typeName;
        }

        internal System.Type Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.type;
            }
        }

        [DefaultValue((string) null), ConstructorArgument("type")]
        public string TypeName
        {
            get
            {
                if (this.type != null)
                {
                    return this.type.FullName;
                }
                return this.typeName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.typeName = value;
            }
        }
    }
}

