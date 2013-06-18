namespace System.Xml.Linq
{
    using System;

    internal class LineInfoAnnotation
    {
        internal int lineNumber;
        internal int linePosition;

        public LineInfoAnnotation(int lineNumber, int linePosition)
        {
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }
    }
}

