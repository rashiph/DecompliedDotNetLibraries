namespace System.Web.UI
{
    using System;
    using System.IO;
    using System.Text;

    public sealed class LosFormatter
    {
        private bool _enableMac;
        private ObjectStateFormatter _formatter;
        private const int InitialBufferSize = 0x18;

        public LosFormatter() : this(false, (byte[]) null)
        {
        }

        public LosFormatter(bool enableMac, string macKeyModifier) : this(enableMac, GetBytes(macKeyModifier))
        {
        }

        public LosFormatter(bool enableMac, byte[] macKeyModifier)
        {
            this._enableMac = enableMac;
            if (enableMac)
            {
                this._formatter = new ObjectStateFormatter(macKeyModifier);
            }
            else
            {
                this._formatter = new ObjectStateFormatter();
            }
        }

        public object Deserialize(Stream stream)
        {
            TextReader input = null;
            input = new StreamReader(stream);
            return this.Deserialize(input);
        }

        public object Deserialize(TextReader input)
        {
            char[] buffer = new char[0x80];
            int num = 0;
            int index = 0;
            int count = 0x18;
            do
            {
                num = input.Read(buffer, index, count);
                index += num;
                if (index > (buffer.Length - count))
                {
                    char[] destinationArray = new char[buffer.Length * 2];
                    Array.Copy(buffer, destinationArray, buffer.Length);
                    buffer = destinationArray;
                }
            }
            while (num == count);
            return this.Deserialize(new string(buffer, 0, index));
        }

        public object Deserialize(string input)
        {
            return this._formatter.Deserialize(input);
        }

        private static byte[] GetBytes(string s)
        {
            if ((s != null) && (s.Length != 0))
            {
                return Encoding.Unicode.GetBytes(s);
            }
            return null;
        }

        public void Serialize(Stream stream, object value)
        {
            TextWriter output = new StreamWriter(stream);
            this.SerializeInternal(output, value);
            output.Flush();
        }

        public void Serialize(TextWriter output, object value)
        {
            this.SerializeInternal(output, value);
        }

        private void SerializeInternal(TextWriter output, object value)
        {
            string str = this._formatter.Serialize(value);
            output.Write(str);
        }
    }
}

