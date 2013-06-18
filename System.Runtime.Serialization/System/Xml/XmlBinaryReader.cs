namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    internal class XmlBinaryReader : XmlBaseReader, IXmlBinaryReaderInitializer
    {
        private int arrayCount;
        private XmlBinaryNodeType arrayNodeType;
        private ArrayState arrayState;
        private bool buffered;
        private bool isTextWithEndElement;
        private int maxBytesPerRead;
        private OnXmlDictionaryReaderClose onClose;

        private bool CanOptimizeReadElementContent()
        {
            return ((this.arrayState == ArrayState.None) && !base.Signing);
        }

        private void CheckArray(Array array, int offset, int count)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > array.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { array.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (array.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { array.Length - offset })));
            }
        }

        public override void Close()
        {
            base.Close();
            OnXmlDictionaryReaderClose onClose = this.onClose;
            this.onClose = null;
            if (onClose != null)
            {
                try
                {
                    onClose(this);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        protected override XmlSigningNodeWriter CreateSigningNodeWriter()
        {
            return new XmlSigningNodeWriter(false);
        }

        private XmlBinaryNodeType GetNodeType()
        {
            return base.BufferReader.GetNodeType();
        }

        private void InsertNode(XmlBinaryNodeType nodeType, int length)
        {
            byte[] buffer = new byte[5];
            buffer[0] = (byte) nodeType;
            buffer[1] = (byte) length;
            length = length >> 8;
            buffer[2] = (byte) length;
            length = length >> 8;
            buffer[3] = (byte) length;
            length = length >> 8;
            buffer[4] = (byte) length;
            base.BufferReader.InsertBytes(buffer, 0, buffer.Length);
        }

        public override bool IsStartArray(out Type type)
        {
            type = null;
            if (this.arrayState == ArrayState.Element)
            {
                switch (this.arrayNodeType)
                {
                    case XmlBinaryNodeType.Int16TextWithEndElement:
                        type = typeof(short);
                        goto Label_0122;

                    case XmlBinaryNodeType.Int32TextWithEndElement:
                        type = typeof(int);
                        goto Label_0122;

                    case XmlBinaryNodeType.Int64TextWithEndElement:
                        type = typeof(long);
                        goto Label_0122;

                    case XmlBinaryNodeType.FloatTextWithEndElement:
                        type = typeof(float);
                        goto Label_0122;

                    case XmlBinaryNodeType.DoubleTextWithEndElement:
                        type = typeof(double);
                        goto Label_0122;

                    case XmlBinaryNodeType.DecimalTextWithEndElement:
                        type = typeof(decimal);
                        goto Label_0122;

                    case XmlBinaryNodeType.DateTimeTextWithEndElement:
                        type = typeof(DateTime);
                        goto Label_0122;

                    case XmlBinaryNodeType.UniqueIdTextWithEndElement:
                        type = typeof(UniqueId);
                        goto Label_0122;

                    case XmlBinaryNodeType.TimeSpanTextWithEndElement:
                        type = typeof(TimeSpan);
                        goto Label_0122;

                    case XmlBinaryNodeType.GuidTextWithEndElement:
                        type = typeof(Guid);
                        goto Label_0122;

                    case XmlBinaryNodeType.BoolTextWithEndElement:
                        type = typeof(bool);
                        goto Label_0122;
                }
            }
            return false;
        Label_0122:
            return true;
        }

        private bool IsStartArray(string localName, string namespaceUri, XmlBinaryNodeType nodeType)
        {
            return (((this.IsStartElement(localName, namespaceUri) && (this.arrayState == ArrayState.Element)) && (this.arrayNodeType == nodeType)) && !base.Signing);
        }

        private bool IsStartArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, XmlBinaryNodeType nodeType)
        {
            return (((this.IsStartElement(localName, namespaceUri) && (this.arrayState == ArrayState.Element)) && (this.arrayNodeType == nodeType)) && !base.Signing);
        }

        private bool IsValidArrayType(XmlBinaryNodeType nodeType)
        {
            switch (nodeType)
            {
                case XmlBinaryNodeType.Int16TextWithEndElement:
                case XmlBinaryNodeType.Int32TextWithEndElement:
                case XmlBinaryNodeType.Int64TextWithEndElement:
                case XmlBinaryNodeType.FloatTextWithEndElement:
                case XmlBinaryNodeType.DoubleTextWithEndElement:
                case XmlBinaryNodeType.DecimalTextWithEndElement:
                case XmlBinaryNodeType.DateTimeTextWithEndElement:
                case XmlBinaryNodeType.TimeSpanTextWithEndElement:
                case XmlBinaryNodeType.GuidTextWithEndElement:
                case XmlBinaryNodeType.BoolTextWithEndElement:
                    return true;
            }
            return false;
        }

        private void MoveToArrayElement()
        {
            this.arrayState = ArrayState.Element;
            base.MoveToNode(base.ElementNode);
        }

        private XmlBaseReader.XmlAtomicTextNode MoveToAtomicTextWithEndElement()
        {
            this.isTextWithEndElement = true;
            return base.MoveToAtomicText();
        }

        private void MoveToInitial(XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
        {
            base.MoveToInitial(quotas);
            this.maxBytesPerRead = quotas.MaxBytesPerRead;
            this.arrayState = ArrayState.None;
            this.onClose = onClose;
            this.isTextWithEndElement = false;
        }

        public override bool Read()
        {
            if (base.Node.ReadState == System.Xml.ReadState.Closed)
            {
                return false;
            }
            base.SignNode();
            if (this.isTextWithEndElement)
            {
                this.isTextWithEndElement = false;
                base.MoveToEndElement();
                return true;
            }
            if (this.arrayState == ArrayState.Content)
            {
                if (this.arrayCount != 0)
                {
                    this.MoveToArrayElement();
                    return true;
                }
                this.arrayState = ArrayState.None;
            }
            if (base.Node.ExitScope)
            {
                base.ExitScope();
            }
            return this.ReadNode();
        }

        private void ReadArray()
        {
            if (this.GetNodeType() == XmlBinaryNodeType.Array)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            this.ReadNode();
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            if (this.GetNodeType() == XmlBinaryNodeType.Array)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            this.ReadNode();
            if (base.Node.NodeType != XmlNodeType.EndElement)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            this.arrayState = ArrayState.Element;
            this.arrayNodeType = this.GetNodeType();
            if (!this.IsValidArrayType(this.arrayNodeType))
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            this.SkipNodeType();
            this.arrayCount = this.ReadMultiByteUInt31();
            if (this.arrayCount == 0)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            this.MoveToArrayElement();
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(bool[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (bool* flagRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) flagRef, (byte*) (flagRef + num));
            }
            this.SkipArrayElements(num);
            return num;
        }

        private int ReadArray(DateTime[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            for (int i = 0; i < num; i++)
            {
                array[offset + i] = base.BufferReader.ReadDateTime();
            }
            this.SkipArrayElements(num);
            return num;
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(decimal[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (decimal* numRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) numRef, (byte*) (numRef + (num * 0x10)));
            }
            this.SkipArrayElements(num);
            return num;
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(double[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (double* numRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) numRef, (byte*) (numRef + (num * 8)));
            }
            this.SkipArrayElements(num);
            return num;
        }

        private int ReadArray(Guid[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            for (int i = 0; i < num; i++)
            {
                array[offset + i] = base.BufferReader.ReadGuid();
            }
            this.SkipArrayElements(num);
            return num;
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(short[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (short* numRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) numRef, (byte*) (numRef + num));
            }
            this.SkipArrayElements(num);
            return num;
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(int[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (int* numRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) numRef, (byte*) (numRef + num));
            }
            this.SkipArrayElements(num);
            return num;
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(long[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (long* numRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) numRef, (byte*) (numRef + num));
            }
            this.SkipArrayElements(num);
            return num;
        }

        [SecuritySafeCritical]
        private unsafe int ReadArray(float[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            fixed (float* numRef = &(array[offset]))
            {
                base.BufferReader.UnsafeReadArray((byte*) numRef, (byte*) (numRef + (num * 4)));
            }
            this.SkipArrayElements(num);
            return num;
        }

        private int ReadArray(TimeSpan[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = Math.Min(count, this.arrayCount);
            for (int i = 0; i < num; i++)
            {
                array[offset + i] = base.BufferReader.ReadTimeSpan();
            }
            this.SkipArrayElements(num);
            return num;
        }

        public override int ReadArray(string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.BoolTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.DateTimeTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.DecimalTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, double[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.DoubleTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.GuidTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, short[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.Int16TextWithEndElement) && BitConverter.IsLittleEndian)
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, int[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.Int32TextWithEndElement) && BitConverter.IsLittleEndian)
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, long[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.Int64TextWithEndElement) && BitConverter.IsLittleEndian)
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, float[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.FloatTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.TimeSpanTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.BoolTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.DateTimeTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.DecimalTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.DoubleTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.GuidTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.Int16TextWithEndElement) && BitConverter.IsLittleEndian)
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.Int32TextWithEndElement) && BitConverter.IsLittleEndian)
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.Int64TextWithEndElement) && BitConverter.IsLittleEndian)
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.FloatTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        public override int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            if (this.IsStartArray(localName, namespaceUri, XmlBinaryNodeType.TimeSpanTextWithEndElement))
            {
                return this.ReadArray(array, offset, count);
            }
            return base.ReadArray(localName, namespaceUri, array, offset, count);
        }

        private void ReadAttributes()
        {
            XmlBinaryNodeType nodeType = this.GetNodeType();
            if ((nodeType >= XmlBinaryNodeType.MinAttribute) && (nodeType <= XmlBinaryNodeType.PrefixAttributeZ))
            {
                this.ReadAttributes2();
            }
        }

        private void ReadAttributes2()
        {
            XmlBaseReader.XmlAttributeNode node;
            XmlBaseReader.Namespace namespace2;
            PrefixHandleType alphaPrefix;
            XmlBinaryNodeType type2;
            int offset = 0;
            if (this.buffered)
            {
                offset = base.BufferReader.Offset;
            }
        Label_0016:
            type2 = this.GetNodeType();
            switch (type2)
            {
                case XmlBinaryNodeType.MinAttribute:
                    this.SkipNodeType();
                    node = base.AddAttribute();
                    node.Prefix.SetValue(PrefixHandleType.Empty);
                    this.ReadName(node.LocalName);
                    this.ReadAttributeText(node.AttributeText);
                    goto Label_0016;

                case XmlBinaryNodeType.Attribute:
                    this.SkipNodeType();
                    node = base.AddAttribute();
                    this.ReadName(node.Prefix);
                    this.ReadName(node.LocalName);
                    this.ReadAttributeText(node.AttributeText);
                    base.FixXmlAttribute(node);
                    goto Label_0016;

                case XmlBinaryNodeType.ShortDictionaryAttribute:
                    this.SkipNodeType();
                    node = base.AddAttribute();
                    node.Prefix.SetValue(PrefixHandleType.Empty);
                    this.ReadDictionaryName(node.LocalName);
                    this.ReadAttributeText(node.AttributeText);
                    goto Label_0016;

                case XmlBinaryNodeType.DictionaryAttribute:
                    this.SkipNodeType();
                    node = base.AddAttribute();
                    this.ReadName(node.Prefix);
                    this.ReadDictionaryName(node.LocalName);
                    this.ReadAttributeText(node.AttributeText);
                    goto Label_0016;

                case XmlBinaryNodeType.ShortXmlnsAttribute:
                    this.SkipNodeType();
                    namespace2 = base.AddNamespace();
                    namespace2.Prefix.SetValue(PrefixHandleType.Empty);
                    this.ReadName(namespace2.Uri);
                    node = base.AddXmlnsAttribute(namespace2);
                    goto Label_0016;

                case XmlBinaryNodeType.XmlnsAttribute:
                    this.SkipNodeType();
                    namespace2 = base.AddNamespace();
                    this.ReadName(namespace2.Prefix);
                    this.ReadName(namespace2.Uri);
                    node = base.AddXmlnsAttribute(namespace2);
                    goto Label_0016;

                case XmlBinaryNodeType.ShortDictionaryXmlnsAttribute:
                    this.SkipNodeType();
                    namespace2 = base.AddNamespace();
                    namespace2.Prefix.SetValue(PrefixHandleType.Empty);
                    this.ReadDictionaryName(namespace2.Uri);
                    node = base.AddXmlnsAttribute(namespace2);
                    goto Label_0016;

                case XmlBinaryNodeType.DictionaryXmlnsAttribute:
                    this.SkipNodeType();
                    namespace2 = base.AddNamespace();
                    this.ReadName(namespace2.Prefix);
                    this.ReadDictionaryName(namespace2.Uri);
                    node = base.AddXmlnsAttribute(namespace2);
                    goto Label_0016;

                case XmlBinaryNodeType.PrefixDictionaryAttributeA:
                case XmlBinaryNodeType.PrefixDictionaryAttributeB:
                case XmlBinaryNodeType.PrefixDictionaryAttributeC:
                case XmlBinaryNodeType.PrefixDictionaryAttributeD:
                case XmlBinaryNodeType.PrefixDictionaryAttributeE:
                case XmlBinaryNodeType.PrefixDictionaryAttributeF:
                case XmlBinaryNodeType.PrefixDictionaryAttributeG:
                case XmlBinaryNodeType.PrefixDictionaryAttributeH:
                case XmlBinaryNodeType.PrefixDictionaryAttributeI:
                case XmlBinaryNodeType.PrefixDictionaryAttributeJ:
                case XmlBinaryNodeType.PrefixDictionaryAttributeK:
                case XmlBinaryNodeType.PrefixDictionaryAttributeL:
                case XmlBinaryNodeType.PrefixDictionaryAttributeM:
                case XmlBinaryNodeType.PrefixDictionaryAttributeN:
                case XmlBinaryNodeType.PrefixDictionaryAttributeO:
                case XmlBinaryNodeType.PrefixDictionaryAttributeP:
                case XmlBinaryNodeType.PrefixDictionaryAttributeQ:
                case XmlBinaryNodeType.PrefixDictionaryAttributeR:
                case XmlBinaryNodeType.PrefixDictionaryAttributeS:
                case XmlBinaryNodeType.PrefixDictionaryAttributeT:
                case XmlBinaryNodeType.PrefixDictionaryAttributeU:
                case XmlBinaryNodeType.PrefixDictionaryAttributeV:
                case XmlBinaryNodeType.PrefixDictionaryAttributeW:
                case XmlBinaryNodeType.PrefixDictionaryAttributeX:
                case XmlBinaryNodeType.PrefixDictionaryAttributeY:
                case XmlBinaryNodeType.PrefixDictionaryAttributeZ:
                    this.SkipNodeType();
                    node = base.AddAttribute();
                    alphaPrefix = PrefixHandle.GetAlphaPrefix(((int) type2) - 12);
                    node.Prefix.SetValue(alphaPrefix);
                    this.ReadDictionaryName(node.LocalName);
                    this.ReadAttributeText(node.AttributeText);
                    goto Label_0016;

                case XmlBinaryNodeType.PrefixAttributeA:
                case XmlBinaryNodeType.PrefixAttributeB:
                case XmlBinaryNodeType.PrefixAttributeC:
                case XmlBinaryNodeType.PrefixAttributeD:
                case XmlBinaryNodeType.PrefixAttributeE:
                case XmlBinaryNodeType.PrefixAttributeF:
                case XmlBinaryNodeType.PrefixAttributeG:
                case XmlBinaryNodeType.PrefixAttributeH:
                case XmlBinaryNodeType.PrefixAttributeI:
                case XmlBinaryNodeType.PrefixAttributeJ:
                case XmlBinaryNodeType.PrefixAttributeK:
                case XmlBinaryNodeType.PrefixAttributeL:
                case XmlBinaryNodeType.PrefixAttributeM:
                case XmlBinaryNodeType.PrefixAttributeN:
                case XmlBinaryNodeType.PrefixAttributeO:
                case XmlBinaryNodeType.PrefixAttributeP:
                case XmlBinaryNodeType.PrefixAttributeQ:
                case XmlBinaryNodeType.PrefixAttributeR:
                case XmlBinaryNodeType.PrefixAttributeS:
                case XmlBinaryNodeType.PrefixAttributeT:
                case XmlBinaryNodeType.PrefixAttributeU:
                case XmlBinaryNodeType.PrefixAttributeV:
                case XmlBinaryNodeType.PrefixAttributeW:
                case XmlBinaryNodeType.PrefixAttributeX:
                case XmlBinaryNodeType.PrefixAttributeY:
                case XmlBinaryNodeType.PrefixAttributeZ:
                    this.SkipNodeType();
                    node = base.AddAttribute();
                    alphaPrefix = PrefixHandle.GetAlphaPrefix(((int) type2) - 0x26);
                    node.Prefix.SetValue(alphaPrefix);
                    this.ReadName(node.LocalName);
                    this.ReadAttributeText(node.AttributeText);
                    goto Label_0016;
            }
            if (this.buffered && ((base.BufferReader.Offset - offset) > this.maxBytesPerRead))
            {
                XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this, this.maxBytesPerRead);
            }
            base.ProcessAttributes();
        }

        private void ReadAttributeText(XmlBaseReader.XmlAttributeTextNode textNode)
        {
            XmlBinaryNodeType nodeType = this.GetNodeType();
            this.SkipNodeType();
            base.BufferReader.ReadValue(nodeType, textNode.Value);
        }

        private void ReadBinaryText(XmlBaseReader.XmlTextNode textNode, int length)
        {
            this.ReadText(textNode, ValueHandleType.Base64, length);
        }

        private int ReadDictionaryKey()
        {
            return base.BufferReader.ReadDictionaryKey();
        }

        private void ReadDictionaryName(StringHandle s)
        {
            int key = this.ReadDictionaryKey();
            s.SetValue(key);
        }

        public override bool ReadElementContentAsBoolean()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent())
            {
                bool flag;
                switch (this.GetNodeType())
                {
                    case XmlBinaryNodeType.FalseTextWithEndElement:
                        this.SkipNodeType();
                        flag = false;
                        this.ReadTextWithEndElement();
                        return flag;

                    case XmlBinaryNodeType.TrueTextWithEndElement:
                        this.SkipNodeType();
                        flag = true;
                        this.ReadTextWithEndElement();
                        return flag;

                    case XmlBinaryNodeType.BoolTextWithEndElement:
                        this.SkipNodeType();
                        flag = base.BufferReader.ReadUInt8() != 0;
                        this.ReadTextWithEndElement();
                        return flag;
                }
            }
            return base.ReadElementContentAsBoolean();
        }

        public override DateTime ReadElementContentAsDateTime()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.DateTimeTextWithEndElement))
            {
                this.SkipNodeType();
                DateTime time = base.BufferReader.ReadDateTime();
                this.ReadTextWithEndElement();
                return time;
            }
            return base.ReadElementContentAsDateTime();
        }

        public override decimal ReadElementContentAsDecimal()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.DecimalTextWithEndElement))
            {
                this.SkipNodeType();
                decimal num = base.BufferReader.ReadDecimal();
                this.ReadTextWithEndElement();
                return num;
            }
            return base.ReadElementContentAsDecimal();
        }

        public override double ReadElementContentAsDouble()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.DoubleTextWithEndElement))
            {
                this.SkipNodeType();
                double num = base.BufferReader.ReadDouble();
                this.ReadTextWithEndElement();
                return num;
            }
            return base.ReadElementContentAsDouble();
        }

        public override float ReadElementContentAsFloat()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.FloatTextWithEndElement))
            {
                this.SkipNodeType();
                float num = base.BufferReader.ReadSingle();
                this.ReadTextWithEndElement();
                return num;
            }
            return base.ReadElementContentAsFloat();
        }

        public override Guid ReadElementContentAsGuid()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.GuidTextWithEndElement))
            {
                this.SkipNodeType();
                Guid guid = base.BufferReader.ReadGuid();
                this.ReadTextWithEndElement();
                return guid;
            }
            return base.ReadElementContentAsGuid();
        }

        public override int ReadElementContentAsInt()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent())
            {
                int num;
                switch (this.GetNodeType())
                {
                    case XmlBinaryNodeType.ZeroTextWithEndElement:
                        this.SkipNodeType();
                        num = 0;
                        this.ReadTextWithEndElement();
                        return num;

                    case XmlBinaryNodeType.OneTextWithEndElement:
                        this.SkipNodeType();
                        num = 1;
                        this.ReadTextWithEndElement();
                        return num;

                    case XmlBinaryNodeType.Int8TextWithEndElement:
                        this.SkipNodeType();
                        num = base.BufferReader.ReadInt8();
                        this.ReadTextWithEndElement();
                        return num;

                    case XmlBinaryNodeType.Int16TextWithEndElement:
                        this.SkipNodeType();
                        num = base.BufferReader.ReadInt16();
                        this.ReadTextWithEndElement();
                        return num;

                    case XmlBinaryNodeType.Int32TextWithEndElement:
                        this.SkipNodeType();
                        num = base.BufferReader.ReadInt32();
                        this.ReadTextWithEndElement();
                        return num;
                }
            }
            return base.ReadElementContentAsInt();
        }

        public override string ReadElementContentAsString()
        {
            string str;
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (!this.CanOptimizeReadElementContent())
            {
                return base.ReadElementContentAsString();
            }
            XmlBinaryNodeType nodeType = this.GetNodeType();
            if (nodeType == XmlBinaryNodeType.Chars8TextWithEndElement)
            {
                this.SkipNodeType();
                str = base.BufferReader.ReadUTF8String(this.ReadUInt8());
                this.ReadTextWithEndElement();
            }
            else if (nodeType == XmlBinaryNodeType.DictionaryTextWithEndElement)
            {
                this.SkipNodeType();
                str = base.BufferReader.GetDictionaryString(this.ReadDictionaryKey()).Value;
                this.ReadTextWithEndElement();
            }
            else
            {
                str = base.ReadElementContentAsString();
            }
            if (str.Length > this.Quotas.MaxStringContentLength)
            {
                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, this.Quotas.MaxStringContentLength);
            }
            return str;
        }

        public override TimeSpan ReadElementContentAsTimeSpan()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.TimeSpanTextWithEndElement))
            {
                this.SkipNodeType();
                TimeSpan span = base.BufferReader.ReadTimeSpan();
                this.ReadTextWithEndElement();
                return span;
            }
            return base.ReadElementContentAsTimeSpan();
        }

        public override UniqueId ReadElementContentAsUniqueId()
        {
            if (base.Node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.CanOptimizeReadElementContent() && (this.GetNodeType() == XmlBinaryNodeType.UniqueIdTextWithEndElement))
            {
                this.SkipNodeType();
                UniqueId id = base.BufferReader.ReadUniqueId();
                this.ReadTextWithEndElement();
                return id;
            }
            return base.ReadElementContentAsUniqueId();
        }

        private int ReadMultiByteUInt31()
        {
            return base.BufferReader.ReadMultiByteUInt31();
        }

        private void ReadName(PrefixHandle prefix)
        {
            int count = this.ReadMultiByteUInt31();
            int offset = base.BufferReader.ReadBytes(count);
            prefix.SetValue(offset, count);
        }

        private void ReadName(StringHandle handle)
        {
            int count = this.ReadMultiByteUInt31();
            int offset = base.BufferReader.ReadBytes(count);
            handle.SetValue(offset, count);
        }

        private void ReadName(ValueHandle value)
        {
            int count = this.ReadMultiByteUInt31();
            int offset = base.BufferReader.ReadBytes(count);
            value.SetValue(ValueHandleType.UTF8, offset, count);
        }

        private bool ReadNode()
        {
            XmlBinaryNodeType nodeType;
            XmlBaseReader.XmlElementNode node;
            PrefixHandleType alphaPrefix;
            if (!this.buffered)
            {
                base.BufferReader.SetWindow(base.ElementNode.BufferOffset, this.maxBytesPerRead);
            }
            if (base.BufferReader.EndOfFile)
            {
                base.MoveToEndOfFile();
                return false;
            }
            if (this.arrayState == ArrayState.None)
            {
                nodeType = this.GetNodeType();
                this.SkipNodeType();
            }
            else
            {
                nodeType = this.arrayNodeType;
                this.arrayCount--;
                this.arrayState = ArrayState.Content;
            }
            switch (nodeType)
            {
                case XmlBinaryNodeType.EndElement:
                    base.MoveToEndElement();
                    return true;

                case XmlBinaryNodeType.Comment:
                    this.ReadName(base.MoveToComment().Value);
                    return true;

                case XmlBinaryNodeType.Array:
                    this.ReadArray();
                    return true;

                case XmlBinaryNodeType.MinElement:
                    node = base.EnterScope();
                    node.Prefix.SetValue(PrefixHandleType.Empty);
                    this.ReadName(node.LocalName);
                    this.ReadAttributes();
                    node.Namespace = base.LookupNamespace(PrefixHandleType.Empty);
                    node.BufferOffset = base.BufferReader.Offset;
                    return true;

                case XmlBinaryNodeType.Element:
                    node = base.EnterScope();
                    this.ReadName(node.Prefix);
                    this.ReadName(node.LocalName);
                    this.ReadAttributes();
                    node.Namespace = base.LookupNamespace(node.Prefix);
                    node.BufferOffset = base.BufferReader.Offset;
                    return true;

                case XmlBinaryNodeType.ShortDictionaryElement:
                    node = base.EnterScope();
                    node.Prefix.SetValue(PrefixHandleType.Empty);
                    this.ReadDictionaryName(node.LocalName);
                    this.ReadAttributes();
                    node.Namespace = base.LookupNamespace(PrefixHandleType.Empty);
                    node.BufferOffset = base.BufferReader.Offset;
                    return true;

                case XmlBinaryNodeType.DictionaryElement:
                    node = base.EnterScope();
                    this.ReadName(node.Prefix);
                    this.ReadDictionaryName(node.LocalName);
                    this.ReadAttributes();
                    node.Namespace = base.LookupNamespace(node.Prefix);
                    node.BufferOffset = base.BufferReader.Offset;
                    return true;

                case XmlBinaryNodeType.PrefixDictionaryElementA:
                case XmlBinaryNodeType.PrefixDictionaryElementB:
                case XmlBinaryNodeType.PrefixDictionaryElementC:
                case XmlBinaryNodeType.PrefixDictionaryElementD:
                case XmlBinaryNodeType.PrefixDictionaryElementE:
                case XmlBinaryNodeType.PrefixDictionaryElementF:
                case XmlBinaryNodeType.PrefixDictionaryElementG:
                case XmlBinaryNodeType.PrefixDictionaryElementH:
                case XmlBinaryNodeType.PrefixDictionaryElementI:
                case XmlBinaryNodeType.PrefixDictionaryElementJ:
                case XmlBinaryNodeType.PrefixDictionaryElementK:
                case XmlBinaryNodeType.PrefixDictionaryElementL:
                case XmlBinaryNodeType.PrefixDictionaryElementM:
                case XmlBinaryNodeType.PrefixDictionaryElementN:
                case XmlBinaryNodeType.PrefixDictionaryElementO:
                case XmlBinaryNodeType.PrefixDictionaryElementP:
                case XmlBinaryNodeType.PrefixDictionaryElementQ:
                case XmlBinaryNodeType.PrefixDictionaryElementR:
                case XmlBinaryNodeType.PrefixDictionaryElementS:
                case XmlBinaryNodeType.PrefixDictionaryElementT:
                case XmlBinaryNodeType.PrefixDictionaryElementU:
                case XmlBinaryNodeType.PrefixDictionaryElementV:
                case XmlBinaryNodeType.PrefixDictionaryElementW:
                case XmlBinaryNodeType.PrefixDictionaryElementX:
                case XmlBinaryNodeType.PrefixDictionaryElementY:
                case XmlBinaryNodeType.PrefixDictionaryElementZ:
                    node = base.EnterScope();
                    alphaPrefix = PrefixHandle.GetAlphaPrefix(((int) nodeType) - 0x44);
                    node.Prefix.SetValue(alphaPrefix);
                    this.ReadDictionaryName(node.LocalName);
                    this.ReadAttributes();
                    node.Namespace = base.LookupNamespace(alphaPrefix);
                    node.BufferOffset = base.BufferReader.Offset;
                    return true;

                case XmlBinaryNodeType.PrefixElementA:
                case XmlBinaryNodeType.PrefixElementB:
                case XmlBinaryNodeType.PrefixElementC:
                case XmlBinaryNodeType.PrefixElementD:
                case XmlBinaryNodeType.PrefixElementE:
                case XmlBinaryNodeType.PrefixElementF:
                case XmlBinaryNodeType.PrefixElementG:
                case XmlBinaryNodeType.PrefixElementH:
                case XmlBinaryNodeType.PrefixElementI:
                case XmlBinaryNodeType.PrefixElementJ:
                case XmlBinaryNodeType.PrefixElementK:
                case XmlBinaryNodeType.PrefixElementL:
                case XmlBinaryNodeType.PrefixElementM:
                case XmlBinaryNodeType.PrefixElementN:
                case XmlBinaryNodeType.PrefixElementO:
                case XmlBinaryNodeType.PrefixElementP:
                case XmlBinaryNodeType.PrefixElementQ:
                case XmlBinaryNodeType.PrefixElementR:
                case XmlBinaryNodeType.PrefixElementS:
                case XmlBinaryNodeType.PrefixElementT:
                case XmlBinaryNodeType.PrefixElementU:
                case XmlBinaryNodeType.PrefixElementV:
                case XmlBinaryNodeType.PrefixElementW:
                case XmlBinaryNodeType.PrefixElementX:
                case XmlBinaryNodeType.PrefixElementY:
                case XmlBinaryNodeType.PrefixElementZ:
                    node = base.EnterScope();
                    alphaPrefix = PrefixHandle.GetAlphaPrefix(((int) nodeType) - 0x5e);
                    node.Prefix.SetValue(alphaPrefix);
                    this.ReadName(node.LocalName);
                    this.ReadAttributes();
                    node.Namespace = base.LookupNamespace(alphaPrefix);
                    node.BufferOffset = base.BufferReader.Offset;
                    return true;

                case XmlBinaryNodeType.ZeroTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetValue(ValueHandleType.Zero);
                    if (base.OutsideRootElement)
                    {
                        this.VerifyWhitespace();
                    }
                    return true;

                case XmlBinaryNodeType.OneTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetValue(ValueHandleType.One);
                    if (base.OutsideRootElement)
                    {
                        this.VerifyWhitespace();
                    }
                    return true;

                case XmlBinaryNodeType.FalseTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetValue(ValueHandleType.False);
                    if (base.OutsideRootElement)
                    {
                        this.VerifyWhitespace();
                    }
                    return true;

                case XmlBinaryNodeType.TrueTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetValue(ValueHandleType.True);
                    if (base.OutsideRootElement)
                    {
                        this.VerifyWhitespace();
                    }
                    return true;

                case XmlBinaryNodeType.Int8TextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Int8, 1);
                    return true;

                case XmlBinaryNodeType.Int16TextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Int16, 2);
                    return true;

                case XmlBinaryNodeType.Int32TextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Int32, 4);
                    return true;

                case XmlBinaryNodeType.Int64TextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Int64, 8);
                    return true;

                case XmlBinaryNodeType.FloatTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Single, 4);
                    return true;

                case XmlBinaryNodeType.DoubleTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Double, 8);
                    return true;

                case XmlBinaryNodeType.DecimalTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Decimal, 0x10);
                    return true;

                case XmlBinaryNodeType.DateTimeTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.DateTime, 8);
                    return true;

                case XmlBinaryNodeType.Chars8Text:
                    if (!this.buffered)
                    {
                        this.ReadPartialUTF8Text(false, this.ReadUInt8());
                    }
                    else
                    {
                        this.ReadText(base.MoveToComplexText(), ValueHandleType.UTF8, this.ReadUInt8());
                    }
                    return true;

                case XmlBinaryNodeType.Chars8TextWithEndElement:
                    if (!this.buffered)
                    {
                        this.ReadPartialUTF8Text(true, this.ReadUInt8());
                        break;
                    }
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.UTF8, this.ReadUInt8());
                    break;

                case XmlBinaryNodeType.Chars16Text:
                    if (!this.buffered)
                    {
                        this.ReadPartialUTF8Text(false, this.ReadUInt16());
                    }
                    else
                    {
                        this.ReadText(base.MoveToComplexText(), ValueHandleType.UTF8, this.ReadUInt16());
                    }
                    return true;

                case XmlBinaryNodeType.Chars16TextWithEndElement:
                    if (!this.buffered)
                    {
                        this.ReadPartialUTF8Text(true, this.ReadUInt16());
                    }
                    else
                    {
                        this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.UTF8, this.ReadUInt16());
                    }
                    return true;

                case XmlBinaryNodeType.Chars32Text:
                    if (!this.buffered)
                    {
                        this.ReadPartialUTF8Text(false, this.ReadUInt31());
                    }
                    else
                    {
                        this.ReadText(base.MoveToComplexText(), ValueHandleType.UTF8, this.ReadUInt31());
                    }
                    return true;

                case XmlBinaryNodeType.Chars32TextWithEndElement:
                    if (!this.buffered)
                    {
                        this.ReadPartialUTF8Text(true, this.ReadUInt31());
                    }
                    else
                    {
                        this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.UTF8, this.ReadUInt31());
                    }
                    return true;

                case XmlBinaryNodeType.Bytes8Text:
                    if (!this.buffered)
                    {
                        this.ReadPartialBinaryText(false, this.ReadUInt8());
                    }
                    else
                    {
                        this.ReadBinaryText(base.MoveToComplexText(), this.ReadUInt8());
                    }
                    return true;

                case XmlBinaryNodeType.Bytes8TextWithEndElement:
                    if (!this.buffered)
                    {
                        this.ReadPartialBinaryText(true, this.ReadUInt8());
                    }
                    else
                    {
                        this.ReadBinaryText(this.MoveToAtomicTextWithEndElement(), this.ReadUInt8());
                    }
                    return true;

                case XmlBinaryNodeType.Bytes16Text:
                    if (!this.buffered)
                    {
                        this.ReadPartialBinaryText(false, this.ReadUInt16());
                    }
                    else
                    {
                        this.ReadBinaryText(base.MoveToComplexText(), this.ReadUInt16());
                    }
                    return true;

                case XmlBinaryNodeType.Bytes16TextWithEndElement:
                    if (!this.buffered)
                    {
                        this.ReadPartialBinaryText(true, this.ReadUInt16());
                    }
                    else
                    {
                        this.ReadBinaryText(this.MoveToAtomicTextWithEndElement(), this.ReadUInt16());
                    }
                    return true;

                case XmlBinaryNodeType.Bytes32Text:
                    if (!this.buffered)
                    {
                        this.ReadPartialBinaryText(false, this.ReadUInt31());
                    }
                    else
                    {
                        this.ReadBinaryText(base.MoveToComplexText(), this.ReadUInt31());
                    }
                    return true;

                case XmlBinaryNodeType.Bytes32TextWithEndElement:
                    if (!this.buffered)
                    {
                        this.ReadPartialBinaryText(true, this.ReadUInt31());
                    }
                    else
                    {
                        this.ReadBinaryText(this.MoveToAtomicTextWithEndElement(), this.ReadUInt31());
                    }
                    return true;

                case XmlBinaryNodeType.EmptyTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetValue(ValueHandleType.Empty);
                    if (base.OutsideRootElement)
                    {
                        this.VerifyWhitespace();
                    }
                    return true;

                case XmlBinaryNodeType.DictionaryTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetDictionaryValue(this.ReadDictionaryKey());
                    return true;

                case XmlBinaryNodeType.UniqueIdTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.UniqueId, 0x10);
                    return true;

                case XmlBinaryNodeType.TimeSpanTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.TimeSpan, 8);
                    return true;

                case XmlBinaryNodeType.GuidTextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Guid, 0x10);
                    return true;

                case XmlBinaryNodeType.UInt64TextWithEndElement:
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.UInt64, 8);
                    return true;

                case XmlBinaryNodeType.BoolTextWithEndElement:
                    this.MoveToAtomicTextWithEndElement().Value.SetValue((this.ReadUInt8() != 0) ? ValueHandleType.True : ValueHandleType.False);
                    if (base.OutsideRootElement)
                    {
                        this.VerifyWhitespace();
                    }
                    return true;

                case XmlBinaryNodeType.UnicodeChars8Text:
                    this.ReadUnicodeText(false, this.ReadUInt8());
                    return true;

                case XmlBinaryNodeType.UnicodeChars8TextWithEndElement:
                    this.ReadUnicodeText(true, this.ReadUInt8());
                    return true;

                case XmlBinaryNodeType.UnicodeChars16Text:
                    this.ReadUnicodeText(false, this.ReadUInt16());
                    return true;

                case XmlBinaryNodeType.UnicodeChars16TextWithEndElement:
                    this.ReadUnicodeText(true, this.ReadUInt16());
                    return true;

                case XmlBinaryNodeType.UnicodeChars32Text:
                    this.ReadUnicodeText(false, this.ReadUInt31());
                    return true;

                case XmlBinaryNodeType.UnicodeChars32TextWithEndElement:
                    this.ReadUnicodeText(true, this.ReadUInt31());
                    return true;

                case XmlBinaryNodeType.QNameDictionaryTextWithEndElement:
                    base.BufferReader.ReadQName(this.MoveToAtomicTextWithEndElement().Value);
                    return true;

                default:
                    base.BufferReader.ReadValue(nodeType, base.MoveToComplexText().Value);
                    return true;
            }
            return true;
        }

        private void ReadPartialBinaryText(bool withEndElement, int length)
        {
            int num = Math.Max(this.maxBytesPerRead - 5, 0);
            if (length <= num)
            {
                if (withEndElement)
                {
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Base64, length);
                }
                else
                {
                    this.ReadText(base.MoveToComplexText(), ValueHandleType.Base64, length);
                }
            }
            else
            {
                int num2 = num;
                if (num2 > 3)
                {
                    num2 -= num2 % 3;
                }
                this.ReadText(base.MoveToComplexText(), ValueHandleType.Base64, num2);
                XmlBinaryNodeType nodeType = withEndElement ? XmlBinaryNodeType.Bytes32TextWithEndElement : XmlBinaryNodeType.Bytes32Text;
                this.InsertNode(nodeType, length - num2);
            }
        }

        private void ReadPartialUnicodeText(bool withEndElement, int length)
        {
            int num = Math.Max(this.maxBytesPerRead - 5, 0);
            if (length <= num)
            {
                if (withEndElement)
                {
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Unicode, length);
                }
                else
                {
                    this.ReadText(base.MoveToComplexText(), ValueHandleType.Unicode, length);
                }
            }
            else
            {
                int count = Math.Max(num - 5, 0);
                if ((count & 1) != 0)
                {
                    count--;
                }
                int offset = base.BufferReader.ReadBytes(count);
                int num4 = 0;
                char ch = (char) base.BufferReader.GetInt16((offset + count) - 2);
                if ((ch >= 0xd800) && (ch < 0xdc00))
                {
                    num4 = 2;
                }
                base.BufferReader.Offset -= num4;
                count -= num4;
                base.MoveToComplexText().Value.SetValue(ValueHandleType.Unicode, offset, count);
                if (base.OutsideRootElement)
                {
                    this.VerifyWhitespace();
                }
                XmlBinaryNodeType nodeType = withEndElement ? XmlBinaryNodeType.UnicodeChars32TextWithEndElement : XmlBinaryNodeType.UnicodeChars32Text;
                this.InsertNode(nodeType, length - count);
            }
        }

        private void ReadPartialUTF8Text(bool withEndElement, int length)
        {
            int num = Math.Max(this.maxBytesPerRead - 5, 0);
            if (length <= num)
            {
                if (withEndElement)
                {
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.UTF8, length);
                }
                else
                {
                    this.ReadText(base.MoveToComplexText(), ValueHandleType.UTF8, length);
                }
            }
            else
            {
                int count = Math.Max(num - 5, 0);
                int offset = base.BufferReader.ReadBytes(count);
                int num4 = (offset + count) - 1;
                while (num4 >= offset)
                {
                    byte @byte = base.BufferReader.GetByte(num4);
                    if (((@byte & 0x80) == 0) || ((@byte & 0xc0) == 0xc0))
                    {
                        break;
                    }
                    num4--;
                }
                int num6 = (offset + count) - num4;
                base.BufferReader.Offset -= num6;
                count -= num6;
                base.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, offset, count);
                if (base.OutsideRootElement)
                {
                    this.VerifyWhitespace();
                }
                XmlBinaryNodeType nodeType = withEndElement ? XmlBinaryNodeType.Chars32TextWithEndElement : XmlBinaryNodeType.Chars32Text;
                this.InsertNode(nodeType, length - count);
            }
        }

        private void ReadText(XmlBaseReader.XmlTextNode textNode, ValueHandleType type, int length)
        {
            int offset = base.BufferReader.ReadBytes(length);
            textNode.Value.SetValue(type, offset, length);
            if (base.OutsideRootElement)
            {
                this.VerifyWhitespace();
            }
        }

        private void ReadTextWithEndElement()
        {
            base.ExitScope();
            this.ReadNode();
        }

        private int ReadUInt16()
        {
            return base.BufferReader.ReadUInt16();
        }

        private int ReadUInt31()
        {
            return base.BufferReader.ReadUInt31();
        }

        private int ReadUInt8()
        {
            return base.BufferReader.ReadUInt8();
        }

        private void ReadUnicodeText(bool withEndElement, int length)
        {
            if ((length & 1) != 0)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            if (this.buffered)
            {
                if (withEndElement)
                {
                    this.ReadText(this.MoveToAtomicTextWithEndElement(), ValueHandleType.Unicode, length);
                }
                else
                {
                    this.ReadText(base.MoveToComplexText(), ValueHandleType.Unicode, length);
                }
            }
            else
            {
                this.ReadPartialUnicodeText(withEndElement, length);
            }
        }

        public void SetInput(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.MoveToInitial(quotas, session, onClose);
            base.BufferReader.SetBuffer(stream, dictionary, session);
            this.buffered = false;
        }

        public void SetInput(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
            this.MoveToInitial(quotas, session, onClose);
            base.BufferReader.SetBuffer(buffer, offset, count, dictionary, session);
            this.buffered = true;
        }

        private void SkipArrayElements(int count)
        {
            this.arrayCount -= count;
            if (this.arrayCount == 0)
            {
                this.arrayState = ArrayState.None;
                base.ExitScope();
                this.ReadNode();
            }
        }

        private void SkipNodeType()
        {
            base.BufferReader.SkipNodeType();
        }

        public override bool TryGetArrayLength(out int count)
        {
            count = 0;
            if (!this.buffered)
            {
                return false;
            }
            if (this.arrayState != ArrayState.Element)
            {
                return false;
            }
            count = this.arrayCount;
            return true;
        }

        public override bool TryGetBase64ContentLength(out int length)
        {
            int num;
            bool flag2;
            length = 0;
            if (!this.buffered)
            {
                return false;
            }
            if (this.arrayState != ArrayState.None)
            {
                return false;
            }
            if (!base.Node.Value.TryGetByteArrayLength(out num))
            {
                return false;
            }
            int offset = base.BufferReader.Offset;
            try
            {
                bool flag = false;
                while (!flag && !base.BufferReader.EndOfFile)
                {
                    int num3;
                    XmlBinaryNodeType nodeType = this.GetNodeType();
                    this.SkipNodeType();
                    switch (nodeType)
                    {
                        case XmlBinaryNodeType.Bytes8Text:
                            num3 = base.BufferReader.ReadUInt8();
                            break;

                        case XmlBinaryNodeType.Bytes8TextWithEndElement:
                            num3 = base.BufferReader.ReadUInt8();
                            flag = true;
                            break;

                        case XmlBinaryNodeType.Bytes16Text:
                            num3 = base.BufferReader.ReadUInt16();
                            break;

                        case XmlBinaryNodeType.Bytes16TextWithEndElement:
                            num3 = base.BufferReader.ReadUInt16();
                            flag = true;
                            break;

                        case XmlBinaryNodeType.Bytes32Text:
                            num3 = base.BufferReader.ReadUInt31();
                            break;

                        case XmlBinaryNodeType.Bytes32TextWithEndElement:
                            num3 = base.BufferReader.ReadUInt31();
                            flag = true;
                            break;

                        case XmlBinaryNodeType.EndElement:
                            num3 = 0;
                            flag = true;
                            break;

                        default:
                            return false;
                    }
                    base.BufferReader.Advance(num3);
                    if (num > (0x7fffffff - num3))
                    {
                        return false;
                    }
                    num += num3;
                }
                length = num;
                flag2 = true;
            }
            finally
            {
                base.BufferReader.Offset = offset;
            }
            return flag2;
        }

        private void VerifyWhitespace()
        {
            if (!base.Node.Value.IsWhitespace())
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
        }

        private enum ArrayState
        {
            None,
            Element,
            Content
        }
    }
}

