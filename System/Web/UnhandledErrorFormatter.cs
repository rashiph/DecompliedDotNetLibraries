namespace System.Web
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal class UnhandledErrorFormatter : ErrorFormatter
    {
        private string _coloredSquare2Content;
        protected System.Exception _e;
        protected ArrayList _exStack;
        private bool _fGeneratedCodeOnStack;
        protected System.Exception _initialException;
        protected int _line;
        protected string _message;
        protected string _physicalPath;
        protected string _postMessage;

        internal UnhandledErrorFormatter(System.Exception e) : this(e, null, null)
        {
        }

        internal UnhandledErrorFormatter(System.Exception e, string message, string postMessage)
        {
            this._exStack = new ArrayList();
            this._message = message;
            this._postMessage = postMessage;
            this._e = e;
        }

        private string GetFileName(StackFrame sf)
        {
            string fileName = null;
            try
            {
                fileName = sf.GetFileName();
            }
            catch (SecurityException)
            {
            }
            return fileName;
        }

        internal override void PrepareFormatter()
        {
            for (System.Exception exception = this._e; exception != null; exception = exception.InnerException)
            {
                this._exStack.Add(exception);
                this._initialException = exception;
            }
            this._coloredSquare2Content = this.ColoredSquare2Content;
        }

        protected override string ColoredSquare2Content
        {
            get
            {
                if (this._coloredSquare2Content == null)
                {
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    int startIndex = 0;
                    for (int i = this._exStack.Count - 1; i >= 0; i--)
                    {
                        if (i < (this._exStack.Count - 1))
                        {
                            builder.Append("\r\n");
                        }
                        System.Exception e = (System.Exception) this._exStack[i];
                        builder.Append("[" + this._exStack[i].GetType().Name);
                        if ((e is ExternalException) && (((ExternalException) e).ErrorCode != 0))
                        {
                            builder.Append(" (0x" + ((ExternalException) e).ErrorCode.ToString("x", CultureInfo.CurrentCulture) + ")");
                        }
                        if ((e.Message != null) && (e.Message.Length > 0))
                        {
                            builder.Append(": " + e.Message);
                        }
                        builder.Append("]\r\n");
                        StackTrace trace = new StackTrace(e, true);
                        for (int j = 0; j < trace.FrameCount; j++)
                        {
                            if (flag)
                            {
                                startIndex = builder.Length;
                            }
                            StackFrame sf = trace.GetFrame(j);
                            MethodBase method = sf.GetMethod();
                            Type declaringType = method.DeclaringType;
                            string str = string.Empty;
                            if (declaringType != null)
                            {
                                string path = null;
                                try
                                {
                                    path = Util.GetAssemblyCodeBase(declaringType.Assembly);
                                }
                                catch
                                {
                                }
                                if (((path != null) && (string.Compare(Path.GetDirectoryName(path), HttpRuntime.CodegenDirInternal, StringComparison.OrdinalIgnoreCase) == 0)) && (sf.GetNativeOffset() > 0))
                                {
                                    this._fGeneratedCodeOnStack = true;
                                }
                                str = declaringType.Namespace;
                            }
                            if (str != null)
                            {
                                str = str + ".";
                            }
                            if (declaringType == null)
                            {
                                builder.Append("   " + method.Name + "(");
                            }
                            else
                            {
                                builder.Append("   " + str + declaringType.Name + "." + method.Name + "(");
                            }
                            ParameterInfo[] parameters = method.GetParameters();
                            for (int k = 0; k < parameters.Length; k++)
                            {
                                builder.Append(((k != 0) ? ", " : string.Empty) + parameters[k].ParameterType.Name + " " + parameters[k].Name);
                            }
                            builder.Append(")");
                            string fileName = this.GetFileName(sf);
                            if (fileName != null)
                            {
                                fileName = ErrorFormatter.ResolveHttpFileName(fileName);
                                if (fileName != null)
                                {
                                    if ((this._physicalPath == null) && FileUtil.FileExists(fileName))
                                    {
                                        this._physicalPath = fileName;
                                        this._line = sf.GetFileLineNumber();
                                    }
                                    builder.Append(string.Concat(new object[] { " in ", HttpRuntime.GetSafePath(fileName), ":", sf.GetFileLineNumber() }));
                                }
                            }
                            else
                            {
                                builder.Append(" +" + sf.GetNativeOffset());
                            }
                            if (flag)
                            {
                                string s = builder.ToString(startIndex, builder.Length - startIndex);
                                this.AdaptiveStackTrace.Add(HttpUtility.HtmlEncode(s));
                            }
                            builder.Append("\r\n");
                        }
                        flag = false;
                    }
                    this._coloredSquare2Content = HttpUtility.HtmlEncode(builder.ToString());
                    this._coloredSquare2Content = base.WrapWithLeftToRightTextFormatIfNeeded(this._coloredSquare2Content);
                }
                return this._coloredSquare2Content;
            }
        }

        protected override string ColoredSquare2Title
        {
            get
            {
                return System.Web.SR.GetString("Unhandled_Err_Stack_Trace");
            }
        }

        protected override string ColoredSquareContent
        {
            get
            {
                string str;
                if (this._physicalPath != null)
                {
                    return FormatterWithFileInfo.GetSourceFileLines(this._physicalPath, Encoding.Default, null, this._line);
                }
                bool flag = false;
                if (!this._fGeneratedCodeOnStack || !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                {
                    str = System.Web.SR.GetString("Src_not_available_nodebug");
                }
                else
                {
                    if (ErrorFormatter.IsTextRightToLeft)
                    {
                        flag = true;
                    }
                    str = System.Web.SR.GetString("Src_not_available", new object[] { flag ? "BeginMarker" : string.Empty, flag ? "EndMarker" : string.Empty, flag ? "BeginMarker" : string.Empty, flag ? "EndMarker" : string.Empty });
                }
                str = HttpUtility.FormatPlainTextAsHtml(str);
                if (flag)
                {
                    str = str.Replace("BeginMarker", "</code><div dir=\"ltr\"><code>").Replace("EndMarker", "</code></div><code>");
                }
                return str;
            }
        }

        protected override string ColoredSquareTitle
        {
            get
            {
                return System.Web.SR.GetString("TmplCompilerSourceSecTitle");
            }
        }

        protected override string Description
        {
            get
            {
                if (this._message != null)
                {
                    return this._message;
                }
                return System.Web.SR.GetString("Unhandled_Err_Desc");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                string message = this._initialException.Message;
                if (!string.IsNullOrEmpty(message))
                {
                    return HttpUtility.FormatPlainTextAsHtml(message);
                }
                return System.Web.SR.GetString("Unhandled_Err_Error");
            }
        }

        protected override System.Exception Exception
        {
            get
            {
                return this._e;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                string fullName = this._initialException.GetType().FullName;
                StringBuilder builder = new StringBuilder(fullName);
                string str2 = fullName;
                if (this._initialException.Message != null)
                {
                    string str3 = HttpUtility.FormatPlainTextAsHtml(this._initialException.Message);
                    builder.Append(": ");
                    builder.Append(str3);
                    str2 = str2 + ": " + str3;
                }
                this.AdaptiveMiscContent.Add(str2);
                if (this._initialException is UnauthorizedAccessException)
                {
                    builder.Append("\r\n<br><br>");
                    string str4 = HttpUtility.HtmlEncode(System.Web.SR.GetString("Unauthorized_Err_Desc1"));
                    builder.Append(str4);
                    this.AdaptiveMiscContent.Add(str4);
                    builder.Append("\r\n<br><br>");
                    str4 = HttpUtility.HtmlEncode(System.Web.SR.GetString("Unauthorized_Err_Desc2"));
                    builder.Append(str4);
                    this.AdaptiveMiscContent.Add(str4);
                }
                else if (this._initialException is HostingEnvironmentException)
                {
                    string details = ((HostingEnvironmentException) this._initialException).Details;
                    if (!string.IsNullOrEmpty(details))
                    {
                        builder.Append("\r\n<br><br><b>");
                        builder.Append(details);
                        builder.Append("</b>");
                        this.AdaptiveMiscContent.Add(details);
                    }
                }
                return builder.ToString();
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("Unhandled_Err_Exception_Details");
            }
        }

        protected override string PhysicalPath
        {
            get
            {
                return this._physicalPath;
            }
        }

        protected override string PostMessage
        {
            get
            {
                return this._postMessage;
            }
        }

        protected override bool ShowSourceFileInfo
        {
            get
            {
                return (this._physicalPath != null);
            }
        }

        protected override int SourceFileLineNumber
        {
            get
            {
                return this._line;
            }
        }

        protected override bool WrapColoredSquareContentLines
        {
            get
            {
                return (this._physicalPath == null);
            }
        }
    }
}

