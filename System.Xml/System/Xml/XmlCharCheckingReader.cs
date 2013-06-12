namespace System.Xml
{
    using System;

    internal class XmlCharCheckingReader : XmlWrappingReader
    {
        private bool checkCharacters;
        private DtdProcessing dtdProcessing;
        private bool ignoreComments;
        private bool ignorePis;
        private bool ignoreWhitespace;
        private XmlNodeType lastNodeType;
        private ReadContentAsBinaryHelper readBinaryHelper;
        private State state;
        private XmlCharType xmlCharType;

        internal XmlCharCheckingReader(XmlReader reader, bool checkCharacters, bool ignoreWhitespace, bool ignoreComments, bool ignorePis, DtdProcessing dtdProcessing) : base(reader)
        {
            this.state = State.Initial;
            this.checkCharacters = checkCharacters;
            this.ignoreWhitespace = ignoreWhitespace;
            this.ignoreComments = ignoreComments;
            this.ignorePis = ignorePis;
            this.dtdProcessing = dtdProcessing;
            this.lastNodeType = XmlNodeType.None;
            if (checkCharacters)
            {
                this.xmlCharType = XmlCharType.Instance;
            }
        }

        private void CheckCharacters(string value)
        {
            XmlConvert.VerifyCharData(value, ExceptionType.ArgumentException, ExceptionType.XmlException);
        }

        private void CheckWhitespace(string value)
        {
            int invCharIndex = this.xmlCharType.IsOnlyWhitespaceWithPos(value);
            if (invCharIndex != -1)
            {
                this.Throw("Xml_InvalidWhitespaceCharacter", XmlException.BuildCharExceptionArgs(value, invCharIndex));
            }
        }

        private void FinishReadBinary()
        {
            this.state = State.Interactive;
            if (this.readBinaryHelper != null)
            {
                this.readBinaryHelper.Finish();
            }
        }

        public override void MoveToAttribute(int i)
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            base.reader.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            return base.reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            return base.reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            return base.reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            return base.reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            return base.reader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            XmlNodeType type;
            switch (this.state)
            {
                case State.Initial:
                    this.state = State.Interactive;
                    if (base.reader.ReadState != System.Xml.ReadState.Initial)
                    {
                        goto Label_0055;
                    }
                    break;

                case State.InReadBinary:
                    this.FinishReadBinary();
                    this.state = State.Interactive;
                    break;

                case State.Error:
                    return false;

                case State.Interactive:
                    break;

                default:
                    return false;
            }
            if (!base.reader.Read())
            {
                return false;
            }
        Label_0055:
            type = base.reader.NodeType;
            if (this.checkCharacters)
            {
                switch (type)
                {
                    case XmlNodeType.Element:
                        if (this.checkCharacters)
                        {
                            this.ValidateQName(base.reader.Prefix, base.reader.LocalName);
                            if (base.reader.MoveToFirstAttribute())
                            {
                                do
                                {
                                    this.ValidateQName(base.reader.Prefix, base.reader.LocalName);
                                    this.CheckCharacters(base.reader.Value);
                                }
                                while (base.reader.MoveToNextAttribute());
                                base.reader.MoveToElement();
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        if (this.checkCharacters)
                        {
                            this.CheckCharacters(base.reader.Value);
                        }
                        break;

                    case XmlNodeType.EntityReference:
                        if (this.checkCharacters)
                        {
                            this.ValidateQName(base.reader.Name);
                        }
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        if (!this.ignorePis)
                        {
                            if (this.checkCharacters)
                            {
                                this.ValidateQName(base.reader.Name);
                                this.CheckCharacters(base.reader.Value);
                            }
                            break;
                        }
                        return this.Read();

                    case XmlNodeType.Comment:
                        if (!this.ignoreComments)
                        {
                            if (this.checkCharacters)
                            {
                                this.CheckCharacters(base.reader.Value);
                            }
                            break;
                        }
                        return this.Read();

                    case XmlNodeType.DocumentType:
                        if (this.dtdProcessing != DtdProcessing.Prohibit)
                        {
                            if (this.dtdProcessing == DtdProcessing.Ignore)
                            {
                                return this.Read();
                            }
                        }
                        else
                        {
                            this.Throw("Xml_DtdIsProhibitedEx", string.Empty);
                        }
                        if (this.checkCharacters)
                        {
                            int num;
                            this.ValidateQName(base.reader.Name);
                            this.CheckCharacters(base.reader.Value);
                            string attribute = base.reader.GetAttribute("SYSTEM");
                            if (attribute != null)
                            {
                                this.CheckCharacters(attribute);
                            }
                            attribute = base.reader.GetAttribute("PUBLIC");
                            if ((attribute != null) && ((num = this.xmlCharType.IsPublicId(attribute)) >= 0))
                            {
                                this.Throw("Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(attribute, num));
                            }
                        }
                        break;

                    case XmlNodeType.Whitespace:
                        if (!this.ignoreWhitespace)
                        {
                            if (this.checkCharacters)
                            {
                                this.CheckWhitespace(base.reader.Value);
                            }
                            break;
                        }
                        return this.Read();

                    case XmlNodeType.SignificantWhitespace:
                        if (this.checkCharacters)
                        {
                            this.CheckWhitespace(base.reader.Value);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (this.checkCharacters)
                        {
                            this.ValidateQName(base.reader.Prefix, base.reader.LocalName);
                        }
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case XmlNodeType.ProcessingInstruction:
                        if (!this.ignorePis)
                        {
                            break;
                        }
                        return this.Read();

                    case XmlNodeType.Comment:
                        if (!this.ignoreComments)
                        {
                            break;
                        }
                        return this.Read();

                    case XmlNodeType.DocumentType:
                        if (this.dtdProcessing != DtdProcessing.Prohibit)
                        {
                            if (this.dtdProcessing == DtdProcessing.Ignore)
                            {
                                return this.Read();
                            }
                            break;
                        }
                        this.Throw("Xml_DtdIsProhibitedEx", string.Empty);
                        break;

                    case XmlNodeType.Whitespace:
                        if (!this.ignoreWhitespace)
                        {
                            break;
                        }
                        return this.Read();
                }
                return true;
            }
            this.lastNodeType = type;
            return true;
        }

        public override bool ReadAttributeValue()
        {
            if (this.state == State.InReadBinary)
            {
                this.FinishReadBinary();
            }
            return base.reader.ReadAttributeValue();
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                if (base.CanReadBinaryContent && !this.checkCharacters)
                {
                    this.readBinaryHelper = null;
                    this.state = State.InReadBinary;
                    return base.ReadContentAsBase64(buffer, index, count);
                }
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            else if (this.readBinaryHelper == null)
            {
                return base.ReadContentAsBase64(buffer, index, count);
            }
            this.state = State.Interactive;
            int num = this.readBinaryHelper.ReadContentAsBase64(buffer, index, count);
            this.state = State.InReadBinary;
            return num;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                if (base.CanReadBinaryContent && !this.checkCharacters)
                {
                    this.readBinaryHelper = null;
                    this.state = State.InReadBinary;
                    return base.ReadContentAsBinHex(buffer, index, count);
                }
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            else if (this.readBinaryHelper == null)
            {
                return base.ReadContentAsBinHex(buffer, index, count);
            }
            this.state = State.Interactive;
            int num = this.readBinaryHelper.ReadContentAsBinHex(buffer, index, count);
            this.state = State.InReadBinary;
            return num;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                if (base.CanReadBinaryContent && !this.checkCharacters)
                {
                    this.readBinaryHelper = null;
                    this.state = State.InReadBinary;
                    return base.ReadElementContentAsBase64(buffer, index, count);
                }
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            else if (this.readBinaryHelper == null)
            {
                return base.ReadElementContentAsBase64(buffer, index, count);
            }
            this.state = State.Interactive;
            int num = this.readBinaryHelper.ReadElementContentAsBase64(buffer, index, count);
            this.state = State.InReadBinary;
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                if (base.CanReadBinaryContent && !this.checkCharacters)
                {
                    this.readBinaryHelper = null;
                    this.state = State.InReadBinary;
                    return base.ReadElementContentAsBinHex(buffer, index, count);
                }
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            else if (this.readBinaryHelper == null)
            {
                return base.ReadElementContentAsBinHex(buffer, index, count);
            }
            this.state = State.Interactive;
            int num = this.readBinaryHelper.ReadElementContentAsBinHex(buffer, index, count);
            this.state = State.InReadBinary;
            return num;
        }

        private void Throw(string res, string arg)
        {
            this.state = State.Error;
            throw new XmlException(res, arg, null);
        }

        private void Throw(string res, string[] args)
        {
            this.state = State.Error;
            throw new XmlException(res, args, null);
        }

        private void ValidateQName(string name)
        {
            string str;
            string str2;
            ValidateNames.ParseQNameThrow(name, out str, out str2);
        }

        private void ValidateQName(string prefix, string localName)
        {
            try
            {
                if (prefix.Length > 0)
                {
                    ValidateNames.ParseNCNameThrow(prefix);
                }
                ValidateNames.ParseNCNameThrow(localName);
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return true;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                switch (this.state)
                {
                    case State.Initial:
                        if (base.reader.ReadState == System.Xml.ReadState.Closed)
                        {
                            return System.Xml.ReadState.Closed;
                        }
                        return System.Xml.ReadState.Initial;

                    case State.Error:
                        return System.Xml.ReadState.Error;
                }
                return base.reader.ReadState;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                XmlReaderSettings settings = base.reader.Settings;
                if (settings == null)
                {
                    settings = new XmlReaderSettings();
                }
                else
                {
                    settings = settings.Clone();
                }
                if (this.checkCharacters)
                {
                    settings.CheckCharacters = true;
                }
                if (this.ignoreWhitespace)
                {
                    settings.IgnoreWhitespace = true;
                }
                if (this.ignoreComments)
                {
                    settings.IgnoreComments = true;
                }
                if (this.ignorePis)
                {
                    settings.IgnoreProcessingInstructions = true;
                }
                if (this.dtdProcessing != ~DtdProcessing.Prohibit)
                {
                    settings.DtdProcessing = this.dtdProcessing;
                }
                settings.ReadOnly = true;
                return settings;
            }
        }

        private enum State
        {
            Initial,
            InReadBinary,
            Error,
            Interactive
        }
    }
}

