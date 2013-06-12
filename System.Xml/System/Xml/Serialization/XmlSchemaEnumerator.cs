namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Schema;

    public class XmlSchemaEnumerator : IEnumerator<XmlSchema>, IDisposable, IEnumerator
    {
        private int end;
        private int idx;
        private XmlSchemas list;

        public XmlSchemaEnumerator(XmlSchemas list)
        {
            this.list = list;
            this.idx = -1;
            this.end = list.Count - 1;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (this.idx >= this.end)
            {
                return false;
            }
            this.idx++;
            return true;
        }

        void IEnumerator.Reset()
        {
            this.idx = -1;
        }

        public XmlSchema Current
        {
            get
            {
                return this.list[this.idx];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.list[this.idx];
            }
        }
    }
}

