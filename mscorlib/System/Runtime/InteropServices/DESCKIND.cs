namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Obsolete("Use System.Runtime.InteropServices.ComTypes.DESCKIND instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public enum DESCKIND
    {
        DESCKIND_NONE,
        DESCKIND_FUNCDESC,
        DESCKIND_VARDESC,
        DESCKIND_TYPECOMP,
        DESCKIND_IMPLICITAPPOBJ,
        DESCKIND_MAX
    }
}

