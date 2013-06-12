namespace System
{
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_Exception))]
    public class Exception : ISerializable, _Exception
    {
        private string _className;
        private const int _COMPlusExceptionCode = -532462766;
        private IDictionary _data;
        private object _dynamicMethods;
        private MethodBase _exceptionMethod;
        private string _exceptionMethodString;
        private string _helpURL;
        internal int _HResult;
        private Exception _innerException;
        [OptionalField]
        private UIntPtr _ipForWatsonBuckets;
        internal string _message;
        private int _remoteStackIndex;
        private string _remoteStackTraceString;
        [OptionalField(VersionAdded=4)]
        private SafeSerializationManager _safeSerializationManager;
        private string _source;
        private object _stackTrace;
        private string _stackTraceString;
        [OptionalField]
        private object _watsonBuckets;
        private int _xcode;
        private IntPtr _xptrs;

        protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState
        {
            add
            {
                this._safeSerializationManager.SerializeObjectState += value;
            }
            remove
            {
                this._safeSerializationManager.SerializeObjectState -= value;
            }
        }

        public Exception()
        {
            this.Init();
        }

        public Exception(string message)
        {
            this.Init();
            this._message = message;
        }

        [SecuritySafeCritical]
        protected Exception(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this._className = info.GetString("ClassName");
            this._message = info.GetString("Message");
            this._data = (IDictionary) info.GetValueNoThrow("Data", typeof(IDictionary));
            this._innerException = (Exception) info.GetValue("InnerException", typeof(Exception));
            this._helpURL = info.GetString("HelpURL");
            this._stackTraceString = info.GetString("StackTraceString");
            this._remoteStackTraceString = info.GetString("RemoteStackTraceString");
            this._remoteStackIndex = info.GetInt32("RemoteStackIndex");
            this._exceptionMethodString = (string) info.GetValue("ExceptionMethod", typeof(string));
            this.HResult = info.GetInt32("HResult");
            this._source = info.GetString("Source");
            this._watsonBuckets = info.GetValueNoThrow("WatsonBuckets", typeof(byte[]));
            this._safeSerializationManager = info.GetValueNoThrow("SafeSerializationManager", typeof(SafeSerializationManager)) as SafeSerializationManager;
            if ((this._className == null) || (this.HResult == 0))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
            }
            if (context.State == StreamingContextStates.CrossAppDomain)
            {
                this._remoteStackTraceString = this._remoteStackTraceString + this._stackTraceString;
                this._stackTraceString = null;
            }
        }

        public Exception(string message, Exception innerException)
        {
            this.Init();
            this._message = message;
            this._innerException = innerException;
        }

        public virtual Exception GetBaseException()
        {
            Exception innerException = this.InnerException;
            Exception exception2 = this;
            while (innerException != null)
            {
                exception2 = innerException;
                innerException = innerException.InnerException;
            }
            return exception2;
        }

        [SecuritySafeCritical]
        private string GetClassName()
        {
            if (this._className == null)
            {
                this._className = Type.GetTypeHandle(this).ConstructName(true, false, false);
            }
            return this._className;
        }

        [SecuritySafeCritical]
        private MethodBase GetExceptionMethodFromStackTrace()
        {
            return RuntimeType.GetMethodBase(GetMethodFromStackTrace(this._stackTrace));
        }

        [SecurityCritical]
        private MethodBase GetExceptionMethodFromString()
        {
            char[] separator = new char[2];
            separator[1] = '\n';
            string[] strArray = this._exceptionMethodString.Split(separator);
            if (strArray.Length != 5)
            {
                throw new SerializationException();
            }
            SerializationInfo info = new SerializationInfo(typeof(MemberInfoSerializationHolder), new FormatterConverter());
            info.AddValue("MemberType", int.Parse(strArray[0], CultureInfo.InvariantCulture), typeof(int));
            info.AddValue("Name", strArray[1], typeof(string));
            info.AddValue("AssemblyName", strArray[2], typeof(string));
            info.AddValue("ClassName", strArray[3]);
            info.AddValue("Signature", strArray[4]);
            StreamingContext context = new StreamingContext(StreamingContextStates.All);
            try
            {
                return (MethodBase) new MemberInfoSerializationHolder(info, context).GetRealObject(context);
            }
            catch (SerializationException)
            {
                return null;
            }
        }

        [SecurityCritical]
        private string GetExceptionMethodString()
        {
            MethodBase targetSiteInternal = this.GetTargetSiteInternal();
            if (targetSiteInternal == null)
            {
                return null;
            }
            if (targetSiteInternal is DynamicMethod.RTDynamicMethod)
            {
                return null;
            }
            char ch = '\n';
            StringBuilder builder = new StringBuilder();
            if (targetSiteInternal is ConstructorInfo)
            {
                RuntimeConstructorInfo info = (RuntimeConstructorInfo) targetSiteInternal;
                Type reflectedType = info.ReflectedType;
                builder.Append(1);
                builder.Append(ch);
                builder.Append(info.Name);
                if (reflectedType != null)
                {
                    builder.Append(ch);
                    builder.Append(reflectedType.Assembly.FullName);
                    builder.Append(ch);
                    builder.Append(reflectedType.FullName);
                }
                builder.Append(ch);
                builder.Append(info.ToString());
            }
            else
            {
                RuntimeMethodInfo info2 = (RuntimeMethodInfo) targetSiteInternal;
                Type declaringType = info2.DeclaringType;
                builder.Append(8);
                builder.Append(ch);
                builder.Append(info2.Name);
                builder.Append(ch);
                builder.Append(info2.Module.Assembly.FullName);
                builder.Append(ch);
                if (declaringType != null)
                {
                    builder.Append(declaringType.FullName);
                    builder.Append(ch);
                }
                builder.Append(info2.ToString());
            }
            return builder.ToString();
        }

        [SecuritySafeCritical]
        internal static string GetMessageFromNativeResources(ExceptionMessageKind kind)
        {
            string s = null;
            GetMessageFromNativeResources(kind, JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetMessageFromNativeResources(ExceptionMessageKind kind, StringHandleOnStack retMesg);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern IRuntimeMethodInfo GetMethodFromStackTrace(object stackTrace);
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            string stackTrace = this._stackTraceString;
            if (this._stackTrace != null)
            {
                if (stackTrace == null)
                {
                    stackTrace = Environment.GetStackTrace(this, true);
                }
                if (this._exceptionMethod == null)
                {
                    this._exceptionMethod = this.GetExceptionMethodFromStackTrace();
                }
            }
            if (this._source == null)
            {
                this._source = this.Source;
            }
            info.AddValue("ClassName", this.GetClassName(), typeof(string));
            info.AddValue("Message", this._message, typeof(string));
            info.AddValue("Data", this._data, typeof(IDictionary));
            info.AddValue("InnerException", this._innerException, typeof(Exception));
            info.AddValue("HelpURL", this._helpURL, typeof(string));
            info.AddValue("StackTraceString", stackTrace, typeof(string));
            info.AddValue("RemoteStackTraceString", this._remoteStackTraceString, typeof(string));
            info.AddValue("RemoteStackIndex", this._remoteStackIndex, typeof(int));
            info.AddValue("ExceptionMethod", this.GetExceptionMethodString(), typeof(string));
            info.AddValue("HResult", this.HResult);
            info.AddValue("Source", this._source, typeof(string));
            info.AddValue("WatsonBuckets", this._watsonBuckets, typeof(byte[]));
            if ((this._safeSerializationManager != null) && this._safeSerializationManager.IsActive)
            {
                info.AddValue("SafeSerializationManager", this._safeSerializationManager, typeof(SafeSerializationManager));
                this._safeSerializationManager.CompleteSerialization(this, info, context);
            }
        }

        private string GetStackTrace(bool needFileInfo)
        {
            if (this._stackTraceString != null)
            {
                return (this._remoteStackTraceString + this._stackTraceString);
            }
            if (this._stackTrace == null)
            {
                return this._remoteStackTraceString;
            }
            string stackTrace = Environment.GetStackTrace(this, needFileInfo);
            return (this._remoteStackTraceString + stackTrace);
        }

        [SecurityCritical]
        private MethodBase GetTargetSiteInternal()
        {
            if (this._exceptionMethod == null)
            {
                if (this._stackTrace == null)
                {
                    return null;
                }
                if (this._exceptionMethodString != null)
                {
                    this._exceptionMethod = this.GetExceptionMethodFromString();
                }
                else
                {
                    this._exceptionMethod = this.GetExceptionMethodFromStackTrace();
                }
            }
            return this._exceptionMethod;
        }

        public Type GetType()
        {
            return base.GetType();
        }

        [SecuritySafeCritical]
        private void Init()
        {
            this._message = null;
            this._stackTrace = null;
            this._dynamicMethods = null;
            this.HResult = -2146233088;
            this._xcode = -532462766;
            this._xptrs = IntPtr.Zero;
            this._watsonBuckets = null;
            this._ipForWatsonBuckets = UIntPtr.Zero;
            this._safeSerializationManager = new SafeSerializationManager();
        }

        internal void InternalPreserveStackTrace()
        {
            string stackTrace = this.StackTrace;
            if ((stackTrace != null) && (stackTrace.Length > 0))
            {
                this._remoteStackTraceString = stackTrace + Environment.NewLine;
            }
            this._stackTrace = null;
            this._stackTraceString = null;
        }

        [SecurityCritical]
        internal virtual string InternalToString()
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence).Assert();
            }
            catch
            {
            }
            bool needFileLineInfo = true;
            return this.ToString(needFileLineInfo);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool IsImmutableAgileException(Exception e);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool nIsTransient(int hr);
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this._stackTrace = null;
            this._ipForWatsonBuckets = UIntPtr.Zero;
            if (this._safeSerializationManager == null)
            {
                this._safeSerializationManager = new SafeSerializationManager();
            }
            else
            {
                this._safeSerializationManager.CompleteDeserialization(this);
            }
        }

        internal Exception PrepForRemoting()
        {
            string str = null;
            if (this._remoteStackIndex == 0)
            {
                str = string.Concat(new object[] { Environment.NewLine, "Server stack trace: ", Environment.NewLine, this.StackTrace, Environment.NewLine, Environment.NewLine, "Exception rethrown at [", this._remoteStackIndex, "]: ", Environment.NewLine });
            }
            else
            {
                str = string.Concat(new object[] { this.StackTrace, Environment.NewLine, Environment.NewLine, "Exception rethrown at [", this._remoteStackIndex, "]: ", Environment.NewLine });
            }
            this._remoteStackTraceString = str;
            this._remoteStackIndex++;
            return this;
        }

        internal void SetErrorCode(int hr)
        {
            this.HResult = hr;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return this.ToString(true);
        }

        private string ToString(bool needFileLineInfo)
        {
            string className;
            string message = this.Message;
            if ((message == null) || (message.Length <= 0))
            {
                className = this.GetClassName();
            }
            else
            {
                className = this.GetClassName() + ": " + message;
            }
            if (this._innerException != null)
            {
                className = className + " ---> " + this._innerException.ToString(needFileLineInfo) + Environment.NewLine + "   " + Environment.GetRuntimeResourceString("Exception_EndOfInnerExceptionStack");
            }
            string stackTrace = this.GetStackTrace(needFileLineInfo);
            if (stackTrace != null)
            {
                className = className + Environment.NewLine + stackTrace;
            }
            return className;
        }

        public virtual IDictionary Data
        {
            [SecuritySafeCritical]
            get
            {
                if (this._data == null)
                {
                    if (IsImmutableAgileException(this))
                    {
                        this._data = new EmptyReadOnlyDictionaryInternal();
                    }
                    else
                    {
                        this._data = new ListDictionaryInternal();
                    }
                }
                return this._data;
            }
        }

        public virtual string HelpLink
        {
            get
            {
                return this._helpURL;
            }
            set
            {
                this._helpURL = value;
            }
        }

        protected int HResult
        {
            get
            {
                return this._HResult;
            }
            set
            {
                this._HResult = value;
            }
        }

        public Exception InnerException
        {
            get
            {
                return this._innerException;
            }
        }

        internal bool IsTransient
        {
            [SecuritySafeCritical]
            get
            {
                return nIsTransient(this._HResult);
            }
        }

        public virtual string Message
        {
            [SecuritySafeCritical]
            get
            {
                if (this._message != null)
                {
                    return this._message;
                }
                if (this._className == null)
                {
                    this._className = this.GetClassName();
                }
                return Environment.GetRuntimeResourceString("Exception_WasThrown", new object[] { this._className });
            }
        }

        public virtual string Source
        {
            [SecuritySafeCritical]
            get
            {
                if (this._source == null)
                {
                    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(this, true);
                    if (trace.FrameCount > 0)
                    {
                        Module module = trace.GetFrame(0).GetMethod().Module;
                        RuntimeModule internalModule = module as RuntimeModule;
                        if (internalModule == null)
                        {
                            ModuleBuilder builder = module as ModuleBuilder;
                            if (builder == null)
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));
                            }
                            internalModule = builder.InternalModule;
                        }
                        this._source = internalModule.GetRuntimeAssembly().GetSimpleName();
                    }
                }
                return this._source;
            }
            set
            {
                this._source = value;
            }
        }

        public virtual string StackTrace
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetStackTrace(true);
            }
        }

        public MethodBase TargetSite
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetTargetSiteInternal();
            }
        }

        internal enum ExceptionMessageKind
        {
            OutOfMemory = 3,
            ThreadAbort = 1,
            ThreadInterrupted = 2
        }
    }
}

