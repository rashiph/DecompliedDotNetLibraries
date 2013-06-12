namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IMenuCommandService
    {
        void AddCommand(MenuCommand command);
        void AddVerb(DesignerVerb verb);
        MenuCommand FindCommand(CommandID commandID);
        bool GlobalInvoke(CommandID commandID);
        void RemoveCommand(MenuCommand command);
        void RemoveVerb(DesignerVerb verb);
        void ShowContextMenu(CommandID menuID, int x, int y);

        DesignerVerbCollection Verbs { get; }
    }
}

