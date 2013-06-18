namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [Serializable, TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ContextMessageProperty : IMessageProperty
    {
        private IDictionary<string, string> contextStore;
        private static ContextMessageProperty empty;
        internal const string InstanceIdKey = "instanceId";
        private const string PropertyName = "ContextMessageProperty";

        public ContextMessageProperty()
        {
            this.contextStore = new ContextDictionary();
        }

        public ContextMessageProperty(IDictionary<string, string> context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.contextStore = new ContextDictionary(context);
        }

        public void AddOrReplaceInMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.AddOrReplaceInMessageProperties(message.Properties);
        }

        public void AddOrReplaceInMessageProperties(MessageProperties properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            properties["ContextMessageProperty"] = this;
        }

        public IMessageProperty CreateCopy()
        {
            return new ContextMessageProperty(this.Context);
        }

        public static bool TryCreateFromHttpCookieHeader(string httpCookieHeader, out ContextMessageProperty context)
        {
            return ContextProtocol.HttpCookieToolbox.TryCreateFromHttpCookieHeader(httpCookieHeader, out context);
        }

        public static bool TryGet(Message message, out ContextMessageProperty contextMessageProperty)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out contextMessageProperty);
        }

        public static bool TryGet(MessageProperties properties, out ContextMessageProperty contextMessageProperty)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            object obj2 = null;
            if (properties.TryGetValue("ContextMessageProperty", out obj2))
            {
                contextMessageProperty = obj2 as ContextMessageProperty;
            }
            else
            {
                contextMessageProperty = null;
            }
            return (contextMessageProperty != null);
        }

        public IDictionary<string, string> Context
        {
            get
            {
                return this.contextStore;
            }
        }

        internal static ContextMessageProperty Empty
        {
            get
            {
                if (empty == null)
                {
                    ContextMessageProperty property = new ContextMessageProperty {
                        contextStore = ContextDictionary.Empty
                    };
                    empty = property;
                }
                return empty;
            }
        }

        public static string Name
        {
            get
            {
                return "ContextMessageProperty";
            }
        }
    }
}

