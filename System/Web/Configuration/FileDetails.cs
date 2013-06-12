namespace System.Web.Configuration
{
    using System;
    using System.Web.Util;

    internal class FileDetails
    {
        private bool _exists;
        private long _fileSize;
        private DateTime _utcCreationTime;
        private DateTime _utcLastWriteTime;

        internal FileDetails(bool exists, long fileSize, DateTime utcCreationTime, DateTime utcLastWriteTime)
        {
            this._exists = exists;
            this._fileSize = fileSize;
            this._utcCreationTime = utcCreationTime;
            this._utcLastWriteTime = utcLastWriteTime;
        }

        public override bool Equals(object obj)
        {
            FileDetails details = obj as FileDetails;
            return ((((details != null) && (this._exists == details._exists)) && ((this._fileSize == details._fileSize) && (this._utcCreationTime == details._utcCreationTime))) && (this._utcLastWriteTime == details._utcLastWriteTime));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this._exists.GetHashCode(), this._fileSize.GetHashCode(), this._utcCreationTime.GetHashCode(), this._utcLastWriteTime.GetHashCode());
        }
    }
}

