namespace System.Web
{
    using System;
    using System.Text;

    internal sealed class MultipartContentElement
    {
        private string _contentType;
        private HttpRawUploadedContent _data;
        private string _filename;
        private int _length;
        private string _name;
        private int _offset;

        internal MultipartContentElement(string name, string filename, string contentType, HttpRawUploadedContent data, int offset, int length)
        {
            this._name = name;
            this._filename = filename;
            this._contentType = contentType;
            this._data = data;
            this._offset = offset;
            this._length = length;
        }

        internal HttpPostedFile GetAsPostedFile()
        {
            return new HttpPostedFile(this._filename, this._contentType, new HttpInputStream(this._data, this._offset, this._length));
        }

        internal string GetAsString(Encoding encoding)
        {
            if (this._length > 0)
            {
                return encoding.GetString(this._data.GetAsByteArray(this._offset, this._length));
            }
            return string.Empty;
        }

        internal bool IsFile
        {
            get
            {
                return (this._filename != null);
            }
        }

        internal bool IsFormItem
        {
            get
            {
                return (this._filename == null);
            }
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

