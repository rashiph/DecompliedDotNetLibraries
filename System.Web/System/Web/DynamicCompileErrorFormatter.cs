namespace System.Web
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;

    internal class DynamicCompileErrorFormatter : ErrorFormatter
    {
        private HttpCompileException _excep;
        protected bool _hideDetailedCompilerOutput;
        private int _sourceFileLineNumber;
        private string _sourceFilePath;
        private const string endExpandableBlock = "</pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            \r\n\r\n</div>\r\n";
        private const int errorRange = 2;
        private const string startExpandableBlock = "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">{1}:</div>\r\n<div id=\"{0}\" style=\"display: none;\">\r\n            <br><table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n\r\n";

        internal DynamicCompileErrorFormatter(HttpCompileException excep)
        {
            this._excep = excep;
        }

        protected override string Description
        {
            get
            {
                return System.Web.SR.GetString("TmplCompilerErrorDesc");
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return System.Web.SR.GetString("TmplCompilerErrorTitle");
            }
        }

        protected override System.Exception Exception
        {
            get
            {
                return this._excep;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                StringBuilder builder = new StringBuilder(0x80);
                CompilerResults resultsWithoutDemand = this._excep.ResultsWithoutDemand;
                if ((resultsWithoutDemand.Errors.Count == 0) && (resultsWithoutDemand.NativeCompilerReturnValue != 0))
                {
                    string str = System.Web.SR.GetString("TmplCompilerFatalError", new object[] { resultsWithoutDemand.NativeCompilerReturnValue.ToString("G", CultureInfo.CurrentCulture) });
                    this.AdaptiveMiscContent.Add(str);
                    builder.Append(str);
                    builder.Append("<br><br>\r\n");
                }
                if (resultsWithoutDemand.Errors.HasErrors)
                {
                    CompilerError firstCompileError = this._excep.FirstCompileError;
                    if (firstCompileError != null)
                    {
                        string str2 = HttpUtility.HtmlEncode(firstCompileError.ErrorNumber);
                        string str3 = str2;
                        builder.Append(str2);
                        if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                        {
                            str2 = HttpUtility.HtmlEncode(firstCompileError.ErrorText);
                            builder.Append(": ");
                            builder.Append(str2);
                            str3 = str3 + ": " + str2;
                        }
                        this.AdaptiveMiscContent.Add(str3);
                        builder.Append("<br><br>\r\n");
                        builder.Append("<b>");
                        builder.Append(System.Web.SR.GetString("TmplCompilerSourceSecTitle"));
                        builder.Append(":</b><br><br>\r\n");
                        builder.Append("            <table width=100% bgcolor=\"#ffffcc\">\r\n");
                        builder.Append("               <tr><td>\r\n");
                        builder.Append("               ");
                        builder.Append("               </td></tr>\r\n");
                        builder.Append("               <tr>\r\n");
                        builder.Append("                  <td>\r\n");
                        builder.Append("                      <code><pre>\r\n\r\n");
                        builder.Append(FormatterWithFileInfo.GetSourceFileLines(firstCompileError.FileName, Encoding.Default, this._excep.SourceCodeWithoutDemand, firstCompileError.Line));
                        builder.Append("</pre></code>\r\n\r\n");
                        builder.Append("                  </td>\r\n");
                        builder.Append("               </tr>\r\n");
                        builder.Append("            </table>\r\n\r\n");
                        builder.Append("            <br>\r\n\r\n");
                        builder.Append("            <b>");
                        builder.Append(System.Web.SR.GetString("TmplCompilerSourceFileTitle"));
                        builder.Append(":</b> ");
                        this._sourceFilePath = ErrorFormatter.GetSafePath(firstCompileError.FileName);
                        builder.Append(HttpUtility.HtmlEncode(this._sourceFilePath));
                        builder.Append("\r\n");
                        TypeConverter converter = new Int32Converter();
                        builder.Append("            &nbsp;&nbsp; <b>");
                        builder.Append(System.Web.SR.GetString("TmplCompilerSourceFileLine"));
                        builder.Append(":</b>  ");
                        this._sourceFileLineNumber = firstCompileError.Line;
                        builder.Append(HttpUtility.HtmlEncode(converter.ConvertToString(this._sourceFileLineNumber)));
                        builder.Append("\r\n");
                        builder.Append("            <br><br>\r\n");
                    }
                }
                if (resultsWithoutDemand.Errors.HasWarnings)
                {
                    builder.Append("<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('warningDiv')\">");
                    builder.Append(System.Web.SR.GetString("TmplCompilerWarningBanner"));
                    builder.Append(":</div>\r\n");
                    builder.Append("<div id=\"warningDiv\" style=\"display: none;\">\r\n");
                    foreach (CompilerError error2 in resultsWithoutDemand.Errors)
                    {
                        if (error2.IsWarning)
                        {
                            builder.Append("<b>");
                            builder.Append(System.Web.SR.GetString("TmplCompilerWarningSecTitle"));
                            builder.Append(":</b> ");
                            builder.Append(HttpUtility.HtmlEncode(error2.ErrorNumber));
                            if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                            {
                                builder.Append(": ");
                                builder.Append(HttpUtility.HtmlEncode(error2.ErrorText));
                            }
                            builder.Append("<br>\r\n");
                            builder.Append("<b>");
                            builder.Append(System.Web.SR.GetString("TmplCompilerSourceSecTitle"));
                            builder.Append(":</b><br><br>\r\n");
                            builder.Append("            <table width=100% bgcolor=\"#ffffcc\">\r\n");
                            builder.Append("               <tr><td>\r\n");
                            builder.Append("               <b>");
                            builder.Append(HttpUtility.HtmlEncode(HttpRuntime.GetSafePath(error2.FileName)));
                            builder.Append("</b>\r\n");
                            builder.Append("               </td></tr>\r\n");
                            builder.Append("               <tr>\r\n");
                            builder.Append("                  <td>\r\n");
                            builder.Append("                      <code><pre>\r\n\r\n");
                            builder.Append(FormatterWithFileInfo.GetSourceFileLines(error2.FileName, Encoding.Default, this._excep.SourceCodeWithoutDemand, error2.Line));
                            builder.Append("</pre></code>\r\n\r\n");
                            builder.Append("                  </td>\r\n");
                            builder.Append("               </tr>\r\n");
                            builder.Append("            </table>\r\n\r\n");
                            builder.Append("            <br>\r\n\r\n");
                        }
                    }
                    builder.Append("</div>\r\n");
                }
                if (!this._hideDetailedCompilerOutput)
                {
                    if ((resultsWithoutDemand.Output.Count > 0) && HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                    {
                        builder.Append(string.Format(CultureInfo.CurrentCulture, "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">{1}:</div>\r\n<div id=\"{0}\" style=\"display: none;\">\r\n            <br><table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n\r\n", new object[] { "compilerOutputDiv", System.Web.SR.GetString("TmplCompilerCompleteOutput") }));
                        foreach (string str4 in resultsWithoutDemand.Output)
                        {
                            builder.Append(HttpUtility.HtmlEncode(str4));
                            builder.Append("\r\n");
                        }
                        builder.Append("</pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            \r\n\r\n</div>\r\n");
                    }
                    if ((this._excep.SourceCodeWithoutDemand != null) && HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                    {
                        builder.Append(string.Format(CultureInfo.CurrentCulture, "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">{1}:</div>\r\n<div id=\"{0}\" style=\"display: none;\">\r\n            <br><table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n\r\n", new object[] { "dynamicCodeDiv", System.Web.SR.GetString("TmplCompilerGeneratedFile") }));
                        string[] strArray = this._excep.SourceCodeWithoutDemand.Split(new char[] { '\n' });
                        int num = 1;
                        foreach (string str5 in strArray)
                        {
                            string str6 = num.ToString("G", CultureInfo.CurrentCulture);
                            builder.Append(System.Web.SR.GetString("TmplCompilerLineHeader", new object[] { str6 }));
                            if (str6.Length < 5)
                            {
                                builder.Append(' ', 5 - str6.Length);
                            }
                            num++;
                            builder.Append(HttpUtility.HtmlEncode(str5));
                        }
                        builder.Append("</pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            \r\n\r\n</div>\r\n");
                    }
                    builder.Append("\r\n    <script type=\"text/javascript\">\r\n    function OnToggleTOCLevel1(level2ID)\r\n    {\r\n      var elemLevel2 = document.getElementById(level2ID);\r\n      if (elemLevel2.style.display == 'none')\r\n      {\r\n        elemLevel2.style.display = '';\r\n      }\r\n      else {\r\n        elemLevel2.style.display = 'none';\r\n      }\r\n    }\r\n    </script>\r\n                          ");
                }
                return builder.ToString();
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("TmplCompilerErrorSecTitle");
            }
        }

        protected override string PhysicalPath
        {
            get
            {
                return this._sourceFilePath;
            }
        }

        protected override bool ShowSourceFileInfo
        {
            get
            {
                return false;
            }
        }

        protected override int SourceFileLineNumber
        {
            get
            {
                return this._sourceFileLineNumber;
            }
        }
    }
}

