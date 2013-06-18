namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Messaging.Design;
    using System.Messaging.Interop;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    [Editor("System.Messaging.Design.QueuePathEditor", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MessagingDescription("MessageQueueDesc"), DefaultEvent("ReceiveCompleted"), TypeConverter(typeof(MessageQueueConverter)), InstallerType(typeof(MessageQueueInstaller))]
    public class MessageQueue : Component, IEnumerable
    {
        private QueueAccessMode accessMode;
        private bool administerGranted;
        private bool attached;
        private bool authenticate;
        private short basePriority;
        private bool browseGranted;
        private static string computerName;
        private DateTime createTime;
        private System.Messaging.DefaultPropertiesToSend defaultProperties;
        private bool enableCache;
        private static bool enableConnectionCache = false;
        private int encryptionLevel;
        private QueuePropertyFilter filter;
        private string formatName;
        private static CacheTable<string, string> formatNameCache = new CacheTable<string, string>("formatNameCache", 4, new TimeSpan(0, 0, 100));
        private IMessageFormatter formatter;
        private Guid id;
        public static readonly long InfiniteQueueSize = 0xffffffffL;
        public static readonly TimeSpan InfiniteTimeout = TimeSpan.FromMilliseconds(4294967295);
        private long journalSize;
        private string label;
        private DateTime lastModifyTime;
        private MQCacheableInfo mqInfo;
        internal static readonly bool Msmq3OrNewer = (OSVersion >= WinXP);
        private string multicastAddress;
        private PeekCompletedEventHandler onPeekCompleted;
        private ReceiveCompletedEventHandler onReceiveCompleted;
        private AsyncCallback onRequestCompleted;
        internal static readonly Version OSVersion = Environment.OSVersion.Version;
        private Hashtable outstandingAsyncRequests;
        private string path;
        private bool peekGranted;
        private static readonly string PREFIX_FORMAT_NAME = "FORMATNAME:";
        private static readonly string PREFIX_LABEL = "LABEL:";
        private QueuePropertyVariants properties;
        private static CacheTable<QueueInfoKeyHolder, MQCacheableInfo> queueInfoCache = new CacheTable<QueueInfoKeyHolder, MQCacheableInfo>("queue info", 4, new TimeSpan(0, 0, 100));
        private QueueInfoKeyHolder queueInfoKey;
        private string queuePath;
        private long queueSize;
        private Guid queueType;
        private MessagePropertyFilter receiveFilter;
        private bool receiveGranted;
        private bool sendGranted;
        private int sharedMode;
        private static object staticSyncRoot = new object();
        private static readonly string SUFIX_DEADLETTER = @"\DEADLETTER$";
        private static readonly string SUFIX_DEADXACT = @"\XACTDEADLETTER$";
        private static readonly string SUFIX_JOURNAL = @"\JOURNAL$";
        private static readonly string SUFIX_PRIVATE = @"\PRIVATE$";
        private ISynchronizeInvoke synchronizingObject;
        private object syncRoot;
        private bool useJournaling;
        private bool useThreadPool;
        internal static readonly Version WinXP = new Version(5, 1);

        [MessagingDescription("MQ_PeekCompleted")]
        public event PeekCompletedEventHandler PeekCompleted
        {
            add
            {
                if (!this.peekGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Peek, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.peekGranted = true;
                }
                this.onPeekCompleted = (PeekCompletedEventHandler) Delegate.Combine(this.onPeekCompleted, value);
            }
            remove
            {
                this.onPeekCompleted = (PeekCompletedEventHandler) Delegate.Remove(this.onPeekCompleted, value);
            }
        }

        [MessagingDescription("MQ_ReceiveCompleted")]
        public event ReceiveCompletedEventHandler ReceiveCompleted
        {
            add
            {
                if (!this.receiveGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Receive, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.receiveGranted = true;
                }
                this.onReceiveCompleted = (ReceiveCompletedEventHandler) Delegate.Combine(this.onReceiveCompleted, value);
            }
            remove
            {
                this.onReceiveCompleted = (ReceiveCompletedEventHandler) Delegate.Remove(this.onReceiveCompleted, value);
            }
        }

        public MessageQueue()
        {
            this.syncRoot = new object();
            this.path = string.Empty;
            this.accessMode = QueueAccessMode.SendAndReceive;
        }

        public MessageQueue(string path) : this(path, false, enableConnectionCache)
        {
        }

        public MessageQueue(string path, bool sharedModeDenyReceive) : this(path, sharedModeDenyReceive, enableConnectionCache)
        {
        }

        internal MessageQueue(string path, Guid id)
        {
            this.syncRoot = new object();
            this.PropertyFilter.Id = true;
            this.id = id;
            this.path = path;
            this.accessMode = QueueAccessMode.SendAndReceive;
        }

        public MessageQueue(string path, QueueAccessMode accessMode) : this(path, false, enableConnectionCache, accessMode)
        {
        }

        public MessageQueue(string path, bool sharedModeDenyReceive, bool enableCache)
        {
            this.syncRoot = new object();
            this.path = path;
            this.enableCache = enableCache;
            if (sharedModeDenyReceive)
            {
                this.sharedMode = 1;
            }
            this.accessMode = QueueAccessMode.SendAndReceive;
        }

        public MessageQueue(string path, bool sharedModeDenyReceive, bool enableCache, QueueAccessMode accessMode)
        {
            this.syncRoot = new object();
            this.path = path;
            this.enableCache = enableCache;
            if (sharedModeDenyReceive)
            {
                this.sharedMode = 1;
            }
            this.SetAccessMode(accessMode);
        }

        public IAsyncResult BeginPeek()
        {
            return this.ReceiveAsync(InfiniteTimeout, CursorHandle.NullHandle, -2147483648, null, null);
        }

        public IAsyncResult BeginPeek(TimeSpan timeout)
        {
            return this.ReceiveAsync(timeout, CursorHandle.NullHandle, -2147483648, null, null);
        }

        public IAsyncResult BeginPeek(TimeSpan timeout, object stateObject)
        {
            return this.ReceiveAsync(timeout, CursorHandle.NullHandle, -2147483648, null, stateObject);
        }

        public IAsyncResult BeginPeek(TimeSpan timeout, object stateObject, AsyncCallback callback)
        {
            return this.ReceiveAsync(timeout, CursorHandle.NullHandle, -2147483648, callback, stateObject);
        }

        public IAsyncResult BeginPeek(TimeSpan timeout, Cursor cursor, PeekAction action, object state, AsyncCallback callback)
        {
            if ((action != PeekAction.Current) && (action != PeekAction.Next))
            {
                throw new ArgumentOutOfRangeException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "action", action.ToString() }));
            }
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return this.ReceiveAsync(timeout, cursor.Handle, (int) action, callback, state);
        }

        public IAsyncResult BeginReceive()
        {
            return this.ReceiveAsync(InfiniteTimeout, CursorHandle.NullHandle, 0, null, null);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout)
        {
            return this.ReceiveAsync(timeout, CursorHandle.NullHandle, 0, null, null);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, object stateObject)
        {
            return this.ReceiveAsync(timeout, CursorHandle.NullHandle, 0, null, stateObject);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, object stateObject, AsyncCallback callback)
        {
            return this.ReceiveAsync(timeout, CursorHandle.NullHandle, 0, callback, stateObject);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, Cursor cursor, object state, AsyncCallback callback)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return this.ReceiveAsync(timeout, cursor.Handle, 0, callback, state);
        }

        private void Cleanup(bool disposing)
        {
            this.formatName = null;
            this.queuePath = null;
            this.attached = false;
            this.administerGranted = false;
            this.browseGranted = false;
            this.sendGranted = false;
            this.receiveGranted = false;
            this.peekGranted = false;
            if (disposing && (this.mqInfo != null))
            {
                this.mqInfo.Release();
                if ((this.sharedMode == 1) || !this.enableCache)
                {
                    this.mqInfo.Dispose();
                }
                this.mqInfo = null;
            }
        }

        public static void ClearConnectionCache()
        {
            formatNameCache.ClearStale(new TimeSpan(0L));
            queueInfoCache.ClearStale(new TimeSpan(0L));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Close()
        {
            this.Cleanup(true);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static MessageQueue Create(string path)
        {
            return Create(path, false);
        }

        public static MessageQueue Create(string path, bool transactional)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "path", path }));
            }
            if (!IsCanonicalPath(path, true))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidQueuePathToCreate", new object[] { path }));
            }
            new MessageQueuePermission(MessageQueuePermissionAccess.Administer, "*").Demand();
            QueuePropertyVariants variants = new QueuePropertyVariants();
            variants.SetString(0x67, Message.StringToBytes(path));
            if (transactional)
            {
                variants.SetUI1(0x71, 1);
            }
            else
            {
                variants.SetUI1(0x71, 0);
            }
            StringBuilder formatName = new StringBuilder(0x7c);
            int formatNameLength = 0x7c;
            int num2 = 0;
            num2 = System.Messaging.Interop.UnsafeNativeMethods.MQCreateQueue(IntPtr.Zero, variants.Lock(), formatName, ref formatNameLength);
            variants.Unlock();
            if (IsFatalError(num2))
            {
                throw new MessageQueueException(num2);
            }
            return new MessageQueue(path);
        }

        public Cursor CreateCursor()
        {
            return new Cursor(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static MessageQueue[] CreateMessageQueuesSnapshot(MessageQueueCriteria criteria)
        {
            return CreateMessageQueuesSnapshot(criteria, true);
        }

        private static MessageQueue[] CreateMessageQueuesSnapshot(MessageQueueCriteria criteria, bool checkSecurity)
        {
            ArrayList list = new ArrayList();
            IEnumerator messageQueueEnumerator = GetMessageQueueEnumerator(criteria, checkSecurity);
            while (messageQueueEnumerator.MoveNext())
            {
                MessageQueue current = (MessageQueue) messageQueueEnumerator.Current;
                list.Add(current);
            }
            MessageQueue[] array = new MessageQueue[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public static void Delete(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "path", path }));
            }
            if (!ValidatePath(path, false))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("PathSyntax"));
            }
            int num = 0;
            MessageQueue queue = new MessageQueue(path);
            new MessageQueuePermission(MessageQueuePermissionAccess.Administer, PREFIX_FORMAT_NAME + queue.FormatName).Demand();
            num = System.Messaging.Interop.UnsafeNativeMethods.MQDeleteQueue(queue.FormatName);
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
            queueInfoCache.Remove(queue.QueueInfoKey);
            formatNameCache.Remove(path.ToUpper(CultureInfo.InvariantCulture));
        }

        [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
        protected override void Dispose(bool disposing)
        {
            this.Cleanup(disposing);
            base.Dispose(disposing);
        }

        private Message EndAsyncOperation(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!(asyncResult is AsynchronousRequest))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("AsyncResultInvalid"));
            }
            AsynchronousRequest request = (AsynchronousRequest) asyncResult;
            return request.End();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message EndPeek(IAsyncResult asyncResult)
        {
            return this.EndAsyncOperation(asyncResult);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message EndReceive(IAsyncResult asyncResult)
        {
            return this.EndAsyncOperation(asyncResult);
        }

        public static bool Exists(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (!ValidatePath(path, false))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("PathSyntax"));
            }
            new MessageQueuePermission(MessageQueuePermissionAccess.Browse, "*").Demand();
            string str = path.ToUpper(CultureInfo.InvariantCulture);
            if (str.StartsWith(PREFIX_FORMAT_NAME))
            {
                throw new InvalidOperationException(System.Messaging.Res.GetString("QueueExistsError"));
            }
            if (str.StartsWith(PREFIX_LABEL))
            {
                if (ResolveQueueFromLabel(path, false) == null)
                {
                    return false;
                }
                return true;
            }
            if (ResolveFormatNameFromQueuePath(path, false) == null)
            {
                return false;
            }
            return true;
        }

        private void GenerateQueueProperties()
        {
            if (!this.browseGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Browse, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.browseGranted = true;
            }
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQGetQueueProperties(this.FormatName, this.Properties.Lock());
            this.Properties.Unlock();
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
        }

        public Message[] GetAllMessages()
        {
            ArrayList list = new ArrayList();
            MessageEnumerator enumerator = this.GetMessageEnumerator2();
            while (enumerator.MoveNext())
            {
                Message current = enumerator.Current;
                list.Add(current);
            }
            Message[] array = new Message[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        [Obsolete("This method returns a MessageEnumerator that implements RemoveCurrent family of methods incorrectly. Please use GetMessageEnumerator2 instead."), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerator GetEnumerator()
        {
            return this.GetMessageEnumerator();
        }

        public static Guid GetMachineId(string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "MachineName", machineName }));
            }
            if (machineName == ".")
            {
                machineName = ComputerName;
            }
            new MessageQueuePermission(MessageQueuePermissionAccess.Browse, "*").Demand();
            MachinePropertyVariants variants = new MachinePropertyVariants();
            byte[] destination = new byte[0x10];
            variants.SetNull(0xca);
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQGetMachineProperties(machineName, IntPtr.Zero, variants.Lock());
            variants.Unlock();
            IntPtr intPtr = variants.GetIntPtr(0xca);
            if (IsFatalError(num))
            {
                if (intPtr != IntPtr.Zero)
                {
                    SafeNativeMethods.MQFreeMemory(intPtr);
                }
                throw new MessageQueueException(num);
            }
            if (intPtr != IntPtr.Zero)
            {
                Marshal.Copy(intPtr, destination, 0, 0x10);
                SafeNativeMethods.MQFreeMemory(intPtr);
            }
            return new Guid(destination);
        }

        [Obsolete("This method returns a MessageEnumerator that implements RemoveCurrent family of methods incorrectly. Please use GetMessageEnumerator2 instead.")]
        public MessageEnumerator GetMessageEnumerator()
        {
            if (!this.peekGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Peek, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.peekGranted = true;
            }
            return new MessageEnumerator(this, false);
        }

        public MessageEnumerator GetMessageEnumerator2()
        {
            if (!this.peekGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Peek, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.peekGranted = true;
            }
            return new MessageEnumerator(this, true);
        }

        public static MessageQueueEnumerator GetMessageQueueEnumerator()
        {
            return new MessageQueueEnumerator(null);
        }

        public static MessageQueueEnumerator GetMessageQueueEnumerator(MessageQueueCriteria criteria)
        {
            return new MessageQueueEnumerator(criteria);
        }

        internal static MessageQueueEnumerator GetMessageQueueEnumerator(MessageQueueCriteria criteria, bool checkSecurity)
        {
            return new MessageQueueEnumerator(criteria, checkSecurity);
        }

        public static MessageQueue[] GetPrivateQueuesByMachine(string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "MachineName", machineName }));
            }
            new MessageQueuePermission(MessageQueuePermissionAccess.Browse, "*").Demand();
            if ((machineName == ".") || (string.Compare(machineName, ComputerName, true, CultureInfo.InvariantCulture) == 0))
            {
                machineName = null;
            }
            MessagePropertyVariants variants = new MessagePropertyVariants(5, 0);
            variants.SetNull(2);
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQMgmtGetInfo(machineName, "MACHINE", variants.Lock());
            variants.Unlock();
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
            uint stringVectorLength = variants.GetStringVectorLength(2);
            IntPtr stringVectorBasePointer = variants.GetStringVectorBasePointer(2);
            MessageQueue[] queueArray = new MessageQueue[stringVectorLength];
            for (int i = 0; i < stringVectorLength; i++)
            {
                IntPtr ptr = Marshal.ReadIntPtr((IntPtr) (((long) stringVectorBasePointer) + (i * IntPtr.Size)));
                string str = Marshal.PtrToStringUni(ptr);
                queueArray[i] = new MessageQueue("FormatName:DIRECT=OS:" + str);
                queueArray[i].queuePath = str;
                SafeNativeMethods.MQFreeMemory(ptr);
            }
            SafeNativeMethods.MQFreeMemory(stringVectorBasePointer);
            return queueArray;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static MessageQueue[] GetPublicQueues()
        {
            return CreateMessageQueuesSnapshot(null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static MessageQueue[] GetPublicQueues(MessageQueueCriteria criteria)
        {
            return CreateMessageQueuesSnapshot(criteria);
        }

        public static MessageQueue[] GetPublicQueuesByCategory(Guid category)
        {
            MessageQueueCriteria criteria = new MessageQueueCriteria {
                Category = category
            };
            return CreateMessageQueuesSnapshot(criteria);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static MessageQueue[] GetPublicQueuesByLabel(string label)
        {
            return GetPublicQueuesByLabel(label, true);
        }

        private static MessageQueue[] GetPublicQueuesByLabel(string label, bool checkSecurity)
        {
            MessageQueueCriteria criteria = new MessageQueueCriteria {
                Label = label
            };
            return CreateMessageQueuesSnapshot(criteria, checkSecurity);
        }

        public static MessageQueue[] GetPublicQueuesByMachine(string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "MachineName", machineName }));
            }
            new MessageQueuePermission(MessageQueuePermissionAccess.Browse, "*").Demand();
            try
            {
                new DirectoryServicesPermission(PermissionState.Unrestricted).Assert();
                SearchResult result = new DirectorySearcher(string.Format(CultureInfo.InvariantCulture, "(&(CN={0})(objectCategory=Computer))", new object[] { ComputerName })).FindOne();
                if (result != null)
                {
                    SearchResult result2 = new DirectorySearcher(result.GetDirectoryEntry()) { Filter = "(CN=msmq)" }.FindOne();
                    SearchResult result3 = null;
                    if (result2 != null)
                    {
                        if ((machineName != ".") && (string.Compare(machineName, ComputerName, true, CultureInfo.InvariantCulture) != 0))
                        {
                            SearchResult result4 = new DirectorySearcher(string.Format(CultureInfo.InvariantCulture, "(&(CN={0})(objectCategory=Computer))", new object[] { machineName })).FindOne();
                            if (result4 == null)
                            {
                                return new MessageQueue[0];
                            }
                            result3 = new DirectorySearcher(result4.GetDirectoryEntry()) { Filter = "(CN=msmq)" }.FindOne();
                            if (result3 == null)
                            {
                                return new MessageQueue[0];
                            }
                        }
                        else
                        {
                            result3 = result2;
                        }
                        DirectorySearcher searcher5 = new DirectorySearcher(result3.GetDirectoryEntry()) {
                            Filter = "(objectClass=mSMQQueue)"
                        };
                        searcher5.PropertiesToLoad.Add("Name");
                        SearchResultCollection results = searcher5.FindAll();
                        MessageQueue[] queueArray = new MessageQueue[results.Count];
                        for (int i = 0; i < queueArray.Length; i++)
                        {
                            string str = (string) results[i].Properties["Name"][0];
                            queueArray[i] = new MessageQueue(string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { machineName, str }));
                        }
                        return queueArray;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            MessageQueueCriteria criteria = new MessageQueueCriteria {
                MachineName = machineName
            };
            return CreateMessageQueuesSnapshot(criteria, false);
        }

        public static System.Messaging.SecurityContext GetSecurityContext()
        {
            SecurityContextHandle handle;
            int num = System.Messaging.Interop.NativeMethods.MQGetSecurityContextEx(out handle);
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
            return new System.Messaging.SecurityContext(handle);
        }

        internal Message InternalReceiveByLookupId(bool receive, MessageLookupAction lookupAction, long lookupId, MessageQueueTransaction internalTransaction, MessageQueueTransactionType transactionType)
        {
            int num;
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            if (!ValidationUtility.ValidateMessageLookupAction(lookupAction))
            {
                throw new InvalidEnumArgumentException("action", (int) lookupAction, typeof(MessageLookupAction));
            }
            if (!Msmq3OrNewer)
            {
                throw new PlatformNotSupportedException(System.Messaging.Res.GetString("PlatformNotSupported"));
            }
            if (receive)
            {
                if (!this.receiveGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Receive, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.receiveGranted = true;
                }
                num = (int) (((MessageLookupAction) 0x40000020) | lookupAction);
            }
            else
            {
                if (!this.peekGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Peek, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.peekGranted = true;
                }
                num = (int) (((MessageLookupAction) 0x40000010) | lookupAction);
            }
            MessagePropertyFilter messageReadPropertyFilter = this.MessageReadPropertyFilter;
            int num2 = 0;
            Message message = null;
            MessagePropertyVariants.MQPROPS properties = null;
            if (messageReadPropertyFilter != null)
            {
                message = new Message((MessagePropertyFilter) messageReadPropertyFilter.Clone());
                message.SetLookupId(lookupId);
                if (this.formatter != null)
                {
                    message.Formatter = (IMessageFormatter) this.formatter.Clone();
                }
                properties = message.Lock();
            }
            try
            {
                if ((internalTransaction != null) && receive)
                {
                    num2 = this.StaleSafeReceiveByLookupId(lookupId, num, properties, null, null, internalTransaction.BeginQueueOperation());
                }
                else
                {
                    num2 = this.StaleSafeReceiveByLookupId(lookupId, num, properties, null, null, (IntPtr) ((long) transactionType));
                }
                if (message != null)
                {
                    while (IsMemoryError(num2))
                    {
                        message.Unlock();
                        message.AdjustMemory();
                        properties = message.Lock();
                        if ((internalTransaction != null) && receive)
                        {
                            num2 = this.StaleSafeReceiveByLookupId(lookupId, num, properties, null, null, internalTransaction.InnerTransaction);
                        }
                        else
                        {
                            num2 = this.StaleSafeReceiveByLookupId(lookupId, num, properties, null, null, (IntPtr) ((long) transactionType));
                        }
                    }
                    message.Unlock();
                }
            }
            finally
            {
                if ((internalTransaction != null) && receive)
                {
                    internalTransaction.EndQueueOperation();
                }
            }
            if (num2 == -1072824184)
            {
                throw new InvalidOperationException(System.Messaging.Res.GetString("MessageNotFound"));
            }
            if (IsFatalError(num2))
            {
                throw new MessageQueueException(num2);
            }
            return message;
        }

        private static bool IsCanonicalPath(string path, bool checkQueueNameSize)
        {
            if (!ValidatePath(path, checkQueueNameSize))
            {
                return false;
            }
            string str = path.ToUpper(CultureInfo.InvariantCulture);
            return ((!str.StartsWith(PREFIX_LABEL) && !str.StartsWith(PREFIX_FORMAT_NAME)) && ((!str.EndsWith(SUFIX_DEADLETTER) && !str.EndsWith(SUFIX_DEADXACT)) && !str.EndsWith(SUFIX_JOURNAL)));
        }

        private bool IsCashedInfoInvalidOnReceive(int receiveResult)
        {
            if (((receiveResult != -1072824234) && (receiveResult != -1072824313)) && (receiveResult != -1072824314))
            {
                return (receiveResult == -1072824230);
            }
            return true;
        }

        internal static bool IsFatalError(int value)
        {
            bool flag = value == 0;
            return (((value & -1073741824) != 0x40000000) && !flag);
        }

        internal static bool IsMemoryError(int value)
        {
            if ((((value != -1072824294) && (value != -1072824226)) && ((value != -1072824221) && (value != -1072824277))) && ((((value != -1072824286) && (value != -1072824285)) && ((value != -1072824222) && (value != -1072824223))) && ((value != -1072824280) && (value != -1072824289))))
            {
                return false;
            }
            return true;
        }

        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            if (((AsynchronousRequest) asyncResult).Action == -2147483648)
            {
                if (this.onPeekCompleted != null)
                {
                    PeekCompletedEventArgs e = new PeekCompletedEventArgs(this, asyncResult);
                    this.onPeekCompleted(this, e);
                }
            }
            else if (this.onReceiveCompleted != null)
            {
                ReceiveCompletedEventArgs args2 = new ReceiveCompletedEventArgs(this, asyncResult);
                this.onReceiveCompleted(this, args2);
            }
        }

        public Message Peek()
        {
            return this.ReceiveCurrent(InfiniteTimeout, -2147483648, CursorHandle.NullHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
        }

        public Message Peek(TimeSpan timeout)
        {
            return this.ReceiveCurrent(timeout, -2147483648, CursorHandle.NullHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
        }

        public Message Peek(TimeSpan timeout, Cursor cursor, PeekAction action)
        {
            if ((action != PeekAction.Current) && (action != PeekAction.Next))
            {
                throw new ArgumentOutOfRangeException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "action", action.ToString() }));
            }
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return this.ReceiveCurrent(timeout, (int) action, cursor.Handle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
        }

        public Message PeekByCorrelationId(string correlationId)
        {
            return this.ReceiveBy(correlationId, TimeSpan.Zero, false, false, false, null, MessageQueueTransactionType.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message PeekByCorrelationId(string correlationId, TimeSpan timeout)
        {
            return this.ReceiveBy(correlationId, timeout, false, false, true, null, MessageQueueTransactionType.None);
        }

        public Message PeekById(string id)
        {
            return this.ReceiveBy(id, TimeSpan.Zero, false, true, false, null, MessageQueueTransactionType.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message PeekById(string id, TimeSpan timeout)
        {
            return this.ReceiveBy(id, timeout, false, true, true, null, MessageQueueTransactionType.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message PeekByLookupId(long lookupId)
        {
            return this.InternalReceiveByLookupId(false, MessageLookupAction.Current, lookupId, null, MessageQueueTransactionType.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message PeekByLookupId(MessageLookupAction action, long lookupId)
        {
            return this.InternalReceiveByLookupId(false, action, lookupId, null, MessageQueueTransactionType.None);
        }

        public void Purge()
        {
            if (!this.receiveGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Receive, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.receiveGranted = true;
            }
            int num = this.StaleSafePurgeQueue();
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
        }

        public Message Receive()
        {
            return this.ReceiveCurrent(InfiniteTimeout, 0, CursorHandle.NullHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
        }

        public Message Receive(MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.ReceiveCurrent(InfiniteTimeout, 0, CursorHandle.NullHandle, this.MessageReadPropertyFilter, transaction, MessageQueueTransactionType.None);
        }

        public Message Receive(MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.ReceiveCurrent(InfiniteTimeout, 0, CursorHandle.NullHandle, this.MessageReadPropertyFilter, null, transactionType);
        }

        public Message Receive(TimeSpan timeout)
        {
            return this.ReceiveCurrent(timeout, 0, CursorHandle.NullHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
        }

        public Message Receive(TimeSpan timeout, Cursor cursor)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return this.ReceiveCurrent(timeout, 0, cursor.Handle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
        }

        public Message Receive(TimeSpan timeout, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.ReceiveCurrent(timeout, 0, CursorHandle.NullHandle, this.MessageReadPropertyFilter, transaction, MessageQueueTransactionType.None);
        }

        public Message Receive(TimeSpan timeout, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.ReceiveCurrent(timeout, 0, CursorHandle.NullHandle, this.MessageReadPropertyFilter, null, transactionType);
        }

        public Message Receive(TimeSpan timeout, Cursor cursor, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return this.ReceiveCurrent(timeout, 0, cursor.Handle, this.MessageReadPropertyFilter, transaction, MessageQueueTransactionType.None);
        }

        public Message Receive(TimeSpan timeout, Cursor cursor, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }
            return this.ReceiveCurrent(timeout, 0, cursor.Handle, this.MessageReadPropertyFilter, null, transactionType);
        }

        private IAsyncResult ReceiveAsync(TimeSpan timeout, CursorHandle cursorHandle, int action, AsyncCallback callback, object stateObject)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 0xffffffffL))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "timeout", timeout.ToString() }));
            }
            if (action == 0)
            {
                if (!this.receiveGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Receive, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.receiveGranted = true;
                }
            }
            else if (!this.peekGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Peek, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.peekGranted = true;
            }
            if (!this.attached)
            {
                lock (this)
                {
                    if (!this.attached)
                    {
                        int num2;
                        if (!SafeNativeMethods.GetHandleInformation(this.MQInfo.ReadHandle, out num2))
                        {
                            this.useThreadPool = false;
                        }
                        else
                        {
                            this.MQInfo.BindToThreadPool();
                            this.useThreadPool = true;
                        }
                        this.attached = true;
                    }
                }
            }
            if (callback == null)
            {
                if (this.onRequestCompleted == null)
                {
                    this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
                }
                callback = this.onRequestCompleted;
            }
            AsynchronousRequest request = new AsynchronousRequest(this, (uint) totalMilliseconds, cursorHandle, action, this.useThreadPool, stateObject, callback);
            if (!this.useThreadPool)
            {
                this.OutstandingAsyncRequests[request] = request;
            }
            request.BeginRead();
            return request;
        }

        private Message ReceiveBy(string id, TimeSpan timeout, bool remove, bool compareId, bool throwTimeout, MessageQueueTransaction transaction, MessageQueueTransactionType transactionType)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if ((timeout < TimeSpan.Zero) || (timeout > InfiniteTimeout))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "timeout", timeout.ToString() }));
            }
            MessagePropertyFilter receiveFilter = this.receiveFilter;
            CursorHandle cursorHandle = null;
            try
            {
                this.receiveFilter = new MessagePropertyFilter();
                this.receiveFilter.ClearAll();
                if (!compareId)
                {
                    this.receiveFilter.CorrelationId = true;
                }
                else
                {
                    this.receiveFilter.Id = true;
                }
                int num = SafeNativeMethods.MQCreateCursor(this.MQInfo.ReadHandle, out cursorHandle);
                if (IsFatalError(num))
                {
                    throw new MessageQueueException(num);
                }
                try
                {
                    for (Message message = this.ReceiveCurrent(timeout, -2147483648, cursorHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None); message != null; message = this.ReceiveCurrent(timeout, -2147483647, cursorHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None))
                    {
                        if ((compareId && (string.Compare(message.Id, id, true, CultureInfo.InvariantCulture) == 0)) || (!compareId && (string.Compare(message.CorrelationId, id, true, CultureInfo.InvariantCulture) == 0)))
                        {
                            this.receiveFilter = receiveFilter;
                            if (remove)
                            {
                                if (transaction == null)
                                {
                                    return this.ReceiveCurrent(timeout, 0, cursorHandle, this.MessageReadPropertyFilter, null, transactionType);
                                }
                                return this.ReceiveCurrent(timeout, 0, cursorHandle, this.MessageReadPropertyFilter, transaction, MessageQueueTransactionType.None);
                            }
                            return this.ReceiveCurrent(timeout, -2147483648, cursorHandle, this.MessageReadPropertyFilter, null, MessageQueueTransactionType.None);
                        }
                    }
                }
                catch (MessageQueueException)
                {
                }
            }
            finally
            {
                this.receiveFilter = receiveFilter;
                if (cursorHandle != null)
                {
                    cursorHandle.Close();
                }
            }
            if (!throwTimeout)
            {
                throw new InvalidOperationException(System.Messaging.Res.GetString("MessageNotFound"));
            }
            throw new MessageQueueException(-1072824293);
        }

        public Message ReceiveByCorrelationId(string correlationId)
        {
            return this.ReceiveBy(correlationId, TimeSpan.Zero, true, false, false, null, MessageQueueTransactionType.None);
        }

        public Message ReceiveByCorrelationId(string correlationId, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.ReceiveBy(correlationId, TimeSpan.Zero, true, false, false, transaction, MessageQueueTransactionType.None);
        }

        public Message ReceiveByCorrelationId(string correlationId, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.ReceiveBy(correlationId, TimeSpan.Zero, true, false, false, null, transactionType);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message ReceiveByCorrelationId(string correlationId, TimeSpan timeout)
        {
            return this.ReceiveBy(correlationId, timeout, true, false, true, null, MessageQueueTransactionType.None);
        }

        public Message ReceiveByCorrelationId(string correlationId, TimeSpan timeout, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.ReceiveBy(correlationId, timeout, true, false, true, transaction, MessageQueueTransactionType.None);
        }

        public Message ReceiveByCorrelationId(string correlationId, TimeSpan timeout, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.ReceiveBy(correlationId, timeout, true, false, true, null, transactionType);
        }

        public Message ReceiveById(string id)
        {
            return this.ReceiveBy(id, TimeSpan.Zero, true, true, false, null, MessageQueueTransactionType.None);
        }

        public Message ReceiveById(string id, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.ReceiveBy(id, TimeSpan.Zero, true, true, false, transaction, MessageQueueTransactionType.None);
        }

        public Message ReceiveById(string id, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.ReceiveBy(id, TimeSpan.Zero, true, true, false, null, transactionType);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message ReceiveById(string id, TimeSpan timeout)
        {
            return this.ReceiveBy(id, timeout, true, true, true, null, MessageQueueTransactionType.None);
        }

        public Message ReceiveById(string id, TimeSpan timeout, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            return this.ReceiveBy(id, timeout, true, true, true, transaction, MessageQueueTransactionType.None);
        }

        public Message ReceiveById(string id, TimeSpan timeout, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            return this.ReceiveBy(id, timeout, true, true, true, null, transactionType);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message ReceiveByLookupId(long lookupId)
        {
            return this.InternalReceiveByLookupId(true, MessageLookupAction.Current, lookupId, null, MessageQueueTransactionType.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message ReceiveByLookupId(MessageLookupAction action, long lookupId, MessageQueueTransaction transaction)
        {
            return this.InternalReceiveByLookupId(true, action, lookupId, transaction, MessageQueueTransactionType.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Message ReceiveByLookupId(MessageLookupAction action, long lookupId, MessageQueueTransactionType transactionType)
        {
            return this.InternalReceiveByLookupId(true, action, lookupId, null, transactionType);
        }

        internal Message ReceiveCurrent(TimeSpan timeout, int action, CursorHandle cursor, MessagePropertyFilter filter, MessageQueueTransaction internalTransaction, MessageQueueTransactionType transactionType)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 0xffffffffL))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("InvalidParameter", new object[] { "timeout", timeout.ToString() }));
            }
            if (action == 0)
            {
                if (!this.receiveGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Receive, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.receiveGranted = true;
                }
            }
            else if (!this.peekGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Peek, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.peekGranted = true;
            }
            int num2 = 0;
            Message message = null;
            MessagePropertyVariants.MQPROPS properties = null;
            if (filter != null)
            {
                message = new Message((MessagePropertyFilter) filter.Clone());
                if (this.formatter != null)
                {
                    message.Formatter = (IMessageFormatter) this.formatter.Clone();
                }
                properties = message.Lock();
            }
            try
            {
                if (internalTransaction != null)
                {
                    num2 = this.StaleSafeReceiveMessage((uint) totalMilliseconds, action, properties, null, null, cursor, internalTransaction.BeginQueueOperation());
                }
                else
                {
                    num2 = this.StaleSafeReceiveMessage((uint) totalMilliseconds, action, properties, null, null, cursor, (IntPtr) ((long) transactionType));
                }
                if (message != null)
                {
                    while (IsMemoryError(num2))
                    {
                        if (action == -2147483647)
                        {
                            action = -2147483648;
                        }
                        message.Unlock();
                        message.AdjustMemory();
                        properties = message.Lock();
                        if (internalTransaction != null)
                        {
                            num2 = this.StaleSafeReceiveMessage((uint) totalMilliseconds, action, properties, null, null, cursor, internalTransaction.InnerTransaction);
                        }
                        else
                        {
                            num2 = this.StaleSafeReceiveMessage((uint) totalMilliseconds, action, properties, null, null, cursor, (IntPtr) ((long) transactionType));
                        }
                    }
                }
            }
            finally
            {
                if (message != null)
                {
                    message.Unlock();
                }
                if (internalTransaction != null)
                {
                    internalTransaction.EndQueueOperation();
                }
            }
            if (IsFatalError(num2))
            {
                throw new MessageQueueException(num2);
            }
            return message;
        }

        public void Refresh()
        {
            this.PropertyFilter.ClearAll();
        }

        public void ResetPermissions()
        {
            if (!this.administerGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Administer, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.administerGranted = true;
            }
            int error = System.Messaging.Interop.UnsafeNativeMethods.MQSetQueueSecurity(this.FormatName, 4, null);
            if (error != 0)
            {
                throw new MessageQueueException(error);
            }
        }

        private static string ResolveFormatNameFromQueuePath(string queuePath, bool throwException)
        {
            string machineName = queuePath.Substring(0, queuePath.IndexOf('\\'));
            string strA = queuePath.Substring(queuePath.IndexOf('\\'));
            if (((string.Compare(strA, SUFIX_DEADLETTER, true, CultureInfo.InvariantCulture) == 0) || (string.Compare(strA, SUFIX_DEADXACT, true, CultureInfo.InvariantCulture) == 0)) || (string.Compare(strA, SUFIX_JOURNAL, true, CultureInfo.InvariantCulture) == 0))
            {
                if (machineName.CompareTo(".") == 0)
                {
                    machineName = ComputerName;
                }
                Guid machineId = GetMachineId(machineName);
                StringBuilder builder = new StringBuilder();
                builder.Append("MACHINE=");
                builder.Append(machineId.ToString());
                if (string.Compare(strA, SUFIX_DEADXACT, true, CultureInfo.InvariantCulture) == 0)
                {
                    builder.Append(";DEADXACT");
                }
                else if (string.Compare(strA, SUFIX_DEADLETTER, true, CultureInfo.InvariantCulture) == 0)
                {
                    builder.Append(";DEADLETTER");
                }
                else
                {
                    builder.Append(";JOURNAL");
                }
                return builder.ToString();
            }
            string pathName = queuePath;
            bool flag = false;
            if (queuePath.ToUpper(CultureInfo.InvariantCulture).EndsWith(SUFIX_JOURNAL))
            {
                flag = true;
                int length = pathName.LastIndexOf('\\');
                pathName = pathName.Substring(0, length);
            }
            int error = 0;
            StringBuilder formatName = new StringBuilder(0x7c);
            int count = 0x7c;
            error = SafeNativeMethods.MQPathNameToFormatName(pathName, formatName, ref count);
            if (error == 0)
            {
                if (flag)
                {
                    formatName.Append(";JOURNAL");
                }
                return formatName.ToString();
            }
            if (throwException)
            {
                throw new MessageQueueException(error);
            }
            if (error == -1072824300)
            {
                throw new MessageQueueException(error);
            }
            return null;
        }

        private static MessageQueue ResolveQueueFromLabel(string path, bool throwException)
        {
            MessageQueue[] publicQueuesByLabel = GetPublicQueuesByLabel(path.Substring(PREFIX_LABEL.Length), false);
            if (publicQueuesByLabel.Length == 0)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(System.Messaging.Res.GetString("InvalidLabel", new object[] { path.Substring(PREFIX_LABEL.Length) }));
                }
                return null;
            }
            if (publicQueuesByLabel.Length > 1)
            {
                throw new InvalidOperationException(System.Messaging.Res.GetString("AmbiguousLabel", new object[] { path.Substring(PREFIX_LABEL.Length) }));
            }
            return publicQueuesByLabel[0];
        }

        private void SaveQueueProperties()
        {
            if (!this.administerGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Administer, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.administerGranted = true;
            }
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQSetQueueProperties(this.FormatName, this.Properties.Lock());
            this.Properties.Unlock();
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Send(object obj)
        {
            this.SendInternal(obj, null, MessageQueueTransactionType.None);
        }

        public void Send(object obj, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            this.SendInternal(obj, transaction, MessageQueueTransactionType.None);
        }

        public void Send(object obj, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            this.SendInternal(obj, null, transactionType);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Send(object obj, string label)
        {
            this.Send(obj, label, null, MessageQueueTransactionType.None);
        }

        public void Send(object obj, string label, MessageQueueTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            this.Send(obj, label, transaction, MessageQueueTransactionType.None);
        }

        public void Send(object obj, string label, MessageQueueTransactionType transactionType)
        {
            if (!ValidationUtility.ValidateMessageQueueTransactionType(transactionType))
            {
                throw new InvalidEnumArgumentException("transactionType", (int) transactionType, typeof(MessageQueueTransactionType));
            }
            this.Send(obj, label, null, transactionType);
        }

        private void Send(object obj, string label, MessageQueueTransaction transaction, MessageQueueTransactionType transactionType)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }
            if (obj is Message)
            {
                ((Message) obj).Label = label;
                this.SendInternal(obj, transaction, transactionType);
            }
            else
            {
                string str = this.DefaultPropertiesToSend.Label;
                try
                {
                    this.DefaultPropertiesToSend.Label = label;
                    this.SendInternal(obj, transaction, transactionType);
                }
                finally
                {
                    this.DefaultPropertiesToSend.Label = str;
                }
            }
        }

        private void SendInternal(object obj, MessageQueueTransaction internalTransaction, MessageQueueTransactionType transactionType)
        {
            if (!this.sendGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Send, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.sendGranted = true;
            }
            Message cachedMessage = null;
            if (obj is Message)
            {
                cachedMessage = (Message) obj;
            }
            if (cachedMessage == null)
            {
                cachedMessage = this.DefaultPropertiesToSend.CachedMessage;
                cachedMessage.Formatter = this.Formatter;
                cachedMessage.Body = obj;
            }
            int num = 0;
            cachedMessage.AdjustToSend();
            MessagePropertyVariants.MQPROPS properties = cachedMessage.Lock();
            try
            {
                if (internalTransaction != null)
                {
                    num = this.StaleSafeSendMessage(properties, internalTransaction.BeginQueueOperation());
                }
                else
                {
                    num = this.StaleSafeSendMessage(properties, (IntPtr) ((long) transactionType));
                }
            }
            finally
            {
                cachedMessage.Unlock();
                if (internalTransaction != null)
                {
                    internalTransaction.EndQueueOperation();
                }
            }
            if (IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
        }

        internal void SetAccessMode(QueueAccessMode accessMode)
        {
            if (!ValidationUtility.ValidateQueueAccessMode(accessMode))
            {
                throw new InvalidEnumArgumentException("accessMode", (int) accessMode, typeof(QueueAccessMode));
            }
            this.accessMode = accessMode;
        }

        public void SetPermissions(AccessControlList dacl)
        {
            if (dacl == null)
            {
                throw new ArgumentNullException("dacl");
            }
            if (!this.administerGranted)
            {
                new MessageQueuePermission(MessageQueuePermissionAccess.Administer, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                this.administerGranted = true;
            }
            AccessControlList.CheckEnvironment();
            byte[] buffer = new byte[100];
            int lengthNeeded = 0;
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                bool flag;
                bool flag2;
                IntPtr ptr;
                int error = System.Messaging.Interop.UnsafeNativeMethods.MQGetQueueSecurity(this.FormatName, 4, handle.AddrOfPinnedObject(), buffer.Length, out lengthNeeded);
                if (error == -1072824285)
                {
                    handle.Free();
                    buffer = new byte[lengthNeeded];
                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    error = System.Messaging.Interop.UnsafeNativeMethods.MQGetQueueSecurity(this.FormatName, 4, handle.AddrOfPinnedObject(), buffer.Length, out lengthNeeded);
                }
                if (error != 0)
                {
                    throw new MessageQueueException(error);
                }
                if (!System.Messaging.Interop.UnsafeNativeMethods.GetSecurityDescriptorDacl(handle.AddrOfPinnedObject(), out flag, out ptr, out flag2))
                {
                    throw new Win32Exception();
                }
                System.Messaging.Interop.NativeMethods.SECURITY_DESCRIPTOR sD = new System.Messaging.Interop.NativeMethods.SECURITY_DESCRIPTOR();
                System.Messaging.Interop.UnsafeNativeMethods.InitializeSecurityDescriptor(sD, 1);
                IntPtr pDacl = dacl.MakeAcl(ptr);
                try
                {
                    if (!System.Messaging.Interop.UnsafeNativeMethods.SetSecurityDescriptorDacl(sD, true, pDacl, false))
                    {
                        throw new Win32Exception();
                    }
                    int num3 = System.Messaging.Interop.UnsafeNativeMethods.MQSetQueueSecurity(this.FormatName, 4, sD);
                    if (num3 != 0)
                    {
                        throw new MessageQueueException(num3);
                    }
                }
                finally
                {
                    AccessControlList.FreeAcl(pDacl);
                }
                queueInfoCache.Remove(this.QueueInfoKey);
                formatNameCache.Remove(this.path.ToUpper(CultureInfo.InvariantCulture));
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        public void SetPermissions(MessageQueueAccessControlEntry ace)
        {
            if (ace == null)
            {
                throw new ArgumentNullException("ace");
            }
            AccessControlList dacl = new AccessControlList();
            dacl.Add(ace);
            this.SetPermissions(dacl);
        }

        public void SetPermissions(string user, MessageQueueAccessRights rights)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            this.SetPermissions(user, rights, AccessControlEntryType.Allow);
        }

        public void SetPermissions(string user, MessageQueueAccessRights rights, AccessControlEntryType entryType)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            Trustee trustee = new Trustee(user);
            MessageQueueAccessControlEntry entry = new MessageQueueAccessControlEntry(trustee, rights, entryType);
            AccessControlList dacl = new AccessControlList();
            dacl.Add(entry);
            this.SetPermissions(dacl);
        }

        private int StaleSafePurgeQueue()
        {
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQPurgeQueue(this.MQInfo.ReadHandle);
            if ((num != -1072824234) && (num != -1072824230))
            {
                return num;
            }
            this.MQInfo.Close();
            return System.Messaging.Interop.UnsafeNativeMethods.MQPurgeQueue(this.MQInfo.ReadHandle);
        }

        private unsafe int StaleSafeReceiveByLookupId(long lookupId, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, IntPtr transaction)
        {
            if (((int) transaction) == 1)
            {
                Transaction current = Transaction.Current;
                if (current != null)
                {
                    IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(current);
                    return this.StaleSafeReceiveByLookupId(lookupId, action, properties, overlapped, receiveCallback, (ITransaction) dtcTransaction);
                }
            }
            int receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessageByLookupId(this.MQInfo.ReadHandle, lookupId, action, properties, overlapped, receiveCallback, transaction);
            if (this.IsCashedInfoInvalidOnReceive(receiveResult))
            {
                this.MQInfo.Close();
                receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessageByLookupId(this.MQInfo.ReadHandle, lookupId, action, properties, overlapped, receiveCallback, transaction);
            }
            return receiveResult;
        }

        private unsafe int StaleSafeReceiveByLookupId(long lookupId, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, ITransaction transaction)
        {
            int receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessageByLookupId(this.MQInfo.ReadHandle, lookupId, action, properties, overlapped, receiveCallback, transaction);
            if (this.IsCashedInfoInvalidOnReceive(receiveResult))
            {
                this.MQInfo.Close();
                receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessageByLookupId(this.MQInfo.ReadHandle, lookupId, action, properties, overlapped, receiveCallback, transaction);
            }
            return receiveResult;
        }

        internal unsafe int StaleSafeReceiveMessage(uint timeout, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, CursorHandle cursorHandle, IntPtr transaction)
        {
            if (((int) transaction) == 1)
            {
                Transaction current = Transaction.Current;
                if (current != null)
                {
                    IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(current);
                    return this.StaleSafeReceiveMessage(timeout, action, properties, overlapped, receiveCallback, cursorHandle, (ITransaction) dtcTransaction);
                }
            }
            int receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessage(this.MQInfo.ReadHandle, timeout, action, properties, overlapped, receiveCallback, cursorHandle, transaction);
            if (this.IsCashedInfoInvalidOnReceive(receiveResult))
            {
                this.MQInfo.Close();
                receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessage(this.MQInfo.ReadHandle, timeout, action, properties, overlapped, receiveCallback, cursorHandle, transaction);
            }
            return receiveResult;
        }

        private unsafe int StaleSafeReceiveMessage(uint timeout, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, CursorHandle cursorHandle, ITransaction transaction)
        {
            int receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessage(this.MQInfo.ReadHandle, timeout, action, properties, overlapped, receiveCallback, cursorHandle, transaction);
            if (this.IsCashedInfoInvalidOnReceive(receiveResult))
            {
                this.MQInfo.Close();
                receiveResult = System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessage(this.MQInfo.ReadHandle, timeout, action, properties, overlapped, receiveCallback, cursorHandle, transaction);
            }
            return receiveResult;
        }

        private int StaleSafeSendMessage(MessagePropertyVariants.MQPROPS properties, IntPtr transaction)
        {
            if (((int) transaction) == 1)
            {
                Transaction current = Transaction.Current;
                if (current != null)
                {
                    IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(current);
                    return this.StaleSafeSendMessage(properties, (ITransaction) dtcTransaction);
                }
            }
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQSendMessage(this.MQInfo.WriteHandle, properties, transaction);
            if ((num != -1072824234) && (num != -1072824230))
            {
                return num;
            }
            this.MQInfo.Close();
            return System.Messaging.Interop.UnsafeNativeMethods.MQSendMessage(this.MQInfo.WriteHandle, properties, transaction);
        }

        private int StaleSafeSendMessage(MessagePropertyVariants.MQPROPS properties, ITransaction transaction)
        {
            int num = System.Messaging.Interop.UnsafeNativeMethods.MQSendMessage(this.MQInfo.WriteHandle, properties, transaction);
            if ((num != -1072824234) && (num != -1072824230))
            {
                return num;
            }
            this.MQInfo.Close();
            return System.Messaging.Interop.UnsafeNativeMethods.MQSendMessage(this.MQInfo.WriteHandle, properties, transaction);
        }

        internal static bool ValidatePath(string path, bool checkQueueNameSize)
        {
            if ((path == null) || (path.Length == 0))
            {
                return true;
            }
            string str = path.ToUpper(CultureInfo.InvariantCulture);
            if (str.StartsWith(PREFIX_LABEL))
            {
                return true;
            }
            if (str.StartsWith(PREFIX_FORMAT_NAME))
            {
                return true;
            }
            int num = 0;
            int num2 = -1;
            while (true)
            {
                int index = str.IndexOf('\\', num2 + 1);
                if (index == -1)
                {
                    break;
                }
                num2 = index;
                num++;
            }
            switch (num)
            {
                case 1:
                    if (checkQueueNameSize)
                    {
                        long num4 = path.Length - (num2 + 1);
                        if (num4 > 0xffL)
                        {
                            throw new ArgumentException(System.Messaging.Res.GetString("LongQueueName"));
                        }
                    }
                    return true;

                case 2:
                    if (str.EndsWith(SUFIX_JOURNAL))
                    {
                        return true;
                    }
                    if (str.LastIndexOf(SUFIX_PRIVATE + @"\") != -1)
                    {
                        return true;
                    }
                    break;
            }
            return (((num == 3) && str.EndsWith(SUFIX_JOURNAL)) && (str.LastIndexOf(SUFIX_PRIVATE + @"\") != -1));
        }

        public QueueAccessMode AccessMode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.accessMode;
            }
        }

        [MessagingDescription("MQ_Authenticate"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Authenticate
        {
            get
            {
                if (!this.PropertyFilter.Authenticate)
                {
                    this.Properties.SetUI1(0x6f, 0);
                    this.GenerateQueueProperties();
                    this.authenticate = this.Properties.GetUI1(0x6f) != 0;
                    this.PropertyFilter.Authenticate = true;
                    this.Properties.Remove(0x6f);
                }
                return this.authenticate;
            }
            set
            {
                if (value)
                {
                    this.Properties.SetUI1(0x6f, 1);
                }
                else
                {
                    this.Properties.SetUI1(0x6f, 0);
                }
                this.SaveQueueProperties();
                this.authenticate = value;
                this.PropertyFilter.Authenticate = true;
                this.Properties.Remove(0x6f);
            }
        }

        [MessagingDescription("MQ_BasePriority"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public short BasePriority
        {
            get
            {
                if (!this.PropertyFilter.BasePriority)
                {
                    this.Properties.SetI2(0x6a, 0);
                    this.GenerateQueueProperties();
                    this.basePriority = this.properties.GetI2(0x6a);
                    this.PropertyFilter.BasePriority = true;
                    this.Properties.Remove(0x6a);
                }
                return this.basePriority;
            }
            set
            {
                this.Properties.SetI2(0x6a, value);
                this.SaveQueueProperties();
                this.basePriority = value;
                this.PropertyFilter.BasePriority = true;
                this.Properties.Remove(0x6a);
            }
        }

        [MessagingDescription("MQ_CanRead"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanRead
        {
            get
            {
                if (!this.browseGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Browse, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.browseGranted = true;
                }
                return this.MQInfo.CanRead;
            }
        }

        [MessagingDescription("MQ_CanWrite"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanWrite
        {
            get
            {
                if (!this.browseGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Browse, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.browseGranted = true;
                }
                return this.MQInfo.CanWrite;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MQ_Category")]
        public Guid Category
        {
            get
            {
                if (!this.PropertyFilter.Category)
                {
                    this.Properties.SetNull(0x66);
                    this.GenerateQueueProperties();
                    byte[] destination = new byte[0x10];
                    IntPtr intPtr = this.Properties.GetIntPtr(0x66);
                    if (intPtr != IntPtr.Zero)
                    {
                        Marshal.Copy(intPtr, destination, 0, 0x10);
                        SafeNativeMethods.MQFreeMemory(intPtr);
                    }
                    this.queueType = new Guid(destination);
                    this.PropertyFilter.Category = true;
                    this.Properties.Remove(0x66);
                }
                return this.queueType;
            }
            set
            {
                this.Properties.SetGuid(0x66, value.ToByteArray());
                this.SaveQueueProperties();
                this.queueType = value;
                this.PropertyFilter.Category = true;
                this.Properties.Remove(0x66);
            }
        }

        internal static string ComputerName
        {
            get
            {
                if (computerName == null)
                {
                    lock (staticSyncRoot)
                    {
                        if (computerName == null)
                        {
                            StringBuilder lpBuffer = new StringBuilder(0x100);
                            SafeNativeMethods.GetComputerName(lpBuffer, new int[] { lpBuffer.Capacity });
                            computerName = lpBuffer.ToString();
                        }
                    }
                }
                return computerName;
            }
        }

        [MessagingDescription("MQ_CreateTime"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime CreateTime
        {
            get
            {
                if (!this.PropertyFilter.CreateTime)
                {
                    DateTime time = new DateTime(0x7b2, 1, 1);
                    this.Properties.SetI4(0x6d, 0);
                    this.GenerateQueueProperties();
                    this.createTime = time.AddSeconds((double) this.properties.GetI4(0x6d)).ToLocalTime();
                    this.PropertyFilter.CreateTime = true;
                    this.Properties.Remove(0x6d);
                }
                return this.createTime;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), MessagingDescription("MQ_DefaultPropertiesToSend")]
        public System.Messaging.DefaultPropertiesToSend DefaultPropertiesToSend
        {
            get
            {
                if (this.defaultProperties == null)
                {
                    if (base.DesignMode)
                    {
                        this.defaultProperties = new System.Messaging.DefaultPropertiesToSend(true);
                    }
                    else
                    {
                        this.defaultProperties = new System.Messaging.DefaultPropertiesToSend();
                    }
                }
                return this.defaultProperties;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.defaultProperties = value;
            }
        }

        [DefaultValue(false), Browsable(false), MessagingDescription("MQ_DenySharedReceive")]
        public bool DenySharedReceive
        {
            get
            {
                return (this.sharedMode == 1);
            }
            set
            {
                if (value && (this.sharedMode != 1))
                {
                    this.Close();
                    this.sharedMode = 1;
                }
                else if (!value && (this.sharedMode == 1))
                {
                    this.Close();
                    this.sharedMode = 0;
                }
            }
        }

        [Browsable(false)]
        public static bool EnableConnectionCache
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return enableConnectionCache;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                enableConnectionCache = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MQ_EncryptionRequired")]
        public System.Messaging.EncryptionRequired EncryptionRequired
        {
            get
            {
                if (!this.PropertyFilter.EncryptionLevel)
                {
                    this.Properties.SetUI4(0x70, 0);
                    this.GenerateQueueProperties();
                    this.encryptionLevel = this.Properties.GetUI4(0x70);
                    this.PropertyFilter.EncryptionLevel = true;
                    this.Properties.Remove(0x70);
                }
                return (System.Messaging.EncryptionRequired) this.encryptionLevel;
            }
            set
            {
                if (!ValidationUtility.ValidateEncryptionRequired(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Messaging.EncryptionRequired));
                }
                this.Properties.SetUI4(0x70, (int) value);
                this.SaveQueueProperties();
                this.encryptionLevel = this.properties.GetUI4(0x70);
                this.PropertyFilter.EncryptionLevel = true;
                this.Properties.Remove(0x70);
            }
        }

        [MessagingDescription("MQ_FormatName"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FormatName
        {
            get
            {
                if (this.formatName == null)
                {
                    if ((this.path == null) || (this.path.Length == 0))
                    {
                        return string.Empty;
                    }
                    string key = this.path.ToUpper(CultureInfo.InvariantCulture);
                    if (this.enableCache)
                    {
                        this.formatName = formatNameCache.Get(key);
                    }
                    if (this.formatName == null)
                    {
                        if (this.PropertyFilter.Id)
                        {
                            int error = 0;
                            StringBuilder formatName = new StringBuilder(0x7c);
                            int count = 0x7c;
                            error = SafeNativeMethods.MQInstanceToFormatName(this.id.ToByteArray(), formatName, ref count);
                            if (error != 0)
                            {
                                throw new MessageQueueException(error);
                            }
                            this.formatName = formatName.ToString();
                            return this.formatName;
                        }
                        if (key.StartsWith(PREFIX_FORMAT_NAME))
                        {
                            this.formatName = this.path.Substring(PREFIX_FORMAT_NAME.Length);
                        }
                        else if (key.StartsWith(PREFIX_LABEL))
                        {
                            MessageQueue queue = ResolveQueueFromLabel(this.path, true);
                            this.formatName = queue.FormatName;
                            this.queuePath = queue.QueuePath;
                        }
                        else
                        {
                            this.queuePath = this.path;
                            this.formatName = ResolveFormatNameFromQueuePath(this.queuePath, true);
                        }
                        formatNameCache.Put(key, this.formatName);
                    }
                }
                return this.formatName;
            }
        }

        [MessagingDescription("MQ_Formatter"), DefaultValue((string) null), TypeConverter(typeof(MessageFormatterConverter)), Browsable(false)]
        public IMessageFormatter Formatter
        {
            get
            {
                if ((this.formatter == null) && !base.DesignMode)
                {
                    this.formatter = new XmlMessageFormatter();
                }
                return this.formatter;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.formatter = value;
            }
        }

        [MessagingDescription("MQ_GuidId"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guid Id
        {
            get
            {
                if (!this.PropertyFilter.Id)
                {
                    this.Properties.SetNull(0x65);
                    this.GenerateQueueProperties();
                    byte[] destination = new byte[0x10];
                    IntPtr intPtr = this.Properties.GetIntPtr(0x65);
                    if (intPtr != IntPtr.Zero)
                    {
                        Marshal.Copy(intPtr, destination, 0, 0x10);
                        SafeNativeMethods.MQFreeMemory(intPtr);
                    }
                    this.id = new Guid(destination);
                    this.PropertyFilter.Id = true;
                    this.Properties.Remove(0x65);
                }
                return this.id;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MQ_Label")]
        public string Label
        {
            get
            {
                if (!this.PropertyFilter.Label)
                {
                    this.Properties.SetNull(0x6c);
                    this.GenerateQueueProperties();
                    string str = null;
                    IntPtr intPtr = this.Properties.GetIntPtr(0x6c);
                    if (intPtr != IntPtr.Zero)
                    {
                        str = Marshal.PtrToStringUni(intPtr);
                        SafeNativeMethods.MQFreeMemory(intPtr);
                    }
                    this.label = str;
                    this.PropertyFilter.Label = true;
                    this.Properties.Remove(0x6c);
                }
                return this.label;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Properties.SetString(0x6c, Message.StringToBytes(value));
                this.SaveQueueProperties();
                this.label = value;
                this.PropertyFilter.Label = true;
                this.Properties.Remove(0x6c);
            }
        }

        [MessagingDescription("MQ_LastModifyTime"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime LastModifyTime
        {
            get
            {
                if (!this.PropertyFilter.LastModifyTime)
                {
                    DateTime time = new DateTime(0x7b2, 1, 1);
                    this.Properties.SetI4(110, 0);
                    this.GenerateQueueProperties();
                    this.lastModifyTime = time.AddSeconds((double) this.properties.GetI4(110)).ToLocalTime();
                    this.PropertyFilter.LastModifyTime = true;
                    this.Properties.Remove(110);
                }
                return this.lastModifyTime;
            }
        }

        [MessagingDescription("MQ_MachineName"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MachineName
        {
            get
            {
                string queuePath = this.QueuePath;
                if (queuePath.Length == 0)
                {
                    return queuePath;
                }
                return queuePath.Substring(0, queuePath.IndexOf('\\'));
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("InvalidProperty", new object[] { "MachineName", value }));
                }
                StringBuilder builder = new StringBuilder();
                if (((this.path == null) || (this.path.Length == 0)) && (this.formatName == null))
                {
                    builder.Append(value);
                    builder.Append(SUFIX_JOURNAL);
                }
                else
                {
                    builder.Append(value);
                    builder.Append(@"\");
                    builder.Append(this.QueueName);
                }
                this.Path = builder.ToString();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MQ_MaximumJournalSize"), TypeConverter(typeof(SizeConverter))]
        public long MaximumJournalSize
        {
            get
            {
                if (!this.PropertyFilter.MaximumJournalSize)
                {
                    this.Properties.SetUI4(0x6b, 0);
                    this.GenerateQueueProperties();
                    this.journalSize = (long) ((ulong) this.properties.GetUI4(0x6b));
                    this.PropertyFilter.MaximumJournalSize = true;
                    this.Properties.Remove(0x6b);
                }
                return this.journalSize;
            }
            set
            {
                if ((value > InfiniteQueueSize) || (value < 0L))
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("InvalidProperty", new object[] { "MaximumJournalSize", value }));
                }
                this.Properties.SetUI4(0x6b, (int) ((uint) value));
                this.SaveQueueProperties();
                this.journalSize = value;
                this.PropertyFilter.MaximumJournalSize = true;
                this.Properties.Remove(0x6b);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(SizeConverter)), MessagingDescription("MQ_MaximumQueueSize")]
        public long MaximumQueueSize
        {
            get
            {
                if (!this.PropertyFilter.MaximumQueueSize)
                {
                    this.Properties.SetUI4(0x69, 0);
                    this.GenerateQueueProperties();
                    this.queueSize = (long) ((ulong) this.properties.GetUI4(0x69));
                    this.PropertyFilter.MaximumQueueSize = true;
                    this.Properties.Remove(0x69);
                }
                return this.queueSize;
            }
            set
            {
                if ((value > InfiniteQueueSize) || (value < 0L))
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("InvalidProperty", new object[] { "MaximumQueueSize", value }));
                }
                this.Properties.SetUI4(0x69, (int) ((uint) value));
                this.SaveQueueProperties();
                this.queueSize = value;
                this.PropertyFilter.MaximumQueueSize = true;
                this.Properties.Remove(0x69);
            }
        }

        [MessagingDescription("MQ_MessageReadPropertyFilter"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MessagePropertyFilter MessageReadPropertyFilter
        {
            get
            {
                if (this.receiveFilter == null)
                {
                    this.receiveFilter = new MessagePropertyFilter();
                    this.receiveFilter.SetDefaults();
                }
                return this.receiveFilter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.receiveFilter = value;
            }
        }

        internal MQCacheableInfo MQInfo
        {
            get
            {
                if (this.mqInfo == null)
                {
                    MQCacheableInfo info = queueInfoCache.Get(this.QueueInfoKey);
                    if ((this.sharedMode == 1) || !this.enableCache)
                    {
                        if (info != null)
                        {
                            info.CloseIfNotReferenced();
                        }
                        this.mqInfo = new MQCacheableInfo(this.FormatName, this.accessMode, this.sharedMode);
                        this.mqInfo.AddRef();
                    }
                    else if (info != null)
                    {
                        info.AddRef();
                        this.mqInfo = info;
                    }
                    else
                    {
                        this.mqInfo = new MQCacheableInfo(this.FormatName, this.accessMode, this.sharedMode);
                        this.mqInfo.AddRef();
                        queueInfoCache.Put(this.QueueInfoKey, this.mqInfo);
                    }
                }
                return this.mqInfo;
            }
        }

        [MessagingDescription("MQ_MulticastAddress"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MulticastAddress
        {
            get
            {
                if (!Msmq3OrNewer)
                {
                    if (!base.DesignMode)
                    {
                        throw new PlatformNotSupportedException(System.Messaging.Res.GetString("PlatformNotSupported"));
                    }
                    return string.Empty;
                }
                if (!this.PropertyFilter.MulticastAddress)
                {
                    this.Properties.SetNull(0x7d);
                    this.GenerateQueueProperties();
                    string str = null;
                    IntPtr intPtr = this.Properties.GetIntPtr(0x7d);
                    if (intPtr != IntPtr.Zero)
                    {
                        str = Marshal.PtrToStringUni(intPtr);
                        SafeNativeMethods.MQFreeMemory(intPtr);
                    }
                    this.multicastAddress = str;
                    this.PropertyFilter.MulticastAddress = true;
                    this.Properties.Remove(0x7d);
                }
                return this.multicastAddress;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!Msmq3OrNewer)
                {
                    throw new PlatformNotSupportedException(System.Messaging.Res.GetString("PlatformNotSupported"));
                }
                if (value.Length == 0)
                {
                    this.Properties.SetEmpty(0x7d);
                }
                else
                {
                    this.Properties.SetString(0x7d, Message.StringToBytes(value));
                }
                this.SaveQueueProperties();
                this.multicastAddress = value;
                this.PropertyFilter.MulticastAddress = true;
                this.Properties.Remove(0x7d);
            }
        }

        private Hashtable OutstandingAsyncRequests
        {
            get
            {
                if (this.outstandingAsyncRequests == null)
                {
                    lock (this.syncRoot)
                    {
                        if (this.outstandingAsyncRequests == null)
                        {
                            Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
                            Thread.MemoryBarrier();
                            this.outstandingAsyncRequests = hashtable;
                        }
                    }
                }
                return this.outstandingAsyncRequests;
            }
        }

        [RefreshProperties(RefreshProperties.All), Browsable(false), DefaultValue(""), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MessagingDescription("MQ_Path"), Editor("System.Messaging.Design.QueuePathEditor", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SettingsBindable(true)]
        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!ValidatePath(value, false))
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("PathSyntax"));
                }
                if (!string.IsNullOrEmpty(this.path))
                {
                    this.Close();
                }
                this.path = value;
            }
        }

        private QueuePropertyVariants Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new QueuePropertyVariants();
                }
                return this.properties;
            }
        }

        private QueuePropertyFilter PropertyFilter
        {
            get
            {
                if (this.filter == null)
                {
                    this.filter = new QueuePropertyFilter();
                }
                return this.filter;
            }
        }

        private QueueInfoKeyHolder QueueInfoKey
        {
            get
            {
                if (this.queueInfoKey == null)
                {
                    lock (this.syncRoot)
                    {
                        if (this.queueInfoKey == null)
                        {
                            QueueInfoKeyHolder holder = new QueueInfoKeyHolder(this.FormatName, this.accessMode);
                            Thread.MemoryBarrier();
                            this.queueInfoKey = holder;
                        }
                    }
                }
                return this.queueInfoKey;
            }
        }

        [MessagingDescription("MQ_QueueName"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string QueueName
        {
            get
            {
                string queuePath = this.QueuePath;
                if (queuePath.Length == 0)
                {
                    return queuePath;
                }
                return queuePath.Substring(queuePath.IndexOf('\\') + 1);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                StringBuilder builder = new StringBuilder();
                if (((this.path == null) || (this.path.Length == 0)) && (this.formatName == null))
                {
                    builder.Append(@".\");
                    builder.Append(value);
                }
                else
                {
                    builder.Append(this.MachineName);
                    builder.Append(@"\");
                    builder.Append(value);
                }
                this.Path = builder.ToString();
            }
        }

        internal string QueuePath
        {
            get
            {
                if (this.queuePath == null)
                {
                    if ((this.path == null) || (this.path.Length == 0))
                    {
                        return string.Empty;
                    }
                    string str = this.path.ToUpper(CultureInfo.InvariantCulture);
                    if (str.StartsWith(PREFIX_LABEL))
                    {
                        MessageQueue queue = ResolveQueueFromLabel(this.path, true);
                        this.formatName = queue.FormatName;
                        this.queuePath = queue.QueuePath;
                    }
                    else if (str.StartsWith(PREFIX_FORMAT_NAME))
                    {
                        this.Properties.SetNull(0x67);
                        this.GenerateQueueProperties();
                        string str2 = null;
                        IntPtr intPtr = this.Properties.GetIntPtr(0x67);
                        if (intPtr != IntPtr.Zero)
                        {
                            str2 = Marshal.PtrToStringUni(intPtr);
                            SafeNativeMethods.MQFreeMemory(intPtr);
                        }
                        this.Properties.Remove(0x67);
                        this.queuePath = str2;
                    }
                    else
                    {
                        this.queuePath = this.path;
                    }
                }
                return this.queuePath;
            }
        }

        [Browsable(false), MessagingDescription("MQ_ReadHandle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr ReadHandle
        {
            get
            {
                if (!this.receiveGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Receive, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.receiveGranted = true;
                }
                return this.MQInfo.ReadHandle.DangerousGetHandle();
            }
        }

        [MessagingDescription("MQ_SynchronizingObject"), DefaultValue((string) null), Browsable(false)]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if ((this.synchronizingObject == null) && base.DesignMode)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        object rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is ISynchronizeInvoke))
                        {
                            this.synchronizingObject = (ISynchronizeInvoke) rootComponent;
                        }
                    }
                }
                return this.synchronizingObject;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.synchronizingObject = value;
            }
        }

        [MessagingDescription("MQ_Transactional"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Transactional
        {
            get
            {
                if (!this.browseGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Browse, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.browseGranted = true;
                }
                return this.MQInfo.Transactional;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MessagingDescription("MQ_UseJournalQueue")]
        public bool UseJournalQueue
        {
            get
            {
                if (!this.PropertyFilter.UseJournalQueue)
                {
                    this.Properties.SetUI1(0x68, 0);
                    this.GenerateQueueProperties();
                    this.useJournaling = this.Properties.GetUI1(0x68) != 0;
                    this.PropertyFilter.UseJournalQueue = true;
                    this.Properties.Remove(0x68);
                }
                return this.useJournaling;
            }
            set
            {
                if (value)
                {
                    this.Properties.SetUI1(0x68, 1);
                }
                else
                {
                    this.Properties.SetUI1(0x68, 0);
                }
                this.SaveQueueProperties();
                this.useJournaling = value;
                this.PropertyFilter.UseJournalQueue = true;
                this.Properties.Remove(0x68);
            }
        }

        [Browsable(false), MessagingDescription("MQ_WriteHandle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr WriteHandle
        {
            get
            {
                if (!this.sendGranted)
                {
                    new MessageQueuePermission(MessageQueuePermissionAccess.Send, PREFIX_FORMAT_NAME + this.FormatName).Demand();
                    this.sendGranted = true;
                }
                return this.MQInfo.WriteHandle.DangerousGetHandle();
            }
        }

        private class AsynchronousRequest : IAsyncResult
        {
            private int action;
            private object asyncState;
            private AsyncCallback callback;
            private CursorHandle cursorHandle;
            private bool isCompleted;
            private Message message;
            private IOCompletionCallback onCompletionStatusChanged;
            private SafeNativeMethods.ReceiveCallback onMessageReceived;
            private MessageQueue owner;
            private ManualResetEvent resetEvent;
            private int status;
            private uint timeout;

            internal AsynchronousRequest(MessageQueue owner, uint timeout, CursorHandle cursorHandle, int action, bool useThreadPool, object asyncState, AsyncCallback callback)
            {
                this.owner = owner;
                this.asyncState = asyncState;
                this.callback = callback;
                this.action = action;
                this.timeout = timeout;
                this.resetEvent = new ManualResetEvent(false);
                this.cursorHandle = cursorHandle;
                if (!useThreadPool)
                {
                    this.onMessageReceived = new SafeNativeMethods.ReceiveCallback(this.OnMessageReceived);
                }
                else
                {
                    this.onCompletionStatusChanged = new IOCompletionCallback(this.OnCompletionStatusChanged);
                }
            }

            internal unsafe void BeginRead()
            {
                NativeOverlapped* overlapped = null;
                if (this.onCompletionStatusChanged != null)
                {
                    overlapped = new Overlapped { AsyncResult = this }.Pack(this.onCompletionStatusChanged, null);
                }
                int num = 0;
                this.message = new Message(this.owner.MessageReadPropertyFilter);
                try
                {
                    num = this.owner.StaleSafeReceiveMessage(this.timeout, this.action, this.message.Lock(), overlapped, this.onMessageReceived, this.cursorHandle, IntPtr.Zero);
                    while (MessageQueue.IsMemoryError(num))
                    {
                        if (this.action == -2147483647)
                        {
                            this.action = -2147483648;
                        }
                        this.message.Unlock();
                        this.message.AdjustMemory();
                        num = this.owner.StaleSafeReceiveMessage(this.timeout, this.action, this.message.Lock(), overlapped, this.onMessageReceived, this.cursorHandle, IntPtr.Zero);
                    }
                }
                catch (Exception exception)
                {
                    this.message.Unlock();
                    if (overlapped != null)
                    {
                        Overlapped.Free(overlapped);
                    }
                    if (!this.owner.useThreadPool)
                    {
                        this.owner.OutstandingAsyncRequests.Remove(this);
                    }
                    throw exception;
                }
                if (MessageQueue.IsFatalError(num))
                {
                    this.RaiseCompletionEvent(num, overlapped);
                }
            }

            internal Message End()
            {
                this.resetEvent.WaitOne();
                if (MessageQueue.IsFatalError(this.status))
                {
                    throw new MessageQueueException(this.status);
                }
                if (this.owner.formatter != null)
                {
                    this.message.Formatter = (IMessageFormatter) this.owner.formatter.Clone();
                }
                return this.message;
            }

            private unsafe void OnCompletionStatusChanged(uint errorCode, uint numBytes, NativeOverlapped* overlappedPointer)
            {
                int result = 0;
                if (errorCode != 0)
                {
                    long internalLow = (long) overlappedPointer.InternalLow;
                    result = (int) internalLow;
                }
                this.RaiseCompletionEvent(result, overlappedPointer);
            }

            private unsafe void OnMessageReceived(int result, IntPtr handle, int timeout, int action, IntPtr propertiesPointer, NativeOverlapped* overlappedPointer, IntPtr cursorHandle)
            {
                this.RaiseCompletionEvent(result, overlappedPointer);
            }

            private unsafe void RaiseCompletionEvent(int result, NativeOverlapped* overlappedPointer)
            {
                if (MessageQueue.IsMemoryError(result))
                {
                    while (MessageQueue.IsMemoryError(result))
                    {
                        if (this.action == -2147483647)
                        {
                            this.action = -2147483648;
                        }
                        this.message.Unlock();
                        this.message.AdjustMemory();
                        result = this.owner.StaleSafeReceiveMessage(this.timeout, this.action, this.message.Lock(), overlappedPointer, this.onMessageReceived, this.cursorHandle, IntPtr.Zero);
                    }
                    if (!MessageQueue.IsFatalError(result))
                    {
                        return;
                    }
                }
                this.message.Unlock();
                if (this.owner.IsCashedInfoInvalidOnReceive(result))
                {
                    this.owner.MQInfo.Close();
                    result = this.owner.StaleSafeReceiveMessage(this.timeout, this.action, this.message.Lock(), overlappedPointer, this.onMessageReceived, this.cursorHandle, IntPtr.Zero);
                    if (!MessageQueue.IsFatalError(result))
                    {
                        return;
                    }
                }
                this.status = result;
                if (overlappedPointer != null)
                {
                    Overlapped.Free(overlappedPointer);
                }
                this.isCompleted = true;
                this.resetEvent.Set();
                try
                {
                    if ((this.owner.SynchronizingObject != null) && this.owner.SynchronizingObject.InvokeRequired)
                    {
                        this.owner.SynchronizingObject.BeginInvoke(this.callback, new object[] { this });
                    }
                    else
                    {
                        this.callback(this);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (!this.owner.useThreadPool)
                    {
                        this.owner.OutstandingAsyncRequests.Remove(this);
                    }
                }
            }

            internal int Action
            {
                get
                {
                    return this.action;
                }
            }

            public object AsyncState
            {
                get
                {
                    return this.asyncState;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    return this.resetEvent;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return false;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this.isCompleted;
                }
            }
        }

        internal class CacheTable<Key, Value>
        {
            private int capacity;
            private string name;
            private int originalCapacity;
            private ReaderWriterLock rwLock;
            private TimeSpan staleTime;
            private Dictionary<Key, CacheEntry<Key, Value, Value>> table;

            public CacheTable(string name, int capacity, TimeSpan staleTime)
            {
                this.originalCapacity = capacity;
                this.capacity = capacity;
                this.staleTime = staleTime;
                this.name = name;
                this.rwLock = new ReaderWriterLock();
                this.table = new Dictionary<Key, CacheEntry<Key, Value, Value>>();
            }

            public void ClearStale(TimeSpan staleAge)
            {
                DateTime utcNow = DateTime.UtcNow;
                Dictionary<Key, CacheEntry<Key, Value, Value>> dictionary = new Dictionary<Key, CacheEntry<Key, Value, Value>>();
                this.rwLock.AcquireReaderLock(-1);
                try
                {
                    foreach (KeyValuePair<Key, CacheEntry<Key, Value, Value>> pair in this.table)
                    {
                        CacheEntry<Key, Value, Value> entry = pair.Value;
                        if ((utcNow - entry.timeStamp) < staleAge)
                        {
                            dictionary[pair.Key] = pair.Value;
                        }
                    }
                }
                finally
                {
                    this.rwLock.ReleaseReaderLock();
                }
                this.rwLock.AcquireWriterLock(-1);
                this.table = dictionary;
                this.capacity = 2 * this.table.Count;
                if (this.capacity < this.originalCapacity)
                {
                    this.capacity = this.originalCapacity;
                }
                this.rwLock.ReleaseWriterLock();
            }

            public Value Get(Key key)
            {
                Value contents = default(Value);
                this.rwLock.AcquireReaderLock(-1);
                try
                {
                    if (this.table.ContainsKey(key))
                    {
                        CacheEntry<Key, Value, Value> entry = this.table[key];
                        if (entry != null)
                        {
                            entry.timeStamp = DateTime.UtcNow;
                            contents = entry.contents;
                        }
                    }
                }
                finally
                {
                    this.rwLock.ReleaseReaderLock();
                }
                return contents;
            }

            public void Put(Key key, Value val)
            {
                this.rwLock.AcquireWriterLock(-1);
                try
                {
                    if (val == null)
                    {
                        this.table[key] = null;
                    }
                    else
                    {
                        CacheEntry<Key, Value, Value> entry = null;
                        if (this.table.ContainsKey(key))
                        {
                            entry = this.table[key];
                        }
                        if (entry == null)
                        {
                            entry = new CacheEntry<Key, Value, Value>();
                            this.table[key] = entry;
                            if (this.table.Count >= this.capacity)
                            {
                                this.ClearStale(this.staleTime);
                            }
                        }
                        entry.timeStamp = DateTime.UtcNow;
                        entry.contents = val;
                    }
                }
                finally
                {
                    this.rwLock.ReleaseWriterLock();
                }
            }

            public void Remove(Key key)
            {
                this.rwLock.AcquireWriterLock(-1);
                try
                {
                    if (this.table.ContainsKey(key))
                    {
                        this.table.Remove(key);
                    }
                }
                finally
                {
                    this.rwLock.ReleaseWriterLock();
                }
            }

            private class CacheEntry<T>
            {
                public T contents;
                public DateTime timeStamp;
            }
        }

        internal class MQCacheableInfo
        {
            private QueueAccessModeHolder accessMode;
            private bool boundToThreadPool;
            private bool disposed;
            private string formatName;
            private bool isTransactional;
            private bool isTransactional_valid;
            private MessageQueueHandle readHandle = MessageQueueHandle.InvalidHandle;
            private int refCount;
            private int shareMode;
            private object syncRoot = new object();
            private MessageQueueHandle writeHandle = MessageQueueHandle.InvalidHandle;

            public MQCacheableInfo(string formatName, QueueAccessMode accessMode, int shareMode)
            {
                this.formatName = formatName;
                this.shareMode = shareMode;
                this.accessMode = QueueAccessModeHolder.GetQueueAccessModeHolder(accessMode);
            }

            public void AddRef()
            {
                lock (this)
                {
                    this.refCount++;
                }
            }

            public void BindToThreadPool()
            {
                if (!this.boundToThreadPool)
                {
                    lock (this)
                    {
                        if (!this.boundToThreadPool)
                        {
                            new SecurityPermission(PermissionState.Unrestricted).Assert();
                            try
                            {
                                ThreadPool.BindHandle(this.ReadHandle);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                            this.boundToThreadPool = true;
                        }
                    }
                }
            }

            public void Close()
            {
                this.boundToThreadPool = false;
                if (!this.writeHandle.IsInvalid)
                {
                    lock (this.syncRoot)
                    {
                        if (!this.writeHandle.IsInvalid)
                        {
                            this.writeHandle.Close();
                        }
                    }
                }
                if (!this.readHandle.IsInvalid)
                {
                    lock (this.syncRoot)
                    {
                        if (!this.readHandle.IsInvalid)
                        {
                            this.readHandle.Close();
                        }
                    }
                }
            }

            public void CloseIfNotReferenced()
            {
                lock (this)
                {
                    if (this.RefCount == 0)
                    {
                        this.Close();
                    }
                }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Close();
                }
                else
                {
                    if (!this.writeHandle.IsInvalid)
                    {
                        this.writeHandle.Close();
                    }
                    if (!this.readHandle.IsInvalid)
                    {
                        this.readHandle.Close();
                    }
                }
                this.disposed = true;
            }

            ~MQCacheableInfo()
            {
                this.Dispose(false);
            }

            public void Release()
            {
                lock (this)
                {
                    this.refCount--;
                }
            }

            public bool CanRead
            {
                get
                {
                    if (!this.accessMode.CanRead())
                    {
                        return false;
                    }
                    if (this.readHandle.IsInvalid)
                    {
                        if (this.disposed)
                        {
                            throw new ObjectDisposedException(base.GetType().Name);
                        }
                        lock (this.syncRoot)
                        {
                            if (this.readHandle.IsInvalid)
                            {
                                MessageQueueHandle handle;
                                if (MessageQueue.IsFatalError(System.Messaging.Interop.UnsafeNativeMethods.MQOpenQueue(this.formatName, this.accessMode.GetReadAccessMode(), this.shareMode, out handle)))
                                {
                                    return false;
                                }
                                this.readHandle = handle;
                            }
                        }
                    }
                    return true;
                }
            }

            public bool CanWrite
            {
                get
                {
                    if (!this.accessMode.CanWrite())
                    {
                        return false;
                    }
                    if (this.writeHandle.IsInvalid)
                    {
                        if (this.disposed)
                        {
                            throw new ObjectDisposedException(base.GetType().Name);
                        }
                        lock (this.syncRoot)
                        {
                            if (this.writeHandle.IsInvalid)
                            {
                                MessageQueueHandle handle;
                                if (MessageQueue.IsFatalError(System.Messaging.Interop.UnsafeNativeMethods.MQOpenQueue(this.formatName, this.accessMode.GetWriteAccessMode(), 0, out handle)))
                                {
                                    return false;
                                }
                                this.writeHandle = handle;
                            }
                        }
                    }
                    return true;
                }
            }

            public MessageQueueHandle ReadHandle
            {
                get
                {
                    if (this.readHandle.IsInvalid)
                    {
                        if (this.disposed)
                        {
                            throw new ObjectDisposedException(base.GetType().Name);
                        }
                        lock (this.syncRoot)
                        {
                            if (this.readHandle.IsInvalid)
                            {
                                MessageQueueHandle handle;
                                int num = System.Messaging.Interop.UnsafeNativeMethods.MQOpenQueue(this.formatName, this.accessMode.GetReadAccessMode(), this.shareMode, out handle);
                                if (MessageQueue.IsFatalError(num))
                                {
                                    throw new MessageQueueException(num);
                                }
                                this.readHandle = handle;
                            }
                        }
                    }
                    return this.readHandle;
                }
            }

            public int RefCount
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.refCount;
                }
            }

            public bool Transactional
            {
                get
                {
                    if (!this.isTransactional_valid)
                    {
                        lock (this.syncRoot)
                        {
                            if (!this.isTransactional_valid)
                            {
                                QueuePropertyVariants variants = new QueuePropertyVariants();
                                variants.SetUI1(0x71, 0);
                                int num = System.Messaging.Interop.UnsafeNativeMethods.MQGetQueueProperties(this.formatName, variants.Lock());
                                variants.Unlock();
                                if (MessageQueue.IsFatalError(num))
                                {
                                    throw new MessageQueueException(num);
                                }
                                this.isTransactional = variants.GetUI1(0x71) != 0;
                                this.isTransactional_valid = true;
                            }
                        }
                    }
                    return this.isTransactional;
                }
            }

            public MessageQueueHandle WriteHandle
            {
                get
                {
                    if (this.writeHandle.IsInvalid)
                    {
                        if (this.disposed)
                        {
                            throw new ObjectDisposedException(base.GetType().Name);
                        }
                        lock (this.syncRoot)
                        {
                            if (this.writeHandle.IsInvalid)
                            {
                                MessageQueueHandle handle;
                                int num = System.Messaging.Interop.UnsafeNativeMethods.MQOpenQueue(this.formatName, this.accessMode.GetWriteAccessMode(), 0, out handle);
                                if (MessageQueue.IsFatalError(num))
                                {
                                    throw new MessageQueueException(num);
                                }
                                this.writeHandle = handle;
                            }
                        }
                    }
                    return this.writeHandle;
                }
            }
        }

        internal class QueueInfoKeyHolder
        {
            private QueueAccessMode accessMode;
            private string formatName;

            public QueueInfoKeyHolder(string formatName, QueueAccessMode accessMode)
            {
                this.formatName = formatName.ToUpper(CultureInfo.InvariantCulture);
                this.accessMode = accessMode;
            }

            public bool Equals(MessageQueue.QueueInfoKeyHolder qik)
            {
                if (qik == null)
                {
                    return false;
                }
                return ((this.accessMode == qik.accessMode) && this.formatName.Equals(qik.formatName));
            }

            public override bool Equals(object obj)
            {
                if ((obj == null) || (base.GetType() != obj.GetType()))
                {
                    return false;
                }
                MessageQueue.QueueInfoKeyHolder qik = (MessageQueue.QueueInfoKeyHolder) obj;
                return this.Equals(qik);
            }

            public override int GetHashCode()
            {
                return (this.formatName.GetHashCode() + this.accessMode);
            }
        }

        private class QueuePropertyFilter
        {
            public bool Authenticate;
            public bool BasePriority;
            public bool Category;
            public bool CreateTime;
            public bool EncryptionLevel;
            public bool Id;
            public bool Label;
            public bool LastModifyTime;
            public bool MaximumJournalSize;
            public bool MaximumQueueSize;
            public bool MulticastAddress;
            public bool Path;
            public bool Transactional;
            public bool UseJournalQueue;

            public void ClearAll()
            {
                this.Authenticate = false;
                this.BasePriority = false;
                this.CreateTime = false;
                this.EncryptionLevel = false;
                this.Id = false;
                this.Transactional = false;
                this.Label = false;
                this.LastModifyTime = false;
                this.MaximumJournalSize = false;
                this.MaximumQueueSize = false;
                this.Path = false;
                this.Category = false;
                this.UseJournalQueue = false;
                this.MulticastAddress = false;
            }
        }
    }
}

