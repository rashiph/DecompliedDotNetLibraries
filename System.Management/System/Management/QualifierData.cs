namespace System.Management
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public class QualifierData
    {
        private ManagementBaseObject parent;
        private string propertyOrMethodName;
        private int qualifierFlavor;
        private string qualifierName;
        private IWbemQualifierSetFreeThreaded qualifierSet;
        private QualifierType qualifierType;
        private object qualifierValue;

        internal QualifierData(ManagementBaseObject parent, string propName, string qualName, QualifierType type)
        {
            this.parent = parent;
            this.propertyOrMethodName = propName;
            this.qualifierName = qualName;
            this.qualifierType = type;
            this.RefreshQualifierInfo();
        }

        private static object MapQualValueToWmiValue(object qualVal)
        {
            object obj2 = DBNull.Value;
            if (qualVal != null)
            {
                if (!(qualVal is Array))
                {
                    return qualVal;
                }
                if (((qualVal is int[]) || (qualVal is double[])) || ((qualVal is string[]) || (qualVal is bool[])))
                {
                    return qualVal;
                }
                Array array = (Array) qualVal;
                int length = array.Length;
                Type type = (length > 0) ? array.GetValue(0).GetType() : null;
                if (type == typeof(int))
                {
                    obj2 = new int[length];
                    for (int j = 0; j < length; j++)
                    {
                        ((int[]) obj2)[j] = Convert.ToInt32(array.GetValue(j), (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)));
                    }
                    return obj2;
                }
                if (type == typeof(double))
                {
                    obj2 = new double[length];
                    for (int k = 0; k < length; k++)
                    {
                        ((double[]) obj2)[k] = Convert.ToDouble(array.GetValue(k), (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(double)));
                    }
                    return obj2;
                }
                if (type == typeof(string))
                {
                    obj2 = new string[length];
                    for (int m = 0; m < length; m++)
                    {
                        ((string[]) obj2)[m] = array.GetValue(m).ToString();
                    }
                    return obj2;
                }
                if (!(type == typeof(bool)))
                {
                    return array;
                }
                obj2 = new bool[length];
                for (int i = 0; i < length; i++)
                {
                    ((bool[]) obj2)[i] = Convert.ToBoolean(array.GetValue(i), (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(bool)));
                }
            }
            return obj2;
        }

        private void RefreshQualifierInfo()
        {
            int errorCode = -2147217407;
            this.qualifierSet = null;
            switch (this.qualifierType)
            {
                case QualifierType.ObjectQualifier:
                    errorCode = this.parent.wbemObject.GetQualifierSet_(out this.qualifierSet);
                    break;

                case QualifierType.PropertyQualifier:
                    errorCode = this.parent.wbemObject.GetPropertyQualifierSet_(this.propertyOrMethodName, out this.qualifierSet);
                    break;

                case QualifierType.MethodQualifier:
                    errorCode = this.parent.wbemObject.GetMethodQualifierSet_(this.propertyOrMethodName, out this.qualifierSet);
                    break;

                default:
                    throw new ManagementException(ManagementStatus.Unexpected, null, null);
            }
            if ((errorCode & 0x80000000L) == 0L)
            {
                this.qualifierValue = null;
                if (this.qualifierSet != null)
                {
                    errorCode = this.qualifierSet.Get_(this.qualifierName, 0, ref this.qualifierValue, ref this.qualifierFlavor);
                }
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

        public bool IsAmended
        {
            get
            {
                this.RefreshQualifierInfo();
                return (0x80 == (this.qualifierFlavor & 0x80));
            }
            set
            {
                int errorCode = 0;
                this.RefreshQualifierInfo();
                int lFlavor = this.qualifierFlavor & -97;
                if (value)
                {
                    lFlavor |= 0x80;
                }
                else
                {
                    lFlavor &= -129;
                }
                errorCode = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, lFlavor);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public bool IsLocal
        {
            get
            {
                this.RefreshQualifierInfo();
                return (0 == (this.qualifierFlavor & 0x60));
            }
        }

        public bool IsOverridable
        {
            get
            {
                this.RefreshQualifierInfo();
                return (0 == (this.qualifierFlavor & 0x10));
            }
            set
            {
                int errorCode = 0;
                this.RefreshQualifierInfo();
                int lFlavor = this.qualifierFlavor & -97;
                if (value)
                {
                    lFlavor &= -17;
                }
                else
                {
                    lFlavor |= 0x10;
                }
                errorCode = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, lFlavor);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public string Name
        {
            get
            {
                if (this.qualifierName == null)
                {
                    return "";
                }
                return this.qualifierName;
            }
        }

        public bool PropagatesToInstance
        {
            get
            {
                this.RefreshQualifierInfo();
                return (1 == (this.qualifierFlavor & 1));
            }
            set
            {
                int errorCode = 0;
                this.RefreshQualifierInfo();
                int lFlavor = this.qualifierFlavor & -97;
                if (value)
                {
                    lFlavor |= 1;
                }
                else
                {
                    lFlavor &= -2;
                }
                errorCode = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, lFlavor);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public bool PropagatesToSubclass
        {
            get
            {
                this.RefreshQualifierInfo();
                return (2 == (this.qualifierFlavor & 2));
            }
            set
            {
                int errorCode = 0;
                this.RefreshQualifierInfo();
                int lFlavor = this.qualifierFlavor & -97;
                if (value)
                {
                    lFlavor |= 2;
                }
                else
                {
                    lFlavor &= -3;
                }
                errorCode = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, lFlavor);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public object Value
        {
            get
            {
                this.RefreshQualifierInfo();
                return ValueTypeSafety.GetSafeObject(this.qualifierValue);
            }
            set
            {
                int errorCode = 0;
                this.RefreshQualifierInfo();
                object pVal = MapQualValueToWmiValue(value);
                errorCode = this.qualifierSet.Put_(this.qualifierName, ref pVal, this.qualifierFlavor & -97);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else if ((errorCode & 0x80000000L) != 0L)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }
    }
}

