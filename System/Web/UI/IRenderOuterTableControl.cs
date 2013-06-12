namespace System.Web.UI
{
    using System;

    internal interface IRenderOuterTableControl
    {
        string ID { get; }

        bool RenderOuterTable { get; set; }
    }
}

