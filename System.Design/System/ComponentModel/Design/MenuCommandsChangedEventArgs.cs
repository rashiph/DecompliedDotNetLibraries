namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class MenuCommandsChangedEventArgs : EventArgs
    {
        private MenuCommandsChangedType changeType;
        private MenuCommand command;

        public MenuCommandsChangedEventArgs(MenuCommandsChangedType changeType, MenuCommand command)
        {
            this.changeType = changeType;
            this.command = command;
        }

        public MenuCommandsChangedType ChangeType
        {
            get
            {
                return this.changeType;
            }
        }

        public MenuCommand Command
        {
            get
            {
                return this.command;
            }
        }
    }
}

