namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class ServiceModelDictionary : IXmlDictionary
    {
        private int count;
        private Dictionary<string, int> dictionary;
        private XmlDictionaryString[] dictionaryStrings1;
        private XmlDictionaryString[] dictionaryStrings2;
        private ServiceModelStrings strings;
        public static readonly ServiceModelDictionary Version1 = new ServiceModelDictionary(new ServiceModelStringsVersion1());
        private XmlDictionaryString[] versionedDictionaryStrings;

        public ServiceModelDictionary(ServiceModelStrings strings)
        {
            this.strings = strings;
            this.count = strings.Count;
        }

        public XmlDictionaryString CreateString(string value, int key)
        {
            return new XmlDictionaryString(this, value, key);
        }

        public bool TryLookup(int key, out XmlDictionaryString value)
        {
            XmlDictionaryString str;
            if ((key < 0) || (key >= this.count))
            {
                value = null;
                return false;
            }
            if (key < 0x20)
            {
                if (this.dictionaryStrings1 == null)
                {
                    this.dictionaryStrings1 = new XmlDictionaryString[0x20];
                }
                str = this.dictionaryStrings1[key];
                if (str == null)
                {
                    str = this.CreateString(this.strings[key], key);
                    this.dictionaryStrings1[key] = str;
                }
            }
            else
            {
                if (this.dictionaryStrings2 == null)
                {
                    this.dictionaryStrings2 = new XmlDictionaryString[this.count - 0x20];
                }
                str = this.dictionaryStrings2[key - 0x20];
                if (str == null)
                {
                    str = this.CreateString(this.strings[key], key);
                    this.dictionaryStrings2[key - 0x20] = str;
                }
            }
            value = str;
            return true;
        }

        public bool TryLookup(string key, out XmlDictionaryString value)
        {
            int num2;
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
            }
            if (this.dictionary == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(this.count);
                for (int i = 0; i < this.count; i++)
                {
                    dictionary.Add(this.strings[i], i);
                }
                this.dictionary = dictionary;
            }
            if (this.dictionary.TryGetValue(key, out num2))
            {
                return this.TryLookup(num2, out value);
            }
            value = null;
            return false;
        }

        public bool TryLookup(XmlDictionaryString key, out XmlDictionaryString value)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
            }
            if (key.Dictionary == this)
            {
                value = key;
                return true;
            }
            if (key.Dictionary == CurrentVersion)
            {
                if (this.versionedDictionaryStrings == null)
                {
                    this.versionedDictionaryStrings = new XmlDictionaryString[CurrentVersion.count];
                }
                XmlDictionaryString str = this.versionedDictionaryStrings[key.Key];
                if (str == null)
                {
                    if (!this.TryLookup(key.Value, out str))
                    {
                        value = null;
                        return false;
                    }
                    this.versionedDictionaryStrings[key.Key] = str;
                }
                value = str;
                return true;
            }
            value = null;
            return false;
        }

        public static ServiceModelDictionary CurrentVersion
        {
            get
            {
                return Version1;
            }
        }
    }
}

