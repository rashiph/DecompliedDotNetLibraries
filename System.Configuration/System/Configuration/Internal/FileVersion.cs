namespace System.Configuration.Internal
{
    using System;

    internal class FileVersion
    {
        private bool _exists;
        private long _fileSize;
        private DateTime _utcCreationTime;
        private DateTime _utcLastWriteTime;

        internal FileVersion(bool exists, long fileSize, DateTime utcCreationTime, DateTime utcLastWriteTime)
        {
            this._exists = exists;
            this._fileSize = fileSize;
            this._utcCreationTime = utcCreationTime;
            this._utcLastWriteTime = utcLastWriteTime;
        }

        public override bool Equals(object obj)
        {
            FileVersion version = obj as FileVersion;
            return ((((version != null) && (this._exists == version._exists)) && ((this._fileSize == version._fileSize) && (this._utcCreationTime == version._utcCreationTime))) && (this._utcLastWriteTime == version._utcLastWriteTime));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

