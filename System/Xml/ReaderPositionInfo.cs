namespace System.Xml
{
    using System;

    internal class ReaderPositionInfo : PositionInfo
    {
        private IXmlLineInfo lineInfo;

        public ReaderPositionInfo(IXmlLineInfo lineInfo)
        {
            this.lineInfo = lineInfo;
        }

        public override bool HasLineInfo()
        {
            return this.lineInfo.HasLineInfo();
        }

        public override int LineNumber
        {
            get
            {
                return this.lineInfo.LineNumber;
            }
        }

        public override int LinePosition
        {
            get
            {
                return this.lineInfo.LinePosition;
            }
        }
    }
}

