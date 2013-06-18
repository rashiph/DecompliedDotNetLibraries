namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;

    internal sealed class MsmqIntegrationReceiveParameters : MsmqReceiveParameters
    {
        private MsmqMessageSerializationFormat serializationFormat;
        private System.Type[] targetSerializationTypes;

        internal MsmqIntegrationReceiveParameters(MsmqIntegrationBindingElement bindingElement) : base(bindingElement)
        {
            this.serializationFormat = bindingElement.SerializationFormat;
            List<System.Type> list = new List<System.Type>();
            if (bindingElement.TargetSerializationTypes != null)
            {
                foreach (System.Type type in bindingElement.TargetSerializationTypes)
                {
                    if (!list.Contains(type))
                    {
                        list.Add(type);
                    }
                }
            }
            this.targetSerializationTypes = list.ToArray();
        }

        internal MsmqMessageSerializationFormat SerializationFormat
        {
            get
            {
                return this.serializationFormat;
            }
        }

        internal System.Type[] TargetSerializationTypes
        {
            get
            {
                return this.targetSerializationTypes;
            }
        }
    }
}

