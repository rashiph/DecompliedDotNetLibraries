namespace System.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Transactions;

    internal static class Fx
    {
        [SecurityCritical]
        private static ExceptionHandler asynchronousThreadExceptionHandler;
        private const string defaultEventSource = "System.Runtime";
        private static DiagnosticTrace diagnosticTrace;
        private static ExceptionTrace exceptionTrace;

        public static byte[] AllocateByteArray(int size)
        {
            byte[] buffer;
            try
            {
                buffer = new byte[size];
            }
            catch (OutOfMemoryException exception)
            {
                throw Exception.AsError(new InsufficientMemoryException(SRCore.BufferAllocationFailed(size), exception));
            }
            return buffer;
        }

        public static char[] AllocateCharArray(int size)
        {
            char[] chArray;
            try
            {
                chArray = new char[size];
            }
            catch (OutOfMemoryException exception)
            {
                throw Exception.AsError(new InsufficientMemoryException(SRCore.BufferAllocationFailed(size * 2), exception));
            }
            return chArray;
        }

        [Conditional("DEBUG")]
        public static void Assert(string description)
        {
            AssertHelper.FireAssert(description);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string description)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static System.Exception AssertAndFailFast(string description)
        {
            string message = SRCore.FailFastMessage(description);
            try
            {
                try
                {
                    Exception.TraceFailFast(message);
                }
                finally
                {
                    Environment.FailFast(message);
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        public static void AssertAndFailFast(bool condition, string description)
        {
            if (!condition)
            {
                AssertAndFailFast(description);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static System.Exception AssertAndThrow(string description)
        {
            TraceCore.ShipAssertExceptionMessage(Trace, description);
            throw new InternalException(description);
        }

        public static void AssertAndThrow(bool condition, string description)
        {
            if (!condition)
            {
                AssertAndThrow(description);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static System.Exception AssertAndThrowFatal(string description)
        {
            TraceCore.ShipAssertExceptionMessage(Trace, description);
            throw new FatalInternalException(description);
        }

        public static void AssertAndThrowFatal(bool condition, string description)
        {
            if (!condition)
            {
                AssertAndThrowFatal(description);
            }
        }

        public static void CompleteTransactionScope(ref TransactionScope scope)
        {
            TransactionScope scope2 = scope;
            if (scope2 != null)
            {
                scope = null;
                try
                {
                    scope2.Complete();
                }
                finally
                {
                    scope2.Dispose();
                }
            }
        }

        public static Guid CreateGuid(string guidString)
        {
            bool flag = false;
            Guid empty = Guid.Empty;
            try
            {
                empty = new Guid(guidString);
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    AssertAndThrow("Creation of the Guid failed.");
                }
            }
            return empty;
        }

        public static TransactionScope CreateTransactionScope(Transaction transaction)
        {
            TransactionScope scope;
            try
            {
                scope = (transaction == null) ? null : new TransactionScope(transaction);
            }
            catch (TransactionAbortedException)
            {
                CommittableTransaction transaction2 = new CommittableTransaction();
                try
                {
                    scope = new TransactionScope(transaction2.Clone());
                }
                finally
                {
                    transaction2.Rollback();
                }
            }
            return scope;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static bool HandleAtThreadBase(System.Exception exception)
        {
            if (exception != null)
            {
                TraceExceptionNoThrow(exception);
                try
                {
                    ExceptionHandler asynchronousThreadExceptionHandler = AsynchronousThreadExceptionHandler;
                    return ((asynchronousThreadExceptionHandler != null) && asynchronousThreadExceptionHandler.HandleException(exception));
                }
                catch (System.Exception exception2)
                {
                    TraceExceptionNoThrow(exception2);
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        private static DiagnosticTrace InitializeTracing()
        {
            return new DiagnosticTrace("System.Runtime", DiagnosticTrace.DefaultEtwProviderId);
        }

        public static bool IsFatal(System.Exception exception)
        {
            while (exception != null)
            {
                if (((exception is FatalException) || ((exception is OutOfMemoryException) && !(exception is InsufficientMemoryException))) || ((exception is ThreadAbortException) || (exception is FatalInternalException)))
                {
                    return true;
                }
                if (!(exception is TypeInitializationException) && !(exception is TargetInvocationException))
                {
                    break;
                }
                exception = exception.InnerException;
            }
            return false;
        }

        public static void ThrowIfTransactionAbortedOrInDoubt(Transaction transaction)
        {
            if ((transaction != null) && ((transaction.TransactionInformation.Status == TransactionStatus.Aborted) || (transaction.TransactionInformation.Status == TransactionStatus.InDoubt)))
            {
                using (new TransactionScope(transaction))
                {
                }
            }
        }

        public static AsyncCallback ThunkCallback(AsyncCallback callback)
        {
            return new AsyncThunk(callback).ThunkFrame;
        }

        [SecurityCritical]
        public static IOCompletionCallback ThunkCallback(IOCompletionCallback callback)
        {
            return new IOCompletionThunk(callback).ThunkFrame;
        }

        public static SendOrPostCallback ThunkCallback(SendOrPostCallback callback)
        {
            return new SendOrPostThunk(callback).ThunkFrame;
        }

        public static TimerCallback ThunkCallback(TimerCallback callback)
        {
            return new TimerThunk(callback).ThunkFrame;
        }

        public static WaitCallback ThunkCallback(WaitCallback callback)
        {
            return new WaitThunk(callback).ThunkFrame;
        }

        public static WaitOrTimerCallback ThunkCallback(WaitOrTimerCallback callback)
        {
            return new WaitOrTimerThunk(callback).ThunkFrame;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static void TraceExceptionNoThrow(System.Exception exception)
        {
            try
            {
                Exception.TraceUnhandledException(exception);
            }
            catch
            {
            }
        }

        public static bool TryCreateGuid(string guidString, out Guid result)
        {
            bool flag = false;
            result = Guid.Empty;
            try
            {
                result = new Guid(guidString);
                flag = true;
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return flag;
        }

        internal static bool AssertsFailFast
        {
            get
            {
                return false;
            }
        }

        public static ExceptionHandler AsynchronousThreadExceptionHandler
        {
            [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return asynchronousThreadExceptionHandler;
            }
            [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                asynchronousThreadExceptionHandler = value;
            }
        }

        internal static Type[] BreakOnExceptionTypes
        {
            get
            {
                return null;
            }
        }

        public static ExceptionTrace Exception
        {
            get
            {
                if (exceptionTrace == null)
                {
                    exceptionTrace = new ExceptionTrace("System.Runtime");
                }
                return exceptionTrace;
            }
        }

        internal static bool FastDebug
        {
            get
            {
                return false;
            }
        }

        internal static bool StealthDebugger
        {
            get
            {
                return false;
            }
        }

        public static DiagnosticTrace Trace
        {
            get
            {
                if (diagnosticTrace == null)
                {
                    diagnosticTrace = InitializeTracing();
                }
                return diagnosticTrace;
            }
        }

        private sealed class AsyncThunk : Fx.Thunk<AsyncCallback>
        {
            public AsyncThunk(AsyncCallback callback) : base(callback)
            {
            }

            [SecuritySafeCritical]
            private void UnhandledExceptionFrame(IAsyncResult result)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.Callback(result);
                }
                catch (Exception exception)
                {
                    if (!Fx.HandleAtThreadBase(exception))
                    {
                        throw;
                    }
                }
            }

            public AsyncCallback ThunkFrame
            {
                get
                {
                    return new AsyncCallback(this.UnhandledExceptionFrame);
                }
            }
        }

        public abstract class ExceptionHandler
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected ExceptionHandler()
            {
            }

            public abstract bool HandleException(Exception exception);
        }

        [Serializable]
        private class FatalInternalException : Fx.InternalException
        {
            public FatalInternalException(string description) : base(description)
            {
            }

            protected FatalInternalException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        private class InternalException : SystemException
        {
            public InternalException(string description) : base(SRCore.ShipAssertExceptionMessage(description))
            {
            }

            protected InternalException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        [SecurityCritical]
        private sealed class IOCompletionThunk
        {
            private IOCompletionCallback callback;

            public IOCompletionThunk(IOCompletionCallback callback)
            {
                this.callback = callback;
            }

            private unsafe void UnhandledExceptionFrame(uint error, uint bytesRead, NativeOverlapped* nativeOverlapped)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this.callback(error, bytesRead, nativeOverlapped);
                }
                catch (Exception exception)
                {
                    if (!Fx.HandleAtThreadBase(exception))
                    {
                        throw;
                    }
                }
            }

            public IOCompletionCallback ThunkFrame
            {
                get
                {
                    return new IOCompletionCallback(this.UnhandledExceptionFrame);
                }
            }
        }

        private sealed class SendOrPostThunk : Fx.Thunk<SendOrPostCallback>
        {
            public SendOrPostThunk(SendOrPostCallback callback) : base(callback)
            {
            }

            [SecuritySafeCritical]
            private void UnhandledExceptionFrame(object state)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.Callback(state);
                }
                catch (Exception exception)
                {
                    if (!Fx.HandleAtThreadBase(exception))
                    {
                        throw;
                    }
                }
            }

            public SendOrPostCallback ThunkFrame
            {
                get
                {
                    return new SendOrPostCallback(this.UnhandledExceptionFrame);
                }
            }
        }

        public static class Tag
        {
            [Conditional("CODE_ANALYSIS_CDF"), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false)]
            public sealed class BlockingAttribute : Attribute
            {
                public Type CancelDeclaringType
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<CancelDeclaringType>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<CancelDeclaringType>k__BackingField = value;
                    }
                }

                public string CancelMethod
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<CancelMethod>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<CancelMethod>k__BackingField = value;
                    }
                }

                public string Conditional
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Conditional>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Conditional>k__BackingField = value;
                    }
                }
            }

            [Flags]
            public enum BlocksUsing
            {
                MonitorEnter,
                MonitorWait,
                ManualResetEvent,
                AutoResetEvent,
                AsyncResult,
                IAsyncResult,
                PInvoke,
                InputQueue,
                ThreadNeutralSemaphore,
                PrivatePrimitive,
                OtherInternalPrimitive,
                OtherFrameworkPrimitive,
                OtherInterop,
                Other,
                NonBlocking
            }

            [AttributeUsage(AttributeTargets.Field), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class CacheAttribute : Attribute
            {
                private readonly System.Runtime.Fx.Tag.CacheAttrition cacheAttrition;
                private readonly Type elementType;

                public CacheAttribute(Type elementType, System.Runtime.Fx.Tag.CacheAttrition cacheAttrition)
                {
                    this.Scope = "instance of declaring class";
                    this.SizeLimit = "unbounded";
                    this.Timeout = "infinite";
                    if (elementType == null)
                    {
                        throw Fx.Exception.ArgumentNull("elementType");
                    }
                    this.elementType = elementType;
                    this.cacheAttrition = cacheAttrition;
                }

                public System.Runtime.Fx.Tag.CacheAttrition CacheAttrition
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.cacheAttrition;
                    }
                }

                public Type ElementType
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.elementType;
                    }
                }

                public string Scope
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Scope>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Scope>k__BackingField = value;
                    }
                }

                public string SizeLimit
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<SizeLimit>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<SizeLimit>k__BackingField = value;
                    }
                }

                public string Timeout
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Timeout>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Timeout>k__BackingField = value;
                    }
                }
            }

            public enum CacheAttrition
            {
                None,
                ElementOnTimer,
                ElementOnGC,
                ElementOnCallback,
                FullPurgeOnTimer,
                FullPurgeOnEachAccess,
                PartialPurgeOnTimer,
                PartialPurgeOnEachAccess
            }

            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=true, Inherited=false), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class ExternalResourceAttribute : Attribute
            {
                private readonly string description;
                private readonly System.Runtime.Fx.Tag.Location location;

                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                public ExternalResourceAttribute(System.Runtime.Fx.Tag.Location location, string description)
                {
                    this.location = location;
                    this.description = description;
                }

                public string Description
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.description;
                    }
                }

                public System.Runtime.Fx.Tag.Location Location
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.location;
                    }
                }
            }

            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, AllowMultiple=true, Inherited=false), Conditional("DEBUG")]
            public sealed class FriendAccessAllowedAttribute : Attribute
            {
                public FriendAccessAllowedAttribute(string assemblyName)
                {
                    this.AssemblyName = assemblyName;
                }

                public string AssemblyName
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<AssemblyName>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<AssemblyName>k__BackingField = value;
                    }
                }
            }

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class GuaranteeNonBlockingAttribute : Attribute
            {
            }

            [Conditional("CODE_ANALYSIS_CDF"), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false)]
            public sealed class InheritThrowsAttribute : Attribute
            {
                public string From
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<From>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<From>k__BackingField = value;
                    }
                }

                public Type FromDeclaringType
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<FromDeclaringType>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<FromDeclaringType>k__BackingField = value;
                    }
                }
            }

            [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class KnownXamlExternalAttribute : Attribute
            {
            }

            public enum Location
            {
                InProcess,
                OutOfProcess,
                LocalSystem,
                LocalOrRemoteSystem,
                RemoteSystem
            }

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class NonThrowingAttribute : Attribute
            {
            }

            [AttributeUsage(AttributeTargets.Field), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class QueueAttribute : Attribute
            {
                private readonly Type elementType;

                public QueueAttribute(Type elementType)
                {
                    this.Scope = "instance of declaring class";
                    this.SizeLimit = "unbounded";
                    if (elementType == null)
                    {
                        throw Fx.Exception.ArgumentNull("elementType");
                    }
                    this.elementType = elementType;
                }

                public Type ElementType
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.elementType;
                    }
                }

                public bool EnqueueThrowsIfFull
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<EnqueueThrowsIfFull>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<EnqueueThrowsIfFull>k__BackingField = value;
                    }
                }

                public string Scope
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Scope>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Scope>k__BackingField = value;
                    }
                }

                public string SizeLimit
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<SizeLimit>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<SizeLimit>k__BackingField = value;
                    }
                }

                public bool StaleElementsRemovedImmediately
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<StaleElementsRemovedImmediately>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<StaleElementsRemovedImmediately>k__BackingField = value;
                    }
                }
            }

            [Conditional("CODE_ANALYSIS_CDF"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Module | AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
            public sealed class SecurityNoteAttribute : Attribute
            {
                public string Critical
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Critical>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Critical>k__BackingField = value;
                    }
                }

                public string Miscellaneous
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Miscellaneous>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Miscellaneous>k__BackingField = value;
                    }
                }

                public string Safe
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Safe>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Safe>k__BackingField = value;
                    }
                }
            }

            public static class Strings
            {
                internal const string AppDomain = "AppDomain";
                internal const string DeclaringInstance = "instance of declaring class";
                internal const string ExternallyManaged = "externally managed";
                internal const string Infinite = "infinite";
                internal const string Unbounded = "unbounded";
            }

            public enum SynchronizationKind
            {
                LockStatement,
                MonitorWait,
                MonitorExplicit,
                InterlockedNoSpin,
                InterlockedWithSpin,
                FromFieldType
            }

            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited=false), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class SynchronizationObjectAttribute : Attribute
            {
                public SynchronizationObjectAttribute()
                {
                    this.Blocking = true;
                    this.Scope = "instance of declaring class";
                    this.Kind = Fx.Tag.SynchronizationKind.FromFieldType;
                }

                public bool Blocking
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Blocking>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Blocking>k__BackingField = value;
                    }
                }

                public Fx.Tag.SynchronizationKind Kind
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Kind>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Kind>k__BackingField = value;
                    }
                }

                public string Scope
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Scope>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Scope>k__BackingField = value;
                    }
                }
            }

            [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited=true), Conditional("CODE_ANALYSIS_CDF")]
            public sealed class SynchronizationPrimitiveAttribute : Attribute
            {
                private readonly System.Runtime.Fx.Tag.BlocksUsing blocksUsing;

                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                public SynchronizationPrimitiveAttribute(System.Runtime.Fx.Tag.BlocksUsing blocksUsing)
                {
                    this.blocksUsing = blocksUsing;
                }

                public System.Runtime.Fx.Tag.BlocksUsing BlocksUsing
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.blocksUsing;
                    }
                }

                public string ReleaseMethod
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<ReleaseMethod>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<ReleaseMethod>k__BackingField = value;
                    }
                }

                public bool Spins
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Spins>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Spins>k__BackingField = value;
                    }
                }

                public bool SupportsAsync
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<SupportsAsync>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<SupportsAsync>k__BackingField = value;
                    }
                }
            }

            public enum ThrottleAction
            {
                Reject,
                Pause
            }

            [Conditional("CODE_ANALYSIS_CDF"), AttributeUsage(AttributeTargets.Field)]
            public sealed class ThrottleAttribute : Attribute
            {
                private readonly string limit;
                private readonly System.Runtime.Fx.Tag.ThrottleAction throttleAction;
                private readonly System.Runtime.Fx.Tag.ThrottleMetric throttleMetric;

                public ThrottleAttribute(System.Runtime.Fx.Tag.ThrottleAction throttleAction, System.Runtime.Fx.Tag.ThrottleMetric throttleMetric, string limit)
                {
                    this.Scope = "AppDomain";
                    if (string.IsNullOrEmpty(limit))
                    {
                        throw Fx.Exception.ArgumentNullOrEmpty("limit");
                    }
                    this.throttleAction = throttleAction;
                    this.throttleMetric = throttleMetric;
                    this.limit = limit;
                }

                public string Limit
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.limit;
                    }
                }

                public string Scope
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Scope>k__BackingField;
                    }
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.<Scope>k__BackingField = value;
                    }
                }

                public System.Runtime.Fx.Tag.ThrottleAction ThrottleAction
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.throttleAction;
                    }
                }

                public System.Runtime.Fx.Tag.ThrottleMetric ThrottleMetric
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.throttleMetric;
                    }
                }
            }

            public enum ThrottleMetric
            {
                Count,
                Rate,
                Other
            }

            public static class Throws
            {
                [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=true, Inherited=false), Conditional("CODE_ANALYSIS_CDF")]
                public sealed class TimeoutAttribute : Fx.Tag.ThrowsAttribute
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    public TimeoutAttribute() : this("The operation timed out.")
                    {
                    }

                    public TimeoutAttribute(string diagnosis) : base(typeof(TimeoutException), diagnosis)
                    {
                    }
                }
            }

            [Conditional("CODE_ANALYSIS_CDF"), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=true, Inherited=false)]
            public class ThrowsAttribute : Attribute
            {
                private readonly string diagnosis;
                private readonly Type exceptionType;

                public ThrowsAttribute(Type exceptionType, string diagnosis)
                {
                    if (exceptionType == null)
                    {
                        throw Fx.Exception.ArgumentNull("exceptionType");
                    }
                    if (string.IsNullOrEmpty(diagnosis))
                    {
                        throw Fx.Exception.ArgumentNullOrEmpty("diagnosis");
                    }
                    this.exceptionType = exceptionType;
                    this.diagnosis = diagnosis;
                }

                public string Diagnosis
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.diagnosis;
                    }
                }

                public Type ExceptionType
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.exceptionType;
                    }
                }
            }

            [Conditional("CODE_ANALYSIS_CDF"), AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
            public sealed class XamlVisibleAttribute : Attribute
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                public XamlVisibleAttribute() : this(true)
                {
                }

                public XamlVisibleAttribute(bool visible)
                {
                    this.Visible = visible;
                }

                public bool Visible
                {
                    [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.<Visible>k__BackingField;
                    }
                    [CompilerGenerated]
                    private set
                    {
                        this.<Visible>k__BackingField = value;
                    }
                }
            }
        }

        private abstract class Thunk<T> where T: class
        {
            [SecurityCritical]
            private T callback;

            [SecuritySafeCritical]
            protected Thunk(T callback)
            {
                this.callback = callback;
            }

            internal T Callback
            {
                [SecuritySafeCritical]
                get
                {
                    return this.callback;
                }
            }
        }

        private sealed class TimerThunk : Fx.Thunk<TimerCallback>
        {
            public TimerThunk(TimerCallback callback) : base(callback)
            {
            }

            [SecuritySafeCritical]
            private void UnhandledExceptionFrame(object state)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.Callback(state);
                }
                catch (Exception exception)
                {
                    if (!Fx.HandleAtThreadBase(exception))
                    {
                        throw;
                    }
                }
            }

            public TimerCallback ThunkFrame
            {
                get
                {
                    return new TimerCallback(this.UnhandledExceptionFrame);
                }
            }
        }

        private sealed class WaitOrTimerThunk : Fx.Thunk<WaitOrTimerCallback>
        {
            public WaitOrTimerThunk(WaitOrTimerCallback callback) : base(callback)
            {
            }

            [SecuritySafeCritical]
            private void UnhandledExceptionFrame(object state, bool timedOut)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.Callback(state, timedOut);
                }
                catch (Exception exception)
                {
                    if (!Fx.HandleAtThreadBase(exception))
                    {
                        throw;
                    }
                }
            }

            public WaitOrTimerCallback ThunkFrame
            {
                get
                {
                    return new WaitOrTimerCallback(this.UnhandledExceptionFrame);
                }
            }
        }

        private sealed class WaitThunk : Fx.Thunk<WaitCallback>
        {
            public WaitThunk(WaitCallback callback) : base(callback)
            {
            }

            [SecuritySafeCritical]
            private void UnhandledExceptionFrame(object state)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.Callback(state);
                }
                catch (Exception exception)
                {
                    if (!Fx.HandleAtThreadBase(exception))
                    {
                        throw;
                    }
                }
            }

            public WaitCallback ThunkFrame
            {
                get
                {
                    return new WaitCallback(this.UnhandledExceptionFrame);
                }
            }
        }
    }
}

