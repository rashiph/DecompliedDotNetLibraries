namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    [StandardModule]
    public sealed class Interaction
    {
        private static string m_CommandLine;
        private static object m_EnvironSyncObject = new object();
        private static SortedList m_SortedEnvList;

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void AppActivate(int ProcessId)
        {
            int num;
            IntPtr window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(Microsoft.VisualBasic.CompilerServices.NativeMethods.GetDesktopWindow(), 5);
            while (window != IntPtr.Zero)
            {
                Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.GetWindowThreadProcessId(window, ref num);
                if (((num == ProcessId) && Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.IsWindowEnabled(window)) && Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.IsWindowVisible(window))
                {
                    break;
                }
                window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(window, 2);
            }
            if (window == IntPtr.Zero)
            {
                window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(Microsoft.VisualBasic.CompilerServices.NativeMethods.GetDesktopWindow(), 5);
                while (window != IntPtr.Zero)
                {
                    Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.GetWindowThreadProcessId(window, ref num);
                    if (num == ProcessId)
                    {
                        break;
                    }
                    window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(window, 2);
                }
            }
            if (window == IntPtr.Zero)
            {
                throw new ArgumentException(Utils.GetResourceString("ProcessNotFound", new string[] { Conversions.ToString(ProcessId) }));
            }
            AppActivateHelper(window);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void AppActivate(string Title)
        {
            string lpClassName = null;
            IntPtr hWnd = Microsoft.VisualBasic.CompilerServices.NativeMethods.FindWindow(ref lpClassName, ref Title);
            if (hWnd == IntPtr.Zero)
            {
                int num;
                string strA = string.Empty;
                StringBuilder lpString = new StringBuilder(0x1ff);
                int length = Strings.Len(Title);
                hWnd = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(Microsoft.VisualBasic.CompilerServices.NativeMethods.GetDesktopWindow(), 5);
                while (hWnd != IntPtr.Zero)
                {
                    num = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindowText(hWnd, lpString, lpString.Capacity);
                    strA = lpString.ToString();
                    if ((num >= length) && (string.Compare(strA, 0, Title, 0, length, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        break;
                    }
                    hWnd = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(hWnd, 2);
                }
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(Microsoft.VisualBasic.CompilerServices.NativeMethods.GetDesktopWindow(), 5);
                    while (hWnd != IntPtr.Zero)
                    {
                        num = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindowText(hWnd, lpString, lpString.Capacity);
                        strA = lpString.ToString();
                        if ((num >= length) && (string.Compare(Strings.Right(strA, length), 0, Title, 0, length, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            break;
                        }
                        hWnd = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(hWnd, 2);
                    }
                }
            }
            if (hWnd == IntPtr.Zero)
            {
                throw new ArgumentException(Utils.GetResourceString("ProcessNotFound", new string[] { Title }));
            }
            AppActivateHelper(hWnd);
        }

        [SecuritySafeCritical]
        private static void AppActivateHelper(IntPtr hwndApp)
        {
            int num;
            try
            {
                new UIPermission(UIPermissionWindow.AllWindows).Demand();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            if (!Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.IsWindowEnabled(hwndApp) || !Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.IsWindowVisible(hwndApp))
            {
                IntPtr window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(hwndApp, 0);
                while (window != IntPtr.Zero)
                {
                    if (Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(window, 4) == hwndApp)
                    {
                        if (Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.IsWindowEnabled(window) && Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.IsWindowVisible(window))
                        {
                            break;
                        }
                        hwndApp = window;
                        window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(hwndApp, 0);
                    }
                    window = Microsoft.VisualBasic.CompilerServices.NativeMethods.GetWindow(window, 2);
                }
                if (window == IntPtr.Zero)
                {
                    throw new ArgumentException(Utils.GetResourceString("ProcessNotFound"));
                }
                hwndApp = window;
            }
            Microsoft.VisualBasic.CompilerServices.NativeMethods.AttachThreadInput(0, Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.GetWindowThreadProcessId(hwndApp, ref num), 1);
            Microsoft.VisualBasic.CompilerServices.NativeMethods.SetForegroundWindow(hwndApp);
            Microsoft.VisualBasic.CompilerServices.NativeMethods.SetFocus(hwndApp);
            Microsoft.VisualBasic.CompilerServices.NativeMethods.AttachThreadInput(0, Microsoft.VisualBasic.CompilerServices.SafeNativeMethods.GetWindowThreadProcessId(hwndApp, ref num), 0);
        }

        [SecuritySafeCritical]
        public static void Beep()
        {
            try
            {
                new UIPermission(UIPermissionWindow.SafeSubWindows).Demand();
            }
            catch (SecurityException)
            {
                try
                {
                    new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                }
                catch (SecurityException)
                {
                    return;
                }
            }
            Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.MessageBeep(0);
        }

        public static object CallByName(object ObjectRef, string ProcName, CallType UseCallType, params object[] Args)
        {
            switch (UseCallType)
            {
                case CallType.Method:
                    return LateBinding.InternalLateCall(ObjectRef, null, ProcName, Args, null, null, false);

                case CallType.Get:
                    return LateBinding.LateGet(ObjectRef, null, ProcName, Args, null, null);

                case CallType.Let:
                case CallType.Set:
                {
                    System.Type objType = null;
                    LateBinding.InternalLateSet(ObjectRef, ref objType, ProcName, Args, null, false, UseCallType);
                    return null;
                }
            }
            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "CallType" }));
        }

        private static void CheckPathComponent(string s)
        {
            if ((s == null) || (s.Length == 0))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty"));
            }
        }

        public static object Choose(double Index, params object[] Choice)
        {
            int index = (int) Math.Round((double) (Conversion.Fix(Index) - 1.0));
            if (Choice.Rank != 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", new string[] { "Choice" }));
            }
            if ((index >= 0) && (index <= Choice.GetUpperBound(0)))
            {
                return Choice[index];
            }
            return null;
        }

        [SecuritySafeCritical]
        public static string Command()
        {
            new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Demand();
            if (m_CommandLine == null)
            {
                int index;
                string commandLine = Environment.CommandLine;
                if ((commandLine == null) || (commandLine.Length == 0))
                {
                    return "";
                }
                int length = Environment.GetCommandLineArgs()[0].Length;
                do
                {
                    index = commandLine.IndexOf('"', index);
                    if ((index >= 0) && (index <= length))
                    {
                        commandLine = commandLine.Remove(index, 1);
                    }
                }
                while ((index >= 0) && (index <= length));
                if ((index == 0) || (index > commandLine.Length))
                {
                    m_CommandLine = "";
                }
                else
                {
                    m_CommandLine = Strings.LTrim(commandLine.Substring(length));
                }
            }
            return m_CommandLine;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public static object CreateObject(string ProgId, string ServerName = "")
        {
            object obj2;
            if (ProgId.Length == 0)
            {
                throw ExceptionUtils.VbMakeException(0x1ad);
            }
            if ((ServerName == null) || (ServerName.Length == 0))
            {
                ServerName = null;
            }
            else if (string.Compare(Environment.MachineName, ServerName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                ServerName = null;
            }
            try
            {
                System.Type typeFromProgID;
                if (ServerName == null)
                {
                    typeFromProgID = System.Type.GetTypeFromProgID(ProgId);
                }
                else
                {
                    typeFromProgID = System.Type.GetTypeFromProgID(ProgId, ServerName, true);
                }
                obj2 = Activator.CreateInstance(typeFromProgID);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147023174)
                {
                    throw ExceptionUtils.VbMakeException(0x1ce);
                }
                throw ExceptionUtils.VbMakeException(0x1ad);
            }
            catch (StackOverflowException exception2)
            {
                throw exception2;
            }
            catch (OutOfMemoryException exception3)
            {
                throw exception3;
            }
            catch (ThreadAbortException exception4)
            {
                throw exception4;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(0x1ad);
            }
            return obj2;
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public static void DeleteSetting(string AppName, string Section = null, string Key = null)
        {
            RegistryKey key = null;
            CheckPathComponent(AppName);
            string subkey = FormRegKey(AppName, Section);
            try
            {
                RegistryKey currentUser = Registry.CurrentUser;
                if (Information.IsNothing(Key) || (Key.Length == 0))
                {
                    currentUser.DeleteSubKeyTree(subkey);
                }
                else
                {
                    key = currentUser.OpenSubKey(subkey, true);
                    if (key == null)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Section" }));
                    }
                    key.DeleteValue(Key);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
        }

        [SecuritySafeCritical]
        public static string Environ(int Expression)
        {
            if ((Expression <= 0) || (Expression > 0xff))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_Range1toFF1", new string[] { "Expression" }));
            }
            if (m_SortedEnvList == null)
            {
                object environSyncObject = m_EnvironSyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(environSyncObject);
                lock (environSyncObject)
                {
                    if (m_SortedEnvList == null)
                    {
                        new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                        m_SortedEnvList = new SortedList(Environment.GetEnvironmentVariables());
                        PermissionSet.RevertAssert();
                    }
                }
            }
            if (Expression > m_SortedEnvList.Count)
            {
                return "";
            }
            string pathList = m_SortedEnvList.GetKey(Expression - 1).ToString();
            string str3 = m_SortedEnvList.GetByIndex(Expression - 1).ToString();
            new EnvironmentPermission(EnvironmentPermissionAccess.Read, pathList).Demand();
            return (pathList + "=" + str3);
        }

        public static string Environ(string Expression)
        {
            Expression = Strings.Trim(Expression);
            if (Expression.Length == 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Expression" }));
            }
            return Environment.GetEnvironmentVariable(Expression);
        }

        private static string FormRegKey(string sApp, string sSect)
        {
            if (Information.IsNothing(sApp) || (sApp.Length == 0))
            {
                return @"Software\VB and VBA Program Settings";
            }
            if (Information.IsNothing(sSect) || (sSect.Length == 0))
            {
                return (@"Software\VB and VBA Program Settings\" + sApp);
            }
            return (@"Software\VB and VBA Program Settings\" + sApp + @"\" + sSect);
        }

        public static string[,] GetAllSettings(string AppName, string Section)
        {
            CheckPathComponent(AppName);
            CheckPathComponent(Section);
            string name = FormRegKey(AppName, Section);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(name);
            if (key == null)
            {
                return null;
            }
            string[,] strArray = null;
            try
            {
                if (key.ValueCount == 0)
                {
                    return strArray;
                }
                string[] valueNames = key.GetValueNames();
                int upperBound = valueNames.GetUpperBound(0);
                string[,] strArray3 = new string[upperBound + 1, 2];
                int num3 = upperBound;
                for (int i = 0; i <= num3; i++)
                {
                    string str2 = valueNames[i];
                    strArray3[i, 0] = str2;
                    object obj2 = key.GetValue(str2);
                    if ((obj2 != null) && (obj2 is string))
                    {
                        strArray3[i, 1] = obj2.ToString();
                    }
                }
                strArray = strArray3;
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
            }
            finally
            {
                key.Close();
            }
            return strArray;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public static object GetObject(string PathName = null, string Class = null)
        {
            IPersistFile activeObject;
            if (Strings.Len(Class) == 0)
            {
                try
                {
                    return Marshal.BindToMoniker(PathName);
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    throw ExceptionUtils.VbMakeException(0x1ad);
                }
            }
            if (PathName == null)
            {
                try
                {
                    return Marshal.GetActiveObject(Class);
                }
                catch (StackOverflowException exception4)
                {
                    throw exception4;
                }
                catch (OutOfMemoryException exception5)
                {
                    throw exception5;
                }
                catch (ThreadAbortException exception6)
                {
                    throw exception6;
                }
                catch (Exception)
                {
                    throw ExceptionUtils.VbMakeException(0x1ad);
                }
            }
            if (Strings.Len(PathName) == 0)
            {
                try
                {
                    return Activator.CreateInstance(System.Type.GetTypeFromProgID(Class));
                }
                catch (StackOverflowException exception7)
                {
                    throw exception7;
                }
                catch (OutOfMemoryException exception8)
                {
                    throw exception8;
                }
                catch (ThreadAbortException exception9)
                {
                    throw exception9;
                }
                catch (Exception)
                {
                    throw ExceptionUtils.VbMakeException(0x1ad);
                }
            }
            try
            {
                activeObject = (IPersistFile) Marshal.GetActiveObject(Class);
            }
            catch (StackOverflowException exception10)
            {
                throw exception10;
            }
            catch (OutOfMemoryException exception11)
            {
                throw exception11;
            }
            catch (ThreadAbortException exception12)
            {
                throw exception12;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(0x1b0);
            }
            try
            {
                activeObject.Load(PathName, 0);
            }
            catch (StackOverflowException exception13)
            {
                throw exception13;
            }
            catch (OutOfMemoryException exception14)
            {
                throw exception14;
            }
            catch (ThreadAbortException exception15)
            {
                throw exception15;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(0x1ad);
            }
            return activeObject;
        }

        public static string GetSetting(string AppName, string Section, string Key, string Default = "")
        {
            object obj2;
            RegistryKey key = null;
            CheckPathComponent(AppName);
            CheckPathComponent(Section);
            CheckPathComponent(Key);
            if (Default == null)
            {
                Default = "";
            }
            string name = FormRegKey(AppName, Section);
            try
            {
                key = Registry.CurrentUser.OpenSubKey(name);
                if (key == null)
                {
                    return Default;
                }
                obj2 = key.GetValue(Key, Default);
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
            if (obj2 == null)
            {
                return null;
            }
            if (!(obj2 is string))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
            }
            return (string) obj2;
        }

        private static string GetTitleFromAssembly(Assembly CallingAssembly)
        {
            try
            {
                return CallingAssembly.GetName().Name;
            }
            catch (SecurityException)
            {
                string fullName = CallingAssembly.FullName;
                int index = fullName.IndexOf(',');
                if (index >= 0)
                {
                    return fullName.Substring(0, index);
                }
                return "";
            }
        }

        public static object IIf(bool Expression, object TruePart, object FalsePart)
        {
            if (Expression)
            {
                return TruePart;
            }
            return FalsePart;
        }

        internal static T IIf<T>(bool Condition, T TruePart, T FalsePart)
        {
            if (Condition)
            {
                return TruePart;
            }
            return FalsePart;
        }

        [MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.UI)]
        public static string InputBox(string Prompt, string Title = "", string DefaultResponse = "", int XPos = -1, int YPos = -1)
        {
            IWin32Window parentWindow = null;
            IVbHost vBHost = HostServices.VBHost;
            if (vBHost != null)
            {
                parentWindow = vBHost.GetParentWindow();
            }
            if (Title.Length == 0)
            {
                if (vBHost == null)
                {
                    Title = GetTitleFromAssembly(Assembly.GetCallingAssembly());
                }
                else
                {
                    Title = vBHost.GetWindowTitle();
                }
            }
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                return InternalInputBox(Prompt, Title, DefaultResponse, XPos, YPos, parentWindow);
            }
            InputBoxHandler handler = new InputBoxHandler(Prompt, Title, DefaultResponse, XPos, YPos, parentWindow);
            Thread thread = new Thread(new ThreadStart(handler.StartHere));
            thread.Start();
            thread.Join();
            if (handler.Exception != null)
            {
                throw handler.Exception;
            }
            return handler.Result;
        }

        private static void InsertNumber(ref string Buffer, long Num, long Spaces)
        {
            string expression = Conversions.ToString(Num);
            InsertSpaces(ref Buffer, Spaces - Strings.Len(expression));
            Buffer = Buffer + expression;
        }

        private static void InsertSpaces(ref string Buffer, long Spaces)
        {
            while (Spaces > 0L)
            {
                Buffer = Buffer + " ";
                Spaces -= 1L;
            }
        }

        private static string InternalInputBox(string Prompt, string Title, string DefaultResponse, int XPos, int YPos, IWin32Window ParentWindow)
        {
            VBInputBox box = new VBInputBox(Prompt, Title, DefaultResponse, XPos, YPos);
            box.ShowDialog(ParentWindow);
            string output = box.Output;
            box.Dispose();
            return output;
        }

        [MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.UI)]
        public static MsgBoxResult MsgBox(object Prompt, MsgBoxStyle Buttons = 0, object Title = null)
        {
            IWin32Window owner = null;
            string text = null;
            string titleFromAssembly;
            IVbHost vBHost = HostServices.VBHost;
            if (vBHost != null)
            {
                owner = vBHost.GetParentWindow();
            }
            if ((((Buttons & 15) > MsgBoxStyle.RetryCancel) || ((Buttons & 240) > MsgBoxStyle.Information)) || ((Buttons & 0xf00) > MsgBoxStyle.DefaultButton3))
            {
                Buttons = MsgBoxStyle.ApplicationModal;
            }
            try
            {
                if (Prompt != null)
                {
                    text = (string) Conversions.ChangeType(Prompt, typeof(string));
                }
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", new string[] { "Prompt", "String" }));
            }
            try
            {
                if (Title == null)
                {
                    if (vBHost == null)
                    {
                        titleFromAssembly = GetTitleFromAssembly(Assembly.GetCallingAssembly());
                    }
                    else
                    {
                        titleFromAssembly = vBHost.GetWindowTitle();
                    }
                }
                else
                {
                    titleFromAssembly = Conversions.ToString(Title);
                }
            }
            catch (StackOverflowException exception4)
            {
                throw exception4;
            }
            catch (OutOfMemoryException exception5)
            {
                throw exception5;
            }
            catch (ThreadAbortException exception6)
            {
                throw exception6;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", new string[] { "Title", "String" }));
            }
            return (MsgBoxResult) MessageBox.Show(owner, text, titleFromAssembly, ((MessageBoxButtons) Buttons) & ((MessageBoxButtons) 15), ((MessageBoxIcon) Buttons) & ((MessageBoxIcon) 240), ((MessageBoxDefaultButton) Buttons) & ((MessageBoxDefaultButton) 0xf00), ((MessageBoxOptions) Buttons) & ((MessageBoxOptions) (-4096)));
        }

        public static string Partition(long Number, long Start, long Stop, long Interval)
        {
            string buffer = null;
            long num;
            bool flag;
            bool flag2;
            long num2;
            long num3;
            if (Start < 0L)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Start" }));
            }
            if (Stop <= Start)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Stop" }));
            }
            if (Interval < 1L)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Interval" }));
            }
            if (Number < Start)
            {
                num3 = Start - 1L;
                flag = true;
            }
            else if (Number > Stop)
            {
                num = Stop + 1L;
                flag2 = true;
            }
            else if (Interval == 1L)
            {
                num = Number;
                num3 = Number;
            }
            else
            {
                num = (((Number - Start) / Interval) * Interval) + Start;
                num3 = (num + Interval) - 1L;
                if (num3 > Stop)
                {
                    num3 = Stop;
                }
                if (num < Start)
                {
                    num = Start;
                }
            }
            string expression = Conversions.ToString((long) (Stop + 1L));
            string str3 = Conversions.ToString((long) (Start - 1L));
            if (Strings.Len(expression) > Strings.Len(str3))
            {
                num2 = Strings.Len(expression);
            }
            else
            {
                num2 = Strings.Len(str3);
            }
            if (flag)
            {
                expression = Conversions.ToString(num3);
                if (num2 < Strings.Len(expression))
                {
                    num2 = Strings.Len(expression);
                }
            }
            if (flag)
            {
                InsertSpaces(ref buffer, num2);
            }
            else
            {
                InsertNumber(ref buffer, num, num2);
            }
            buffer = buffer + ":";
            if (flag2)
            {
                InsertSpaces(ref buffer, num2);
                return buffer;
            }
            InsertNumber(ref buffer, num3, num2);
            return buffer;
        }

        public static void SaveSetting(string AppName, string Section, string Key, string Setting)
        {
            CheckPathComponent(AppName);
            CheckPathComponent(Section);
            CheckPathComponent(Key);
            string subkey = FormRegKey(AppName, Section);
            RegistryKey key = Registry.CurrentUser.CreateSubKey(subkey);
            if (key == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Interaction_ResKeyNotCreated1", new string[] { subkey }));
            }
            try
            {
                key.SetValue(Key, Setting);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                key.Close();
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static int Shell(string PathName, AppWinStyle Style = 2, bool Wait = false, int Timeout = -1)
        {
            int num3;
            NativeTypes.STARTUPINFO lpStartupInfo = new NativeTypes.STARTUPINFO();
            NativeTypes.PROCESS_INFORMATION lpProcessInformation = new NativeTypes.PROCESS_INFORMATION();
            NativeTypes.LateInitSafeHandleZeroOrMinusOneIsInvalid hHandle = new NativeTypes.LateInitSafeHandleZeroOrMinusOneIsInvalid();
            NativeTypes.LateInitSafeHandleZeroOrMinusOneIsInvalid invalid2 = new NativeTypes.LateInitSafeHandleZeroOrMinusOneIsInvalid();
            int num = 0;
            try
            {
                new UIPermission(UIPermissionWindow.AllWindows).Demand();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            if (PathName == null)
            {
                throw new NullReferenceException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Pathname" }));
            }
            if ((Style < AppWinStyle.Hide) || (Style > ((AppWinStyle) 9)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Style" }));
            }
            Microsoft.VisualBasic.CompilerServices.NativeMethods.GetStartupInfo(lpStartupInfo);
            try
            {
                int num2;
                lpStartupInfo.dwFlags = 1;
                lpStartupInfo.wShowWindow = (short) Style;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    IntPtr ptr;
                    num2 = Microsoft.VisualBasic.CompilerServices.NativeMethods.CreateProcess(null, PathName, null, null, false, 0x20, ptr, null, lpStartupInfo, lpProcessInformation);
                    if (num2 == 0)
                    {
                        num = Marshal.GetLastWin32Error();
                    }
                    if ((lpProcessInformation.hProcess != IntPtr.Zero) && (lpProcessInformation.hProcess != NativeTypes.INVALID_HANDLE))
                    {
                        hHandle.InitialSetHandle(lpProcessInformation.hProcess);
                    }
                    if ((lpProcessInformation.hThread != IntPtr.Zero) && (lpProcessInformation.hThread != NativeTypes.INVALID_HANDLE))
                    {
                        invalid2.InitialSetHandle(lpProcessInformation.hThread);
                    }
                }
                try
                {
                    if (num2 != 0)
                    {
                        if (Wait)
                        {
                            if (Microsoft.VisualBasic.CompilerServices.NativeMethods.WaitForSingleObject(hHandle, Timeout) == 0)
                            {
                                return 0;
                            }
                            return lpProcessInformation.dwProcessId;
                        }
                        Microsoft.VisualBasic.CompilerServices.NativeMethods.WaitForInputIdle(hHandle, 0x2710);
                        return lpProcessInformation.dwProcessId;
                    }
                    if (num == 5)
                    {
                        throw ExceptionUtils.VbMakeException(70);
                    }
                    throw ExceptionUtils.VbMakeException(0x35);
                }
                finally
                {
                    hHandle.Close();
                    invalid2.Close();
                }
            }
            finally
            {
                lpStartupInfo.Dispose();
            }
            return num3;
        }

        public static object Switch(params object[] VarExpr)
        {
            if (VarExpr != null)
            {
                int length = VarExpr.Length;
                int index = 0;
                if ((length % 2) != 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "VarExpr" }));
                }
                while (length > 0)
                {
                    if (Conversions.ToBoolean(VarExpr[index]))
                    {
                        return VarExpr[index + 1];
                    }
                    index += 2;
                    length -= 2;
                }
            }
            return null;
        }

        private sealed class InputBoxHandler
        {
            private string m_DefaultResponse;
            private System.Exception m_Exception;
            private IWin32Window m_ParentWindow;
            private string m_Prompt;
            private string m_Result;
            private string m_Title;
            private int m_XPos;
            private int m_YPos;

            public InputBoxHandler(string Prompt, string Title, string DefaultResponse, int XPos, int YPos, IWin32Window ParentWindow)
            {
                this.m_Prompt = Prompt;
                this.m_Title = Title;
                this.m_DefaultResponse = DefaultResponse;
                this.m_XPos = XPos;
                this.m_YPos = YPos;
                this.m_ParentWindow = ParentWindow;
            }

            [STAThread]
            public void StartHere()
            {
                try
                {
                    this.m_Result = Interaction.InternalInputBox(this.m_Prompt, this.m_Title, this.m_DefaultResponse, this.m_XPos, this.m_YPos, this.m_ParentWindow);
                }
                catch (System.Exception exception)
                {
                    this.m_Exception = exception;
                }
            }

            internal System.Exception Exception
            {
                get
                {
                    return this.m_Exception;
                }
            }

            public string Result
            {
                get
                {
                    return this.m_Result;
                }
            }
        }

        [Guid("0000010B-0000-0000-C000-000000000046"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            void GetClassID(ref Guid pClassID);
            void IsDirty();
            void Load(string pszFileName, int dwMode);
            void Save(string pszFileName, int fRemember);
            void SaveCompleted(string pszFileName);
            string GetCurFile();
        }
    }
}

