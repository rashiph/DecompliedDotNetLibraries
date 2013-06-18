namespace System.Management
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string ASSEMBLY_NOT_REGISTERED = "ASSEMBLY_NOT_REGISTERED";
        internal const string CLASS_ENSURE = "CLASS_ENSURE";
        internal const string CLASS_ENSURECREATE = "CLASS_ENSURECREATE";
        internal const string CLASS_NOTREPLACED_EXCEPT = "CLASS_NOTREPLACED_EXCEPT";
        internal const string CLASSINST_EXCEPT = "CLASSINST_EXCEPT";
        internal const string CLASSNAME_NOTINIT_EXCEPT = "CLASSNAME_NOTINIT_EXCEPT";
        internal const string CLASSNOT_FOUND_EXCEPT = "CLASSNOT_FOUND_EXCEPT";
        internal const string COMMENT_ATTRIBPROP = "COMMENT_ATTRIBPROP";
        internal const string COMMENT_AUTOCOMMITPROP = "COMMENT_AUTOCOMMITPROP";
        internal const string COMMENT_CLASSBEGIN = "COMMENT_CLASSBEGIN";
        internal const string COMMENT_CLASSNAME = "COMMENT_CLASSNAME";
        internal const string COMMENT_CONSTRUCTORS = "COMMENT_CONSTRUCTORS";
        internal const string COMMENT_CREATEDCLASS = "COMMENT_CREATEDCLASS";
        internal const string COMMENT_CREATEDWMINAMESPACE = "COMMENT_CREATEDWMINAMESPACE";
        internal const string COMMENT_CURRENTOBJ = "COMMENT_CURRENTOBJ";
        internal const string COMMENT_DATECONVFUNC = "COMMENT_DATECONVFUNC";
        internal const string COMMENT_EMBEDDEDOBJ = "COMMENT_EMBEDDEDOBJ";
        internal const string COMMENT_ENUMIMPL = "COMMENT_ENUMIMPL";
        internal const string COMMENT_FLAGFOREMBEDDED = "COMMENT_FLAGFOREMBEDDED";
        internal const string COMMENT_GETINSTANCES = "COMMENT_GETINSTANCES";
        internal const string COMMENT_ISPROPNULL = "COMMENT_ISPROPNULL";
        internal const string COMMENT_LATEBOUNDOBJ = "COMMENT_LATEBOUNDOBJ";
        internal const string COMMENT_LATEBOUNDPROP = "COMMENT_LATEBOUNDPROP";
        internal const string COMMENT_MGMTPATH = "COMMENT_MGMTPATH";
        internal const string COMMENT_MGMTSCOPE = "COMMENT_MGMTSCOPE";
        internal const string COMMENT_ORIGNAMESPACE = "COMMENT_ORIGNAMESPACE";
        internal const string COMMENT_PRIVAUTOCOMMIT = "COMMENT_PRIVAUTOCOMMIT";
        internal const string COMMENT_PROPTYPECONVERTER = "COMMENT_PROPTYPECONVERTER";
        internal const string COMMENT_RESETPROP = "COMMENT_RESETPROP";
        internal const string COMMENT_SHOULDSERIALIZE = "COMMENT_SHOULDSERIALIZE";
        internal const string COMMENT_STATICMANAGEMENTSCOPE = "COMMENT_STATICMANAGEMENTSCOPE";
        internal const string COMMENT_STATICSCOPEPROPERTY = "COMMENT_STATICSCOPEPROPERTY";
        internal const string COMMENT_SYSOBJECT = "COMMENT_SYSOBJECT";
        internal const string COMMENT_SYSPROPCLASS = "COMMENT_SYSPROPCLASS";
        internal const string COMMENT_TIMESPANCONVFUNC = "COMMENT_TIMESPANCONVFUNC";
        internal const string COMMENT_TODATETIME = "COMMENT_TODATETIME";
        internal const string COMMENT_TODMTFDATETIME = "COMMENT_TODMTFDATETIME";
        internal const string COMMENT_TODMTFTIMEINTERVAL = "COMMENT_TODMTFTIMEINTERVAL";
        internal const string COMMENT_TOTIMESPAN = "COMMENT_TOTIMESPAN";
        internal const string EMBEDDED_COMMENT1 = "EMBEDDED_COMMENT1";
        internal const string EMBEDDED_COMMENT2 = "EMBEDDED_COMMENT2";
        internal const string EMBEDDED_COMMENT3 = "EMBEDDED_COMMENT3";
        internal const string EMBEDDED_COMMENT4 = "EMBEDDED_COMMENT4";
        internal const string EMBEDDED_COMMENT5 = "EMBEDDED_COMMENT5";
        internal const string EMBEDDED_COMMENT6 = "EMBEDDED_COMMENT6";
        internal const string EMBEDDED_COMMENT7 = "EMBEDDED_COMMENT7";
        internal const string EMBEDDED_COMMENT8 = "EMBEDDED_COMMENT8";
        internal const string EMBEDED_CS_CODESAMP4 = "EMBEDED_CS_CODESAMP4";
        internal const string EMBEDED_CS_CODESAMP5 = "EMBEDED_CS_CODESAMP5";
        internal const string EMBEDED_VB_CODESAMP4 = "EMBEDED_VB_CODESAMP4";
        internal const string EMBEDED_VB_CODESAMP5 = "EMBEDED_VB_CODESAMP5";
        internal const string EMPTY_FILEPATH_EXCEPT = "EMPTY_FILEPATH_EXCEPT";
        internal const string FAILED_TO_BUILD_GENERATED_ASSEMBLY = "FAILED_TO_BUILD_GENERATED_ASSEMBLY";
        internal const string FILETOWRITE_MOF = "FILETOWRITE_MOF";
        internal const string FORCE_UPDATE = "FORCE_UPDATE";
        internal const string INVALID_QUERY = "INVALID_QUERY";
        internal const string INVALID_QUERY_DUP_TOKEN = "INVALID_QUERY_DUP_TOKEN";
        internal const string INVALID_QUERY_NULL_TOKEN = "INVALID_QUERY_NULL_TOKEN";
        private static System.Management.SR loader;
        internal const string MEMBERCONFLILCT_EXCEPT = "MEMBERCONFLILCT_EXCEPT";
        internal const string MOFFILE_GENERATING = "MOFFILE_GENERATING";
        internal const string NAMESPACE_ENSURE = "NAMESPACE_ENSURE";
        internal const string NAMESPACE_NOTINIT_EXCEPT = "NAMESPACE_NOTINIT_EXCEPT";
        internal const string NONCLS_COMPLIANT_EXCEPTION = "NONCLS_COMPLIANT_EXCEPTION";
        internal const string NULLFILEPATH_EXCEPT = "NULLFILEPATH_EXCEPT";
        internal const string REGESTRING_ASSEMBLY = "REGESTRING_ASSEMBLY";
        private ResourceManager resources;
        internal const string UNABLE_TOCREATE_GEN_EXCEPT = "UNABLE_TOCREATE_GEN_EXCEPT";
        internal const string UNSUPPORTEDMEMBER_EXCEPT = "UNSUPPORTEDMEMBER_EXCEPT";
        internal const string WMISCHEMA_INSTALLATIONEND = "WMISCHEMA_INSTALLATIONEND";
        internal const string WMISCHEMA_INSTALLATIONSTART = "WMISCHEMA_INSTALLATIONSTART";
        internal const string WORKER_THREAD_WAKEUP_FAILED = "WORKER_THREAD_WAKEUP_FAILED";

        internal SR()
        {
            this.resources = new ResourceManager("System.Management", base.GetType().Assembly);
        }

        private static System.Management.SR GetLoader()
        {
            if (loader == null)
            {
                System.Management.SR sr = new System.Management.SR();
                Interlocked.CompareExchange<System.Management.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.Management.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.Management.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.Management.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

