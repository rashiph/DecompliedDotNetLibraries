namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    public sealed class ErrObject
    {
        private bool m_ClearOnCapture;
        private string m_curDescription;
        private int m_curErl;
        private Exception m_curException;
        private int m_curHelpContext;
        private string m_curHelpFile;
        private int m_curNumber;
        private string m_curSource;
        private bool m_DescriptionIsSet;
        private bool m_HelpContextIsSet;
        private bool m_HelpFileIsSet;
        private bool m_NumberIsSet;
        private bool m_SourceIsSet;

        internal ErrObject()
        {
            this.Clear();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        internal void CaptureException(Exception ex)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (ex != this.m_curException)
                {
                    if (this.m_ClearOnCapture)
                    {
                        this.Clear();
                    }
                    else
                    {
                        this.m_ClearOnCapture = true;
                    }
                    this.m_curException = ex;
                }
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void CaptureException(Exception ex, int lErl)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.CaptureException(ex);
                this.m_curErl = lErl;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public void Clear()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.m_curException = null;
                this.m_curNumber = 0;
                this.m_curSource = "";
                this.m_curDescription = "";
                this.m_curHelpFile = "";
                this.m_curHelpContext = 0;
                this.m_curErl = 0;
                this.m_NumberIsSet = false;
                this.m_SourceIsSet = false;
                this.m_DescriptionIsSet = false;
                this.m_HelpFileIsSet = false;
                this.m_HelpContextIsSet = false;
                this.m_ClearOnCapture = true;
            }
        }

        internal Exception CreateException(int Number, string Description)
        {
            this.Clear();
            this.Number = Number;
            if (Number == 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Number" }));
            }
            Exception exception2 = this.MapNumberToException(this.m_curNumber, Description);
            this.m_ClearOnCapture = false;
            return exception2;
        }

        private string FilterDefaultMessage(string Msg)
        {
            if (this.m_curException != null)
            {
                int number = this.Number;
                if ((Msg == null) || (Msg.Length == 0))
                {
                    Msg = Utils.GetResourceString("ID" + Conversions.ToString(number));
                    return Msg;
                }
                if (string.CompareOrdinal("Exception from HRESULT: 0x", 0, Msg, 0, Math.Min(Msg.Length, 0x1a)) == 0)
                {
                    string resourceString = Utils.GetResourceString("ID" + Conversions.ToString(this.m_curNumber), false);
                    if (resourceString != null)
                    {
                        Msg = resourceString;
                    }
                }
            }
            return Msg;
        }

        public Exception GetException()
        {
            return this.m_curException;
        }

        private string MakeHelpLink(string HelpFile, int HelpContext)
        {
            return (HelpFile + "#" + Conversions.ToString(HelpContext));
        }

        internal int MapErrorNumber(int Number)
        {
            if (Number > 0xffff)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Number" }));
            }
            if (Number < 0)
            {
                if ((Number & 0x1fff0000) == 0xa0000)
                {
                    return (Number & 0xffff);
                }
                switch (Number)
                {
                    case -2147467263:
                        return 0x1bd;

                    case -2147467262:
                        return 430;

                    case -2147467261:
                        return -2147467261;

                    case -2147467260:
                        return 0x11f;

                    case -2147352575:
                        return 0x1b6;

                    case -2147352573:
                        return 0x1b6;

                    case -2147352572:
                        return 0x1c0;

                    case -2147352571:
                        return 13;

                    case -2147352570:
                        return 0x1b6;

                    case -2147352569:
                        return 0x1be;

                    case -2147352568:
                        return 0x1ca;

                    case -2147352566:
                        return 6;

                    case -2147352565:
                        return 9;

                    case -2147352564:
                        return 0x1bf;

                    case -2147352563:
                        return 10;

                    case -2147352562:
                        return 450;

                    case -2147352561:
                        return 0x1c1;

                    case -2147352559:
                        return 0x1c3;

                    case -2147352558:
                        return 11;

                    case -2147319786:
                        return 0x8016;

                    case -2147319785:
                        return 0x1cd;

                    case -2147319784:
                        return 0x8018;

                    case -2147319783:
                        return 0x8019;

                    case -2147319780:
                        return 0x801c;

                    case -2147319779:
                        return 0x801d;

                    case -2147319769:
                        return 0x8027;

                    case -2147319768:
                        return 0x8028;

                    case -2147319767:
                        return 0x8029;

                    case -2147319766:
                        return 0x802a;

                    case -2147319765:
                        return 0x802b;

                    case -2147319764:
                        return 0x802c;

                    case -2147319763:
                        return 0x802d;

                    case -2147319762:
                        return 0x802e;

                    case -2147319761:
                        return 0x1c5;

                    case -2147317571:
                        return 0x88bd;

                    case -2147317563:
                        return 0x88c5;

                    case -2147316576:
                        return 13;

                    case -2147316575:
                        return 9;

                    case -2147316574:
                        return 0x39;

                    case -2147316573:
                        return 0x142;

                    case -2147312566:
                        return 0x30;

                    case -2147312509:
                        return 0x9c83;

                    case -2147312508:
                        return 0x9c84;

                    case -2147287039:
                        return 0x8006;

                    case -2147287038:
                        return 0x35;

                    case -2147287037:
                        return 0x4c;

                    case -2147287036:
                        return 0x43;

                    case -2147287035:
                        return 70;

                    case -2147287034:
                        return 0x8004;

                    case -2147287032:
                        return 7;

                    case -2147287022:
                        return 0x43;

                    case -2147287021:
                        return 70;

                    case -2147287015:
                        return 0x8003;

                    case -2147287011:
                        return 0x8005;

                    case -2147287010:
                        return 0x8004;

                    case -2147287008:
                        return 0x4b;

                    case -2147287007:
                        return 70;

                    case -2147286960:
                        return 0x3a;

                    case -2147286928:
                        return 0x3d;

                    case -2147286789:
                        return 0x8018;

                    case -2147286788:
                        return 0x35;

                    case -2147286787:
                        return 0x8018;

                    case -2147286786:
                        return 0x8000;

                    case -2147286784:
                        return 70;

                    case -2147286783:
                        return 70;

                    case -2147286782:
                        return 0x8005;

                    case -2147286781:
                        return 0x39;

                    case -2147286780:
                        return 0x8019;

                    case -2147286779:
                        return 0x8019;

                    case -2147286778:
                        return 0x8015;

                    case -2147286777:
                        return 0x8019;

                    case -2147286776:
                        return 0x8019;

                    case -2147221230:
                        return 0x1ad;

                    case -2147221164:
                        return 0x1ad;

                    case -2147221021:
                        return 0x1ad;

                    case -2147221018:
                        return 0x1b0;

                    case -2147221014:
                        return 0x1b0;

                    case -2147221005:
                        return 0x1ad;

                    case -2147221003:
                        return 0x1ad;

                    case -2147220994:
                        return 0x1ad;

                    case -2147024891:
                        return 70;

                    case -2147024882:
                        return 7;

                    case -2147024809:
                        return 5;

                    case -2147023174:
                        return 0x1ce;

                    case -2146959355:
                        return 0x1ad;
                }
            }
            return Number;
        }

        private int MapExceptionToNumber(Exception e)
        {
            Type type = e.GetType();
            if (type == typeof(IndexOutOfRangeException))
            {
                return 9;
            }
            if (type == typeof(RankException))
            {
                return 9;
            }
            if (type == typeof(DivideByZeroException))
            {
                return 11;
            }
            if (type == typeof(OverflowException))
            {
                return 6;
            }
            if (type == typeof(NotFiniteNumberException))
            {
                NotFiniteNumberException exception = (NotFiniteNumberException) e;
                if (exception.OffendingNumber == 0.0)
                {
                    return 11;
                }
                return 6;
            }
            if (type == typeof(NullReferenceException))
            {
                return 0x5b;
            }
            if (e is AccessViolationException)
            {
                return -2147467261;
            }
            if (type == typeof(InvalidCastException))
            {
                return 13;
            }
            if (type == typeof(NotSupportedException))
            {
                return 13;
            }
            if (type == typeof(COMException))
            {
                COMException exception2 = (COMException) e;
                return Utils.MapHRESULT(exception2.ErrorCode);
            }
            if (type == typeof(SEHException))
            {
                return 0x63;
            }
            if (type == typeof(DllNotFoundException))
            {
                return 0x35;
            }
            if (type == typeof(EntryPointNotFoundException))
            {
                return 0x1c5;
            }
            if (type == typeof(TypeLoadException))
            {
                return 0x1ad;
            }
            if (type == typeof(OutOfMemoryException))
            {
                return 7;
            }
            if (type == typeof(FormatException))
            {
                return 13;
            }
            if (type == typeof(DirectoryNotFoundException))
            {
                return 0x4c;
            }
            if (type == typeof(IOException))
            {
                return 0x39;
            }
            if (type == typeof(FileNotFoundException))
            {
                return 0x35;
            }
            if (e is MissingMemberException)
            {
                return 0x1b6;
            }
            if (e is InvalidOleVariantTypeException)
            {
                return 0x1ca;
            }
            return 5;
        }

        private Exception MapNumberToException(int Number, string Description)
        {
            bool vBDefinedError = false;
            return ExceptionUtils.BuildException(Number, Description, ref vBDefinedError);
        }

        private void ParseHelpLink(string HelpLink)
        {
            if ((HelpLink == null) || (HelpLink.Length == 0))
            {
                if (!this.m_HelpContextIsSet)
                {
                    this.HelpContext = 0;
                }
                if (!this.m_HelpFileIsSet)
                {
                    this.HelpFile = "";
                }
            }
            else
            {
                int length = Strings.m_InvariantCompareInfo.IndexOf(HelpLink, "#", CompareOptions.Ordinal);
                if (length != -1)
                {
                    if (!this.m_HelpContextIsSet)
                    {
                        if (length < HelpLink.Length)
                        {
                            this.HelpContext = Conversions.ToInteger(HelpLink.Substring(length + 1));
                        }
                        else
                        {
                            this.HelpContext = 0;
                        }
                    }
                    if (!this.m_HelpFileIsSet)
                    {
                        this.HelpFile = HelpLink.Substring(0, length);
                    }
                }
                else
                {
                    if (!this.m_HelpContextIsSet)
                    {
                        this.HelpContext = 0;
                    }
                    if (!this.m_HelpFileIsSet)
                    {
                        this.HelpFile = HelpLink;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Raise(int Number, object Source = null, object Description = null, object HelpFile = null, object HelpContext = null)
        {
            if (Number == 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Number" }));
            }
            this.Number = Number;
            if (Source != null)
            {
                this.Source = Conversions.ToString(Source);
            }
            else
            {
                IVbHost vBHost = HostServices.VBHost;
                if (vBHost == null)
                {
                    string fullName = Assembly.GetCallingAssembly().FullName;
                    int num = Strings.InStr(fullName, ",", CompareMethod.Binary);
                    if (num < 1)
                    {
                        this.Source = fullName;
                    }
                    else
                    {
                        this.Source = Strings.Left(fullName, num - 1);
                    }
                }
                else
                {
                    this.Source = vBHost.GetWindowTitle();
                }
            }
            if (HelpFile != null)
            {
                this.HelpFile = Conversions.ToString(HelpFile);
            }
            if (HelpContext != null)
            {
                this.HelpContext = Conversions.ToInteger(HelpContext);
            }
            if (Description != null)
            {
                this.Description = Conversions.ToString(Description);
            }
            else if (!this.m_DescriptionIsSet)
            {
                this.Description = Utils.GetResourceString((vbErrors) this.m_curNumber);
            }
            Exception exception = this.MapNumberToException(this.m_curNumber, this.m_curDescription);
            exception.Source = this.m_curSource;
            exception.HelpLink = this.MakeHelpLink(this.m_curHelpFile, this.m_curHelpContext);
            this.m_ClearOnCapture = false;
            throw exception;
        }

        internal void SetUnmappedError(int Number)
        {
            this.Clear();
            this.Number = Number;
            this.m_ClearOnCapture = false;
        }

        public string Description
        {
            get
            {
                if (this.m_DescriptionIsSet)
                {
                    return this.m_curDescription;
                }
                if (this.m_curException != null)
                {
                    this.Description = this.FilterDefaultMessage(this.m_curException.Message);
                    return this.m_curDescription;
                }
                return "";
            }
            set
            {
                this.m_curDescription = value;
                this.m_DescriptionIsSet = true;
            }
        }

        public int Erl
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_curErl;
            }
        }

        public int HelpContext
        {
            get
            {
                if (this.m_HelpContextIsSet)
                {
                    return this.m_curHelpContext;
                }
                if (this.m_curException != null)
                {
                    this.ParseHelpLink(this.m_curException.HelpLink);
                    return this.m_curHelpContext;
                }
                return 0;
            }
            set
            {
                this.m_curHelpContext = value;
                this.m_HelpContextIsSet = true;
            }
        }

        public string HelpFile
        {
            get
            {
                if (this.m_HelpFileIsSet)
                {
                    return this.m_curHelpFile;
                }
                if (this.m_curException != null)
                {
                    this.ParseHelpLink(this.m_curException.HelpLink);
                    return this.m_curHelpFile;
                }
                return "";
            }
            set
            {
                this.m_curHelpFile = value;
                this.m_HelpFileIsSet = true;
            }
        }

        public int LastDllError
        {
            [SecurityCritical]
            get
            {
                return Marshal.GetLastWin32Error();
            }
        }

        public int Number
        {
            get
            {
                if (this.m_NumberIsSet)
                {
                    return this.m_curNumber;
                }
                if (this.m_curException != null)
                {
                    this.Number = this.MapExceptionToNumber(this.m_curException);
                    return this.m_curNumber;
                }
                return 0;
            }
            set
            {
                this.m_curNumber = this.MapErrorNumber(value);
                this.m_NumberIsSet = true;
            }
        }

        public string Source
        {
            get
            {
                if (this.m_SourceIsSet)
                {
                    return this.m_curSource;
                }
                if (this.m_curException != null)
                {
                    this.Source = this.m_curException.Source;
                    return this.m_curSource;
                }
                return "";
            }
            set
            {
                this.m_curSource = value;
                this.m_SourceIsSet = true;
            }
        }
    }
}

