namespace System.Data.Common
{
    using System;
    using System.Data.OracleClient;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Transactions;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class UnsafeNativeMethods
    {
        private UnsafeNativeMethods()
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool CheckTokenMembership(IntPtr tokenHandle, byte[] sidToCheck, out bool isMember);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool ConvertSidToStringSidW(IntPtr sid, out IntPtr stringSid);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int CreateWellKnownSid(int sidType, byte[] domainSid, [Out] byte[] resultSid, ref uint resultSidLength);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool GetTokenInformation(IntPtr tokenHandle, uint token_class, IntPtr tokenStruct, uint tokenInformationLength, ref uint tokenString);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool IsTokenRestricted(IntPtr tokenHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        internal static extern int lstrlenA(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int lstrlenW(IntPtr ptr);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIAttrGet(OciHandle trgthndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE trghndltyp, OciHandle attributep, out uint sizep, [In, MarshalAs(UnmanagedType.U4)] OCI.ATTR attrtype, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIAttrGet(OciHandle trgthndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE trghndltyp, out int attributep, out uint sizep, [In, MarshalAs(UnmanagedType.U4)] OCI.ATTR attrtype, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIAttrGet(OciHandle trgthndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE trghndltyp, ref IntPtr attributep, ref uint sizep, [In, MarshalAs(UnmanagedType.U4)] OCI.ATTR attrtype, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIAttrSet(OciHandle trgthndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE trghndltyp, OciHandle attributep, uint size, [In, MarshalAs(UnmanagedType.U4)] OCI.ATTR attrtype, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIAttrSet(OciHandle trgthndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE trghndltyp, ref int attributep, uint size, [In, MarshalAs(UnmanagedType.U4)] OCI.ATTR attrtype, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIAttrSet(OciHandle trgthndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE trghndltyp, [In, MarshalAs(UnmanagedType.LPArray)] byte[] attributep, uint size, [In, MarshalAs(UnmanagedType.U4)] OCI.ATTR attrtype, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIBindByName(OciHandle stmtp, out IntPtr bindpp, OciHandle errhp, [In, MarshalAs(UnmanagedType.LPArray)] byte[] placeholder, int placeh_len, IntPtr valuep, int value_sz, [In, MarshalAs(UnmanagedType.U2)] OCI.DATATYPE dty, IntPtr indp, IntPtr alenp, IntPtr rcodep, uint maxarr_len, IntPtr curelap, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCICharSetToUnicode(OciHandle hndl, IntPtr dst, uint dstsz, IntPtr src, uint srcsz, out uint size);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDateTimeFromArray(OciHandle hndl, OciHandle err, [In, MarshalAs(UnmanagedType.LPArray)] byte[] inarray, uint len, byte type, OciHandle datetime, OciHandle reftz, byte fsprec);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDateTimeGetTimeZoneOffset(OciHandle hndl, OciHandle err, OciHandle datetime, out sbyte hour, out sbyte min);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDateTimeToArray(OciHandle hndl, OciHandle err, OciHandle datetime, OciHandle reftz, [In, MarshalAs(UnmanagedType.LPArray)] byte[] outarray, ref uint len, byte fsprec);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDefineArrayOfStruct(OciHandle defnp, OciHandle errhp, uint pvskip, uint indskip, uint rlskip, uint rcskip);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDefineByPos(OciHandle stmtp, out IntPtr hndlpp, OciHandle errhp, uint position, IntPtr valuep, int value_sz, [In, MarshalAs(UnmanagedType.U2)] OCI.DATATYPE dty, IntPtr indp, IntPtr alenp, IntPtr rcodep, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDefineDynamic(OciHandle defnp, OciHandle errhp, IntPtr octxp, OCI.Callback.OCICallbackDefine ocbfp);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDescriptorAlloc(OciHandle parenth, out IntPtr descp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE type, IntPtr xtramem_sz, IntPtr usrmempp);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIDescriptorFree(IntPtr hndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE type);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIEnvCreate(out IntPtr envhpp, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode, IntPtr ctxp, IntPtr malocfp, IntPtr ralocfp, IntPtr mfreefp, IntPtr xtramemsz, IntPtr usrmempp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIEnvNlsCreate(out IntPtr envhpp, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode, IntPtr ctxp, IntPtr malocfp, IntPtr ralocfp, IntPtr mfreefp, IntPtr xtramemsz, IntPtr usrmempp, ushort charset, ushort ncharset);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIErrorGet(OciHandle hndlp, uint recordno, IntPtr sqlstate, out int errcodep, NativeBuffer bufp, uint bufsiz, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE type);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIHandleAlloc(OciHandle parenth, out IntPtr hndlpp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE type, IntPtr xtramemsz, IntPtr usrmempp);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIHandleFree(IntPtr hndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE type);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobAppend(OciHandle svchp, OciHandle errhp, OciHandle dst_locp, OciHandle src_locp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobClose(OciHandle svchp, OciHandle errhp, OciHandle locp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobCopy(OciHandle svchp, OciHandle errhp, OciHandle dst_locp, OciHandle src_locp, uint amount, uint dst_offset, uint src_offset);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobCopy2(IntPtr svchp, IntPtr errhp, IntPtr dst_locp, IntPtr src_locp, ulong amount, ulong dst_offset, ulong src_offset);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobCreateTemporary(OciHandle svchp, OciHandle errhp, OciHandle locp, ushort csid, [In, MarshalAs(UnmanagedType.U1)] OCI.CHARSETFORM csfrm, [In, MarshalAs(UnmanagedType.U1)] OCI.LOB_TYPE lobtype, int cache, [In, MarshalAs(UnmanagedType.U2)] OCI.DURATION duration);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobErase(OciHandle svchp, OciHandle errhp, OciHandle locp, ref uint amount, uint offset);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobFileExists(OciHandle svchp, OciHandle errhp, OciHandle locp, out int flag);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobFileGetName(OciHandle envhp, OciHandle errhp, OciHandle filep, IntPtr dir_alias, ref ushort d_length, IntPtr filename, ref ushort f_length);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobFileSetName(OciHandle envhp, OciHandle errhp, ref IntPtr filep, [In, MarshalAs(UnmanagedType.LPArray)] byte[] dir_alias, ushort d_length, [In, MarshalAs(UnmanagedType.LPArray)] byte[] filename, ushort f_length);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobFreeTemporary(OciHandle svchp, OciHandle errhp, OciHandle locp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobGetChunkSize(OciHandle svchp, OciHandle errhp, OciHandle locp, out uint lenp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobGetLength(OciHandle svchp, OciHandle errhp, OciHandle locp, out uint lenp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobIsOpen(OciHandle svchp, OciHandle errhp, OciHandle locp, out int flag);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobIsTemporary(OciHandle envhp, OciHandle errhp, OciHandle locp, out int flag);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobLoadFromFile(OciHandle svchp, OciHandle errhp, OciHandle dst_locp, OciHandle src_locp, uint amount, uint dst_offset, uint src_offset);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobOpen(OciHandle svchp, OciHandle errhp, OciHandle locp, byte mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobRead(OciHandle svchp, OciHandle errhp, OciHandle locp, ref uint amtp, uint offset, IntPtr bufp, uint bufl, IntPtr ctxp, IntPtr cbfp, ushort csid, [In, MarshalAs(UnmanagedType.U1)] OCI.CHARSETFORM csfrm);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobTrim(OciHandle svchp, OciHandle errhp, OciHandle locp, uint newlen);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCILobWrite(OciHandle svchp, OciHandle errhp, OciHandle locp, ref uint amtp, uint offset, IntPtr bufp, uint buflen, byte piece, IntPtr ctxp, IntPtr cbfp, ushort csid, [In, MarshalAs(UnmanagedType.U1)] OCI.CHARSETFORM csfrm);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberAbs(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberAdd(OciHandle err, byte[] number1, byte[] number2, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberArcCos(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberArcSin(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberArcTan(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberArcTan2(OciHandle err, byte[] number1, byte[] number2, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberCeil(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberCmp(OciHandle err, byte[] number1, byte[] number2, out int result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberCos(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberDiv(OciHandle err, byte[] number1, byte[] number2, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberExp(OciHandle err, byte[] p, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberFloor(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberFromInt(OciHandle err, ref int inum, uint inum_length, OCI.SIGN inum_s_flag, byte[] number);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberFromInt(OciHandle err, ref long inum, uint inum_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN inum_s_flag, byte[] number);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberFromInt(OciHandle err, ref uint inum, uint inum_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN inum_s_flag, byte[] number);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberFromInt(OciHandle err, ref ulong inum, uint inum_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN inum_s_flag, byte[] number);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberFromReal(OciHandle err, ref double rnum, uint rnum_length, byte[] number);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        internal static extern int OCINumberFromText(OciHandle err, [In, MarshalAs(UnmanagedType.LPStr)] string str, uint str_length, [In, MarshalAs(UnmanagedType.LPStr)] string fmt, uint fmt_length, IntPtr nls_params, uint nls_p_length, byte[] number);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberHypCos(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberHypSin(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberHypTan(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberIntPower(OciHandle err, byte[] baseNumber, int exponent, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberIsInt(OciHandle err, byte[] number, out int result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberLn(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberLog(OciHandle err, byte[] b, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberMod(OciHandle err, byte[] number1, byte[] number2, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberMul(OciHandle err, byte[] number1, byte[] number2, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberNeg(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberPower(OciHandle err, byte[] baseNumber, byte[] exponent, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberRound(OciHandle err, byte[] number, int decplace, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberShift(OciHandle err, byte[] baseNumber, int nDig, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberSign(OciHandle err, byte[] number, out int result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberSin(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberSqrt(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberSub(OciHandle err, byte[] number1, byte[] number2, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberTan(OciHandle err, byte[] number, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberToInt(OciHandle err, byte[] number, uint rsl_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN rsl_flag, out int rsl);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberToInt(OciHandle err, byte[] number, uint rsl_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN rsl_flag, out long rsl);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberToInt(OciHandle err, byte[] number, uint rsl_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN rsl_flag, out uint rsl);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberToInt(OciHandle err, byte[] number, uint rsl_length, [In, MarshalAs(UnmanagedType.U4)] OCI.SIGN rsl_flag, out ulong rsl);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberToReal(OciHandle err, byte[] number, uint rsl_length, out double rsl);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        internal static extern int OCINumberToText(OciHandle err, byte[] number, [In, MarshalAs(UnmanagedType.LPStr)] string fmt, int fmt_length, IntPtr nls_params, uint nls_p_length, ref uint buf_size, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] buffer);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCINumberTrunc(OciHandle err, byte[] number, int decplace, byte[] result);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIParamGet(OciHandle hndlp, [In, MarshalAs(UnmanagedType.U4)] OCI.HTYPE htype, OciHandle errhp, out IntPtr paramdpp, uint pos);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIRowidToChar(OciHandle rowidDesc, NativeBuffer outbfp, ref ushort outbflp, OciHandle errhp);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIServerAttach(OciHandle srvhp, OciHandle errhp, [In, MarshalAs(UnmanagedType.LPArray)] byte[] dblink, int dblink_len, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIServerDetach(IntPtr srvhp, IntPtr errhp, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIServerVersion(OciHandle hndlp, OciHandle errhp, NativeBuffer bufp, uint bufsz, [In, MarshalAs(UnmanagedType.U1)] byte hndltype);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCISessionBegin(OciHandle svchp, OciHandle errhp, OciHandle usrhp, [In, MarshalAs(UnmanagedType.U4)] OCI.CRED credt, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCISessionEnd(IntPtr svchp, IntPtr errhp, IntPtr usrhp, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIStmtExecute(OciHandle svchp, OciHandle stmtp, OciHandle errhp, uint iters, uint rowoff, IntPtr snap_in, IntPtr snap_out, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIStmtFetch(OciHandle stmtp, OciHandle errhp, uint nrows, [In, MarshalAs(UnmanagedType.U2)] OCI.FETCH orientation, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIStmtPrepare(OciHandle stmtp, OciHandle errhp, [In, MarshalAs(UnmanagedType.LPArray)] byte[] stmt, uint stmt_len, [In, MarshalAs(UnmanagedType.U4)] OCI.SYNTAX language, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE mode);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCITransCommit(OciHandle svchp, OciHandle errhp, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE flags);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCITransRollback(OciHandle svchp, OciHandle errhp, [In, MarshalAs(UnmanagedType.U4)] OCI.MODE flags);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OCIUnicodeToCharSet(OciHandle hndl, IntPtr dst, uint dstsz, IntPtr src, uint srcsz, out uint size);
        [DllImport("oci.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int oermsg(short rcode, NativeBuffer buf);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oramts.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OraMTSEnlCtxGet([In, MarshalAs(UnmanagedType.LPArray)] byte[] lpUname, [In, MarshalAs(UnmanagedType.LPArray)] byte[] lpPsswd, [In, MarshalAs(UnmanagedType.LPArray)] byte[] lpDbnam, OciHandle pOCISvc, OciHandle pOCIErr, uint dwFlags, out IntPtr pCtxt);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oramts.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OraMTSEnlCtxRel(IntPtr pCtxt);
        [DllImport("oramts.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OraMTSJoinTxn(OciEnlistContext pCtxt, IDtcTransaction pTrans);
        [DllImport("oramts.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int OraMTSOCIErrGet(ref int dwErr, NativeBuffer lpcEMsg, ref int lpdLen);
        [DllImport("kernel32.dll")]
        internal static extern void SetLastError(int dwErrCode);
    }
}

