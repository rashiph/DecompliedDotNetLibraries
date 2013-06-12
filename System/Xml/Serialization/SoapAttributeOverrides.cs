namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    public class SoapAttributeOverrides
    {
        private Hashtable types = new Hashtable();

        public void Add(Type type, SoapAttributes attributes)
        {
            this.Add(type, string.Empty, attributes);
        }

        public void Add(Type type, string member, SoapAttributes attributes)
        {
            Hashtable hashtable = (Hashtable) this.types[type];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                this.types.Add(type, hashtable);
            }
            else if (hashtable[member] != null)
            {
                throw new InvalidOperationException(Res.GetString("XmlMultipleAttributeOverrides", new object[] { type.FullName, member }));
            }
            hashtable.Add(member, attributes);
        }

        public SoapAttributes this[Type type]
        {
            get
            {
                return this[type, string.Empty];
            }
        }

        public SoapAttributes this[Type type, string member]
        {
            get
            {
                Hashtable hashtable = (Hashtable) this.types[type];
                if (hashtable == null)
                {
                    return null;
                }
                return (SoapAttributes) hashtable[member];
            }
        }
    }
}

