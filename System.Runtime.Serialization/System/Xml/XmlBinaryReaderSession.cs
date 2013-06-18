namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    public class XmlBinaryReaderSession : IXmlDictionary
    {
        private const int MaxArrayEntries = 0x800;
        private Dictionary<int, XmlDictionaryString> stringDict;
        private XmlDictionaryString[] strings;

        public XmlDictionaryString Add(int id, string value)
        {
            XmlDictionaryString str;
            if (id < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(System.Runtime.Serialization.SR.GetString("XmlInvalidID")));
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (this.TryLookup(id, out str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlIDDefined")));
            }
            str = new XmlDictionaryString(this, value, id);
            if (id >= 0x800)
            {
                if (this.stringDict == null)
                {
                    this.stringDict = new Dictionary<int, XmlDictionaryString>();
                }
                this.stringDict.Add(id, str);
                return str;
            }
            if (this.strings == null)
            {
                this.strings = new XmlDictionaryString[Math.Max(id + 1, 0x10)];
            }
            else if (id >= this.strings.Length)
            {
                XmlDictionaryString[] destinationArray = new XmlDictionaryString[Math.Min(Math.Max((int) (id + 1), (int) (this.strings.Length * 2)), 0x800)];
                Array.Copy(this.strings, destinationArray, this.strings.Length);
                this.strings = destinationArray;
            }
            this.strings[id] = str;
            return str;
        }

        public void Clear()
        {
            if (this.strings != null)
            {
                Array.Clear(this.strings, 0, this.strings.Length);
            }
            if (this.stringDict != null)
            {
                this.stringDict.Clear();
            }
        }

        public bool TryLookup(int key, out XmlDictionaryString result)
        {
            if (((this.strings != null) && (key >= 0)) && (key < this.strings.Length))
            {
                result = this.strings[key];
                return (result != null);
            }
            if ((key >= 0x800) && (this.stringDict != null))
            {
                return this.stringDict.TryGetValue(key, out result);
            }
            result = null;
            return false;
        }

        public bool TryLookup(string value, out XmlDictionaryString result)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (this.strings != null)
            {
                for (int i = 0; i < this.strings.Length; i++)
                {
                    XmlDictionaryString str = this.strings[i];
                    if ((str != null) && (str.Value == value))
                    {
                        result = str;
                        return true;
                    }
                }
            }
            if (this.stringDict != null)
            {
                foreach (XmlDictionaryString str2 in this.stringDict.Values)
                {
                    if (str2.Value == value)
                    {
                        result = str2;
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            if (value.Dictionary != this)
            {
                result = null;
                return false;
            }
            result = value;
            return true;
        }
    }
}

