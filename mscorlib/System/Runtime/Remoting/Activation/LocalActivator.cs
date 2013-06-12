namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Security;

    [SecurityCritical]
    internal class LocalActivator : ContextAttribute, IActivator
    {
        internal LocalActivator() : base("RemoteActivationService.rem")
        {
        }

        [SecurityCritical, ComVisible(true)]
        public virtual IConstructionReturnMessage Activate(IConstructionCallMessage ctorMsg)
        {
            if (ctorMsg == null)
            {
                throw new ArgumentNullException("ctorMsg");
            }
            if (ctorMsg.Properties.Contains("Remote"))
            {
                return DoRemoteActivation(ctorMsg);
            }
            if (!ctorMsg.Properties.Contains("Permission"))
            {
                return ctorMsg.Activator.Activate(ctorMsg);
            }
            Type activationType = ctorMsg.ActivationType;
            object[] activationAttributes = null;
            if (activationType.IsContextful)
            {
                IList contextProperties = ctorMsg.ContextProperties;
                if ((contextProperties != null) && (contextProperties.Count > 0))
                {
                    RemotePropertyHolderAttribute attribute = new RemotePropertyHolderAttribute(contextProperties);
                    activationAttributes = new object[] { attribute };
                }
            }
            RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(GetMethodBase(ctorMsg));
            object[] args = Message.CoerceArgs(ctorMsg, reflectionCachedData.Parameters);
            object obj2 = Activator.CreateInstance(activationType, args, activationAttributes);
            if (RemotingServices.IsClientProxy(obj2))
            {
                RedirectionProxy proxy = new RedirectionProxy((MarshalByRefObject) obj2, activationType);
                RemotingServices.MarshalInternal(proxy, null, activationType);
                obj2 = proxy;
            }
            return ActivationServices.SetupConstructionReply(obj2, ctorMsg, null);
        }

        internal static IConstructionReturnMessage DoRemoteActivation(IConstructionCallMessage ctorMsg)
        {
            IActivator activator = null;
            string url = (string) ctorMsg.Properties["Remote"];
            try
            {
                activator = (IActivator) RemotingServices.Connect(typeof(IActivator), url);
            }
            catch (Exception exception)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Activation_ConnectFailed"), new object[] { exception }));
            }
            ctorMsg.Properties.Remove("Remote");
            return activator.Activate(ctorMsg);
        }

        private static MethodBase GetMethodBase(IConstructionCallMessage msg)
        {
            MethodBase methodBase = msg.MethodBase;
            if (null == methodBase)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MethodMissing"), new object[] { msg.MethodName, msg.TypeName }));
            }
            return methodBase;
        }

        [SecurityCritical]
        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            if (ctorMsg.Properties.Contains("Remote"))
            {
                string remActivatorURL = (string) ctorMsg.Properties["Remote"];
                AppDomainLevelActivator activator = new AppDomainLevelActivator(remActivatorURL);
                IActivator nextActivator = ctorMsg.Activator;
                if (nextActivator.Level < ActivatorLevel.AppDomain)
                {
                    activator.NextActivator = nextActivator;
                    ctorMsg.Activator = activator;
                }
                else if (nextActivator.NextActivator != null)
                {
                    while (nextActivator.NextActivator.Level >= ActivatorLevel.AppDomain)
                    {
                        nextActivator = nextActivator.NextActivator;
                    }
                    activator.NextActivator = nextActivator.NextActivator;
                    nextActivator.NextActivator = activator;
                }
                else
                {
                    nextActivator.NextActivator = activator;
                }
            }
        }

        [SecurityCritical]
        public override bool IsContextOK(Context ctx, IConstructionCallMessage ctorMsg)
        {
            if (RemotingConfigHandler.Info == null)
            {
                return true;
            }
            RuntimeType activationType = ctorMsg.ActivationType as RuntimeType;
            if (activationType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            WellKnownClientTypeEntry entry = RemotingConfigHandler.IsWellKnownClientType(activationType);
            string str = (entry == null) ? null : entry.ObjectUrl;
            if (str != null)
            {
                ctorMsg.Properties["Connect"] = str;
                return false;
            }
            ActivatedClientTypeEntry entry2 = RemotingConfigHandler.IsRemotelyActivatedClientType(activationType);
            string urlValue = null;
            if (entry2 == null)
            {
                object[] callSiteActivationAttributes = ctorMsg.CallSiteActivationAttributes;
                if (callSiteActivationAttributes != null)
                {
                    for (int i = 0; i < callSiteActivationAttributes.Length; i++)
                    {
                        UrlAttribute attribute = callSiteActivationAttributes[i] as UrlAttribute;
                        if (attribute != null)
                        {
                            urlValue = attribute.UrlValue;
                        }
                    }
                }
                if (urlValue == null)
                {
                    return true;
                }
            }
            else
            {
                urlValue = entry2.ApplicationUrl;
            }
            string str3 = null;
            if (!urlValue.EndsWith("/", StringComparison.Ordinal))
            {
                str3 = urlValue + "/RemoteActivationService.rem";
            }
            else
            {
                str3 = urlValue + "RemoteActivationService.rem";
            }
            ctorMsg.Properties["Remote"] = str3;
            return false;
        }

        public virtual ActivatorLevel Level
        {
            [SecurityCritical]
            get
            {
                return ActivatorLevel.AppDomain;
            }
        }

        public virtual IActivator NextActivator
        {
            [SecurityCritical]
            get
            {
                return null;
            }
            [SecurityCritical]
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}

