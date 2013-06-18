namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    public abstract class CorrelationCallbackMessageProperty : IMessageProperty
    {
        private CorrelationDataMessageProperty data;
        private static ReadOnlyCollection<string> emptyNeededData = new ReadOnlyCollection<string>(new List<string>(0));
        private List<string> neededData;
        private const string PropertyName = "CorrelationCallbackMessageProperty";

        protected CorrelationCallbackMessageProperty(ICollection<string> neededData)
        {
            if (neededData == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("neededData");
            }
            if (neededData.Count > 0)
            {
                this.neededData = new List<string>(neededData);
            }
        }

        protected CorrelationCallbackMessageProperty(CorrelationCallbackMessageProperty callback)
        {
            if (callback == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callback");
            }
            if (callback.data != null)
            {
                this.data = (CorrelationDataMessageProperty) callback.data.CreateCopy();
            }
            if ((callback.neededData != null) && (callback.neededData.Count > 0))
            {
                this.neededData = new List<string>(callback.neededData);
            }
        }

        public void AddData(string name, Func<string> value)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (this.data == null)
            {
                this.data = new CorrelationDataMessageProperty();
            }
            this.data.Add(name, value);
            if (this.neededData != null)
            {
                this.neededData.Remove(name);
            }
        }

        public IAsyncResult BeginFinalizeCorrelation(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            if ((this.data != null) && !message.Properties.ContainsKey(CorrelationDataMessageProperty.Name))
            {
                message.Properties[CorrelationDataMessageProperty.Name] = this.data;
            }
            return this.OnBeginFinalizeCorrelation(message, timeout, callback, state);
        }

        public abstract IMessageProperty CreateCopy();
        public Message EndFinalizeCorrelation(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            return this.OnEndFinalizeCorrelation(result);
        }

        public Message FinalizeCorrelation(Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            if ((this.data != null) && !message.Properties.ContainsKey(CorrelationDataMessageProperty.Name))
            {
                message.Properties[CorrelationDataMessageProperty.Name] = this.data;
            }
            return this.OnFinalizeCorrelation(message, timeout);
        }

        protected abstract IAsyncResult OnBeginFinalizeCorrelation(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract Message OnEndFinalizeCorrelation(IAsyncResult result);
        protected abstract Message OnFinalizeCorrelation(Message message, TimeSpan timeout);
        public static bool TryGet(Message message, out CorrelationCallbackMessageProperty property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out CorrelationCallbackMessageProperty property)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            object obj2 = null;
            if (properties.TryGetValue("CorrelationCallbackMessageProperty", out obj2))
            {
                property = obj2 as CorrelationCallbackMessageProperty;
            }
            else
            {
                property = null;
            }
            return (property != null);
        }

        public bool IsFullyDefined
        {
            get
            {
                if (this.neededData != null)
                {
                    return (this.neededData.Count == 0);
                }
                return true;
            }
        }

        public static string Name
        {
            get
            {
                return "CorrelationCallbackMessageProperty";
            }
        }

        public IEnumerable<string> NeededData
        {
            get
            {
                if (this.neededData == null)
                {
                    return emptyNeededData;
                }
                return this.neededData;
            }
        }
    }
}

