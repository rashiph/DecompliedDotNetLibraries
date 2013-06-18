namespace System.Web.UI.Design
{
    using System;
    using System.Collections;

    public interface IContentResolutionService
    {
        ContentDesignerState GetContentDesignerState(string identifier);
        void SetContentDesignerState(string identifier, ContentDesignerState state);

        IDictionary ContentDefinitions { get; }
    }
}

