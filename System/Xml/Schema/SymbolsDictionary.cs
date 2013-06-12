namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    internal class SymbolsDictionary
    {
        private bool isUpaEnforced = true;
        private int last;
        private Hashtable names = new Hashtable();
        private object particleLast;
        private ArrayList particles = new ArrayList();
        private Hashtable wildcards;

        public int AddName(XmlQualifiedName name, object particle)
        {
            object obj2 = this.names[name];
            if (obj2 != null)
            {
                int num = (int) obj2;
                if (this.particles[num] != particle)
                {
                    this.isUpaEnforced = false;
                }
                return num;
            }
            this.names.Add(name, this.last);
            this.particles.Add(particle);
            return this.last++;
        }

        public void AddNamespaceList(NamespaceList list, object particle, bool allowLocal)
        {
            switch (list.Type)
            {
                case NamespaceList.ListType.Any:
                    this.particleLast = particle;
                    return;

                case NamespaceList.ListType.Other:
                    this.AddWildcard(list.Excluded, null);
                    if (allowLocal)
                    {
                        break;
                    }
                    this.AddWildcard(string.Empty, null);
                    return;

                case NamespaceList.ListType.Set:
                    foreach (string str in list.Enumerate)
                    {
                        this.AddWildcard(str, particle);
                    }
                    break;

                default:
                    return;
            }
        }

        private void AddWildcard(string wildcard, object particle)
        {
            if (this.wildcards == null)
            {
                this.wildcards = new Hashtable();
            }
            object obj2 = this.wildcards[wildcard];
            if (obj2 == null)
            {
                this.wildcards.Add(wildcard, this.last);
                this.particles.Add(particle);
                this.last++;
            }
            else if (particle != null)
            {
                this.particles[(int) obj2] = particle;
            }
        }

        public bool Exists(XmlQualifiedName name)
        {
            return (this.names[name] != null);
        }

        public ICollection GetNamespaceListSymbols(NamespaceList list)
        {
            ArrayList list2 = new ArrayList();
            foreach (XmlQualifiedName name in this.names.Keys)
            {
                if ((name != XmlQualifiedName.Empty) && list.Allows(name))
                {
                    list2.Add(this.names[name]);
                }
            }
            if (this.wildcards != null)
            {
                foreach (string str in this.wildcards.Keys)
                {
                    if (list.Allows(str))
                    {
                        list2.Add(this.wildcards[str]);
                    }
                }
            }
            if ((list.Type == NamespaceList.ListType.Any) || (list.Type == NamespaceList.ListType.Other))
            {
                list2.Add(this.last);
            }
            return list2;
        }

        public object GetParticle(int symbol)
        {
            if (symbol != this.last)
            {
                return this.particles[symbol];
            }
            return this.particleLast;
        }

        public string NameOf(int symbol)
        {
            foreach (DictionaryEntry entry in this.names)
            {
                if (((int) entry.Value) == symbol)
                {
                    return ((XmlQualifiedName) entry.Key).ToString();
                }
            }
            if (this.wildcards != null)
            {
                foreach (DictionaryEntry entry2 in this.wildcards)
                {
                    if (((int) entry2.Value) == symbol)
                    {
                        return (((string) entry2.Key) + ":*");
                    }
                }
            }
            return "##other:*";
        }

        public int Count
        {
            get
            {
                return (this.last + 1);
            }
        }

        public int CountOfNames
        {
            get
            {
                return this.names.Count;
            }
        }

        public bool IsUpaEnforced
        {
            get
            {
                return this.isUpaEnforced;
            }
            set
            {
                this.isUpaEnforced = value;
            }
        }

        public int this[XmlQualifiedName name]
        {
            get
            {
                object obj2 = this.names[name];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                if (this.wildcards != null)
                {
                    obj2 = this.wildcards[name.Namespace];
                    if (obj2 != null)
                    {
                        return (int) obj2;
                    }
                }
                return this.last;
            }
        }
    }
}

