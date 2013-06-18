namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class DictionaryMarkupSerializer : WorkflowMarkupSerializer
    {
        private bool deserializingDictionary;
        private IDictionary keylookupDictionary;

        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object parentObj, object childObj)
        {
            if (parentObj == null)
            {
                throw new ArgumentNullException("parentObj");
            }
            if (childObj == null)
            {
                throw new ArgumentNullException("childObj");
            }
            IDictionary dictionary = parentObj as IDictionary;
            if (dictionary == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_DictionarySerializerNonDictionaryObject"));
            }
            object key = null;
            foreach (DictionaryEntry entry in this.keylookupDictionary)
            {
                if ((!entry.Value.GetType().IsValueType && (entry.Value == childObj)) || (entry.Value.GetType().IsValueType && entry.Value.Equals(childObj)))
                {
                    key = entry.Key;
                    break;
                }
            }
            if (key == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_DictionarySerializerKeyNotFound", new object[] { childObj.GetType().FullName }));
            }
            dictionary.Add(key, childObj);
            this.keylookupDictionary.Remove(key);
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object deserializedObject)
        {
            if (deserializedObject == null)
            {
                throw new ArgumentNullException("deserializedObject");
            }
            IDictionary dictionary = deserializedObject as IDictionary;
            if (dictionary == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_DictionarySerializerNonDictionaryObject"));
            }
            dictionary.Clear();
        }

        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            IDictionary dictionary = obj as IDictionary;
            if (dictionary == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_DictionarySerializerNonDictionaryObject"));
            }
            List<object> list = new List<object>();
            foreach (DictionaryEntry entry in dictionary)
            {
                list.Add(entry);
            }
            return list;
        }

        internal override ExtendedPropertyInfo[] GetExtendedProperties(WorkflowMarkupSerializationManager manager, object extendee)
        {
            List<ExtendedPropertyInfo> list = new List<ExtendedPropertyInfo>();
            DictionaryEntry? nullable = null;
            if (manager.WorkflowMarkupStack[typeof(DictionaryEntry)] != null)
            {
                nullable = new DictionaryEntry?((DictionaryEntry) manager.WorkflowMarkupStack[typeof(DictionaryEntry)]);
            }
            if (this.deserializingDictionary || (nullable.HasValue && (nullable.Value.Value == extendee)))
            {
                ExtendedPropertyInfo item = new ExtendedPropertyInfo(typeof(DictionaryEntry).GetProperty("Key", BindingFlags.Public | BindingFlags.Instance), new GetValueHandler(this.OnGetKeyValue), new SetValueHandler(this.OnSetKeyValue), new GetQualifiedNameHandler(this.OnGetXmlQualifiedName), manager);
                list.Add(item);
            }
            return list.ToArray();
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            return new PropertyInfo[0];
        }

        protected override void OnAfterDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnAfterDeserialize(serializationManager, obj);
            serializationManager.ExtendedPropertiesProviders.Remove(this);
            this.keylookupDictionary = null;
            this.deserializingDictionary = false;
        }

        protected override void OnAfterSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnAfterSerialize(serializationManager, obj);
            serializationManager.ExtendedPropertiesProviders.Remove(this);
            this.keylookupDictionary = null;
        }

        internal override void OnBeforeDeserializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeDeserializeContents(serializationManager, obj);
            serializationManager.ExtendedPropertiesProviders.Add(this);
            this.keylookupDictionary = new Hashtable();
            this.deserializingDictionary = true;
        }

        internal override void OnBeforeSerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeSerializeContents(serializationManager, obj);
            serializationManager.ExtendedPropertiesProviders.Add(this);
            this.keylookupDictionary = new Hashtable();
        }

        private object OnGetKeyValue(ExtendedPropertyInfo extendedProperty, object extendee)
        {
            DictionaryEntry? nullable = null;
            if (extendedProperty.SerializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)] != null)
            {
                nullable = new DictionaryEntry?((DictionaryEntry) extendedProperty.SerializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)]);
            }
            if (nullable.HasValue && (nullable.Value.Value == extendee))
            {
                return nullable.Value.Key;
            }
            return null;
        }

        private XmlQualifiedName OnGetXmlQualifiedName(ExtendedPropertyInfo extendedProperty, WorkflowMarkupSerializationManager manager, out string prefix)
        {
            prefix = "x";
            return new XmlQualifiedName(extendedProperty.Name, "http://schemas.microsoft.com/winfx/2006/xaml");
        }

        private void OnSetKeyValue(ExtendedPropertyInfo extendedProperty, object extendee, object value)
        {
            if (((extendee != null) && (value != null)) && !this.keylookupDictionary.Contains(value))
            {
                this.keylookupDictionary.Add(value, extendee);
            }
        }

        protected internal override bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (value == null)
            {
                return false;
            }
            if (!(value is IDictionary))
            {
                throw new InvalidOperationException(SR.GetString("Error_DictionarySerializerNonDictionaryObject"));
            }
            return (((IDictionary) value).Count > 0);
        }
    }
}

