namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Configuration;
    using System.Workflow.Runtime.Hosting;

    public class ExternalDataExchangeService : WorkflowRuntimeService
    {
        private const string configurationSectionAttributeName = "ConfigurationSection";
        private IDeliverMessage enqueueMessageWrapper;
        private Dictionary<int, WorkflowMessageEventHandler> eventHandlers;
        private object handlersLock;
        private List<object> services;
        private object servicesLock;
        private ExternalDataExchangeServiceSection settings;

        public ExternalDataExchangeService()
        {
            this.handlersLock = new object();
            this.servicesLock = new object();
            this.eventHandlers = new Dictionary<int, WorkflowMessageEventHandler>();
            this.services = new List<object>();
            this.enqueueMessageWrapper = new EnqueueMessageWrapper(this);
        }

        public ExternalDataExchangeService(NameValueCollection parameters) : this()
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            string sectionName = null;
            foreach (string str2 in parameters.Keys)
            {
                if (!str2.Equals("ConfigurationSection", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, SR.GetString("Error_UnknownConfigurationParameter"), new object[] { str2 }), "parameters");
                }
                sectionName = parameters[str2];
            }
            if (sectionName != null)
            {
                this.settings = ConfigurationManager.GetSection(sectionName) as ExternalDataExchangeServiceSection;
                if (this.settings == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ConfigurationSectionNotFound"), new object[] { sectionName }));
                }
            }
        }

        public ExternalDataExchangeService(string configSectionName) : this()
        {
            if (configSectionName == null)
            {
                throw new ArgumentNullException("configSectionName");
            }
            this.settings = ConfigurationManager.GetSection(configSectionName) as ExternalDataExchangeServiceSection;
            if (this.settings == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ConfigurationSectionNotFound"), new object[] { configSectionName }));
            }
        }

        public ExternalDataExchangeService(ExternalDataExchangeServiceSection settings) : this()
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            this.settings = settings;
        }

        private void AddRemove(object addedservice, Delegate delg, bool add, string eventName)
        {
            try
            {
                string str;
                if (add)
                {
                    str = "add_" + eventName;
                }
                else
                {
                    str = "remove_" + eventName;
                }
                Type type = addedservice.GetType();
                if (delg != null)
                {
                    object[] args = new object[] { delg };
                    type.InvokeMember(str, BindingFlags.InvokeMethod, null, addedservice, args, null);
                }
            }
            catch (Exception exception)
            {
                if (IsIrrecoverableException(exception))
                {
                    throw;
                }
            }
        }

        public virtual void AddService(object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            this.InterceptService(service, true);
            if (base.Runtime != null)
            {
                base.Runtime.AddService(service);
            }
            else
            {
                lock (this.servicesLock)
                {
                    this.services.Add(service);
                }
            }
        }

        internal ReadOnlyCollection<object> GetAllServices()
        {
            lock (this.servicesLock)
            {
                return this.services.AsReadOnly();
            }
        }

        public virtual object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if (base.Runtime != null)
            {
                return base.Runtime.GetService(serviceType);
            }
            lock (this.servicesLock)
            {
                foreach (object obj2 in this.services)
                {
                    if (serviceType.IsAssignableFrom(obj2.GetType()))
                    {
                        return obj2;
                    }
                }
                return null;
            }
        }

        internal void InterceptService(object service, bool add)
        {
            bool flag = false;
            foreach (Type type in service.GetType().GetInterfaces())
            {
                if (type.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false).Length != 0)
                {
                    if (((base.Runtime != null) && (base.Runtime.GetService(type) != null)) && add)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ExternalDataExchangeServiceExists"), new object[] { type }));
                    }
                    flag = true;
                    EventInfo[] events = type.GetEvents();
                    if (events != null)
                    {
                        foreach (EventInfo info in events)
                        {
                            WorkflowMessageEventHandler handler = null;
                            int key = type.GetHashCode() ^ info.Name.GetHashCode();
                            lock (this.handlersLock)
                            {
                                if (!this.eventHandlers.ContainsKey(key))
                                {
                                    handler = new WorkflowMessageEventHandler(type, info, this.enqueueMessageWrapper);
                                    this.eventHandlers.Add(key, handler);
                                }
                                else
                                {
                                    handler = this.eventHandlers[key];
                                }
                            }
                            this.AddRemove(service, handler.Delegate, add, info.Name);
                        }
                    }
                }
            }
            if (!flag)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ServiceMissingExternalDataExchangeInterface"), new object[0]));
            }
        }

        internal static bool IsIrrecoverableException(Exception e)
        {
            return ((((e is OutOfMemoryException) || (e is StackOverflowException)) || (e is ThreadInterruptedException)) || (e is ThreadAbortException));
        }

        public virtual void RemoveService(object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            this.InterceptService(service, false);
            if (base.Runtime != null)
            {
                base.Runtime.RemoveService(service);
            }
            else
            {
                lock (this.servicesLock)
                {
                    this.services.Remove(service);
                }
            }
        }

        internal object ServiceFromSettings(WorkflowRuntimeServiceElement serviceSettings)
        {
            Type type = Type.GetType(serviceSettings.Type, true);
            ConstructorInfo info = null;
            ConstructorInfo info2 = null;
            ConstructorInfo info3 = null;
            foreach (ConstructorInfo info4 in type.GetConstructors())
            {
                ParameterInfo[] parameters = info4.GetParameters();
                if (parameters.Length == 1)
                {
                    if (typeof(IServiceProvider).IsAssignableFrom(parameters[0].ParameterType))
                    {
                        info2 = info4;
                    }
                    else if (typeof(NameValueCollection).IsAssignableFrom(parameters[0].ParameterType))
                    {
                        info3 = info4;
                    }
                }
                else if (((parameters.Length == 2) && typeof(IServiceProvider).IsAssignableFrom(parameters[0].ParameterType)) && typeof(NameValueCollection).IsAssignableFrom(parameters[1].ParameterType))
                {
                    info = info4;
                    break;
                }
            }
            if (info != null)
            {
                return info.Invoke(new object[] { base.Runtime, serviceSettings.Parameters });
            }
            if (info2 != null)
            {
                return info2.Invoke(new object[] { base.Runtime });
            }
            if (info3 != null)
            {
                return info3.Invoke(new object[] { serviceSettings.Parameters });
            }
            return Activator.CreateInstance(type);
        }

        internal void SetEnqueueMessageWrapper(IDeliverMessage wrapper)
        {
            this.enqueueMessageWrapper = wrapper;
            foreach (WorkflowMessageEventHandler handler in this.eventHandlers.Values)
            {
                handler.EnqueueWrapper = wrapper;
            }
        }

        protected override void Start()
        {
            if (this.settings != null)
            {
                foreach (WorkflowRuntimeServiceElement element in this.settings.Services)
                {
                    this.AddService(this.ServiceFromSettings(element));
                }
            }
            if (base.Runtime != null)
            {
                base.Start();
            }
        }

        private class EnqueueMessageWrapper : IDeliverMessage
        {
            private ExternalDataExchangeService eds;

            public EnqueueMessageWrapper(ExternalDataExchangeService eds)
            {
                this.eds = eds;
            }

            public void DeliverMessage(ExternalDataEventArgs eventArgs, IComparable queueName, object message, object workItem, IPendingWork workHandler)
            {
                WorkflowInstance workflow = this.eds.Runtime.GetWorkflow(eventArgs.InstanceId);
                if (eventArgs.WaitForIdle)
                {
                    workflow.EnqueueItemOnIdle(queueName, message, workHandler, workItem);
                }
                else
                {
                    workflow.EnqueueItem(queueName, message, workHandler, workItem);
                }
            }

            public object[] PrepareEventArgsArray(object sender, ExternalDataEventArgs eventArgs, out object workItem, out IPendingWork workHandler)
            {
                workItem = eventArgs.WorkItem;
                eventArgs.WorkItem = null;
                workHandler = eventArgs.WorkHandler;
                eventArgs.WorkHandler = null;
                return new object[] { sender, eventArgs };
            }
        }
    }
}

