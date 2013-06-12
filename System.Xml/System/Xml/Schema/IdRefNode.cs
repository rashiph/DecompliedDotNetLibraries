namespace System.Xml.Schema
{
    using System;

    internal class IdRefNode
    {
        internal string Id;
        internal int LineNo;
        internal int LinePos;
        internal IdRefNode Next;

        internal IdRefNode(IdRefNode next, string id, int lineNo, int linePos)
        {
            this.Id = id;
            this.LineNo = lineNo;
            this.LinePos = linePos;
            this.Next = next;
        }
    }
}

