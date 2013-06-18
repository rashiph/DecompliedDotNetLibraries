namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public class SendKeys
    {
        private const int ALTKEYSCAN = 0x400;
        private static bool capslockChanged;
        private const int CTRLKEYSCAN = 0x200;
        private static Queue events;
        private static bool fStartNewChar;
        private const int HAVEALT = 2;
        private const int HAVECTRL = 1;
        private const int HAVESHIFT = 0;
        private static IntPtr hhook;
        private static System.Windows.Forms.NativeMethods.HookProc hook;
        private static bool? hookSupported = null;
        private static bool kanaChanged;
        private static KeywordVk[] keywords = new KeywordVk[] { 
            new KeywordVk("ENTER", 13), new KeywordVk("TAB", 9), new KeywordVk("ESC", 0x1b), new KeywordVk("ESCAPE", 0x1b), new KeywordVk("HOME", 0x24), new KeywordVk("END", 0x23), new KeywordVk("LEFT", 0x25), new KeywordVk("RIGHT", 0x27), new KeywordVk("UP", 0x26), new KeywordVk("DOWN", 40), new KeywordVk("PGUP", 0x21), new KeywordVk("PGDN", 0x22), new KeywordVk("NUMLOCK", 0x90), new KeywordVk("SCROLLLOCK", 0x91), new KeywordVk("PRTSC", 0x2c), new KeywordVk("BREAK", 3), 
            new KeywordVk("BACKSPACE", 8), new KeywordVk("BKSP", 8), new KeywordVk("BS", 8), new KeywordVk("CLEAR", 12), new KeywordVk("CAPSLOCK", 20), new KeywordVk("INS", 0x2d), new KeywordVk("INSERT", 0x2d), new KeywordVk("DEL", 0x2e), new KeywordVk("DELETE", 0x2e), new KeywordVk("HELP", 0x2f), new KeywordVk("F1", 0x70), new KeywordVk("F2", 0x71), new KeywordVk("F3", 0x72), new KeywordVk("F4", 0x73), new KeywordVk("F5", 0x74), new KeywordVk("F6", 0x75), 
            new KeywordVk("F7", 0x76), new KeywordVk("F8", 0x77), new KeywordVk("F9", 120), new KeywordVk("F10", 0x79), new KeywordVk("F11", 0x7a), new KeywordVk("F12", 0x7b), new KeywordVk("F13", 0x7c), new KeywordVk("F14", 0x7d), new KeywordVk("F15", 0x7e), new KeywordVk("F16", 0x7f), new KeywordVk("MULTIPLY", 0x6a), new KeywordVk("ADD", 0x6b), new KeywordVk("SUBTRACT", 0x6d), new KeywordVk("DIVIDE", 0x6f), new KeywordVk("+", 0x6b), new KeywordVk("%", 0x10035), 
            new KeywordVk("^", 0x10036)
         };
        private static SKWindow messageWindow;
        private static bool numlockChanged;
        private static bool scrollLockChanged;
        private static SendMethodTypes? sendMethod = null;
        private const int SHIFTKEYSCAN = 0x100;
        private static bool stopHook;
        private const int UNKNOWN_GROUPING = 10;

        static SendKeys()
        {
            Application.ThreadExit += new EventHandler(SendKeys.OnThreadExit);
            messageWindow = new SKWindow();
            messageWindow.CreateControl();
        }

        private SendKeys()
        {
        }

        private static void AddCancelModifiersForPreviousEvents(Queue previousEvents)
        {
            if (previousEvents != null)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                while (previousEvents.Count > 0)
                {
                    bool flag4;
                    SKEvent event2 = (SKEvent) previousEvents.Dequeue();
                    if ((event2.wm == 0x101) || (event2.wm == 0x105))
                    {
                        flag4 = false;
                    }
                    else
                    {
                        if ((event2.wm != 0x100) && (event2.wm != 260))
                        {
                            continue;
                        }
                        flag4 = true;
                    }
                    if (event2.paramL == 0x10)
                    {
                        flag = flag4;
                    }
                    else
                    {
                        if (event2.paramL == 0x11)
                        {
                            flag2 = flag4;
                            continue;
                        }
                        if (event2.paramL == 0x12)
                        {
                            flag3 = flag4;
                        }
                    }
                }
                if (flag)
                {
                    AddEvent(new SKEvent(0x101, 0x10, false, IntPtr.Zero));
                }
                else if (flag2)
                {
                    AddEvent(new SKEvent(0x101, 0x11, false, IntPtr.Zero));
                }
                else if (flag3)
                {
                    AddEvent(new SKEvent(0x105, 0x12, false, IntPtr.Zero));
                }
            }
        }

        private static void AddEvent(SKEvent skevent)
        {
            if (events == null)
            {
                events = new Queue();
            }
            events.Enqueue(skevent);
        }

        private static void AddMsgsForVK(int vk, int repeat, bool altnoctrldown, IntPtr hwnd)
        {
            for (int i = 0; i < repeat; i++)
            {
                AddEvent(new SKEvent(altnoctrldown ? 260 : 0x100, vk, fStartNewChar, hwnd));
                AddEvent(new SKEvent(altnoctrldown ? 0x105 : 0x101, vk, fStartNewChar, hwnd));
            }
        }

        private static bool AddSimpleKey(char character, int repeat, IntPtr hwnd, int[] haveKeys, bool fStartNewChar, int cGrp)
        {
            int num = UnsafeNativeMethods.VkKeyScan(character);
            if (num != -1)
            {
                if ((haveKeys[0] == 0) && ((num & 0x100) != 0))
                {
                    AddEvent(new SKEvent(0x100, 0x10, fStartNewChar, hwnd));
                    fStartNewChar = false;
                    haveKeys[0] = 10;
                }
                if ((haveKeys[1] == 0) && ((num & 0x200) != 0))
                {
                    AddEvent(new SKEvent(0x100, 0x11, fStartNewChar, hwnd));
                    fStartNewChar = false;
                    haveKeys[1] = 10;
                }
                if ((haveKeys[2] == 0) && ((num & 0x400) != 0))
                {
                    AddEvent(new SKEvent(0x100, 0x12, fStartNewChar, hwnd));
                    fStartNewChar = false;
                    haveKeys[2] = 10;
                }
                AddMsgsForVK(num & 0xff, repeat, (haveKeys[2] > 0) && (haveKeys[1] == 0), hwnd);
                CancelMods(haveKeys, 10, hwnd);
            }
            else
            {
                int num2 = SafeNativeMethods.OemKeyScan((short) ('\x00ff' & character));
                for (int i = 0; i < repeat; i++)
                {
                    AddEvent(new SKEvent(0x102, character, num2 & 0xffff, hwnd));
                }
            }
            if (cGrp != 0)
            {
                fStartNewChar = true;
            }
            return fStartNewChar;
        }

        private static void CancelMods(int[] haveKeys, int level, IntPtr hwnd)
        {
            if (haveKeys[0] == level)
            {
                AddEvent(new SKEvent(0x101, 0x10, false, hwnd));
                haveKeys[0] = 0;
            }
            if (haveKeys[1] == level)
            {
                AddEvent(new SKEvent(0x101, 0x11, false, hwnd));
                haveKeys[1] = 0;
            }
            if (haveKeys[2] == level)
            {
                AddEvent(new SKEvent(0x105, 0x12, false, hwnd));
                haveKeys[2] = 0;
            }
        }

        private static void CheckGlobalKeys(SKEvent skEvent)
        {
            if (skEvent.wm == 0x100)
            {
                switch (skEvent.paramL)
                {
                    case 20:
                        capslockChanged = !capslockChanged;
                        return;

                    case 0x15:
                        kanaChanged = !kanaChanged;
                        break;

                    case 0x90:
                        numlockChanged = !numlockChanged;
                        return;

                    case 0x91:
                        scrollLockChanged = !scrollLockChanged;
                        return;

                    default:
                        return;
                }
            }
        }

        private static void ClearGlobalKeys()
        {
            capslockChanged = false;
            numlockChanged = false;
            scrollLockChanged = false;
            kanaChanged = false;
        }

        private static void ClearKeyboardState()
        {
            byte[] keyboardState = GetKeyboardState();
            keyboardState[20] = 0;
            keyboardState[0x90] = 0;
            keyboardState[0x91] = 0;
            SetKeyboardState(keyboardState);
        }

        private static IntPtr EmptyHookCallback(int code, IntPtr wparam, IntPtr lparam)
        {
            return IntPtr.Zero;
        }

        public static void Flush()
        {
            Application.DoEvents();
            while ((events != null) && (events.Count > 0))
            {
                Application.DoEvents();
            }
        }

        private static byte[] GetKeyboardState()
        {
            byte[] keystate = new byte[0x100];
            UnsafeNativeMethods.GetKeyboardState(keystate);
            return keystate;
        }

        private static void InstallHook()
        {
            if (hhook == IntPtr.Zero)
            {
                SendKeysHookProc proc1 = new SendKeysHookProc();
                hook = new System.Windows.Forms.NativeMethods.HookProc(proc1.Callback);
                stopHook = false;
                hhook = UnsafeNativeMethods.SetWindowsHookEx(1, hook, new HandleRef(null, UnsafeNativeMethods.GetModuleHandle(null)), 0);
                if (hhook == IntPtr.Zero)
                {
                    throw new SecurityException(System.Windows.Forms.SR.GetString("SendKeysHookFailed"));
                }
            }
        }

        private static bool IsExtendedKey(SKEvent skEvent)
        {
            if (((((skEvent.paramL != 0x26) && (skEvent.paramL != 40)) && ((skEvent.paramL != 0x25) && (skEvent.paramL != 0x27))) && (((skEvent.paramL != 0x21) && (skEvent.paramL != 0x22)) && ((skEvent.paramL != 0x24) && (skEvent.paramL != 0x23)))) && (skEvent.paramL != 0x2d))
            {
                return (skEvent.paramL == 0x2e);
            }
            return true;
        }

        private static void JournalCancel()
        {
            if (hhook != IntPtr.Zero)
            {
                stopHook = false;
                if (events != null)
                {
                    events.Clear();
                }
                hhook = IntPtr.Zero;
            }
        }

        private static void LoadSendMethodFromConfig()
        {
            if (!sendMethod.HasValue)
            {
                sendMethod = 1;
                try
                {
                    string str = ConfigurationManager.AppSettings.Get("SendKeys");
                    if (str.Equals("JournalHook", StringComparison.OrdinalIgnoreCase))
                    {
                        sendMethod = 2;
                    }
                    else if (str.Equals("SendInput", StringComparison.OrdinalIgnoreCase))
                    {
                        sendMethod = 3;
                    }
                }
                catch
                {
                }
            }
        }

        private static int MatchKeyword(string keyword)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (string.Equals(keywords[i].keyword, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return keywords[i].vk;
                }
            }
            return -1;
        }

        private static void OnThreadExit(object sender, EventArgs e)
        {
            try
            {
                UninstallJournalingHook();
            }
            catch
            {
            }
        }

        private static void ParseKeys(string keys, IntPtr hwnd)
        {
            int num = 0;
            int[] haveKeys = new int[3];
            int cGrp = 0;
            fStartNewChar = true;
            int length = keys.Length;
            while (num < length)
            {
                int num6;
                int num7;
                int repeat = 1;
                char ch = keys[num];
                int vk = 0;
                switch (ch)
                {
                    case '%':
                        if (haveKeys[2] != 0)
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysString", new object[] { keys }));
                        }
                        goto Label_03C9;

                    case '(':
                        cGrp++;
                        if (cGrp > 3)
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("SendKeysNestingError"));
                        }
                        goto Label_0414;

                    case ')':
                        if (cGrp < 1)
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysString", new object[] { keys }));
                        }
                        goto Label_045A;

                    case '+':
                        if (haveKeys[0] != 0)
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysString", new object[] { keys }));
                        }
                        goto Label_0333;

                    case '^':
                        if (haveKeys[1] != 0)
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysString", new object[] { keys }));
                        }
                        AddEvent(new SKEvent(0x100, 0x11, fStartNewChar, hwnd));
                        fStartNewChar = false;
                        haveKeys[1] = 10;
                        goto Label_04AB;

                    case '{':
                        num6 = num + 1;
                        if (((num6 + 1) >= length) || (keys[num6] != '}'))
                        {
                            goto Label_00EB;
                        }
                        num7 = num6 + 1;
                        goto Label_00C7;

                    case '}':
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysString", new object[] { keys }));

                    case '~':
                        vk = 13;
                        AddMsgsForVK(vk, repeat, (haveKeys[2] > 0) && (haveKeys[1] == 0), hwnd);
                        goto Label_04AB;

                    default:
                        fStartNewChar = AddSimpleKey(keys[num], repeat, hwnd, haveKeys, fStartNewChar, cGrp);
                        goto Label_04AB;
                }
            Label_00C1:
                num7++;
            Label_00C7:
                if ((num7 < length) && (keys[num7] != '}'))
                {
                    goto Label_00C1;
                }
                if (num7 < length)
                {
                    num6++;
                }
            Label_00EB:
                while (((num6 < length) && (keys[num6] != '}')) && !char.IsWhiteSpace(keys[num6]))
                {
                    num6++;
                }
                if (num6 >= length)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("SendKeysKeywordDelimError"));
                }
                string keyword = keys.Substring(num + 1, num6 - (num + 1));
                if (char.IsWhiteSpace(keys[num6]))
                {
                    while ((num6 < length) && char.IsWhiteSpace(keys[num6]))
                    {
                        num6++;
                    }
                    if (num6 >= length)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("SendKeysKeywordDelimError"));
                    }
                    if (char.IsDigit(keys[num6]))
                    {
                        int startIndex = num6;
                        while ((num6 < length) && char.IsDigit(keys[num6]))
                        {
                            num6++;
                        }
                        repeat = int.Parse(keys.Substring(startIndex, num6 - startIndex), CultureInfo.InvariantCulture);
                    }
                }
                if (num6 >= length)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("SendKeysKeywordDelimError"));
                }
                if (keys[num6] != '}')
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysRepeat"));
                }
                vk = MatchKeyword(keyword);
                if (vk != -1)
                {
                    if ((haveKeys[0] == 0) && ((vk & 0x10000) != 0))
                    {
                        AddEvent(new SKEvent(0x100, 0x10, fStartNewChar, hwnd));
                        fStartNewChar = false;
                        haveKeys[0] = 10;
                    }
                    if ((haveKeys[1] == 0) && ((vk & 0x20000) != 0))
                    {
                        AddEvent(new SKEvent(0x100, 0x11, fStartNewChar, hwnd));
                        fStartNewChar = false;
                        haveKeys[1] = 10;
                    }
                    if ((haveKeys[2] == 0) && ((vk & 0x40000) != 0))
                    {
                        AddEvent(new SKEvent(0x100, 0x12, fStartNewChar, hwnd));
                        fStartNewChar = false;
                        haveKeys[2] = 10;
                    }
                    AddMsgsForVK(vk, repeat, (haveKeys[2] > 0) && (haveKeys[1] == 0), hwnd);
                    CancelMods(haveKeys, 10, hwnd);
                }
                else
                {
                    if (keyword.Length != 1)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidSendKeysKeyword", new object[] { keys.Substring(num + 1, num6 - (num + 1)) }));
                    }
                    fStartNewChar = AddSimpleKey(keyword[0], repeat, hwnd, haveKeys, fStartNewChar, cGrp);
                }
                num = num6;
                goto Label_04AB;
            Label_0333:
                AddEvent(new SKEvent(0x100, 0x10, fStartNewChar, hwnd));
                fStartNewChar = false;
                haveKeys[0] = 10;
                goto Label_04AB;
            Label_03C9:
                AddEvent(new SKEvent((haveKeys[1] != 0) ? 0x100 : 260, 0x12, fStartNewChar, hwnd));
                fStartNewChar = false;
                haveKeys[2] = 10;
                goto Label_04AB;
            Label_0414:
                if (haveKeys[0] == 10)
                {
                    haveKeys[0] = cGrp;
                }
                if (haveKeys[1] == 10)
                {
                    haveKeys[1] = cGrp;
                }
                if (haveKeys[2] == 10)
                {
                    haveKeys[2] = cGrp;
                }
                goto Label_04AB;
            Label_045A:
                CancelMods(haveKeys, cGrp, hwnd);
                cGrp--;
                if (cGrp == 0)
                {
                    fStartNewChar = true;
                }
            Label_04AB:
                num++;
            }
            if (cGrp != 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("SendKeysGroupDelimError"));
            }
            CancelMods(haveKeys, 10, hwnd);
        }

        private static void ResetKeyboardUsingSendInput(int INPUTSize)
        {
            if ((capslockChanged || numlockChanged) || (scrollLockChanged || kanaChanged))
            {
                System.Windows.Forms.NativeMethods.INPUT[] pInputs = new System.Windows.Forms.NativeMethods.INPUT[2];
                pInputs[0].type = 1;
                pInputs[0].inputUnion.ki.dwFlags = 0;
                pInputs[1].type = 1;
                pInputs[1].inputUnion.ki.dwFlags = 2;
                if (capslockChanged)
                {
                    pInputs[0].inputUnion.ki.wVk = 20;
                    pInputs[1].inputUnion.ki.wVk = 20;
                    UnsafeNativeMethods.SendInput(2, pInputs, INPUTSize);
                }
                if (numlockChanged)
                {
                    pInputs[0].inputUnion.ki.wVk = 0x90;
                    pInputs[1].inputUnion.ki.wVk = 0x90;
                    UnsafeNativeMethods.SendInput(2, pInputs, INPUTSize);
                }
                if (scrollLockChanged)
                {
                    pInputs[0].inputUnion.ki.wVk = 0x91;
                    pInputs[1].inputUnion.ki.wVk = 0x91;
                    UnsafeNativeMethods.SendInput(2, pInputs, INPUTSize);
                }
                if (kanaChanged)
                {
                    pInputs[0].inputUnion.ki.wVk = 0x15;
                    pInputs[1].inputUnion.ki.wVk = 0x15;
                    UnsafeNativeMethods.SendInput(2, pInputs, INPUTSize);
                }
            }
        }

        public static void Send(string keys)
        {
            Send(keys, null, false);
        }

        private static void Send(string keys, Control control, bool wait)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if ((keys != null) && (keys.Length != 0))
            {
                if (!wait && !Application.MessageLoop)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("SendKeysNoMessageLoop"));
                }
                Queue previousEvents = null;
                if ((events != null) && (events.Count != 0))
                {
                    previousEvents = (Queue) events.Clone();
                }
                ParseKeys(keys, (control != null) ? control.Handle : IntPtr.Zero);
                if (events != null)
                {
                    LoadSendMethodFromConfig();
                    byte[] keyboardState = GetKeyboardState();
                    if (((SendMethodTypes) sendMethod.Value) != SendMethodTypes.SendInput)
                    {
                        if (!hookSupported.HasValue && (((SendMethodTypes) sendMethod.Value) == SendMethodTypes.Default))
                        {
                            TestHook();
                        }
                        if ((((SendMethodTypes) sendMethod.Value) == SendMethodTypes.JournalHook) || hookSupported.Value)
                        {
                            ClearKeyboardState();
                            InstallHook();
                            SetKeyboardState(keyboardState);
                        }
                    }
                    if ((((SendMethodTypes) sendMethod.Value) == SendMethodTypes.SendInput) || ((((SendMethodTypes) sendMethod.Value) == SendMethodTypes.Default) && !hookSupported.Value))
                    {
                        SendInput(keyboardState, previousEvents);
                    }
                    if (wait)
                    {
                        Flush();
                    }
                }
            }
        }

        private static void SendInput(byte[] oldKeyboardState, Queue previousEvents)
        {
            int count;
            AddCancelModifiersForPreviousEvents(previousEvents);
            System.Windows.Forms.NativeMethods.INPUT[] pInputs = new System.Windows.Forms.NativeMethods.INPUT[2];
            pInputs[0].type = 1;
            pInputs[1].type = 1;
            pInputs[1].inputUnion.ki.wVk = 0;
            pInputs[1].inputUnion.ki.dwFlags = 6;
            pInputs[0].inputUnion.ki.dwExtraInfo = IntPtr.Zero;
            pInputs[0].inputUnion.ki.time = 0;
            pInputs[1].inputUnion.ki.dwExtraInfo = IntPtr.Zero;
            pInputs[1].inputUnion.ki.time = 0;
            int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.INPUT));
            uint num2 = 0;
            lock (events.SyncRoot)
            {
                bool flag = UnsafeNativeMethods.BlockInput(true);
                try
                {
                    count = events.Count;
                    ClearGlobalKeys();
                    for (int i = 0; i < count; i++)
                    {
                        SKEvent skEvent = (SKEvent) events.Dequeue();
                        pInputs[0].inputUnion.ki.dwFlags = 0;
                        if (skEvent.wm == 0x102)
                        {
                            pInputs[0].inputUnion.ki.wVk = 0;
                            pInputs[0].inputUnion.ki.wScan = (short) skEvent.paramL;
                            pInputs[0].inputUnion.ki.dwFlags = 4;
                            pInputs[1].inputUnion.ki.wScan = (short) skEvent.paramL;
                            num2 += UnsafeNativeMethods.SendInput(2, pInputs, cbSize) - 1;
                        }
                        else
                        {
                            pInputs[0].inputUnion.ki.wScan = 0;
                            if ((skEvent.wm == 0x101) || (skEvent.wm == 0x105))
                            {
                                pInputs[0].inputUnion.ki.dwFlags |= 2;
                            }
                            if (IsExtendedKey(skEvent))
                            {
                                pInputs[0].inputUnion.ki.dwFlags |= 1;
                            }
                            pInputs[0].inputUnion.ki.wVk = (short) skEvent.paramL;
                            num2 += UnsafeNativeMethods.SendInput(1, pInputs, cbSize);
                            CheckGlobalKeys(skEvent);
                        }
                        Thread.Sleep(1);
                    }
                    ResetKeyboardUsingSendInput(cbSize);
                }
                finally
                {
                    SetKeyboardState(oldKeyboardState);
                    if (flag)
                    {
                        UnsafeNativeMethods.BlockInput(false);
                    }
                }
            }
            if (num2 != count)
            {
                throw new Win32Exception();
            }
        }

        public static void SendWait(string keys)
        {
            SendWait(keys, null);
        }

        private static void SendWait(string keys, Control control)
        {
            Send(keys, control, true);
        }

        private static void SetKeyboardState(byte[] keystate)
        {
            UnsafeNativeMethods.SetKeyboardState(keystate);
        }

        private static void TestHook()
        {
            hookSupported = false;
            try
            {
                System.Windows.Forms.NativeMethods.HookProc pfnhook = new System.Windows.Forms.NativeMethods.HookProc(SendKeys.EmptyHookCallback);
                IntPtr handle = UnsafeNativeMethods.SetWindowsHookEx(1, pfnhook, new HandleRef(null, UnsafeNativeMethods.GetModuleHandle(null)), 0);
                hookSupported = new bool?(handle != IntPtr.Zero);
                if (handle != IntPtr.Zero)
                {
                    UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(null, handle));
                }
            }
            catch
            {
            }
        }

        private static void UninstallJournalingHook()
        {
            if (hhook != IntPtr.Zero)
            {
                stopHook = false;
                if (events != null)
                {
                    events.Clear();
                }
                UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(null, hhook));
                hhook = IntPtr.Zero;
            }
        }

        private class KeywordVk
        {
            internal string keyword;
            internal int vk;

            public KeywordVk(string key, int v)
            {
                this.keyword = key;
                this.vk = v;
            }
        }

        private class SendKeysHookProc
        {
            private bool gotNextEvent;

            public virtual IntPtr Callback(int code, IntPtr wparam, IntPtr lparam)
            {
                System.Windows.Forms.NativeMethods.EVENTMSG structure = (System.Windows.Forms.NativeMethods.EVENTMSG) UnsafeNativeMethods.PtrToStructure(lparam, typeof(System.Windows.Forms.NativeMethods.EVENTMSG));
                if (UnsafeNativeMethods.GetAsyncKeyState(0x13) != 0)
                {
                    SendKeys.stopHook = true;
                }
                switch (code)
                {
                    case 1:
                    {
                        this.gotNextEvent = true;
                        SendKeys.SKEvent event2 = (SendKeys.SKEvent) SendKeys.events.Peek();
                        structure.message = event2.wm;
                        structure.paramL = event2.paramL;
                        structure.paramH = event2.paramH;
                        structure.hwnd = event2.hwnd;
                        structure.time = SafeNativeMethods.GetTickCount();
                        Marshal.StructureToPtr(structure, lparam, true);
                        break;
                    }
                    case 2:
                        if (this.gotNextEvent)
                        {
                            if ((SendKeys.events != null) && (SendKeys.events.Count > 0))
                            {
                                SendKeys.events.Dequeue();
                            }
                            SendKeys.stopHook = (SendKeys.events == null) || (SendKeys.events.Count == 0);
                        }
                        break;

                    default:
                        if (code < 0)
                        {
                            UnsafeNativeMethods.CallNextHookEx(new HandleRef(null, SendKeys.hhook), code, wparam, lparam);
                        }
                        break;
                }
                if (SendKeys.stopHook)
                {
                    SendKeys.UninstallJournalingHook();
                    this.gotNextEvent = false;
                }
                return IntPtr.Zero;
            }
        }

        private enum SendMethodTypes
        {
            Default = 1,
            JournalHook = 2,
            SendInput = 3
        }

        private class SKEvent
        {
            internal IntPtr hwnd;
            internal int paramH;
            internal int paramL;
            internal int wm;

            public SKEvent(int a, int b, bool c, IntPtr hwnd)
            {
                this.wm = a;
                this.paramL = b;
                this.paramH = c ? 1 : 0;
                this.hwnd = hwnd;
            }

            public SKEvent(int a, int b, int c, IntPtr hwnd)
            {
                this.wm = a;
                this.paramL = b;
                this.paramH = c;
                this.hwnd = hwnd;
            }
        }

        private class SKWindow : Control
        {
            public SKWindow()
            {
                base.SetState(0x80000, true);
                base.SetState2(8, false);
                base.SetBounds(-1, -1, 0, 0);
                base.Visible = false;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x4b)
                {
                    try
                    {
                        SendKeys.JournalCancel();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}

