namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    internal class MimeHeaderReader
    {
        private byte[] buffer = new byte[0x400];
        private int maxOffset;
        private string name;
        private int offset;
        private ReadState readState;
        private Stream stream;
        private string value;

        public MimeHeaderReader(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.stream = stream;
        }

        private void AppendName(string value, int maxBuffer, ref int remaining)
        {
            XmlMtomReader.DecrementBufferQuota(maxBuffer, ref remaining, value.Length * 2);
            if (this.name == null)
            {
                this.name = value;
            }
            else
            {
                this.name = this.name + value;
            }
        }

        private void AppendValue(string value, int maxBuffer, ref int remaining)
        {
            XmlMtomReader.DecrementBufferQuota(maxBuffer, ref remaining, value.Length * 2);
            if (this.value == null)
            {
                this.value = value;
            }
            else
            {
                this.value = this.value + value;
            }
        }

        private bool BufferEnd()
        {
            if (this.maxOffset != 0)
            {
                return false;
            }
            if ((this.readState != ReadState.ReadWS) && (this.readState != ReadState.ReadValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeReaderMalformedHeader")));
            }
            this.readState = ReadState.EOF;
            return true;
        }

        public void Close()
        {
            this.stream.Close();
            this.readState = ReadState.EOF;
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private unsafe bool ProcessBuffer(int maxBuffer, ref int remaining)
        {
            fixed (byte* numRef = this.buffer)
            {
                byte* numPtr = numRef + this.offset;
                byte* numPtr2 = numRef + this.maxOffset;
                byte* numPtr3 = numPtr;
                switch (this.readState)
                {
                    case ReadState.ReadName:
                        goto Label_0136;

                    case ReadState.SkipWS:
                        goto Label_0170;

                    case ReadState.ReadValue:
                        goto Label_0180;

                    case ReadState.ReadLF:
                        goto Label_01F2;

                    case ReadState.ReadWS:
                        goto Label_0226;

                    case ReadState.EOF:
                        goto Label_025F;

                    default:
                        goto Label_0279;
                }
            Label_0061:
                if (numPtr3[0] == 0x3a)
                {
                    this.AppendName(new string((sbyte*) numPtr, 0, (int) ((long) ((numPtr3 - numPtr) / 1))), maxBuffer, ref remaining);
                    numPtr3++;
                    goto Label_0170;
                }
                if ((numPtr3[0] >= 0x41) && (numPtr3[0] <= 90))
                {
                    numPtr3[0] = (byte) (numPtr3[0] + 0x20);
                }
                else if ((numPtr3[0] < 0x21) || (numPtr3[0] > 0x7e))
                {
                    if ((this.name != null) || (numPtr3[0] != 13))
                    {
                        object[] args = new object[] { (char) numPtr3[0], ((int) numPtr3[0]).ToString("X", CultureInfo.InvariantCulture) };
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeHeaderInvalidCharacter", args)));
                    }
                    numPtr3++;
                    if ((numPtr3 >= numPtr2) || (numPtr3[0] != 10))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeReaderMalformedHeader")));
                    }
                    goto Label_025F;
                }
                numPtr3++;
            Label_0136:
                if (numPtr3 < numPtr2)
                {
                    goto Label_0061;
                }
                this.AppendName(new string((sbyte*) numPtr, 0, (int) ((long) ((numPtr3 - numPtr) / 1))), maxBuffer, ref remaining);
                this.readState = ReadState.ReadName;
                goto Label_0279;
            Label_015F:
                if ((numPtr3[0] != 9) && (numPtr3[0] != 0x20))
                {
                    goto Label_0180;
                }
                numPtr3++;
            Label_0170:
                if (numPtr3 < numPtr2)
                {
                    goto Label_015F;
                }
                this.readState = ReadState.SkipWS;
                goto Label_0279;
            Label_0180:
                numPtr = numPtr3;
                while (numPtr3 < numPtr2)
                {
                    if (numPtr3[0] == 13)
                    {
                        this.AppendValue(new string((sbyte*) numPtr, 0, (int) ((long) ((numPtr3 - numPtr) / 1))), maxBuffer, ref remaining);
                        numPtr3++;
                        goto Label_01F2;
                    }
                    if (numPtr3[0] == 10)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeReaderMalformedHeader")));
                    }
                    numPtr3++;
                }
                this.AppendValue(new string((sbyte*) numPtr, 0, (int) ((long) ((numPtr3 - numPtr) / 1))), maxBuffer, ref remaining);
                this.readState = ReadState.ReadValue;
                goto Label_0279;
            Label_01F2:
                if (numPtr3 < numPtr2)
                {
                    if (numPtr3[0] != 10)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeReaderMalformedHeader")));
                    }
                    numPtr3++;
                }
                else
                {
                    this.readState = ReadState.ReadLF;
                    goto Label_0279;
                }
            Label_0226:
                if (numPtr3 < numPtr2)
                {
                    if ((numPtr3[0] != 0x20) && (numPtr3[0] != 9))
                    {
                        this.readState = ReadState.ReadName;
                        this.offset = (int) ((long) ((numPtr3 - numRef) / 1));
                        return true;
                    }
                    goto Label_0180;
                }
                this.readState = ReadState.ReadWS;
                goto Label_0279;
            Label_025F:
                this.readState = ReadState.EOF;
                this.offset = (int) ((long) ((numPtr3 - numRef) / 1));
                return true;
            Label_0279:
                this.offset = (int) ((long) ((numPtr3 - numRef) / 1));
            }
            return false;
        }

        public bool Read(int maxBuffer, ref int remaining)
        {
            this.name = null;
            this.value = null;
            while (this.readState != ReadState.EOF)
            {
                if (this.offset == this.maxOffset)
                {
                    this.maxOffset = this.stream.Read(this.buffer, 0, this.buffer.Length);
                    this.offset = 0;
                    if (this.BufferEnd())
                    {
                        break;
                    }
                }
                if (this.ProcessBuffer(maxBuffer, ref remaining))
                {
                    break;
                }
            }
            return (this.value != null);
        }

        public void Reset(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (this.readState != ReadState.EOF)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeReaderResetCalledBeforeEOF")));
            }
            this.stream = stream;
            this.readState = ReadState.ReadName;
            this.maxOffset = 0;
            this.offset = 0;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }

        private enum ReadState
        {
            ReadName,
            SkipWS,
            ReadValue,
            ReadLF,
            ReadWS,
            EOF
        }
    }
}

