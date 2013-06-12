namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal class RemotingCachedData
    {
        private SoapAttribute _soapAttr;
        protected object RI;

        internal RemotingCachedData(RuntimeConstructorInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(RuntimeEventInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(RuntimeFieldInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(RuntimeMethodInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(RuntimeParameterInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(RuntimePropertyInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(SerializationFieldInfo ri)
        {
            this.RI = ri;
        }

        internal RemotingCachedData(RuntimeType ri)
        {
            this.RI = ri;
        }

        internal SoapAttribute GetSoapAttribute()
        {
            if (this._soapAttr == null)
            {
                lock (this)
                {
                    if (this._soapAttr == null)
                    {
                        SoapAttribute attribute = null;
                        ICustomAttributeProvider rI = (ICustomAttributeProvider) this.RI;
                        if (this.RI is Type)
                        {
                            object[] customAttributes = rI.GetCustomAttributes(typeof(SoapTypeAttribute), true);
                            if ((customAttributes != null) && (customAttributes.Length != 0))
                            {
                                attribute = (SoapAttribute) customAttributes[0];
                            }
                            else
                            {
                                attribute = new SoapTypeAttribute();
                            }
                        }
                        else if (this.RI is MethodBase)
                        {
                            object[] objArray2 = rI.GetCustomAttributes(typeof(SoapMethodAttribute), true);
                            if ((objArray2 != null) && (objArray2.Length != 0))
                            {
                                attribute = (SoapAttribute) objArray2[0];
                            }
                            else
                            {
                                attribute = new SoapMethodAttribute();
                            }
                        }
                        else if (this.RI is FieldInfo)
                        {
                            object[] objArray3 = rI.GetCustomAttributes(typeof(SoapFieldAttribute), false);
                            if ((objArray3 != null) && (objArray3.Length != 0))
                            {
                                attribute = (SoapAttribute) objArray3[0];
                            }
                            else
                            {
                                attribute = new SoapFieldAttribute();
                            }
                        }
                        else if (this.RI is ParameterInfo)
                        {
                            object[] objArray4 = rI.GetCustomAttributes(typeof(SoapParameterAttribute), true);
                            if ((objArray4 != null) && (objArray4.Length != 0))
                            {
                                attribute = (SoapParameterAttribute) objArray4[0];
                            }
                            else
                            {
                                attribute = new SoapParameterAttribute();
                            }
                        }
                        attribute.SetReflectInfo(this.RI);
                        this._soapAttr = attribute;
                    }
                }
            }
            return this._soapAttr;
        }
    }
}

