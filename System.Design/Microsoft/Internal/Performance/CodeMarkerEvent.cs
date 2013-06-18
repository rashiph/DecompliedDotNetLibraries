namespace Microsoft.Internal.Performance
{
    using System;

    internal enum CodeMarkerEvent
    {
        perfFXBindEventDesignToCode = 0x1d57,
        perfFXCodeGenerationEnd = 0x1d65,
        perfFXCreateDesignerEnd = 0x1d6c,
        perfFXCreateDesignerStart = 0x1d6b,
        perfFXCreateDesignSurface = 0x1d6d,
        perfFXCreateDesignSurfaceEnd = 0x1d6e,
        perfFXCreateEditorEnd = 0x1d5f,
        perfFXCreateEditorStart = 0x1d5e,
        perfFXDeserializeEnd = 0x1d68,
        perfFXDeserializeStart = 0x1d67,
        perfFXDesignCreateComponentEnd = 0x1d4d,
        perfFXDesignElementHostDesignerSetChildEnd = 0x1d78,
        perfFXDesignFlushEnd = 0x1d55,
        perfFXDesignFlushStart = 0x1d54,
        perfFXDesignFromCodeToDesign = 0x1d53,
        perfFXDesignFromCodeToDesignStart = 0x1d52,
        perfFXDesignPBOnSelectionChanged = 0x1d76,
        perfFXDesignPBOnSelectionChangedEnd = 0x1d77,
        perfFXDesignPropertyBrowserCreate = 0x1d73,
        perfFXDesignPropertyBrowserCreateEnd = 0x1d74,
        perfFXDesignPropertyBrowserLoadState = 0x1d75,
        perfFXDesignPropertyBrowserPopulationEnd = 0x1d4f,
        perfFXDesignPropertyBrowserPopulationStart = 0x1d4e,
        perfFXDesignShowCode = 0x1d51,
        perfFXEmitMethodEnd = 0x1d63,
        perfFXFormatMethodEnd = 0x1d64,
        perfFXGenerateCodeTreeEnd = 0x1d59,
        perfFXGetDocumentType = 0x1d66,
        perfFXGetFileDocDataEnd = 0x1d6a,
        perfFXGetFileDocDataStart = 0x1d69,
        perfFXGetGlobalObjects = 0x1d71,
        perfFXGetGlobalObjectsEnd = 0x1d72,
        perfFXIntegrateSerializedTreeEnd = 0x1d5b,
        perfFXNotifyStartupServices = 0x1d6f,
        perfFXNotifyStartupServicesEnd = 0x1d70,
        perfFXOnLoadedEnd = 0x1d5d,
        perfFXOnLoadedStart = 0x1d5c,
        perfFXParseEnd = 0x1d60,
        perfFXPerformLoadEnd = 0x1d62,
        perfFXPerformLoadStart = 0x1d61
    }
}

