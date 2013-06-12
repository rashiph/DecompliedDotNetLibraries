namespace System.Drawing
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string CantChangeImmutableObjects = "CantChangeImmutableObjects";
        internal const string CantMakeIconTransparent = "CantMakeIconTransparent";
        internal const string CantTellPrinterName = "CantTellPrinterName";
        internal const string ColorNotSystemColor = "ColorNotSystemColor";
        internal const string DCTypeInvalid = "DCTypeInvalid";
        internal const string DotNET_ComponentType = "DotNET_ComponentType";
        internal const string GdiplusAborted = "GdiplusAborted";
        internal const string GdiplusAccessDenied = "GdiplusAccessDenied";
        internal const string GdiplusCannotCreateGraphicsFromIndexedPixelFormat = "GdiplusCannotCreateGraphicsFromIndexedPixelFormat";
        internal const string GdiplusCannotSetPixelFromIndexedPixelFormat = "GdiplusCannotSetPixelFromIndexedPixelFormat";
        internal const string GdiplusDestPointsInvalidLength = "GdiplusDestPointsInvalidLength";
        internal const string GdiplusDestPointsInvalidParallelogram = "GdiplusDestPointsInvalidParallelogram";
        internal const string GdiplusFileNotFound = "GdiplusFileNotFound";
        internal const string GdiplusFontFamilyNotFound = "GdiplusFontFamilyNotFound";
        internal const string GdiplusFontStyleNotFound = "GdiplusFontStyleNotFound";
        internal const string GdiplusGenericError = "GdiplusGenericError";
        internal const string GdiplusInsufficientBuffer = "GdiplusInsufficientBuffer";
        internal const string GdiplusInvalidParameter = "GdiplusInvalidParameter";
        internal const string GdiplusInvalidRectangle = "GdiplusInvalidRectangle";
        internal const string GdiplusInvalidSize = "GdiplusInvalidSize";
        internal const string GdiplusNotImplemented = "GdiplusNotImplemented";
        internal const string GdiplusNotInitialized = "GdiplusNotInitialized";
        internal const string GdiplusNotTrueTypeFont = "GdiplusNotTrueTypeFont";
        internal const string GdiplusNotTrueTypeFont_NoName = "GdiplusNotTrueTypeFont_NoName";
        internal const string GdiplusObjectBusy = "GdiplusObjectBusy";
        internal const string GdiplusOutOfMemory = "GdiplusOutOfMemory";
        internal const string GdiplusOverflow = "GdiplusOverflow";
        internal const string GdiplusPropertyNotFoundError = "GdiplusPropertyNotFoundError";
        internal const string GdiplusPropertyNotSupportedError = "GdiplusPropertyNotSupportedError";
        internal const string GdiplusUnknown = "GdiplusUnknown";
        internal const string GdiplusUnknownImageFormat = "GdiplusUnknownImageFormat";
        internal const string GdiplusUnsupportedGdiplusVersion = "GdiplusUnsupportedGdiplusVersion";
        internal const string GdiplusWrongState = "GdiplusWrongState";
        internal const string GlobalAssemblyCache = "GlobalAssemblyCache";
        internal const string GraphicsBufferCurrentlyBusy = "GraphicsBufferCurrentlyBusy";
        internal const string GraphicsBufferQueryFail = "GraphicsBufferQueryFail";
        internal const string IllegalState = "IllegalState";
        internal const string InterpolationColorsColorBlendNotSet = "InterpolationColorsColorBlendNotSet";
        internal const string InterpolationColorsCommon = "InterpolationColorsCommon";
        internal const string InterpolationColorsInvalidColorBlendObject = "InterpolationColorsInvalidColorBlendObject";
        internal const string InterpolationColorsInvalidEndPosition = "InterpolationColorsInvalidEndPosition";
        internal const string InterpolationColorsInvalidStartPosition = "InterpolationColorsInvalidStartPosition";
        internal const string InterpolationColorsLength = "InterpolationColorsLength";
        internal const string InterpolationColorsLengthsDiffer = "InterpolationColorsLengthsDiffer";
        internal const string InvalidArgument = "InvalidArgument";
        internal const string InvalidBoundArgument = "InvalidBoundArgument";
        internal const string InvalidClassName = "InvalidClassName";
        internal const string InvalidColor = "InvalidColor";
        internal const string InvalidDashPattern = "InvalidDashPattern";
        internal const string InvalidEx2BoundArgument = "InvalidEx2BoundArgument";
        internal const string InvalidFrame = "InvalidFrame";
        internal const string InvalidGDIHandle = "InvalidGDIHandle";
        internal const string InvalidImage = "InvalidImage";
        internal const string InvalidLowBoundArgumentEx = "InvalidLowBoundArgumentEx";
        internal const string InvalidPermissionLevel = "InvalidPermissionLevel";
        internal const string InvalidPermissionState = "InvalidPermissionState";
        internal const string InvalidPictureType = "InvalidPictureType";
        internal const string InvalidPrinterException_InvalidPrinter = "InvalidPrinterException_InvalidPrinter";
        internal const string InvalidPrinterException_NoDefaultPrinter = "InvalidPrinterException_NoDefaultPrinter";
        internal const string InvalidPrinterHandle = "InvalidPrinterHandle";
        private static System.Drawing.SR loader;
        internal const string NativeHandle0 = "NativeHandle0";
        internal const string NoDefaultPrinter = "NoDefaultPrinter";
        internal const string NotImplemented = "NotImplemented";
        internal const string PDOCbeginPrintDescr = "PDOCbeginPrintDescr";
        internal const string PDOCdocumentNameDescr = "PDOCdocumentNameDescr";
        internal const string PDOCdocumentPageSettingsDescr = "PDOCdocumentPageSettingsDescr";
        internal const string PDOCendPrintDescr = "PDOCendPrintDescr";
        internal const string PDOCoriginAtMarginsDescr = "PDOCoriginAtMarginsDescr";
        internal const string PDOCprintControllerDescr = "PDOCprintControllerDescr";
        internal const string PDOCprinterSettingsDescr = "PDOCprinterSettingsDescr";
        internal const string PDOCprintPageDescr = "PDOCprintPageDescr";
        internal const string PDOCqueryPageSettingsDescr = "PDOCqueryPageSettingsDescr";
        internal const string PrintDocumentDesc = "PrintDocumentDesc";
        internal const string PrintingPermissionAttributeInvalidPermissionLevel = "PrintingPermissionAttributeInvalidPermissionLevel";
        internal const string PrintingPermissionBadXml = "PrintingPermissionBadXml";
        internal const string PropertyValueInvalidEntry = "PropertyValueInvalidEntry";
        internal const string PSizeNotCustom = "PSizeNotCustom";
        internal const string ResourceNotFound = "ResourceNotFound";
        private ResourceManager resources;
        internal const string TargetNotPrintingPermission = "TargetNotPrintingPermission";
        internal const string TextParseFailedFormat = "TextParseFailedFormat";
        internal const string ToolboxItemInvalidKey = "ToolboxItemInvalidKey";
        internal const string ToolboxItemInvalidPropertyType = "ToolboxItemInvalidPropertyType";
        internal const string ToolboxItemLocked = "ToolboxItemLocked";
        internal const string ToolboxItemValueNotSerializable = "ToolboxItemValueNotSerializable";
        internal const string toStringIcon = "toStringIcon";
        internal const string toStringNone = "toStringNone";
        internal const string TriStateCompareError = "TriStateCompareError";
        internal const string ValidRangeX = "ValidRangeX";
        internal const string ValidRangeY = "ValidRangeY";

        internal SR()
        {
            this.resources = new ResourceManager("System.Drawing.Res", base.GetType().Assembly);
        }

        private static System.Drawing.SR GetLoader()
        {
            if (loader == null)
            {
                System.Drawing.SR sr = new System.Drawing.SR();
                Interlocked.CompareExchange<System.Drawing.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.Drawing.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.Drawing.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.Drawing.SR loader = GetLoader();
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

