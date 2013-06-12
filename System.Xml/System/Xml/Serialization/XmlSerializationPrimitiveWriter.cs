namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    internal class XmlSerializationPrimitiveWriter : XmlSerializationWriter
    {
        protected override void InitCallbacks()
        {
        }

        internal void Write_base64Binary(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("base64Binary", "");
            }
            else
            {
                base.TopLevelElement();
                base.WriteNullableStringLiteralRaw("base64Binary", "", XmlSerializationWriter.FromByteArrayBase64((byte[]) o));
            }
        }

        internal void Write_boolean(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("boolean", "");
            }
            else
            {
                base.WriteElementStringRaw("boolean", "", XmlConvert.ToString((bool) o));
            }
        }

        internal void Write_byte(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("byte", "");
            }
            else
            {
                base.WriteElementStringRaw("byte", "", XmlConvert.ToString((sbyte) o));
            }
        }

        internal void Write_char(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("char", "");
            }
            else
            {
                base.WriteElementString("char", "", XmlSerializationWriter.FromChar((char) o));
            }
        }

        internal void Write_dateTime(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("dateTime", "");
            }
            else
            {
                base.WriteElementStringRaw("dateTime", "", XmlSerializationWriter.FromDateTime((DateTime) o));
            }
        }

        internal void Write_decimal(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("decimal", "");
            }
            else
            {
                base.WriteElementStringRaw("decimal", "", XmlConvert.ToString((decimal) o));
            }
        }

        internal void Write_double(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("double", "");
            }
            else
            {
                base.WriteElementStringRaw("double", "", XmlConvert.ToString((double) o));
            }
        }

        internal void Write_float(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("float", "");
            }
            else
            {
                base.WriteElementStringRaw("float", "", XmlConvert.ToString((float) o));
            }
        }

        internal void Write_guid(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("guid", "");
            }
            else
            {
                base.WriteElementStringRaw("guid", "", XmlConvert.ToString((Guid) o));
            }
        }

        internal void Write_int(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("int", "");
            }
            else
            {
                base.WriteElementStringRaw("int", "", XmlConvert.ToString((int) o));
            }
        }

        internal void Write_long(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("long", "");
            }
            else
            {
                base.WriteElementStringRaw("long", "", XmlConvert.ToString((long) o));
            }
        }

        internal void Write_QName(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("QName", "");
            }
            else
            {
                base.TopLevelElement();
                base.WriteNullableQualifiedNameLiteral("QName", "", (XmlQualifiedName) o);
            }
        }

        internal void Write_short(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("short", "");
            }
            else
            {
                base.WriteElementStringRaw("short", "", XmlConvert.ToString((short) o));
            }
        }

        internal void Write_string(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("string", "");
            }
            else
            {
                base.TopLevelElement();
                base.WriteNullableStringLiteral("string", "", (string) o);
            }
        }

        internal void Write_unsignedByte(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("unsignedByte", "");
            }
            else
            {
                base.WriteElementStringRaw("unsignedByte", "", XmlConvert.ToString((byte) o));
            }
        }

        internal void Write_unsignedInt(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("unsignedInt", "");
            }
            else
            {
                base.WriteElementStringRaw("unsignedInt", "", XmlConvert.ToString((uint) o));
            }
        }

        internal void Write_unsignedLong(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("unsignedLong", "");
            }
            else
            {
                base.WriteElementStringRaw("unsignedLong", "", XmlConvert.ToString((ulong) o));
            }
        }

        internal void Write_unsignedShort(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("unsignedShort", "");
            }
            else
            {
                base.WriteElementStringRaw("unsignedShort", "", XmlConvert.ToString((ushort) o));
            }
        }
    }
}

