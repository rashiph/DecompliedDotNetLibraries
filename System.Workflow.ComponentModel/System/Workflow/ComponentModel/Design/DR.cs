namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Resources;

    internal static class DR
    {
        internal const string AccessibleAction = "AccessibleAction";
        internal const string Activities = "Activities";
        internal const string ActivitiesDesc = "ActivitiesDesc";
        internal const string Activity = "Activity";
        internal const string ActivityDesignerAccessibleDescription = "ActivityDesignerAccessibleDescription";
        internal const string ActivityDesignerAccessibleHelp = "ActivityDesignerAccessibleHelp";
        internal const string ActivityInsertError = "ActivityInsertError";
        internal const string ActivitySetDefaultFileName = "ActivitySetDefaultFileName";
        internal const string ActivitySetDefaultName = "ActivitySetDefaultName";
        internal const string ActivitySetMessageBoxTitle = "ActivitySetMessageBoxTitle";
        internal const string ActivitySetNoActivity = "ActivitySetNoActivity";
        internal const string ActivitySetNoName = "ActivitySetNoName";
        internal const string AddBranch = "AddBranch";
        internal const string AddingBranch = "AddingBranch";
        internal const string AmbientThemeException = "AmbientThemeException";
        internal const string ArgumentExceptionDesignerVerbIdsRange = "ArgumentExceptionDesignerVerbIdsRange";
        internal const string ArgumentExceptionSmartActionIdsRange = "ArgumentExceptionSmartActionIdsRange";
        internal const string ArrowLeft = "ArrowLeft";
        internal const string Bind = "Bind";
        internal const string BindProperty = "BindProperty";
        internal const string BindSelectedProperty = "BindSelectedProperty";
        internal const string BindSelectedPropertyFormat = "BindSelectedPropertyFormat";
        internal const string BMPImageFormat = "BMPImageFormat";
        internal const string BringToFront = "BringToFront";
        internal const string BuildTargetWorkflow = "BuildTargetWorkflow";
        internal const string ButtonInformationMissing = "ButtonInformationMissing";
        internal const string CancelDescriptionString = "CancelDescriptionString";
        internal const string CheckAll = "CheckAll";
        internal const string ChildActivitiesNotConfigured = "ChildActivitiesNotConfigured";
        internal const string Compensation = "Compensation";
        internal const string Complete = "Complete";
        internal const string ConfigError = "ConfigError";
        internal const string ConnectorAccessibleDescription = "ConnectorAccessibleDescription";
        internal const string ConnectorAccessibleHelp = "ConnectorAccessibleHelp";
        internal const string ConnectorDesc = "ConnectorDesc";
        internal const string ContentAlignmentNotSupported = "ContentAlignmentNotSupported";
        internal const string CreatePackageTitle = "CreatePackageTitle";
        internal const string CustomEventType = "CustomEventType";
        internal const string CustomPropertyType = "CustomPropertyType";
        internal const string CustomStyleNotSupported = "CustomStyleNotSupported";
        internal const string DeclarativeRules = "DeclarativeRules";
        internal const string DefaultTheme = "DefaultTheme";
        internal const string DefaultThemeDescription = "DefaultThemeDescription";
        internal const string DesignerNotInitialized = "DesignerNotInitialized";
        internal const string DesignerThemeException = "DesignerThemeException";
        internal const string DropActivitiesHere = "DropActivitiesHere";
        internal const string DropExceptionsHere = "DropExceptionsHere";
        internal const string DropShapeShort = "DropShapeShort";
        internal const string EditActivity = "EditActivity";
        internal const string EditMode = "EditMode";
        internal const string EditModeIcon = "EditModeIcon";
        internal const string EMFImageFormat = "EMFImageFormat";
        internal const string Empty = "Empty";
        internal const string EmptyFontFamilyNotSupported = "EmptyFontFamilyNotSupported";
        internal const string End = "End";
        internal const string EndWorkflow = "EndWorkflow";
        internal const string EnteredMarginsAreNotValidErrorMessage = "EnteredMarginsAreNotValidErrorMessage";
        internal const string Error_AddConnector1 = "Error_AddConnector1";
        internal const string Error_AddConnector2 = "Error_AddConnector2";
        internal const string Error_AddConnector3 = "Error_AddConnector3";
        internal const string Error_ConnectionPoint = "Error_ConnectionPoint";
        internal const string Error_Connector1 = "Error_Connector1";
        internal const string Error_Connector2 = "Error_Connector2";
        internal const string Error_FailedToDeserializeComponents = "Error_FailedToDeserializeComponents";
        internal const string Error_InvalidActivity = "Error_InvalidActivity";
        internal const string Error_InvalidImageResource = "Error_InvalidImageResource";
        internal const string Error_MultiviewSequentialActivityDesigner = "Error_MultiviewSequentialActivityDesigner";
        internal const string Error_Reason = "Error_Reason";
        internal const string Error_TabExistsWithSameId = "Error_TabExistsWithSameId";
        internal const string Error_ThemeAttributeMissing = "Error_ThemeAttributeMissing";
        internal const string Error_ThemeTypeMissing = "Error_ThemeTypeMissing";
        internal const string Error_ThemeTypesMismatch = "Error_ThemeTypesMismatch";
        internal const string Error_WorkflowLayoutNull = "Error_WorkflowLayoutNull";
        internal const string Error_WorkflowNotLoaded = "Error_WorkflowNotLoaded";
        internal const string ErrorInitPackage = "ErrorInitPackage";
        internal const string Event = "Event";
        internal const string EventsDesc = "EventsDesc";
        internal const string Exception = "Exception";
        internal const string EXIFImageFormat = "EXIFImageFormat";
        internal const string FilterDescription = "FilterDescription";
        internal const string FitToScreen = "FitToScreen";
        internal const string FitToScreenDescription = "FitToScreenDescription";
        internal const string FitToWorkflow = "FitToWorkflow";
        internal const string FitToWorkflowDescription = "FitToWorkflowDescription";
        internal const string FontFamilyNotSupported = "FontFamilyNotSupported";
        internal const string GenerateEventHandlers = "GenerateEventHandlers";
        internal const string HeaderFooterFormat1 = "HeaderFooterFormat1";
        internal const string HeaderFooterFormat2 = "HeaderFooterFormat2";
        internal const string HeaderFooterFormat3 = "HeaderFooterFormat3";
        internal const string HeaderFooterFormat4 = "HeaderFooterFormat4";
        internal const string HeaderFooterFormat5 = "HeaderFooterFormat5";
        internal const string HeaderFooterFormat6 = "HeaderFooterFormat6";
        internal const string HeaderFooterFormat7 = "HeaderFooterFormat7";
        internal const string HeaderFooterFormat8 = "HeaderFooterFormat8";
        internal const string HeaderFooterFormat9 = "HeaderFooterFormat9";
        internal const string HeaderFooterStringCustom = "HeaderFooterStringCustom";
        internal const string HeaderFooterStringNone = "HeaderFooterStringNone";
        internal const string ImageFileFilter = "ImageFileFilter";
        internal const string InfoTipDescription = "InfoTipDescription";
        internal const string InfoTipId = "InfoTipId";
        internal const string InfoTipTitle = "InfoTipTitle";
        internal const string InvalidBaseTypeOfCompanion = "InvalidBaseTypeOfCompanion";
        internal const string InvalidClassNameIdentifier = "InvalidClassNameIdentifier";
        internal const string InvalidDesignerSpecified = "InvalidDesignerSpecified";
        internal const string InvalidDockingStyle = "InvalidDockingStyle";
        internal const string InvalidOperationBadClipboardFormat = "InvalidOperationBadClipboardFormat";
        internal const string InvalidOperationDeserializationReturnedNonActivity = "InvalidOperationDeserializationReturnedNonActivity";
        internal const string InvalidOperationStoreAlreadyClosed = "InvalidOperationStoreAlreadyClosed";
        internal const string InvokeWebServiceDisplayName = "InvokeWebServiceDisplayName";
        internal const string JPEGImageFormat = "JPEGImageFormat";
        internal const string LeftScrollButtonAccessibleDescription = "LeftScrollButtonAccessibleDescription";
        internal const string LeftScrollButtonAccessibleHelp = "LeftScrollButtonAccessibleHelp";
        internal const string LeftScrollButtonName = "LeftScrollButtonName";
        internal const string ModifyPackageTitle = "ModifyPackageTitle";
        internal const string More = "More";
        internal const string MoveAnchor = "MoveAnchor";
        internal const string MoveLeft = "MoveLeft";
        internal const string MoveLeftDesc = "MoveLeftDesc";
        internal const string MoveLeftUp = "MoveLeftUp";
        internal const string MoveRight = "MoveRight";
        internal const string MoveRightDesc = "MoveRightDesc";
        internal const string MoveRightUp = "MoveRightUp";
        internal const string MyFavoriteTheme = "MyFavoriteTheme";
        internal const string NoHelpAvailable = "NoHelpAvailable";
        internal const string ObjectDoesNotSupportIPropertyValuesProvider = "ObjectDoesNotSupportIPropertyValuesProvider";
        internal const string OpenfileDialogTitle = "OpenfileDialogTitle";
        internal const string OSTheme = "OSTheme";
        internal const string PackageAssemblyReferenceFilter = "PackageAssemblyReferenceFilter";
        internal const string PackageFileDefault = "PackageFileDefault";
        internal const string PackageFileExist = "PackageFileExist";
        internal const string PackageFileInvalid = "PackageFileInvalid";
        internal const string PackageFileInvalidChars = "PackageFileInvalidChars";
        internal const string PackageInvalidValidatorType = "PackageInvalidValidatorType";
        internal const string PNGImageFormat = "PNGImageFormat";
        internal const string Preview = "Preview";
        internal const string PreviewActivity = "PreviewActivity";
        internal const string PreviewButtonAccessibleDescription = "PreviewButtonAccessibleDescription";
        internal const string PreviewButtonAccessibleHelp = "PreviewButtonAccessibleHelp";
        internal const string PreviewButtonName = "PreviewButtonName";
        internal const string PreviewIndicator = "PreviewIndicator";
        internal const string PreviewMode = "PreviewMode";
        internal const string PreviewModeIcon = "PreviewModeIcon";
        internal const string PromoteBindings = "PromoteBindings";
        internal const string ReadOnly = "ReadOnly";
        internal const string ResizeUndoDescription = "ResizeUndoDescription";
        private static ResourceManager resourceManager = new ResourceManager("System.Workflow.ComponentModel.Design.DesignerResources", Assembly.GetExecutingAssembly());
        internal const string ResourceSet = "System.Workflow.ComponentModel.Design.DesignerResources";
        internal const string RightScrollButtonAccessibleDescription = "RightScrollButtonAccessibleDescription";
        internal const string RightScrollButtonAccessibleHelp = "RightScrollButtonAccessibleHelp";
        internal const string RightScrollButtonName = "RightScrollButtonName";
        internal const string RuleExpression = "RuleExpression";
        internal const string RuleName = "RuleName";
        internal const string Rules = "Rules";
        internal const string SaveWorkflowImageDialogTitle = "SaveWorkflowImageDialogTitle";
        internal const string ScopeDesc = "ScopeDesc";
        internal const string SelectActivityDesc = "SelectActivityDesc";
        internal const string SelectedPrinterIsInvalidErrorMessage = "SelectedPrinterIsInvalidErrorMessage";
        internal const string SendToBack = "SendToBack";
        internal const string SequenceArrow = "SequenceArrow";
        internal const string ServiceCompensation = "ServiceCompensation";
        internal const string ServiceEvents = "ServiceEvents";
        internal const string ServiceExceptions = "ServiceExceptions";
        internal const string ServiceHelpText = "ServiceHelpText";
        internal const string ShadowDepthException = "ShadowDepthException";
        internal const string SmartTag = "SmartTag";
        internal const string SpecifyTargetWorkflow = "SpecifyTargetWorkflow";
        internal const string Start = "Start";
        internal const string StartWorkflow = "StartWorkflow";
        internal const string StartWorkFlow = "StartWorkFlow";
        internal const string SystemThemeDescription = "SystemThemeDescription";
        internal const string ThemeConfig = "ThemeConfig";
        internal const string ThemeDescription = "ThemeDescription";
        internal const string ThemeFileCreationError = "ThemeFileCreationError";
        internal const string ThemeFileFilter = "ThemeFileFilter";
        internal const string ThemeFileNotXml = "ThemeFileNotXml";
        internal const string ThemeMessageBoxTitle = "ThemeMessageBoxTitle";
        internal const string ThemeNameNotValid = "ThemeNameNotValid";
        internal const string ThemePathNotValid = "ThemePathNotValid";
        internal const string ThemePropertyReadOnly = "ThemePropertyReadOnly";
        internal const string ThemeTypesMismatch = "ThemeTypesMismatch";
        internal const string ThereIsNoPrinterInstalledErrorMessage = "ThereIsNoPrinterInstalledErrorMessage";
        internal const string TIFFImageFormat = "TIFFImageFormat";
        internal const string TypeBrowser_ProblemsLoadingAssembly = "TypeBrowser_ProblemsLoadingAssembly";
        internal const string TypeBrowser_UnableToLoadOneOrMoreTypes = "TypeBrowser_UnableToLoadOneOrMoreTypes";
        internal const string TypeInvalid = "TypeInvalid";
        internal const string UpdateRelativePaths = "UpdateRelativePaths";
        internal const string ViewActivity = "ViewActivity";
        internal const string ViewCancelHandler = "ViewCancelHandler";
        internal const string ViewCompensation = "ViewCompensation";
        internal const string ViewEvents = "ViewEvents";
        internal const string ViewExceptions = "ViewExceptions";
        internal const string ViewNextActivity = "ViewNextActivity";
        internal const string ViewPackageTitle = "ViewPackageTitle";
        internal const string ViewPreviousActivity = "ViewPreviousActivity";
        internal const string WMFImageFormat = "WMFImageFormat";
        internal const string WorkflowDesc = "WorkflowDesc";
        internal const string WorkflowDesignerTitle = "WorkflowDesignerTitle";
        internal const string WorkflowPrintDocumentNotFound = "WorkflowPrintDocumentNotFound";
        internal const string WorkflowViewAccessibleDescription = "WorkflowViewAccessibleDescription";
        internal const string WorkflowViewAccessibleHelp = "WorkflowViewAccessibleHelp";
        internal const string WorkflowViewAccessibleName = "WorkflowViewAccessibleName";
        internal const string WorkflowViewNull = "WorkflowViewNull";
        internal const string Zoom100Mode = "Zoom100Mode";
        internal const string Zoom150Mode = "Zoom150Mode";
        internal const string Zoom200Mode = "Zoom200Mode";
        internal const string Zoom300Mode = "Zoom300Mode";
        internal const string Zoom400Mode = "Zoom400Mode";
        internal const string Zoom50Mode = "Zoom50Mode";
        internal const string Zoom75Mode = "Zoom75Mode";
        internal const string ZoomLevelException2 = "ZoomLevelException2";
        internal const string ZoomShowAll = "ZoomShowAll";
        internal const string ZOrderUndoDescription = "ZOrderUndoDescription";

        internal static Image GetImage(string resID)
        {
            Image image = resourceManager.GetObject(resID) as Image;
            Bitmap bitmap = image as Bitmap;
            if (bitmap != null)
            {
                bitmap.MakeTransparent(AmbientTheme.TransparentColor);
            }
            return image;
        }

        internal static string GetString(string resID, params object[] args)
        {
            return GetString(CultureInfo.CurrentUICulture, resID, args);
        }

        internal static string GetString(CultureInfo culture, string resID, params object[] args)
        {
            string format = resourceManager.GetString(resID, culture);
            if ((args != null) && (args.Length > 0))
            {
                format = string.Format(culture, format, args);
            }
            return format;
        }
    }
}

