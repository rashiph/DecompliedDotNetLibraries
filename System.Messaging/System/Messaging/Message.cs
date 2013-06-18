namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Messaging.Design;
    using System.Messaging.Interop;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;

    [Designer("System.Messaging.Design.MessageDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class Message : Component
    {
        private MessageQueue cachedAdminQueue;
        private object cachedBodyObject;
        private Stream cachedBodyStream;
        private MessageQueue cachedDestinationQueue;
        private IMessageFormatter cachedFormatter;
        private MessageQueue cachedResponseQueue;
        private MessageQueue cachedTransactionStatusQueue;
        private const int DefaultCryptographicProviderNameSize = 0xff;
        private const int DefaultDigitalSignatureSize = 0xff;
        private const int DefaultQueueNameSize = 0xff;
        private const int DefaultSenderCertificateSize = 0xff;
        private const int DefaultSenderIdSize = 0xff;
        private const int DefaultSymmetricKeySize = 0xff;
        private MessagePropertyFilter filter;
        private const int GenericIdSize = 0x10;
        public static readonly TimeSpan InfiniteTimeout = TimeSpan.FromSeconds(4294967295);
        private string machineName;
        private const int MessageIdSize = 20;
        internal MessagePropertyVariants properties;
        private bool receiveCreated;

        public Message()
        {
            this.properties = new MessagePropertyVariants();
            this.receiveCreated = false;
            this.filter = new MessagePropertyFilter();
            this.properties.SetUI1Vector(2, new byte[20]);
            this.filter.Id = true;
        }

        internal Message(MessagePropertyFilter filter)
        {
            this.properties = new MessagePropertyVariants();
            this.receiveCreated = true;
            this.filter = filter;
            if (filter.data1 != 0)
            {
                int num = filter.data1;
                if ((num & 1) != 0)
                {
                    this.properties.SetUI2(1, 0);
                }
                if ((num & 4) != 0)
                {
                    this.properties.SetUI1(6, 0);
                }
                if ((num & 8) != 0)
                {
                    this.properties.SetString(0x11, new byte[510]);
                    this.properties.SetUI4(0x12, 0xff);
                }
                if ((num & 0x10) != 0)
                {
                    this.properties.SetUI1Vector(9, new byte[filter.bodySize]);
                    this.properties.SetUI4(10, filter.bodySize);
                    this.properties.SetUI4(0x2a, 0);
                }
                if ((num & 0x20) != 0)
                {
                    this.properties.SetString(11, new byte[filter.labelSize * 2]);
                    this.properties.SetUI4(12, filter.labelSize);
                }
                if ((num & 0x40) != 0)
                {
                    this.properties.SetUI1Vector(2, new byte[20]);
                }
                if ((num & 0x800) != 0)
                {
                    this.properties.SetUI8(60, 0L);
                }
                if ((num & 0x80) != 0)
                {
                    this.properties.SetUI1(7, 0);
                }
                if ((num & 0x100) != 0)
                {
                    this.properties.SetString(15, new byte[510]);
                    this.properties.SetUI4(0x10, 0xff);
                }
                if (((num & 1) == 0) && ((num & 0x200) != 0))
                {
                    this.properties.SetUI2(1, 0);
                }
                if (((num & 0x80) == 0) && ((num & 0x400) != 0))
                {
                    this.properties.SetUI1(7, 0);
                }
            }
            if (filter.data2 != 0)
            {
                int num2 = filter.data2;
                if ((num2 & 1) != 0)
                {
                    this.properties.SetUI4(8, 0);
                }
                if ((num2 & 4) != 0)
                {
                    this.properties.SetUI4(0x20, 0);
                }
                if ((num2 & 8) != 0)
                {
                    this.properties.SetUI4(0x16, 0);
                }
                if ((num2 & 0x10) != 0)
                {
                    this.properties.SetUI1(0x19, 0);
                }
                if ((num2 & 0x20) != 0)
                {
                    this.properties.SetGuid(0x26, new byte[0x10]);
                }
                if ((num2 & 0x40) != 0)
                {
                    this.properties.SetUI1Vector(3, new byte[20]);
                }
                if ((num2 & 0x80) != 0)
                {
                    this.properties.SetString(0x30, new byte[510]);
                    this.properties.SetUI4(0x31, 0xff);
                }
                if ((num2 & 0x100) != 0)
                {
                    this.properties.SetUI4(0x2f, 0);
                }
                if ((num2 & 0x200) != 0)
                {
                    this.properties.SetUI1(5, 0);
                }
                if ((num2 & 0x8000) != 0)
                {
                    this.properties.SetString(0x21, new byte[510]);
                    this.properties.SetUI4(0x22, 0xff);
                }
                if ((num2 & 0x400) != 0)
                {
                    this.properties.SetUI1Vector(0x2d, new byte[0xff]);
                    this.properties.SetUI4(0x2e, 0xff);
                }
                if ((num2 & 0x800) != 0)
                {
                    this.properties.SetUI4(0x1b, 0);
                }
                if ((num2 & 0x1000) != 0)
                {
                    this.properties.SetUI1Vector(0x23, new byte[filter.extensionSize]);
                    this.properties.SetUI4(0x24, filter.extensionSize);
                }
                if ((num2 & 0x2000) != 0)
                {
                    this.properties.SetString(0x27, new byte[510]);
                    this.properties.SetUI4(40, 0xff);
                }
                if ((num2 & 0x4000) != 0)
                {
                    this.properties.SetUI4(0x1a, 0);
                }
                if ((num2 & 0x20000000) != 0)
                {
                    this.properties.SetUI1(50, 0);
                }
                if ((num2 & 0x40000000) != 0)
                {
                    this.properties.SetUI1(0x33, 0);
                }
                if ((num2 & 0x10000) != 0)
                {
                    this.properties.SetUI1(4, 0);
                }
                if ((num2 & 0x40000) != 0)
                {
                    this.properties.SetUI1Vector(0x1c, new byte[0xff]);
                    this.properties.SetUI4(0x1d, 0xff);
                }
                if ((num2 & 0x80000) != 0)
                {
                    this.properties.SetUI1Vector(20, new byte[0xff]);
                    this.properties.SetUI4(0x15, 0xff);
                }
                if ((num2 & 0x100000) != 0)
                {
                    this.properties.SetUI4(0x1f, 0);
                }
                if ((num2 & 0x200000) != 0)
                {
                    this.properties.SetGuid(30, new byte[0x10]);
                }
                if ((num2 & 0x400000) != 0)
                {
                    this.properties.SetUI1Vector(0x2b, new byte[0xff]);
                    this.properties.SetUI4(0x2c, 0xff);
                }
                if ((num2 & 0x800000) != 0)
                {
                    this.properties.SetUI4(14, 0);
                }
                if ((num2 & 0x1000000) != 0)
                {
                    this.properties.SetUI4(13, 0);
                }
                if ((num2 & -2147483648) != 0)
                {
                    this.properties.SetUI1Vector(0x34, new byte[20]);
                }
                if ((num2 & 0x2000000) != 0)
                {
                    this.properties.SetUI4(0x18, 0);
                }
                if ((num2 & 0x4000000) != 0)
                {
                    this.properties.SetUI4(0x17, 0);
                }
                if ((num2 & 0x8000000) != 0)
                {
                    this.properties.SetUI1(0x29, 0);
                }
                if ((num2 & 0x10000000) != 0)
                {
                    this.properties.SetUI4(0x13, 0);
                }
            }
        }

        public Message(object body) : this()
        {
            this.Body = body;
        }

        public Message(object body, IMessageFormatter formatter) : this()
        {
            this.Formatter = formatter;
            this.Body = body;
        }

        internal void AdjustMemory()
        {
            if (this.filter.AdministrationQueue)
            {
                int num = this.properties.GetUI4(0x12);
                if (num > 0xff)
                {
                    this.properties.SetString(0x11, new byte[num * 2]);
                }
            }
            if (this.filter.Body)
            {
                int num2 = this.properties.GetUI4(10);
                if (num2 > this.DefaultBodySize)
                {
                    this.properties.SetUI1Vector(9, new byte[num2]);
                }
            }
            if (this.filter.AuthenticationProviderName)
            {
                int num3 = this.properties.GetUI4(0x31);
                if (num3 > 0xff)
                {
                    this.properties.SetString(0x30, new byte[num3 * 2]);
                }
            }
            if (this.filter.DestinationQueue)
            {
                int num4 = this.properties.GetUI4(0x22);
                if (num4 > 0xff)
                {
                    this.properties.SetString(0x21, new byte[num4 * 2]);
                }
            }
            if (this.filter.Extension)
            {
                int num5 = this.properties.GetUI4(0x24);
                if (num5 > this.DefaultExtensionSize)
                {
                    this.properties.SetUI1Vector(0x23, new byte[num5]);
                }
            }
            if (this.filter.TransactionStatusQueue)
            {
                int num6 = this.properties.GetUI4(40);
                if (num6 > 0xff)
                {
                    this.properties.SetString(0x27, new byte[num6 * 2]);
                }
            }
            if (this.filter.Label)
            {
                int num7 = this.properties.GetUI4(12);
                if (num7 > this.DefaultLabelSize)
                {
                    this.properties.SetString(11, new byte[num7 * 2]);
                }
            }
            if (this.filter.ResponseQueue)
            {
                int num8 = this.properties.GetUI4(0x10);
                if (num8 > 0xff)
                {
                    this.properties.SetString(15, new byte[num8 * 2]);
                }
            }
            if (this.filter.SenderCertificate)
            {
                int num9 = this.properties.GetUI4(0x1d);
                if (num9 > 0xff)
                {
                    this.properties.SetUI1Vector(0x1c, new byte[num9]);
                }
            }
            if (this.filter.SenderId)
            {
                int num10 = this.properties.GetUI4(0x15);
                if (num10 > 0xff)
                {
                    this.properties.SetUI1Vector(20, new byte[num10]);
                }
            }
            if (this.filter.DestinationSymmetricKey)
            {
                int num11 = this.properties.GetUI4(0x2c);
                if (num11 > 0xff)
                {
                    this.properties.SetUI1Vector(0x2b, new byte[num11]);
                }
            }
            if (this.filter.DigitalSignature)
            {
                int num12 = this.properties.GetUI4(0x2e);
                if (num12 > 0xff)
                {
                    this.properties.SetUI1Vector(0x2d, new byte[num12]);
                }
            }
        }

        internal void AdjustToSend()
        {
            string formatName;
            if (this.filter.AdministrationQueue && (this.cachedAdminQueue != null))
            {
                formatName = this.cachedAdminQueue.FormatName;
                this.properties.SetString(0x11, StringToBytes(formatName));
                this.properties.SetUI4(0x12, formatName.Length);
            }
            if (this.filter.ResponseQueue && (this.cachedResponseQueue != null))
            {
                formatName = this.cachedResponseQueue.FormatName;
                this.properties.SetString(15, StringToBytes(formatName));
                this.properties.SetUI4(0x10, formatName.Length);
            }
            if (this.filter.TransactionStatusQueue && (this.cachedTransactionStatusQueue != null))
            {
                formatName = this.cachedTransactionStatusQueue.FormatName;
                this.properties.SetString(0x27, StringToBytes(formatName));
                this.properties.SetUI4(40, formatName.Length);
            }
            if (this.filter.Body && (this.cachedBodyObject != null))
            {
                if (this.Formatter == null)
                {
                    this.Formatter = new XmlMessageFormatter();
                }
                this.Formatter.Write(this, this.cachedBodyObject);
            }
            if (this.filter.Body && (this.cachedBodyStream != null))
            {
                this.cachedBodyStream.Position = 0L;
                byte[] buffer = new byte[(int) this.cachedBodyStream.Length];
                this.cachedBodyStream.Read(buffer, 0, buffer.Length);
                this.properties.SetUI1Vector(9, buffer);
                this.properties.SetUI4(10, buffer.Length);
            }
            if (this.receiveCreated)
            {
                lock (this)
                {
                    if (this.receiveCreated)
                    {
                        if (this.filter.Body)
                        {
                            int length = this.properties.GetUI4(10);
                            byte[] sourceArray = this.properties.GetUI1Vector(9);
                            if (length < sourceArray.Length)
                            {
                                byte[] destinationArray = new byte[length];
                                Array.Copy(sourceArray, destinationArray, length);
                                this.properties.SetUI1Vector(9, destinationArray);
                            }
                        }
                        if (this.filter.Extension)
                        {
                            this.properties.AdjustSize(0x23, this.properties.GetUI4(0x24));
                        }
                        if (this.filter.SenderCertificate)
                        {
                            this.properties.AdjustSize(0x1c, this.properties.GetUI4(0x1d));
                        }
                        if (this.filter.DestinationSymmetricKey)
                        {
                            this.properties.AdjustSize(0x2b, this.properties.GetUI4(0x2c));
                        }
                        if (this.filter.Acknowledgment || this.filter.MessageType)
                        {
                            this.properties.Ghost(1);
                        }
                        if (this.filter.ArrivedTime)
                        {
                            this.properties.Ghost(0x20);
                        }
                        if (this.filter.Authenticated)
                        {
                            this.properties.Ghost(0x19);
                        }
                        if (this.filter.DestinationQueue)
                        {
                            this.properties.Ghost(0x21);
                            this.properties.Ghost(0x22);
                            this.cachedDestinationQueue = null;
                        }
                        if (this.filter.IsFirstInTransaction)
                        {
                            this.properties.Ghost(50);
                        }
                        if (this.filter.IsLastInTransaction)
                        {
                            this.properties.Ghost(0x33);
                        }
                        if (this.filter.SenderId)
                        {
                            this.properties.Ghost(20);
                            this.properties.Ghost(0x15);
                        }
                        if (this.filter.SentTime)
                        {
                            this.properties.Ghost(0x1f);
                        }
                        if (this.filter.SourceMachine)
                        {
                            this.properties.Ghost(30);
                        }
                        if (this.filter.TransactionId)
                        {
                            this.properties.Ghost(0x34);
                        }
                        if (this.filter.SenderVersion)
                        {
                            this.properties.Ghost(0x13);
                        }
                        if (this.filter.AdministrationQueue && (this.properties.GetUI4(0x12) == 0))
                        {
                            this.properties.Ghost(0x11);
                            this.properties.Ghost(0x12);
                        }
                        if (this.filter.EncryptionAlgorithm && ((this.filter.UseEncryption && !this.UseEncryption) || !this.filter.UseEncryption))
                        {
                            this.properties.Ghost(0x1b);
                        }
                        if (this.filter.DigitalSignature && (this.properties.GetUI4(0x2e) == 0))
                        {
                            this.properties.Ghost(0x2d);
                            this.properties.Ghost(0x2e);
                        }
                        if (this.filter.DestinationSymmetricKey && (this.properties.GetUI4(0x2c) == 0))
                        {
                            this.properties.Ghost(0x2b);
                            this.properties.Ghost(0x2c);
                        }
                        if (this.filter.ResponseQueue && (this.properties.GetUI4(0x10) == 0))
                        {
                            this.properties.Ghost(15);
                            this.properties.Ghost(0x10);
                        }
                        if (this.filter.TransactionStatusQueue && (this.properties.GetUI4(40) == 0))
                        {
                            this.properties.Ghost(0x27);
                            this.properties.Ghost(40);
                        }
                        this.receiveCreated = false;
                    }
                }
            }
        }

        private string IdFromByteArray(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            byte[] destinationArray = new byte[0x10];
            Array.Copy(bytes, destinationArray, 0x10);
            int num = BitConverter.ToInt32(bytes, 0x10);
            builder.Append(new Guid(destinationArray).ToString());
            builder.Append(@"\");
            builder.Append(num);
            return builder.ToString();
        }

        private byte[] IdToByteArray(string id)
        {
            Guid guid;
            int num;
            string[] strArray = id.Split(new char[] { '\\' });
            if (strArray.Length != 2)
            {
                throw new InvalidOperationException(Res.GetString("InvalidId"));
            }
            try
            {
                guid = new Guid(strArray[0]);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(Res.GetString("InvalidId"));
            }
            try
            {
                num = Convert.ToInt32(strArray[1], CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(Res.GetString("InvalidId"));
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException(Res.GetString("InvalidId"));
            }
            byte[] destinationArray = new byte[20];
            Array.Copy(guid.ToByteArray(), destinationArray, 0x10);
            Array.Copy(BitConverter.GetBytes(num), 0, destinationArray, 0x10, 4);
            return destinationArray;
        }

        internal MessagePropertyVariants.MQPROPS Lock()
        {
            return this.properties.Lock();
        }

        internal void SetLookupId(long value)
        {
            this.filter.LookupId = true;
            this.properties.SetUI8(60, value);
        }

        internal static string StringFromBytes(byte[] bytes, int len)
        {
            if (((len != 0) && (bytes[(len * 2) - 1] == 0)) && (bytes[(len * 2) - 2] == 0))
            {
                len--;
            }
            char[] chars = new char[len];
            Encoding.Unicode.GetChars(bytes, 0, len * 2, chars, 0);
            return new string(chars, 0, len);
        }

        internal static byte[] StringToBytes(string value)
        {
            int num = (value.Length * 2) + 1;
            byte[] bytes = new byte[num];
            bytes[num - 1] = 0;
            Encoding.Unicode.GetBytes(value.ToCharArray(), 0, value.Length, bytes, 0);
            return bytes;
        }

        internal void Unlock()
        {
            this.properties.Unlock();
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgAcknowledgeType")]
        public AcknowledgeTypes AcknowledgeType
        {
            get
            {
                if (this.filter.AcknowledgeType)
                {
                    return (AcknowledgeTypes) this.properties.GetUI1(6);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "AcknowledgeType" }));
                }
                return AcknowledgeTypes.None;
            }
            set
            {
                if (value == AcknowledgeTypes.None)
                {
                    this.filter.AcknowledgeType = false;
                    this.properties.Remove(6);
                }
                else
                {
                    this.filter.AcknowledgeType = true;
                    this.properties.SetUI1(6, (byte) value);
                }
            }
        }

        [MessagingDescription("MsgAcknowledgement"), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Messaging.Acknowledgment Acknowledgment
        {
            get
            {
                if (!this.filter.Acknowledgment)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("NotAcknowledgement"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Acknowledgment" }));
                }
                int num = this.properties.GetUI2(1) & 0xffff;
                return (System.Messaging.Acknowledgment) num;
            }
        }

        [MessagingDescription("MsgAdministrationQueue"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true)]
        public MessageQueue AdministrationQueue
        {
            get
            {
                if (!this.filter.AdministrationQueue)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "AdministrationQueue" }));
                    }
                    return null;
                }
                if ((this.cachedAdminQueue == null) && (this.properties.GetUI4(0x12) != 0))
                {
                    string str = StringFromBytes(this.properties.GetString(0x11), this.properties.GetUI4(0x12));
                    this.cachedAdminQueue = new MessageQueue("FORMATNAME:" + str);
                }
                return this.cachedAdminQueue;
            }
            set
            {
                if (value != null)
                {
                    this.filter.AdministrationQueue = true;
                }
                else if (this.filter.AdministrationQueue)
                {
                    this.filter.AdministrationQueue = false;
                    this.properties.Remove(0x11);
                    this.properties.Remove(0x12);
                }
                this.cachedAdminQueue = value;
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgAppSpecific")]
        public int AppSpecific
        {
            get
            {
                if (this.filter.AppSpecific)
                {
                    return this.properties.GetUI4(8);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "AppSpecific" }));
                }
                return 0;
            }
            set
            {
                if (value == 0)
                {
                    this.filter.AppSpecific = false;
                    this.properties.Remove(8);
                }
                else
                {
                    this.filter.AppSpecific = true;
                    this.properties.SetUI4(8, value);
                }
            }
        }

        [MessagingDescription("MsgArrivedTime"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime ArrivedTime
        {
            get
            {
                if (!this.filter.ArrivedTime)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("ArrivedTimeNotSet"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "ArrivedTime" }));
                }
                DateTime time = new DateTime(0x7b2, 1, 1);
                return time.AddSeconds((double) this.properties.GetUI4(0x20)).ToLocalTime();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgAttachSenderId"), ReadOnly(true)]
        public bool AttachSenderId
        {
            get
            {
                if (!this.filter.AttachSenderId)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "AttachSenderId" }));
                    }
                    return true;
                }
                if (this.properties.GetUI4(0x16) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                if (value)
                {
                    this.filter.AttachSenderId = false;
                    this.properties.Remove(0x16);
                }
                else
                {
                    this.filter.AttachSenderId = true;
                    if (value)
                    {
                        this.properties.SetUI4(0x16, 1);
                    }
                    else
                    {
                        this.properties.SetUI4(0x16, 0);
                    }
                }
            }
        }

        [MessagingDescription("MsgAuthenticated"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Authenticated
        {
            get
            {
                if (this.filter.Authenticated)
                {
                    return (this.properties.GetUI1(0x19) != 0);
                }
                if (!this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("AuthenticationNotSet"));
                }
                throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Authenticated" }));
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgAuthenticationProviderName")]
        public string AuthenticationProviderName
        {
            get
            {
                if (!this.filter.AuthenticationProviderName)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "AuthenticationProviderName" }));
                    }
                    return "Microsoft Base Cryptographic Provider, Ver. 1.0";
                }
                if (this.properties.GetUI4(0x31) != 0)
                {
                    return StringFromBytes(this.properties.GetString(0x30), this.properties.GetUI4(0x31));
                }
                return "";
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.filter.AuthenticationProviderName = true;
                this.properties.SetString(0x30, StringToBytes(value));
                this.properties.SetUI4(0x31, value.Length);
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgAuthenticationProviderType")]
        public CryptographicProviderType AuthenticationProviderType
        {
            get
            {
                if (this.filter.AuthenticationProviderType)
                {
                    return (CryptographicProviderType) this.properties.GetUI4(0x2f);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "AuthenticationProviderType" }));
                }
                return CryptographicProviderType.RsaFull;
            }
            set
            {
                if (!ValidationUtility.ValidateCryptographicProviderType(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(CryptographicProviderType));
                }
                this.filter.AuthenticationProviderType = true;
                this.properties.SetUI4(0x2f, (int) value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object Body
        {
            get
            {
                if (!this.filter.Body)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Body" }));
                    }
                    return null;
                }
                if (this.cachedBodyObject == null)
                {
                    if (this.Formatter == null)
                    {
                        throw new InvalidOperationException(Res.GetString("FormatterMissing"));
                    }
                    this.cachedBodyObject = this.Formatter.Read(this);
                }
                return this.cachedBodyObject;
            }
            set
            {
                this.filter.Body = true;
                this.cachedBodyObject = value;
            }
        }

        [MessagingDescription("MsgBodyStream"), Editor("System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Stream BodyStream
        {
            get
            {
                if (!this.filter.Body)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Body" }));
                    }
                    this.filter.Body = true;
                    if (this.cachedBodyStream == null)
                    {
                        this.cachedBodyStream = new MemoryStream();
                    }
                    return this.cachedBodyStream;
                }
                if (this.cachedBodyStream == null)
                {
                    this.cachedBodyStream = new MemoryStream(this.properties.GetUI1Vector(9), 0, this.properties.GetUI4(10));
                }
                return this.cachedBodyStream;
            }
            set
            {
                if (value != null)
                {
                    this.filter.Body = true;
                }
                else
                {
                    this.filter.Body = false;
                    this.properties.Remove(9);
                    this.properties.Remove(0x2a);
                    this.properties.Remove(10);
                }
                this.cachedBodyStream = value;
            }
        }

        [ReadOnly(true), MessagingDescription("MsgBodyType"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BodyType
        {
            get
            {
                if (this.filter.Body)
                {
                    return this.properties.GetUI4(0x2a);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Body" }));
                }
                return 0;
            }
            set
            {
                this.properties.SetUI4(0x2a, value);
            }
        }

        [MessagingDescription("MsgConnectorType"), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guid ConnectorType
        {
            get
            {
                if (this.filter.ConnectorType)
                {
                    return new Guid(this.properties.GetGuid(0x26));
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "ConnectorType" }));
                }
                return Guid.Empty;
            }
            set
            {
                if (value.Equals(Guid.Empty))
                {
                    this.filter.ConnectorType = false;
                    this.properties.Remove(0x26);
                }
                else
                {
                    this.filter.ConnectorType = true;
                    this.properties.SetGuid(0x26, value.ToByteArray());
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true), MessagingDescription("MsgCorrelationId")]
        public string CorrelationId
        {
            get
            {
                if (this.filter.CorrelationId)
                {
                    return this.IdFromByteArray(this.properties.GetUI1Vector(3));
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "CorrelationId" }));
                }
                return string.Empty;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.filter.CorrelationId = false;
                    this.properties.Remove(3);
                }
                else
                {
                    this.filter.CorrelationId = true;
                    this.properties.SetUI1Vector(3, this.IdToByteArray(value));
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int DefaultBodySize
        {
            get
            {
                return this.filter.DefaultBodySize;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int DefaultExtensionSize
        {
            get
            {
                return this.filter.DefaultExtensionSize;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int DefaultLabelSize
        {
            get
            {
                return this.filter.DefaultLabelSize;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgDestinationQueue")]
        public MessageQueue DestinationQueue
        {
            get
            {
                if (!this.filter.DestinationQueue)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("DestinationQueueNotSet"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "DestinationQueue" }));
                }
                if ((this.cachedDestinationQueue == null) && (this.properties.GetUI4(0x22) != 0))
                {
                    string str = StringFromBytes(this.properties.GetString(0x21), this.properties.GetUI4(0x22));
                    this.cachedDestinationQueue = new MessageQueue("FORMATNAME:" + str);
                }
                return this.cachedDestinationQueue;
            }
        }

        [MessagingDescription("MsgDestinationSymmetricKey"), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] DestinationSymmetricKey
        {
            get
            {
                if (!this.filter.DestinationSymmetricKey)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "DestinationSymmetricKey" }));
                    }
                    return new byte[0];
                }
                byte[] destinationArray = new byte[this.properties.GetUI4(0x2c)];
                Array.Copy(this.properties.GetUI1Vector(0x2b), destinationArray, destinationArray.Length);
                return destinationArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.filter.DestinationSymmetricKey = false;
                    this.properties.Remove(0x2b);
                    this.properties.Remove(0x2c);
                }
                else
                {
                    this.filter.DestinationSymmetricKey = true;
                    this.properties.SetUI1Vector(0x2b, value);
                    this.properties.SetUI4(0x2c, value.Length);
                }
            }
        }

        [MessagingDescription("MsgDigitalSignature"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true)]
        public byte[] DigitalSignature
        {
            get
            {
                if (!this.filter.DigitalSignature)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "DigitalSignature" }));
                    }
                    return new byte[0];
                }
                byte[] destinationArray = new byte[this.properties.GetUI4(0x2e)];
                Array.Copy(this.properties.GetUI1Vector(0x2d), destinationArray, destinationArray.Length);
                return destinationArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.filter.DigitalSignature = false;
                    this.properties.Remove(0x2d);
                    this.properties.Remove(0x2e);
                }
                else
                {
                    this.filter.DigitalSignature = true;
                    this.filter.UseAuthentication = true;
                    this.properties.SetUI1Vector(0x2d, value);
                    this.properties.SetUI4(0x2e, value.Length);
                }
            }
        }

        [ReadOnly(true), MessagingDescription("MsgEncryptionAlgorithm"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Messaging.EncryptionAlgorithm EncryptionAlgorithm
        {
            get
            {
                if (this.filter.EncryptionAlgorithm)
                {
                    return (System.Messaging.EncryptionAlgorithm) this.properties.GetUI4(0x1b);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "EncryptionAlgorithm" }));
                }
                return System.Messaging.EncryptionAlgorithm.Rc2;
            }
            set
            {
                if (!ValidationUtility.ValidateEncryptionAlgorithm(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Messaging.EncryptionAlgorithm));
                }
                this.filter.EncryptionAlgorithm = true;
                this.properties.SetUI4(0x1b, (int) value);
            }
        }

        [MessagingDescription("MsgExtension"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true)]
        public byte[] Extension
        {
            get
            {
                if (!this.filter.Extension)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Extension" }));
                    }
                    return new byte[0];
                }
                byte[] destinationArray = new byte[this.properties.GetUI4(0x24)];
                Array.Copy(this.properties.GetUI1Vector(0x23), destinationArray, destinationArray.Length);
                return destinationArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.filter.Extension = false;
                    this.properties.Remove(0x23);
                    this.properties.Remove(0x24);
                }
                else
                {
                    this.filter.Extension = true;
                    this.properties.SetUI1Vector(0x23, value);
                    this.properties.SetUI4(0x24, value.Length);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IMessageFormatter Formatter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cachedFormatter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.cachedFormatter = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true), MessagingDescription("MsgHashAlgorithm")]
        public System.Messaging.HashAlgorithm HashAlgorithm
        {
            get
            {
                if (this.filter.HashAlgorithm)
                {
                    return (System.Messaging.HashAlgorithm) this.properties.GetUI4(0x1a);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "HashAlgorithm" }));
                }
                return System.Messaging.HashAlgorithm.Md5;
            }
            set
            {
                if (!ValidationUtility.ValidateHashAlgorithm(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Messaging.HashAlgorithm));
                }
                this.filter.HashAlgorithm = true;
                this.properties.SetUI4(0x1a, (int) value);
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgId")]
        public string Id
        {
            get
            {
                if (this.filter.Id)
                {
                    return this.IdFromByteArray(this.properties.GetUI1Vector(2));
                }
                if (!this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("IdNotSet"));
                }
                throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Id" }));
            }
        }

        [MessagingDescription("MsgIsFirstInTransaction"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFirstInTransaction
        {
            get
            {
                if (this.filter.IsFirstInTransaction)
                {
                    return (this.properties.GetUI1(50) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "IsFirstInTransaction" }));
                }
                return false;
            }
        }

        [MessagingDescription("MsgIsLastInTransaction"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsLastInTransaction
        {
            get
            {
                if (this.filter.IsLastInTransaction)
                {
                    return (this.properties.GetUI1(0x33) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "IsLastInTransaction" }));
                }
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true), MessagingDescription("MsgLabel")]
        public string Label
        {
            get
            {
                if (!this.filter.Label)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Label" }));
                    }
                    return string.Empty;
                }
                if (this.properties.GetUI4(12) != 0)
                {
                    return StringFromBytes(this.properties.GetString(11), this.properties.GetUI4(12));
                }
                return "";
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.filter.Label = false;
                    this.properties.Remove(11);
                    this.properties.Remove(12);
                }
                else
                {
                    this.filter.Label = true;
                    this.properties.SetString(11, StringToBytes(value));
                    this.properties.SetUI4(12, value.Length);
                }
            }
        }

        public long LookupId
        {
            get
            {
                if (!MessageQueue.Msmq3OrNewer)
                {
                    throw new PlatformNotSupportedException(Res.GetString("PlatformNotSupported"));
                }
                if (this.filter.LookupId)
                {
                    return this.properties.GetUI8(60);
                }
                if (!this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("LookupIdNotSet"));
                }
                throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "LookupId" }));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgMessageType")]
        public System.Messaging.MessageType MessageType
        {
            get
            {
                if (!this.filter.MessageType)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MessageTypeNotSet"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "MessageType" }));
                }
                switch (this.properties.GetUI2(1))
                {
                    case 0:
                        return System.Messaging.MessageType.Normal;

                    case 1:
                        return System.Messaging.MessageType.Report;
                }
                return System.Messaging.MessageType.Acknowledgment;
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgPriority")]
        public MessagePriority Priority
        {
            get
            {
                if (this.filter.Priority)
                {
                    return (MessagePriority) this.properties.GetUI1(4);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Priority" }));
                }
                return MessagePriority.Normal;
            }
            set
            {
                if (!ValidationUtility.ValidateMessagePriority(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(MessagePriority));
                }
                if (value == MessagePriority.Normal)
                {
                    this.filter.Priority = false;
                    this.properties.Remove(4);
                }
                else
                {
                    this.filter.Priority = true;
                    this.properties.SetUI1(4, (byte) value);
                }
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgRecoverable")]
        public bool Recoverable
        {
            get
            {
                if (this.filter.Recoverable)
                {
                    return (this.properties.GetUI1(5) == 1);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "Recoverable" }));
                }
                return false;
            }
            set
            {
                if (!value)
                {
                    this.filter.Recoverable = false;
                    this.properties.Remove(5);
                }
                else
                {
                    this.filter.Recoverable = true;
                    this.properties.SetUI1(5, 1);
                }
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgResponseQueue")]
        public MessageQueue ResponseQueue
        {
            get
            {
                if (!this.filter.ResponseQueue)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "ResponseQueue" }));
                    }
                    return null;
                }
                if ((this.cachedResponseQueue == null) && (this.properties.GetUI4(0x10) != 0))
                {
                    string str = StringFromBytes(this.properties.GetString(15), this.properties.GetUI4(0x10));
                    this.cachedResponseQueue = new MessageQueue("FORMATNAME:" + str);
                }
                return this.cachedResponseQueue;
            }
            set
            {
                if (value != null)
                {
                    this.filter.ResponseQueue = true;
                }
                else if (this.filter.ResponseQueue)
                {
                    this.filter.ResponseQueue = false;
                    this.properties.Remove(15);
                    this.properties.Remove(0x10);
                }
                this.cachedResponseQueue = value;
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Messaging.SecurityContext SecurityContext
        {
            get
            {
                if (!this.filter.SecurityContext)
                {
                    return null;
                }
                IntPtr existingHandle = (IntPtr) this.properties.GetUI4(0x25);
                return new System.Messaging.SecurityContext(new SecurityContextHandle(existingHandle));
            }
            set
            {
                if (value == null)
                {
                    this.filter.SecurityContext = false;
                    this.properties.Remove(0x25);
                }
                else
                {
                    this.filter.SecurityContext = true;
                    int num = value.Handle.DangerousGetHandle().ToInt32();
                    this.properties.SetUI4(0x25, num);
                }
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgSenderCertificate")]
        public byte[] SenderCertificate
        {
            get
            {
                if (!this.filter.SenderCertificate)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "SenderCertificate" }));
                    }
                    return new byte[0];
                }
                byte[] destinationArray = new byte[this.properties.GetUI4(0x1d)];
                Array.Copy(this.properties.GetUI1Vector(0x1c), destinationArray, destinationArray.Length);
                return destinationArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.filter.SenderCertificate = false;
                    this.properties.Remove(0x1c);
                    this.properties.Remove(0x1d);
                }
                else
                {
                    this.filter.SenderCertificate = true;
                    this.properties.SetUI1Vector(0x1c, value);
                    this.properties.SetUI4(0x1d, value.Length);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgSenderId")]
        public byte[] SenderId
        {
            get
            {
                if (!this.filter.SenderId)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("SenderIdNotSet"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "SenderId" }));
                }
                byte[] destinationArray = new byte[this.properties.GetUI4(0x15)];
                Array.Copy(this.properties.GetUI1Vector(20), destinationArray, destinationArray.Length);
                return destinationArray;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true), MessagingDescription("MsgSenderVersion")]
        public long SenderVersion
        {
            get
            {
                if (this.filter.SenderVersion)
                {
                    return (long) ((ulong) this.properties.GetUI4(0x13));
                }
                if (!this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("VersionNotSet"));
                }
                throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "SenderVersion" }));
            }
        }

        [MessagingDescription("MsgSentTime"), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SentTime
        {
            get
            {
                if (!this.filter.SentTime)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("SentTimeNotSet"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "SentTime" }));
                }
                DateTime time = new DateTime(0x7b2, 1, 1);
                return time.AddSeconds((double) this.properties.GetUI4(0x1f)).ToLocalTime();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgSourceMachine")]
        public string SourceMachine
        {
            get
            {
                if (!this.filter.SourceMachine)
                {
                    if (!this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("SourceMachineNotSet"));
                    }
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "SourceMachine" }));
                }
                if (this.machineName == null)
                {
                    GCHandle handle = GCHandle.Alloc(this.properties.GetGuid(30), GCHandleType.Pinned);
                    MachinePropertyVariants variants = new MachinePropertyVariants();
                    variants.SetNull(0xcb);
                    int num = UnsafeNativeMethods.MQGetMachineProperties(null, handle.AddrOfPinnedObject(), variants.Lock());
                    variants.Unlock();
                    handle.Free();
                    IntPtr intPtr = variants.GetIntPtr(0xcb);
                    if (intPtr != IntPtr.Zero)
                    {
                        this.machineName = Marshal.PtrToStringUni(intPtr);
                        SafeNativeMethods.MQFreeMemory(intPtr);
                    }
                    if (MessageQueue.IsFatalError(num))
                    {
                        throw new MessageQueueException(num);
                    }
                }
                return this.machineName;
            }
        }

        [MessagingDescription("MsgTimeToBeReceived"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(TimeoutConverter)), ReadOnly(true)]
        public TimeSpan TimeToBeReceived
        {
            get
            {
                if (this.filter.TimeToBeReceived)
                {
                    return TimeSpan.FromSeconds((double) this.properties.GetUI4(14));
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "TimeToBeReceived" }));
                }
                return InfiniteTimeout;
            }
            set
            {
                long totalSeconds = (long) value.TotalSeconds;
                if (totalSeconds < 0L)
                {
                    throw new ArgumentException(Res.GetString("InvalidProperty", new object[] { "TimeToBeReceived", value.ToString() }));
                }
                if (totalSeconds > 0xffffffffL)
                {
                    totalSeconds = 0xffffffffL;
                }
                if (totalSeconds == 0xffffffffL)
                {
                    this.filter.TimeToBeReceived = false;
                    this.properties.Remove(14);
                }
                else
                {
                    this.filter.TimeToBeReceived = true;
                    this.properties.SetUI4(14, (int) ((uint) totalSeconds));
                }
            }
        }

        [ReadOnly(true), MessagingDescription("MsgTimeToReachQueue"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(TimeoutConverter))]
        public TimeSpan TimeToReachQueue
        {
            get
            {
                if (this.filter.TimeToReachQueue)
                {
                    return TimeSpan.FromSeconds((double) this.properties.GetUI4(13));
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "TimeToReachQueue" }));
                }
                return InfiniteTimeout;
            }
            set
            {
                long totalSeconds = (long) value.TotalSeconds;
                if (totalSeconds < 0L)
                {
                    throw new ArgumentException(Res.GetString("InvalidProperty", new object[] { "TimeToReachQueue", value.ToString() }));
                }
                if (totalSeconds > 0xffffffffL)
                {
                    totalSeconds = 0xffffffffL;
                }
                if (totalSeconds == 0xffffffffL)
                {
                    this.filter.TimeToReachQueue = false;
                    this.properties.Remove(13);
                }
                else
                {
                    this.filter.TimeToReachQueue = true;
                    this.properties.SetUI4(13, (int) ((uint) totalSeconds));
                }
            }
        }

        [MessagingDescription("MsgTransactionId"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TransactionId
        {
            get
            {
                if (this.filter.TransactionId)
                {
                    return this.IdFromByteArray(this.properties.GetUI1Vector(0x34));
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "TransactionId" }));
                }
                return string.Empty;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgTransactionStatusQueue"), ReadOnly(true)]
        public MessageQueue TransactionStatusQueue
        {
            get
            {
                if (!this.filter.TransactionStatusQueue)
                {
                    if (this.receiveCreated)
                    {
                        throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "TransactionStatusQueue" }));
                    }
                    return null;
                }
                if ((this.cachedTransactionStatusQueue == null) && (this.properties.GetUI4(40) != 0))
                {
                    string str = StringFromBytes(this.properties.GetString(0x27), this.properties.GetUI4(40));
                    this.cachedTransactionStatusQueue = new MessageQueue("FORMATNAME:" + str);
                }
                return this.cachedTransactionStatusQueue;
            }
            set
            {
                if (value != null)
                {
                    this.filter.TransactionStatusQueue = true;
                }
                else if (this.filter.TransactionStatusQueue)
                {
                    this.filter.TransactionStatusQueue = false;
                    this.properties.Remove(0x27);
                    this.properties.Remove(40);
                }
                this.cachedTransactionStatusQueue = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true), MessagingDescription("MsgUseAuthentication")]
        public bool UseAuthentication
        {
            get
            {
                if (this.filter.UseAuthentication)
                {
                    return (this.properties.GetUI4(0x18) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "UseAuthentication" }));
                }
                return false;
            }
            set
            {
                this.filter.UseAuthentication = true;
                if (!value)
                {
                    this.properties.SetUI4(0x18, 0);
                }
                else
                {
                    this.properties.SetUI4(0x18, 1);
                }
            }
        }

        [MessagingDescription("MsgUseDeadLetterQueue"), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseDeadLetterQueue
        {
            get
            {
                if (this.filter.UseDeadLetterQueue)
                {
                    return ((this.properties.GetUI1(7) & 1) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "UseDeadLetterQueue" }));
                }
                return false;
            }
            set
            {
                if (!value)
                {
                    if (this.filter.UseDeadLetterQueue)
                    {
                        this.filter.UseDeadLetterQueue = false;
                        if (!this.filter.UseJournalQueue)
                        {
                            this.properties.Remove(7);
                        }
                        else
                        {
                            this.properties.SetUI1(7, (byte) (this.properties.GetUI1(7) & -2));
                        }
                    }
                }
                else
                {
                    if (!this.filter.UseDeadLetterQueue && !this.filter.UseJournalQueue)
                    {
                        this.properties.SetUI1(7, 1);
                    }
                    else
                    {
                        this.properties.SetUI1(7, (byte) (this.properties.GetUI1(7) | 1));
                    }
                    this.filter.UseDeadLetterQueue = true;
                }
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgUseEncryption")]
        public bool UseEncryption
        {
            get
            {
                if (this.filter.UseEncryption)
                {
                    return (this.properties.GetUI4(0x17) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "UseEncryption" }));
                }
                return false;
            }
            set
            {
                if (!value)
                {
                    this.filter.UseEncryption = false;
                    this.properties.Remove(0x17);
                }
                else
                {
                    this.filter.UseEncryption = true;
                    this.properties.SetUI4(0x17, 1);
                }
            }
        }

        [MessagingDescription("MsgUseJournalQueue"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true)]
        public bool UseJournalQueue
        {
            get
            {
                if (this.filter.UseJournalQueue)
                {
                    return ((this.properties.GetUI1(7) & 2) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "UseJournalQueue" }));
                }
                return false;
            }
            set
            {
                if (!value)
                {
                    if (this.filter.UseJournalQueue)
                    {
                        this.filter.UseJournalQueue = false;
                        if (!this.filter.UseDeadLetterQueue)
                        {
                            this.properties.Remove(7);
                        }
                        else
                        {
                            this.properties.SetUI1(7, (byte) (this.properties.GetUI1(7) & -3));
                        }
                    }
                }
                else
                {
                    if (!this.filter.UseDeadLetterQueue && !this.filter.UseJournalQueue)
                    {
                        this.properties.SetUI1(7, 2);
                    }
                    else
                    {
                        this.properties.SetUI1(7, (byte) (this.properties.GetUI1(7) | 2));
                    }
                    this.filter.UseJournalQueue = true;
                }
            }
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MsgUseTracing")]
        public bool UseTracing
        {
            get
            {
                if (this.filter.UseTracing)
                {
                    return (this.properties.GetUI1(0x29) != 0);
                }
                if (this.receiveCreated)
                {
                    throw new InvalidOperationException(Res.GetString("MissingProperty", new object[] { "UseTracing" }));
                }
                return false;
            }
            set
            {
                if (!value)
                {
                    this.filter.UseTracing = false;
                    this.properties.Remove(0x29);
                }
                else
                {
                    this.filter.UseTracing = true;
                    if (!value)
                    {
                        this.properties.SetUI1(0x29, 0);
                    }
                    else
                    {
                        this.properties.SetUI1(0x29, 1);
                    }
                }
            }
        }
    }
}

