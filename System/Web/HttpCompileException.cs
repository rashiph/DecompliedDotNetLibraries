namespace System.Web
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web.Util;

    [Serializable]
    public sealed class HttpCompileException : HttpException
    {
        private bool _dontCache;
        private CompilerResults _results;
        private string _sourceCode;
        private ICollection _virtualPathDependencies;
        private const string compileErrorFormat = "{0}({1}): error {2}: {3}";

        public HttpCompileException()
        {
        }

        public HttpCompileException(string message) : base(message)
        {
        }

        public HttpCompileException(CompilerResults results, string sourceCode)
        {
            this._results = results;
            this._sourceCode = sourceCode;
            base.SetFormatter(new DynamicCompileErrorFormatter(this));
        }

        private HttpCompileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._results = (CompilerResults) info.GetValue("_results", typeof(CompilerResults));
            this._sourceCode = info.GetString("_sourceCode");
        }

        public HttpCompileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_results", this._results);
            info.AddValue("_sourceCode", this._sourceCode);
        }

        internal bool DontCache
        {
            get
            {
                return this._dontCache;
            }
            set
            {
                this._dontCache = value;
            }
        }

        internal CompilerError FirstCompileError
        {
            get
            {
                if ((this._results == null) || !this._results.Errors.HasErrors)
                {
                    return null;
                }
                CompilerError error = null;
                foreach (CompilerError error2 in this._results.Errors)
                {
                    if (!error2.IsWarning)
                    {
                        if (((HttpRuntime.CodegenDirInternal != null) && (error2.FileName != null)) && !StringUtil.StringStartsWith(error2.FileName, HttpRuntime.CodegenDirInternal))
                        {
                            return error2;
                        }
                        if (error == null)
                        {
                            error = error2;
                        }
                    }
                }
                return error;
            }
        }

        public override string Message
        {
            get
            {
                CompilerError firstCompileError = this.FirstCompileError;
                if (firstCompileError == null)
                {
                    return base.Message;
                }
                return string.Format(CultureInfo.CurrentCulture, "{0}({1}): error {2}: {3}", new object[] { firstCompileError.FileName, firstCompileError.Line, firstCompileError.ErrorNumber, firstCompileError.ErrorText });
            }
        }

        public CompilerResults Results
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get
            {
                return this._results;
            }
        }

        internal CompilerResults ResultsWithoutDemand
        {
            get
            {
                return this._results;
            }
        }

        public string SourceCode
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get
            {
                return this._sourceCode;
            }
        }

        internal string SourceCodeWithoutDemand
        {
            get
            {
                return this._sourceCode;
            }
        }

        internal ICollection VirtualPathDependencies
        {
            get
            {
                return this._virtualPathDependencies;
            }
            set
            {
                this._virtualPathDependencies = value;
            }
        }
    }
}

