namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Xml;

    internal sealed class StringCollectionMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return (value is ICollection<string>);
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.IsValidCompactAttributeFormat(value))
            {
                return base.DeserializeFromCompactFormat(serializationManager, serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader, value);
            }
            return SynchronizationHandlesTypeConverter.UnStringify(value);
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager manager, object obj)
        {
            return new PropertyInfo[0];
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return SynchronizationHandlesTypeConverter.Stringify(value as ICollection<string>);
        }
    }
}

