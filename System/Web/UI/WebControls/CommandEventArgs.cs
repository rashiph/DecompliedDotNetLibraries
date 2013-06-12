namespace System.Web.UI.WebControls
{
    using System;

    public class CommandEventArgs : EventArgs
    {
        private object argument;
        private string commandName;

        public CommandEventArgs(CommandEventArgs e) : this(e.CommandName, e.CommandArgument)
        {
        }

        public CommandEventArgs(string commandName, object argument)
        {
            this.commandName = commandName;
            this.argument = argument;
        }

        public object CommandArgument
        {
            get
            {
                return this.argument;
            }
        }

        public string CommandName
        {
            get
            {
                return this.commandName;
            }
        }
    }
}

