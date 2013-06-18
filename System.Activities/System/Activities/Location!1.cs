namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    public class Location<T> : System.Activities.Location
    {
        [DataMember(EmitDefaultValue=false)]
        private T value;

        internal override object CreateDefaultValue()
        {
            return Activator.CreateInstance<T>();
        }

        internal override System.Activities.Location CreateReference(bool bufferGets)
        {
            if (!this.CanBeMapped && !bufferGets)
            {
                return this;
            }
            return new ReferenceLocation<T>((Location<T>) this, bufferGets);
        }

        public override string ToString()
        {
            if (this.value == null)
            {
                return "<null>";
            }
            return this.value.ToString();
        }

        public override Type LocationType
        {
            get
            {
                return typeof(T);
            }
        }

        internal T TypedValue
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = value;
            }
        }

        public virtual T Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        protected sealed override object ValueCore
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = TypeHelper.Convert<T>(value);
            }
        }

        [DataContract]
        private class ReferenceLocation : Location<T>
        {
            [DataMember(EmitDefaultValue=false)]
            private bool bufferGets;
            [DataMember]
            private Location<T> innerLocation;

            public ReferenceLocation(Location<T> innerLocation, bool bufferGets)
            {
                this.innerLocation = innerLocation;
                this.bufferGets = bufferGets;
            }

            public override string ToString()
            {
                if (this.bufferGets)
                {
                    return base.ToString();
                }
                return this.innerLocation.ToString();
            }

            public override T Value
            {
                get
                {
                    if (this.bufferGets)
                    {
                        return base.value;
                    }
                    return this.innerLocation.Value;
                }
                set
                {
                    this.innerLocation.Value = value;
                    if (this.bufferGets)
                    {
                        base.value = value;
                    }
                }
            }
        }
    }
}

