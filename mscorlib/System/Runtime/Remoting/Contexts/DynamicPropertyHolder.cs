namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Globalization;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class DynamicPropertyHolder
    {
        private int _numProps;
        private IDynamicProperty[] _props;
        private IDynamicMessageSink[] _sinks;
        private const int GROW_BY = 8;

        [SecurityCritical]
        internal virtual bool AddDynamicProperty(IDynamicProperty prop)
        {
            lock (this)
            {
                CheckPropertyNameClash(prop.Name, this._props, this._numProps);
                bool flag = false;
                if ((this._props == null) || (this._numProps == this._props.Length))
                {
                    this._props = GrowPropertiesArray(this._props);
                    flag = true;
                }
                this._props[this._numProps++] = prop;
                if (flag)
                {
                    this._sinks = GrowDynamicSinksArray(this._sinks);
                }
                if (this._sinks == null)
                {
                    this._sinks = new IDynamicMessageSink[this._props.Length];
                    for (int i = 0; i < this._numProps; i++)
                    {
                        this._sinks[i] = ((IContributeDynamicSink) this._props[i]).GetDynamicSink();
                    }
                }
                else
                {
                    this._sinks[this._numProps - 1] = ((IContributeDynamicSink) prop).GetDynamicSink();
                }
                return true;
            }
        }

        [SecurityCritical]
        internal static void CheckPropertyNameClash(string name, IDynamicProperty[] props, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (props[i].Name.Equals(name))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DuplicatePropertyName"));
                }
            }
        }

        private static IDynamicMessageSink[] GrowDynamicSinksArray(IDynamicMessageSink[] sinks)
        {
            int num = ((sinks != null) ? sinks.Length : 0) + 8;
            IDynamicMessageSink[] destinationArray = new IDynamicMessageSink[num];
            if (sinks != null)
            {
                Array.Copy(sinks, destinationArray, sinks.Length);
            }
            return destinationArray;
        }

        internal static IDynamicProperty[] GrowPropertiesArray(IDynamicProperty[] props)
        {
            int num = ((props != null) ? props.Length : 0) + 8;
            IDynamicProperty[] destinationArray = new IDynamicProperty[num];
            if (props != null)
            {
                Array.Copy(props, destinationArray, props.Length);
            }
            return destinationArray;
        }

        [SecurityCritical]
        internal static void NotifyDynamicSinks(IMessage msg, ArrayWithSize dynSinks, bool bCliSide, bool bStart, bool bAsync)
        {
            for (int i = 0; i < dynSinks.Count; i++)
            {
                if (bStart)
                {
                    dynSinks.Sinks[i].ProcessMessageStart(msg, bCliSide, bAsync);
                }
                else
                {
                    dynSinks.Sinks[i].ProcessMessageFinish(msg, bCliSide, bAsync);
                }
            }
        }

        [SecurityCritical]
        internal virtual bool RemoveDynamicProperty(string name)
        {
            bool flag2;
            lock (this)
            {
                for (int i = 0; i < this._numProps; i++)
                {
                    if (this._props[i].Name.Equals(name))
                    {
                        this._props[i] = this._props[this._numProps - 1];
                        this._numProps--;
                        this._sinks = null;
                        return true;
                    }
                }
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Contexts_NoProperty"), new object[] { name }));
            }
            return flag2;
        }

        internal virtual IDynamicProperty[] DynamicProperties
        {
            get
            {
                if (this._props == null)
                {
                    return null;
                }
                lock (this)
                {
                    IDynamicProperty[] destinationArray = new IDynamicProperty[this._numProps];
                    Array.Copy(this._props, destinationArray, this._numProps);
                    return destinationArray;
                }
            }
        }

        internal virtual ArrayWithSize DynamicSinks
        {
            [SecurityCritical]
            get
            {
                if (this._numProps == 0)
                {
                    return null;
                }
                lock (this)
                {
                    if (this._sinks == null)
                    {
                        this._sinks = new IDynamicMessageSink[this._numProps + 8];
                        for (int i = 0; i < this._numProps; i++)
                        {
                            this._sinks[i] = ((IContributeDynamicSink) this._props[i]).GetDynamicSink();
                        }
                    }
                }
                return new ArrayWithSize(this._sinks, this._numProps);
            }
        }
    }
}

