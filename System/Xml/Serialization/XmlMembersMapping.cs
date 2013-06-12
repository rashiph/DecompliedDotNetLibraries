namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;
    using System.Text;

    public class XmlMembersMapping : XmlMapping
    {
        private XmlMemberMapping[] mappings;

        internal XmlMembersMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access) : base(scope, accessor, access)
        {
            MembersMapping mapping = (MembersMapping) accessor.Mapping;
            StringBuilder builder = new StringBuilder();
            builder.Append(":");
            this.mappings = new XmlMemberMapping[mapping.Members.Length];
            for (int i = 0; i < this.mappings.Length; i++)
            {
                if (mapping.Members[i].TypeDesc.Type != null)
                {
                    builder.Append(XmlMapping.GenerateKey(mapping.Members[i].TypeDesc.Type, null, null));
                    builder.Append(":");
                }
                this.mappings[i] = new XmlMemberMapping(mapping.Members[i]);
            }
            base.SetKeyInternal(builder.ToString());
        }

        public int Count
        {
            get
            {
                return this.mappings.Length;
            }
        }

        public XmlMemberMapping this[int index]
        {
            get
            {
                return this.mappings[index];
            }
        }

        public string TypeName
        {
            get
            {
                return base.Accessor.Mapping.TypeName;
            }
        }

        public string TypeNamespace
        {
            get
            {
                return base.Accessor.Mapping.Namespace;
            }
        }
    }
}

