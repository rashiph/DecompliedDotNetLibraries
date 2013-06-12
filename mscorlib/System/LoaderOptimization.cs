namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum LoaderOptimization
    {
        [Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        DisallowBindings = 4,
        [Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        DomainMask = 3,
        MultiDomain = 2,
        MultiDomainHost = 3,
        NotSpecified = 0,
        SingleDomain = 1
    }
}

