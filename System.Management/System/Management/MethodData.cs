namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    public class MethodData
    {
        private string methodName;
        private ManagementObject parent;
        private QualifierDataCollection qualifiers;
        private IWbemClassObjectFreeThreaded wmiInParams;
        private IWbemClassObjectFreeThreaded wmiOutParams;

        internal MethodData(ManagementObject parent, string methodName)
        {
            this.parent = parent;
            this.methodName = methodName;
            this.RefreshMethodInfo();
            this.qualifiers = null;
        }

        private void RefreshMethodInfo()
        {
            int errorCode = -2147217407;
            try
            {
                errorCode = this.parent.wbemObject.GetMethod_(this.methodName, 0, out this.wmiInParams, out this.wmiOutParams);
            }
            catch (COMException exception)
            {
                ManagementException.ThrowWithExtendedInfo(exception);
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
            }
            else if ((errorCode & 0x80000000L) != 0L)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public ManagementBaseObject InParameters
        {
            get
            {
                this.RefreshMethodInfo();
                if (this.wmiInParams != null)
                {
                    return new ManagementBaseObject(this.wmiInParams);
                }
                return null;
            }
        }

        public string Name
        {
            get
            {
                if (this.methodName == null)
                {
                    return "";
                }
                return this.methodName;
            }
        }

        public string Origin
        {
            get
            {
                string pstrClassName = null;
                int errorCode = this.parent.wbemObject.GetMethodOrigin_(this.methodName, out pstrClassName);
                if (errorCode < 0)
                {
                    if (errorCode == -2147217393)
                    {
                        return string.Empty;
                    }
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        return pstrClassName;
                    }
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return pstrClassName;
            }
        }

        public ManagementBaseObject OutParameters
        {
            get
            {
                this.RefreshMethodInfo();
                if (this.wmiOutParams != null)
                {
                    return new ManagementBaseObject(this.wmiOutParams);
                }
                return null;
            }
        }

        public QualifierDataCollection Qualifiers
        {
            get
            {
                if (this.qualifiers == null)
                {
                    this.qualifiers = new QualifierDataCollection(this.parent, this.methodName, QualifierType.MethodQualifier);
                }
                return this.qualifiers;
            }
        }
    }
}

