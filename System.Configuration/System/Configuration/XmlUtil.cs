namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal sealed class XmlUtil : IDisposable, IConfigErrorInfo
    {
        private StringWriter _cachedStringWriter;
        private int _lastLineNumber;
        private int _lastLinePosition;
        private XmlTextReader _reader;
        private ConfigurationSchemaErrors _schemaErrors;
        private Stream _stream;
        private string _streamName;
        private const int MAX_LINE_WIDTH = 60;
        private static readonly int[] s_positionOffset = new int[] { 
            0, 1, -1, 0, 9, 1, -1, 2, 4, -1, 10, -1, -1, 0, 0, 2, 
            -1, 2
         };

        internal XmlUtil(Stream stream, string name, bool readToFirstElement) : this(stream, name, readToFirstElement, new ConfigurationSchemaErrors())
        {
        }

        internal XmlUtil(Stream stream, string name, bool readToFirstElement, ConfigurationSchemaErrors schemaErrors)
        {
            try
            {
                this._streamName = name;
                this._stream = stream;
                this._reader = new XmlTextReader(this._stream);
                this._reader.XmlResolver = null;
                this._schemaErrors = schemaErrors;
                this._lastLineNumber = 1;
                this._lastLinePosition = 1;
                if (readToFirstElement)
                {
                    this._reader.WhitespaceHandling = WhitespaceHandling.None;
                    bool flag = false;
                    while (!flag && this._reader.Read())
                    {
                        switch (this._reader.NodeType)
                        {
                            case XmlNodeType.Comment:
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.XmlDeclaration:
                            {
                                continue;
                            }
                            case XmlNodeType.Element:
                            {
                                flag = true;
                                continue;
                            }
                        }
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_element"), this);
                    }
                }
            }
            catch
            {
                this.ReleaseResources();
                throw;
            }
        }

        internal void AddErrorRequiredAttribute(string attrib, ExceptionAction action)
        {
            ConfigurationErrorsException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_missing_required_attribute", new object[] { attrib, this._reader.Name }), this);
            this.SchemaErrors.AddError(ce, action);
        }

        internal void AddErrorReservedAttribute(ExceptionAction action)
        {
            ConfigurationErrorsException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_reserved_attribute", new object[] { this._reader.Name }), this);
            this.SchemaErrors.AddError(ce, action);
        }

        internal void AddErrorUnrecognizedAttribute(ExceptionAction action)
        {
            ConfigurationErrorsException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_attribute", new object[] { this._reader.Name }), this);
            this.SchemaErrors.AddError(ce, action);
        }

        internal void AddErrorUnrecognizedElement(ExceptionAction action)
        {
            ConfigurationErrorsException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_element"), this);
            this.SchemaErrors.AddError(ce, action);
        }

        private void CopyElement(XmlUtilWriter utilWriter)
        {
            int depth = this._reader.Depth;
            bool isEmptyElement = this._reader.IsEmptyElement;
            this.CopyXmlNode(utilWriter);
            while (this._reader.Depth > depth)
            {
                this.CopyXmlNode(utilWriter);
            }
            if (!isEmptyElement)
            {
                this.CopyXmlNode(utilWriter);
            }
        }

        internal bool CopyOuterXmlToNextElement(XmlUtilWriter utilWriter, bool limitDepth)
        {
            this.CopyElement(utilWriter);
            return this.CopyReaderToNextElement(utilWriter, limitDepth);
        }

        internal bool CopyReaderToNextElement(XmlUtilWriter utilWriter, bool limitDepth)
        {
            int depth;
            bool flag = true;
            if (limitDepth)
            {
                if (this._reader.NodeType == XmlNodeType.EndElement)
                {
                    return true;
                }
                depth = this._reader.Depth;
            }
            else
            {
                depth = 0;
            }
        Label_0026:
            if ((this._reader.NodeType != XmlNodeType.Element) && (this._reader.Depth >= depth))
            {
                flag = this.CopyXmlNode(utilWriter);
                if (flag)
                {
                    goto Label_0026;
                }
            }
            return flag;
        }

        internal string CopySection()
        {
            this.ResetCachedStringWriter();
            WhitespaceHandling whitespaceHandling = this._reader.WhitespaceHandling;
            this._reader.WhitespaceHandling = WhitespaceHandling.All;
            XmlUtilWriter utilWriter = new XmlUtilWriter(this._cachedStringWriter, false);
            this.CopyElement(utilWriter);
            this._reader.WhitespaceHandling = whitespaceHandling;
            if ((whitespaceHandling == WhitespaceHandling.None) && (this.Reader.NodeType == XmlNodeType.Whitespace))
            {
                this._reader.Read();
            }
            utilWriter.Flush();
            return ((StringWriter) utilWriter.Writer).ToString();
        }

        internal bool CopyXmlNode(XmlUtilWriter utilWriter)
        {
            string s = null;
            int fromLineNumber = -1;
            int fromLinePosition = -1;
            int lineNumber = 0;
            int linePosition = 0;
            int num5 = 0;
            int num6 = 0;
            if (utilWriter.TrackPosition)
            {
                lineNumber = this._reader.LineNumber;
                linePosition = this._reader.LinePosition;
                num5 = utilWriter.LineNumber;
                num6 = utilWriter.LinePosition;
            }
            XmlNodeType nodeType = this._reader.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Whitespace:
                    utilWriter.Write(this._reader.Value);
                    break;

                case XmlNodeType.Element:
                    s = this._reader.IsEmptyElement ? "/>" : ">";
                    fromLineNumber = this._reader.LineNumber;
                    fromLinePosition = this._reader.LinePosition + this._reader.Name.Length;
                    utilWriter.Write('<');
                    utilWriter.Write(this._reader.Name);
                    while (this._reader.MoveToNextAttribute())
                    {
                        int toLineNumber = this._reader.LineNumber;
                        int toLinePosition = this._reader.LinePosition;
                        utilWriter.AppendRequiredWhiteSpace(fromLineNumber, fromLinePosition, toLineNumber, toLinePosition);
                        int num9 = utilWriter.Write(this._reader.Name) + utilWriter.Write('=');
                        num9 += utilWriter.AppendAttributeValue(this._reader);
                        fromLineNumber = toLineNumber;
                        fromLinePosition = toLinePosition + num9;
                    }
                    break;

                case XmlNodeType.EndElement:
                    s = ">";
                    fromLineNumber = this._reader.LineNumber;
                    fromLinePosition = this._reader.LinePosition + this._reader.Name.Length;
                    utilWriter.Write("</");
                    utilWriter.Write(this._reader.Name);
                    break;

                case XmlNodeType.Comment:
                    utilWriter.AppendComment(this._reader.Value);
                    break;

                case XmlNodeType.Text:
                    utilWriter.AppendEscapeTextString(this._reader.Value);
                    break;

                case XmlNodeType.XmlDeclaration:
                    s = "?>";
                    fromLineNumber = this._reader.LineNumber;
                    fromLinePosition = this._reader.LinePosition + 3;
                    utilWriter.Write("<?xml");
                    while (this._reader.MoveToNextAttribute())
                    {
                        int num10 = this._reader.LineNumber;
                        int num11 = this._reader.LinePosition;
                        utilWriter.AppendRequiredWhiteSpace(fromLineNumber, fromLinePosition, num10, num11);
                        int num12 = utilWriter.Write(this._reader.Name) + utilWriter.Write('=');
                        num12 += utilWriter.AppendAttributeValue(this._reader);
                        fromLineNumber = num10;
                        fromLinePosition = num11 + num12;
                    }
                    this._reader.MoveToElement();
                    break;

                case XmlNodeType.SignificantWhitespace:
                    utilWriter.Write(this._reader.Value);
                    break;

                case XmlNodeType.ProcessingInstruction:
                    utilWriter.AppendProcessingInstruction(this._reader.Name, this._reader.Value);
                    break;

                case XmlNodeType.EntityReference:
                    utilWriter.AppendEntityRef(this._reader.Name);
                    break;

                case XmlNodeType.CDATA:
                    utilWriter.AppendCData(this._reader.Value);
                    break;

                default:
                    if (nodeType == XmlNodeType.DocumentType)
                    {
                        int num13 = utilWriter.Write("<!DOCTYPE");
                        utilWriter.AppendRequiredWhiteSpace(this._lastLineNumber, this._lastLinePosition + num13, this._reader.LineNumber, this._reader.LinePosition);
                        utilWriter.Write(this._reader.Name);
                        string str2 = null;
                        if (this._reader.HasValue)
                        {
                            str2 = this._reader.Value;
                        }
                        fromLineNumber = this._reader.LineNumber;
                        fromLinePosition = this._reader.LinePosition + this._reader.Name.Length;
                        if (this._reader.MoveToFirstAttribute())
                        {
                            utilWriter.AppendRequiredWhiteSpace(fromLineNumber, fromLinePosition, this._reader.LineNumber, this._reader.LinePosition);
                            string name = this._reader.Name;
                            utilWriter.Write(name);
                            utilWriter.AppendSpace();
                            utilWriter.AppendAttributeValue(this._reader);
                            this._reader.MoveToAttribute(0);
                            if (name == "PUBLIC")
                            {
                                this._reader.MoveToAttribute(1);
                                utilWriter.AppendSpace();
                                utilWriter.AppendAttributeValue(this._reader);
                                this._reader.MoveToAttribute(1);
                            }
                        }
                        if ((str2 != null) && (str2.Length > 0))
                        {
                            utilWriter.Write(" [");
                            utilWriter.Write(str2);
                            utilWriter.Write(']');
                        }
                        utilWriter.Write('>');
                    }
                    break;
            }
            bool flag = this._reader.Read();
            nodeType = this._reader.NodeType;
            if (s != null)
            {
                int positionOffset = GetPositionOffset(nodeType);
                int num15 = this._reader.LineNumber;
                int num16 = (this._reader.LinePosition - positionOffset) - s.Length;
                utilWriter.AppendWhiteSpace(fromLineNumber, fromLinePosition, num15, num16);
                utilWriter.Write(s);
            }
            if (utilWriter.TrackPosition)
            {
                this._lastLineNumber = (lineNumber - num5) + utilWriter.LineNumber;
                if (num5 == utilWriter.LineNumber)
                {
                    this._lastLinePosition = (linePosition - num6) + utilWriter.LinePosition;
                    return flag;
                }
                this._lastLinePosition = utilWriter.LinePosition;
            }
            return flag;
        }

        public void Dispose()
        {
            this.ReleaseResources();
        }

        internal static string FormatXmlElement(string xmlElement, int linePosition, int indent, bool skipFirstIndent)
        {
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.Default, Encoding.Unicode);
            XmlTextReader reader = new XmlTextReader(xmlElement, XmlNodeType.Element, context);
            StringWriter writer = new StringWriter(new StringBuilder(0x40), CultureInfo.InvariantCulture);
            XmlUtilWriter writer2 = new XmlUtilWriter(writer, false);
            bool newLine = false;
            bool flag2 = false;
            int num2 = 0;
            while (reader.Read())
            {
                int num;
                int attributeCount;
                int num4;
                bool flag3;
                XmlNodeType nodeType = reader.NodeType;
                if (flag2)
                {
                    writer2.Flush();
                    num = num2 - ((StringWriter) writer2.Writer).GetStringBuilder().Length;
                }
                else
                {
                    num = 0;
                }
                XmlNodeType type2 = nodeType;
                if (type2 <= XmlNodeType.CDATA)
                {
                    switch (type2)
                    {
                        case XmlNodeType.Element:
                        case XmlNodeType.CDATA:
                            goto Label_0091;
                    }
                    goto Label_00CA;
                }
                if ((type2 != XmlNodeType.Comment) && (type2 != XmlNodeType.EndElement))
                {
                    goto Label_00CA;
                }
            Label_0091:
                if (!skipFirstIndent && !flag2)
                {
                    writer2.AppendIndent(linePosition, indent, reader.Depth, newLine);
                    if (newLine)
                    {
                        writer2.Flush();
                        num2 = ((StringWriter) writer2.Writer).GetStringBuilder().Length;
                    }
                }
            Label_00CA:
                flag2 = false;
                switch (nodeType)
                {
                    case XmlNodeType.Element:
                        writer2.Write('<');
                        writer2.Write(reader.Name);
                        num += reader.Name.Length + 2;
                        attributeCount = reader.AttributeCount;
                        num4 = 0;
                        goto Label_0274;

                    case XmlNodeType.Text:
                        writer2.AppendEscapeTextString(reader.Value);
                        flag2 = true;
                        goto Label_02D6;

                    case XmlNodeType.CDATA:
                        writer2.AppendCData(reader.Value);
                        goto Label_02D6;

                    case XmlNodeType.EntityReference:
                        writer2.AppendEntityRef(reader.Name);
                        goto Label_02D6;

                    case XmlNodeType.ProcessingInstruction:
                        writer2.AppendProcessingInstruction(reader.Name, reader.Value);
                        goto Label_02D6;

                    case XmlNodeType.Comment:
                        writer2.AppendComment(reader.Value);
                        goto Label_02D6;

                    case XmlNodeType.SignificantWhitespace:
                        writer2.Write(reader.Value);
                        goto Label_02D6;

                    case XmlNodeType.EndElement:
                        writer2.Write("</");
                        writer2.Write(reader.Name);
                        writer2.Write('>');
                        goto Label_02D6;

                    default:
                        goto Label_02D6;
                }
            Label_01BE:
                if (num > 60)
                {
                    writer2.AppendIndent(linePosition, indent, reader.Depth - 1, true);
                    num = indent;
                    flag3 = false;
                    writer2.Flush();
                    num2 = ((StringWriter) writer2.Writer).GetStringBuilder().Length;
                }
                else
                {
                    flag3 = true;
                }
                reader.MoveToNextAttribute();
                writer2.Flush();
                int length = ((StringWriter) writer2.Writer).GetStringBuilder().Length;
                if (flag3)
                {
                    writer2.AppendSpace();
                }
                writer2.Write(reader.Name);
                writer2.Write('=');
                writer2.AppendAttributeValue(reader);
                writer2.Flush();
                num += ((StringWriter) writer2.Writer).GetStringBuilder().Length - length;
                num4++;
            Label_0274:
                if (num4 < attributeCount)
                {
                    goto Label_01BE;
                }
                reader.MoveToElement();
                if (reader.IsEmptyElement)
                {
                    writer2.Write(" />");
                }
                else
                {
                    writer2.Write('>');
                }
            Label_02D6:
                newLine = true;
                skipFirstIndent = false;
            }
            writer2.Flush();
            return ((StringWriter) writer2.Writer).ToString();
        }

        private static int GetPositionOffset(XmlNodeType nodeType)
        {
            return s_positionOffset[(int) nodeType];
        }

        internal void ReadToNextElement()
        {
            while (this._reader.Read())
            {
                if (this._reader.MoveToContent() == XmlNodeType.Element)
                {
                    return;
                }
            }
        }

        private void ReleaseResources()
        {
            if (this._reader != null)
            {
                this._reader.Close();
                this._reader = null;
            }
            else if (this._stream != null)
            {
                this._stream.Close();
            }
            this._stream = null;
            if (this._cachedStringWriter != null)
            {
                this._cachedStringWriter.Close();
                this._cachedStringWriter = null;
            }
        }

        private void ResetCachedStringWriter()
        {
            if (this._cachedStringWriter == null)
            {
                this._cachedStringWriter = new StringWriter(new StringBuilder(0x40), CultureInfo.InvariantCulture);
            }
            else
            {
                this._cachedStringWriter.GetStringBuilder().Length = 0;
            }
        }

        private string RetrieveFullOpenElementTag()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("<");
            builder.Append(this._reader.Name);
            while (this._reader.MoveToNextAttribute())
            {
                builder.Append(" ");
                builder.Append(this._reader.Name);
                builder.Append("=");
                builder.Append('"');
                builder.Append(this._reader.Value);
                builder.Append('"');
            }
            builder.Append(">");
            return builder.ToString();
        }

        internal bool SkipAndCopyReaderToNextElement(XmlUtilWriter utilWriter, bool limitDepth)
        {
            int depth;
            if (!utilWriter.IsLastLineBlank)
            {
                this._reader.Skip();
                return this.CopyReaderToNextElement(utilWriter, limitDepth);
            }
            if (limitDepth)
            {
                depth = this._reader.Depth;
            }
            else
            {
                depth = 0;
            }
            this._reader.Skip();
            int lineNumber = this._reader.LineNumber;
            while (!this._reader.EOF)
            {
                if (this._reader.NodeType != XmlNodeType.Whitespace)
                {
                    if (this._reader.LineNumber > lineNumber)
                    {
                        utilWriter.SeekToLineStart();
                        utilWriter.AppendWhiteSpace(lineNumber + 1, 1, this.LineNumber, this.TrueLinePosition);
                    }
                    break;
                }
                this._reader.Read();
            }
            while (!this._reader.EOF)
            {
                if ((this._reader.NodeType == XmlNodeType.Element) || (this._reader.Depth < depth))
                {
                    break;
                }
                this.CopyXmlNode(utilWriter);
            }
            return !this._reader.EOF;
        }

        internal bool SkipChildElementsAndCopyOuterXmlToNextElement(XmlUtilWriter utilWriter)
        {
            bool isEmptyElement = this._reader.IsEmptyElement;
            int lineNumber = this._reader.LineNumber;
            this.CopyXmlNode(utilWriter);
            if (!isEmptyElement)
            {
                while (this._reader.NodeType != XmlNodeType.EndElement)
                {
                    if (this._reader.NodeType == XmlNodeType.Element)
                    {
                        this._reader.Skip();
                        if (this._reader.NodeType == XmlNodeType.Whitespace)
                        {
                            this._reader.Skip();
                        }
                    }
                    else
                    {
                        this.CopyXmlNode(utilWriter);
                    }
                }
                if (this._reader.LineNumber != lineNumber)
                {
                    utilWriter.AppendSpacesToLinePosition(this.TrueLinePosition);
                }
                this.CopyXmlNode(utilWriter);
            }
            return this.CopyReaderToNextElement(utilWriter, true);
        }

        internal void SkipToNextElement()
        {
            this._reader.Skip();
            this._reader.MoveToContent();
            while (!this._reader.EOF && (this._reader.NodeType != XmlNodeType.Element))
            {
                this._reader.Read();
                this._reader.MoveToContent();
            }
        }

        internal void StrictReadToNextElement(ExceptionAction action)
        {
            while (this._reader.Read())
            {
                if (this._reader.NodeType == XmlNodeType.Element)
                {
                    return;
                }
                this.VerifyIgnorableNodeType(action);
            }
        }

        internal void StrictSkipToNextElement(ExceptionAction action)
        {
            this._reader.Skip();
            while (!this._reader.EOF && (this._reader.NodeType != XmlNodeType.Element))
            {
                this.VerifyIgnorableNodeType(action);
                this._reader.Read();
            }
        }

        internal void StrictSkipToOurParentsEndElement(ExceptionAction action)
        {
            int depth = this._reader.Depth;
            while (this._reader.Depth >= depth)
            {
                this._reader.Skip();
            }
            while (!this._reader.EOF && (this._reader.NodeType != XmlNodeType.EndElement))
            {
                this.VerifyIgnorableNodeType(action);
                this._reader.Read();
            }
        }

        internal string UpdateStartElement(XmlUtilWriter utilWriter, string updatedStartElement, bool needsChildren, int linePosition, int indent)
        {
            string str = null;
            string str6;
            bool flag = false;
            string name = this._reader.Name;
            if (this._reader.IsEmptyElement)
            {
                if ((updatedStartElement == null) && needsChildren)
                {
                    updatedStartElement = this.RetrieveFullOpenElementTag();
                }
                flag = updatedStartElement != null;
            }
            if (updatedStartElement == null)
            {
                this.CopyXmlNode(utilWriter);
                return str;
            }
            string str5 = FormatXmlElement(updatedStartElement + ("</" + name + ">"), linePosition, indent, true);
            int startIndex = str5.LastIndexOf('\n') + 1;
            if (flag)
            {
                str = str5.Substring(startIndex);
                str6 = str5.Substring(0, startIndex);
            }
            else
            {
                str6 = str5.Substring(0, startIndex - 2);
            }
            utilWriter.Write(str6);
            this._reader.Read();
            return str;
        }

        internal void VerifyAndGetBooleanAttribute(ExceptionAction action, bool defaultValue, out bool newValue)
        {
            if (this._reader.Value == "true")
            {
                newValue = true;
            }
            else if (this._reader.Value == "false")
            {
                newValue = false;
            }
            else
            {
                newValue = defaultValue;
                ConfigurationErrorsException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_invalid_boolean_attribute", new object[] { this._reader.Name }), this);
                this.SchemaErrors.AddError(ce, action);
            }
        }

        internal void VerifyAndGetNonEmptyStringAttribute(ExceptionAction action, out string newValue)
        {
            if (!string.IsNullOrEmpty(this._reader.Value))
            {
                newValue = this._reader.Value;
            }
            else
            {
                newValue = null;
                ConfigurationException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Empty_attribute", new object[] { this._reader.Name }), this);
                this.SchemaErrors.AddError(ce, action);
            }
        }

        internal void VerifyIgnorableNodeType(ExceptionAction action)
        {
            XmlNodeType nodeType = this._reader.NodeType;
            if ((nodeType != XmlNodeType.Comment) && (nodeType != XmlNodeType.EndElement))
            {
                ConfigurationException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_element"), this);
                this.SchemaErrors.AddError(ce, action);
            }
        }

        internal void VerifyNoUnrecognizedAttributes(ExceptionAction action)
        {
            if (this._reader.MoveToNextAttribute())
            {
                this.AddErrorUnrecognizedAttribute(action);
            }
        }

        internal bool VerifyRequiredAttribute(object o, string attrName, ExceptionAction action)
        {
            if (o == null)
            {
                this.AddErrorRequiredAttribute(attrName, action);
                return false;
            }
            return true;
        }

        public string Filename
        {
            get
            {
                return this._streamName;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.Reader.LineNumber;
            }
        }

        internal XmlTextReader Reader
        {
            get
            {
                return this._reader;
            }
        }

        internal ConfigurationSchemaErrors SchemaErrors
        {
            get
            {
                return this._schemaErrors;
            }
        }

        internal int TrueLinePosition
        {
            get
            {
                return (this.Reader.LinePosition - GetPositionOffset(this.Reader.NodeType));
            }
        }
    }
}

