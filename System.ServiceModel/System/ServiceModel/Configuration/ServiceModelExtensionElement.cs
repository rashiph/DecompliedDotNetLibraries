namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    [ConfigurationPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class ServiceModelExtensionElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        private string configurationElementName = string.Empty;
        private ContextInformation containingEvaluationContext;
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private string extensionCollectionName = string.Empty;
        private bool modified;
        private Type thisType;

        protected ServiceModelExtensionElement()
        {
        }

        [SecuritySafeCritical]
        internal bool CanAdd(string extensionCollectionName, ContextInformation evaluationContext)
        {
            bool flag = false;
            ExtensionElementCollection elements = ExtensionsSection.UnsafeLookupCollection(extensionCollectionName, evaluationContext);
            if ((elements == null) || (elements.Count == 0))
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    int num;
                    string str3;
                    if ((elements != null) && (elements.Count == 0))
                    {
                        num = 0x8001c;
                        str3 = System.ServiceModel.SR.GetString("TraceCodeExtensionCollectionIsEmpty");
                    }
                    else
                    {
                        num = 0x8001a;
                        str3 = System.ServiceModel.SR.GetString("TraceCodeExtensionCollectionDoesNotExist");
                    }
                    TraceUtility.TraceEvent(TraceEventType.Warning, num, str3, this.CreateCanAddRecord(extensionCollectionName), this, null);
                }
                return flag;
            }
            string assemblyQualifiedName = this.ThisType.AssemblyQualifiedName;
            foreach (ExtensionElement element in elements)
            {
                string str2 = element.Type;
                if (str2.Equals(assemblyQualifiedName, StringComparison.Ordinal))
                {
                    flag = true;
                    break;
                }
                if (assemblyQualifiedName.StartsWith(str2, StringComparison.Ordinal))
                {
                    Type type = Type.GetType(str2, false);
                    if ((type != null) && type.Equals(this.ThisType))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag && DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x80017, System.ServiceModel.SR.GetString("TraceCodeConfiguredExtensionTypeNotFound"), this.CreateCanAddRecord(extensionCollectionName), this, null);
            }
            return flag;
        }

        public virtual void CopyFrom(ServiceModelExtensionElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
        }

        private DictionaryTraceRecord CreateCanAddRecord(string extensionCollectionName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
            dictionary["ElementType"] = System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(this.ThisType.AssemblyQualifiedName);
            dictionary["CollectionName"] = ConfigurationStrings.ExtensionsSectionPath + "/" + extensionCollectionName;
            return new DictionaryTraceRecord(dictionary);
        }

        internal void DeserializeInternal(XmlReader reader, bool serializeCollectionKey)
        {
            this.DeserializeElement(reader, serializeCollectionKey);
        }

        internal object FromProperty(ConfigurationProperty property)
        {
            return base[property];
        }

        [SecuritySafeCritical]
        private string GetConfigurationElementName()
        {
            string name = string.Empty;
            ExtensionElementCollection elements = null;
            Type thisType = this.ThisType;
            ContextInformation containingEvaluationContext = this.ContainingEvaluationContext;
            if (containingEvaluationContext == null)
            {
                containingEvaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            }
            if (string.IsNullOrEmpty(this.extensionCollectionName))
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x8001b, System.ServiceModel.SR.GetString("TraceCodeExtensionCollectionNameNotFound"), this, (Exception) null);
                }
                elements = ExtensionsSection.UnsafeLookupAssociatedCollection(this.ThisType, containingEvaluationContext, out this.extensionCollectionName);
            }
            else
            {
                elements = ExtensionsSection.UnsafeLookupCollection(this.extensionCollectionName, containingEvaluationContext);
            }
            if (elements == null)
            {
                if (string.IsNullOrEmpty(this.extensionCollectionName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigNoExtensionCollectionAssociatedWithType", new object[] { thisType.AssemblyQualifiedName }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigExtensionCollectionNotFound", new object[] { this.extensionCollectionName }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            for (int i = 0; i < elements.Count; i++)
            {
                ExtensionElement element = elements[i];
                if (element.Type.Equals(thisType.AssemblyQualifiedName, StringComparison.Ordinal))
                {
                    name = element.Name;
                    break;
                }
                Type type = Type.GetType(element.Type, false);
                if ((null != type) && thisType.Equals(type))
                {
                    name = element.Name;
                    break;
                }
            }
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigExtensionTypeNotRegisteredInCollection", new object[] { thisType.AssemblyQualifiedName, this.extensionCollectionName }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            return name;
        }

        internal void InternalInitializeDefault()
        {
            this.InitializeDefault();
        }

        protected override bool IsModified()
        {
            return (this.modified | base.IsModified());
        }

        internal bool IsModifiedInternal()
        {
            return this.IsModified();
        }

        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);
            base.Reset(parentElement);
        }

        internal void ResetModifiedInternal()
        {
            this.ResetModified();
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            base.SerializeElement(writer, serializeCollectionKey);
            return true;
        }

        internal bool SerializeInternal(XmlWriter writer, bool serializeCollectionKey)
        {
            return this.SerializeElement(writer, serializeCollectionKey);
        }

        internal void SetReadOnlyInternal()
        {
            this.SetReadOnly();
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }

        public string ConfigurationElementName
        {
            get
            {
                if (string.IsNullOrEmpty(this.configurationElementName))
                {
                    this.configurationElementName = this.GetConfigurationElementName();
                }
                return this.configurationElementName;
            }
        }

        internal ContextInformation ContainingEvaluationContext
        {
            get
            {
                return this.containingEvaluationContext;
            }
            set
            {
                this.containingEvaluationContext = value;
            }
        }

        internal ContextInformation EvalContext
        {
            get
            {
                return base.EvaluationContext;
            }
        }

        internal string ExtensionCollectionName
        {
            get
            {
                return this.extensionCollectionName;
            }
            set
            {
                this.extensionCollectionName = value;
            }
        }

        internal ConfigurationPropertyCollection PropertiesInternal
        {
            get
            {
                return this.Properties;
            }
        }

        private Type ThisType
        {
            get
            {
                if (this.thisType == null)
                {
                    this.thisType = base.GetType();
                }
                return this.thisType;
            }
        }
    }
}

