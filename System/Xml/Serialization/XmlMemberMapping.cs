namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;

    public class XmlMemberMapping
    {
        private MemberMapping mapping;

        internal XmlMemberMapping(MemberMapping mapping)
        {
            this.mapping = mapping;
        }

        public string GenerateTypeName(CodeDomProvider codeProvider)
        {
            return this.mapping.GetTypeName(codeProvider);
        }

        internal System.Xml.Serialization.Accessor Accessor
        {
            get
            {
                return this.mapping.Accessor;
            }
        }

        public bool Any
        {
            get
            {
                return this.Accessor.Any;
            }
        }

        public bool CheckSpecified
        {
            get
            {
                return (this.mapping.CheckSpecified != SpecifiedAccessor.None);
            }
        }

        public string ElementName
        {
            get
            {
                return System.Xml.Serialization.Accessor.UnescapeName(this.Accessor.Name);
            }
        }

        internal bool IsNullable
        {
            get
            {
                return this.mapping.IsNeedNullable;
            }
        }

        internal MemberMapping Mapping
        {
            get
            {
                return this.mapping;
            }
        }

        public string MemberName
        {
            get
            {
                return this.mapping.Name;
            }
        }

        public string Namespace
        {
            get
            {
                return this.Accessor.Namespace;
            }
        }

        public string TypeFullName
        {
            get
            {
                return this.mapping.TypeDesc.FullName;
            }
        }

        public string TypeName
        {
            get
            {
                if (this.Accessor.Mapping == null)
                {
                    return string.Empty;
                }
                return this.Accessor.Mapping.TypeName;
            }
        }

        public string TypeNamespace
        {
            get
            {
                if (this.Accessor.Mapping == null)
                {
                    return null;
                }
                return this.Accessor.Mapping.Namespace;
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

