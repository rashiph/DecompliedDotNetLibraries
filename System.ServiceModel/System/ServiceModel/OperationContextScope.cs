namespace System.ServiceModel
{
    using System;
    using System.Threading;

    public sealed class OperationContextScope : IDisposable
    {
        private OperationContext currentContext;
        [ThreadStatic]
        private static OperationContextScope currentScope;
        private bool disposed;
        private readonly OperationContext originalContext;
        private readonly OperationContextScope originalScope;
        private readonly Thread thread;

        public OperationContextScope(IContextChannel channel)
        {
            this.originalContext = OperationContext.Current;
            this.originalScope = currentScope;
            this.thread = Thread.CurrentThread;
            this.PushContext(new OperationContext(channel));
        }

        public OperationContextScope(OperationContext context)
        {
            this.originalContext = OperationContext.Current;
            this.originalScope = currentScope;
            this.thread = Thread.CurrentThread;
            this.PushContext(context);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.PopContext();
            }
        }

        private void PopContext()
        {
            if (this.thread != Thread.CurrentThread)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidContextScopeThread0")));
            }
            if (currentScope != this)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInterleavedContextScopes0")));
            }
            if (OperationContext.Current != this.currentContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxContextModifiedInsideScope0")));
            }
            currentScope = this.originalScope;
            OperationContext.Current = this.originalContext;
            if (this.currentContext != null)
            {
                this.currentContext.SetClientReply(null, false);
            }
        }

        private void PushContext(OperationContext context)
        {
            this.currentContext = context;
            currentScope = this;
            OperationContext.Current = this.currentContext;
        }
    }
}

