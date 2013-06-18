using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=12), MiscellaneousBits(0x40), NativeCppClass, DebugInfoInPDB]
internal static class IIterator<KeyValuePair<ILTree::ILNode *,ILTree::ILNode *> > : ValueType
{
}

