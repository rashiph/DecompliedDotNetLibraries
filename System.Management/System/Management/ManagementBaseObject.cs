namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, ToolboxItem(false)]
    public class ManagementBaseObject : Component, ICloneable, ISerializable
    {
        internal IWbemClassObjectFreeThreaded _wbemObject;
        private static WbemContext lockOnFastProx = (WMICapabilities.IsWindowsXPOrHigher() ? null : new WbemContext());
        private PropertyDataCollection properties;
        private QualifierDataCollection qualifiers;
        private PropertyDataCollection systemProperties;

        internal ManagementBaseObject(IWbemClassObjectFreeThreaded wbemObject)
        {
            this.wbemObject = wbemObject;
            this.properties = null;
            this.systemProperties = null;
            this.qualifiers = null;
        }

        protected ManagementBaseObject(SerializationInfo info, StreamingContext context)
        {
            this._wbemObject = info.GetValue("wbemObject", typeof(IWbemClassObjectFreeThreaded)) as IWbemClassObjectFreeThreaded;
            if (this._wbemObject == null)
            {
                throw new SerializationException();
            }
            this.properties = null;
            this.systemProperties = null;
            this.qualifiers = null;
        }

        private static bool _IsClass(IWbemClassObjectFreeThreaded wbemObject)
        {
            object pVal = null;
            int pType = 0;
            int plFlavor = 0;
            int errorCode = wbemObject.Get_("__GENUS", 0, ref pVal, ref pType, ref plFlavor);
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            return (((int) pVal) == 1);
        }

        public virtual object Clone()
        {
            IWbemClassObjectFreeThreaded ppCopy = null;
            int errorCode = this.wbemObject.Clone_(out ppCopy);
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            return new ManagementBaseObject(ppCopy);
        }

        public bool CompareTo(ManagementBaseObject otherObject, ComparisonSettings settings)
        {
            if (otherObject == null)
            {
                throw new ArgumentNullException("otherObject");
            }
            bool flag = false;
            if (this.wbemObject != null)
            {
                int errorCode = 0;
                errorCode = this.wbemObject.CompareTo_((int) settings, otherObject.wbemObject);
                if (0x40003 == errorCode)
                {
                    return false;
                }
                if (errorCode == 0)
                {
                    return true;
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return flag;
                }
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            return flag;
        }

        public void Dispose()
        {
            if (this._wbemObject != null)
            {
                this._wbemObject.Dispose();
                this._wbemObject = null;
            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((obj is ManagementBaseObject) && this.CompareTo((ManagementBaseObject) obj, ComparisonSettings.IncludeAll));
            }
            catch (ManagementException exception)
            {
                return ((((exception.ErrorCode == ManagementStatus.NotFound) && (this is ManagementObject)) && (obj is ManagementObject)) && (string.Compare(((ManagementObject) this).Path.Path, ((ManagementObject) obj).Path.Path, StringComparison.OrdinalIgnoreCase) == 0));
            }
            catch
            {
                return false;
            }
            return false;
        }

        internal static ManagementBaseObject GetBaseObject(IWbemClassObjectFreeThreaded wbemObject, ManagementScope scope)
        {
            if (_IsClass(wbemObject))
            {
                return ManagementClass.GetManagementClass(wbemObject, scope);
            }
            return ManagementObject.GetManagementObject(wbemObject, scope);
        }

        public override int GetHashCode()
        {
            try
            {
                return this.GetText(TextFormat.Mof).GetHashCode();
            }
            catch (ManagementException)
            {
                return string.Empty.GetHashCode();
            }
            catch (COMException)
            {
                return string.Empty.GetHashCode();
            }
        }

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable) this).GetObjectData(info, context);
        }

        public object GetPropertyQualifierValue(string propertyName, string qualifierName)
        {
            return this.Properties[propertyName].Qualifiers[qualifierName].Value;
        }

        public object GetPropertyValue(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (propertyName.StartsWith("__", StringComparison.Ordinal))
            {
                return this.SystemProperties[propertyName].Value;
            }
            return this.Properties[propertyName].Value;
        }

        public object GetQualifierValue(string qualifierName)
        {
            return this.Qualifiers[qualifierName].Value;
        }

        public string GetText(TextFormat format)
        {
            string pstrObjectText = null;
            int errorCode = 0;
            switch (format)
            {
                case TextFormat.Mof:
                    errorCode = this.wbemObject.GetObjectText_(0, out pstrObjectText);
                    if (errorCode < 0)
                    {
                        if ((errorCode & 0xfffff000L) != 0x80041000L)
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                            return pstrObjectText;
                        }
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    return pstrObjectText;

                case TextFormat.CimDtd20:
                case TextFormat.WmiDtd20:
                {
                    IWbemObjectTextSrc src = (IWbemObjectTextSrc) new WbemObjectTextSrc();
                    IWbemContext pCtx = (IWbemContext) new WbemContext();
                    pCtx.SetValue_("IncludeQualifiers", 0, true);
                    pCtx.SetValue_("IncludeClassOrigin", 0, ref pValue);
                    if (src != null)
                    {
                        errorCode = src.GetText_(0, (IWbemClassObject_DoNotMarshal) Marshal.GetObjectForIUnknown((IntPtr) this.wbemObject), (uint) format, pCtx, out pstrObjectText);
                        if (errorCode < 0)
                        {
                            if ((errorCode & 0xfffff000L) != 0x80041000L)
                            {
                                Marshal.ThrowExceptionForHR(errorCode);
                                return pstrObjectText;
                            }
                            ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        }
                    }
                    return pstrObjectText;
                }
            }
            return null;
        }

        internal virtual void Initialize(bool getObject)
        {
        }

        public static explicit operator IntPtr(ManagementBaseObject managementObject)
        {
            if (managementObject == null)
            {
                return IntPtr.Zero;
            }
            return (IntPtr) managementObject.wbemObject;
        }

        public void SetPropertyQualifierValue(string propertyName, string qualifierName, object qualifierValue)
        {
            this.Properties[propertyName].Qualifiers[qualifierName].Value = qualifierValue;
        }

        public void SetPropertyValue(string propertyName, object propertyValue)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (propertyName.StartsWith("__", StringComparison.Ordinal))
            {
                this.SystemProperties[propertyName].Value = propertyValue;
            }
            else
            {
                this.Properties[propertyName].Value = propertyValue;
            }
        }

        public void SetQualifierValue(string qualifierName, object qualifierValue)
        {
            this.Qualifiers[qualifierName].Value = qualifierValue;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("wbemObject", this.wbemObject, typeof(IWbemClassObjectFreeThreaded));
            info.AssemblyName = typeof(ManagementBaseObject).Assembly.FullName;
            info.FullTypeName = typeof(ManagementBaseObject).ToString();
        }

        internal string ClassName
        {
            get
            {
                object pVal = null;
                int pType = 0;
                int plFlavor = 0;
                int errorCode = 0;
                errorCode = this.wbemObject.Get_("__CLASS", 0, ref pVal, ref pType, ref plFlavor);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                if (pVal is DBNull)
                {
                    return string.Empty;
                }
                return (string) pVal;
            }
        }

        public virtual ManagementPath ClassPath
        {
            get
            {
                object pVal = null;
                object obj3 = null;
                object obj4 = null;
                int pType = 0;
                int plFlavor = 0;
                int errorCode = 0;
                errorCode = this.wbemObject.Get_("__SERVER", 0, ref pVal, ref pType, ref plFlavor);
                if (errorCode == 0)
                {
                    errorCode = this.wbemObject.Get_("__NAMESPACE", 0, ref obj3, ref pType, ref plFlavor);
                    if (errorCode == 0)
                    {
                        errorCode = this.wbemObject.Get_("__CLASS", 0, ref obj4, ref pType, ref plFlavor);
                    }
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                ManagementPath path = new ManagementPath {
                    Server = string.Empty,
                    NamespacePath = string.Empty,
                    ClassName = string.Empty
                };
                try
                {
                    path.Server = (pVal is DBNull) ? "" : ((string) pVal);
                    path.NamespacePath = (obj3 is DBNull) ? "" : ((string) obj3);
                    path.ClassName = (obj4 is DBNull) ? "" : ((string) obj4);
                }
                catch
                {
                }
                return path;
            }
        }

        internal bool IsClass
        {
            get
            {
                return _IsClass(this.wbemObject);
            }
        }

        public object this[string propertyName]
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetPropertyValue(propertyName);
            }
            set
            {
                this.Initialize(true);
                try
                {
                    this.SetPropertyValue(propertyName, value);
                }
                catch (COMException exception)
                {
                    ManagementException.ThrowWithExtendedInfo(exception);
                }
            }
        }

        public virtual PropertyDataCollection Properties
        {
            get
            {
                this.Initialize(true);
                if (this.properties == null)
                {
                    this.properties = new PropertyDataCollection(this, false);
                }
                return this.properties;
            }
        }

        public virtual QualifierDataCollection Qualifiers
        {
            get
            {
                this.Initialize(true);
                if (this.qualifiers == null)
                {
                    this.qualifiers = new QualifierDataCollection(this);
                }
                return this.qualifiers;
            }
        }

        public virtual PropertyDataCollection SystemProperties
        {
            get
            {
                this.Initialize(false);
                if (this.systemProperties == null)
                {
                    this.systemProperties = new PropertyDataCollection(this, true);
                }
                return this.systemProperties;
            }
        }

        internal IWbemClassObjectFreeThreaded wbemObject
        {
            get
            {
                if (this._wbemObject == null)
                {
                    this.Initialize(true);
                }
                return this._wbemObject;
            }
            set
            {
                this._wbemObject = value;
            }
        }
    }
}

