namespace System.Web.Hosting
{
    using System;

    public interface IRegisteredObject
    {
        void Stop(bool immediate);
    }
}

