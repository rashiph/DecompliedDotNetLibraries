namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;

    public class CodeIdentifiers
    {
        private bool camelCase;
        private Hashtable identifiers;
        private ArrayList list;
        private Hashtable reservedIdentifiers;

        public CodeIdentifiers() : this(true)
        {
        }

        public CodeIdentifiers(bool caseSensitive)
        {
            if (caseSensitive)
            {
                this.identifiers = new Hashtable();
                this.reservedIdentifiers = new Hashtable();
            }
            else
            {
                IEqualityComparer equalityComparer = new CaseInsensitiveKeyComparer();
                this.identifiers = new Hashtable(equalityComparer);
                this.reservedIdentifiers = new Hashtable(equalityComparer);
            }
            this.list = new ArrayList();
        }

        public void Add(string identifier, object value)
        {
            this.identifiers.Add(identifier, value);
            this.list.Add(value);
        }

        public void AddReserved(string identifier)
        {
            this.reservedIdentifiers.Add(identifier, identifier);
        }

        public string AddUnique(string identifier, object value)
        {
            identifier = this.MakeUnique(identifier);
            this.Add(identifier, value);
            return identifier;
        }

        public void Clear()
        {
            this.identifiers.Clear();
            this.list.Clear();
        }

        internal CodeIdentifiers Clone()
        {
            return new CodeIdentifiers { identifiers = (Hashtable) this.identifiers.Clone(), reservedIdentifiers = (Hashtable) this.reservedIdentifiers.Clone(), list = (ArrayList) this.list.Clone(), camelCase = this.camelCase };
        }

        public bool IsInUse(string identifier)
        {
            if (!this.identifiers.Contains(identifier))
            {
                return this.reservedIdentifiers.Contains(identifier);
            }
            return true;
        }

        public string MakeRightCase(string identifier)
        {
            if (this.camelCase)
            {
                return CodeIdentifier.MakeCamel(identifier);
            }
            return CodeIdentifier.MakePascal(identifier);
        }

        public string MakeUnique(string identifier)
        {
            if (this.IsInUse(identifier))
            {
                int num = 1;
                while (true)
                {
                    string str = identifier + num.ToString(CultureInfo.InvariantCulture);
                    if (!this.IsInUse(str))
                    {
                        identifier = str;
                        break;
                    }
                    num++;
                }
            }
            if (identifier.Length > 0x1ff)
            {
                return this.MakeUnique("Item");
            }
            return identifier;
        }

        public void Remove(string identifier)
        {
            this.list.Remove(this.identifiers[identifier]);
            this.identifiers.Remove(identifier);
        }

        public void RemoveReserved(string identifier)
        {
            this.reservedIdentifiers.Remove(identifier);
        }

        public object ToArray(Type type)
        {
            Array array = Array.CreateInstance(type, this.list.Count);
            this.list.CopyTo(array, 0);
            return array;
        }

        public bool UseCamelCasing
        {
            get
            {
                return this.camelCase;
            }
            set
            {
                this.camelCase = value;
            }
        }
    }
}

