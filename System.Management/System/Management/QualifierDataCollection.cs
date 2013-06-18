namespace System.Management
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class QualifierDataCollection : ICollection, IEnumerable
    {
        private ManagementBaseObject parent;
        private string propertyOrMethodName;
        private QualifierType qualifierSetType;

        internal QualifierDataCollection(ManagementBaseObject parent)
        {
            this.parent = parent;
            this.qualifierSetType = QualifierType.ObjectQualifier;
            this.propertyOrMethodName = null;
        }

        internal QualifierDataCollection(ManagementBaseObject parent, string propertyOrMethodName, QualifierType type)
        {
            this.parent = parent;
            this.propertyOrMethodName = propertyOrMethodName;
            this.qualifierSetType = type;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void Add(string qualifierName, object qualifierValue)
        {
            this.Add(qualifierName, qualifierValue, false, false, false, true);
        }

        public virtual void Add(string qualifierName, object qualifierValue, bool isAmended, bool propagatesToInstance, bool propagatesToSubclass, bool isOverridable)
        {
            int lFlavor = 0;
            if (isAmended)
            {
                lFlavor |= 0x80;
            }
            if (propagatesToInstance)
            {
                lFlavor |= 1;
            }
            if (propagatesToSubclass)
            {
                lFlavor |= 2;
            }
            if (!isOverridable)
            {
                lFlavor |= 0x10;
            }
            int errorCode = this.GetTypeQualifierSet().Put_(qualifierName, ref qualifierValue, lFlavor);
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
            IWbemQualifierSetFreeThreaded typeQualifierSet;
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < array.GetLowerBound(0)) || (index > array.GetUpperBound(0)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            string[] pNames = null;
            try
            {
                typeQualifierSet = this.GetTypeQualifierSet();
            }
            catch (ManagementException exception)
            {
                if ((this.qualifierSetType != QualifierType.PropertyQualifier) || (exception.ErrorCode != ManagementStatus.SystemProperty))
                {
                    throw;
                }
                return;
            }
            int errorCode = typeQualifierSet.GetNames_(0, out pNames);
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
            if ((index + pNames.Length) > array.Length)
            {
                throw new ArgumentException(null, "index");
            }
            foreach (string str in pNames)
            {
                array.SetValue(new QualifierData(this.parent, this.propertyOrMethodName, str, this.qualifierSetType), index++);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void CopyTo(QualifierData[] qualifierArray, int index)
        {
            this.CopyTo((Array) qualifierArray, index);
        }

        public QualifierDataEnumerator GetEnumerator()
        {
            return new QualifierDataEnumerator(this.parent, this.propertyOrMethodName, this.qualifierSetType);
        }

        private IWbemQualifierSetFreeThreaded GetTypeQualifierSet()
        {
            return this.GetTypeQualifierSet(this.qualifierSetType);
        }

        private IWbemQualifierSetFreeThreaded GetTypeQualifierSet(QualifierType qualifierSetType)
        {
            IWbemQualifierSetFreeThreaded ppQualSet = null;
            int errorCode = 0;
            switch (qualifierSetType)
            {
                case QualifierType.ObjectQualifier:
                    errorCode = this.parent.wbemObject.GetQualifierSet_(out ppQualSet);
                    break;

                case QualifierType.PropertyQualifier:
                    errorCode = this.parent.wbemObject.GetPropertyQualifierSet_(this.propertyOrMethodName, out ppQualSet);
                    break;

                case QualifierType.MethodQualifier:
                    errorCode = this.parent.wbemObject.GetMethodQualifierSet_(this.propertyOrMethodName, out ppQualSet);
                    break;

                default:
                    throw new ManagementException(ManagementStatus.Unexpected, null, null);
            }
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return ppQualSet;
                }
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return ppQualSet;
        }

        public virtual void Remove(string qualifierName)
        {
            int errorCode = this.GetTypeQualifierSet().Delete_(qualifierName);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new QualifierDataEnumerator(this.parent, this.propertyOrMethodName, this.qualifierSetType);
        }

        public int Count
        {
            get
            {
                string[] pNames = null;
                IWbemQualifierSetFreeThreaded typeQualifierSet;
                try
                {
                    typeQualifierSet = this.GetTypeQualifierSet();
                }
                catch (ManagementException exception)
                {
                    if ((this.qualifierSetType != QualifierType.PropertyQualifier) || (exception.ErrorCode != ManagementStatus.SystemProperty))
                    {
                        throw;
                    }
                    return 0;
                }
                int errorCode = typeQualifierSet.GetNames_(0, out pNames);
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

        public virtual QualifierData this[string qualifierName]
        {
            get
            {
                if (qualifierName == null)
                {
                    throw new ArgumentNullException("qualifierName");
                }
                return new QualifierData(this.parent, this.propertyOrMethodName, qualifierName, this.qualifierSetType);
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public class QualifierDataEnumerator : IEnumerator
        {
            private int index = -1;
            private ManagementBaseObject parent;
            private string propertyOrMethodName;
            private string[] qualifierNames;
            private QualifierType qualifierType;

            internal QualifierDataEnumerator(ManagementBaseObject parent, string propertyOrMethodName, QualifierType qualifierType)
            {
                this.parent = parent;
                this.propertyOrMethodName = propertyOrMethodName;
                this.qualifierType = qualifierType;
                this.qualifierNames = null;
                IWbemQualifierSetFreeThreaded ppQualSet = null;
                int errorCode = 0;
                switch (qualifierType)
                {
                    case QualifierType.ObjectQualifier:
                        errorCode = parent.wbemObject.GetQualifierSet_(out ppQualSet);
                        break;

                    case QualifierType.PropertyQualifier:
                        errorCode = parent.wbemObject.GetPropertyQualifierSet_(propertyOrMethodName, out ppQualSet);
                        break;

                    case QualifierType.MethodQualifier:
                        errorCode = parent.wbemObject.GetMethodQualifierSet_(propertyOrMethodName, out ppQualSet);
                        break;

                    default:
                        throw new ManagementException(ManagementStatus.Unexpected, null, null);
                }
                if (errorCode < 0)
                {
                    this.qualifierNames = new string[0];
                }
                else
                {
                    errorCode = ppQualSet.GetNames_(0, out this.qualifierNames);
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

            public bool MoveNext()
            {
                if (this.index == this.qualifierNames.Length)
                {
                    return false;
                }
                this.index++;
                return (this.index != this.qualifierNames.Length);
            }

            public void Reset()
            {
                this.index = -1;
            }

            public QualifierData Current
            {
                get
                {
                    if ((this.index == -1) || (this.index == this.qualifierNames.Length))
                    {
                        throw new InvalidOperationException();
                    }
                    return new QualifierData(this.parent, this.propertyOrMethodName, this.qualifierNames[this.index], this.qualifierType);
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

