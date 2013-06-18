namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract]
    public class CorrelationMessageProperty
    {
        private ReadOnlyCollection<InstanceKey> additionalKeys;
        private InstanceKey correlationKey;
        private static readonly ReadOnlyCollection<InstanceKey> emptyInstanceKeyList = new ReadOnlyCollection<InstanceKey>(new List<InstanceKey>(0));
        private const string PropertyName = "CorrelationMessageProperty";
        private ReadOnlyCollection<InstanceKey> transientCorrelations;

        public CorrelationMessageProperty(InstanceKey correlationKey, IEnumerable<InstanceKey> additionalKeys) : this(correlationKey, additionalKeys, null)
        {
        }

        public CorrelationMessageProperty(InstanceKey correlationKey, IEnumerable<InstanceKey> additionalKeys, IEnumerable<InstanceKey> transientCorrelations)
        {
            if (correlationKey == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("correlationKey");
            }
            if (additionalKeys == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("additionalKeys");
            }
            this.correlationKey = correlationKey;
            ICollection<InstanceKey> is2 = additionalKeys as ICollection<InstanceKey>;
            if ((is2 != null) && (is2.Count == 0))
            {
                this.additionalKeys = emptyInstanceKeyList;
            }
            else
            {
                this.additionalKeys = additionalKeys as ReadOnlyCollection<InstanceKey>;
                if (this.additionalKeys == null)
                {
                    IList<InstanceKey> list = additionalKeys as IList<InstanceKey>;
                    if (list == null)
                    {
                        list = new List<InstanceKey>(additionalKeys);
                    }
                    this.additionalKeys = new ReadOnlyCollection<InstanceKey>(list);
                }
            }
            ICollection<InstanceKey> is3 = transientCorrelations as ICollection<InstanceKey>;
            if ((transientCorrelations == null) || ((is3 != null) && (is3.Count == 0)))
            {
                this.transientCorrelations = emptyInstanceKeyList;
            }
            else
            {
                this.transientCorrelations = transientCorrelations as ReadOnlyCollection<InstanceKey>;
                if (this.transientCorrelations == null)
                {
                    IList<InstanceKey> list2 = transientCorrelations as IList<InstanceKey>;
                    if (list2 == null)
                    {
                        list2 = new List<InstanceKey>(transientCorrelations);
                    }
                    this.transientCorrelations = new ReadOnlyCollection<InstanceKey>(list2);
                }
            }
        }

        public static bool TryGet(Message message, out CorrelationMessageProperty property)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out CorrelationMessageProperty property)
        {
            if (properties == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            object obj2 = null;
            if (properties.TryGetValue("CorrelationMessageProperty", out obj2))
            {
                property = obj2 as CorrelationMessageProperty;
            }
            else
            {
                property = null;
            }
            return (property != null);
        }

        public ReadOnlyCollection<InstanceKey> AdditionalKeys
        {
            get
            {
                if (this.additionalKeys == null)
                {
                    this.additionalKeys = emptyInstanceKeyList;
                }
                return this.additionalKeys;
            }
        }

        public InstanceKey CorrelationKey
        {
            get
            {
                return this.correlationKey;
            }
        }

        public static string Name
        {
            get
            {
                return "CorrelationMessageProperty";
            }
        }

        [DataMember(Name="AdditionalCorrelations", EmitDefaultValue=false)]
        internal List<InstanceKey> SerializedAdditionalKeys
        {
            get
            {
                if (this.AdditionalKeys.Count == 0)
                {
                    return null;
                }
                return new List<InstanceKey>(this.AdditionalKeys);
            }
            set
            {
                this.additionalKeys = new ReadOnlyCollection<InstanceKey>(value);
            }
        }

        [DataMember(Name="CorrelationKey", EmitDefaultValue=false)]
        internal InstanceKey SerializedCorrelationKey
        {
            get
            {
                return this.correlationKey;
            }
            set
            {
                this.correlationKey = value;
            }
        }

        [DataMember(Name="TransientCorrelations", EmitDefaultValue=false)]
        internal List<InstanceKey> SerializedTransientCorrelations
        {
            get
            {
                if (this.TransientCorrelations.Count == 0)
                {
                    return null;
                }
                return new List<InstanceKey>(this.TransientCorrelations);
            }
            set
            {
                this.transientCorrelations = new ReadOnlyCollection<InstanceKey>(value);
            }
        }

        public ReadOnlyCollection<InstanceKey> TransientCorrelations
        {
            get
            {
                if (this.transientCorrelations == null)
                {
                    this.transientCorrelations = emptyInstanceKeyList;
                }
                return this.transientCorrelations;
            }
        }
    }
}

