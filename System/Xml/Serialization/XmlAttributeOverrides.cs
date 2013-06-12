namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    public class XmlAttributeOverrides
    {
        private Hashtable types = new Hashtable();

        public void Add(Type type, XmlAttributes attributes)
        {
            this.Add(type, string.Empty, attributes);
        }

        public void Add(Type type, string member, XmlAttributes attributes)
        {
            Hashtable hashtable = (Hashtable) this.types[type];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                this.types.Add(type, hashtable);
            }
            else if (hashtable[member] != null)
            {
                throw new InvalidOperationException(Res.GetString("XmlAttributeSetAgain", new object[] { type.FullName, member }));
            }
            hashtable.Add(member, attributes);
        }

        public XmlAttributes this[Type type]
        {
            get
            {
                return this[type, string.Empty];
            }
        }

        public XmlAttributes this[Type type, string member]
        {
            get
            {
                Hashtable hashtable = (Hashtable) this.types[type];
                if (hashtable == null)
                {
                    return null;
                }
                return (XmlAttributes) hashtable[member];
            }
        }
    }
}

