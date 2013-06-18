namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;

    internal class AppDomainInstanceProvider : ProviderBase, IWmiProvider
    {
        private static IWmiInstance[] CreateListenersInfo(TraceSource traceSource, IWmiInstance instance)
        {
            IWmiInstance[] instanceArray = new IWmiInstance[traceSource.Listeners.Count];
            for (int i = 0; i < traceSource.Listeners.Count; i++)
            {
                TraceListener target = traceSource.Listeners[i];
                IWmiInstance instance2 = instance.NewInstance("TraceListener");
                instance2.SetProperty("Name", target.Name);
                List<IWmiInstance> list = new List<IWmiInstance>(1);
                Type type = target.GetType();
                string str = (string) type.InvokeMember("initializeData", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, target, null, CultureInfo.InvariantCulture);
                string[] strArray = (string[]) type.InvokeMember("GetSupportedAttributes", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, target, null, CultureInfo.InvariantCulture);
                IWmiInstance item = instance.NewInstance("TraceListenerArgument");
                item.SetProperty("Name", "initializeData");
                item.SetProperty("Value", str);
                list.Add(item);
                if (strArray != null)
                {
                    foreach (string str2 in strArray)
                    {
                        item = instance.NewInstance("TraceListenerArgument");
                        item.SetProperty("Name", str2);
                        item.SetProperty("Value", target.Attributes[str2]);
                        list.Add(item);
                    }
                }
                instance2.SetProperty("TraceListenerArguments", list.ToArray());
                instanceArray[i] = instance2;
            }
            return instanceArray;
        }

        internal static void FillAppDomainInfo(IWmiInstance instance)
        {
            AppDomainInfo current = AppDomainInfo.Current;
            instance.SetProperty("Name", current.Name);
            instance.SetProperty("AppDomainId", current.Id);
            instance.SetProperty("PerformanceCounters", PerformanceCounters.Scope.ToString());
            instance.SetProperty("IsDefault", current.IsDefaultAppDomain);
            instance.SetProperty("ProcessId", current.ProcessId);
            instance.SetProperty("TraceLevel", DiagnosticUtility.Level.ToString());
            instance.SetProperty("LogMalformedMessages", MessageLogger.LogMalformedMessages);
            instance.SetProperty("LogMessagesAtServiceLevel", MessageLogger.LogMessagesAtServiceLevel);
            instance.SetProperty("LogMessagesAtTransportLevel", MessageLogger.LogMessagesAtTransportLevel);
            instance.SetProperty("ServiceConfigPath", AspNetEnvironment.Current.ConfigurationPath);
            FillListenersInfo(instance);
        }

        private static void FillListenersInfo(IWmiInstance instance)
        {
            TraceSource traceSource = (DiagnosticUtility.DiagnosticTrace == null) ? null : DiagnosticUtility.DiagnosticTrace.TraceSource;
            if (traceSource != null)
            {
                instance.SetProperty("ServiceModelTraceListeners", CreateListenersInfo(traceSource, instance));
            }
            traceSource = MessageLogger.MessageTraceSource;
            if (traceSource != null)
            {
                instance.SetProperty("MessageLoggingTraceListeners", CreateListenersInfo(traceSource, instance));
            }
        }

        internal static string GetReference()
        {
            return string.Format(CultureInfo.InvariantCulture, "AppDomainInfo.AppDomainId={0},Name='{1}',ProcessId={2}", new object[] { AppDomainInfo.Current.Id, AppDomainInfo.Current.Name, AppDomainInfo.Current.ProcessId });
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            IWmiInstance instance = instances.NewInstance(null);
            FillAppDomainInfo(instance);
            instances.AddInstance(instance);
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            bool flag = false;
            if ((((int) instance.GetProperty("ProcessId")) == AppDomainInfo.Current.ProcessId) && string.Equals((string) instance.GetProperty("Name"), AppDomainInfo.Current.Name, StringComparison.Ordinal))
            {
                FillAppDomainInfo(instance);
                flag = true;
            }
            return flag;
        }

        bool IWmiProvider.PutInstance(IWmiInstance instance)
        {
            bool flag = false;
            if ((((int) instance.GetProperty("ProcessId")) != AppDomainInfo.Current.ProcessId) || !string.Equals((string) instance.GetProperty("Name"), AppDomainInfo.Current.Name, StringComparison.Ordinal))
            {
                return flag;
            }
            try
            {
                SourceLevels newValue = (SourceLevels) Enum.Parse(typeof(SourceLevels), (string) instance.GetProperty("TraceLevel"));
                if (DiagnosticUtility.Level != newValue)
                {
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x10001, System.ServiceModel.SR.GetString("TraceCodeWmiPut"), new WmiPutTraceRecord("DiagnosticTrace.Level", DiagnosticUtility.Level, newValue), instance, null);
                    }
                    DiagnosticUtility.Level = newValue;
                }
                bool property = (bool) instance.GetProperty("LogMalformedMessages");
                if (MessageLogger.LogMalformedMessages != property)
                {
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x10001, System.ServiceModel.SR.GetString("TraceCodeWmiPut"), new WmiPutTraceRecord("MessageLogger.LogMalformedMessages", MessageLogger.LogMalformedMessages, property), instance, null);
                    }
                    MessageLogger.LogMalformedMessages = property;
                }
                bool flag3 = (bool) instance.GetProperty("LogMessagesAtServiceLevel");
                if (MessageLogger.LogMessagesAtServiceLevel != flag3)
                {
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x10001, System.ServiceModel.SR.GetString("TraceCodeWmiPut"), new WmiPutTraceRecord("MessageLogger.LogMessagesAtServiceLevel", MessageLogger.LogMessagesAtServiceLevel, flag3), instance, null);
                    }
                    MessageLogger.LogMessagesAtServiceLevel = flag3;
                }
                bool flag4 = (bool) instance.GetProperty("LogMessagesAtTransportLevel");
                if (MessageLogger.LogMessagesAtTransportLevel != flag4)
                {
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x10001, System.ServiceModel.SR.GetString("TraceCodeWmiPut"), new WmiPutTraceRecord("MessageLogger.LogMessagesAtTransportLevel", MessageLogger.LogMessagesAtTransportLevel, flag4), instance, null);
                    }
                    MessageLogger.LogMessagesAtTransportLevel = flag4;
                }
            }
            catch (ArgumentException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidParameterException());
            }
            return true;
        }
    }
}

