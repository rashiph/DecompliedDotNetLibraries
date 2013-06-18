namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Transactions;

    internal static class TracedNativeMethods
    {
        internal static int OCIAttrGet(OciHandle trgthndlp, out byte attributep, out uint sizep, OCI.ATTR attrtype, OciHandle errhp)
        {
            int num2 = 0;
            int num = System.Data.Common.UnsafeNativeMethods.OCIAttrGet(trgthndlp, trgthndlp.HandleType, out num2, out sizep, attrtype, errhp);
            attributep = (byte) num2;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIAttrGet|ADV|OCI|RET>          trgthndlp=0x%-07Ix trghndltyp=%-18ls attrtype=%-22ls errhp=0x%-07Ix attributep=%-20d sizep=%2d rc=%d\n", trgthndlp, trgthndlp.HandleType, attrtype, errhp, attributep, sizep, num);
            }
            return num;
        }

        internal static int OCIAttrGet(OciHandle trgthndlp, out short attributep, out uint sizep, OCI.ATTR attrtype, OciHandle errhp)
        {
            int num2 = 0;
            int num = System.Data.Common.UnsafeNativeMethods.OCIAttrGet(trgthndlp, trgthndlp.HandleType, out num2, out sizep, attrtype, errhp);
            attributep = (short) num2;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIAttrGet|ADV|OCI|RET>          trgthndlp=0x%-07Ix trghndltyp=%-18ls attrtype=%-22ls errhp=0x%-07Ix attributep=%-20d sizep=%2d rc=%d\n", trgthndlp, trgthndlp.HandleType, attrtype, errhp, attributep, sizep, num);
            }
            return num;
        }

        internal static int OCIAttrGet(OciHandle trgthndlp, out int attributep, out uint sizep, OCI.ATTR attrtype, OciHandle errhp)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIAttrGet(trgthndlp, trgthndlp.HandleType, out attributep, out sizep, attrtype, errhp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIAttrGet|ADV|OCI|RET>          trgthndlp=0x%-07Ix trghndltyp=%-18ls attrtype=%-22ls errhp=0x%-07Ix attributep=%-20d sizep=%2d rc=%d\n", trgthndlp, trgthndlp.HandleType, attrtype, errhp, attributep, sizep, num);
            }
            return num;
        }

        internal static int OCIAttrGet(OciHandle trgthndlp, ref IntPtr attributep, ref uint sizep, OCI.ATTR attrtype, OciHandle errhp)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIAttrGet(trgthndlp, trgthndlp.HandleType, ref attributep, ref sizep, attrtype, errhp);
            if (Bid.AdvancedOn)
            {
                if (OCI.ATTR.OCI_ATTR_SQLCODE == attrtype)
                {
                    Bid.Trace("<oc.OCIAttrGet|ADV|OCI|RET>          trgthndlp=0x%-07Ix trghndltyp=%-18ls attrtype=%-22ls errhp=0x%-07Ix attributep=%-20ls sizep=%2d rc=%d\n", trgthndlp, trgthndlp.HandleType, attrtype, errhp, trgthndlp.PtrToString(attributep, (int) sizep), sizep, num);
                    return num;
                }
                Bid.Trace("<oc.OCIAttrGet|ADV|OCI|RET>          trgthndlp=0x%-07Ix trghndltyp=%-18ls attrtype=%-22ls errhp=0x%-07Ix attributep=0x%-18Ix sizep=%2d rc=%d\n", trgthndlp, trgthndlp.HandleType, attrtype, errhp, attributep, sizep, num);
            }
            return num;
        }

        internal static int OCIAttrGet(OciHandle trgthndlp, OciHandle attributep, out uint sizep, OCI.ATTR attrtype, OciHandle errhp)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIAttrGet(trgthndlp, trgthndlp.HandleType, attributep, out sizep, attrtype, errhp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIAttrGet|ADV|OCI|RET>          trgthndlp=0x%-07Ix trghndltyp=%-18ls attrtype=%-22ls errhp=0x%-07Ix attributep=0x%-18Ix sizep=%2d rc=%d\n", trgthndlp, trgthndlp.HandleType, attrtype, errhp, OciHandle.HandleValueToTrace(attributep), sizep, num);
            }
            return num;
        }

        internal static int OCIAttrSet(OciHandle trgthndlp, ref int attributep, uint size, OCI.ATTR attrtype, OciHandle errhp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIAttrSet|ADV|OCI>              trgthndlp=0x%-07Ix trghndltyp=%-18ls attributep=%-9d size=%-2d attrtype=%-22ls errhp=0x%-07Ix\n", trgthndlp, trgthndlp.HandleType, attributep, size, attrtype, errhp);
            }
            return System.Data.Common.UnsafeNativeMethods.OCIAttrSet(trgthndlp, trgthndlp.HandleType, ref attributep, size, attrtype, errhp);
        }

        internal static int OCIAttrSet(OciHandle trgthndlp, OciHandle attributep, uint size, OCI.ATTR attrtype, OciHandle errhp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIAttrSet|ADV|OCI>              trgthndlp=0x%-07Ix trghndltyp=%-18ls attributep=0x%-07Ix size=%d attrtype=%-22ls errhp=0x%-07Ix\n", trgthndlp, trgthndlp.HandleType, attributep, size, attrtype, errhp);
            }
            return System.Data.Common.UnsafeNativeMethods.OCIAttrSet(trgthndlp, trgthndlp.HandleType, attributep, size, attrtype, errhp);
        }

        internal static int OCIAttrSet(OciHandle trgthndlp, byte[] attributep, uint size, OCI.ATTR attrtype, OciHandle errhp)
        {
            if (Bid.AdvancedOn)
            {
                string str;
                if ((OCI.ATTR.OCI_ATTR_EXTERNAL_NAME == attrtype) || (OCI.ATTR.OCI_ATTR_INTERNAL_NAME == attrtype))
                {
                    str = new string(Encoding.UTF8.GetChars(attributep, 0, (int) size));
                }
                else
                {
                    str = attributep.ToString();
                }
                Bid.Trace("<oc.OCIAttrSet|ADV|OCI>              trgthndlp=0x%-07Ix trghndltyp=%-18ls attributep='%ls' size=%d attrtype=%-22ls errhp=0x%-07Ix\n", trgthndlp, trgthndlp.HandleType, str, size, attrtype, errhp);
            }
            return System.Data.Common.UnsafeNativeMethods.OCIAttrSet(trgthndlp, trgthndlp.HandleType, attributep, size, attrtype, errhp);
        }

        internal static int OCIBindByName(OciHandle stmtp, out IntPtr bindpp, OciHandle errhp, string placeholder, int placeh_len, IntPtr valuep, int value_sz, OCI.DATATYPE dty, IntPtr indp, IntPtr alenp, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIBindByName|ADV|OCI>           stmtp=0x%-07Ix errhp=0x%-07Ix placeholder=%-20ls placeh_len=%-2d valuep=0x%-07Ix value_sz=%-4d dty=%d{OCI.DATATYPE} indp=0x%-07Ix *indp=%-3d alenp=0x%-07Ix *alenp=%-4d rcodep=0x%-07Ix maxarr_len=%-4d curelap=0x%-07Ix mode=0x%x{OCI.MODE}\n", OciHandle.HandleValueToTrace(stmtp), OciHandle.HandleValueToTrace(errhp), placeholder, placeh_len, valuep, value_sz, (int) dty, indp, (IntPtr.Zero == indp) ? 0 : Marshal.ReadInt16(indp), alenp, (IntPtr.Zero == alenp) ? 0 : Marshal.ReadInt16(alenp), IntPtr.Zero, 0, IntPtr.Zero, (int) mode);
            }
            byte[] bytes = stmtp.GetBytes(placeholder);
            int length = bytes.Length;
            int num = System.Data.Common.UnsafeNativeMethods.OCIBindByName(stmtp, out bindpp, errhp, bytes, length, valuep, value_sz, dty, indp, alenp, IntPtr.Zero, 0, IntPtr.Zero, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIBindByName|ADV|OCI|RET>       bindpp=0x%-07Ix rc=%d\n", bindpp, num);
            }
            return num;
        }

        internal static int OCIDefineArrayOfStruct(OciHandle defnp, OciHandle errhp, uint pvskip, uint indskip, uint rlskip, uint rcskip)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDefineArrayOfStruct|ADV|OCI>  defnp=0x%-07Ix errhp=0x%-07Ix pvskip=%-4d indskip=%-4d rlskip=%-4d rcskip=%-4d\n", defnp, errhp, pvskip, indskip, rlskip, rcskip);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIDefineArrayOfStruct(defnp, errhp, pvskip, indskip, rlskip, rcskip);
            if ((num != 0) && Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDefineArrayOfStruct|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCIDefineByPos(OciHandle stmtp, out IntPtr hndlpp, OciHandle errhp, uint position, IntPtr valuep, int value_sz, OCI.DATATYPE dty, IntPtr indp, IntPtr rlenp, IntPtr rcodep, OCI.MODE mode)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIDefineByPos(stmtp, out hndlpp, errhp, position, valuep, value_sz, dty, indp, rlenp, rcodep, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDefineByPos|ADV|OCI|RET>      stmtp=0x%-07Ix errhp=0x%-07Ix position=%-2d valuep=0x%-07Ix value_sz=%-4d dty=%-3d %-14ls indp=0x%-07Ix rlenp=0x%-07Ix rcodep=0x%-07Ix mode=0x%x{OCI.MODE} hndlpp=0x%-07Ix rc=%d\n", stmtp, errhp, position, valuep, value_sz, (int) dty, dty, indp, rlenp, rcodep, (int) mode, hndlpp, num);
            }
            return num;
        }

        internal static int OCIDefineDynamic(OciHandle defnp, OciHandle errhp, IntPtr octxp, OCI.Callback.OCICallbackDefine ocbfp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDefineDynamic|ADV|OCI>        defnp=0x%-07Ix errhp=0x%-07Ix octxp=0x%-07Ix ocbfp=...\n", OciHandle.HandleValueToTrace(defnp), OciHandle.HandleValueToTrace(errhp), octxp);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIDefineDynamic(defnp, errhp, octxp, ocbfp);
            if ((num != 0) && Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDefineDynamic|ADV|OCI|RET>    rc=%d\n", num);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static int OCIDescriptorAlloc(OciHandle parenth, out IntPtr hndlpp, OCI.HTYPE type)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIDescriptorAlloc(parenth, out hndlpp, type, IntPtr.Zero, IntPtr.Zero);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDescriptorAlloc|ADV|OCI|RET>  parenth=0x%-07Ix type=%3d xtramemsz=0x%-07Ix usrmempp=0x%-07Ix hndlpp=0x%-07Ix rc=%d\n", parenth, (int) type, IntPtr.Zero, IntPtr.Zero, hndlpp, num);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int OCIDescriptorFree(IntPtr hndlp, OCI.HTYPE type)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIDescriptorFree|ADV|OCI>       hndlp=0x%Id type=%3d\n", hndlp, (int) type);
            }
            return System.Data.Common.UnsafeNativeMethods.OCIDescriptorFree(hndlp, type);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static int OCIEnvCreate(out IntPtr envhpp, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIEnvCreate|ADV|OCI>  mode=0x%x{OCI.MODE} ctxp=0x%-07Ix malocfp=0x%-07Ix ralocfp=0x%-07Ix mfreefp=0x%-07Ix xtramemsz=0x%-07Ix usrmempp=0x%-07Ix", (int) mode, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIEnvCreate(out envhpp, mode, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIEnvCreate|ADV|OCI|RET>       envhpp=0x%-07Ix, rc=%d\n", envhpp, num);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static int OCIEnvNlsCreate(out IntPtr envhpp, OCI.MODE mode, ushort charset, ushort ncharset)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIEnvNlsCreate|ADV|OCI> mode=0x%x{OCI.MODE} ctxp=0x%-07Ix malocfp=0x%-07Ix ralocfp=0x%-07Ix mfreefp=0x%-07Ix xtramemsz=0x%-07Ix usrmempp=0x%-07Ix charset=%d ncharset=%d", (int) mode, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, charset, ncharset);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIEnvNlsCreate(out envhpp, mode, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, charset, ncharset);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIEnvNlsCreate|ADV|OCI|RET>    envhpp=0x%-07Ix rc=%d\n", envhpp, num);
            }
            return num;
        }

        internal static int OCIErrorGet(OciHandle hndlp, int recordno, out int errcodep, NativeBuffer bufp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIErrorGet|ADV|OCI>             hndlp=0x%-07Ix recordno=%d sqlstate=0x%-07Ix bufp=0x%-07Ix bufsiz=%d type=%d{OCI.HTYPE}\n", OciHandle.HandleValueToTrace(hndlp), recordno, IntPtr.Zero, NativeBuffer.HandleValueToTrace(bufp), bufp.Length, (int) hndlp.HandleType);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIErrorGet(hndlp, (uint) recordno, IntPtr.Zero, out errcodep, bufp, (uint) bufp.Length, hndlp.HandleType);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIErrorGet|ADV|OCI|RET>         errcodep=%d rc=%d\n\t%ls\n\n", errcodep, num, hndlp.PtrToString(bufp));
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static int OCIHandleAlloc(OciHandle parenth, out IntPtr hndlpp, OCI.HTYPE type)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIHandleAlloc(parenth, out hndlpp, type, IntPtr.Zero, IntPtr.Zero);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIHandleAlloc|ADV|OCI|RET>      parenth=0x%-07Ix type=%3d xtramemsz=0x%-07Ix usrmempp=0x%-07Ix hndlpp=0x%-07Ix rc=%d\n", parenth, (int) type, IntPtr.Zero, IntPtr.Zero, hndlpp, num);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int OCIHandleFree(IntPtr hndlp, OCI.HTYPE type)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIHandleFree|ADV|OCI>           hndlp=0x%-07Ix type=%3d\n", hndlp, (int) type);
            }
            return System.Data.Common.UnsafeNativeMethods.OCIHandleFree(hndlp, type);
        }

        internal static int OCILobAppend(OciHandle svchp, OciHandle errhp, OciHandle dst_locp, OciHandle src_locp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobAppend|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix dst_locp=0x%-07Ix src_locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(dst_locp), OciHandle.HandleValueToTrace(src_locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobAppend(svchp, errhp, dst_locp, src_locp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobAppend|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobClose(OciHandle svchp, OciHandle errhp, OciHandle locp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oci.OCILobClose|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobClose(svchp, errhp, locp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobClose|ADV|OCI|RET> %d\n", num);
            }
            return num;
        }

        internal static int OCILobCopy(OciHandle svchp, OciHandle errhp, OciHandle dst_locp, OciHandle src_locp, uint amount, uint dst_offset, uint src_offset)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobCopy|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix dst_locp=0x%-07Ix src_locp=0x%-07Ix amount=%u dst_offset=%u src_offset=%u\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(dst_locp), OciHandle.HandleValueToTrace(src_locp), amount, dst_offset, src_offset);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobCopy(svchp, errhp, dst_locp, src_locp, amount, dst_offset, src_offset);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobCopy|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobCreateTemporary(OciHandle svchp, OciHandle errhp, OciHandle locp, [In, MarshalAs(UnmanagedType.U2)] ushort csid, [In, MarshalAs(UnmanagedType.U1)] OCI.CHARSETFORM csfrm, [In, MarshalAs(UnmanagedType.U1)] OCI.LOB_TYPE lobtype, int cache, [In, MarshalAs(UnmanagedType.U2)] OCI.DURATION duration)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobCreateTemporary|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=0x%-07Ix csid=%d csfrm=%d{OCI.CHARSETFORM} lobtype=%d{OCI.LOB_TYPE} cache=%d duration=%d{OCI.DURATION}\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp), csid, (int) csfrm, (int) lobtype, cache, (int) duration);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobCreateTemporary(svchp, errhp, locp, csid, csfrm, lobtype, cache, duration);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobCreateTemporary|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobErase(OciHandle svchp, OciHandle errhp, OciHandle locp, ref uint amount, uint offset)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobErase|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=0x%-07Ix amount=%d, offset=%d\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp), amount, offset);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobErase(svchp, errhp, locp, ref amount, offset);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobErase|ADV|OCI|RET> amount=%u, rc=%d\n", amount, num);
            }
            return num;
        }

        internal static int OCILobFileExists(OciHandle svchp, OciHandle errhp, OciHandle locp, out int flag)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFileExists|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobFileExists(svchp, errhp, locp, out flag);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFileExists|ADV|OCI|RET> flag=%u, rc=%d\n", flag, num);
            }
            return num;
        }

        internal static int OCILobFileGetName(OciHandle envhp, OciHandle errhp, OciHandle filep, IntPtr dir_alias, ref ushort d_length, IntPtr filename, ref ushort f_length)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFileGetName|ADV|OCI> envhp=0x%-07Ix errhp=0x%-07Ix filep=%Id\n", OciHandle.HandleValueToTrace(envhp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(filep));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobFileGetName(envhp, errhp, filep, dir_alias, ref d_length, filename, ref f_length);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFileGetName|ADV|OCI|RET> rc=%d, dir_alias='%ls', d_lenght=%d, filename='%ls', f_length=%d\n", num, envhp.PtrToString(dir_alias, d_length), d_length, envhp.PtrToString(filename, f_length), f_length);
            }
            return num;
        }

        internal static int OCILobFileSetName(OciHandle envhp, OciHandle errhp, OciFileDescriptor filep, string dir_alias, string filename)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFileSetName|ADV|OCI> envhp=0x%-07Ix errhp=0x%-07Ix filep=0x%-07Ix dir_alias='%ls', d_length=%d, filename='%ls', f_length=%d\n", OciHandle.HandleValueToTrace(envhp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(filep), dir_alias, dir_alias.Length, filename, filename.Length);
            }
            byte[] bytes = envhp.GetBytes(dir_alias);
            ushort length = (ushort) bytes.Length;
            byte[] fileName = envhp.GetBytes(filename);
            ushort fileNameLength = (ushort) fileName.Length;
            int num = filep.OCILobFileSetNameWrapper(envhp, errhp, bytes, length, fileName, fileNameLength);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFileSetName|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobFreeTemporary(OciHandle svchp, OciHandle errhp, OciHandle locp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFreeTemporary|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobFreeTemporary(svchp, errhp, locp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobFreeTemporary|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobGetChunkSize(OciHandle svchp, OciHandle errhp, OciHandle locp, out uint lenp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobGetChunkSize|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobGetChunkSize(svchp, errhp, locp, out lenp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobGetChunkSize|ADV|OCI|RET> len=%u, rc=%d\n", lenp, num);
            }
            return num;
        }

        internal static int OCILobGetLength(OciHandle svchp, OciHandle errhp, OciHandle locp, out uint lenp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobGetLength|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobGetLength(svchp, errhp, locp, out lenp);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobGetLength|ADV|OCI|RET> len=%u, rc=%d\n", lenp, num);
            }
            return num;
        }

        internal static int OCILobIsOpen(OciHandle svchp, OciHandle errhp, OciHandle locp, out int flag)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobIsOpen|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobIsOpen(svchp, errhp, locp, out flag);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobIsOpen|ADV|OCI|RET> flag=%d, rc=%d\n", flag, num);
            }
            return num;
        }

        internal static int OCILobIsTemporary(OciHandle envhp, OciHandle errhp, OciHandle locp, out int flag)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobIsTemporary|ADV|OCI> envhp=0x%-07Ix errhp=0x%-07Ix locp=%Id\n", OciHandle.HandleValueToTrace(envhp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobIsTemporary(envhp, errhp, locp, out flag);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobIsTemporary|ADV|OCI|RET> flag=%d, rc=%d\n", flag, num);
            }
            return num;
        }

        internal static int OCILobLoadFromFile(OciHandle svchp, OciHandle errhp, OciHandle dst_locp, OciHandle src_locp, uint amount, uint dst_offset, uint src_offset)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobLoadFromFile|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix dst_locp=0x%-07Ix src_locp=0x%-07Ix amount=%u dst_offset=%u src_offset=%u\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(dst_locp), OciHandle.HandleValueToTrace(src_locp), amount, dst_offset, src_offset);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobLoadFromFile(svchp, errhp, dst_locp, src_locp, amount, dst_offset, src_offset);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobLoadFromFile|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobOpen(OciHandle svchp, OciHandle errhp, OciHandle locp, byte mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobOpen|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=0x%-07Ix mode=%d\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp), (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobOpen(svchp, errhp, locp, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobOpen|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobRead(OciHandle svchp, OciHandle errhp, OciHandle locp, ref int amtp, uint offset, IntPtr bufp, uint bufl, ushort csid, OCI.CHARSETFORM csfrm)
        {
            uint num2 = (uint) amtp;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobRead|ADV|OCI>              svchp=0x%-07Ix errhp=0x%-07Ix locp=0x%-07Ix amt=%-4d offset=%-6u bufp=0x%-07Ix bufl=%-4d ctxp=0x%-07Ix cbfp=0x%-07Ix csid=%-4d csfrm=%d{OCI.CHARSETFORM}\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp), amtp, offset, bufp, (int) bufl, IntPtr.Zero, IntPtr.Zero, csid, (int) csfrm);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobRead(svchp, errhp, locp, ref num2, offset, bufp, bufl, IntPtr.Zero, IntPtr.Zero, csid, csfrm);
            amtp = (int) num2;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobRead|ADV|OCI|RET>          amt=%-4d rc=%d\n", amtp, num);
            }
            return num;
        }

        internal static int OCILobTrim(OciHandle svchp, OciHandle errhp, OciHandle locp, uint newlen)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobTrim|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=0x%-07Ix newlen=%d\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp), newlen);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobTrim(svchp, errhp, locp, newlen);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobTrim|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OCILobWrite(OciHandle svchp, OciHandle errhp, OciHandle locp, ref int amtp, uint offset, IntPtr bufp, uint buflen, byte piece, ushort csid, OCI.CHARSETFORM csfrm)
        {
            uint num2 = (uint) amtp;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobWrite|ADV|OCI> svchp=0x%-07Ix errhp=0x%-07Ix locp=0x%-07Ix amt=%d offset=%u bufp=0x%-07Ix buflen=%d piece=%d{Byte} ctxp=0x%-07Ix cbfp=0x%-07Ix csid=%d csfrm=%d{OCI.CHARSETFORM}\n", OciHandle.HandleValueToTrace(svchp), OciHandle.HandleValueToTrace(errhp), OciHandle.HandleValueToTrace(locp), amtp, offset, bufp, (int) buflen, piece, IntPtr.Zero, IntPtr.Zero, csid, (int) csfrm);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCILobWrite(svchp, errhp, locp, ref num2, offset, bufp, buflen, piece, IntPtr.Zero, IntPtr.Zero, csid, csfrm);
            amtp = (int) num2;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCILobWrite|ADV|OCI|RET> amt=%d, rc=%d\n", amtp, num);
            }
            return num;
        }

        internal static int OCIParamGet(OciHandle hndlp, OCI.HTYPE hType, OciHandle errhp, out IntPtr paramdpp, int pos)
        {
            int num = System.Data.Common.UnsafeNativeMethods.OCIParamGet(hndlp, hType, errhp, out paramdpp, (uint) pos);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIParamGet|ADV|OCI|RET>         hndlp=0x%-07Ix htype=%-18ls errhp=0x%-07Ix pos=%d paramdpp=0x%-07Ix rc=%d\n", hndlp, hType, errhp, pos, paramdpp, num);
            }
            return num;
        }

        internal static int OCIRowidToChar(OciHandle rowidDesc, NativeBuffer outbfp, ref int bufferLength, OciHandle errhp)
        {
            ushort outbflp = (ushort) bufferLength;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIRowidToChar|ADV|OCI>          rowidDesc=0x%-07Ix outbfp=0x%-07Ix outbflp=%d, errhp=0x%-07Ix\n", OciHandle.HandleValueToTrace(rowidDesc), NativeBuffer.HandleValueToTrace(outbfp), outbfp.Length, OciHandle.HandleValueToTrace(errhp));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIRowidToChar(rowidDesc, outbfp, ref outbflp, errhp);
            bufferLength = outbflp;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIRowidToChar|ADV|OCI|RET>      outbfp='%ls' rc=%d\n", outbfp.PtrToStringAnsi(0, outbflp), num);
            }
            return num;
        }

        internal static int OCIServerAttach(OciHandle srvhp, OciHandle errhp, string dblink, int dblink_len, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIServerAttach|ADV|OCI>         srvhp=0x%-07Ix errhp=0x%-07Ix dblink='%ls' dblink_len=%d mode=0x%x{OCI.MODE}\n", srvhp, errhp, dblink, dblink_len, (int) mode);
            }
            byte[] bytes = srvhp.GetBytes(dblink);
            int length = bytes.Length;
            int num = System.Data.Common.UnsafeNativeMethods.OCIServerAttach(srvhp, errhp, bytes, length, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIServerAttach|ADV|OCI|RET>     rc=%d\n", num);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int OCIServerDetach(IntPtr srvhp, IntPtr errhp, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIServerDetach|ADV|OCI>        srvhp=0x%-07Ix errhp=0x%-07Ix mode=0x%x{OCI.MODE}\n", srvhp, errhp, (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIServerDetach(srvhp, errhp, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIServerDetach|ADV|OCI|RET>    rc=%d\n", num);
            }
            return num;
        }

        internal static int OCIServerVersion(OciHandle hndlp, OciHandle errhp, NativeBuffer bufp)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIServerVersion|ADV|OCI>        hndlp=0x%-07Ix errhp=0x%-07Ix bufp=0x%-07Ix bufsz=%d hndltype=%d{OCI.HTYPE}\n", OciHandle.HandleValueToTrace(hndlp), OciHandle.HandleValueToTrace(errhp), NativeBuffer.HandleValueToTrace(bufp), bufp.Length, (int) hndlp.HandleType);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIServerVersion(hndlp, errhp, bufp, (uint) bufp.Length, (byte) hndlp.HandleType);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIServerVersion|ADV|OCI|RET>    rc=%d\n%ls\n\n", num, hndlp.PtrToString(bufp));
            }
            return num;
        }

        internal static int OCISessionBegin(OciHandle svchp, OciHandle errhp, OciHandle usrhp, OCI.CRED credt, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCISessionBegin|ADV|OCI>         svchp=0x%-07Ix errhp=0x%-07Ix usrhp=0x%-07Ix credt=%ls mode=0x%x{OCI.MODE}\n", svchp, errhp, usrhp, credt, (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCISessionBegin(svchp, errhp, usrhp, credt, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCISessionBegin|ADV|OCI|RET>     rc=%d\n", num);
            }
            return num;
        }

        internal static int OCISessionEnd(IntPtr svchp, IntPtr errhp, IntPtr usrhp, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCISessionEnd|ADV|OCI>           svchp=0x%-07Ix errhp=0x%-07Ix usrhp=0x%-07Ix mode=0x%x{OCI.MODE}\n", svchp, errhp, usrhp, (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCISessionEnd(svchp, errhp, usrhp, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCISessionEnd|ADV|OCI|RET>       rc=%d\n", num);
            }
            return num;
        }

        internal static int OCIStmtExecute(OciHandle svchp, OciHandle stmtp, OciHandle errhp, int iters, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIStmtExecute|ADV|OCI>          svchp=0x%-07Ix stmtp=0x%-07Ix errhp=0x%-07Ix iters=%d rowoff=%d snap_in=0x%-07Ix snap_out=0x%-07Ix mode=0x%x{OCI.MODE}\n", svchp, stmtp, errhp, iters, 0, IntPtr.Zero, IntPtr.Zero, (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIStmtExecute(svchp, stmtp, errhp, (uint) iters, 0, IntPtr.Zero, IntPtr.Zero, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIStmtExecute|ADV|OCI|RET>      rc=%d\n", num);
            }
            return num;
        }

        internal static int OCIStmtFetch(OciHandle stmtp, OciHandle errhp, int nrows, OCI.FETCH orientation, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIStmtFetch|ADV|OCI>            stmtp=0x%-07Ix errhp=0x%-07Ix nrows=%d orientation=%d{OCI.FETCH}, mode=0x%x{OCI.MODE}\n", stmtp, errhp, nrows, (int) orientation, (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCIStmtFetch(stmtp, errhp, (uint) nrows, orientation, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIStmtFetch|ADV|OCI|RET>        rc=%d\n", num);
            }
            return num;
        }

        internal static int OCIStmtPrepare(OciHandle stmtp, OciHandle errhp, string stmt, OCI.SYNTAX language, OCI.MODE mode, OracleConnection connection)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIStmtPrepare|ADV|OCI>          stmtp=0x%-07Ix errhp=0x%-07Ix stmt_len=%d language=%d{OCI.SYNTAX} mode=0x%x{OCI.MODE}\n\t\t%ls\n\n", stmtp, errhp, stmt.Length, (int) language, (int) mode, stmt);
            }
            byte[] bytes = connection.GetBytes(stmt, false);
            uint length = (uint) bytes.Length;
            int num = System.Data.Common.UnsafeNativeMethods.OCIStmtPrepare(stmtp, errhp, bytes, length, language, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCIStmtPrepare|ADV|OCI|RET>      rc=%d\n", num);
            }
            return num;
        }

        internal static int OCITransCommit(OciHandle srvhp, OciHandle errhp, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCITransCommit|ADV|OCI>          srvhp=0x%-07Ix errhp=0x%-07Ix mode=0x%x{OCI.MODE}\n", OciHandle.HandleValueToTrace(srvhp), OciHandle.HandleValueToTrace(errhp), (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCITransCommit(srvhp, errhp, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCITransCommit|ADV|OCI|RET>      rc=%d\n", num);
            }
            return num;
        }

        internal static int OCITransRollback(OciHandle srvhp, OciHandle errhp, OCI.MODE mode)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCITransRollback|ADV|OCI>         srvhp=0x%-07Ix errhp=0x%-07Ix mode=0x%x{OCI.MODE}\n", OciHandle.HandleValueToTrace(srvhp), OciHandle.HandleValueToTrace(errhp), (int) mode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OCITransRollback(srvhp, errhp, mode);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OCITransRollback|ADV|OCI|RET>      rc=%d\n", num);
            }
            return num;
        }

        internal static int oermsg(short rcode, NativeBuffer buf)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.oermsg|ADV|OCI> rcode=%d\n", rcode);
            }
            int num = System.Data.Common.UnsafeNativeMethods.oermsg(rcode, buf);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.oermsg|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static int OraMTSEnlCtxGet(byte[] userName, byte[] password, byte[] serverName, OciHandle pOCISvc, OciHandle pOCIErr, out IntPtr pCtxt)
        {
            int num;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<oc.OraMTSEnlCtxGet|ADV|OCI> userName=..., password=..., serverName=..., pOCISvc=0x%-07Ix pOCIErr=0x%-07Ix dwFlags=0x%08X\n", OciHandle.HandleValueToTrace(pOCISvc), OciHandle.HandleValueToTrace(pOCIErr), 0);
                }
                num = System.Data.Common.UnsafeNativeMethods.OraMTSEnlCtxGet(userName, password, serverName, pOCISvc, pOCIErr, 0, out pCtxt);
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<oc.OraMTSEnlCtxGet|ADV|OCI|RET> pCtxt=0x%-07Ix rc=%d\n", pCtxt, num);
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int OraMTSEnlCtxRel(IntPtr pCtxt)
        {
            int num;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<oc.OraMTSEnlCtxRel|ADV|OCI> pCtxt=%Id\n", pCtxt);
                }
                num = System.Data.Common.UnsafeNativeMethods.OraMTSEnlCtxRel(pCtxt);
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<oc.OraMTSEnlCtxRel|ADV|OCI|RET> rc=%d\n", num);
                }
            }
            return num;
        }

        internal static int OraMTSJoinTxn(OciEnlistContext pCtxt, IDtcTransaction pTrans)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OraMTSJoinTxn|ADV|OCI> pCtxt=0x%-07Ix pTrans=...\n", OciEnlistContext.HandleValueToTrace(pCtxt));
            }
            int num = System.Data.Common.UnsafeNativeMethods.OraMTSJoinTxn(pCtxt, pTrans);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OraMTSJoinTxn|ADV|OCI|RET> rc=%d\n", num);
            }
            return num;
        }

        internal static int OraMTSOCIErrGet(ref int dwErr, NativeBuffer lpcEMsg, ref int lpdLen)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc.OraMTSOCIErrGet|ADV|OCI> dwErr=%08X, lpcEMsg=0x%-07Ix lpdLen=%d\n", dwErr, NativeBuffer.HandleValueToTrace(lpcEMsg), lpdLen);
            }
            int num = System.Data.Common.UnsafeNativeMethods.OraMTSOCIErrGet(ref dwErr, lpcEMsg, ref lpdLen);
            if (Bid.AdvancedOn)
            {
                if (num == 0)
                {
                    Bid.Trace("<oc.OraMTSOCIErrGet|ADV|OCI|RET> rc=%d\n", num);
                    return num;
                }
                string str = lpcEMsg.PtrToStringAnsi(0, lpdLen);
                Bid.Trace("<oc.OraMTSOCIErrGet|ADV|OCI|RET> rd=%d message='%ls', lpdLen=%d\n", num, str, lpdLen);
            }
            return num;
        }
    }
}

