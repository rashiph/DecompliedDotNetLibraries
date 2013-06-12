namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    internal class Com2Enum
    {
        private bool allowUnknownValues;
        private string[] names;
        private string[] stringValues;
        private object[] values;

        public Com2Enum(string[] names, object[] values, bool allowUnknownValues)
        {
            this.allowUnknownValues = allowUnknownValues;
            if (((names == null) || (values == null)) || (names.Length != values.Length))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("COM2NamesAndValuesNotEqual"));
            }
            this.PopulateArrays(names, values);
        }

        public virtual object FromString(string s)
        {
            int index = -1;
            for (int i = 0; i < this.stringValues.Length; i++)
            {
                if ((string.Compare(this.names[i], s, true, CultureInfo.InvariantCulture) == 0) || (string.Compare(this.stringValues[i], s, true, CultureInfo.InvariantCulture) == 0))
                {
                    return this.values[i];
                }
                if ((index == -1) && (string.Compare(this.names[i], s, true, CultureInfo.InvariantCulture) == 0))
                {
                    index = i;
                }
            }
            if (index != -1)
            {
                return this.values[index];
            }
            if (!this.allowUnknownValues)
            {
                return null;
            }
            return s;
        }

        protected virtual void PopulateArrays(string[] names, object[] values)
        {
            this.names = new string[names.Length];
            this.stringValues = new string[names.Length];
            this.values = new object[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                this.names[i] = names[i];
                this.values[i] = values[i];
                if (values[i] != null)
                {
                    this.stringValues[i] = values[i].ToString();
                }
            }
        }

        public virtual string ToString(object v)
        {
            if (v != null)
            {
                if ((this.values.Length > 0) && (v.GetType() != this.values[0].GetType()))
                {
                    try
                    {
                        v = Convert.ChangeType(v, this.values[0].GetType(), CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                    }
                }
                string strB = v.ToString();
                for (int i = 0; i < this.values.Length; i++)
                {
                    if (string.Compare(this.stringValues[i], strB, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return this.names[i];
                    }
                }
                if (this.allowUnknownValues)
                {
                    return strB;
                }
            }
            return "";
        }

        public bool IsStrictEnum
        {
            get
            {
                return !this.allowUnknownValues;
            }
        }

        public virtual string[] Names
        {
            get
            {
                return (string[]) this.names.Clone();
            }
        }

        public virtual object[] Values
        {
            get
            {
                return (object[]) this.values.Clone();
            }
        }
    }
}

