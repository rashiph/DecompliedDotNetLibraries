namespace System.Configuration.Internal
{
    using System;

    public interface IConfigSystem
    {
        void Init(Type typeConfigHost, params object[] hostInitParams);

        IInternalConfigHost Host { get; }

        IInternalConfigRoot Root { get; }
    }
}

