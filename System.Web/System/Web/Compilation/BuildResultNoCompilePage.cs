namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI;

    internal class BuildResultNoCompilePage : BuildResultNoCompileTemplateControl
    {
        private string[] _fileDependencies;
        private OutputCacheParameters _outputCacheData;
        private string _stylesheetTheme;
        private TraceEnable _traceEnabled;
        private TraceMode _traceMode;
        private bool _validateRequest;

        internal BuildResultNoCompilePage(Type baseType, TemplateParser parser) : base(baseType, parser)
        {
            PageParser parser2 = (PageParser) parser;
            this._traceEnabled = parser2.TraceEnabled;
            this._traceMode = parser2.TraceMode;
            if (parser2.OutputCacheParameters != null)
            {
                this._outputCacheData = parser2.OutputCacheParameters;
                if ((this._outputCacheData.Duration == 0) || (this._outputCacheData.Location == OutputCacheLocation.None))
                {
                    this._outputCacheData = null;
                }
                else
                {
                    this._fileDependencies = new string[parser2.SourceDependencies.Count];
                    int num = 0;
                    foreach (string str in (IEnumerable) parser2.SourceDependencies)
                    {
                        this._fileDependencies[num++] = str;
                    }
                }
            }
            this._validateRequest = parser2.ValidateRequest;
            this._stylesheetTheme = parser2.StyleSheetTheme;
        }

        internal override void FrameworkInitialize(TemplateControl templateControl)
        {
            Page page = (Page) templateControl;
            page.StyleSheetTheme = this._stylesheetTheme;
            page.InitializeStyleSheet();
            base.FrameworkInitialize(templateControl);
            if (this._traceEnabled != TraceEnable.Default)
            {
                page.TraceEnabled = this._traceEnabled == TraceEnable.Enable;
            }
            if (this._traceMode != TraceMode.Default)
            {
                page.TraceModeValue = this._traceMode;
            }
            if (this._outputCacheData != null)
            {
                page.AddWrappedFileDependencies(this._fileDependencies);
                page.InitOutputCache(this._outputCacheData);
            }
            if (this._validateRequest)
            {
                page.Request.ValidateInput();
            }
        }
    }
}

