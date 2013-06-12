namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IDynamicProperty
    {
        string Name { [SecurityCritical] get; }
    }
}

