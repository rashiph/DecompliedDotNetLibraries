namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public interface ITransformerConfigurationControl
    {
        event EventHandler Cancelled;

        event EventHandler Succeeded;
    }
}

