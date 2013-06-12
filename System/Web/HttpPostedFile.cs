namespace System.Web
{
    using System;
    using System.IO;
    using System.Web.Configuration;

    public sealed class HttpPostedFile
    {
        private string _contentType;
        private string _filename;
        private HttpInputStream _stream;

        internal HttpPostedFile(string filename, string contentType, HttpInputStream stream)
        {
            this._filename = filename;
            this._contentType = contentType;
            this._stream = stream;
        }

        public void SaveAs(string filename)
        {
            if (!Path.IsPathRooted(filename) && RuntimeConfig.GetConfig().HttpRuntime.RequireRootedSaveAsPath)
            {
                throw new HttpException(System.Web.SR.GetString("SaveAs_requires_rooted_path", new object[] { filename }));
            }
            FileStream s = new FileStream(filename, FileMode.Create);
            try
            {
                this._stream.WriteTo(s);
                s.Flush();
            }
            finally
            {
                s.Close();
            }
        }

        public int ContentLength
        {
            get
            {
                return (int) this._stream.Length;
            }
        }

        public string ContentType
        {
            get
            {
                return this._contentType;
            }
        }

        public string FileName
        {
            get
            {
                return this._filename;
            }
        }

        public Stream InputStream
        {
            get
            {
                return this._stream;
            }
        }
    }
}

