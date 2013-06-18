namespace System.Drawing.Design
{
    using System;

    public interface IToolboxUser
    {
        bool GetToolSupported(ToolboxItem tool);
        void ToolPicked(ToolboxItem tool);
    }
}

