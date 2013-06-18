namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal class PropertySourceInfo
    {
        private string _fileName;
        private int _lineNumber;

        internal PropertySourceInfo(XmlReader reader)
        {
            this._fileName = this.GetFilename(reader);
            this._lineNumber = this.GetLineNumber(reader);
        }

        private string GetFilename(XmlReader reader)
        {
            IConfigErrorInfo info = reader as IConfigErrorInfo;
            if (info != null)
            {
                return info.Filename;
            }
            return "";
        }

        private int GetLineNumber(XmlReader reader)
        {
            IConfigErrorInfo info = reader as IConfigErrorInfo;
            if (info != null)
            {
                return info.LineNumber;
            }
            return 0;
        }

        internal string FileName
        {
            get
            {
                string path = this._fileName;
                try
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
                }
                catch (SecurityException)
                {
                    path = Path.GetFileName(this._fileName);
                    if (path == null)
                    {
                        path = string.Empty;
                    }
                }
                return path;
            }
        }

        internal int LineNumber
        {
            get
            {
                return this._lineNumber;
            }
        }
    }
}

