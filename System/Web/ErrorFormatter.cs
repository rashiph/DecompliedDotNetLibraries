namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    internal abstract class ErrorFormatter
    {
        private StringCollection _adaptiveMiscContent;
        private StringCollection _adaptiveStackTrace;
        protected bool _dontShowVersion;
        protected const string BeginLeftToRightTag = "<div dir=\"ltr\">";
        private const string endExpandableBlock = "                      </pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            \r\n\r\n</div>\r\n";
        protected const string EndLeftToRightTag = "</div>";
        private const string startExpandableBlock = "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">{1}:</div>\r\n<div id=\"{0}\" style=\"display: none;\">\r\n            <br><table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n\r\n";
        private const string toggleScript = "\r\n        <script type=\"text/javascript\">\r\n        function OnToggleTOCLevel1(level2ID)\r\n        {\r\n        var elemLevel2 = document.getElementById(level2ID);\r\n        if (elemLevel2.style.display == 'none')\r\n        {\r\n            elemLevel2.style.display = '';\r\n        }\r\n        else {\r\n            elemLevel2.style.display = 'none';\r\n        }\r\n        }\r\n        </script>\r\n                            ";

        protected ErrorFormatter()
        {
        }

        private Literal CreateBreakLiteral()
        {
            return new Literal { Text = "<br/>" };
        }

        private Label CreateLabelFromText(string text)
        {
            return new Label { Text = text };
        }

        private string FormatStaticErrorMessage(string errorBeginTemplate, string errorEndTemplate)
        {
            StringBuilder builder = new StringBuilder();
            string str = System.Web.SR.GetString("Error_Formatter_ASPNET_Error", new object[] { HttpRuntime.AppDomainAppVirtualPath });
            builder.Append(string.Format(CultureInfo.CurrentCulture, errorBeginTemplate, new object[] { str, this.ErrorTitle }));
            builder.Append(System.Web.SR.GetString("Error_Formatter_Description") + " " + this.Description);
            builder.Append("<br/>\r\n");
            string miscSectionTitle = this.MiscSectionTitle;
            if ((miscSectionTitle != null) && (miscSectionTitle.Length > 0))
            {
                builder.Append(miscSectionTitle);
                builder.Append("<br/>\r\n");
            }
            StringCollection adaptiveMiscContent = this.AdaptiveMiscContent;
            if ((adaptiveMiscContent != null) && (adaptiveMiscContent.Count > 0))
            {
                foreach (string str3 in adaptiveMiscContent)
                {
                    builder.Append(str3);
                    builder.Append("<br/>\r\n");
                }
            }
            string displayPath = this.GetDisplayPath();
            if (!string.IsNullOrEmpty(displayPath))
            {
                string str5 = System.Web.SR.GetString("Error_Formatter_Source_File") + " " + displayPath;
                builder.Append(str5);
                builder.Append("<br/>\r\n");
                str5 = System.Web.SR.GetString("Error_Formatter_Line") + " " + this.SourceFileLineNumber;
                builder.Append(str5);
                builder.Append("<br/>\r\n");
            }
            StringCollection adaptiveStackTrace = this.AdaptiveStackTrace;
            if ((adaptiveStackTrace != null) && (adaptiveStackTrace.Count > 0))
            {
                foreach (string str6 in adaptiveStackTrace)
                {
                    builder.Append(str6);
                    builder.Append("<br/>\r\n");
                }
            }
            builder.Append(errorEndTemplate);
            return builder.ToString();
        }

        internal virtual string GetAdaptiveErrorMessage(HttpContext context, bool dontShowSensitiveInfo)
        {
            this.GetHtmlErrorMessage(dontShowSensitiveInfo);
            context.Response.UseAdaptiveError = true;
            try
            {
                Page page = new ErrorFormatterPage {
                    EnableViewState = false
                };
                HtmlForm child = new HtmlForm();
                page.Controls.Add(child);
                IParserAccessor accessor = child;
                Label label = this.CreateLabelFromText(System.Web.SR.GetString("Error_Formatter_ASPNET_Error", new object[] { HttpRuntime.AppDomainAppVirtualPath }));
                label.ForeColor = Color.Red;
                label.Font.Bold = true;
                label.Font.Size = FontUnit.Large;
                accessor.AddParsedSubObject(label);
                accessor.AddParsedSubObject(this.CreateBreakLiteral());
                label = this.CreateLabelFromText(this.ErrorTitle);
                label.ForeColor = Color.Maroon;
                label.Font.Bold = true;
                label.Font.Italic = true;
                accessor.AddParsedSubObject(label);
                accessor.AddParsedSubObject(this.CreateBreakLiteral());
                accessor.AddParsedSubObject(this.CreateLabelFromText(System.Web.SR.GetString("Error_Formatter_Description") + " " + this.Description));
                accessor.AddParsedSubObject(this.CreateBreakLiteral());
                string miscSectionTitle = this.MiscSectionTitle;
                if (!string.IsNullOrEmpty(miscSectionTitle))
                {
                    accessor.AddParsedSubObject(this.CreateLabelFromText(miscSectionTitle));
                    accessor.AddParsedSubObject(this.CreateBreakLiteral());
                }
                StringCollection adaptiveMiscContent = this.AdaptiveMiscContent;
                if ((adaptiveMiscContent != null) && (adaptiveMiscContent.Count > 0))
                {
                    foreach (string str2 in adaptiveMiscContent)
                    {
                        accessor.AddParsedSubObject(this.CreateLabelFromText(str2));
                        accessor.AddParsedSubObject(this.CreateBreakLiteral());
                    }
                }
                string displayPath = this.GetDisplayPath();
                if (!string.IsNullOrEmpty(displayPath))
                {
                    string text = System.Web.SR.GetString("Error_Formatter_Source_File") + " " + displayPath;
                    accessor.AddParsedSubObject(this.CreateLabelFromText(text));
                    accessor.AddParsedSubObject(this.CreateBreakLiteral());
                    text = System.Web.SR.GetString("Error_Formatter_Line") + " " + this.SourceFileLineNumber;
                    accessor.AddParsedSubObject(this.CreateLabelFromText(text));
                    accessor.AddParsedSubObject(this.CreateBreakLiteral());
                }
                StringCollection adaptiveStackTrace = this.AdaptiveStackTrace;
                if ((adaptiveStackTrace != null) && (adaptiveStackTrace.Count > 0))
                {
                    foreach (string str5 in adaptiveStackTrace)
                    {
                        accessor.AddParsedSubObject(this.CreateLabelFromText(str5));
                        accessor.AddParsedSubObject(this.CreateBreakLiteral());
                    }
                }
                StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
                TextWriter writer2 = context.Response.SwitchWriter(writer);
                page.ProcessRequest(context);
                context.Response.SwitchWriter(writer2);
                return writer.ToString();
            }
            catch
            {
                return this.GetStaticErrorMessage(context);
            }
        }

        private string GetDisplayPath()
        {
            if (this.VirtualPath != null)
            {
                return this.VirtualPath;
            }
            if (this.PhysicalPath != null)
            {
                return HttpRuntime.GetSafePath(this.PhysicalPath);
            }
            return null;
        }

        internal string GetErrorMessage()
        {
            return this.GetErrorMessage(HttpContext.Current, true);
        }

        internal virtual string GetErrorMessage(HttpContext context, bool dontShowSensitiveInfo)
        {
            if (RequiresAdaptiveErrorReporting(context))
            {
                return this.GetAdaptiveErrorMessage(context, dontShowSensitiveInfo);
            }
            return this.GetHtmlErrorMessage(dontShowSensitiveInfo);
        }

        internal string GetHtmlErrorMessage()
        {
            return this.GetHtmlErrorMessage(true);
        }

        internal string GetHtmlErrorMessage(bool dontShowSensitiveInfo)
        {
            this.PrepareFormatter();
            StringBuilder sb = new StringBuilder();
            sb.Append("<html");
            if (IsTextRightToLeft)
            {
                sb.Append(" dir=\"rtl\"");
            }
            sb.Append(">\r\n");
            sb.Append("    <head>\r\n");
            sb.Append("        <title>" + this.ErrorTitle + "</title>\r\n");
            sb.Append("        <style>\r\n");
            sb.Append("         body {font-family:\"Verdana\";font-weight:normal;font-size: .7em;color:black;} \r\n");
            sb.Append("         p {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n");
            sb.Append("         b {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n");
            sb.Append("         H1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n");
            sb.Append("         H2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n");
            sb.Append("         pre {font-family:\"Lucida Console\";font-size: .9em}\r\n");
            sb.Append("         .marker {font-weight: bold; color: black;text-decoration: none;}\r\n");
            sb.Append("         .version {color: gray;}\r\n");
            sb.Append("         .error {margin-bottom: 10px;}\r\n");
            sb.Append("         .expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n");
            sb.Append("        </style>\r\n");
            sb.Append("    </head>\r\n\r\n");
            sb.Append("    <body bgcolor=\"white\">\r\n\r\n");
            sb.Append("            <span><H1>" + System.Web.SR.GetString("Error_Formatter_ASPNET_Error", new object[] { HttpRuntime.AppDomainAppVirtualPath }) + "<hr width=100% size=1 color=silver></H1>\r\n\r\n");
            sb.Append("            <h2> <i>" + this.ErrorTitle + "</i> </h2></span>\r\n\r\n");
            sb.Append("            <font face=\"Arial, Helvetica, Geneva, SunSans-Regular, sans-serif \">\r\n\r\n");
            sb.Append("            <b> " + System.Web.SR.GetString("Error_Formatter_Description") + " </b>" + this.Description + "\r\n");
            sb.Append("            <br><br>\r\n\r\n");
            if (this.MiscSectionTitle != null)
            {
                sb.Append("            <b> " + this.MiscSectionTitle + ": </b>" + this.MiscSectionContent + "<br><br>\r\n\r\n");
            }
            this.WriteColoredSquare(sb, this.ColoredSquareTitle, this.ColoredSquareDescription, this.ColoredSquareContent, this.WrapColoredSquareContentLines);
            if (this.ShowSourceFileInfo)
            {
                string displayPath = this.GetDisplayPath();
                if (displayPath == null)
                {
                    displayPath = System.Web.SR.GetString("Error_Formatter_No_Source_File");
                }
                sb.Append(string.Concat(new object[] { "            <b> ", System.Web.SR.GetString("Error_Formatter_Source_File"), " </b> ", displayPath, "<b> &nbsp;&nbsp; ", System.Web.SR.GetString("Error_Formatter_Line"), " </b> ", this.SourceFileLineNumber, "\r\n" }));
                sb.Append("            <br><br>\r\n\r\n");
            }
            ConfigurationErrorsException exception = this.Exception as ConfigurationErrorsException;
            if ((exception != null) && (exception.Errors.Count > 1))
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">{1}:</div>\r\n<div id=\"{0}\" style=\"display: none;\">\r\n            <br><table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n\r\n", new object[] { "additionalConfigurationErrors", System.Web.SR.GetString("TmplConfigurationAdditionalError") }));
                bool flag = false;
                try
                {
                    PermissionSet namedPermissionSet = HttpRuntime.NamedPermissionSet;
                    if (namedPermissionSet != null)
                    {
                        namedPermissionSet.PermitOnly();
                        flag = true;
                    }
                    int num = 0;
                    foreach (ConfigurationException exception2 in exception.Errors)
                    {
                        if (num > 0)
                        {
                            sb.Append(exception2.Message);
                            sb.Append("<BR/>\r\n");
                        }
                        num++;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
                sb.Append("                      </pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            \r\n\r\n</div>\r\n");
                sb.Append("\r\n        <script type=\"text/javascript\">\r\n        function OnToggleTOCLevel1(level2ID)\r\n        {\r\n        var elemLevel2 = document.getElementById(level2ID);\r\n        if (elemLevel2.style.display == 'none')\r\n        {\r\n            elemLevel2.style.display = '';\r\n        }\r\n        else {\r\n            elemLevel2.style.display = 'none';\r\n        }\r\n        }\r\n        </script>\r\n                            ");
            }
            if ((!dontShowSensitiveInfo && (this.Exception != null)) && HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
            {
                this.WriteFusionLogWithAssert(sb);
            }
            this.WriteColoredSquare(sb, this.ColoredSquare2Title, this.ColoredSquare2Description, this.ColoredSquare2Content, false);
            if (!dontShowSensitiveInfo && !this._dontShowVersion)
            {
                sb.Append("            <hr width=100% size=1 color=silver>\r\n\r\n");
                sb.Append("            <b>" + System.Web.SR.GetString("Error_Formatter_Version") + "</b>&nbsp;" + System.Web.SR.GetString("Error_Formatter_CLR_Build") + VersionInfo.ClrVersion + System.Web.SR.GetString("Error_Formatter_ASPNET_Build") + VersionInfo.EngineVersion + "\r\n\r\n");
                sb.Append("            </font>\r\n\r\n");
            }
            sb.Append("    </body>\r\n");
            sb.Append("</html>\r\n");
            sb.Append(this.PostMessage);
            return sb.ToString();
        }

        private string GetPreferredRenderingType(HttpContext context)
        {
            HttpRequest request = (context != null) ? context.Request : null;
            HttpBrowserCapabilities capabilities = null;
            try
            {
                capabilities = (request != null) ? request.Browser : null;
            }
            catch
            {
                return string.Empty;
            }
            if (capabilities == null)
            {
                return string.Empty;
            }
            return capabilities["preferredRenderingType"];
        }

        internal static string GetSafePath(string linePragma)
        {
            string virtualPathFromHttpLinePragma = GetVirtualPathFromHttpLinePragma(linePragma);
            if (virtualPathFromHttpLinePragma != null)
            {
                return virtualPathFromHttpLinePragma;
            }
            return HttpRuntime.GetSafePath(linePragma);
        }

        private string GetStaticErrorMessage(HttpContext context)
        {
            string preferredRenderingType = this.GetPreferredRenderingType(context);
            if (System.Web.Util.StringUtil.StringStartsWithIgnoreCase(preferredRenderingType, "xhtml"))
            {
                return this.FormatStaticErrorMessage("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD XHTML Mobile 1.0//EN\" \"http://www.wapforum.org/DTD/xhtml-mobile10.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n<title></title>\r\n</head>\r\n<body>\r\n<form>\r\n<div>\r\n<span style=\"color:Red;font-size:Large;font-weight:bold;\">{0}</span><br/>\r\n<span style=\"color:Maroon;font-weight:bold;font-style:italic;\">{1}</span><br/>\r\n", "</div>\r\n</form>\r\n</body>\r\n</html>");
            }
            if (System.Web.Util.StringUtil.StringStartsWithIgnoreCase(preferredRenderingType, "wml"))
            {
                string str2 = this.FormatStaticErrorMessage("<?xml version='1.0'?>\r\n<!DOCTYPE wml PUBLIC '-//WAPFORUM//DTD WML 1.1//EN' 'http://www.wapforum.org/DTD/wml_1.1.xml'><wml><head>\r\n<meta http-equiv=\"Cache-Control\" content=\"max-age=0\" forua=\"true\"/>\r\n</head>\r\n<card>\r\n<p>\r\n<b><big>{0}</big></b><br/>\r\n<b><i>{1}</i></b><br/>\r\n", "</p>\r\n</card>\r\n</wml>\r\n");
                if (string.Compare(context.Response.ContentType, 0, "text/vnd.wap.wml", 0, "text/vnd.wap.wml".Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    context.Response.ContentType = "text/vnd.wap.wml";
                }
                return str2;
            }
            return this.FormatStaticErrorMessage("<html>\r\n<body>\r\n<form>\r\n<font color=\"Red\" size=\"5\">{0}</font><br/>\r\n<font color=\"Maroon\">{1}</font><br/>\r\n", "</form>\r\n</body>\r\n</html>");
        }

        internal static string GetVirtualPathFromHttpLinePragma(string linePragma)
        {
            if (!string.IsNullOrEmpty(linePragma))
            {
                try
                {
                    Uri uri = new Uri(linePragma);
                    if ((uri.Scheme == Uri.UriSchemeHttp) || (uri.Scheme == Uri.UriSchemeHttps))
                    {
                        return uri.LocalPath;
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        internal static string MakeHttpLinePragma(string virtualPath)
        {
            string str = "http://server";
            if ((virtualPath != null) && !virtualPath.StartsWith("/", StringComparison.Ordinal))
            {
                str = str + "/";
            }
            return new Uri(str + virtualPath).ToString();
        }

        internal virtual void PrepareFormatter()
        {
            if (this._adaptiveMiscContent != null)
            {
                this._adaptiveMiscContent.Clear();
            }
            if (this._adaptiveStackTrace != null)
            {
                this._adaptiveStackTrace.Clear();
            }
        }

        internal static bool RequiresAdaptiveErrorReporting(HttpContext context)
        {
            if (HttpRuntime.HostingInitFailed)
            {
                return false;
            }
            HttpRequest request = (context != null) ? context.Request : null;
            if ((context != null) && (context.WorkerRequest is StateHttpWorkerRequest))
            {
                return false;
            }
            HttpBrowserCapabilities capabilities = null;
            try
            {
                capabilities = (request != null) ? request.Browser : null;
            }
            catch
            {
                return false;
            }
            return ((capabilities != null) && (capabilities["requiresAdaptiveErrorReporting"] == "true"));
        }

        internal static string ResolveHttpFileName(string linePragma)
        {
            string virtualPathFromHttpLinePragma = GetVirtualPathFromHttpLinePragma(linePragma);
            if (virtualPathFromHttpLinePragma == null)
            {
                return linePragma;
            }
            return HostingEnvironment.MapPathInternal(virtualPathFromHttpLinePragma);
        }

        protected string WrapWithLeftToRightTextFormatIfNeeded(string content)
        {
            if (IsTextRightToLeft)
            {
                content = "<div dir=\"ltr\">" + content + "</div>";
            }
            return content;
        }

        private void WriteColoredSquare(StringBuilder sb, string title, string description, string content, bool wrapContentLines)
        {
            if (title != null)
            {
                sb.Append("            <b>" + title + ":</b> " + description + "<br><br>\r\n\r\n");
                sb.Append("            <table width=100% bgcolor=\"#ffffcc\">\r\n");
                sb.Append("               <tr>\r\n");
                sb.Append("                  <td>\r\n");
                sb.Append("                      <code>");
                if (!wrapContentLines)
                {
                    sb.Append("<pre>");
                }
                sb.Append("\r\n\r\n");
                sb.Append(content);
                if (!wrapContentLines)
                {
                    sb.Append("</pre>");
                }
                sb.Append("</code>\r\n\r\n");
                sb.Append("                  </td>\r\n");
                sb.Append("               </tr>\r\n");
                sb.Append("            </table>\r\n\r\n");
                sb.Append("            <br>\r\n\r\n");
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void WriteFusionLogWithAssert(StringBuilder sb)
        {
            for (System.Exception exception = this.Exception; exception != null; exception = exception.InnerException)
            {
                string fusionLog = null;
                string fileName = null;
                FileNotFoundException exception2 = exception as FileNotFoundException;
                if (exception2 != null)
                {
                    fusionLog = exception2.FusionLog;
                    fileName = exception2.FileName;
                }
                FileLoadException exception3 = exception as FileLoadException;
                if (exception3 != null)
                {
                    fusionLog = exception3.FusionLog;
                    fileName = exception3.FileName;
                }
                BadImageFormatException exception4 = exception as BadImageFormatException;
                if (exception4 != null)
                {
                    fusionLog = exception4.FusionLog;
                    fileName = exception4.FileName;
                }
                if (!string.IsNullOrEmpty(fusionLog))
                {
                    this.WriteColoredSquare(sb, System.Web.SR.GetString("Error_Formatter_FusionLog"), System.Web.SR.GetString("Error_Formatter_FusionLogDesc", new object[] { fileName }), HttpUtility.HtmlEncode(fusionLog), false);
                    return;
                }
            }
        }

        protected virtual StringCollection AdaptiveMiscContent
        {
            get
            {
                if (this._adaptiveMiscContent == null)
                {
                    this._adaptiveMiscContent = new StringCollection();
                }
                return this._adaptiveMiscContent;
            }
        }

        protected virtual StringCollection AdaptiveStackTrace
        {
            get
            {
                if (this._adaptiveStackTrace == null)
                {
                    this._adaptiveStackTrace = new StringCollection();
                }
                return this._adaptiveStackTrace;
            }
        }

        internal virtual bool CanBeShownToAllUsers
        {
            get
            {
                return false;
            }
        }

        protected virtual string ColoredSquare2Content
        {
            get
            {
                return null;
            }
        }

        protected virtual string ColoredSquare2Description
        {
            get
            {
                return null;
            }
        }

        protected virtual string ColoredSquare2Title
        {
            get
            {
                return null;
            }
        }

        protected virtual string ColoredSquareContent
        {
            get
            {
                return null;
            }
        }

        protected virtual string ColoredSquareDescription
        {
            get
            {
                return null;
            }
        }

        protected virtual string ColoredSquareTitle
        {
            get
            {
                return null;
            }
        }

        protected abstract string Description { get; }

        protected abstract string ErrorTitle { get; }

        protected virtual System.Exception Exception
        {
            get
            {
                return null;
            }
        }

        protected static bool IsTextRightToLeft
        {
            get
            {
                return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
            }
        }

        protected abstract string MiscSectionContent { get; }

        protected abstract string MiscSectionTitle { get; }

        protected virtual string PhysicalPath
        {
            get
            {
                return null;
            }
        }

        protected virtual string PostMessage
        {
            get
            {
                return null;
            }
        }

        protected abstract bool ShowSourceFileInfo { get; }

        protected virtual int SourceFileLineNumber
        {
            get
            {
                return 0;
            }
        }

        protected virtual string VirtualPath
        {
            get
            {
                return null;
            }
        }

        protected virtual bool WrapColoredSquareContentLines
        {
            get
            {
                return false;
            }
        }
    }
}

