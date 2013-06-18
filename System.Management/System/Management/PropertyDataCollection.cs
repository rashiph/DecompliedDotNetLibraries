namespace System.Management
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class PropertyDataCollection : ICollection, IEnumerable
    {
        private bool isSystem;
        private ManagementBaseObject parent;

        internal PropertyDataCollection(ManagementBaseObject parent, bool isSystem)
        {
            this.parent = parent;
            this.isSystem = isSystem;
        }

        public virtual void Add(string propertyName, object propertyValue)
        {
            if (propertyValue == null)
            {
                throw new ArgumentNullException("propertyValue");
            }
            if (this.parent.GetType() == typeof(ManagementObject))
            {
                throw new InvalidOperationException();
            }
            CimType none = CimType.None;
            bool isArray = false;
            object pVal = PropertyData.MapValueToWmiValue(propertyValue, out isArray, out none);
            int type = (int) none;
            if (isArray)
            {
                type |= 0x2000;
            }
            int errorCode = this.parent.wbemObject.Put_(propertyName, 0, ref pVal, type);
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
        }

        public void Add(string propertyName, CimType propertyType, bool isArray)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(propertyName);
            }
            if (this.parent.GetType() == typeof(ManagementObject))
            {
                throw new InvalidOperationException();
            }
            int type = (int) propertyType;
            if (isArray)
            {
                type |= 0x2000;
            }
            object pVal = DBNull.Value;
            int errorCode = this.parent.wbemObject.Put_(propertyName, 0, ref pVal, type);
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
        }

        public void Add(string propertyName, object propertyValue, CimType propertyType)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (this.parent.GetType() == typeof(ManagementObject))
            {
                throw new InvalidOperationException();
            }
            int type = (int) propertyType;
            bool isArray = false;
            if ((propertyValue != null) && propertyValue.GetType().IsArray)
            {
                isArray = true;
                type |= 0x2000;
            }
            object pVal = PropertyData.MapValueToWmiValue(propertyValue, propertyType, isArray);
            int errorCode = this.parent.wbemObject.Put_(propertyName, 0, ref pVal, type);
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
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < array.GetLowerBound(0)) || (index > array.GetUpperBound(0)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            string[] pNames = null;
            object pQualifierVal = null;
            int lFlags = 0;
            if (this.isSystem)
            {
                lFlags |= 0x30;
            }
            else
            {
                lFlags |= 0x40;
            }
            int errorCode = this.parent.wbemObject.GetNames_(null, lFlags, ref pQualifierVal, out pNames);
            if (errorCode >= 0)
            {
                if ((index + pNames.Length) > array.Length)
                {
                    throw new ArgumentException(null, "index");
                }
                foreach (string str in pNames)
                {
                    array.SetValue(new PropertyData(this.parent, str), index++);
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
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void CopyTo(PropertyData[] propertyArray, int index)
        {
            this.CopyTo((Array) propertyArray, index);
        }

        public PropertyDataEnumerator GetEnumerator()
        {
            return new PropertyDataEnumerator(this.parent, this.isSystem);
        }

        public virtual void Remove(string propertyName)
        {
            if (this.parent.GetType() == typeof(ManagementObject))
            {
                ManagementClass class2 = new ManagementClass(this.parent.ClassPath);
                this.parent.SetPropertyValue(propertyName, class2.GetPropertyValue(propertyName));
            }
            else
            {
                int errorCode = this.parent.wbemObject.Delete_(propertyName);
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
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PropertyDataEnumerator(this.parent, this.isSystem);
        }

        public int Count
        {
            get
            {
                string[] pNames = null;
                object pQualifierVal = null;
                int num;
                if (this.isSystem)
                {
                    num = 0x30;
                }
                else
                {
                    num = 0x40;
                }
                int errorCode = this.parent.wbemObject.GetNames_(null, num, ref pQualifierVal, out pNames);
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
                return pNames.Length;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual PropertyData this[string propertyName]
        {
            get
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException("propertyName");
                }
                return new PropertyData(this.parent, propertyName);
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public class PropertyDataEnumerator : IEnumerator
        {
            private int index;
            private ManagementBaseObject parent;
            private string[] propertyNames;

            internal PropertyDataEnumerator(ManagementBaseObject parent, bool isSystem)
            {
                int num;
                this.parent = parent;
                this.propertyNames = null;
                this.index = -1;
                object pQualifierVal = null;
                if (isSystem)
                {
                    num = 0x30;
                }
                else
                {
                    num = 0x40;
                }
                int errorCode = parent.wbemObject.GetNames_(null, num, ref pQualifierVal, out this.propertyNames);
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
            }

            public bool MoveNext()
            {
                if (this.index == this.propertyNames.Length)
                {
                    return false;
                }
                this.index++;
                return (this.index != this.propertyNames.Length);
            }

            public void Reset()
            {
                this.index = -1;
            }

            public PropertyData Current
            {
                get
                {
                    if ((this.index == -1) || (this.index == this.propertyNames.Length))
                    {
                        throw new InvalidOperationException();
                    }
                    return new PropertyData(this.parent, this.propertyNames[this.index]);
                }
            }

            object IEnumerator.Current
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

