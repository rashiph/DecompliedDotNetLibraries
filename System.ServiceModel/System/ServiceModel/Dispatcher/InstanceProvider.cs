namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class InstanceProvider : IInstanceProvider
    {
        private CreateInstanceDelegate creator;

        internal InstanceProvider(CreateInstanceDelegate creator)
        {
            if (creator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("creator");
            }
            this.creator = creator;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.creator();
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.creator();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}

