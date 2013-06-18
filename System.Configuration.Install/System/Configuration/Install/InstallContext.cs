namespace System.Configuration.Install
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Text;

    public class InstallContext
    {
        private StringDictionary parameters;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstallContext() : this(null, null)
        {
        }

        public InstallContext(string logFilePath, string[] commandLine)
        {
            this.parameters = ParseCommandLine(commandLine);
            if ((this.Parameters["logfile"] == null) && (logFilePath != null))
            {
                this.Parameters["logfile"] = logFilePath;
            }
        }

        public bool IsParameterTrue(string paramName)
        {
            string strA = this.Parameters[paramName.ToLower(CultureInfo.InvariantCulture)];
            if (strA == null)
            {
                return false;
            }
            if (((string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(strA, "yes", StringComparison.OrdinalIgnoreCase) != 0)) && (string.Compare(strA, "1", StringComparison.OrdinalIgnoreCase) != 0))
            {
                return "".Equals(strA);
            }
            return true;
        }

        public void LogMessage(string message)
        {
            try
            {
                this.LogMessageHelper(message);
            }
            catch (Exception)
            {
                try
                {
                    this.Parameters["logfile"] = Path.Combine(Path.GetTempPath(), Path.GetFileName(this.Parameters["logfile"]));
                    this.LogMessageHelper(message);
                }
                catch (Exception)
                {
                    this.Parameters["logfile"] = null;
                }
            }
            if (this.IsParameterTrue("LogToConsole") || (this.Parameters["logtoconsole"] == null))
            {
                Console.WriteLine(message);
            }
        }

        internal void LogMessageHelper(string message)
        {
            StreamWriter writer = null;
            try
            {
                if (!string.IsNullOrEmpty(this.Parameters["logfile"]))
                {
                    writer = new StreamWriter(this.Parameters["logfile"], true, Encoding.UTF8);
                    writer.WriteLine(message);
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        protected static StringDictionary ParseCommandLine(string[] args)
        {
            StringDictionary dictionary = new StringDictionary();
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("/", StringComparison.Ordinal) || args[i].StartsWith("-", StringComparison.Ordinal))
                    {
                        args[i] = args[i].Substring(1);
                    }
                    int index = args[i].IndexOf('=');
                    if (index < 0)
                    {
                        dictionary[args[i].ToLower(CultureInfo.InvariantCulture)] = "";
                    }
                    else
                    {
                        dictionary[args[i].Substring(0, index).ToLower(CultureInfo.InvariantCulture)] = args[i].Substring(index + 1);
                    }
                }
            }
            return dictionary;
        }

        public StringDictionary Parameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameters;
            }
        }
    }
}

