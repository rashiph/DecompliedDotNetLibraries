namespace System.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal abstract class FormatterWithFileInfo : ErrorFormatter
    {
        protected int _line;
        protected string _physicalPath;
        protected string _sourceCode;
        protected string _virtualPath;
        private const int errorRange = 2;

        internal FormatterWithFileInfo(string virtualPath, string physicalPath, string sourceCode, int line)
        {
            this._virtualPath = virtualPath;
            this._physicalPath = physicalPath;
            if (((sourceCode == null) && (this._physicalPath == null)) && (this._virtualPath != null))
            {
                if (UrlPath.IsValidVirtualPathWithoutProtocol(this._virtualPath))
                {
                    this._physicalPath = HostingEnvironment.MapPath(this._virtualPath);
                }
                else
                {
                    this._physicalPath = this._virtualPath;
                }
            }
            this._sourceCode = sourceCode;
            this._line = line;
        }

        private string GetSourceFileLines()
        {
            return GetSourceFileLines(this._physicalPath, this.SourceFileEncoding, this._sourceCode, this._line);
        }

        internal static string GetSourceFileLines(string fileName, Encoding encoding, string sourceCode, int lineNumber)
        {
            if ((fileName != null) && !HttpRuntime.HasFilePermission(fileName))
            {
                return System.Web.SR.GetString("WithFile_No_Relevant_Line");
            }
            StringBuilder builder = new StringBuilder();
            if (lineNumber <= 0)
            {
                return System.Web.SR.GetString("WithFile_No_Relevant_Line");
            }
            TextReader reader = null;
            string virtualPathFromHttpLinePragma = ErrorFormatter.GetVirtualPathFromHttpLinePragma(fileName);
            if (virtualPathFromHttpLinePragma != null)
            {
                Stream stream = VirtualPathProvider.OpenFile(virtualPathFromHttpLinePragma);
                if (stream != null)
                {
                    reader = Util.ReaderFromStream(stream, System.Web.VirtualPath.Create(virtualPathFromHttpLinePragma));
                }
            }
            try
            {
                if ((reader == null) && (fileName != null))
                {
                    reader = new StreamReader(fileName, encoding, true, 0x1000);
                }
            }
            catch
            {
            }
            if (reader == null)
            {
                if (sourceCode == null)
                {
                    return System.Web.SR.GetString("WithFile_No_Relevant_Line");
                }
                reader = new StringReader(sourceCode);
            }
            try
            {
                string str2;
                bool flag = false;
                if (ErrorFormatter.IsTextRightToLeft)
                {
                    builder.Append("<div dir=\"ltr\">");
                }
                int num = 1;
            Label_0098:
                str2 = reader.ReadLine();
                if (str2 != null)
                {
                    if (num == lineNumber)
                    {
                        builder.Append("<font color=red>");
                    }
                    if ((num >= (lineNumber - 2)) && (num <= (lineNumber + 2)))
                    {
                        flag = true;
                        string str3 = num.ToString("G", CultureInfo.CurrentCulture);
                        builder.Append(System.Web.SR.GetString("WithFile_Line_Num", new object[] { str3 }));
                        if (str3.Length < 3)
                        {
                            builder.Append(' ', 3 - str3.Length);
                        }
                        builder.Append(HttpUtility.HtmlEncode(str2));
                        if (num != (lineNumber + 2))
                        {
                            builder.Append("\r\n");
                        }
                    }
                    if (num == lineNumber)
                    {
                        builder.Append("</font>");
                    }
                    if (num <= (lineNumber + 2))
                    {
                        num++;
                        goto Label_0098;
                    }
                }
                if (ErrorFormatter.IsTextRightToLeft)
                {
                    builder.Append("</div>");
                }
                if (!flag)
                {
                    return System.Web.SR.GetString("WithFile_No_Relevant_Line");
                }
            }
            finally
            {
                reader.Close();
            }
            return builder.ToString();
        }

        protected override string ColoredSquareContent
        {
            get
            {
                return this.GetSourceFileLines();
            }
        }

        protected override string PhysicalPath
        {
            get
            {
                return this._physicalPath;
            }
        }

        protected override bool ShowSourceFileInfo
        {
            get
            {
                return true;
            }
        }

        protected virtual Encoding SourceFileEncoding
        {
            get
            {
                return Encoding.Default;
            }
        }

        protected override int SourceFileLineNumber
        {
            get
            {
                return this._line;
            }
        }

        protected override string VirtualPath
        {
            get
            {
                return this._virtualPath;
            }
        }
    }
}

