namespace System.Web.Util
{
    using System;

    internal interface ITypedWebObjectFactory : IWebObjectFactory
    {
        Type InstantiatedType { get; }
    }
}

