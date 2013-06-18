namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;

    internal class BindCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly System.ActivationContext _actCtx;
        private readonly bool _cached;
        private readonly string _name;

        internal BindCompletedEventArgs(Exception error, bool cancelled, object userState, System.ActivationContext actCtx, string name, bool cached) : base(error, cancelled, userState)
        {
            this._actCtx = actCtx;
            this._name = name;
            this._cached = cached;
        }

        public System.ActivationContext ActivationContext
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._actCtx;
            }
        }

        public string FriendlyName
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._name;
            }
        }

        public bool IsCached
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._cached;
            }
        }
    }
}

