namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Xml;

    internal class ListenerElement : TypedElement
    {
        private bool _allowReferences;
        private Hashtable _attributes;
        internal bool _isAddedByDefault;
        private static readonly ConfigurationProperty _propFilter = new ConfigurationProperty("filter", typeof(FilterElement), null, ConfigurationPropertyOptions.None);
        private ConfigurationProperty _propListenerTypeName;
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propOutputOpts = new ConfigurationProperty("traceOutputOptions", typeof(TraceOptions), TraceOptions.None, ConfigurationPropertyOptions.None);

        public ListenerElement(bool allowReferences) : base(typeof(TraceListener))
        {
            this._allowReferences = allowReferences;
            ConfigurationPropertyOptions none = ConfigurationPropertyOptions.None;
            if (!this._allowReferences)
            {
                none |= ConfigurationPropertyOptions.IsRequired;
            }
            this._propListenerTypeName = new ConfigurationProperty("type", typeof(string), null, none);
            base._properties.Remove("type");
            base._properties.Add(this._propListenerTypeName);
            base._properties.Add(_propFilter);
            base._properties.Add(_propName);
            base._properties.Add(_propOutputOpts);
        }

        public override bool Equals(object compareTo)
        {
            if (!this.Name.Equals("Default") || !this.TypeName.Equals(typeof(DefaultTraceListener).FullName))
            {
                return base.Equals(compareTo);
            }
            ListenerElement element = compareTo as ListenerElement;
            return (((element != null) && element.Name.Equals("Default")) && element.TypeName.Equals(typeof(DefaultTraceListener).FullName));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public TraceListener GetRuntimeObject()
        {
            TraceListener listener2;
            if (base._runtimeObject != null)
            {
                return (TraceListener) base._runtimeObject;
            }
            try
            {
                if (string.IsNullOrEmpty(this.TypeName))
                {
                    if (((this._attributes != null) || (base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.SetHere)) || ((this.TraceOutputOptions != TraceOptions.None) || !string.IsNullOrEmpty(base.InitData)))
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Reference_listener_cant_have_properties", new object[] { this.Name }));
                    }
                    if (DiagnosticsConfiguration.SharedListeners == null)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Reference_to_nonexistent_listener", new object[] { this.Name }));
                    }
                    ListenerElement element = DiagnosticsConfiguration.SharedListeners[this.Name];
                    if (element == null)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Reference_to_nonexistent_listener", new object[] { this.Name }));
                    }
                    base._runtimeObject = element.GetRuntimeObject();
                    return (TraceListener) base._runtimeObject;
                }
                TraceListener listener = (TraceListener) base.BaseGetRuntimeObject();
                listener.initializeData = base.InitData;
                listener.Name = this.Name;
                listener.SetAttributes(this.Attributes);
                listener.TraceOutputOptions = this.TraceOutputOptions;
                if (((this.Filter != null) && (this.Filter.TypeName != null)) && (this.Filter.TypeName.Length != 0))
                {
                    listener.Filter = this.Filter.GetRuntimeObject();
                }
                base._runtimeObject = listener;
                listener2 = listener;
            }
            catch (ArgumentException exception)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Could_not_create_listener", new object[] { this.Name }), exception);
            }
            return listener2;
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            this.Attributes.Add(name, value);
            return true;
        }

        protected override void PreSerialize(XmlWriter writer)
        {
            if (this._attributes != null)
            {
                IDictionaryEnumerator enumerator = this._attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string str = (string) enumerator.Value;
                    string key = (string) enumerator.Key;
                    if ((str != null) && (writer != null))
                    {
                        writer.WriteAttributeString(key, str);
                    }
                }
            }
        }

        internal TraceListener RefreshRuntimeObject(TraceListener listener)
        {
            TraceListener listener2;
            base._runtimeObject = null;
            try
            {
                string typeName = this.TypeName;
                if (string.IsNullOrEmpty(typeName))
                {
                    if (((this._attributes != null) || (base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.SetHere)) || ((this.TraceOutputOptions != TraceOptions.None) || !string.IsNullOrEmpty(base.InitData)))
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Reference_listener_cant_have_properties", new object[] { this.Name }));
                    }
                    if (DiagnosticsConfiguration.SharedListeners == null)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Reference_to_nonexistent_listener", new object[] { this.Name }));
                    }
                    ListenerElement element = DiagnosticsConfiguration.SharedListeners[this.Name];
                    if (element == null)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Reference_to_nonexistent_listener", new object[] { this.Name }));
                    }
                    base._runtimeObject = element.RefreshRuntimeObject(listener);
                    return (TraceListener) base._runtimeObject;
                }
                if ((Type.GetType(typeName) != listener.GetType()) || (base.InitData != listener.initializeData))
                {
                    return this.GetRuntimeObject();
                }
                listener.SetAttributes(this.Attributes);
                listener.TraceOutputOptions = this.TraceOutputOptions;
                if ((base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.SetHere) || (base.ElementInformation.Properties[_propFilter.Name].ValueOrigin == PropertyValueOrigin.Inherited))
                {
                    listener.Filter = this.Filter.RefreshRuntimeObject(listener.Filter);
                }
                else
                {
                    listener.Filter = null;
                }
                base._runtimeObject = listener;
                listener2 = listener;
            }
            catch (ArgumentException exception)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Could_not_create_listener", new object[] { this.Name }), exception);
            }
            return listener2;
        }

        internal void ResetProperties()
        {
            if (this._attributes != null)
            {
                this._attributes.Clear();
                base._properties.Clear();
                base._properties.Add(this._propListenerTypeName);
                base._properties.Add(_propFilter);
                base._properties.Add(_propName);
                base._properties.Add(_propOutputOpts);
            }
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            return (base.SerializeElement(writer, serializeCollectionKey) || ((this._attributes != null) && (this._attributes.Count > 0)));
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
            ListenerElement element = sourceElement as ListenerElement;
            if ((element != null) && (element._attributes != null))
            {
                this._attributes = element._attributes;
            }
        }

        public Hashtable Attributes
        {
            get
            {
                if (this._attributes == null)
                {
                    this._attributes = new Hashtable(StringComparer.OrdinalIgnoreCase);
                }
                return this._attributes;
            }
        }

        [ConfigurationProperty("filter")]
        public FilterElement Filter
        {
            get
            {
                return (FilterElement) base[_propFilter];
            }
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("traceOutputOptions", DefaultValue=0)]
        public TraceOptions TraceOutputOptions
        {
            get
            {
                return (TraceOptions) base[_propOutputOpts];
            }
            set
            {
                base[_propOutputOpts] = value;
            }
        }

        [ConfigurationProperty("type")]
        public override string TypeName
        {
            get
            {
                return (string) base[this._propListenerTypeName];
            }
            set
            {
                base[this._propListenerTypeName] = value;
            }
        }
    }
}

