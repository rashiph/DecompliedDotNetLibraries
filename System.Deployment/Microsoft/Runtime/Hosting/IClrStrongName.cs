namespace Microsoft.Runtime.Hosting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, SecurityCritical, ComConversionLoss, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")]
    internal interface IClrStrongName
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetHashFromAssemblyFile([In, MarshalAs(UnmanagedType.LPStr)] string pszFilePath, [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] out byte[] pbHash, [In, MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetHashFromAssemblyFileW([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbHash, [In, MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetHashFromBlob([In] IntPtr pbBlob, [In, MarshalAs(UnmanagedType.U4)] int cchBlob, [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbHash, [In, MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetHashFromFile([In, MarshalAs(UnmanagedType.LPStr)] string pszFilePath, [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbHash, [In, MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetHashFromFileW([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbHash, [In, MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetHashFromHandle([In] IntPtr hFile, [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbHash, [In, MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);
        [return: MarshalAs(UnmanagedType.U4)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameCompareAssemblies([In, MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly1, [In, MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly2, [MarshalAs(UnmanagedType.U4)] out int dwResult);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameFreeBuffer([In] IntPtr pbMemory);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameGetBlob([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pbBlob, [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcbBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameGetBlobFromImage([In] IntPtr pbBase, [In, MarshalAs(UnmanagedType.U4)] int dwLength, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pbBlob, [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcbBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameGetPublicKey([In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pbKeyBlob, [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);
        [return: MarshalAs(UnmanagedType.U4)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameHashSize([In, MarshalAs(UnmanagedType.U4)] int ulHashAlg, [MarshalAs(UnmanagedType.U4)] out int cbSize);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameKeyDelete([In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameKeyGen([In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In, MarshalAs(UnmanagedType.U4)] int dwFlags, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameKeyGenEx([In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In, MarshalAs(UnmanagedType.U4)] int dwFlags, [In, MarshalAs(UnmanagedType.U4)] int dwKeySize, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameKeyInstall([In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pbKeyBlob, [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameSignatureGeneration([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbKeyBlob, [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob, [In, Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameSignatureGenerationEx([In, MarshalAs(UnmanagedType.LPWStr)] string wszFilePath, [In, MarshalAs(UnmanagedType.LPWStr)] string wszKeyContainer, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbKeyBlob, [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob, [In, Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob, [In, MarshalAs(UnmanagedType.U4)] int dwFlags);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameSignatureSize([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pbPublicKeyBlob, [In, MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSize);
        [return: MarshalAs(UnmanagedType.U4)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameSignatureVerification([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In, MarshalAs(UnmanagedType.U4)] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);
        [return: MarshalAs(UnmanagedType.U4)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameSignatureVerificationEx([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In, MarshalAs(UnmanagedType.I1)] bool fForceVerification, [MarshalAs(UnmanagedType.I1)] out bool fWasVerified);
        [return: MarshalAs(UnmanagedType.U4)]
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameSignatureVerificationFromImage([In] IntPtr pbBase, [In, MarshalAs(UnmanagedType.U4)] int dwLength, [In, MarshalAs(UnmanagedType.U4)] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameTokenFromAssembly([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameTokenFromAssemblyEx([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int StrongNameTokenFromPublicKey([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pbPublicKeyBlob, [In, MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
    }
}

