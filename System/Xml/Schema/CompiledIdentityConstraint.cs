namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class CompiledIdentityConstraint
    {
        public static readonly CompiledIdentityConstraint Empty = new CompiledIdentityConstraint();
        private Asttree[] fields;
        internal XmlQualifiedName name;
        internal XmlQualifiedName refer;
        private ConstraintRole role;
        private Asttree selector;

        private CompiledIdentityConstraint()
        {
            this.name = XmlQualifiedName.Empty;
            this.refer = XmlQualifiedName.Empty;
        }

        public CompiledIdentityConstraint(XmlSchemaIdentityConstraint constraint, XmlNamespaceManager nsmgr)
        {
            this.name = XmlQualifiedName.Empty;
            this.refer = XmlQualifiedName.Empty;
            this.name = constraint.QualifiedName;
            try
            {
                this.selector = new Asttree(constraint.Selector.XPath, false, nsmgr);
            }
            catch (XmlSchemaException exception)
            {
                exception.SetSource(constraint.Selector);
                throw exception;
            }
            XmlSchemaObjectCollection fields = constraint.Fields;
            this.fields = new Asttree[fields.Count];
            for (int i = 0; i < fields.Count; i++)
            {
                try
                {
                    this.fields[i] = new Asttree(((XmlSchemaXPath) fields[i]).XPath, true, nsmgr);
                }
                catch (XmlSchemaException exception2)
                {
                    exception2.SetSource(constraint.Fields[i]);
                    throw exception2;
                }
            }
            if (constraint is XmlSchemaUnique)
            {
                this.role = ConstraintRole.Unique;
            }
            else if (constraint is XmlSchemaKey)
            {
                this.role = ConstraintRole.Key;
            }
            else
            {
                this.role = ConstraintRole.Keyref;
                this.refer = ((XmlSchemaKeyref) constraint).Refer;
            }
        }

        public Asttree[] Fields
        {
            get
            {
                return this.fields;
            }
        }

        public ConstraintRole Role
        {
            get
            {
                return this.role;
            }
        }

        public Asttree Selector
        {
            get
            {
                return this.selector;
            }
        }

        public enum ConstraintRole
        {
            Unique,
            Key,
            Keyref
        }
    }
}

