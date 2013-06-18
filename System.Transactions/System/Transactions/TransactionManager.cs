namespace System.Transactions
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions.Configuration;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    public static class TransactionManager
    {
        private static bool _cachedMaxTimeout;
        private static TimeSpan _defaultTimeout;
        private static bool _defaultTimeoutValidated;
        private static TimeSpan _maximumTimeout;
        internal static bool _platformValidated;
        private static object classSyncObject;
        internal static HostCurrentTransactionCallback currentDelegate = null;
        internal static bool currentDelegateSet = false;
        private const int currentRecoveryVersion = 1;
        private static DefaultSettingsSection defaultSettings;
        internal static OletxTransactionManager distributedTransactionManager;
        private static TransactionStartedEventHandler distributedTransactionStartedDelegate;
        private static MachineSettingsSection machineSettings;
        private static Hashtable promotedTransactionTable;
        private const int recoveryInformationVersion1 = 1;
        private static System.Transactions.TransactionTable transactionTable;

        public static  event TransactionStartedEventHandler DistributedTransactionStarted
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] add
            {
                if (!_platformValidated)
                {
                    ValidatePlatform();
                }
                lock (ClassSyncObject)
                {
                    distributedTransactionStartedDelegate = (TransactionStartedEventHandler) Delegate.Combine(distributedTransactionStartedDelegate, value);
                    if (value != null)
                    {
                        ProcessExistingTransactions(value);
                    }
                }
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] remove
            {
                if (!_platformValidated)
                {
                    ValidatePlatform();
                }
                lock (ClassSyncObject)
                {
                    distributedTransactionStartedDelegate = (TransactionStartedEventHandler) Delegate.Remove(distributedTransactionStartedDelegate, value);
                }
            }
        }

        private static OletxTransactionManager CheckTransactionManager(string nodeName)
        {
            OletxTransactionManager distributedTransactionManager = DistributedTransactionManager;
            if (((distributedTransactionManager.NodeName != null) || ((nodeName != null) && (nodeName.Length != 0))) && ((distributedTransactionManager.NodeName == null) || !distributedTransactionManager.NodeName.Equals(nodeName)))
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidRecoveryInformation"), "recoveryInformation");
            }
            return distributedTransactionManager;
        }

        internal static byte[] ConvertToByteArray(object thingToConvert)
        {
            MemoryStream serializationStream = new MemoryStream();
            byte[] buffer = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(serializationStream, thingToConvert);
                buffer = new byte[serializationStream.Length];
                serializationStream.Position = 0L;
                serializationStream.Read(buffer, 0, Convert.ToInt32(serializationStream.Length, CultureInfo.InvariantCulture));
            }
            finally
            {
                serializationStream.Close();
            }
            return buffer;
        }

        internal static Transaction FindOrCreatePromotedTransaction(Guid transactionIdentifier, OletxTransaction oletx)
        {
            Transaction target = null;
            Hashtable promotedTransactionTable = PromotedTransactionTable;
            lock (promotedTransactionTable)
            {
                WeakReference reference = (WeakReference) promotedTransactionTable[transactionIdentifier];
                if (reference != null)
                {
                    target = reference.Target as Transaction;
                    if (null != target)
                    {
                        oletx.Dispose();
                        return target.InternalClone();
                    }
                    lock (promotedTransactionTable)
                    {
                        promotedTransactionTable.Remove(transactionIdentifier);
                    }
                }
                target = new Transaction(oletx) {
                    internalTransaction = { finalizedObject = new FinalizedObject(target.internalTransaction, oletx.Identifier) }
                };
                reference = new WeakReference(target, false);
                promotedTransactionTable[oletx.Identifier] = reference;
            }
            oletx.savedLtmPromotedTransaction = target;
            FireDistributedTransactionStarted(target);
            return target;
        }

        internal static Transaction FindPromotedTransaction(Guid transactionIdentifier)
        {
            Hashtable promotedTransactionTable = PromotedTransactionTable;
            WeakReference reference = (WeakReference) promotedTransactionTable[transactionIdentifier];
            if (reference != null)
            {
                Transaction target = reference.Target as Transaction;
                if (null != target)
                {
                    return target.InternalClone();
                }
                lock (promotedTransactionTable)
                {
                    promotedTransactionTable.Remove(transactionIdentifier);
                }
            }
            return null;
        }

        internal static void FireDistributedTransactionStarted(Transaction transaction)
        {
            TransactionStartedEventHandler distributedTransactionStartedDelegate = null;
            lock (ClassSyncObject)
            {
                distributedTransactionStartedDelegate = TransactionManager.distributedTransactionStartedDelegate;
            }
            if (distributedTransactionStartedDelegate != null)
            {
                TransactionEventArgs e = new TransactionEventArgs {
                    transaction = transaction.InternalClone()
                };
                distributedTransactionStartedDelegate(e.transaction, e);
            }
        }

        internal static byte[] GetRecoveryInformation(string startupInfo, byte[] resourceManagerRecoveryInformation)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.GetRecoveryInformation");
            }
            MemoryStream output = new MemoryStream();
            byte[] buffer = null;
            try
            {
                BinaryWriter writer = new BinaryWriter(output);
                writer.Write(1);
                if (startupInfo != null)
                {
                    writer.Write(startupInfo);
                }
                else
                {
                    writer.Write("");
                }
                writer.Write(resourceManagerRecoveryInformation);
                writer.Flush();
                buffer = output.ToArray();
            }
            finally
            {
                output.Close();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.GetRecoveryInformation");
            }
            return buffer;
        }

        internal static void ProcessExistingTransactions(TransactionStartedEventHandler eventHandler)
        {
            lock (PromotedTransactionTable)
            {
                foreach (DictionaryEntry entry in PromotedTransactionTable)
                {
                    WeakReference reference = (WeakReference) entry.Value;
                    Transaction target = (Transaction) reference.Target;
                    if (target != null)
                    {
                        TransactionEventArgs e = new TransactionEventArgs {
                            transaction = target.InternalClone()
                        };
                        eventHandler(e.transaction, e);
                    }
                }
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void RecoveryComplete(Guid resourceManagerIdentifier)
        {
            if (resourceManagerIdentifier == Guid.Empty)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("BadResourceManagerId"), "resourceManagerIdentifier");
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.RecoveryComplete");
            }
            if (DiagnosticTrace.Information)
            {
                RecoveryCompleteTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), resourceManagerIdentifier);
            }
            DistributedTransactionManager.ResourceManagerRecoveryComplete(resourceManagerIdentifier);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.RecoveryComplete");
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static Enlistment Reenlist(Guid resourceManagerIdentifier, byte[] recoveryInformation, IEnlistmentNotification enlistmentNotification)
        {
            if (resourceManagerIdentifier == Guid.Empty)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("BadResourceManagerId"), "resourceManagerIdentifier");
            }
            if (recoveryInformation == null)
            {
                throw new ArgumentNullException("recoveryInformation");
            }
            if (enlistmentNotification == null)
            {
                throw new ArgumentNullException("enlistmentNotification");
            }
            if (!_platformValidated)
            {
                ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.Reenlist");
            }
            if (DiagnosticTrace.Information)
            {
                ReenlistTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), resourceManagerIdentifier);
            }
            MemoryStream input = new MemoryStream(recoveryInformation);
            string nodeName = null;
            byte[] buffer = null;
            try
            {
                BinaryReader reader = new BinaryReader(input);
                if (reader.ReadInt32() != 1)
                {
                    if (DiagnosticTrace.Error)
                    {
                        TransactionExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("UnrecognizedRecoveryInformation"));
                    }
                    throw new ArgumentException(System.Transactions.SR.GetString("UnrecognizedRecoveryInformation"), "recoveryInformation");
                }
                nodeName = reader.ReadString();
                buffer = reader.ReadBytes(recoveryInformation.Length - ((int) input.Position));
            }
            catch (EndOfStreamException exception2)
            {
                if (DiagnosticTrace.Error)
                {
                    TransactionExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("UnrecognizedRecoveryInformation"));
                }
                throw new ArgumentException(System.Transactions.SR.GetString("UnrecognizedRecoveryInformation"), "recoveryInformation", exception2);
            }
            catch (FormatException exception)
            {
                if (DiagnosticTrace.Error)
                {
                    TransactionExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("UnrecognizedRecoveryInformation"));
                }
                throw new ArgumentException(System.Transactions.SR.GetString("UnrecognizedRecoveryInformation"), "recoveryInformation", exception);
            }
            finally
            {
                input.Close();
            }
            OletxTransactionManager manager = CheckTransactionManager(nodeName);
            object syncRoot = new object();
            Enlistment enlistment = new Enlistment(enlistmentNotification, syncRoot);
            EnlistmentState._EnlistmentStatePromoted.EnterState(enlistment.InternalEnlistment);
            enlistment.InternalEnlistment.PromotedEnlistment = manager.ReenlistTransaction(resourceManagerIdentifier, buffer, (RecoveringInternalEnlistment) enlistment.InternalEnlistment);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.Reenlist");
            }
            return enlistment;
        }

        internal static void ValidateIsolationLevel(IsolationLevel transactionIsolationLevel)
        {
            switch (transactionIsolationLevel)
            {
                case IsolationLevel.Serializable:
                case IsolationLevel.RepeatableRead:
                case IsolationLevel.ReadCommitted:
                case IsolationLevel.ReadUncommitted:
                case IsolationLevel.Snapshot:
                case IsolationLevel.Chaos:
                case IsolationLevel.Unspecified:
                    return;
            }
            throw new ArgumentOutOfRangeException("transactionIsolationLevel");
        }

        internal static void ValidatePlatform()
        {
            if (PlatformID.Win32NT != Environment.OSVersion.Platform)
            {
                throw new PlatformNotSupportedException(System.Transactions.SR.GetString("OnlySupportedOnWinNT"));
            }
            _platformValidated = true;
        }

        internal static TimeSpan ValidateTimeout(TimeSpan transactionTimeout)
        {
            if (transactionTimeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("transactionTimeout");
            }
            if (!(MaximumTimeout != TimeSpan.Zero) || ((transactionTimeout <= MaximumTimeout) && !(transactionTimeout == TimeSpan.Zero)))
            {
                return transactionTimeout;
            }
            return MaximumTimeout;
        }

        private static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }

        internal static IsolationLevel DefaultIsolationLevel
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.get_DefaultIsolationLevel");
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.get_DefaultIsolationLevel");
                }
                return IsolationLevel.Serializable;
            }
        }

        private static DefaultSettingsSection DefaultSettings
        {
            get
            {
                if (defaultSettings == null)
                {
                    defaultSettings = DefaultSettingsSection.GetSection();
                }
                return defaultSettings;
            }
        }

        public static TimeSpan DefaultTimeout
        {
            get
            {
                if (!_platformValidated)
                {
                    ValidatePlatform();
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.get_DefaultTimeout");
                }
                if (!_defaultTimeoutValidated)
                {
                    _defaultTimeout = ValidateTimeout(DefaultSettings.Timeout);
                    if ((_defaultTimeout != DefaultSettings.Timeout) && DiagnosticTrace.Warning)
                    {
                        ConfiguredDefaultTimeoutAdjustedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"));
                    }
                    _defaultTimeoutValidated = true;
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.get_DefaultTimeout");
                }
                return _defaultTimeout;
            }
        }

        internal static OletxTransactionManager DistributedTransactionManager
        {
            get
            {
                if (distributedTransactionManager == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (distributedTransactionManager == null)
                        {
                            OletxTransactionManager manager = new OletxTransactionManager(DefaultSettings.DistributedTransactionManagerName);
                            Thread.MemoryBarrier();
                            distributedTransactionManager = manager;
                        }
                    }
                }
                return distributedTransactionManager;
            }
        }

        public static HostCurrentTransactionCallback HostCurrentCallback
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (!_platformValidated)
                {
                    ValidatePlatform();
                }
                return currentDelegate;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                if (!_platformValidated)
                {
                    ValidatePlatform();
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                lock (ClassSyncObject)
                {
                    if (currentDelegateSet)
                    {
                        throw new InvalidOperationException(System.Transactions.SR.GetString("CurrentDelegateSet"));
                    }
                    currentDelegateSet = true;
                }
                currentDelegate = value;
            }
        }

        private static MachineSettingsSection MachineSettings
        {
            get
            {
                if (machineSettings == null)
                {
                    machineSettings = MachineSettingsSection.GetSection();
                }
                return machineSettings;
            }
        }

        public static TimeSpan MaximumTimeout
        {
            get
            {
                if (!_platformValidated)
                {
                    ValidatePlatform();
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.get_DefaultMaximumTimeout");
                }
                if (!_cachedMaxTimeout)
                {
                    lock (ClassSyncObject)
                    {
                        if (!_cachedMaxTimeout)
                        {
                            TimeSpan maxTimeout = MachineSettings.MaxTimeout;
                            Thread.MemoryBarrier();
                            _maximumTimeout = maxTimeout;
                            _cachedMaxTimeout = true;
                        }
                    }
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionManager.get_DefaultMaximumTimeout");
                }
                return _maximumTimeout;
            }
        }

        internal static Hashtable PromotedTransactionTable
        {
            get
            {
                if (promotedTransactionTable == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (promotedTransactionTable == null)
                        {
                            Hashtable hashtable = new Hashtable(100);
                            Thread.MemoryBarrier();
                            promotedTransactionTable = hashtable;
                        }
                    }
                }
                return promotedTransactionTable;
            }
        }

        internal static System.Transactions.TransactionTable TransactionTable
        {
            get
            {
                if (transactionTable == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (transactionTable == null)
                        {
                            System.Transactions.TransactionTable table = new System.Transactions.TransactionTable();
                            Thread.MemoryBarrier();
                            transactionTable = table;
                        }
                    }
                }
                return transactionTable;
            }
        }
    }
}

