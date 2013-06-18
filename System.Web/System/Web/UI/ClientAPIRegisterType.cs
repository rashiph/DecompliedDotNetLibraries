namespace System.Web.UI
{
    using System;

    internal enum ClientAPIRegisterType
    {
        WebFormsScript,
        PostBackScript,
        FocusScript,
        ClientScriptBlocks,
        ClientScriptBlocksWithoutTags,
        ClientStartupScripts,
        ClientStartupScriptsWithoutTags,
        OnSubmitStatement,
        ArrayDeclaration,
        HiddenField,
        ExpandoAttribute,
        EventValidation
    }
}

