namespace System.Xml
{
    using System;
    using System.Collections;

    internal sealed class XmlChildEnumerator : IEnumerator
    {
        internal XmlNode child;
        internal XmlNode container;
        internal bool isFirst;

        internal XmlChildEnumerator(XmlNode container)
        {
            this.container = container;
            this.child = container.FirstChild;
            this.isFirst = true;
        }

        internal bool MoveNext()
        {
            if (this.isFirst)
            {
                this.child = this.container.FirstChild;
                this.isFirst = false;
            }
            else if (this.child != null)
            {
                this.child = this.child.NextSibling;
            }
            return (this.child != null);
        }

        bool IEnumerator.MoveNext()
        {
            return this.MoveNext();
        }

        void IEnumerator.Reset()
        {
            this.isFirst = true;
            this.child = this.container.FirstChild;
        }

        internal XmlNode Current
        {
            get
            {
                if (this.isFirst || (this.child == null))
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                }
                return this.child;
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

