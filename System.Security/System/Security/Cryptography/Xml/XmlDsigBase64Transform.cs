namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDsigBase64Transform : Transform
    {
        private CryptoStream _cs;
        private Type[] _inputTypes = new Type[] { typeof(Stream), typeof(XmlNodeList), typeof(XmlDocument) };
        private Type[] _outputTypes = new Type[] { typeof(Stream) };

        public XmlDsigBase64Transform()
        {
            base.Algorithm = "http://www.w3.org/2000/09/xmldsig#base64";
        }

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override object GetOutput()
        {
            return this._cs;
        }

        public override object GetOutput(Type type)
        {
            if ((type != typeof(Stream)) && !type.IsSubclassOf(typeof(Stream)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return this._cs;
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
        }

        public override void LoadInput(object obj)
        {
            if (obj is Stream)
            {
                this.LoadStreamInput((Stream) obj);
            }
            else if (obj is XmlNodeList)
            {
                this.LoadXmlNodeListInput((XmlNodeList) obj);
            }
            else if (obj is XmlDocument)
            {
                this.LoadXmlNodeListInput(((XmlDocument) obj).SelectNodes("//."));
            }
        }

        private void LoadStreamInput(Stream inputStream)
        {
            int num;
            if (inputStream == null)
            {
                throw new ArgumentException("obj");
            }
            MemoryStream stream = new MemoryStream();
            byte[] buffer = new byte[0x400];
            do
            {
                num = inputStream.Read(buffer, 0, 0x400);
                if (num > 0)
                {
                    int index = 0;
                    int num3 = 0;
                    while ((num3 < num) && !char.IsWhiteSpace((char) buffer[num3]))
                    {
                        num3++;
                    }
                    index = num3;
                    num3++;
                    while (num3 < num)
                    {
                        if (!char.IsWhiteSpace((char) buffer[num3]))
                        {
                            buffer[index] = buffer[num3];
                            index++;
                        }
                        num3++;
                    }
                    stream.Write(buffer, 0, index);
                }
            }
            while (num > 0);
            stream.Position = 0L;
            this._cs = new CryptoStream(stream, new FromBase64Transform(), CryptoStreamMode.Read);
        }

        private void LoadXmlNodeListInput(XmlNodeList nodeList)
        {
            StringBuilder builder = new StringBuilder();
            foreach (XmlNode node in nodeList)
            {
                XmlNode node2 = node.SelectSingleNode("self::text()");
                if (node2 != null)
                {
                    builder.Append(node2.OuterXml);
                }
            }
            byte[] bytes = new UTF8Encoding(false).GetBytes(builder.ToString());
            int index = 0;
            int num2 = 0;
            while ((num2 < bytes.Length) && !char.IsWhiteSpace((char) bytes[num2]))
            {
                num2++;
            }
            index = num2;
            num2++;
            while (num2 < bytes.Length)
            {
                if (!char.IsWhiteSpace((char) bytes[num2]))
                {
                    bytes[index] = bytes[num2];
                    index++;
                }
                num2++;
            }
            MemoryStream stream = new MemoryStream(bytes, 0, index);
            this._cs = new CryptoStream(stream, new FromBase64Transform(), CryptoStreamMode.Read);
        }

        public override Type[] InputTypes
        {
            get
            {
                return this._inputTypes;
            }
        }

        public override Type[] OutputTypes
        {
            get
            {
                return this._outputTypes;
            }
        }
    }
}

