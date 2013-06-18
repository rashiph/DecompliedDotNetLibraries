namespace System.Web.UI.WebControls
{
    using System;

    public interface IButtonControl
    {
        event EventHandler Click;

        event CommandEventHandler Command;

        bool CausesValidation { get; set; }

        string CommandArgument { get; set; }

        string CommandName { get; set; }

        string PostBackUrl { get; set; }

        string Text { get; set; }

        string ValidationGroup { get; set; }
    }
}

