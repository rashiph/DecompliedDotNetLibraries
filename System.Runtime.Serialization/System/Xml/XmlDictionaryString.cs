namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    public class XmlDictionaryString
    {
        private byte[] buffer;
        private IXmlDictionary dictionary;
        private static EmptyStringDictionary emptyStringDictionary = new EmptyStringDictionary();
        private int key;
        internal const int MaxKey = 0x1fffffff;
        internal const int MinKey = 0;
        private string value;

        public XmlDictionaryString(IXmlDictionary dictionary, string value, int key)
        {
            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dictionary"));
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            if ((key < 0) || (key > 0x1fffffff))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("key", System.Runtime.Serialization.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x1fffffff })));
            }
            this.dictionary = dictionary;
            this.value = value;
            this.key = key;
        }

        internal static string GetString(XmlDictionaryString s)
        {
            if (s == null)
            {
                return null;
            }
            return s.Value;
        }

        public override string ToString()
        {
            return this.value;
        }

        internal byte[] ToUTF8()
        {
            if (this.buffer == null)
            {
                this.buffer = Encoding.UTF8.GetBytes(this.value);
            }
            return this.buffer;
        }

        public IXmlDictionary Dictionary
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dictionary;
            }
        }

        public static XmlDictionaryString Empty
        {
            get
            {
                return emptyStringDictionary.EmptyString;
            }
        }

        public int Key
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.key;
            }
        }

        public string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }

        private class EmptyStringDictionary : IXmlDictionary
        {
            private XmlDictionaryString empty;

            public EmptyStringDictionary()
            {
                this.empty = new XmlDictionaryString(this, string.Empty, 0);
            }

            public bool TryLookup(int key, out XmlDictionaryString result)
            {
                if (key == 0)
                {
                    result = this.empty;
                    return true;
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
                if (value.Length == 0)
                {
                    result = this.empty;
                    return true;
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

            public XmlDictionaryString EmptyString
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.empty;
                }
            }
        }
    }
}

