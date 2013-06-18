namespace System.Web
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;

    internal sealed class HttpMultipartContentTemplateParser
    {
        private byte[] _boundary;
        private HttpRawUploadedContent _data;
        private ArrayList _elements = new ArrayList();
        private Encoding _encoding;
        private bool _lastBoundaryFound;
        private int _length;
        private int _lineLength = -1;
        private int _lineStart = -1;
        private string _partContentType;
        private int _partDataLength = -1;
        private int _partDataStart = -1;
        private string _partFilename;
        private string _partName;
        private int _pos;

        private HttpMultipartContentTemplateParser(HttpRawUploadedContent data, int length, byte[] boundary, Encoding encoding)
        {
            this._data = data;
            this._length = length;
            this._boundary = boundary;
            this._encoding = encoding;
        }

        private bool AtBoundaryLine()
        {
            int length = this._boundary.Length;
            if ((this._lineLength != length) && (this._lineLength != (length + 2)))
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (this._data[this._lineStart + i] != this._boundary[i])
                {
                    return false;
                }
            }
            if (this._lineLength != length)
            {
                if ((this._data[this._lineStart + length] != 0x2d) || (this._data[(this._lineStart + length) + 1] != 0x2d))
                {
                    return false;
                }
                this._lastBoundaryFound = true;
            }
            return true;
        }

        private bool AtEndOfData()
        {
            if (this._pos < this._length)
            {
                return this._lastBoundaryFound;
            }
            return true;
        }

        private string ExtractValueFromContentDispositionHeader(string l, int pos, string name)
        {
            string str = name + "=\"";
            int startIndex = CultureInfo.InvariantCulture.CompareInfo.IndexOf(l, str, pos, CompareOptions.IgnoreCase);
            if (startIndex < 0)
            {
                return null;
            }
            startIndex += str.Length;
            int index = l.IndexOf('"', startIndex);
            if (index < 0)
            {
                return null;
            }
            if (index == startIndex)
            {
                return string.Empty;
            }
            return l.Substring(startIndex, index - startIndex);
        }

        private bool GetNextLine()
        {
            int num = this._pos;
            this._lineStart = -1;
            while (num < this._length)
            {
                if (this._data[num] == 10)
                {
                    this._lineStart = this._pos;
                    this._lineLength = num - this._pos;
                    this._pos = num + 1;
                    if ((this._lineLength > 0) && (this._data[num - 1] == 13))
                    {
                        this._lineLength--;
                    }
                    break;
                }
                if (++num == this._length)
                {
                    this._lineStart = this._pos;
                    this._lineLength = num - this._pos;
                    this._pos = this._length;
                }
            }
            return (this._lineStart >= 0);
        }

        internal static MultipartContentElement[] Parse(HttpRawUploadedContent data, int length, byte[] boundary, Encoding encoding)
        {
            HttpMultipartContentTemplateParser parser = new HttpMultipartContentTemplateParser(data, length, boundary, encoding);
            parser.ParseIntoElementList();
            return (MultipartContentElement[]) parser._elements.ToArray(typeof(MultipartContentElement));
        }

        private void ParseIntoElementList()
        {
            while (this.GetNextLine())
            {
                if (this.AtBoundaryLine())
                {
                    break;
                }
            }
            if (this.AtEndOfData())
            {
                return;
            }
        Label_001B:
            this.ParsePartHeaders();
            if (!this.AtEndOfData())
            {
                this.ParsePartData();
                if (this._partDataLength != -1)
                {
                    if (this._partName != null)
                    {
                        this._elements.Add(new MultipartContentElement(this._partName, this._partFilename, this._partContentType, this._data, this._partDataStart, this._partDataLength));
                    }
                    if (!this.AtEndOfData())
                    {
                        goto Label_001B;
                    }
                }
            }
        }

        private void ParsePartData()
        {
            this._partDataStart = this._pos;
            this._partDataLength = -1;
            while (this.GetNextLine())
            {
                if (this.AtBoundaryLine())
                {
                    int num = this._lineStart - 1;
                    if (this._data[num] == 10)
                    {
                        num--;
                    }
                    if (this._data[num] == 13)
                    {
                        num--;
                    }
                    this._partDataLength = (num - this._partDataStart) + 1;
                    return;
                }
            }
        }

        private void ParsePartHeaders()
        {
            this._partName = null;
            this._partFilename = null;
            this._partContentType = null;
            while (this.GetNextLine())
            {
                if (this._lineLength == 0)
                {
                    return;
                }
                byte[] buffer = new byte[this._lineLength];
                this._data.CopyBytes(this._lineStart, buffer, 0, this._lineLength);
                string l = this._encoding.GetString(buffer);
                int index = l.IndexOf(':');
                if (index >= 0)
                {
                    string str2 = l.Substring(0, index);
                    if (StringUtil.EqualsIgnoreCase(str2, "Content-Disposition"))
                    {
                        this._partName = this.ExtractValueFromContentDispositionHeader(l, index + 1, "name");
                        this._partFilename = this.ExtractValueFromContentDispositionHeader(l, index + 1, "filename");
                    }
                    else if (StringUtil.EqualsIgnoreCase(str2, "Content-Type"))
                    {
                        this._partContentType = l.Substring(index + 1).Trim();
                    }
                }
            }
        }
    }
}

