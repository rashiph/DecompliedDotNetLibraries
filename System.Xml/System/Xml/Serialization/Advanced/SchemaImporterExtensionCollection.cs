namespace System.Xml.Serialization.Advanced
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    public class SchemaImporterExtensionCollection : CollectionBase
    {
        private Hashtable exNames;

        public int Add(SchemaImporterExtension extension)
        {
            return this.Add(extension.GetType().FullName, extension);
        }

        public int Add(string name, Type type)
        {
            if (!type.IsSubclassOf(typeof(SchemaImporterExtension)))
            {
                throw new ArgumentException(Res.GetString("XmlInvalidSchemaExtension", new object[] { type }));
            }
            return this.Add(name, (SchemaImporterExtension) Activator.CreateInstance(type));
        }

        internal int Add(string name, SchemaImporterExtension extension)
        {
            if (this.Names[name] != null)
            {
                if (this.Names[name].GetType() != extension.GetType())
                {
                    throw new InvalidOperationException(Res.GetString("XmlConfigurationDuplicateExtension", new object[] { name }));
                }
                return -1;
            }
            this.Names[name] = extension;
            return base.List.Add(extension);
        }

        public void Clear()
        {
            this.Names.Clear();
            base.List.Clear();
        }

        internal SchemaImporterExtensionCollection Clone()
        {
            SchemaImporterExtensionCollection extensions = new SchemaImporterExtensionCollection {
                exNames = (Hashtable) this.Names.Clone()
            };
            foreach (object obj2 in base.List)
            {
                extensions.List.Add(obj2);
            }
            return extensions;
        }

        public bool Contains(SchemaImporterExtension extension)
        {
            return base.List.Contains(extension);
        }

        public void CopyTo(SchemaImporterExtension[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(SchemaImporterExtension extension)
        {
            return base.List.IndexOf(extension);
        }

        public void Insert(int index, SchemaImporterExtension extension)
        {
            base.List.Insert(index, extension);
        }

        public void Remove(string name)
        {
            if (this.Names[name] != null)
            {
                base.List.Remove(this.Names[name]);
                this.Names[name] = null;
            }
        }

        public void Remove(SchemaImporterExtension extension)
        {
            base.List.Remove(extension);
        }

        public SchemaImporterExtension this[int index]
        {
            get
            {
                return (SchemaImporterExtension) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        internal Hashtable Names
        {
            get
            {
                if (this.exNames == null)
                {
                    this.exNames = new Hashtable();
                }
                return this.exNames;
            }
        }
    }
}

