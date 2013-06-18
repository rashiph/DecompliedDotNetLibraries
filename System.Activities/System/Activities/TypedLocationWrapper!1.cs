namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class TypedLocationWrapper<T> : Location<T>
    {
        [DataMember]
        private Location innerLocation;

        public TypedLocationWrapper(Location innerLocation)
        {
            this.innerLocation = innerLocation;
        }

        public override string ToString()
        {
            return this.innerLocation.ToString();
        }

        internal override bool CanBeMapped
        {
            get
            {
                return this.innerLocation.CanBeMapped;
            }
        }

        public override T Value
        {
            get
            {
                return (T) this.innerLocation.Value;
            }
            set
            {
                this.innerLocation.Value = value;
            }
        }
    }
}

