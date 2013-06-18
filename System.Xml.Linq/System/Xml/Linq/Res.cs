namespace System.Xml.Linq
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string Argument_AddAttribute = "Argument_AddAttribute";
        internal const string Argument_AddNode = "Argument_AddNode";
        internal const string Argument_AddNonWhitespace = "Argument_AddNonWhitespace";
        internal const string Argument_ConvertToString = "Argument_ConvertToString";
        internal const string Argument_CreateNavigator = "Argument_CreateNavigator";
        internal const string Argument_InvalidExpandedName = "Argument_InvalidExpandedName";
        internal const string Argument_InvalidPIName = "Argument_InvalidPIName";
        internal const string Argument_InvalidPrefix = "Argument_InvalidPrefix";
        internal const string Argument_MustBeDerivedFrom = "Argument_MustBeDerivedFrom";
        internal const string Argument_NamespaceDeclarationPrefixed = "Argument_NamespaceDeclarationPrefixed";
        internal const string Argument_NamespaceDeclarationXml = "Argument_NamespaceDeclarationXml";
        internal const string Argument_NamespaceDeclarationXmlns = "Argument_NamespaceDeclarationXmlns";
        internal const string Argument_XObjectValue = "Argument_XObjectValue";
        internal const string InvalidOperation_BadNodeType = "InvalidOperation_BadNodeType";
        internal const string InvalidOperation_DeserializeInstance = "InvalidOperation_DeserializeInstance";
        internal const string InvalidOperation_DocumentStructure = "InvalidOperation_DocumentStructure";
        internal const string InvalidOperation_DuplicateAttribute = "InvalidOperation_DuplicateAttribute";
        internal const string InvalidOperation_ExpectedEndOfFile = "InvalidOperation_ExpectedEndOfFile";
        internal const string InvalidOperation_ExpectedInteractive = "InvalidOperation_ExpectedInteractive";
        internal const string InvalidOperation_ExpectedNodeType = "InvalidOperation_ExpectedNodeType";
        internal const string InvalidOperation_ExternalCode = "InvalidOperation_ExternalCode";
        internal const string InvalidOperation_MissingAncestor = "InvalidOperation_MissingAncestor";
        internal const string InvalidOperation_MissingParent = "InvalidOperation_MissingParent";
        internal const string InvalidOperation_MissingRoot = "InvalidOperation_MissingRoot";
        internal const string InvalidOperation_UnexpectedEvaluation = "InvalidOperation_UnexpectedEvaluation";
        internal const string InvalidOperation_UnexpectedNodeType = "InvalidOperation_UnexpectedNodeType";
        internal const string InvalidOperation_UnresolvedEntityReference = "InvalidOperation_UnresolvedEntityReference";
        internal const string InvalidOperation_WriteAttribute = "InvalidOperation_WriteAttribute";
        private static Res loader;
        internal const string NotSupported_CheckValidity = "NotSupported_CheckValidity";
        internal const string NotSupported_MoveToId = "NotSupported_MoveToId";
        internal const string NotSupported_WriteBase64 = "NotSupported_WriteBase64";
        internal const string NotSupported_WriteEntityRef = "NotSupported_WriteEntityRef";
        private ResourceManager resources;

        internal Res()
        {
            this.resources = new ResourceManager("System.Xml.Linq", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (loader == null)
            {
                Res res = new Res();
                Interlocked.CompareExchange<Res>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Res loader = GetLoader();
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

