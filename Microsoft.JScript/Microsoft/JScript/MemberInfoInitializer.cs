namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("98A3BF0A-1B56-4f32-ACE0-594FEB27EC48")]
    public interface MemberInfoInitializer
    {
        void Initialize(string name, COMMemberInfo dispatch);
        COMMemberInfo GetCOMMemberInfo();
    }
}

