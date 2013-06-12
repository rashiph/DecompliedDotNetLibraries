namespace System.Xml
{
    using System;
    using System.Collections;

    internal class XmlNodeListEnumerator : IEnumerator
    {
        private int index;
        private XPathNodeList list;
        private bool valid;

        public XmlNodeListEnumerator(XPathNodeList list)
        {
            this.list = list;
            this.index = -1;
            this.valid = false;
        }

        public bool MoveNext()
        {
            this.index++;
            if ((this.list.ReadUntil(this.index + 1) - 1) < this.index)
            {
                return false;
            }
            this.valid = this.list[this.index] != null;
            return this.valid;
        }

        public void Reset()
        {
            this.index = -1;
        }

        public object Current
        {
            get
            {
                if (this.valid)
                {
                    return this.list[this.index];
                }
                return null;
            }
        }
    }
}

