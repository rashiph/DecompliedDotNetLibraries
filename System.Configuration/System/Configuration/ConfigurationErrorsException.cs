namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Internal;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    [Serializable]
    public class ConfigurationErrorsException : ConfigurationException
    {
        private ConfigurationException[] _errors;
        private string _firstFilename;
        private int _firstLine;
        private const string HTTP_PREFIX = "http:";
        private const string SERIALIZATION_PARAM_ERROR_COUNT = "count";
        private const string SERIALIZATION_PARAM_ERROR_DATA = "_errors";
        private const string SERIALIZATION_PARAM_ERROR_TYPE = "_errors_type";
        private const string SERIALIZATION_PARAM_FILENAME = "firstFilename";
        private const string SERIALIZATION_PARAM_LINE = "firstLine";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationErrorsException() : this(null, null, null, 0)
        {
        }

        internal ConfigurationErrorsException(ArrayList coll) : this((coll.Count > 0) ? ((ConfigurationException) coll[0]) : null)
        {
            if (coll.Count > 1)
            {
                this._errors = new ConfigurationException[coll.Count];
                coll.CopyTo(this._errors, 0);
                ConfigurationException[] exceptionArray = this._errors;
                for (int i = 0; i < exceptionArray.Length; i++)
                {
                    object obj2 = exceptionArray[i];
                    ConfigurationException exception1 = (ConfigurationException) obj2;
                }
            }
        }

        internal ConfigurationErrorsException(ConfigurationException e) : this(GetBareMessage(e), GetInnerException(e), GetUnsafeFilename(e), GetLineNumber(e))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationErrorsException(string message) : this(message, null, null, 0)
        {
        }

        internal ConfigurationErrorsException(ICollection<ConfigurationException> coll) : this(GetFirstException(coll))
        {
            if (coll.Count > 1)
            {
                this._errors = new ConfigurationException[coll.Count];
                coll.CopyTo(this._errors, 0);
            }
        }

        protected ConfigurationErrorsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            string filename = info.GetString("firstFilename");
            int line = info.GetInt32("firstLine");
            this.Init(filename, line);
            int num2 = info.GetInt32("count");
            if (num2 != 0)
            {
                this._errors = new ConfigurationException[num2];
                for (int i = 0; i < num2; i++)
                {
                    string str2 = i.ToString(CultureInfo.InvariantCulture);
                    Type type = Type.GetType(info.GetString(str2 + "_errors_type"), true);
                    if ((type != typeof(ConfigurationException)) && (type != typeof(ConfigurationErrorsException)))
                    {
                        throw ExceptionUtil.UnexpectedError("ConfigurationErrorsException");
                    }
                    this._errors[i] = (ConfigurationException) info.GetValue(str2 + "_errors", type);
                }
            }
        }

        internal ConfigurationErrorsException(string message, IConfigErrorInfo errorInfo) : this(message, null, GetUnsafeConfigErrorInfoFilename(errorInfo), GetConfigErrorInfoLineNumber(errorInfo))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationErrorsException(string message, Exception inner) : this(message, inner, null, 0)
        {
        }

        public ConfigurationErrorsException(string message, XmlNode node) : this(message, null, GetUnsafeFilename(node), GetLineNumber(node))
        {
        }

        public ConfigurationErrorsException(string message, XmlReader reader) : this(message, null, GetUnsafeFilename(reader), GetLineNumber(reader))
        {
        }

        internal ConfigurationErrorsException(string message, Exception inner, IConfigErrorInfo errorInfo) : this(message, inner, GetUnsafeConfigErrorInfoFilename(errorInfo), GetConfigErrorInfoLineNumber(errorInfo))
        {
        }

        public ConfigurationErrorsException(string message, Exception inner, XmlNode node) : this(message, inner, GetUnsafeFilename(node), GetLineNumber(node))
        {
        }

        public ConfigurationErrorsException(string message, Exception inner, XmlReader reader) : this(message, inner, GetUnsafeFilename(reader), GetLineNumber(reader))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationErrorsException(string message, string filename, int line) : this(message, null, filename, line)
        {
        }

        public ConfigurationErrorsException(string message, Exception inner, string filename, int line) : base(message, inner)
        {
            this.Init(filename, line);
        }

        internal static string AlwaysSafeFilename(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                if (StringUtil.StartsWithIgnoreCase(filename, "http:"))
                {
                    return filename;
                }
                try
                {
                    if (!Path.IsPathRooted(filename))
                    {
                        return filename;
                    }
                }
                catch
                {
                    return null;
                }
                try
                {
                    filename = Path.GetFileName(FullPathWithAssert(filename));
                }
                catch
                {
                    filename = null;
                }
            }
            return filename;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private static string FullPathWithAssert(string filename)
        {
            string fullPath = null;
            try
            {
                fullPath = Path.GetFullPath(filename);
            }
            catch
            {
            }
            return fullPath;
        }

        private static string GetBareMessage(ConfigurationException e)
        {
            if (e != null)
            {
                return e.BareMessage;
            }
            return null;
        }

        private static int GetConfigErrorInfoLineNumber(IConfigErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                return errorInfo.LineNumber;
            }
            return 0;
        }

        public static string GetFilename(XmlNode node)
        {
            return SafeFilename(GetUnsafeFilename(node));
        }

        public static string GetFilename(XmlReader reader)
        {
            return SafeFilename(GetUnsafeFilename(reader));
        }

        private static ConfigurationException GetFirstException(ICollection<ConfigurationException> coll)
        {
            using (IEnumerator<ConfigurationException> enumerator = coll.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return null;
        }

        private static Exception GetInnerException(ConfigurationException e)
        {
            if (e != null)
            {
                return e.InnerException;
            }
            return null;
        }

        private static int GetLineNumber(ConfigurationException e)
        {
            if (e != null)
            {
                return e.Line;
            }
            return 0;
        }

        public static int GetLineNumber(XmlNode node)
        {
            return GetConfigErrorInfoLineNumber(node as IConfigErrorInfo);
        }

        public static int GetLineNumber(XmlReader reader)
        {
            return GetConfigErrorInfoLineNumber(reader as IConfigErrorInfo);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int length = 0;
            base.GetObjectData(info, context);
            info.AddValue("firstFilename", this.Filename);
            info.AddValue("firstLine", this.Line);
            if ((this._errors != null) && (this._errors.Length > 1))
            {
                length = this._errors.Length;
                for (int i = 0; i < this._errors.Length; i++)
                {
                    string str = i.ToString(CultureInfo.InvariantCulture);
                    info.AddValue(str + "_errors", this._errors[i]);
                    info.AddValue(str + "_errors_type", this._errors[i].GetType());
                }
            }
            info.AddValue("count", length);
        }

        private static string GetUnsafeConfigErrorInfoFilename(IConfigErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                return errorInfo.Filename;
            }
            return null;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private static string GetUnsafeFilename(ConfigurationException e)
        {
            if (e != null)
            {
                return e.Filename;
            }
            return null;
        }

        private static string GetUnsafeFilename(XmlNode node)
        {
            return GetUnsafeConfigErrorInfoFilename(node as IConfigErrorInfo);
        }

        private static string GetUnsafeFilename(XmlReader reader)
        {
            return GetUnsafeConfigErrorInfoFilename(reader as IConfigErrorInfo);
        }

        private void Init(string filename, int line)
        {
            base.HResult = -2146232062;
            if (line == -1)
            {
                line = 0;
            }
            this._firstFilename = filename;
            this._firstLine = line;
        }

        internal static string SafeFilename(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                if (StringUtil.StartsWithIgnoreCase(filename, "http:"))
                {
                    return filename;
                }
                try
                {
                    if (!Path.IsPathRooted(filename))
                    {
                        return filename;
                    }
                }
                catch
                {
                    return null;
                }
                try
                {
                    Path.GetFullPath(filename);
                }
                catch (SecurityException)
                {
                    try
                    {
                        filename = Path.GetFileName(FullPathWithAssert(filename));
                    }
                    catch
                    {
                        filename = null;
                    }
                }
                catch
                {
                    filename = null;
                }
            }
            return filename;
        }

        public override string BareMessage
        {
            get
            {
                return base.BareMessage;
            }
        }

        public ICollection Errors
        {
            get
            {
                if (this._errors != null)
                {
                    return this._errors;
                }
                ConfigurationErrorsException exception = new ConfigurationErrorsException(this.BareMessage, base.InnerException, this._firstFilename, this._firstLine);
                return new ConfigurationException[] { exception };
            }
        }

        internal ICollection<ConfigurationException> ErrorsGeneric
        {
            get
            {
                return (ICollection<ConfigurationException>) this.Errors;
            }
        }

        public override string Filename
        {
            get
            {
                return SafeFilename(this._firstFilename);
            }
        }

        public override int Line
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._firstLine;
            }
        }

        public override string Message
        {
            get
            {
                string filename = this.Filename;
                if (!string.IsNullOrEmpty(filename))
                {
                    if (this.Line != 0)
                    {
                        return (this.BareMessage + " (" + filename + " line " + this.Line.ToString(CultureInfo.CurrentCulture) + ")");
                    }
                    return (this.BareMessage + " (" + filename + ")");
                }
                if (this.Line != 0)
                {
                    return (this.BareMessage + " (line " + this.Line.ToString("G", CultureInfo.CurrentCulture) + ")");
                }
                return this.BareMessage;
            }
        }
    }
}

