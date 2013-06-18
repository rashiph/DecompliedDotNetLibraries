namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class SoapMessage
    {
        private string contentEncoding;
        private string contentType;
        private SoapException exception;
        private SoapExtensionStream extensionStream;
        private SoapHeaderCollection headers = new SoapHeaderCollection();
        private object[] parameterValues;
        private SoapMessageStage stage;
        private System.IO.Stream stream;

        internal SoapMessage()
        {
        }

        protected abstract void EnsureInStage();
        private void EnsureNoException()
        {
            if (this.exception != null)
            {
                throw new InvalidOperationException(Res.GetString("WebCannotAccessValue"), this.exception);
            }
        }

        protected abstract void EnsureOutStage();
        protected void EnsureStage(SoapMessageStage stage)
        {
            if ((this.stage & stage) == ((SoapMessageStage) 0))
            {
                throw new InvalidOperationException(Res.GetString("WebCannotAccessValueStage", new object[] { this.stage.ToString() }));
            }
        }

        public object GetInParameterValue(int index)
        {
            this.EnsureInStage();
            this.EnsureNoException();
            if ((index < 0) || (index >= this.parameterValues.Length))
            {
                throw new IndexOutOfRangeException(Res.GetString("indexMustBeBetweenAnd0Inclusive", new object[] { this.parameterValues.Length }));
            }
            return this.parameterValues[index];
        }

        public object GetOutParameterValue(int index)
        {
            this.EnsureOutStage();
            this.EnsureNoException();
            if (!this.MethodInfo.IsVoid)
            {
                if (index == 0x7fffffff)
                {
                    throw new IndexOutOfRangeException(Res.GetString("indexMustBeBetweenAnd0Inclusive", new object[] { this.parameterValues.Length }));
                }
                index++;
            }
            if ((index < 0) || (index >= this.parameterValues.Length))
            {
                throw new IndexOutOfRangeException(Res.GetString("indexMustBeBetweenAnd0Inclusive", new object[] { this.parameterValues.Length }));
            }
            return this.parameterValues[index];
        }

        internal object[] GetParameterValues()
        {
            return this.parameterValues;
        }

        public object GetReturnValue()
        {
            this.EnsureOutStage();
            this.EnsureNoException();
            if (this.MethodInfo.IsVoid)
            {
                throw new InvalidOperationException(Res.GetString("WebNoReturnValue"));
            }
            return this.parameterValues[0];
        }

        internal void InitExtensionStreamChain(SoapExtension[] extensions)
        {
            if (extensions != null)
            {
                for (int i = 0; i < extensions.Length; i++)
                {
                    this.stream = extensions[i].ChainStream(this.stream);
                }
            }
        }

        internal static SoapExtension[] InitializeExtensions(SoapReflectedExtension[] reflectedExtensions, object[] extensionInitializers)
        {
            if (reflectedExtensions == null)
            {
                return null;
            }
            SoapExtension[] extensionArray = new SoapExtension[reflectedExtensions.Length];
            for (int i = 0; i < extensionArray.Length; i++)
            {
                extensionArray[i] = reflectedExtensions[i].CreateInstance(extensionInitializers[i]);
            }
            return extensionArray;
        }

        internal void RunExtensions(SoapExtension[] extensions, bool throwOnException)
        {
            if (extensions != null)
            {
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "RunExtensions", new object[] { extensions, throwOnException }) : null;
                if ((this.stage & (SoapMessageStage.AfterDeserialize | SoapMessageStage.BeforeDeserialize)) != ((SoapMessageStage) 0))
                {
                    for (int i = 0; i < extensions.Length; i++)
                    {
                        if (Tracing.On)
                        {
                            Tracing.Enter("SoapExtension", caller, new TraceMethod(extensions[i], "ProcessMessage", new object[] { this.stage }));
                        }
                        extensions[i].ProcessMessage(this);
                        if (Tracing.On)
                        {
                            Tracing.Exit("SoapExtension", caller);
                        }
                        if (this.Exception != null)
                        {
                            if (throwOnException)
                            {
                                throw this.Exception;
                            }
                            if (Tracing.On)
                            {
                                Tracing.ExceptionIgnore(TraceEventType.Warning, caller, this.Exception);
                            }
                        }
                    }
                }
                else
                {
                    for (int j = extensions.Length - 1; j >= 0; j--)
                    {
                        if (Tracing.On)
                        {
                            Tracing.Enter("SoapExtension", caller, new TraceMethod(extensions[j], "ProcessMessage", new object[] { this.stage }));
                        }
                        extensions[j].ProcessMessage(this);
                        if (Tracing.On)
                        {
                            Tracing.Exit("SoapExtension", caller);
                        }
                        if (this.Exception != null)
                        {
                            if (throwOnException)
                            {
                                throw this.Exception;
                            }
                            if (Tracing.On)
                            {
                                Tracing.ExceptionIgnore(TraceEventType.Warning, caller, this.Exception);
                            }
                        }
                    }
                }
            }
        }

        internal void SetExtensionStream(SoapExtensionStream extensionStream)
        {
            this.extensionStream = extensionStream;
            this.stream = extensionStream;
        }

        internal void SetParameterValues(object[] parameterValues)
        {
            this.parameterValues = parameterValues;
        }

        internal void SetStage(SoapMessageStage stage)
        {
            this.stage = stage;
        }

        internal void SetStream(System.IO.Stream stream)
        {
            if (this.extensionStream != null)
            {
                this.extensionStream.SetInnerStream(stream);
                this.extensionStream.SetStreamReady();
                this.extensionStream = null;
            }
            else
            {
                this.stream = stream;
            }
        }

        public abstract string Action { get; }

        public string ContentEncoding
        {
            get
            {
                this.EnsureStage(SoapMessageStage.BeforeDeserialize | SoapMessageStage.BeforeSerialize);
                return this.contentEncoding;
            }
            set
            {
                this.EnsureStage(SoapMessageStage.BeforeDeserialize | SoapMessageStage.BeforeSerialize);
                this.contentEncoding = value;
            }
        }

        public string ContentType
        {
            get
            {
                this.EnsureStage(SoapMessageStage.BeforeDeserialize | SoapMessageStage.BeforeSerialize);
                return this.contentType;
            }
            set
            {
                this.EnsureStage(SoapMessageStage.BeforeDeserialize | SoapMessageStage.BeforeSerialize);
                this.contentType = value;
            }
        }

        public SoapException Exception
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.exception;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.exception = value;
            }
        }

        public SoapHeaderCollection Headers
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.headers;
            }
        }

        public abstract LogicalMethodInfo MethodInfo { get; }

        public abstract bool OneWay { get; }

        [ComVisible(false), DefaultValue(0)]
        public virtual SoapProtocolVersion SoapVersion
        {
            get
            {
                return SoapProtocolVersion.Default;
            }
        }

        public SoapMessageStage Stage
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stage;
            }
        }

        public System.IO.Stream Stream
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stream;
            }
        }

        public abstract string Url { get; }
    }
}

