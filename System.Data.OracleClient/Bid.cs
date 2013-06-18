using System;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[ComVisible(false)]
internal static class Bid
{
    private static IntPtr __defaultCmdSpace;
    private static IntPtr __noData;
    private static object _setBitsLock = new object();
    private static AutoInit ai;
    private const int BidVer = 0x23fa;
    private const uint configFlags = 0xd0000000;
    private static BindingCookie cookieObject;
    private static CtrlCB ctrlCallback;
    private const string dllName = "System.Data.OracleClient.dll";
    private static GCHandle hCookie;
    private static ApiGroup modFlags;
    private static IntPtr modID = internalInitialize();
    private static string modIdentity;

    internal static bool AddMetaText(string metaStr)
    {
        if (modID != NoData)
        {
            NativeMethods.AddMetaText(modID, DefaultCmdSpace, CtlCmd.AddMetaText, IntPtr.Zero, metaStr, IntPtr.Zero);
        }
        return true;
    }

    [Conditional("DEBUG")]
    internal static void DASSERT(bool condition)
    {
        if (!condition)
        {
            System.Diagnostics.Trace.Assert(false);
        }
    }

    private static void deterministicStaticInit()
    {
        __noData = (IntPtr) (-1);
        __defaultCmdSpace = (IntPtr) (-1);
        modFlags = ApiGroup.Off;
        modIdentity = string.Empty;
        ctrlCallback = new CtrlCB(Bid.SetApiGroupBits);
        cookieObject = new BindingCookie();
        hCookie = GCHandle.Alloc(cookieObject, GCHandleType.Pinned);
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    private static void doneEntryPoint()
    {
        if (modID == NoData)
        {
            modFlags = ApiGroup.Off;
        }
        else
        {
            try
            {
                NativeMethods.DllBidEntryPoint(ref modID, 0, IntPtr.Zero, 0xd0000000, ref modFlags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                NativeMethods.DllBidFinalize();
            }
            catch
            {
                modFlags = ApiGroup.Off;
            }
            finally
            {
                cookieObject.Invalidate();
                modID = NoData;
                modFlags = ApiGroup.Off;
            }
        }
    }

    [Conditional("DEBUG")]
    internal static void DTRACE(string strConst)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.PutStr(modID, UIntPtr.Zero, (UIntPtr) 1, strConst);
        }
    }

    [Conditional("DEBUG")]
    internal static void DTRACE(string clrFormatString, params object[] args)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.PutStr(modID, UIntPtr.Zero, (UIntPtr) 1, string.Format(CultureInfo.CurrentCulture, clrFormatString, args));
        }
    }

    private static string getAppDomainFriendlyName()
    {
        string friendlyName = AppDomain.CurrentDomain.FriendlyName;
        if ((friendlyName == null) || (friendlyName.Length <= 0))
        {
            friendlyName = "AppDomain.H" + AppDomain.CurrentDomain.GetHashCode();
        }
        return VersioningHelper.MakeVersionSafeName(friendlyName, ResourceScope.Machine, ResourceScope.AppDomain);
    }

    private static string getIdentity(Module mod)
    {
        object[] customAttributes = mod.GetCustomAttributes(typeof(BidIdentityAttribute), true);
        if (customAttributes.Length == 0)
        {
            return mod.Name;
        }
        return ((BidIdentityAttribute) customAttributes[0]).IdentityString;
    }

    [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
    private static string getModulePath(Module mod)
    {
        return mod.FullyQualifiedName;
    }

    private static void initEntryPoint()
    {
        NativeMethods.DllBidInitialize();
        Module manifestModule = Assembly.GetExecutingAssembly().ManifestModule;
        modIdentity = getIdentity(manifestModule);
        modID = NoData;
        string friendlyName = getAppDomainFriendlyName();
        BIDEXTINFO pExtInfo = new BIDEXTINFO(Marshal.GetHINSTANCE(manifestModule), getModulePath(manifestModule), friendlyName, hCookie.AddrOfPinnedObject());
        NativeMethods.DllBidEntryPoint(ref modID, 0x23fa, modIdentity, 0xd0000000, ref modFlags, ctrlCallback, ref pExtInfo, IntPtr.Zero, IntPtr.Zero);
        if (modID != NoData)
        {
            foreach (object obj2 in manifestModule.GetCustomAttributes(typeof(BidMetaTextAttribute), true))
            {
                AddMetaText(((BidMetaTextAttribute) obj2).MetaText);
            }
            Trace("<ds.Bid|Info> VersionSafeName='%ls'\n", friendlyName);
        }
    }

    private static IntPtr internalInitialize()
    {
        deterministicStaticInit();
        ai = new AutoInit();
        return modID;
    }

    internal static bool IsOn(ApiGroup flag)
    {
        return ((modFlags & flag) != ApiGroup.Off);
    }

    internal static void PoolerScopeEnter(out IntPtr hScp, string fmtPrintfW, int a1)
    {
        if (((modFlags & ApiGroup.Pooling) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.ScopeEnter(modID, UIntPtr.Zero, UIntPtr.Zero, out hScp, fmtPrintfW, a1);
        }
        else
        {
            hScp = NoData;
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void PoolerTrace(string fmtPrintfW, int a1)
    {
        if (((modFlags & ApiGroup.Pooling) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1);
        }
    }

    internal static void PoolerTrace(string fmtPrintfW, int a1, Exception a2)
    {
        if (((modFlags & ApiGroup.Pooling) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2.ToString());
        }
    }

    internal static void PoolerTrace(string fmtPrintfW, int a1, int a2)
    {
        if (((modFlags & ApiGroup.Pooling) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2);
        }
    }

    internal static void PoolerTrace(string fmtPrintfW, int a1, int a2, int a3)
    {
        if (((modFlags & ApiGroup.Pooling) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3);
        }
    }

    internal static void PutStr(string str)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.PutStr(modID, UIntPtr.Zero, (UIntPtr) 0, str);
        }
    }

    internal static void ScopeEnter(out IntPtr hScp, string strConst)
    {
        if (((modFlags & ApiGroup.Scope) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.ScopeEnter(modID, UIntPtr.Zero, UIntPtr.Zero, out hScp, strConst);
        }
        else
        {
            hScp = NoData;
        }
    }

    internal static void ScopeEnter(out IntPtr hScp, string fmtPrintfW, int a1)
    {
        if (((modFlags & ApiGroup.Scope) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.ScopeEnter(modID, UIntPtr.Zero, UIntPtr.Zero, out hScp, fmtPrintfW, a1);
        }
        else
        {
            hScp = NoData;
        }
    }

    internal static void ScopeEnter(out IntPtr hScp, string fmtPrintfW, int a1, int a2)
    {
        if (((modFlags & ApiGroup.Scope) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.ScopeEnter(modID, UIntPtr.Zero, UIntPtr.Zero, out hScp, fmtPrintfW, a1, a2);
        }
        else
        {
            hScp = NoData;
        }
    }

    internal static void ScopeEnter(out IntPtr hScp, string fmtPrintfW, int a1, string a2)
    {
        if (((modFlags & ApiGroup.Scope) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.ScopeEnter(modID, UIntPtr.Zero, UIntPtr.Zero, out hScp, fmtPrintfW, a1, a2);
        }
        else
        {
            hScp = NoData;
        }
    }

    internal static void ScopeLeave(ref IntPtr hScp)
    {
        if (((modFlags & ApiGroup.Scope) != ApiGroup.Off) && (modID != NoData))
        {
            if (hScp != NoData)
            {
                NativeMethods.ScopeLeave(modID, UIntPtr.Zero, UIntPtr.Zero, ref hScp);
            }
        }
        else
        {
            hScp = NoData;
        }
    }

    internal static ApiGroup SetApiGroupBits(ApiGroup mask, ApiGroup bits)
    {
        lock (_setBitsLock)
        {
            ApiGroup modFlags = Bid.modFlags;
            if (mask != ApiGroup.Off)
            {
                Bid.modFlags ^= (bits ^ modFlags) & mask;
            }
            return modFlags;
        }
    }

    internal static void Trace(string strConst)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, strConst);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, int a1)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, IntPtr a1)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1);
        }
    }

    internal static void Trace(string fmtPrintfW, string a1)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1);
        }
    }

    internal static void Trace(string fmtPrintfW, int a1, int a2)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2);
        }
    }

    internal static void Trace(string fmtPrintfW, int a1, string a2)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, IntPtr a1, int a2)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2);
        }
    }

    internal static void Trace(string fmtPrintfW, string a1, int a2)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2);
        }
    }

    internal static void Trace(string fmtPrintfW, uint a1, int a2)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2);
        }
    }

    internal static void Trace(string fmtPrintfW, int a1, int a2, string a3)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3);
        }
    }

    internal static void Trace(string fmtPrintfW, int a1, IntPtr a2, int a3)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3);
        }
    }

    internal static void Trace(string fmtPrintfW, int a1, string a2, int a3)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, int a3)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, int a3, IntPtr a4)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, uint a4)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, OciHandle a3, OCI.CRED a4, int a5)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), OciHandle.HandleValueToTrace(a3), a4.ToString(), a5);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, int a3, int a4, int a5)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), a3, a4, a5);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, string a3, int a4, int a5)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), a3, a4, a5);
        }
    }

    internal static void Trace(string fmtPrintfW, int a1, string a2, int a3, string a4, int a5)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, int a5)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, uint a4, uint a5)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, OciHandle a3, int a4, IntPtr a5, int a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), OciHandle.HandleValueToTrace(a3), a4, a5, a6);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, OciHandle a3, uint a4, OCI.ATTR a5, OciHandle a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), OciHandle.HandleValueToTrace(a3), a4, OciHandle.GetAttributeName(a1, a5), OciHandle.HandleValueToTrace(a6));
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, int a3, uint a4, OCI.ATTR a5, OciHandle a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), a3, a4, OciHandle.GetAttributeName(a1, a5), OciHandle.HandleValueToTrace(a6));
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, string a3, uint a4, OCI.ATTR a5, OciHandle a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), a3, a4, OciHandle.GetAttributeName(a1, a5), OciHandle.HandleValueToTrace(a6));
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, int a3, int a4, int a5, string a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), a3, a4, a5, a6);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, uint a3, uint a4, uint a5, uint a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), a3, a4, a5, a6);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, OciHandle a1, int a2, IntPtr a3, IntPtr a4, IntPtr a5, int a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2, a3, a4, a5, a6);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, int a2, IntPtr a3, IntPtr a4, int a5, int a6)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, OCI.ATTR a3, OciHandle a4, int a5, uint a6, int a7)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), OciHandle.GetAttributeName(a1, a3), OciHandle.HandleValueToTrace(a4), a5, a6, a7);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, OCI.ATTR a3, OciHandle a4, IntPtr a5, uint a6, int a7)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), OciHandle.GetAttributeName(a1, a3), OciHandle.HandleValueToTrace(a4), a5, a6, a7);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OCI.HTYPE a2, OCI.ATTR a3, OciHandle a4, string a5, uint a6, int a7)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), a2.ToString(), OciHandle.GetAttributeName(a1, a3), OciHandle.HandleValueToTrace(a4), a5, a6, a7);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, int a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, uint a5, uint a6, uint a7)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, string a4, int a5, string a6, int a7)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, OciHandle a3, int a4, int a5, IntPtr a6, IntPtr a7, int a8)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), OciHandle.HandleValueToTrace(a3), a4, a5, a6, a7, a8);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, int a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7, a8);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, int a5, int a6, int a7, int a8)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7, a8);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static void Trace(string fmtPrintfW, int a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, int a8, int a9)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7, a8, a9);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, uint a5, IntPtr a6, int a7, IntPtr a8, IntPtr a9, int a10, int a11)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, uint a5, IntPtr a6, int a7, int a8, IntPtr a9, IntPtr a10, int a11, int a12)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
        }
    }

    internal static void Trace(string fmtPrintfW, OciHandle a1, OciHandle a2, uint a3, IntPtr a4, int a5, int a6, OCI.DATATYPE a7, IntPtr a8, IntPtr a9, IntPtr a10, int a11, IntPtr a12, int a13)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, OciHandle.HandleValueToTrace(a1), OciHandle.HandleValueToTrace(a2), a3, a4, a5, a6, a7.ToString(), a8, a9, a10, a11, a12, a13);
        }
    }

    internal static void Trace(string fmtPrintfW, IntPtr a1, IntPtr a2, string a3, int a4, IntPtr a5, int a6, int a7, IntPtr a8, int a9, IntPtr a10, int a11, IntPtr a12, uint a13, IntPtr a14, int a15)
    {
        if (((modFlags & ApiGroup.Trace) != ApiGroup.Off) && (modID != NoData))
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, UIntPtr.Zero, fmtPrintfW, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
        }
    }

    internal static void TraceBin(string constStrHeader, byte[] buff, ushort length)
    {
        if (modID != NoData)
        {
            if ((constStrHeader != null) && (constStrHeader.Length > 0))
            {
                NativeMethods.PutStr(modID, UIntPtr.Zero, (UIntPtr) 1, constStrHeader);
            }
            if (((ushort) buff.Length) < length)
            {
                length = (ushort) buff.Length;
            }
            NativeMethods.TraceBin(modID, UIntPtr.Zero, (UIntPtr) 0x10, "<Trace|BLOB> %p %u\n", buff, length);
        }
    }

    internal static void TraceBinEx(byte[] buff, ushort length)
    {
        if (modID != NoData)
        {
            if (((ushort) buff.Length) < length)
            {
                length = (ushort) buff.Length;
            }
            NativeMethods.TraceBin(modID, UIntPtr.Zero, (UIntPtr) 0x10, "<Trace|BLOB> %p %u\n", buff, length);
        }
    }

    internal static void TraceEx(uint flags, string strConst)
    {
        if (modID != NoData)
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, (UIntPtr) flags, strConst);
        }
    }

    internal static void TraceEx(uint flags, string fmtPrintfW, string a1)
    {
        if (modID != NoData)
        {
            NativeMethods.Trace(modID, UIntPtr.Zero, (UIntPtr) flags, fmtPrintfW, a1);
        }
    }

    internal static bool AdvancedOn
    {
        get
        {
            return ((modFlags & ApiGroup.Advanced) != ApiGroup.Off);
        }
    }

    internal static IntPtr DefaultCmdSpace
    {
        get
        {
            return __defaultCmdSpace;
        }
    }

    internal static IntPtr ID
    {
        get
        {
            return modID;
        }
    }

    internal static bool IsInitialized
    {
        get
        {
            return (modID != NoData);
        }
    }

    internal static IntPtr NoData
    {
        get
        {
            return __noData;
        }
    }

    internal static bool ScopeOn
    {
        get
        {
            return ((modFlags & ApiGroup.Scope) != ApiGroup.Off);
        }
    }

    internal static bool TraceOn
    {
        get
        {
            return ((modFlags & ApiGroup.Trace) != ApiGroup.Off);
        }
    }

    internal enum ApiGroup : uint
    {
        Advanced = 0x80,
        Default = 1,
        Dependency = 0x2000,
        MaskAll = 0xffffffff,
        MaskBid = 0xfff,
        MaskUser = 0xfffff000,
        Memory = 0x20,
        Off = 0,
        Perf = 8,
        Pooling = 0x1000,
        Resource = 0x10,
        Scope = 4,
        StateDump = 0x4000,
        StatusOk = 0x40,
        Trace = 2
    }

    private sealed class AutoInit : SafeHandle
    {
        private bool _bInitialized;

        internal AutoInit() : base(IntPtr.Zero, true)
        {
            Bid.initEntryPoint();
            this._bInitialized = true;
        }

        protected override bool ReleaseHandle()
        {
            this._bInitialized = false;
            Bid.doneEntryPoint();
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                return !this._bInitialized;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BIDEXTINFO
    {
        private IntPtr hModule;
        [MarshalAs(UnmanagedType.LPWStr)]
        private string DomainName;
        private int Reserved2;
        private int Reserved;
        [MarshalAs(UnmanagedType.LPWStr)]
        private string ModulePath;
        private IntPtr ModulePathA;
        private IntPtr pBindCookie;
        internal BIDEXTINFO(IntPtr hMod, string modPath, string friendlyName, IntPtr cookiePtr)
        {
            this.hModule = hMod;
            this.DomainName = friendlyName;
            this.Reserved2 = 0;
            this.Reserved = 0;
            this.ModulePath = modPath;
            this.ModulePathA = IntPtr.Zero;
            this.pBindCookie = cookiePtr;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private class BindingCookie
    {
        internal IntPtr _data = ((IntPtr) (-1));
        internal BindingCookie()
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Invalidate()
        {
            this._data = (IntPtr) (-1);
        }
    }

    private enum CtlCmd : uint
    {
        AddExtension = 0x4000001e,
        AddMetaText = 0x40000022,
        AddResHandle = 0x40000026,
        CmdSpaceCount = 0x40000000,
        CmdSpaceEnum = 0x40000004,
        CmdSpaceQuery = 0x40000008,
        CplBase = 0x60000000,
        CplMax = 0x7ffffffc,
        DcsBase = 0x40000000,
        DcsMax = 0x5ffffffc,
        GetEventID = 0x40000016,
        LastItem = 0x4000002b,
        ParseString = 0x4000001a,
        Reverse = 1,
        Shutdown = 0x4000002a,
        Unicode = 2
    }

    private delegate Bid.ApiGroup CtrlCB(Bid.ApiGroup mask, Bid.ApiGroup bits);

    [ComVisible(false), SuppressUnmanagedCodeSecurity]
    private static class NativeMethods
    {
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidCtlProc", CharSet=CharSet.Unicode)]
        internal static extern void AddMetaText(IntPtr hID, IntPtr cmdSpace, Bid.CtlCmd cmd, IntPtr nop1, string txtID, IntPtr nop2);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll")]
        internal static extern void DllBidEntryPoint(ref IntPtr hID, int bInitAndVer, IntPtr unused1, uint propBits, ref Bid.ApiGroup pGblFlags, IntPtr unused2, IntPtr unused3, IntPtr unused4, IntPtr unused5);
        [DllImport("System.Data.OracleClient.dll", CharSet=CharSet.Ansi)]
        internal static extern void DllBidEntryPoint(ref IntPtr hID, int bInitAndVer, string sIdentity, uint propBits, ref Bid.ApiGroup pGblFlags, Bid.CtrlCB fAddr, ref Bid.BIDEXTINFO pExtInfo, IntPtr pHooks, IntPtr pHdr);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll")]
        internal static extern void DllBidFinalize();
        [DllImport("System.Data.OracleClient.dll")]
        internal static extern void DllBidInitialize();
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidPutStrW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern void PutStr(IntPtr hID, UIntPtr src, UIntPtr info, string str);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidScopeEnterCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void ScopeEnter(IntPtr hID, UIntPtr src, UIntPtr info, out IntPtr hScp, string strConst);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidScopeEnterCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void ScopeEnter(IntPtr hID, UIntPtr src, UIntPtr info, out IntPtr hScp, string fmtPrintfW, int a1);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidScopeEnterCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void ScopeEnter(IntPtr hID, UIntPtr src, UIntPtr info, out IntPtr hScp, string fmtPrintfW, int a1, int a2);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidScopeEnterCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void ScopeEnter(IntPtr hID, UIntPtr src, UIntPtr info, out IntPtr hScp, string fmtPrintfW, int a1, string a2);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidScopeLeave")]
        internal static extern void ScopeLeave(IntPtr hID, UIntPtr src, UIntPtr info, ref IntPtr hScp);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string strConst);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, string a1);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, int a2);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, string a2);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, int a2);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, string a1, int a2);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, uint a1, int a2);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, int a2, int a3);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, int a2, string a3);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, IntPtr a2, int a3);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, string a2, int a3);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, int a3);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, int a3, IntPtr a4);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, uint a4);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, string a2, int a3, string a4, int a5);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, int a3, int a4, int a5);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, int a5);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, string a4, int a5);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, uint a4, uint a5);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, string a3, int a4, int a5);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, int a2, IntPtr a3, IntPtr a4, int a5, int a6);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, int a2, IntPtr a3, IntPtr a4, IntPtr a5, int a6);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, int a3, int a4, int a5, string a6);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, uint a3, uint a4, uint a5, uint a6);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, int a3, uint a4, string a5, IntPtr a6);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, IntPtr a3, int a4, IntPtr a5, int a6);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, IntPtr a3, uint a4, string a5, IntPtr a6);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, string a3, uint a4, string a5, IntPtr a6);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, uint a5, uint a6, uint a7);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, string a4, int a5, string a6, int a7);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, string a3, IntPtr a4, int a5, uint a6, int a7);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, string a3, IntPtr a4, IntPtr a5, uint a6, int a7);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, string a2, string a3, IntPtr a4, string a5, uint a6, int a7);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, int a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, int a5, int a6, int a7, int a8);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, int a5, IntPtr a6, IntPtr a7, int a8);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, int a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, int a8, int a9);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, uint a5, IntPtr a6, int a7, IntPtr a8, IntPtr a9, int a10, int a11);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, IntPtr a3, int a4, uint a5, IntPtr a6, int a7, int a8, IntPtr a9, IntPtr a10, int a11, int a12);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, uint a3, IntPtr a4, int a5, int a6, string a7, IntPtr a8, IntPtr a9, IntPtr a10, int a11, IntPtr a12, int a13);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void Trace(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, IntPtr a1, IntPtr a2, string a3, int a4, IntPtr a5, int a6, int a7, IntPtr a8, int a9, IntPtr a10, int a11, IntPtr a12, uint a13, IntPtr a14, int a15);
        [DllImport("System.Data.OracleClient.dll", EntryPoint="DllBidTraceCW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        internal static extern void TraceBin(IntPtr hID, UIntPtr src, UIntPtr info, string fmtPrintfW, byte[] buff, uint len);
    }
}

