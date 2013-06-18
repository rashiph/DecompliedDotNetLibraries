namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("B62B923C-B500-3158-A543-24F307A8B7E1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedMethod
    {
        uint GetToken();
        uint GetSequencePointCount();
        object GetRootScope();
        object GetScopeFromOffset(uint offset);
        uint GetOffset([In, MarshalAs(UnmanagedType.IUnknown)] ISymUnmanagedDocument document, uint line, uint column);
        void GetRanges([In, MarshalAs(UnmanagedType.IUnknown)] ISymUnmanagedDocument document, uint line, uint column, uint rangeCount, out uint actualRangeCount, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] ranges);
        void GetParameters();
        void GetNamespace();
        void GetSourceStartEnd([In, Out, MarshalAs(UnmanagedType.LPArray)] ISymUnmanagedDocument[] documents, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] lines, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] columns, [MarshalAs(UnmanagedType.Bool)] out bool positionsDefined);
        void GetSequencePoints(uint pointsCount, out uint actualPointsCount, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] offsets, [In, Out, MarshalAs(UnmanagedType.LPArray)] ISymUnmanagedDocument[] documents, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] lines, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] columns, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] endLines, [In, Out, MarshalAs(UnmanagedType.LPArray)] uint[] endColumns);
    }
}

