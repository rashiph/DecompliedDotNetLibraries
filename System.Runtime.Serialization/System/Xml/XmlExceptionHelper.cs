namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    internal static class XmlExceptionHelper
    {
        public static XmlException CreateConversionException(string value, string type, Exception exception)
        {
            return new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidConversion", new object[] { value, type }), exception);
        }

        public static XmlException CreateEncodingException(string value, Exception exception)
        {
            return new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidUTF8Bytes", new object[] { value }), exception);
        }

        public static XmlException CreateEncodingException(byte[] buffer, int offset, int count, Exception exception)
        {
            return CreateEncodingException(new UTF8Encoding(false, false).GetString(buffer, offset, count), exception);
        }

        private static string GetName(string prefix, string localName)
        {
            if (prefix.Length == 0)
            {
                return localName;
            }
            return (prefix + ":" + localName);
        }

        private static string GetWhatWasFound(XmlDictionaryReader reader)
        {
            if (reader.EOF)
            {
                return System.Runtime.Serialization.SR.GetString("XmlFoundEndOfFile");
            }
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    return System.Runtime.Serialization.SR.GetString("XmlFoundElement", new object[] { GetName(reader.Prefix, reader.LocalName), reader.NamespaceURI });

                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return System.Runtime.Serialization.SR.GetString("XmlFoundText", new object[] { reader.Value });

                case XmlNodeType.CDATA:
                    return System.Runtime.Serialization.SR.GetString("XmlFoundCData", new object[] { reader.Value });

                case XmlNodeType.Comment:
                    return System.Runtime.Serialization.SR.GetString("XmlFoundComment", new object[] { reader.Value });

                case XmlNodeType.EndElement:
                    return System.Runtime.Serialization.SR.GetString("XmlFoundEndElement", new object[] { GetName(reader.Prefix, reader.LocalName), reader.NamespaceURI });
            }
            return System.Runtime.Serialization.SR.GetString("XmlFoundNodeType", new object[] { reader.NodeType });
        }

        public static void ThrowBase64DataExpected(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlBase64DataExpected", GetWhatWasFound(reader));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowConversionOverflow(XmlDictionaryReader reader, string value, string type)
        {
            ThrowXmlException(reader, "XmlConversionOverflow", value, type);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowDeclarationNotFirst(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlDeclNotFirst");
        }

        public static void ThrowDuplicateAttribute(XmlDictionaryReader reader, string prefix1, string prefix2, string localName, string ns)
        {
            ThrowXmlException(reader, "XmlDuplicateAttribute", GetName(prefix1, localName), GetName(prefix2, localName), ns);
        }

        public static void ThrowDuplicateXmlnsAttribute(XmlDictionaryReader reader, string localName, string ns)
        {
            string str;
            if (localName.Length == 0)
            {
                str = "xmlns";
            }
            else
            {
                str = "xmlns:" + localName;
            }
            ThrowXmlException(reader, "XmlDuplicateAttribute", str, str, ns);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowEmptyNamespace(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlEmptyNamespaceRequiresNullPrefix");
        }

        public static void ThrowEndElementExpected(XmlDictionaryReader reader, string localName, string ns)
        {
            ThrowXmlException(reader, "XmlEndElementExpected", localName, ns, GetWhatWasFound(reader));
        }

        public static void ThrowFullStartElementExpected(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlFullStartElementExpected", GetWhatWasFound(reader));
        }

        public static void ThrowFullStartElementExpected(XmlDictionaryReader reader, string name)
        {
            ThrowXmlException(reader, "XmlFullStartElementNameExpected", name, GetWhatWasFound(reader));
        }

        public static void ThrowFullStartElementExpected(XmlDictionaryReader reader, string localName, string ns)
        {
            ThrowXmlException(reader, "XmlFullStartElementLocalNameNsExpected", localName, ns, GetWhatWasFound(reader));
        }

        public static void ThrowFullStartElementExpected(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            ThrowFullStartElementExpected(reader, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(ns));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowInvalidBinaryFormat(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlInvalidFormat");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowInvalidCharRef(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlInvalidCharRef");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowInvalidRootData(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlInvalidRootData");
        }

        public static void ThrowInvalidXml(XmlDictionaryReader reader, byte b)
        {
            ThrowXmlException(reader, "XmlInvalidXmlByte", b.ToString("X2", CultureInfo.InvariantCulture));
        }

        public static void ThrowMaxArrayLengthExceeded(XmlDictionaryReader reader, int maxArrayLength)
        {
            ThrowXmlException(reader, "XmlMaxArrayLengthExceeded", maxArrayLength.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowMaxArrayLengthOrMaxItemsQuotaExceeded(XmlDictionaryReader reader, int maxQuota)
        {
            ThrowXmlException(reader, "XmlMaxArrayLengthOrMaxItemsQuotaExceeded", maxQuota.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowMaxBytesPerReadExceeded(XmlDictionaryReader reader, int maxBytesPerRead)
        {
            ThrowXmlException(reader, "XmlMaxBytesPerReadExceeded", maxBytesPerRead.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowMaxDepthExceeded(XmlDictionaryReader reader, int maxDepth)
        {
            ThrowXmlException(reader, "XmlMaxDepthExceeded", maxDepth.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowMaxNameTableCharCountExceeded(XmlDictionaryReader reader, int maxNameTableCharCount)
        {
            ThrowXmlException(reader, "XmlMaxNameTableCharCountExceeded", maxNameTableCharCount.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowMaxStringContentLengthExceeded(XmlDictionaryReader reader, int maxStringContentLength)
        {
            ThrowXmlException(reader, "XmlMaxStringContentLengthExceeded", maxStringContentLength.ToString(NumberFormatInfo.CurrentInfo));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowMultipleRootElements(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlMultipleRootElements");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowProcessingInstructionNotSupported(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlProcessingInstructionNotSupported");
        }

        public static void ThrowStartElementExpected(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlStartElementExpected", GetWhatWasFound(reader));
        }

        public static void ThrowStartElementExpected(XmlDictionaryReader reader, string name)
        {
            ThrowXmlException(reader, "XmlStartElementNameExpected", name, GetWhatWasFound(reader));
        }

        public static void ThrowStartElementExpected(XmlDictionaryReader reader, string localName, string ns)
        {
            ThrowXmlException(reader, "XmlStartElementLocalNameNsExpected", localName, ns, GetWhatWasFound(reader));
        }

        public static void ThrowStartElementExpected(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            ThrowStartElementExpected(reader, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(ns));
        }

        public static void ThrowTagMismatch(XmlDictionaryReader reader, string expectedPrefix, string expectedLocalName, string foundPrefix, string foundLocalName)
        {
            ThrowXmlException(reader, "XmlTagMismatch", GetName(expectedPrefix, expectedLocalName), GetName(foundPrefix, foundLocalName));
        }

        public static void ThrowTokenExpected(XmlDictionaryReader reader, string expected, char found)
        {
            ThrowXmlException(reader, "XmlTokenExpected", expected, found.ToString());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowTokenExpected(XmlDictionaryReader reader, string expected, string found)
        {
            ThrowXmlException(reader, "XmlTokenExpected", expected, found);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowUndefinedPrefix(XmlDictionaryReader reader, string prefix)
        {
            ThrowXmlException(reader, "XmlUndefinedPrefix", prefix);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowUnexpectedEndElement(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlUnexpectedEndElement");
        }

        public static void ThrowUnexpectedEndOfFile(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlUnexpectedEndOfFile", ((XmlBaseReader) reader).GetOpenElements());
        }

        public static void ThrowXmlDictionaryStringIDOutOfRange(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, "XmlDictionaryStringIDRange", 0.ToString(NumberFormatInfo.CurrentInfo), 0x1fffffff.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowXmlDictionaryStringIDUndefinedSession(XmlDictionaryReader reader, int key)
        {
            ThrowXmlException(reader, "XmlDictionaryStringIDUndefinedSession", key.ToString(NumberFormatInfo.CurrentInfo));
        }

        public static void ThrowXmlDictionaryStringIDUndefinedStatic(XmlDictionaryReader reader, int key)
        {
            ThrowXmlException(reader, "XmlDictionaryStringIDUndefinedStatic", key.ToString(NumberFormatInfo.CurrentInfo));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static void ThrowXmlException(XmlDictionaryReader reader, string res)
        {
            ThrowXmlException(reader, res, null);
        }

        public static void ThrowXmlException(XmlDictionaryReader reader, XmlException exception)
        {
            string message = exception.Message;
            IXmlLineInfo info = reader as IXmlLineInfo;
            if ((info != null) && info.HasLineInfo())
            {
                message = message + " " + System.Runtime.Serialization.SR.GetString("XmlLineInfo", new object[] { info.LineNumber, info.LinePosition });
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(message));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1)
        {
            ThrowXmlException(reader, res, arg1, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1, string arg2)
        {
            ThrowXmlException(reader, res, arg1, arg2, null);
        }

        private static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1, string arg2, string arg3)
        {
            string message = System.Runtime.Serialization.SR.GetString(res, new object[] { arg1, arg2, arg3 });
            IXmlLineInfo info = reader as IXmlLineInfo;
            if ((info != null) && info.HasLineInfo())
            {
                message = message + " " + System.Runtime.Serialization.SR.GetString("XmlLineInfo", new object[] { info.LineNumber, info.LinePosition });
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(message));
        }
    }
}

