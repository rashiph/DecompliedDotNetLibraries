namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Web.Util;

    internal class WebServiceHandler
    {
        private ManualResetEvent asyncBeginComplete;
        private AsyncCallback asyncCallback;
        private int asyncCallbackCalls;
        private Exception exception;
        private object[] parameters;
        private ServerProtocol protocol;
        private bool wroteException;

        internal WebServiceHandler(ServerProtocol protocol)
        {
            this.protocol = protocol;
        }

        protected IAsyncResult BeginCoreProcessRequest(AsyncCallback callback, object asyncState)
        {
            if (this.protocol.MethodAttribute.TransactionEnabled)
            {
                throw new InvalidOperationException(Res.GetString("WebAsyncTransaction"));
            }
            this.parameters = this.protocol.ReadParameters();
            if (this.protocol.IsOneWay)
            {
                TraceMethod method = Tracing.On ? new TraceMethod(this, "OneWayAsyncInvoke", new object[0]) : null;
                if (Tracing.On)
                {
                    Tracing.Information("TracePostWorkItemIn", new object[] { method });
                }
                WorkItem.Post(new WorkItemCallback(this.OneWayAsyncInvoke));
                if (Tracing.On)
                {
                    Tracing.Information("TracePostWorkItemOut", new object[] { method });
                }
                IAsyncResult ar = new CompletedAsyncResult(asyncState, true);
                if (callback != null)
                {
                    callback(ar);
                }
                return ar;
            }
            return this.BeginInvoke(callback, asyncState);
        }

        private IAsyncResult BeginInvoke(AsyncCallback callback, object asyncState)
        {
            IAsyncResult result;
            try
            {
                this.PrepareContext();
                this.protocol.CreateServerInstance();
                this.asyncCallback = callback;
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "BeginInvoke", new object[0]) : null;
                TraceMethod callDetails = Tracing.On ? new TraceMethod(this.protocol.Target, this.protocol.MethodInfo.Name, this.parameters) : null;
                if (Tracing.On)
                {
                    Tracing.Enter(this.protocol.MethodInfo.ToString(), caller, callDetails);
                }
                result = this.protocol.MethodInfo.BeginInvoke(this.protocol.Target, this.parameters, new AsyncCallback(this.Callback), asyncState);
                if (Tracing.On)
                {
                    Tracing.Enter(this.protocol.MethodInfo.ToString(), caller);
                }
                if (result == null)
                {
                    throw new InvalidOperationException(Res.GetString("WebNullAsyncResultInBegin"));
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "BeginInvoke", exception);
                }
                this.exception = exception;
                result = new CompletedAsyncResult(asyncState, true);
                this.asyncCallback = callback;
                this.DoCallback(result);
            }
            this.asyncBeginComplete.Set();
            TraceFlush();
            return result;
        }

        private void Callback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                this.asyncBeginComplete.WaitOne();
            }
            this.DoCallback(result);
        }

        protected void CoreProcessRequest()
        {
            try
            {
                bool transactionEnabled = this.protocol.MethodAttribute.TransactionEnabled;
                if (this.protocol.IsOneWay)
                {
                    WorkItemCallback callback = null;
                    TraceMethod method = null;
                    if (this.protocol.OnewayInitException != null)
                    {
                        callback = new WorkItemCallback(this.ThrowInitException);
                        method = Tracing.On ? new TraceMethod(this, "ThrowInitException", new object[0]) : null;
                    }
                    else
                    {
                        this.parameters = this.protocol.ReadParameters();
                        callback = transactionEnabled ? new WorkItemCallback(this.OneWayInvokeTransacted) : new WorkItemCallback(this.OneWayInvoke);
                        method = Tracing.On ? (transactionEnabled ? new TraceMethod(this, "OneWayInvokeTransacted", new object[0]) : new TraceMethod(this, "OneWayInvoke", new object[0])) : null;
                    }
                    if (Tracing.On)
                    {
                        Tracing.Information("TracePostWorkItemIn", new object[] { method });
                    }
                    WorkItem.Post(callback);
                    if (Tracing.On)
                    {
                        Tracing.Information("TracePostWorkItemOut", new object[] { method });
                    }
                    this.protocol.WriteOneWayResponse();
                }
                else if (transactionEnabled)
                {
                    this.parameters = this.protocol.ReadParameters();
                    this.InvokeTransacted();
                }
                else
                {
                    this.parameters = this.protocol.ReadParameters();
                    this.Invoke();
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "CoreProcessRequest", exception);
                }
                if (!this.protocol.IsOneWay)
                {
                    this.WriteException(exception);
                }
            }
            TraceFlush();
        }

        private void DoCallback(IAsyncResult result)
        {
            if ((this.asyncCallback != null) && (Interlocked.Increment(ref this.asyncCallbackCalls) == 1))
            {
                this.asyncCallback(result);
            }
        }

        protected void EndCoreProcessRequest(IAsyncResult asyncResult)
        {
            if (asyncResult != null)
            {
                if (this.protocol.IsOneWay)
                {
                    this.protocol.WriteOneWayResponse();
                }
                else
                {
                    this.EndInvoke(asyncResult);
                }
            }
        }

        private void EndInvoke(IAsyncResult asyncResult)
        {
            try
            {
                if (this.exception != null)
                {
                    throw this.exception;
                }
                object[] returnValues = this.protocol.MethodInfo.EndInvoke(this.protocol.Target, asyncResult);
                this.WriteReturns(returnValues);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "EndInvoke", exception);
                }
                if (!this.protocol.IsOneWay)
                {
                    this.WriteException(exception);
                }
            }
            finally
            {
                this.protocol.DisposeServerInstance();
            }
            TraceFlush();
        }

        private void HandleOneWayException(Exception e, string method)
        {
            if (Tracing.On)
            {
                Tracing.ExceptionCatch(TraceEventType.Error, this, string.IsNullOrEmpty(method) ? "HandleOneWayException" : method, e);
            }
        }

        private void Invoke()
        {
            string str;
            this.PrepareContext();
            this.protocol.CreateServerInstance();
            RemoteDebugger debugger = null;
            if (!this.protocol.IsOneWay && RemoteDebugger.IsServerCallInEnabled(this.protocol, out str))
            {
                debugger = new RemoteDebugger();
                debugger.NotifyServerCallEnter(this.protocol, str);
            }
            try
            {
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "Invoke", new object[0]) : null;
                TraceMethod callDetails = Tracing.On ? new TraceMethod(this.protocol.Target, this.protocol.MethodInfo.Name, this.parameters) : null;
                if (Tracing.On)
                {
                    Tracing.Enter(this.protocol.MethodInfo.ToString(), caller, callDetails);
                }
                object[] returnValues = this.protocol.MethodInfo.Invoke(this.protocol.Target, this.parameters);
                if (Tracing.On)
                {
                    Tracing.Exit(this.protocol.MethodInfo.ToString(), caller);
                }
                this.WriteReturns(returnValues);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "Invoke", exception);
                }
                if (!this.protocol.IsOneWay)
                {
                    this.WriteException(exception);
                    throw;
                }
            }
            finally
            {
                this.protocol.DisposeServerInstance();
                if (debugger != null)
                {
                    debugger.NotifyServerCallExit(this.protocol.Response);
                }
            }
        }

        private void InvokeTransacted()
        {
            Transactions.InvokeTransacted(new TransactedCallback(this.Invoke), this.protocol.MethodAttribute.TransactionOption);
        }

        private void OneWayAsyncInvoke()
        {
            if (this.protocol.OnewayInitException != null)
            {
                this.HandleOneWayException(new Exception(Res.GetString("WebConfigExtensionError"), this.protocol.OnewayInitException), "OneWayAsyncInvoke");
            }
            else
            {
                HttpContext context = null;
                if (this.protocol.Context != null)
                {
                    context = this.SwitchContext(this.protocol.Context);
                }
                try
                {
                    this.BeginInvoke(new AsyncCallback(this.OneWayCallback), null);
                }
                catch (Exception exception)
                {
                    this.HandleOneWayException(exception, "OneWayAsyncInvoke");
                }
                finally
                {
                    if (context != null)
                    {
                        this.SwitchContext(context);
                    }
                }
            }
        }

        private void OneWayCallback(IAsyncResult asyncResult)
        {
            this.EndInvoke(asyncResult);
        }

        private void OneWayInvoke()
        {
            HttpContext context = null;
            if (this.protocol.Context != null)
            {
                context = this.SwitchContext(this.protocol.Context);
            }
            try
            {
                this.Invoke();
            }
            catch (Exception exception)
            {
                this.HandleOneWayException(exception, "OneWayInvoke");
            }
            finally
            {
                if (context != null)
                {
                    this.SwitchContext(context);
                }
            }
        }

        private void OneWayInvokeTransacted()
        {
            HttpContext context = null;
            if (this.protocol.Context != null)
            {
                context = this.SwitchContext(this.protocol.Context);
            }
            try
            {
                this.InvokeTransacted();
            }
            catch (Exception exception)
            {
                this.HandleOneWayException(exception, "OneWayInvokeTransacted");
            }
            finally
            {
                if (context != null)
                {
                    this.SwitchContext(context);
                }
            }
        }

        private void PrepareContext()
        {
            this.exception = null;
            this.wroteException = false;
            this.asyncCallback = null;
            this.asyncBeginComplete = new ManualResetEvent(false);
            this.asyncCallbackCalls = 0;
            if (!this.protocol.IsOneWay)
            {
                HttpContext context = this.protocol.Context;
                if (context != null)
                {
                    int cacheDuration = this.protocol.MethodAttribute.CacheDuration;
                    if (cacheDuration > 0)
                    {
                        context.Response.Cache.SetCacheability(HttpCacheability.Server);
                        context.Response.Cache.SetExpires(DateTime.Now.AddSeconds((double) cacheDuration));
                        context.Response.Cache.SetSlidingExpiration(false);
                        context.Response.Cache.VaryByHeaders["Content-type"] = true;
                        context.Response.Cache.VaryByHeaders["SOAPAction"] = true;
                        context.Response.Cache.VaryByParams["*"] = true;
                    }
                    else
                    {
                        context.Response.Cache.SetNoServerCaching();
                        context.Response.Cache.SetMaxAge(TimeSpan.Zero);
                    }
                    context.Response.BufferOutput = this.protocol.MethodAttribute.BufferResponse;
                    context.Response.ContentType = null;
                }
            }
        }

        private HttpContext SwitchContext(HttpContext context)
        {
            HttpContext current = HttpContext.Current;
            HttpContext.Current = context;
            return current;
        }

        private void ThrowInitException()
        {
            this.HandleOneWayException(new Exception(Res.GetString("WebConfigExtensionError"), this.protocol.OnewayInitException), "ThrowInitException");
        }

        private static void TraceFlush()
        {
        }

        private void WriteException(Exception e)
        {
            if (!this.wroteException)
            {
                bool traceVerbose = System.ComponentModel.CompModSwitches.Remote.TraceVerbose;
                if (e is TargetInvocationException)
                {
                    bool flag2 = System.ComponentModel.CompModSwitches.Remote.TraceVerbose;
                    e = e.InnerException;
                }
                this.wroteException = this.protocol.WriteException(e, this.protocol.Response.OutputStream);
                if (!this.wroteException)
                {
                    throw e;
                }
            }
        }

        private void WriteReturns(object[] returnValues)
        {
            if (!this.protocol.IsOneWay)
            {
                bool bufferResponse = this.protocol.MethodAttribute.BufferResponse;
                Stream outputStream = this.protocol.Response.OutputStream;
                if (!bufferResponse)
                {
                    outputStream = new BufferedResponseStream(outputStream, 0x4000);
                    ((BufferedResponseStream) outputStream).FlushEnabled = false;
                }
                this.protocol.WriteReturns(returnValues, outputStream);
                if (!bufferResponse)
                {
                    ((BufferedResponseStream) outputStream).FlushEnabled = true;
                    outputStream.Flush();
                }
            }
        }
    }
}

