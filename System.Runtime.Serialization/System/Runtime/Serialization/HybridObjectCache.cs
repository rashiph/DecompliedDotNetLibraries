namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class HybridObjectCache
    {
        private Dictionary<string, object> objectDictionary;
        private Dictionary<string, object> referencedObjectDictionary;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal HybridObjectCache()
        {
        }

        internal void Add(string id, object obj)
        {
            object obj2;
            if (this.objectDictionary == null)
            {
                this.objectDictionary = new Dictionary<string, object>();
            }
            if (this.objectDictionary.TryGetValue(id, out obj2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("MultipleIdDefinition", new object[] { id })));
            }
            this.objectDictionary.Add(id, obj);
        }

        internal object GetObject(string id)
        {
            if (this.referencedObjectDictionary == null)
            {
                this.referencedObjectDictionary = new Dictionary<string, object>();
                this.referencedObjectDictionary.Add(id, null);
            }
            else if (!this.referencedObjectDictionary.ContainsKey(id))
            {
                this.referencedObjectDictionary.Add(id, null);
            }
            if (this.objectDictionary != null)
            {
                object obj2;
                this.objectDictionary.TryGetValue(id, out obj2);
                return obj2;
            }
            return null;
        }

        internal bool IsObjectReferenced(string id)
        {
            return ((this.referencedObjectDictionary != null) && this.referencedObjectDictionary.ContainsKey(id));
        }

        internal void Remove(string id)
        {
            if (this.objectDictionary != null)
            {
                this.objectDictionary.Remove(id);
            }
        }
    }
}

