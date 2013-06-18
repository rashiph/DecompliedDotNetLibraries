namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml.Context;
    using System.Xaml.MS.Impl;

    [DebuggerDisplay("{ToString()}")]
    internal class ObjectWriterFrame : XamlCommonFrame
    {
        private HashSet<XamlMember> _assignedProperties;
        private ObjectWriterFrameFlags _flags;
        private object _key;
        private Dictionary<XamlMember, object> _preconstructionPropertyValues;

        public ObjectWriterFrame()
        {
        }

        public ObjectWriterFrame(ObjectWriterFrame source) : base(source)
        {
            if (source._preconstructionPropertyValues != null)
            {
                this._preconstructionPropertyValues = new Dictionary<XamlMember, object>(source.PreconstructionPropertyValues);
            }
            if (source._assignedProperties != null)
            {
                this._assignedProperties = new HashSet<XamlMember>(source.AssignedProperties);
            }
            this._key = source._key;
            this._flags = source._flags;
            this.Instance = source.Instance;
            this.Collection = source.Collection;
            this.NameScopeDictionary = source.NameScopeDictionary;
            this.PositionalCtorArgs = source.PositionalCtorArgs;
            this.InstanceRegisteredName = source.InstanceRegisteredName;
        }

        public override XamlFrame Clone()
        {
            return new ObjectWriterFrame(this);
        }

        private bool GetFlag(ObjectWriterFrameFlags flag)
        {
            return (((byte) (this._flags & flag)) != 0);
        }

        public override void Reset()
        {
            base.Reset();
            this._preconstructionPropertyValues = null;
            this._assignedProperties = null;
            this.Instance = null;
            this.Collection = null;
            this.NameScopeDictionary = null;
            this.PositionalCtorArgs = null;
            this.InstanceRegisteredName = null;
            this._flags = ObjectWriterFrameFlags.None;
            this._key = null;
        }

        private void SetFlag(ObjectWriterFrameFlags flag, bool value)
        {
            if (value)
            {
                this._flags = (ObjectWriterFrameFlags) ((byte) (this._flags | flag));
            }
            else
            {
                this._flags = (ObjectWriterFrameFlags) ((byte) (this._flags & ((byte) ~flag)));
            }
        }

        public override string ToString()
        {
            string str = (base.XamlType == null) ? string.Empty : base.XamlType.Name;
            string str2 = (base.Member == null) ? "-" : base.Member.Name;
            string str3 = (this.Instance == null) ? "-" : ((this.Instance is string) ? this.Instance.ToString() : "*");
            string str4 = (this.Collection == null) ? "-" : "*";
            return KS.Fmt("{0}.{1} inst={2} coll={3}", new object[] { str, str2, str3, str4 });
        }

        public HashSet<XamlMember> AssignedProperties
        {
            get
            {
                if (this._assignedProperties == null)
                {
                    this._assignedProperties = new HashSet<XamlMember>();
                }
                return this._assignedProperties;
            }
        }

        public object Collection { get; set; }

        public bool HasPreconstructionPropertyValuesDictionary
        {
            get
            {
                return (this._preconstructionPropertyValues != null);
            }
        }

        public object Instance { get; set; }

        public string InstanceRegisteredName { get; set; }

        public bool IsKeySet
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.IsKeySet);
            }
            private set
            {
                this.SetFlag(ObjectWriterFrameFlags.IsKeySet, value);
            }
        }

        public bool IsObjectFromMember
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.IsObjectFromMember);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.IsObjectFromMember, value);
            }
        }

        public bool IsPropertyValueSet
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.IsPropertyValueSet);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.IsPropertyValueSet, value);
            }
        }

        public bool IsTypeConvertedObject
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.IsTypeConvertedObject);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.IsTypeConvertedObject, value);
            }
        }

        public object Key
        {
            get
            {
                FixupTargetKeyHolder holder = this._key as FixupTargetKeyHolder;
                if (holder != null)
                {
                    return holder.Key;
                }
                return this._key;
            }
            set
            {
                this._key = value;
                this.IsKeySet = true;
            }
        }

        public bool KeyIsUnconverted
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.KeyIsUnconverted);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.KeyIsUnconverted, value);
            }
        }

        public INameScopeDictionary NameScopeDictionary { get; set; }

        public object[] PositionalCtorArgs { get; set; }

        public Dictionary<XamlMember, object> PreconstructionPropertyValues
        {
            get
            {
                if (this._preconstructionPropertyValues == null)
                {
                    this._preconstructionPropertyValues = new Dictionary<XamlMember, object>();
                }
                return this._preconstructionPropertyValues;
            }
        }

        public bool ShouldConvertChildKeys
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.None | ObjectWriterFrameFlags.ShouldConvertChildKeys);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.None | ObjectWriterFrameFlags.ShouldConvertChildKeys, value);
            }
        }

        public bool ShouldNotConvertChildKeys
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.None | ObjectWriterFrameFlags.ShouldNotConvertChildKeys);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.None | ObjectWriterFrameFlags.ShouldNotConvertChildKeys, value);
            }
        }

        public bool WasAssignedAtCreation
        {
            get
            {
                return this.GetFlag(ObjectWriterFrameFlags.None | ObjectWriterFrameFlags.WasAssignedAtCreation);
            }
            set
            {
                this.SetFlag(ObjectWriterFrameFlags.None | ObjectWriterFrameFlags.WasAssignedAtCreation, value);
            }
        }

        [Flags]
        private enum ObjectWriterFrameFlags : byte
        {
            IsKeySet = 8,
            IsObjectFromMember = 2,
            IsPropertyValueSet = 4,
            IsTypeConvertedObject = 0x10,
            KeyIsUnconverted = 0x20,
            None = 0,
            ShouldConvertChildKeys = 0x40,
            ShouldNotConvertChildKeys = 0x80,
            WasAssignedAtCreation = 1
        }
    }
}

