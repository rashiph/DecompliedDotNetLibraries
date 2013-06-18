namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class StringResourceBuilder
    {
        private ArrayList _literalStrings;
        private int _offset;

        internal StringResourceBuilder()
        {
        }

        internal void AddString(string s, out int offset, out int size, out bool fAsciiOnly)
        {
            if (this._literalStrings == null)
            {
                this._literalStrings = new ArrayList();
            }
            this._literalStrings.Add(s);
            size = Encoding.UTF8.GetByteCount(s);
            fAsciiOnly = size == s.Length;
            offset = this._offset;
            this._offset += size;
        }

        internal void CreateResourceFile(string resFileName)
        {
            using (Stream stream = new FileStream(resFileName, FileMode.Create))
            {
                Encoding encoding = Encoding.UTF8;
                BinaryWriter writer = new BinaryWriter(stream, encoding);
                writer.Write(0);
                writer.Write(0x20);
                writer.Write(0xffff);
                writer.Write(0xffff);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(this._offset);
                writer.Write(0x20);
                writer.Write(0xebbffff);
                writer.Write(0x65ffff);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                foreach (string str in this._literalStrings)
                {
                    byte[] bytes = encoding.GetBytes(str);
                    writer.Write(bytes);
                }
            }
        }

        internal bool HasStrings
        {
            get
            {
                return (this._literalStrings != null);
            }
        }
    }
}

