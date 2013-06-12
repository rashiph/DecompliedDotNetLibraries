namespace System.Globalization
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;

    internal sealed class GlobalizationAssembly
    {
        [SecurityCritical]
        internal static unsafe byte* GetGlobalizationResourceBytePtr(Assembly assembly, string tableName)
        {
            UnmanagedMemoryStream manifestResourceStream = assembly.GetManifestResourceStream(tableName) as UnmanagedMemoryStream;
            if (manifestResourceStream != null)
            {
                byte* positionPointer = manifestResourceStream.PositionPointer;
                if (positionPointer != null)
                {
                    return positionPointer;
                }
            }
            throw new InvalidOperationException();
        }
    }
}

