namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class TaskExceptionHolder
    {
        private List<Exception> m_exceptions;
        private bool m_isHandled;
        private Task m_task;
        private static volatile EventHandler s_adUnloadEventHandler;
        private static volatile bool s_domainUnloadStarted;

        internal TaskExceptionHolder(Task task)
        {
            EnsureADUnloadCallbackRegistered();
            this.m_exceptions = new List<Exception>(1);
            this.m_task = task;
        }

        internal void Add(object exceptionObject)
        {
            Exception item = exceptionObject as Exception;
            if (item != null)
            {
                this.m_exceptions.Add(item);
            }
            else
            {
                IEnumerable<Exception> collection = exceptionObject as IEnumerable<Exception>;
                if (collection == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("TaskExceptionHolder_UnknownExceptionType"), "exceptionObject");
                }
                this.m_exceptions.AddRange(collection);
            }
            for (int i = 0; i < this.m_exceptions.Count; i++)
            {
                if ((this.m_exceptions[i].GetType() != typeof(ThreadAbortException)) && (this.m_exceptions[i].GetType() != typeof(AppDomainUnloadedException)))
                {
                    this.MarkAsUnhandled();
                    return;
                }
                if (i == (this.m_exceptions.Count - 1))
                {
                    this.MarkAsHandled(false);
                }
            }
        }

        private static void AppDomainUnloadCallback(object sender, EventArgs e)
        {
            s_domainUnloadStarted = true;
        }

        internal AggregateException CreateExceptionObject(bool calledFromFinalizer, Exception includeThisException)
        {
            this.MarkAsHandled(calledFromFinalizer);
            List<Exception> exceptions = this.m_exceptions;
            if (includeThisException != null)
            {
                exceptions = new List<Exception>(exceptions) {
                    includeThisException
                };
            }
            return new AggregateException(exceptions);
        }

        private static void EnsureADUnloadCallbackRegistered()
        {
            if ((s_adUnloadEventHandler == null) && (Interlocked.CompareExchange<EventHandler>(ref s_adUnloadEventHandler, new EventHandler(TaskExceptionHolder.AppDomainUnloadCallback), null) == null))
            {
                AppDomain.CurrentDomain.DomainUnload += s_adUnloadEventHandler;
            }
        }

        ~TaskExceptionHolder()
        {
            if ((!this.m_isHandled && !Environment.HasShutdownStarted) && (!AppDomain.CurrentDomain.IsFinalizingForUnload() && !s_domainUnloadStarted))
            {
                foreach (Exception exception in this.m_exceptions)
                {
                    AggregateException exception2 = exception as AggregateException;
                    if (exception2 != null)
                    {
                        foreach (Exception exception4 in exception2.Flatten().InnerExceptions)
                        {
                            if (exception4 is ThreadAbortException)
                            {
                                return;
                            }
                        }
                    }
                    else if (exception is ThreadAbortException)
                    {
                        return;
                    }
                }
                AggregateException exception5 = new AggregateException(Environment.GetResourceString("TaskExceptionHolder_UnhandledException"), this.m_exceptions);
                UnobservedTaskExceptionEventArgs ueea = new UnobservedTaskExceptionEventArgs(exception5);
                TaskScheduler.PublishUnobservedTaskException(this.m_task, ueea);
                if (!ueea.m_observed)
                {
                    throw exception5;
                }
            }
        }

        internal void MarkAsHandled(bool calledFromFinalizer)
        {
            if (!this.m_isHandled)
            {
                if (!calledFromFinalizer)
                {
                    GC.SuppressFinalize(this);
                }
                this.m_isHandled = true;
            }
        }

        private void MarkAsUnhandled()
        {
            if (this.m_isHandled)
            {
                GC.ReRegisterForFinalize(this);
                this.m_isHandled = false;
            }
        }
    }
}

