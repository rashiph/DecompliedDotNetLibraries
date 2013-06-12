namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Xml.Schema;

    internal class SchemaObjectCache
    {
        private Hashtable graph;
        private Hashtable hash;
        internal Hashtable looks = new Hashtable();
        private Hashtable objectCache;
        private StringCollection warnings;

        internal XmlSchemaObject AddItem(XmlSchemaObject item, XmlQualifiedName qname, XmlSchemas schemas)
        {
            if (item == null)
            {
                return null;
            }
            if ((qname == null) || qname.IsEmpty)
            {
                return null;
            }
            string str = item.GetType().Name + ":" + qname.ToString();
            ArrayList list = (ArrayList) this.ObjectCache[str];
            if (list == null)
            {
                list = new ArrayList();
                this.ObjectCache[str] = list;
            }
            for (int i = 0; i < list.Count; i++)
            {
                XmlSchemaObject obj2 = (XmlSchemaObject) list[i];
                if (obj2 == item)
                {
                    return obj2;
                }
                if (this.Match(obj2, item, true))
                {
                    return obj2;
                }
                this.Warnings.Add(Res.GetString("XmlMismatchSchemaObjects", new object[] { item.GetType().Name, qname.Name, qname.Namespace }));
                this.Warnings.Add("DEBUG:Cached item key:\r\n" + ((string) this.looks[obj2]) + "\r\nnew item key:\r\n" + ((string) this.looks[item]));
            }
            list.Add(item);
            return item;
        }

        private int CompositeHash(XmlSchemaObject o, int hash)
        {
            ArrayList list = this.GetDependencies(o, new ArrayList(), new Hashtable());
            double num = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                object obj2 = this.Hash[list[i]];
                if (obj2 is int)
                {
                    num += ((int) obj2) / list.Count;
                }
            }
            return (int) num;
        }

        internal void GenerateSchemaGraph(XmlSchemas schemas)
        {
            ArrayList items = new SchemaGraph(this.Graph, schemas).GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                this.GetHash((XmlSchemaObject) items[i]);
            }
        }

        private ArrayList GetDependencies(XmlSchemaObject o, ArrayList deps, Hashtable refs)
        {
            if (refs[o] == null)
            {
                refs[o] = o;
                deps.Add(o);
                ArrayList list = this.Graph[o] as ArrayList;
                if (list == null)
                {
                    return deps;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    this.GetDependencies((XmlSchemaObject) list[i], deps, refs);
                }
            }
            return deps;
        }

        private int GetHash(XmlSchemaObject o)
        {
            object obj2 = this.Hash[o];
            if ((obj2 != null) && !(obj2 is XmlSchemaObject))
            {
                return (int) obj2;
            }
            string str = this.ToString(o, new SchemaObjectWriter());
            this.looks[o] = str;
            int hashCode = str.GetHashCode();
            this.Hash[o] = hashCode;
            return hashCode;
        }

        internal bool Match(XmlSchemaObject o1, XmlSchemaObject o2, bool shareTypes)
        {
            if (o1 != o2)
            {
                if (o1.GetType() != o2.GetType())
                {
                    return false;
                }
                if (this.Hash[o1] == null)
                {
                    this.Hash[o1] = this.GetHash(o1);
                }
                int hash = (int) this.Hash[o1];
                int num2 = this.GetHash(o2);
                if (hash != num2)
                {
                    return false;
                }
                if (shareTypes)
                {
                    return (this.CompositeHash(o1, hash) == this.CompositeHash(o2, num2));
                }
            }
            return true;
        }

        private string ToString(XmlSchemaObject o, SchemaObjectWriter writer)
        {
            return writer.WriteXmlSchemaObject(o);
        }

        private Hashtable Graph
        {
            get
            {
                if (this.graph == null)
                {
                    this.graph = new Hashtable();
                }
                return this.graph;
            }
        }

        private Hashtable Hash
        {
            get
            {
                if (this.hash == null)
                {
                    this.hash = new Hashtable();
                }
                return this.hash;
            }
        }

        private Hashtable ObjectCache
        {
            get
            {
                if (this.objectCache == null)
                {
                    this.objectCache = new Hashtable();
                }
                return this.objectCache;
            }
        }

        internal StringCollection Warnings
        {
            get
            {
                if (this.warnings == null)
                {
                    this.warnings = new StringCollection();
                }
                return this.warnings;
            }
        }
    }
}

