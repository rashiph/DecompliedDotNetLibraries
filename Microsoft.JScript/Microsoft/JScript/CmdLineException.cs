namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class CmdLineException : Exception
    {
        private string context;
        private CultureInfo culture;
        private CmdLineError errorCode;
        private const int LOCALE_USER_DEFAULT = 0x400;

        public CmdLineException()
        {
        }

        public CmdLineException(string m) : base(m)
        {
        }

        public CmdLineException(CmdLineError errorCode, CultureInfo culture)
        {
            this.culture = culture;
            this.errorCode = errorCode;
        }

        protected CmdLineException(SerializationInfo s, StreamingContext c) : base(s, c)
        {
            this.errorCode = (CmdLineError) s.GetInt32("ErrorCode");
            this.context = s.GetString("Context");
            int culture = s.GetInt32("LCID");
            if (culture != 0x400)
            {
                this.culture = new CultureInfo(culture);
            }
        }

        public CmdLineException(string m, Exception e) : base(m, e)
        {
        }

        public CmdLineException(CmdLineError errorCode, string context, CultureInfo culture)
        {
            this.culture = culture;
            this.errorCode = errorCode;
            if (context != "")
            {
                this.context = context;
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo s, StreamingContext c)
        {
            base.GetObjectData(s, c);
            s.AddValue("ErrorCode", (int) this.errorCode);
            s.AddValue("Context", this.context);
            int lCID = 0x400;
            if (this.culture != null)
            {
                lCID = this.culture.LCID;
            }
            s.AddValue("LCID", lCID);
        }

        public string ResourceKey(CmdLineError errorCode)
        {
            switch (errorCode)
            {
                case CmdLineError.AssemblyNotFound:
                    return "Assembly not found";

                case CmdLineError.CannotCreateEngine:
                    return "Cannot create JScript engine";

                case CmdLineError.CompilerConstant:
                    return "Compiler constant";

                case CmdLineError.DuplicateFileAsSourceAndAssembly:
                    return "Duplicate file as source and assembly";

                case CmdLineError.DuplicateResourceFile:
                    return "Duplicate resource file";

                case CmdLineError.DuplicateResourceName:
                    return "Duplicate resource name";

                case CmdLineError.DuplicateSourceFile:
                    return "Duplicate source file";

                case CmdLineError.ErrorSavingCompiledState:
                    return "Error saving compiled state";

                case CmdLineError.InvalidAssembly:
                    return "Invalid assembly";

                case CmdLineError.InvalidCodePage:
                    return "Invalid code page";

                case CmdLineError.InvalidDefinition:
                    return "Invalid definition";

                case CmdLineError.InvalidLocaleID:
                    return "Invalid Locale ID";

                case CmdLineError.InvalidTarget:
                    return "Invalid target";

                case CmdLineError.InvalidSourceFile:
                    return "Invalid source file";

                case CmdLineError.InvalidWarningLevel:
                    return "Invalid warning level";

                case CmdLineError.MultipleOutputNames:
                    return "Multiple output filenames";

                case CmdLineError.MultipleTargets:
                    return "Multiple targets";

                case CmdLineError.MissingDefineArgument:
                    return "Missing define argument";

                case CmdLineError.MissingExtension:
                    return "Missing extension";

                case CmdLineError.MissingLibArgument:
                    return "Missing lib argument";

                case CmdLineError.ManagedResourceNotFound:
                    return "Managed resource not found";

                case CmdLineError.NestedResponseFiles:
                    return "Nested response files";

                case CmdLineError.NoCodePage:
                    return "No code page";

                case CmdLineError.NoFileName:
                    return "No filename";

                case CmdLineError.NoInputSourcesSpecified:
                    return "No input sources specified";

                case CmdLineError.NoLocaleID:
                    return "No Locale ID";

                case CmdLineError.NoWarningLevel:
                    return "No warning level";

                case CmdLineError.ResourceNotFound:
                    return "Resource not found";

                case CmdLineError.UnknownOption:
                    return "Unknown option";

                case CmdLineError.InvalidVersion:
                    return "Invalid version";

                case CmdLineError.SourceFileTooBig:
                    return "Source file too big";

                case CmdLineError.MultipleWin32Resources:
                    return "Multiple win32resources";

                case CmdLineError.MissingReference:
                    return "Missing reference";

                case CmdLineError.SourceNotFound:
                    return "Source not found";

                case CmdLineError.InvalidCharacters:
                    return "Invalid characters";

                case CmdLineError.InvalidForCompilerOptions:
                    return "Invalid for CompilerOptions";

                case CmdLineError.IncompatibleTargets:
                    return "Incompatible targets";

                case CmdLineError.InvalidPlatform:
                    return "Invalid platform";
            }
            return "No description available";
        }

        public override string Message
        {
            get
            {
                string str2 = JScriptException.Localize(this.ResourceKey(this.errorCode), this.context, this.culture);
                int num = 0x2710 + this.errorCode;
                string str3 = num.ToString(CultureInfo.InvariantCulture).Substring(1);
                return ("fatal error JS" + str3 + ": " + str2);
            }
        }
    }
}

