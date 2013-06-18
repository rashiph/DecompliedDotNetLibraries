namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;

    public class KeysConverter : TypeConverter, IComparer
    {
        private List<string> displayOrder;
        private const Keys FirstAscii = Keys.A;
        private const Keys FirstDigit = Keys.D0;
        private const Keys FirstNumpadDigit = Keys.NumPad0;
        private IDictionary keyNames;
        private const Keys LastAscii = Keys.Z;
        private const Keys LastDigit = Keys.D9;
        private const Keys LastNumpadDigit = Keys.NumPad9;
        private TypeConverter.StandardValuesCollection values;

        private void AddKey(string key, Keys value)
        {
            this.keyNames[key] = value;
            this.displayOrder.Add(key);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (!(sourceType == typeof(string)) && !(sourceType == typeof(Enum[])))
            {
                return base.CanConvertFrom(context, sourceType);
            }
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(Enum[])) || base.CanConvertTo(context, destinationType));
        }

        public int Compare(object a, object b)
        {
            return string.Compare(base.ConvertToString(a), base.ConvertToString(b), false, CultureInfo.InvariantCulture);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string str = ((string) value).Trim();
                if (str.Length == 0)
                {
                    return null;
                }
                string[] strArray = str.Split(new char[] { '+' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    strArray[i] = strArray[i].Trim();
                }
                Keys none = Keys.None;
                bool flag = false;
                for (int j = 0; j < strArray.Length; j++)
                {
                    object obj2 = this.KeyNames[strArray[j]];
                    if (obj2 == null)
                    {
                        obj2 = Enum.Parse(typeof(Keys), strArray[j]);
                    }
                    if (obj2 == null)
                    {
                        throw new FormatException(System.Windows.Forms.SR.GetString("KeysConverterInvalidKeyName", new object[] { strArray[j] }));
                    }
                    Keys keys2 = (Keys) obj2;
                    if ((keys2 & Keys.KeyCode) != Keys.None)
                    {
                        if (flag)
                        {
                            throw new FormatException(System.Windows.Forms.SR.GetString("KeysConverterInvalidKeyCombination"));
                        }
                        flag = true;
                    }
                    none |= keys2;
                }
                return none;
            }
            if (!(value is Enum[]))
            {
                return base.ConvertFrom(context, culture, value);
            }
            long num3 = 0L;
            foreach (Enum enum2 in (Enum[]) value)
            {
                num3 |= Convert.ToInt64(enum2, CultureInfo.InvariantCulture);
            }
            return Enum.ToObject(typeof(Keys), num3);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((value is Keys) || (value is int))
            {
                bool flag = destinationType == typeof(string);
                bool flag2 = false;
                if (!flag)
                {
                    flag2 = destinationType == typeof(Enum[]);
                }
                if (flag || flag2)
                {
                    Keys keys = (Keys) value;
                    bool flag3 = false;
                    ArrayList list = new ArrayList();
                    Keys keys2 = keys & ~Keys.KeyCode;
                    for (int i = 0; i < this.DisplayOrder.Count; i++)
                    {
                        string str = this.DisplayOrder[i];
                        Keys keys3 = (Keys) this.keyNames[str];
                        if ((keys3 & keys2) != Keys.None)
                        {
                            if (flag)
                            {
                                if (flag3)
                                {
                                    list.Add("+");
                                }
                                list.Add(str);
                            }
                            else
                            {
                                list.Add(keys3);
                            }
                            flag3 = true;
                        }
                    }
                    Keys keys4 = keys & Keys.KeyCode;
                    bool flag4 = false;
                    if (flag3 && flag)
                    {
                        list.Add("+");
                    }
                    for (int j = 0; j < this.DisplayOrder.Count; j++)
                    {
                        string str2 = this.DisplayOrder[j];
                        Keys keys5 = (Keys) this.keyNames[str2];
                        if (keys5.Equals(keys4))
                        {
                            if (flag)
                            {
                                list.Add(str2);
                            }
                            else
                            {
                                list.Add(keys5);
                            }
                            flag3 = true;
                            flag4 = true;
                            break;
                        }
                    }
                    if (!flag4 && Enum.IsDefined(typeof(Keys), (int) keys4))
                    {
                        if (flag)
                        {
                            list.Add(keys4.ToString());
                        }
                        else
                        {
                            list.Add(keys4);
                        }
                    }
                    if (!flag)
                    {
                        return (Enum[]) list.ToArray(typeof(Enum));
                    }
                    StringBuilder builder = new StringBuilder(0x20);
                    foreach (string str3 in list)
                    {
                        builder.Append(str3);
                    }
                    return builder.ToString();
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                ArrayList list = new ArrayList();
                foreach (object obj2 in this.KeyNames.Values)
                {
                    list.Add(obj2);
                }
                list.Sort(this);
                this.values = new TypeConverter.StandardValuesCollection(list.ToArray());
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private void Initialize()
        {
            this.keyNames = new Hashtable(0x22);
            this.displayOrder = new List<string>(0x22);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringEnter"), Keys.Enter);
            this.AddKey("F12", Keys.F12);
            this.AddKey("F11", Keys.F11);
            this.AddKey("F10", Keys.F10);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringEnd"), Keys.End);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringControl"), Keys.Control);
            this.AddKey("F8", Keys.F8);
            this.AddKey("F9", Keys.F9);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringAlt"), Keys.Alt);
            this.AddKey("F4", Keys.F4);
            this.AddKey("F5", Keys.F5);
            this.AddKey("F6", Keys.F6);
            this.AddKey("F7", Keys.F7);
            this.AddKey("F1", Keys.F1);
            this.AddKey("F2", Keys.F2);
            this.AddKey("F3", Keys.F3);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringPageDown"), Keys.Next);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringInsert"), Keys.Insert);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringHome"), Keys.Home);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringDelete"), Keys.Delete);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringShift"), Keys.Shift);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringPageUp"), Keys.PageUp);
            this.AddKey(System.Windows.Forms.SR.GetString("toStringBack"), Keys.Back);
            this.AddKey("0", Keys.D0);
            this.AddKey("1", Keys.D1);
            this.AddKey("2", Keys.D2);
            this.AddKey("3", Keys.D3);
            this.AddKey("4", Keys.D4);
            this.AddKey("5", Keys.D5);
            this.AddKey("6", Keys.D6);
            this.AddKey("7", Keys.D7);
            this.AddKey("8", Keys.D8);
            this.AddKey("9", Keys.D9);
        }

        private List<string> DisplayOrder
        {
            get
            {
                if (this.displayOrder == null)
                {
                    this.Initialize();
                }
                return this.displayOrder;
            }
        }

        private IDictionary KeyNames
        {
            get
            {
                if (this.keyNames == null)
                {
                    this.Initialize();
                }
                return this.keyNames;
            }
        }
    }
}

