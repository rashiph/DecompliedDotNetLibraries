namespace System.Xml.Linq
{
    using System;

    internal class LineInfoEndElementAnnotation : LineInfoAnnotation
    {
        public LineInfoEndElementAnnotation(int lineNumber, int linePosition) : base(lineNumber, linePosition)
        {
        }
    }
}

