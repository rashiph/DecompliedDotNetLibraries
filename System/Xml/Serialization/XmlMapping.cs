namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    public abstract class XmlMapping
    {
        private XmlMappingAccess access;
        private ElementAccessor accessor;
        private bool generateSerializer;
        private bool isSoap;
        private string key;
        private TypeScope scope;
        private bool shallow;

        internal XmlMapping(TypeScope scope, ElementAccessor accessor) : this(scope, accessor, XmlMappingAccess.Write | XmlMappingAccess.Read)
        {
        }

        internal XmlMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access)
        {
            this.scope = scope;
            this.accessor = accessor;
            this.access = access;
            this.shallow = scope == null;
        }

        internal void CheckShallow()
        {
            if (this.shallow)
            {
                throw new InvalidOperationException(Res.GetString("XmlMelformMapping"));
            }
        }

        internal static string GenerateKey(Type type, XmlRootAttribute root, string ns)
        {
            if (root == null)
            {
                root = (XmlRootAttribute) XmlAttributes.GetAttr(type, typeof(XmlRootAttribute));
            }
            return (type.FullName + ":" + ((root == null) ? string.Empty : root.Key) + ":" + ((ns == null) ? string.Empty : ns));
        }

        internal static bool IsShallow(XmlMapping[] mappings)
        {
            for (int i = 0; i < mappings.Length; i++)
            {
                if ((mappings[i] == null) || mappings[i].shallow)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetKey(string key)
        {
            this.SetKeyInternal(key);
        }

        internal void SetKeyInternal(string key)
        {
            this.key = key;
        }

        internal ElementAccessor Accessor
        {
            get
            {
                return this.accessor;
            }
        }

        public string ElementName
        {
            get
            {
                return System.Xml.Serialization.Accessor.UnescapeName(this.Accessor.Name);
            }
        }

        internal bool GenerateSerializer
        {
            get
            {
                return this.generateSerializer;
            }
            set
            {
                this.generateSerializer = value;
            }
        }

        internal bool IsReadable
        {
            get
            {
                return ((this.access & XmlMappingAccess.Read) != XmlMappingAccess.None);
            }
        }

        internal bool IsSoap
        {
            get
            {
                return this.isSoap;
            }
            set
            {
                this.isSoap = value;
            }
        }

        internal bool IsWriteable
        {
            get
            {
                return ((this.access & XmlMappingAccess.Write) != XmlMappingAccess.None);
            }
        }

        internal string Key
        {
            get
            {
                return this.key;
            }
        }

        public string Namespace
        {
            get
            {
                return this.accessor.Namespace;
            }
        }

        internal TypeScope Scope
        {
            get
            {
                return this.scope;
            }
        }

        public string XsdElementName
        {
            get
            {
                return this.Accessor.Name;
            }
        }
    }
}

