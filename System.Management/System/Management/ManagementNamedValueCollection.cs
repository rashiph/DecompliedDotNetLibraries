namespace System.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Threading;

    public class ManagementNamedValueCollection : NameObjectCollectionBase
    {
        internal event IdentifierChangedEventHandler IdentifierChanged;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementNamedValueCollection()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ManagementNamedValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public void Add(string name, object value)
        {
            try
            {
                base.BaseRemove(name);
            }
            catch
            {
            }
            base.BaseAdd(name, value);
            this.FireIdentifierChanged();
        }

        public ManagementNamedValueCollection Clone()
        {
            ManagementNamedValueCollection values = new ManagementNamedValueCollection();
            foreach (string str in this)
            {
                object obj2 = base.BaseGet(str);
                if (obj2 != null)
                {
                    if (obj2.GetType().IsByRef)
                    {
                        try
                        {
                            object obj3 = ((ICloneable) obj2).Clone();
                            values.Add(str, obj3);
                            continue;
                        }
                        catch
                        {
                            throw new NotSupportedException();
                        }
                    }
                    values.Add(str, obj2);
                }
                else
                {
                    values.Add(str, null);
                }
            }
            return values;
        }

        private void FireIdentifierChanged()
        {
            if (this.IdentifierChanged != null)
            {
                this.IdentifierChanged(this, null);
            }
        }

        internal IWbemContext GetContext()
        {
            IWbemContext context = null;
            if (0 < this.Count)
            {
                try
                {
                    context = (IWbemContext) new WbemContext();
                    foreach (string str in this)
                    {
                        object pValue = base.BaseGet(str);
                        if ((context.SetValue_(str, 0, ref pValue) & 0x80000000L) != 0L)
                        {
                            return context;
                        }
                    }
                    return context;
                }
                catch
                {
                }
            }
            return context;
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
            this.FireIdentifierChanged();
        }

        public void RemoveAll()
        {
            base.BaseClear();
            this.FireIdentifierChanged();
        }

        public object this[string name]
        {
            get
            {
                return base.BaseGet(name);
            }
        }
    }
}

