namespace System.Resources
{
    using System;
    using System.Drawing;

    internal class DataNodeInfo
    {
        internal string Comment;
        internal string MimeType;
        internal string Name;
        internal Point ReaderPosition;
        internal string TypeName;
        internal string ValueData;

        internal DataNodeInfo Clone()
        {
            return new DataNodeInfo { Name = this.Name, Comment = this.Comment, TypeName = this.TypeName, MimeType = this.MimeType, ValueData = this.ValueData, ReaderPosition = new Point(this.ReaderPosition.X, this.ReaderPosition.Y) };
        }
    }
}

