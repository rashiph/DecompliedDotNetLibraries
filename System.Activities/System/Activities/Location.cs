namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract, DebuggerDisplay("{Value}")]
    public abstract class Location
    {
        [DataMember(EmitDefaultValue=false)]
        private TemporaryResolutionData temporaryResolutionData;

        protected Location()
        {
        }

        internal virtual object CreateDefaultValue()
        {
            return null;
        }

        internal virtual Location CreateReference(bool bufferGets)
        {
            if (!this.CanBeMapped && !bufferGets)
            {
                return this;
            }
            return new ReferenceLocation(this, bufferGets);
        }

        internal void SetTemporaryResolutionData(LocationEnvironment resolutionEnvironment, bool bufferGetsOnCollapse)
        {
            TemporaryResolutionData data = new TemporaryResolutionData {
                TemporaryResolutionEnvironment = resolutionEnvironment,
                BufferGetsOnCollapse = bufferGetsOnCollapse
            };
            this.temporaryResolutionData = data;
        }

        internal bool BufferGetsOnCollapse
        {
            get
            {
                return this.temporaryResolutionData.BufferGetsOnCollapse;
            }
        }

        internal virtual bool CanBeMapped
        {
            get
            {
                return false;
            }
        }

        public abstract Type LocationType { get; }

        internal LocationEnvironment TemporaryResolutionEnvironment
        {
            get
            {
                return this.temporaryResolutionData.TemporaryResolutionEnvironment;
            }
        }

        public object Value
        {
            get
            {
                return this.ValueCore;
            }
            set
            {
                this.ValueCore = value;
            }
        }

        protected abstract object ValueCore { get; set; }

        [DataContract]
        private class ReferenceLocation : Location
        {
            [DataMember(EmitDefaultValue=false)]
            private object bufferedValue;
            [DataMember(EmitDefaultValue=false)]
            private bool bufferGets;
            [DataMember]
            private Location innerLocation;

            public ReferenceLocation(Location innerLocation, bool bufferGets)
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

            public override Type LocationType
            {
                get
                {
                    return this.innerLocation.LocationType;
                }
            }

            protected override object ValueCore
            {
                get
                {
                    if (this.bufferGets)
                    {
                        return this.bufferedValue;
                    }
                    return this.innerLocation.Value;
                }
                set
                {
                    this.innerLocation.Value = value;
                    this.bufferedValue = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), DataContract]
        private struct TemporaryResolutionData
        {
            [DataMember(EmitDefaultValue=false)]
            public LocationEnvironment TemporaryResolutionEnvironment { get; set; }
            [DataMember(EmitDefaultValue=false)]
            public bool BufferGetsOnCollapse { get; set; }
        }
    }
}

