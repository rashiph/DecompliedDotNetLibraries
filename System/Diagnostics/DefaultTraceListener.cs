namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class DefaultTraceListener : TraceListener
    {
        private bool assertUIEnabled;
        private const int internalWriteSize = 0x4000;
        private string logFileName;
        private bool settingsInitialized;

        public DefaultTraceListener() : base("Default")
        {
        }

        public override void Fail(string message)
        {
            this.Fail(message, null);
        }

        public override void Fail(string message, string detailMessage)
        {
            string str;
            StackTrace trace = new StackTrace(true);
            int startFrameIndex = 0;
            bool uiPermission = UiPermission;
            try
            {
                str = this.StackTraceToString(trace, startFrameIndex, trace.FrameCount - 1);
            }
            catch
            {
                str = "";
            }
            this.WriteAssert(str, message, detailMessage);
            if (this.AssertUiEnabled && uiPermission)
            {
                AssertWrapper.ShowAssert(str, trace.GetFrame(startFrameIndex), message, detailMessage);
            }
        }

        private void InitializeSettings()
        {
            this.assertUIEnabled = DiagnosticsConfiguration.AssertUIEnabled;
            this.logFileName = DiagnosticsConfiguration.LogFileName;
            this.settingsInitialized = true;
        }

        private void internalWrite(string message)
        {
            if (Debugger.IsLogging())
            {
                Debugger.Log(0, null, message);
            }
            else if (message == null)
            {
                Microsoft.Win32.SafeNativeMethods.OutputDebugString(string.Empty);
            }
            else
            {
                Microsoft.Win32.SafeNativeMethods.OutputDebugString(message);
            }
        }

        private string StackTraceToString(StackTrace trace, int startFrameIndex, int endFrameIndex)
        {
            StringBuilder builder = new StringBuilder(0x200);
            for (int i = startFrameIndex; i <= endFrameIndex; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                builder.Append(Environment.NewLine);
                builder.Append("    at ");
                if (method.ReflectedType != null)
                {
                    builder.Append(method.ReflectedType.Name);
                }
                else
                {
                    builder.Append("<Module>");
                }
                builder.Append(".");
                builder.Append(method.Name);
                builder.Append("(");
                ParameterInfo[] parameters = method.GetParameters();
                for (int j = 0; j < parameters.Length; j++)
                {
                    ParameterInfo info = parameters[j];
                    if (j > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(info.ParameterType.Name);
                    builder.Append(" ");
                    builder.Append(info.Name);
                }
                builder.Append(")  ");
                builder.Append(frame.GetFileName());
                int fileLineNumber = frame.GetFileLineNumber();
                if (fileLineNumber > 0)
                {
                    builder.Append("(");
                    builder.Append(fileLineNumber.ToString(CultureInfo.InvariantCulture));
                    builder.Append(")");
                }
            }
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }

        public override void Write(string message)
        {
            this.Write(message, true);
        }

        private void Write(string message, bool useLogFile)
        {
            if (base.NeedIndent)
            {
                this.WriteIndent();
            }
            if ((message == null) || (message.Length <= 0x4000))
            {
                this.internalWrite(message);
            }
            else
            {
                int startIndex = 0;
                while (startIndex < (message.Length - 0x4000))
                {
                    this.internalWrite(message.Substring(startIndex, 0x4000));
                    startIndex += 0x4000;
                }
                this.internalWrite(message.Substring(startIndex));
            }
            if (useLogFile && (this.LogFileName.Length != 0))
            {
                this.WriteToLogFile(message, false);
            }
        }

        private void WriteAssert(string stackTrace, string message, string detailMessage)
        {
            string str = SR.GetString("DebugAssertBanner") + Environment.NewLine + SR.GetString("DebugAssertShortMessage") + Environment.NewLine + message + Environment.NewLine + SR.GetString("DebugAssertLongMessage") + Environment.NewLine + detailMessage + Environment.NewLine + stackTrace;
            this.WriteLine(str);
        }

        public override void WriteLine(string message)
        {
            this.WriteLine(message, true);
        }

        private void WriteLine(string message, bool useLogFile)
        {
            if (base.NeedIndent)
            {
                this.WriteIndent();
            }
            this.Write(message + Environment.NewLine, useLogFile);
            base.NeedIndent = true;
        }

        private void WriteToLogFile(string message, bool useWriteLine)
        {
            try
            {
                FileInfo info = new FileInfo(this.LogFileName);
                using (Stream stream = info.Open(FileMode.OpenOrCreate))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        stream.Position = stream.Length;
                        if (useWriteLine)
                        {
                            writer.WriteLine(message);
                        }
                        else
                        {
                            writer.Write(message);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                this.WriteLine(SR.GetString("ExceptionOccurred", new object[] { this.LogFileName, exception.ToString() }), false);
            }
        }

        public bool AssertUiEnabled
        {
            get
            {
                if (!this.settingsInitialized)
                {
                    this.InitializeSettings();
                }
                return this.assertUIEnabled;
            }
            set
            {
                if (!this.settingsInitialized)
                {
                    this.InitializeSettings();
                }
                this.assertUIEnabled = value;
            }
        }

        public string LogFileName
        {
            get
            {
                if (!this.settingsInitialized)
                {
                    this.InitializeSettings();
                }
                return this.logFileName;
            }
            set
            {
                if (!this.settingsInitialized)
                {
                    this.InitializeSettings();
                }
                this.logFileName = value;
            }
        }

        private static bool UiPermission
        {
            get
            {
                bool flag = false;
                try
                {
                    new UIPermission(UIPermissionWindow.SafeSubWindows).Demand();
                    flag = true;
                }
                catch
                {
                }
                return flag;
            }
        }
    }
}

