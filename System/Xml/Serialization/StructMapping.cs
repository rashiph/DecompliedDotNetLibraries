namespace System.Xml.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class StructMapping : TypeMapping, INameScope
    {
        private System.Xml.Serialization.NameTable attributes;
        private StructMapping baseMapping;
        private StructMapping derivedMappings;
        private System.Xml.Serialization.NameTable elements;
        private bool hasSimpleContent;
        private bool isSequence;
        private MemberMapping[] members;
        private StructMapping nextDerivedMapping;
        private bool openModel;
        private CodeIdentifiers scope;
        private MemberMapping xmlnsMember;

        internal bool Declares(MemberMapping member, string parent)
        {
            StructMapping mapping;
            return (this.FindDeclaringMapping(member, out mapping, parent) != null);
        }

        internal MemberMapping FindDeclaringMapping(MemberMapping member, out StructMapping declaringMapping, string parent)
        {
            declaringMapping = null;
            if (this.BaseMapping != null)
            {
                MemberMapping mapping = this.BaseMapping.FindDeclaringMapping(member, out declaringMapping, parent);
                if (mapping != null)
                {
                    return mapping;
                }
            }
            if (this.members != null)
            {
                for (int i = 0; i < this.members.Length; i++)
                {
                    if (this.members[i].Name == member.Name)
                    {
                        if (this.members[i].TypeDesc != member.TypeDesc)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlHiddenMember", new object[] { parent, member.Name, member.TypeDesc.FullName, base.TypeName, this.members[i].Name, this.members[i].TypeDesc.FullName }));
                        }
                        if (!this.members[i].Match(member))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidXmlOverride", new object[] { parent, member.Name, base.TypeName, this.members[i].Name }));
                        }
                        declaringMapping = this;
                        return this.members[i];
                    }
                }
            }
            return null;
        }

        internal bool HasExplicitSequence()
        {
            if (this.members != null)
            {
                for (int i = 0; i < this.members.Length; i++)
                {
                    if (this.members[i].IsParticle && this.members[i].IsSequence)
                    {
                        return true;
                    }
                }
            }
            return ((this.baseMapping != null) && this.baseMapping.HasExplicitSequence());
        }

        internal void SetContentModel(TextAccessor text, bool hasElements)
        {
            if ((this.BaseMapping == null) || this.BaseMapping.TypeDesc.IsRoot)
            {
                this.hasSimpleContent = (!hasElements && (text != null)) && !text.Mapping.IsList;
            }
            else if (this.BaseMapping.HasSimpleContent)
            {
                if ((text != null) || hasElements)
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalSimpleContentExtension", new object[] { base.TypeDesc.FullName, this.BaseMapping.TypeDesc.FullName }));
                }
                this.hasSimpleContent = true;
            }
            else
            {
                this.hasSimpleContent = false;
            }
            if ((!this.hasSimpleContent && (text != null)) && !text.Mapping.TypeDesc.CanBeTextValue)
            {
                throw new InvalidOperationException(Res.GetString("XmlIllegalTypedTextAttribute", new object[] { base.TypeDesc.FullName, text.Name, text.Mapping.TypeDesc.FullName }));
            }
        }

        internal void SetSequence()
        {
            if (!base.TypeDesc.IsRoot)
            {
                StructMapping baseMapping = this;
                while ((!baseMapping.BaseMapping.IsSequence && (baseMapping.BaseMapping != null)) && !baseMapping.BaseMapping.TypeDesc.IsRoot)
                {
                    baseMapping = baseMapping.BaseMapping;
                }
                baseMapping.IsSequence = true;
                for (StructMapping mapping2 = baseMapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
                {
                    mapping2.SetSequence();
                }
            }
        }

        internal StructMapping BaseMapping
        {
            get
            {
                return this.baseMapping;
            }
            set
            {
                this.baseMapping = value;
                if (!base.IsAnonymousType && (this.baseMapping != null))
                {
                    this.nextDerivedMapping = this.baseMapping.derivedMappings;
                    this.baseMapping.derivedMappings = this;
                }
                if (value.isSequence && !this.isSequence)
                {
                    this.isSequence = true;
                    if (this.baseMapping.IsSequence)
                    {
                        for (StructMapping mapping = this.derivedMappings; mapping != null; mapping = mapping.NextDerivedMapping)
                        {
                            mapping.SetSequence();
                        }
                    }
                }
            }
        }

        internal StructMapping DerivedMappings
        {
            get
            {
                return this.derivedMappings;
            }
        }

        internal bool HasElements
        {
            get
            {
                return ((this.elements != null) && (this.elements.Values.Count > 0));
            }
        }

        internal bool HasSimpleContent
        {
            get
            {
                return this.hasSimpleContent;
            }
        }

        internal bool HasXmlnsMember
        {
            get
            {
                for (StructMapping mapping = this; mapping != null; mapping = mapping.BaseMapping)
                {
                    if (mapping.XmlnsMember != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal bool IsFullyInitialized
        {
            get
            {
                return ((this.baseMapping != null) && (this.Members != null));
            }
        }

        internal bool IsOpenModel
        {
            get
            {
                return this.openModel;
            }
            set
            {
                this.openModel = value;
            }
        }

        internal bool IsSequence
        {
            get
            {
                return (this.isSequence && !base.TypeDesc.IsRoot);
            }
            set
            {
                this.isSequence = value;
            }
        }

        internal System.Xml.Serialization.NameTable LocalAttributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new System.Xml.Serialization.NameTable();
                }
                return this.attributes;
            }
        }

        internal System.Xml.Serialization.NameTable LocalElements
        {
            get
            {
                if (this.elements == null)
                {
                    this.elements = new System.Xml.Serialization.NameTable();
                }
                return this.elements;
            }
        }

        internal MemberMapping[] Members
        {
            get
            {
                return this.members;
            }
            set
            {
                this.members = value;
            }
        }

        internal StructMapping NextDerivedMapping
        {
            get
            {
                return this.nextDerivedMapping;
            }
        }

        internal CodeIdentifiers Scope
        {
            get
            {
                if (this.scope == null)
                {
                    this.scope = new CodeIdentifiers();
                }
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }

        object INameScope.this[string name, string ns]
        {
            get
            {
                object obj2 = this.LocalElements[name, ns];
                if (obj2 != null)
                {
                    return obj2;
                }
                if (this.baseMapping != null)
                {
                    return ((INameScope) this.baseMapping)[name, ns];
                }
                return null;
            }
            set
            {
                this.LocalElements[name, ns] = value;
            }
        }

        internal MemberMapping XmlnsMember
        {
            get
            {
                return this.xmlnsMember;
            }
            set
            {
                this.xmlnsMember = value;
            }
        }
    }
}

