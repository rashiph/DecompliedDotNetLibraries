namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;

    internal class NamespaceList
    {
        private Hashtable set;
        private string targetNamespace;
        private ListType type;

        public NamespaceList()
        {
        }

        public NamespaceList(string namespaces, string targetNamespace)
        {
            this.targetNamespace = targetNamespace;
            namespaces = namespaces.Trim();
            if ((namespaces == "##any") || (namespaces.Length == 0))
            {
                this.type = ListType.Any;
            }
            else if (namespaces == "##other")
            {
                this.type = ListType.Other;
            }
            else
            {
                this.type = ListType.Set;
                this.set = new Hashtable();
                string[] strArray = XmlConvert.SplitString(namespaces);
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i] == "##local")
                    {
                        this.set[string.Empty] = string.Empty;
                    }
                    else if (strArray[i] == "##targetNamespace")
                    {
                        this.set[targetNamespace] = targetNamespace;
                    }
                    else
                    {
                        XmlConvert.ToUri(strArray[i]);
                        this.set[strArray[i]] = strArray[i];
                    }
                }
            }
        }

        public virtual bool Allows(string ns)
        {
            switch (this.type)
            {
                case ListType.Any:
                    return true;

                case ListType.Other:
                    if (!(ns != this.targetNamespace))
                    {
                        return false;
                    }
                    return (ns.Length != 0);

                case ListType.Set:
                    return (this.set[ns] != null);
            }
            return false;
        }

        public bool Allows(XmlQualifiedName qname)
        {
            return this.Allows(qname.Namespace);
        }

        public NamespaceList Clone()
        {
            NamespaceList list = (NamespaceList) base.MemberwiseClone();
            if (this.type == ListType.Set)
            {
                list.set = (Hashtable) this.set.Clone();
            }
            return list;
        }

        private NamespaceList CompareSetToOther(NamespaceList other)
        {
            if (this.set.Contains(other.targetNamespace))
            {
                if (this.set.Contains(string.Empty))
                {
                    return new NamespaceList();
                }
                return new NamespaceList("##other", string.Empty);
            }
            if (this.set.Contains(string.Empty))
            {
                return null;
            }
            return other.Clone();
        }

        public static NamespaceList Intersection(NamespaceList o1, NamespaceList o2, bool v1Compat)
        {
            NamespaceList list = null;
            if (o1.type == ListType.Any)
            {
                return o2.Clone();
            }
            if (o2.type == ListType.Any)
            {
                return o1.Clone();
            }
            if ((o1.type == ListType.Set) && (o2.type == ListType.Other))
            {
                list = o1.Clone();
                list.RemoveNamespace(o2.targetNamespace);
                if (!v1Compat)
                {
                    list.RemoveNamespace(string.Empty);
                }
                return list;
            }
            if ((o1.type == ListType.Other) && (o2.type == ListType.Set))
            {
                list = o2.Clone();
                list.RemoveNamespace(o1.targetNamespace);
                if (!v1Compat)
                {
                    list.RemoveNamespace(string.Empty);
                }
                return list;
            }
            if ((o1.type == ListType.Set) && (o2.type == ListType.Set))
            {
                list = o1.Clone();
                list = new NamespaceList {
                    type = ListType.Set,
                    set = new Hashtable()
                };
                foreach (string str in o1.set.Keys)
                {
                    if (o2.set.Contains(str))
                    {
                        list.set.Add(str, str);
                    }
                }
                return list;
            }
            if ((o1.type == ListType.Other) && (o2.type == ListType.Other))
            {
                if (o1.targetNamespace == o2.targetNamespace)
                {
                    return o1.Clone();
                }
                if (v1Compat)
                {
                    return list;
                }
                if (o1.targetNamespace == string.Empty)
                {
                    return o2.Clone();
                }
                if (o2.targetNamespace == string.Empty)
                {
                    list = o1.Clone();
                }
            }
            return list;
        }

        public bool IsEmpty()
        {
            if (this.type != ListType.Set)
            {
                return false;
            }
            if (this.set != null)
            {
                return (this.set.Count == 0);
            }
            return true;
        }

        public static bool IsSubset(NamespaceList sub, NamespaceList super)
        {
            if (super.type != ListType.Any)
            {
                if ((sub.type == ListType.Other) && (super.type == ListType.Other))
                {
                    return (super.targetNamespace == sub.targetNamespace);
                }
                if (sub.type != ListType.Set)
                {
                    return false;
                }
                if (super.type == ListType.Other)
                {
                    return !sub.set.Contains(super.targetNamespace);
                }
                foreach (string str in sub.set.Keys)
                {
                    if (!super.set.Contains(str))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void RemoveNamespace(string tns)
        {
            if (this.set[tns] != null)
            {
                this.set.Remove(tns);
            }
        }

        public override string ToString()
        {
            switch (this.type)
            {
                case ListType.Any:
                    return "##any";

                case ListType.Other:
                    return "##other";

                case ListType.Set:
                {
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    foreach (string str in this.set.Keys)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            builder.Append(" ");
                        }
                        if (str == this.targetNamespace)
                        {
                            builder.Append("##targetNamespace");
                        }
                        else if (str.Length == 0)
                        {
                            builder.Append("##local");
                        }
                        else
                        {
                            builder.Append(str);
                        }
                    }
                    return builder.ToString();
                }
            }
            return string.Empty;
        }

        public static NamespaceList Union(NamespaceList o1, NamespaceList o2, bool v1Compat)
        {
            NamespaceList list = null;
            if (o1.type == ListType.Any)
            {
                return new NamespaceList();
            }
            if (o2.type == ListType.Any)
            {
                return new NamespaceList();
            }
            if ((o1.type == ListType.Set) && (o2.type == ListType.Set))
            {
                list = o1.Clone();
                foreach (string str in o2.set.Keys)
                {
                    list.set[str] = str;
                }
                return list;
            }
            if ((o1.type == ListType.Other) && (o2.type == ListType.Other))
            {
                if (o1.targetNamespace == o2.targetNamespace)
                {
                    return o1.Clone();
                }
                return new NamespaceList("##other", string.Empty);
            }
            if ((o1.type == ListType.Set) && (o2.type == ListType.Other))
            {
                if (v1Compat)
                {
                    if (o1.set.Contains(o2.targetNamespace))
                    {
                        return new NamespaceList();
                    }
                    return o2.Clone();
                }
                if (o2.targetNamespace != string.Empty)
                {
                    return o1.CompareSetToOther(o2);
                }
                if (o1.set.Contains(string.Empty))
                {
                    return new NamespaceList();
                }
                return new NamespaceList("##other", string.Empty);
            }
            if ((o2.type != ListType.Set) || (o1.type != ListType.Other))
            {
                return list;
            }
            if (v1Compat)
            {
                if (o2.set.Contains(o2.targetNamespace))
                {
                    return new NamespaceList();
                }
                return o1.Clone();
            }
            if (o1.targetNamespace != string.Empty)
            {
                return o2.CompareSetToOther(o1);
            }
            if (o2.set.Contains(string.Empty))
            {
                return new NamespaceList();
            }
            return new NamespaceList("##other", string.Empty);
        }

        public ICollection Enumerate
        {
            get
            {
                switch (this.type)
                {
                    case ListType.Set:
                        return this.set.Keys;
                }
                throw new InvalidOperationException();
            }
        }

        public string Excluded
        {
            get
            {
                return this.targetNamespace;
            }
        }

        public ListType Type
        {
            get
            {
                return this.type;
            }
        }

        public enum ListType
        {
            Any,
            Other,
            Set
        }
    }
}

