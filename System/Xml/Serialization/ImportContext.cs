namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    public class ImportContext
    {
        private SchemaObjectCache cache;
        private Hashtable elements;
        private Hashtable mappings;
        private bool shareTypes;
        private CodeIdentifiers typeIdentifiers;

        internal ImportContext() : this(null, false)
        {
        }

        public ImportContext(CodeIdentifiers identifiers, bool shareTypes)
        {
            this.typeIdentifiers = identifiers;
            this.shareTypes = shareTypes;
        }

        internal SchemaObjectCache Cache
        {
            get
            {
                if (this.cache == null)
                {
                    this.cache = new SchemaObjectCache();
                }
                return this.cache;
            }
        }

        internal Hashtable Elements
        {
            get
            {
                if (this.elements == null)
                {
                    this.elements = new Hashtable();
                }
                return this.elements;
            }
        }

        internal Hashtable Mappings
        {
            get
            {
                if (this.mappings == null)
                {
                    this.mappings = new Hashtable();
                }
                return this.mappings;
            }
        }

        public bool ShareTypes
        {
            get
            {
                return this.shareTypes;
            }
        }

        public CodeIdentifiers TypeIdentifiers
        {
            get
            {
                if (this.typeIdentifiers == null)
                {
                    this.typeIdentifiers = new CodeIdentifiers();
                }
                return this.typeIdentifiers;
            }
        }

        public StringCollection Warnings
        {
            get
            {
                return this.Cache.Warnings;
            }
        }
    }
}

