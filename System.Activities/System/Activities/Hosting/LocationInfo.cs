namespace System.Activities.Hosting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class LocationInfo
    {
        internal LocationInfo(string name, string ownerDisplayName, object value)
        {
            this.Name = name;
            this.OwnerDisplayName = ownerDisplayName;
            this.Value = value;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        public string OwnerDisplayName { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        public object Value { get; private set; }
    }
}

