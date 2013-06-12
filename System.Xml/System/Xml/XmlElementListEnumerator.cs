namespace System.Xml
{
    using System;
    using System.Collections;

    internal class XmlElementListEnumerator : IEnumerator
    {
        private int changeCount;
        private XmlNode curElem;
        private XmlElementList list;

        public XmlElementListEnumerator(XmlElementList list)
        {
            this.list = list;
            this.curElem = null;
            this.changeCount = list.ChangeCount;
        }

        public bool MoveNext()
        {
            if (this.list.ChangeCount != this.changeCount)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Enum_ElementList"));
            }
            this.curElem = this.list.GetNextNode(this.curElem);
            return (this.curElem != null);
        }

        public void Reset()
        {
            this.curElem = null;
            this.changeCount = this.list.ChangeCount;
        }

        public object Current
        {
            get
            {
                return this.curElem;
            }
        }
    }
}

