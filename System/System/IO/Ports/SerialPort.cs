namespace System.IO.Ports
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [MonitoringDescription("SerialPortDesc")]
    public class SerialPort : Component
    {
        private int baudRate;
        private int dataBits;
        private System.Text.Decoder decoder;
        private const int defaultBaudRate = 0x2580;
        private const int defaultBufferSize = 0x400;
        private const int defaultDataBits = 8;
        private const bool defaultDiscardNull = false;
        private const bool defaultDtrEnable = false;
        private const System.IO.Ports.Handshake defaultHandshake = System.IO.Ports.Handshake.None;
        private const string defaultNewLine = "\n";
        private const System.IO.Ports.Parity defaultParity = System.IO.Ports.Parity.None;
        private const byte defaultParityReplace = 0x3f;
        private const string defaultPortName = "COM1";
        private const int defaultReadBufferSize = 0x1000;
        private const int defaultReadTimeout = -1;
        private const int defaultReceivedBytesThreshold = 1;
        private const bool defaultRtsEnable = false;
        private const System.IO.Ports.StopBits defaultStopBits = System.IO.Ports.StopBits.One;
        private const int defaultWriteBufferSize = 0x800;
        private const int defaultWriteTimeout = -1;
        private bool discardNull;
        private bool dtrEnable;
        private System.Text.Encoding encoding;
        private System.IO.Ports.Handshake handshake;
        private byte[] inBuffer;
        public const int InfiniteTimeout = -1;
        private SerialStream internalSerialStream;
        private int maxByteCountForSingleChar;
        private const int maxDataBits = 8;
        private const int minDataBits = 5;
        private string newLine;
        private char[] oneChar;
        private System.IO.Ports.Parity parity;
        private byte parityReplace;
        private string portName;
        private int readBufferSize;
        private int readLen;
        private int readPos;
        private int readTimeout;
        private int receivedBytesThreshold;
        private bool rtsEnable;
        private const string SERIAL_NAME = @"\Device\Serial";
        private char[] singleCharBuffer;
        private System.IO.Ports.StopBits stopBits;
        private int writeBufferSize;
        private int writeTimeout;

        [MonitoringDescription("SerialDataReceived")]
        public event SerialDataReceivedEventHandler DataReceived;

        [MonitoringDescription("SerialErrorReceived")]
        public event SerialErrorReceivedEventHandler ErrorReceived;

        [MonitoringDescription("SerialPinChanged")]
        public event SerialPinChangedEventHandler PinChanged;

        public SerialPort()
        {
            this.baudRate = 0x2580;
            this.dataBits = 8;
            this.stopBits = System.IO.Ports.StopBits.One;
            this.portName = "COM1";
            this.encoding = System.Text.Encoding.ASCII;
            this.decoder = System.Text.Encoding.ASCII.GetDecoder();
            this.maxByteCountForSingleChar = System.Text.Encoding.ASCII.GetMaxByteCount(1);
            this.readTimeout = -1;
            this.writeTimeout = -1;
            this.receivedBytesThreshold = 1;
            this.parityReplace = 0x3f;
            this.newLine = "\n";
            this.readBufferSize = 0x1000;
            this.writeBufferSize = 0x800;
            this.inBuffer = new byte[0x400];
            this.oneChar = new char[1];
        }

        public SerialPort(IContainer container)
        {
            this.baudRate = 0x2580;
            this.dataBits = 8;
            this.stopBits = System.IO.Ports.StopBits.One;
            this.portName = "COM1";
            this.encoding = System.Text.Encoding.ASCII;
            this.decoder = System.Text.Encoding.ASCII.GetDecoder();
            this.maxByteCountForSingleChar = System.Text.Encoding.ASCII.GetMaxByteCount(1);
            this.readTimeout = -1;
            this.writeTimeout = -1;
            this.receivedBytesThreshold = 1;
            this.parityReplace = 0x3f;
            this.newLine = "\n";
            this.readBufferSize = 0x1000;
            this.writeBufferSize = 0x800;
            this.inBuffer = new byte[0x400];
            this.oneChar = new char[1];
            container.Add(this);
        }

        public SerialPort(string portName) : this(portName, 0x2580, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate) : this(portName, baudRate, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity) : this(portName, baudRate, parity, 8, System.IO.Ports.StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits) : this(portName, baudRate, parity, dataBits, System.IO.Ports.StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits)
        {
            this.baudRate = 0x2580;
            this.dataBits = 8;
            this.stopBits = System.IO.Ports.StopBits.One;
            this.portName = "COM1";
            this.encoding = System.Text.Encoding.ASCII;
            this.decoder = System.Text.Encoding.ASCII.GetDecoder();
            this.maxByteCountForSingleChar = System.Text.Encoding.ASCII.GetMaxByteCount(1);
            this.readTimeout = -1;
            this.writeTimeout = -1;
            this.receivedBytesThreshold = 1;
            this.parityReplace = 0x3f;
            this.newLine = "\n";
            this.readBufferSize = 0x1000;
            this.writeBufferSize = 0x800;
            this.inBuffer = new byte[0x400];
            this.oneChar = new char[1];
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.DataBits = dataBits;
            this.StopBits = stopBits;
        }

        private void CatchErrorEvents(object src, SerialErrorReceivedEventArgs e)
        {
            SerialErrorReceivedEventHandler errorReceived = this.ErrorReceived;
            SerialStream internalSerialStream = this.internalSerialStream;
            if ((errorReceived != null) && (internalSerialStream != null))
            {
                lock (internalSerialStream)
                {
                    if (internalSerialStream.IsOpen)
                    {
                        errorReceived(this, e);
                    }
                }
            }
        }

        private void CatchPinChangedEvents(object src, SerialPinChangedEventArgs e)
        {
            SerialPinChangedEventHandler pinChanged = this.PinChanged;
            SerialStream internalSerialStream = this.internalSerialStream;
            if ((pinChanged != null) && (internalSerialStream != null))
            {
                lock (internalSerialStream)
                {
                    if (internalSerialStream.IsOpen)
                    {
                        pinChanged(this, e);
                    }
                }
            }
        }

        private void CatchReceivedEvents(object src, SerialDataReceivedEventArgs e)
        {
            SerialDataReceivedEventHandler dataReceived = this.DataReceived;
            SerialStream internalSerialStream = this.internalSerialStream;
            if ((dataReceived != null) && (internalSerialStream != null))
            {
                lock (internalSerialStream)
                {
                    bool flag = false;
                    try
                    {
                        flag = internalSerialStream.IsOpen && ((SerialData.Eof == e.EventType) || (this.BytesToRead >= this.receivedBytesThreshold));
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (flag)
                        {
                            dataReceived(this, e);
                        }
                    }
                }
            }
        }

        public void Close()
        {
            base.Dispose();
        }

        private void CompactBuffer()
        {
            Buffer.BlockCopy(this.inBuffer, this.readPos, this.inBuffer, 0, this.CachedBytesToRead);
            this.readLen = this.CachedBytesToRead;
            this.readPos = 0;
        }

        public void DiscardInBuffer()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            this.internalSerialStream.DiscardInBuffer();
            this.readPos = this.readLen = 0;
        }

        public void DiscardOutBuffer()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            this.internalSerialStream.DiscardOutBuffer();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.IsOpen)
            {
                this.internalSerialStream.Flush();
                this.internalSerialStream.Close();
                this.internalSerialStream = null;
            }
            base.Dispose(disposing);
        }

        private static int GetElapsedTime(int currentTickCount, int startTickCount)
        {
            int num = currentTickCount - startTickCount;
            if (num < 0)
            {
                return 0x7fffffff;
            }
            return num;
        }

        public static string[] GetPortNames()
        {
            RegistryKey localMachine = null;
            RegistryKey key2 = null;
            string[] strArray = null;
            new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM").Assert();
            try
            {
                localMachine = Registry.LocalMachine;
                key2 = localMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM", false);
                if (key2 != null)
                {
                    string[] valueNames = key2.GetValueNames();
                    strArray = new string[valueNames.Length];
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        strArray[i] = (string) key2.GetValue(valueNames[i]);
                    }
                }
            }
            finally
            {
                if (localMachine != null)
                {
                    localMachine.Close();
                }
                if (key2 != null)
                {
                    key2.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
            if (strArray == null)
            {
                strArray = new string[0];
            }
            return strArray;
        }

        private int InternalRead(char[] buffer, int offset, int count, int timeout, bool countMultiByteCharsAsOne)
        {
            if (count == 0)
            {
                return 0;
            }
            int tickCount = Environment.TickCount;
            int bytesToRead = this.internalSerialStream.BytesToRead;
            this.MaybeResizeBuffer(bytesToRead);
            this.readLen += this.internalSerialStream.Read(this.inBuffer, this.readLen, bytesToRead);
            if (this.decoder.GetCharCount(this.inBuffer, this.readPos, this.CachedBytesToRead) > 0)
            {
                return this.ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
            }
            if (timeout == 0)
            {
                throw new TimeoutException();
            }
            int maxByteCount = this.Encoding.GetMaxByteCount(count);
            while (true)
            {
                this.MaybeResizeBuffer(maxByteCount);
                this.readLen += this.internalSerialStream.Read(this.inBuffer, this.readLen, maxByteCount);
                int num4 = this.ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
                if (num4 > 0)
                {
                    return num4;
                }
                if ((timeout != -1) && ((timeout - GetElapsedTime(Environment.TickCount, tickCount)) <= 0))
                {
                    throw new TimeoutException();
                }
            }
        }

        private void MaybeResizeBuffer(int additionalByteLength)
        {
            if ((additionalByteLength + this.readLen) > this.inBuffer.Length)
            {
                if ((this.CachedBytesToRead + additionalByteLength) <= (this.inBuffer.Length / 2))
                {
                    this.CompactBuffer();
                }
                else
                {
                    byte[] dst = new byte[Math.Max((int) (this.CachedBytesToRead + additionalByteLength), (int) (this.inBuffer.Length * 2))];
                    Buffer.BlockCopy(this.inBuffer, this.readPos, dst, 0, this.CachedBytesToRead);
                    this.readLen = this.CachedBytesToRead;
                    this.readPos = 0;
                    this.inBuffer = dst;
                }
            }
        }

        public void Open()
        {
            if (this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_already_open"));
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            this.internalSerialStream = new SerialStream(this.portName, this.baudRate, this.parity, this.dataBits, this.stopBits, this.readTimeout, this.writeTimeout, this.handshake, this.dtrEnable, this.rtsEnable, this.discardNull, this.parityReplace);
            this.internalSerialStream.SetBufferSizes(this.readBufferSize, this.writeBufferSize);
            this.internalSerialStream.ErrorReceived += new SerialErrorReceivedEventHandler(this.CatchErrorEvents);
            this.internalSerialStream.PinChanged += new SerialPinChangedEventHandler(this.CatchPinChangedEvents);
            this.internalSerialStream.DataReceived += new SerialDataReceivedEventHandler(this.CatchReceivedEvents);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            int num = 0;
            if (this.CachedBytesToRead >= 1)
            {
                num = Math.Min(this.CachedBytesToRead, count);
                Buffer.BlockCopy(this.inBuffer, this.readPos, buffer, offset, num);
                this.readPos += num;
                if (num == count)
                {
                    if (this.readPos == this.readLen)
                    {
                        this.readPos = this.readLen = 0;
                    }
                    return count;
                }
                if (this.BytesToRead == 0)
                {
                    return num;
                }
            }
            this.readLen = this.readPos = 0;
            int num2 = count - num;
            num += this.internalSerialStream.Read(buffer, offset + num, num2);
            this.decoder.Reset();
            return num;
        }

        public int Read(char[] buffer, int offset, int count)
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            return this.InternalRead(buffer, offset, count, this.readTimeout, false);
        }

        private int ReadBufferIntoChars(char[] buffer, int offset, int count, bool countMultiByteCharsAsOne)
        {
            int byteCount = Math.Min(count, this.CachedBytesToRead);
            DecoderReplacementFallback decoderFallback = this.encoding.DecoderFallback as DecoderReplacementFallback;
            if ((this.encoding.IsSingleByte && (this.encoding.GetMaxCharCount(byteCount) == byteCount)) && ((decoderFallback != null) && (decoderFallback.MaxCharCount == 1)))
            {
                this.decoder.GetChars(this.inBuffer, this.readPos, byteCount, buffer, offset);
                this.readPos += byteCount;
                if (this.readPos == this.readLen)
                {
                    this.readPos = this.readLen = 0;
                }
                return byteCount;
            }
            int num2 = 0;
            int num3 = 0;
            int readPos = this.readPos;
            do
            {
                int num4 = Math.Min((int) (count - num3), (int) ((this.readLen - this.readPos) - num2));
                if (num4 <= 0)
                {
                    break;
                }
                num2 += num4;
                num4 = (this.readPos + num2) - readPos;
                int num5 = this.decoder.GetCharCount(this.inBuffer, readPos, num4);
                if (num5 > 0)
                {
                    if (((num3 + num5) > count) && !countMultiByteCharsAsOne)
                    {
                        break;
                    }
                    int num7 = num4;
                    do
                    {
                        num7--;
                    }
                    while (this.decoder.GetCharCount(this.inBuffer, readPos, num7) == num5);
                    this.decoder.GetChars(this.inBuffer, readPos, num7 + 1, buffer, offset + num3);
                    readPos = (readPos + num7) + 1;
                }
                num3 += num5;
            }
            while ((num3 < count) && (num2 < this.CachedBytesToRead));
            this.readPos = readPos;
            if (this.readPos == this.readLen)
            {
                this.readPos = this.readLen = 0;
            }
            return num3;
        }

        public int ReadByte()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (this.readLen != this.readPos)
            {
                return this.inBuffer[this.readPos++];
            }
            this.decoder.Reset();
            return this.internalSerialStream.ReadByte();
        }

        public int ReadChar()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            return this.ReadOneChar(this.readTimeout);
        }

        public string ReadExisting()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            byte[] dst = new byte[this.BytesToRead];
            if (this.readPos < this.readLen)
            {
                Buffer.BlockCopy(this.inBuffer, this.readPos, dst, 0, this.CachedBytesToRead);
            }
            this.internalSerialStream.Read(dst, this.CachedBytesToRead, dst.Length - this.CachedBytesToRead);
            System.Text.Decoder decoder = this.Encoding.GetDecoder();
            int num = decoder.GetCharCount(dst, 0, dst.Length);
            int length = dst.Length;
            if (num == 0)
            {
                Buffer.BlockCopy(dst, 0, this.inBuffer, 0, dst.Length);
                this.readPos = 0;
                this.readLen = dst.Length;
                return "";
            }
            do
            {
                decoder.Reset();
                length--;
            }
            while (decoder.GetCharCount(dst, 0, length) == num);
            this.readPos = 0;
            this.readLen = dst.Length - (length + 1);
            Buffer.BlockCopy(dst, length + 1, this.inBuffer, 0, dst.Length - (length + 1));
            return this.Encoding.GetString(dst, 0, length + 1);
        }

        public string ReadLine()
        {
            return this.ReadTo(this.NewLine);
        }

        private int ReadOneChar(int timeout)
        {
            int num2 = 0;
            if (this.decoder.GetCharCount(this.inBuffer, this.readPos, this.CachedBytesToRead) != 0)
            {
                int readPos = this.readPos;
                do
                {
                    this.readPos++;
                }
                while (this.decoder.GetCharCount(this.inBuffer, readPos, this.readPos - readPos) < 1);
                try
                {
                    this.decoder.GetChars(this.inBuffer, readPos, this.readPos - readPos, this.oneChar, 0);
                }
                catch
                {
                    this.readPos = readPos;
                    throw;
                }
                return this.oneChar[0];
            }
            if (timeout == 0)
            {
                int bytesToRead = this.internalSerialStream.BytesToRead;
                if (bytesToRead == 0)
                {
                    bytesToRead = 1;
                }
                this.MaybeResizeBuffer(bytesToRead);
                this.readLen += this.internalSerialStream.Read(this.inBuffer, this.readLen, bytesToRead);
                if (this.ReadBufferIntoChars(this.oneChar, 0, 1, false) == 0)
                {
                    throw new TimeoutException();
                }
                return this.oneChar[0];
            }
            int tickCount = Environment.TickCount;
            do
            {
                int num;
                if (timeout == -1)
                {
                    num = this.internalSerialStream.ReadByte(-1);
                }
                else
                {
                    if ((timeout - num2) < 0)
                    {
                        throw new TimeoutException();
                    }
                    num = this.internalSerialStream.ReadByte(timeout - num2);
                    num2 = Environment.TickCount - tickCount;
                }
                this.MaybeResizeBuffer(1);
                this.inBuffer[this.readLen++] = (byte) num;
            }
            while (this.decoder.GetCharCount(this.inBuffer, this.readPos, this.readLen - this.readPos) < 1);
            this.decoder.GetChars(this.inBuffer, this.readPos, this.readLen - this.readPos, this.oneChar, 0);
            this.readLen = this.readPos = 0;
            return this.oneChar[0];
        }

        public string ReadTo(string value)
        {
            string str2;
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "value" }));
            }
            int tickCount = Environment.TickCount;
            int num2 = 0;
            StringBuilder builder = new StringBuilder();
            char ch = value[value.Length - 1];
            int bytesToRead = this.internalSerialStream.BytesToRead;
            this.MaybeResizeBuffer(bytesToRead);
            this.readLen += this.internalSerialStream.Read(this.inBuffer, this.readLen, bytesToRead);
            if (this.singleCharBuffer == null)
            {
                this.singleCharBuffer = new char[this.maxByteCountForSingleChar];
            }
            try
            {
                int num;
            Label_00C3:
                if (this.readTimeout == -1)
                {
                    num = this.InternalRead(this.singleCharBuffer, 0, 1, this.readTimeout, true);
                }
                else
                {
                    if ((this.readTimeout - num2) < 0)
                    {
                        throw new TimeoutException();
                    }
                    int num3 = Environment.TickCount;
                    num = this.InternalRead(this.singleCharBuffer, 0, 1, this.readTimeout - num2, true);
                    num2 += Environment.TickCount - num3;
                }
                builder.Append(this.singleCharBuffer, 0, num);
                if ((ch != this.singleCharBuffer[num - 1]) || (builder.Length < value.Length))
                {
                    goto Label_00C3;
                }
                bool flag = true;
                for (int i = 2; i <= value.Length; i++)
                {
                    if (value[value.Length - i] != builder[builder.Length - i])
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    goto Label_00C3;
                }
                string str = builder.ToString(0, builder.Length - value.Length);
                if (this.readPos == this.readLen)
                {
                    this.readPos = this.readLen = 0;
                }
                str2 = str;
            }
            catch
            {
                byte[] bytes = this.encoding.GetBytes(builder.ToString());
                if (bytes.Length > 0)
                {
                    int cachedBytesToRead = this.CachedBytesToRead;
                    byte[] dst = new byte[cachedBytesToRead];
                    if (cachedBytesToRead > 0)
                    {
                        Buffer.BlockCopy(this.inBuffer, this.readPos, dst, 0, cachedBytesToRead);
                    }
                    this.readPos = 0;
                    this.readLen = 0;
                    this.MaybeResizeBuffer(bytes.Length + cachedBytesToRead);
                    Buffer.BlockCopy(bytes, 0, this.inBuffer, this.readLen, bytes.Length);
                    this.readLen += bytes.Length;
                    if (cachedBytesToRead > 0)
                    {
                        Buffer.BlockCopy(dst, 0, this.inBuffer, this.readLen, cachedBytesToRead);
                        this.readLen += cachedBytesToRead;
                    }
                }
                throw;
            }
            return str2;
        }

        public void Write(string text)
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (text.Length != 0)
            {
                byte[] bytes = this.encoding.GetBytes(text);
                this.internalSerialStream.Write(bytes, 0, bytes.Length, this.writeTimeout);
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            if (buffer.Length != 0)
            {
                this.internalSerialStream.Write(buffer, offset, count, this.writeTimeout);
            }
        }

        public void Write(char[] buffer, int offset, int count)
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException(SR.GetString("Port_not_open"));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            if (buffer.Length != 0)
            {
                byte[] buffer2 = this.Encoding.GetBytes(buffer, offset, count);
                this.Write(buffer2, 0, buffer2.Length);
            }
        }

        public void WriteLine(string text)
        {
            this.Write(text + this.NewLine);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Stream BaseStream
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("BaseStream_Invalid_Not_Open"));
                }
                return this.internalSerialStream;
            }
        }

        [MonitoringDescription("BaudRate"), Browsable(true), DefaultValue(0x2580)]
        public int BaudRate
        {
            get
            {
                return this.baudRate;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("BaudRate", SR.GetString("ArgumentOutOfRange_NeedPosNum"));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.BaudRate = value;
                }
                this.baudRate = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BreakState
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                return this.internalSerialStream.BreakState;
            }
            set
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                this.internalSerialStream.BreakState = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int BytesToRead
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                return (this.internalSerialStream.BytesToRead + this.CachedBytesToRead);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BytesToWrite
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                return this.internalSerialStream.BytesToWrite;
            }
        }

        private int CachedBytesToRead
        {
            get
            {
                return (this.readLen - this.readPos);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool CDHolding
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                return this.internalSerialStream.CDHolding;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CtsHolding
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                return this.internalSerialStream.CtsHolding;
            }
        }

        [Browsable(true), MonitoringDescription("DataBits"), DefaultValue(8)]
        public int DataBits
        {
            get
            {
                return this.dataBits;
            }
            set
            {
                if ((value < 5) || (value > 8))
                {
                    throw new ArgumentOutOfRangeException("DataBits", SR.GetString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[] { 5, 8 }));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.DataBits = value;
                }
                this.dataBits = value;
            }
        }

        [MonitoringDescription("DiscardNull"), Browsable(true), DefaultValue(false)]
        public bool DiscardNull
        {
            get
            {
                return this.discardNull;
            }
            set
            {
                if (this.IsOpen)
                {
                    this.internalSerialStream.DiscardNull = value;
                }
                this.discardNull = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool DsrHolding
        {
            get
            {
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Port_not_open"));
                }
                return this.internalSerialStream.DsrHolding;
            }
        }

        [DefaultValue(false), Browsable(true), MonitoringDescription("DtrEnable")]
        public bool DtrEnable
        {
            get
            {
                if (this.IsOpen)
                {
                    this.dtrEnable = this.internalSerialStream.DtrEnable;
                }
                return this.dtrEnable;
            }
            set
            {
                if (this.IsOpen)
                {
                    this.internalSerialStream.DtrEnable = value;
                }
                this.dtrEnable = value;
            }
        }

        [MonitoringDescription("Encoding"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Text.Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Encoding");
                }
                if (((!(value is ASCIIEncoding) && !(value is UTF8Encoding)) && (!(value is UnicodeEncoding) && !(value is UTF32Encoding))) && (((value.CodePage >= 0xc350) && (value.CodePage != 0xd698)) || (value.GetType().Assembly != typeof(string).Assembly)))
                {
                    throw new ArgumentException(SR.GetString("NotSupportedEncoding", new object[] { value.WebName }), "value");
                }
                this.encoding = value;
                this.decoder = this.encoding.GetDecoder();
                this.maxByteCountForSingleChar = this.encoding.GetMaxByteCount(1);
                this.singleCharBuffer = null;
            }
        }

        [DefaultValue(0), Browsable(true), MonitoringDescription("Handshake")]
        public System.IO.Ports.Handshake Handshake
        {
            get
            {
                return this.handshake;
            }
            set
            {
                if ((value < System.IO.Ports.Handshake.None) || (value > System.IO.Ports.Handshake.RequestToSendXOnXOff))
                {
                    throw new ArgumentOutOfRangeException("Handshake", SR.GetString("ArgumentOutOfRange_Enum"));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.Handshake = value;
                }
                this.handshake = value;
            }
        }

        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return ((this.internalSerialStream != null) && this.internalSerialStream.IsOpen);
            }
        }

        [DefaultValue("\n"), MonitoringDescription("NewLine"), Browsable(false)]
        public string NewLine
        {
            get
            {
                return this.newLine;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "NewLine" }));
                }
                this.newLine = value;
            }
        }

        [Browsable(true), MonitoringDescription("Parity"), DefaultValue(0)]
        public System.IO.Ports.Parity Parity
        {
            get
            {
                return this.parity;
            }
            set
            {
                if ((value < System.IO.Ports.Parity.None) || (value > System.IO.Ports.Parity.Space))
                {
                    throw new ArgumentOutOfRangeException("Parity", SR.GetString("ArgumentOutOfRange_Enum"));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.Parity = value;
                }
                this.parity = value;
            }
        }

        [MonitoringDescription("ParityReplace"), Browsable(true), DefaultValue((byte) 0x3f)]
        public byte ParityReplace
        {
            get
            {
                return this.parityReplace;
            }
            set
            {
                if (this.IsOpen)
                {
                    this.internalSerialStream.ParityReplace = value;
                }
                this.parityReplace = value;
            }
        }

        [Browsable(true), MonitoringDescription("PortName"), DefaultValue("COM1")]
        public string PortName
        {
            get
            {
                return this.portName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PortName");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("PortNameEmpty_String"), "PortName");
                }
                if (value.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    throw new ArgumentException(SR.GetString("Arg_SecurityException"), "PortName");
                }
                if (this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Cant_be_set_when_open", new object[] { "PortName" }));
                }
                this.portName = value;
            }
        }

        [MonitoringDescription("ReadBufferSize"), Browsable(true), DefaultValue(0x1000)]
        public int ReadBufferSize
        {
            get
            {
                return this.readBufferSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Cant_be_set_when_open", new object[] { "value" }));
                }
                this.readBufferSize = value;
            }
        }

        [Browsable(true), MonitoringDescription("ReadTimeout"), DefaultValue(-1)]
        public int ReadTimeout
        {
            get
            {
                return this.readTimeout;
            }
            set
            {
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("ReadTimeout", SR.GetString("ArgumentOutOfRange_Timeout"));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.ReadTimeout = value;
                }
                this.readTimeout = value;
            }
        }

        [MonitoringDescription("ReceivedBytesThreshold"), DefaultValue(1), Browsable(true)]
        public int ReceivedBytesThreshold
        {
            get
            {
                return this.receivedBytesThreshold;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("ReceivedBytesThreshold", SR.GetString("ArgumentOutOfRange_NeedPosNum"));
                }
                this.receivedBytesThreshold = value;
                if (this.IsOpen)
                {
                    SerialDataReceivedEventArgs e = new SerialDataReceivedEventArgs(SerialData.Chars);
                    this.CatchReceivedEvents(this, e);
                }
            }
        }

        [DefaultValue(false), MonitoringDescription("RtsEnable"), Browsable(true)]
        public bool RtsEnable
        {
            get
            {
                if (this.IsOpen)
                {
                    this.rtsEnable = this.internalSerialStream.RtsEnable;
                }
                return this.rtsEnable;
            }
            set
            {
                if (this.IsOpen)
                {
                    this.internalSerialStream.RtsEnable = value;
                }
                this.rtsEnable = value;
            }
        }

        [Browsable(true), DefaultValue(1), MonitoringDescription("StopBits")]
        public System.IO.Ports.StopBits StopBits
        {
            get
            {
                return this.stopBits;
            }
            set
            {
                if ((value < System.IO.Ports.StopBits.One) || (value > System.IO.Ports.StopBits.OnePointFive))
                {
                    throw new ArgumentOutOfRangeException("StopBits", SR.GetString("ArgumentOutOfRange_Enum"));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.StopBits = value;
                }
                this.stopBits = value;
            }
        }

        [MonitoringDescription("WriteBufferSize"), Browsable(true), DefaultValue(0x800)]
        public int WriteBufferSize
        {
            get
            {
                return this.writeBufferSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.IsOpen)
                {
                    throw new InvalidOperationException(SR.GetString("Cant_be_set_when_open", new object[] { "value" }));
                }
                this.writeBufferSize = value;
            }
        }

        [Browsable(true), MonitoringDescription("WriteTimeout"), DefaultValue(-1)]
        public int WriteTimeout
        {
            get
            {
                return this.writeTimeout;
            }
            set
            {
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("WriteTimeout", SR.GetString("ArgumentOutOfRange_WriteTimeout"));
                }
                if (this.IsOpen)
                {
                    this.internalSerialStream.WriteTimeout = value;
                }
                this.writeTimeout = value;
            }
        }
    }
}

