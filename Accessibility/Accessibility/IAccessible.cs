namespace Accessibility
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("618736E0-3C3D-11CF-810C-00AA00389B71"), TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FHidden)]
    public interface IAccessible
    {
        [DispId(-5000)]
        object accParent { [return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5000)] get; }
        [DispId(-5001)]
        int accChildCount { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5001), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5002)]
        object this[object varChild] { [return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5002), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5003)]
        string this[object varChild] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5003)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5003), TypeLibFunc(TypeLibFuncFlags.FHidden)] set; }
        [DispId(-5004)]
        string this[object varChild] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5004), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5004)] set; }
        [DispId(-5005)]
        string this[object varChild] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5005), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5006)]
        object this[object varChild] { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5006), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5007)]
        object this[object varChild] { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5007)] get; }
        [DispId(-5008)]
        string this[object varChild] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5008), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5009)]
        int this[ref string pszHelpFile, object varChild] { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5009)] get; }
        [DispId(-5010)]
        string this[object varChild] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5010), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5011)]
        object accFocus { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5011), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5012)]
        object accSelection { [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5012), TypeLibFunc(TypeLibFuncFlags.FHidden)] get; }
        [DispId(-5013)]
        string this[object varChild] { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5013)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5014)]
        void accSelect([In] int flagsSelect, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5015), TypeLibFunc(TypeLibFuncFlags.FHidden)]
        void accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5016)]
        object accNavigate([In] int navDir, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varStart);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc(TypeLibFuncFlags.FHidden), DispId(-5017)]
        object accHitTest([In] int xLeft, [In] int yTop);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-5018), TypeLibFunc(TypeLibFuncFlags.FHidden)]
        void accDoDefaultAction([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
    }
}

