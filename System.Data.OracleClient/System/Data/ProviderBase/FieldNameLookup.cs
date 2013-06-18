namespace System.Data.ProviderBase
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    internal sealed class FieldNameLookup
    {
        private CompareInfo _compareInfo;
        private int _defaultLocaleID;
        private Hashtable _fieldNameLookup;
        private string[] _fieldNames;

        public FieldNameLookup(IDataRecord reader, int defaultLocaleID)
        {
            int fieldCount = reader.FieldCount;
            string[] strArray = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                strArray[i] = reader.GetName(i);
            }
            this._fieldNames = strArray;
            this._defaultLocaleID = defaultLocaleID;
        }

        private void GenerateLookup()
        {
            int length = this._fieldNames.Length;
            Hashtable hashtable = new Hashtable(length);
            for (int i = length - 1; 0 <= i; i--)
            {
                string str = this._fieldNames[i];
                hashtable[str] = i;
            }
            this._fieldNameLookup = hashtable;
        }

        public int GetOrdinal(string fieldName)
        {
            if (fieldName == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("fieldName");
            }
            int index = this.IndexOf(fieldName);
            if (-1 == index)
            {
                throw System.Data.Common.ADP.IndexOutOfRange(fieldName);
            }
            return index;
        }

        public int IndexOf(string fieldName)
        {
            if (this._fieldNameLookup == null)
            {
                this.GenerateLookup();
            }
            object obj2 = this._fieldNameLookup[fieldName];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int num = this.LinearIndexOf(fieldName, CompareOptions.IgnoreCase);
            if (-1 == num)
            {
                num = this.LinearIndexOf(fieldName, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            return num;
        }

        private int LinearIndexOf(string fieldName, CompareOptions compareOptions)
        {
            CompareInfo compareInfo = this._compareInfo;
            if (compareInfo == null)
            {
                if (-1 != this._defaultLocaleID)
                {
                    compareInfo = CompareInfo.GetCompareInfo(this._defaultLocaleID);
                }
                if (compareInfo == null)
                {
                    compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                }
                this._compareInfo = compareInfo;
            }
            int length = this._fieldNames.Length;
            for (int i = 0; i < length; i++)
            {
                if (compareInfo.Compare(fieldName, this._fieldNames[i], compareOptions) == 0)
                {
                    this._fieldNameLookup[fieldName] = i;
                    return i;
                }
            }
            return -1;
        }
    }
}

