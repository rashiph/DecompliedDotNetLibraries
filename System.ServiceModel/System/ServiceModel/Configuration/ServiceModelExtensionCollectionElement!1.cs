namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    public abstract class ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> : ConfigurationElement, ICollection<TServiceModelExtensionElement>, IEnumerable<TServiceModelExtensionElement>, IEnumerable, IConfigurationContextProviderInternal where TServiceModelExtensionElement: ServiceModelExtensionElement
    {
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private string extensionCollectionName;
        private List<TServiceModelExtensionElement> items;
        private bool modified;
        private ConfigurationPropertyCollection properties;

        internal ServiceModelExtensionCollectionElement(string extensionCollectionName)
        {
            this.extensionCollectionName = extensionCollectionName;
        }

        public virtual void Add(TServiceModelExtensionElement element)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            element.ExtensionCollectionName = this.extensionCollectionName;
            if (this.Contains(element))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("element", System.ServiceModel.SR.GetString("ConfigDuplicateKey", new object[] { element.ConfigurationElementName }));
            }
            if (!this.CanAdd(element))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("element", System.ServiceModel.SR.GetString("ConfigElementTypeNotAllowed", new object[] { element.ConfigurationElementName, this.extensionCollectionName }));
            }
            element.ContainingEvaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            ConfigurationProperty property = new ConfigurationProperty(element.ConfigurationElementName, element.GetType(), null);
            this.Properties.Add(property);
            base[property] = element;
            this.Items.Add(element);
            this.modified = true;
        }

        internal void AddItem(TServiceModelExtensionElement element)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            element.ExtensionCollectionName = this.extensionCollectionName;
            element.ContainingEvaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            this.Items.Add(element);
            this.modified = true;
        }

        public virtual bool CanAdd(TServiceModelExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            bool flag = false;
            Type elementType = element.GetType();
            if (!this.IsReadOnly())
            {
                if (!this.ContainsKey(elementType))
                {
                    return element.CanAdd(this.extensionCollectionName, ConfigurationHelpers.GetEvaluationContext(this));
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x8001d, System.ServiceModel.SR.GetString("TraceCodeExtensionElementAlreadyExistsInCollection"), this.CreateCanAddRecord(this[elementType]), this, null);
                }
                return flag;
            }
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x80016, System.ServiceModel.SR.GetString("TraceCodeConfigurationIsReadOnly"), null, this, null);
            }
            return flag;
        }

        public void Clear()
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (this.Properties.Count > 0)
            {
                this.modified = true;
            }
            List<string> list = new List<string>(this.Items.Count);
            foreach (TServiceModelExtensionElement local in this.Items)
            {
                list.Add(local.ConfigurationElementName);
            }
            this.Items.Clear();
            foreach (string str in list)
            {
                this.Properties.Remove(str);
            }
        }

        public bool Contains(TServiceModelExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return this.ContainsKey(element.GetType());
        }

        public bool ContainsKey(string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elementName");
            }
            foreach (TServiceModelExtensionElement local in this)
            {
                if ((local != null) && local.ConfigurationElementName.Equals(elementName, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsKey(Type elementType)
        {
            if (elementType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elementType");
            }
            return (this[elementType] != null);
        }

        public void CopyTo(TServiceModelExtensionElement[] elements, int start)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            if ((start < 0) || (start >= elements.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("start", System.ServiceModel.SR.GetString("ConfigInvalidStartValue", new object[] { elements.Length - 1, start }));
            }
            foreach (TServiceModelExtensionElement local in this)
            {
                if (local != null)
                {
                    string configurationElementName = local.ConfigurationElementName;
                    TServiceModelExtensionElement local2 = this.CreateNewSection(configurationElementName);
                    if (start < elements.Length)
                    {
                        local2.CopyFrom(local);
                        elements[start] = local2;
                        start++;
                    }
                }
            }
        }

        private DictionaryTraceRecord CreateCanAddRecord(TServiceModelExtensionElement element)
        {
            return this.CreateCanAddRecord(element, new Dictionary<string, string>(3));
        }

        private DictionaryTraceRecord CreateCanAddRecord(TServiceModelExtensionElement element, Dictionary<string, string> values)
        {
            values["ElementType"] = System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(typeof(TServiceModelExtensionElement).AssemblyQualifiedName);
            values["ConfiguredSectionName"] = element.ConfigurationElementName;
            values["CollectionName"] = ConfigurationStrings.ExtensionsSectionPath + "/" + this.extensionCollectionName;
            return new DictionaryTraceRecord(values);
        }

        private TServiceModelExtensionElement CreateNewSection(string name)
        {
            if ((this.ContainsKey(name) && !(name == "clear")) && !(name == "remove"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigDuplicateItem", new object[] { name, base.GetType().Name }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            TServiceModelExtensionElement local = default(TServiceModelExtensionElement);
            Type extensionType = this.GetExtensionType(ConfigurationHelpers.GetEvaluationContext(this), name);
            if (null == extensionType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidExtensionElementName", new object[] { name, this.extensionCollectionName }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            if (!this.CollectionElementBaseType.IsAssignableFrom(extensionType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidExtensionElement", new object[] { name, this.CollectionElementBaseType.FullName }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            local = (TServiceModelExtensionElement) Activator.CreateInstance(extensionType);
            local.ExtensionCollectionName = this.extensionCollectionName;
            local.InternalInitializeDefault();
            return local;
        }

        [SecuritySafeCritical]
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            this.SetIsPresent();
            this.DeserializeElementCore(reader);
        }

        private void DeserializeElementCore(XmlReader reader)
        {
            if (reader.HasAttributes && (0 < reader.AttributeCount))
            {
                while (reader.MoveToNextAttribute())
                {
                    if (this.Properties.Contains(reader.Name))
                    {
                        base[reader.Name] = this.Properties[reader.Name].Converter.ConvertFromString(reader.Value);
                    }
                    else
                    {
                        this.OnDeserializeUnrecognizedAttribute(reader.Name, reader.Value);
                    }
                }
            }
            if (XmlNodeType.Element != reader.NodeType)
            {
                reader.MoveToElement();
            }
            XmlReader reader2 = reader.ReadSubtree();
            if (reader2.Read())
            {
                while (reader2.Read())
                {
                    if (XmlNodeType.Element == reader2.NodeType)
                    {
                        TServiceModelExtensionElement element = this.CreateNewSection(reader2.Name);
                        this.Add(element);
                        element.DeserializeInternal(reader2, false);
                    }
                }
            }
        }

        public IEnumerator<TServiceModelExtensionElement> GetEnumerator()
        {
            int iteratorVariable0 = 0;
            while (true)
            {
                if (iteratorVariable0 >= this.Items.Count)
                {
                    yield break;
                }
                TServiceModelExtensionElement iteratorVariable1 = this.items[iteratorVariable0];
                yield return iteratorVariable1;
                iteratorVariable0++;
            }
        }

        [SecuritySafeCritical]
        private Type GetExtensionType(ContextInformation evaluationContext, string name)
        {
            ExtensionElementCollection elements = ExtensionsSection.UnsafeLookupCollection(this.extensionCollectionName, evaluationContext);
            if (!elements.ContainsKey(name))
            {
                return null;
            }
            ExtensionElement element = elements[name];
            Type type = Type.GetType(element.Type, false);
            if (null == type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidType", new object[] { element.Type, element.Name }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            return type;
        }

        protected override bool IsModified()
        {
            bool modified = this.modified;
            if (!modified)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    TServiceModelExtensionElement local = this.Items[i];
                    if (local.IsModifiedInternal())
                    {
                        return true;
                    }
                }
            }
            return modified;
        }

        private static void Merge(List<TServiceModelExtensionElement> parentExtensionElements, IEnumerable<TServiceModelExtensionElement> childExtensionElements)
        {
            foreach (TServiceModelExtensionElement local in childExtensionElements)
            {
                if (local is ClearBehaviorElement)
                {
                    parentExtensionElements.Clear();
                }
                else if (local is RemoveBehaviorElement)
                {
                    Predicate<TServiceModelExtensionElement> match = null;
                    string childExtensionElementName = (local as RemoveBehaviorElement).Name;
                    if (!string.IsNullOrEmpty(childExtensionElementName))
                    {
                        if (match == null)
                        {
                            match = element => (element != null) && (element.ConfigurationElementName == childExtensionElementName);
                        }
                        parentExtensionElements.RemoveAll(match);
                    }
                }
                else
                {
                    Type childExtensionElementType = local.GetType();
                    parentExtensionElements.RemoveAll(element => (element != null) && (element.GetType() == childExtensionElementType));
                    parentExtensionElements.Add(local);
                }
            }
        }

        internal void MergeWith(List<TServiceModelExtensionElement> parentExtensionElements)
        {
            ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>.Merge(parentExtensionElements, this);
            this.Clear();
            foreach (TServiceModelExtensionElement local in parentExtensionElements)
            {
                this.Add(local);
            }
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            this.DeserializeElement(reader, false);
            return true;
        }

        public bool Remove(TServiceModelExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            bool flag = false;
            if (this.Contains(element))
            {
                string configurationElementName = element.ConfigurationElementName;
                TServiceModelExtensionElement item = this[element.GetType()];
                this.Items.Remove(item);
                this.Properties.Remove(configurationElementName);
                this.modified = true;
                flag = true;
            }
            return flag;
        }

        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> sourceElement = (ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>) parentElement;
            foreach (TServiceModelExtensionElement local in sourceElement.Items)
            {
                this.Items.Add(local);
            }
            this.UpdateProperties(sourceElement);
            this.contextHelper.OnReset(parentElement);
            base.Reset(parentElement);
        }

        protected override void ResetModified()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].ResetModifiedInternal();
            }
            this.modified = false;
        }

        protected void SetIsModified()
        {
            this.modified = true;
        }

        [SecurityCritical]
        private void SetIsPresent()
        {
            ConfigurationHelpers.SetIsPresent(this);
        }

        protected override void SetReadOnly()
        {
            base.SetReadOnly();
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].SetReadOnlyInternal();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement != null)
            {
                ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> element = (ServiceModelExtensionCollectionElement<TServiceModelExtensionElement>) sourceElement;
                this.UpdateProperties(element);
                base.Unmerge(sourceElement, parentElement, saveMode);
            }
        }

        private void UpdateProperties(ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> sourceElement)
        {
            foreach (ConfigurationProperty property in sourceElement.Properties)
            {
                if (!this.Properties.Contains(property.Name))
                {
                    this.Properties.Add(property);
                }
            }
            foreach (TServiceModelExtensionElement local in this.Items)
            {
                if (!(local is ClearBehaviorElement) && !(local is RemoveBehaviorElement))
                {
                    string configurationElementName = local.ConfigurationElementName;
                    if (!this.Properties.Contains(configurationElementName))
                    {
                        ConfigurationProperty property2 = new ConfigurationProperty(configurationElementName, local.GetType(), null);
                        this.Properties.Add(property2);
                    }
                }
            }
        }

        internal Type CollectionElementBaseType
        {
            get
            {
                return typeof(TServiceModelExtensionElement);
            }
        }

        public int Count
        {
            get
            {
                return this.Items.Count;
            }
        }

        public TServiceModelExtensionElement this[int index]
        {
            get
            {
                return this.Items[index];
            }
        }

        public TServiceModelExtensionElement this[Type extensionType]
        {
            get
            {
                if (extensionType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("extensionType");
                }
                if (!this.CollectionElementBaseType.IsAssignableFrom(extensionType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("extensionType", System.ServiceModel.SR.GetString("ConfigInvalidExtensionType", new object[] { extensionType.ToString(), this.CollectionElementBaseType.FullName, this.extensionCollectionName }));
                }
                TServiceModelExtensionElement local = default(TServiceModelExtensionElement);
                foreach (TServiceModelExtensionElement local2 in this)
                {
                    if ((local2 != null) && (local2.GetType() == extensionType))
                    {
                        local = local2;
                    }
                }
                return local;
            }
        }

        internal List<TServiceModelExtensionElement> Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new List<TServiceModelExtensionElement>();
                }
                return this.items;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ConfigurationPropertyCollection();
                }
                return this.properties;
            }
        }

        bool ICollection<TServiceModelExtensionElement>.IsReadOnly
        {
            get
            {
                return this.IsReadOnly();
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__7 : IEnumerator<TServiceModelExtensionElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TServiceModelExtensionElement <>2__current;
            public ServiceModelExtensionCollectionElement<TServiceModelExtensionElement> <>4__this;
            public TServiceModelExtensionElement <currentValue>5__9;
            public int <index>5__8;

            [DebuggerHidden]
            public <GetEnumerator>d__7(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<index>5__8 = 0;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<index>5__8++;
                        break;

                    default:
                        goto Label_0085;
                }
                if (this.<index>5__8 < this.<>4__this.Items.Count)
                {
                    this.<currentValue>5__9 = this.<>4__this.items[this.<index>5__8];
                    this.<>2__current = this.<currentValue>5__9;
                    this.<>1__state = 1;
                    return true;
                }
            Label_0085:
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            TServiceModelExtensionElement IEnumerator<TServiceModelExtensionElement>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

