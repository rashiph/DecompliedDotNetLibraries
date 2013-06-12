namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    internal class XmlSerializationPrimitiveReader : XmlSerializationReader
    {
        private string id1_QName;
        private string id1_string;
        private string id10_dateTime;
        private string id11_unsignedByte;
        private string id12_byte;
        private string id13_unsignedShort;
        private string id14_unsignedInt;
        private string id15_unsignedLong;
        private string id16_base64Binary;
        private string id17_guid;
        private string id18_char;
        private string id2_Item;
        private string id3_int;
        private string id4_boolean;
        private string id5_short;
        private string id6_long;
        private string id7_float;
        private string id8_double;
        private string id9_decimal;

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id4_boolean = base.Reader.NameTable.Add("boolean");
            this.id14_unsignedInt = base.Reader.NameTable.Add("unsignedInt");
            this.id15_unsignedLong = base.Reader.NameTable.Add("unsignedLong");
            this.id7_float = base.Reader.NameTable.Add("float");
            this.id10_dateTime = base.Reader.NameTable.Add("dateTime");
            this.id6_long = base.Reader.NameTable.Add("long");
            this.id9_decimal = base.Reader.NameTable.Add("decimal");
            this.id8_double = base.Reader.NameTable.Add("double");
            this.id17_guid = base.Reader.NameTable.Add("guid");
            this.id2_Item = base.Reader.NameTable.Add("");
            this.id13_unsignedShort = base.Reader.NameTable.Add("unsignedShort");
            this.id18_char = base.Reader.NameTable.Add("char");
            this.id3_int = base.Reader.NameTable.Add("int");
            this.id12_byte = base.Reader.NameTable.Add("byte");
            this.id16_base64Binary = base.Reader.NameTable.Add("base64Binary");
            this.id11_unsignedByte = base.Reader.NameTable.Add("unsignedByte");
            this.id5_short = base.Reader.NameTable.Add("short");
            this.id1_string = base.Reader.NameTable.Add("string");
            this.id1_QName = base.Reader.NameTable.Add("QName");
        }

        internal object Read_base64Binary()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id16_base64Binary) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                if (base.ReadNull())
                {
                    return null;
                }
                return base.ToByteArrayBase64(false);
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_boolean()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id4_boolean) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToBoolean(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_byte()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id12_byte) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToSByte(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_char()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id18_char) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlSerializationReader.ToChar(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_dateTime()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id10_dateTime) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlSerializationReader.ToDateTime(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_decimal()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id9_decimal) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToDecimal(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_double()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id8_double) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToDouble(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_float()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id7_float) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToSingle(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_guid()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id17_guid) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToGuid(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_int()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id3_int) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToInt32(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_long()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id6_long) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToInt64(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_QName()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id1_QName) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                if (base.ReadNull())
                {
                    return null;
                }
                return base.ReadElementQualifiedName();
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_short()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id5_short) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToInt16(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_string()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id1_string) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                if (base.ReadNull())
                {
                    return null;
                }
                return base.Reader.ReadElementString();
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_unsignedByte()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id11_unsignedByte) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToByte(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_unsignedInt()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id14_unsignedInt) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToUInt32(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_unsignedLong()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id15_unsignedLong) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToUInt64(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }

        internal object Read_unsignedShort()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id13_unsignedShort) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return XmlConvert.ToUInt16(base.Reader.ReadElementString());
            }
            base.UnknownNode(null);
            return null;
        }
    }
}

