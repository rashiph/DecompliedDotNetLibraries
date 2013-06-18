namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DataContract(Namespace="http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
    internal struct KeyValue<K, V>
    {
        private K key;
        private V value;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal KeyValue(K key, V value)
        {
            this.key = key;
            this.value = value;
        }

        [DataMember(IsRequired=true)]
        public K Key
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.key;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.key = value;
            }
        }
        [DataMember(IsRequired=true)]
        public V Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.value = value;
            }
        }
    }
}

