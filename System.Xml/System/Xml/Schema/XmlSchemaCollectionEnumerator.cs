namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    public sealed class XmlSchemaCollectionEnumerator : IEnumerator
    {
        private IDictionaryEnumerator enumerator;

        internal XmlSchemaCollectionEnumerator(Hashtable collection)
        {
            this.enumerator = collection.GetEnumerator();
        }

        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        bool IEnumerator.MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            this.enumerator.Reset();
        }

        public XmlSchema Current
        {
            get
            {
                XmlSchemaCollectionNode node = (XmlSchemaCollectionNode) this.enumerator.Value;
                if (node != null)
                {
                    return node.Schema;
                }
                return null;
            }
        }

        internal XmlSchemaCollectionNode CurrentNode
        {
            get
            {
                return (XmlSchemaCollectionNode) this.enumerator.Value;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }
    }
}

