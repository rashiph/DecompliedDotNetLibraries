namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;

    internal class SchemaInfo : IDtdInfo
    {
        private Dictionary<XmlQualifiedName, SchemaAttDef> attributeDecls = new Dictionary<XmlQualifiedName, SchemaAttDef>();
        private XmlQualifiedName docTypeName = XmlQualifiedName.Empty;
        private Dictionary<XmlQualifiedName, SchemaElementDecl> elementDecls = new Dictionary<XmlQualifiedName, SchemaElementDecl>();
        private Dictionary<XmlQualifiedName, SchemaElementDecl> elementDeclsByType = new Dictionary<XmlQualifiedName, SchemaElementDecl>();
        private int errorCount;
        private Dictionary<XmlQualifiedName, SchemaEntity> generalEntities;
        private bool hasDefaultAttributes;
        private bool hasNonCDataAttributes;
        private string internalDtdSubset = string.Empty;
        private Dictionary<string, SchemaNotation> notations;
        private Dictionary<XmlQualifiedName, SchemaEntity> parameterEntities;
        private System.Xml.Schema.SchemaType schemaType = System.Xml.Schema.SchemaType.None;
        private Dictionary<string, bool> targetNamespaces = new Dictionary<string, bool>();
        private Dictionary<XmlQualifiedName, SchemaElementDecl> undeclaredElementDecls = new Dictionary<XmlQualifiedName, SchemaElementDecl>();

        internal SchemaInfo()
        {
        }

        internal void Add(SchemaInfo sinfo, ValidationEventHandler eventhandler)
        {
            if (this.schemaType == System.Xml.Schema.SchemaType.None)
            {
                this.schemaType = sinfo.SchemaType;
            }
            else if (this.schemaType != sinfo.SchemaType)
            {
                if (eventhandler != null)
                {
                    eventhandler(this, new ValidationEventArgs(new XmlSchemaException("Sch_MixSchemaTypes", string.Empty)));
                }
                return;
            }
            foreach (string str in sinfo.TargetNamespaces.Keys)
            {
                if (!this.targetNamespaces.ContainsKey(str))
                {
                    this.targetNamespaces.Add(str, true);
                }
            }
            foreach (KeyValuePair<XmlQualifiedName, SchemaElementDecl> pair in sinfo.elementDecls)
            {
                if (!this.elementDecls.ContainsKey(pair.Key))
                {
                    this.elementDecls.Add(pair.Key, pair.Value);
                }
            }
            foreach (KeyValuePair<XmlQualifiedName, SchemaElementDecl> pair2 in sinfo.elementDeclsByType)
            {
                if (!this.elementDeclsByType.ContainsKey(pair2.Key))
                {
                    this.elementDeclsByType.Add(pair2.Key, pair2.Value);
                }
            }
            foreach (SchemaAttDef def in sinfo.AttributeDecls.Values)
            {
                if (!this.attributeDecls.ContainsKey(def.Name))
                {
                    this.attributeDecls.Add(def.Name, def);
                }
            }
            foreach (SchemaNotation notation in sinfo.Notations.Values)
            {
                if (!this.Notations.ContainsKey(notation.Name.Name))
                {
                    this.Notations.Add(notation.Name.Name, notation);
                }
            }
        }

        internal bool Contains(string ns)
        {
            return this.targetNamespaces.ContainsKey(ns);
        }

        internal void Finish()
        {
            Dictionary<XmlQualifiedName, SchemaElementDecl> elementDecls = this.elementDecls;
            for (int i = 0; i < 2; i++)
            {
                foreach (SchemaElementDecl decl in elementDecls.Values)
                {
                    if (decl.HasNonCDataAttribute)
                    {
                        this.hasNonCDataAttributes = true;
                    }
                    if (decl.DefaultAttDefs != null)
                    {
                        this.hasDefaultAttributes = true;
                    }
                }
                elementDecls = this.undeclaredElementDecls;
            }
        }

        internal XmlSchemaAttribute GetAttribute(XmlQualifiedName qname)
        {
            SchemaAttDef def = this.attributeDecls[qname];
            if (def != null)
            {
                return def.SchemaAttribute;
            }
            return null;
        }

        internal SchemaAttDef GetAttributeXdr(SchemaElementDecl ed, XmlQualifiedName qname)
        {
            SchemaAttDef attDef = null;
            if (ed != null)
            {
                attDef = ed.GetAttDef(qname);
                if (attDef != null)
                {
                    return attDef;
                }
                if (!ed.ContentValidator.IsOpen || (qname.Namespace.Length == 0))
                {
                    throw new XmlSchemaException("Sch_UndeclaredAttribute", qname.ToString());
                }
                if (!this.attributeDecls.TryGetValue(qname, out attDef) && this.targetNamespaces.ContainsKey(qname.Namespace))
                {
                    throw new XmlSchemaException("Sch_UndeclaredAttribute", qname.ToString());
                }
            }
            return attDef;
        }

        internal SchemaAttDef GetAttributeXsd(SchemaElementDecl ed, XmlQualifiedName qname, ref bool skip)
        {
            AttributeMatchState state;
            SchemaAttDef def = this.GetAttributeXsd(ed, qname, null, out state);
            switch (state)
            {
                case AttributeMatchState.AttributeFound:
                case AttributeMatchState.AnyIdAttributeFound:
                case AttributeMatchState.UndeclaredElementAndAttribute:
                case AttributeMatchState.AnyAttributeLax:
                    return def;

                case AttributeMatchState.UndeclaredAttribute:
                    throw new XmlSchemaException("Sch_UndeclaredAttribute", qname.ToString());

                case AttributeMatchState.AnyAttributeSkip:
                    skip = true;
                    return def;

                case AttributeMatchState.ProhibitedAnyAttribute:
                case AttributeMatchState.ProhibitedAttribute:
                    throw new XmlSchemaException("Sch_ProhibitedAttribute", qname.ToString());
            }
            return def;
        }

        internal SchemaAttDef GetAttributeXsd(SchemaElementDecl ed, XmlQualifiedName qname, XmlSchemaObject partialValidationType, out AttributeMatchState attributeMatchState)
        {
            SchemaAttDef attDef = null;
            attributeMatchState = AttributeMatchState.UndeclaredAttribute;
            if (ed != null)
            {
                attDef = ed.GetAttDef(qname);
                if (attDef != null)
                {
                    attributeMatchState = AttributeMatchState.AttributeFound;
                    return attDef;
                }
                XmlSchemaAnyAttribute anyAttribute = ed.AnyAttribute;
                if (anyAttribute != null)
                {
                    if (!anyAttribute.NamespaceList.Allows(qname))
                    {
                        attributeMatchState = AttributeMatchState.ProhibitedAnyAttribute;
                        return attDef;
                    }
                    if (anyAttribute.ProcessContentsCorrect != XmlSchemaContentProcessing.Skip)
                    {
                        if (this.attributeDecls.TryGetValue(qname, out attDef))
                        {
                            if (attDef.Datatype.TypeCode == XmlTypeCode.Id)
                            {
                                attributeMatchState = AttributeMatchState.AnyIdAttributeFound;
                                return attDef;
                            }
                            attributeMatchState = AttributeMatchState.AttributeFound;
                            return attDef;
                        }
                        if (anyAttribute.ProcessContentsCorrect == XmlSchemaContentProcessing.Lax)
                        {
                            attributeMatchState = AttributeMatchState.AnyAttributeLax;
                        }
                        return attDef;
                    }
                    attributeMatchState = AttributeMatchState.AnyAttributeSkip;
                    return attDef;
                }
                if (ed.ProhibitedAttributes.ContainsKey(qname))
                {
                    attributeMatchState = AttributeMatchState.ProhibitedAttribute;
                }
                return attDef;
            }
            if (partialValidationType != null)
            {
                XmlSchemaAttribute attribute2 = partialValidationType as XmlSchemaAttribute;
                if (attribute2 != null)
                {
                    if (qname.Equals(attribute2.QualifiedName))
                    {
                        attDef = attribute2.AttDef;
                        attributeMatchState = AttributeMatchState.AttributeFound;
                        return attDef;
                    }
                    attributeMatchState = AttributeMatchState.AttributeNameMismatch;
                    return attDef;
                }
                attributeMatchState = AttributeMatchState.ValidateAttributeInvalidCall;
                return attDef;
            }
            if (this.attributeDecls.TryGetValue(qname, out attDef))
            {
                attributeMatchState = AttributeMatchState.AttributeFound;
                return attDef;
            }
            attributeMatchState = AttributeMatchState.UndeclaredElementAndAttribute;
            return attDef;
        }

        internal XmlSchemaElement GetElement(XmlQualifiedName qname)
        {
            SchemaElementDecl elementDecl = this.GetElementDecl(qname);
            if (elementDecl != null)
            {
                return elementDecl.SchemaElement;
            }
            return null;
        }

        internal SchemaElementDecl GetElementDecl(XmlQualifiedName qname)
        {
            SchemaElementDecl decl;
            if (this.elementDecls.TryGetValue(qname, out decl))
            {
                return decl;
            }
            return null;
        }

        internal XmlSchemaElement GetType(XmlQualifiedName qname)
        {
            SchemaElementDecl elementDecl = this.GetElementDecl(qname);
            if (elementDecl != null)
            {
                return elementDecl.SchemaElement;
            }
            return null;
        }

        internal SchemaElementDecl GetTypeDecl(XmlQualifiedName qname)
        {
            SchemaElementDecl decl;
            if (this.elementDeclsByType.TryGetValue(qname, out decl))
            {
                return decl;
            }
            return null;
        }

        internal bool HasSchema(string ns)
        {
            return this.targetNamespaces.ContainsKey(ns);
        }

        IEnumerable<IDtdAttributeListInfo> IDtdInfo.GetAttributeLists()
        {
            foreach (SchemaElementDecl iteratorVariable0 in this.elementDecls.Values)
            {
                IDtdAttributeListInfo iteratorVariable1 = iteratorVariable0;
                yield return iteratorVariable1;
            }
        }

        IDtdAttributeListInfo IDtdInfo.LookupAttributeList(string prefix, string localName)
        {
            SchemaElementDecl decl;
            XmlQualifiedName key = new XmlQualifiedName(prefix, localName);
            if (!this.elementDecls.TryGetValue(key, out decl))
            {
                this.undeclaredElementDecls.TryGetValue(key, out decl);
            }
            return decl;
        }

        IDtdEntityInfo IDtdInfo.LookupEntity(string name)
        {
            if (this.generalEntities != null)
            {
                SchemaEntity entity;
                XmlQualifiedName key = new XmlQualifiedName(name);
                if (this.generalEntities.TryGetValue(key, out entity))
                {
                    return entity;
                }
            }
            return null;
        }

        internal Dictionary<XmlQualifiedName, SchemaAttDef> AttributeDecls
        {
            get
            {
                return this.attributeDecls;
            }
        }

        public XmlQualifiedName DocTypeName
        {
            get
            {
                return this.docTypeName;
            }
            set
            {
                this.docTypeName = value;
            }
        }

        internal Dictionary<XmlQualifiedName, SchemaElementDecl> ElementDecls
        {
            get
            {
                return this.elementDecls;
            }
        }

        internal Dictionary<XmlQualifiedName, SchemaElementDecl> ElementDeclsByType
        {
            get
            {
                return this.elementDeclsByType;
            }
        }

        internal int ErrorCount
        {
            get
            {
                return this.errorCount;
            }
            set
            {
                this.errorCount = value;
            }
        }

        internal Dictionary<XmlQualifiedName, SchemaEntity> GeneralEntities
        {
            get
            {
                if (this.generalEntities == null)
                {
                    this.generalEntities = new Dictionary<XmlQualifiedName, SchemaEntity>();
                }
                return this.generalEntities;
            }
        }

        internal string InternalDtdSubset
        {
            get
            {
                return this.internalDtdSubset;
            }
            set
            {
                this.internalDtdSubset = value;
            }
        }

        internal Dictionary<string, SchemaNotation> Notations
        {
            get
            {
                if (this.notations == null)
                {
                    this.notations = new Dictionary<string, SchemaNotation>();
                }
                return this.notations;
            }
        }

        internal Dictionary<XmlQualifiedName, SchemaEntity> ParameterEntities
        {
            get
            {
                if (this.parameterEntities == null)
                {
                    this.parameterEntities = new Dictionary<XmlQualifiedName, SchemaEntity>();
                }
                return this.parameterEntities;
            }
        }

        internal System.Xml.Schema.SchemaType SchemaType
        {
            get
            {
                return this.schemaType;
            }
            set
            {
                this.schemaType = value;
            }
        }

        bool IDtdInfo.HasDefaultAttributes
        {
            get
            {
                return this.hasDefaultAttributes;
            }
        }

        bool IDtdInfo.HasNonCDataAttributes
        {
            get
            {
                return this.hasNonCDataAttributes;
            }
        }

        string IDtdInfo.InternalDtdSubset
        {
            get
            {
                return this.internalDtdSubset;
            }
        }

        XmlQualifiedName IDtdInfo.Name
        {
            get
            {
                return this.docTypeName;
            }
        }

        internal Dictionary<string, bool> TargetNamespaces
        {
            get
            {
                return this.targetNamespaces;
            }
        }

        internal Dictionary<XmlQualifiedName, SchemaElementDecl> UndeclaredElementDecls
        {
            get
            {
                return this.undeclaredElementDecls;
            }
        }

    }
}

