namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public class CommandLineArgumentRelation : PropertyRelation
    {
        public CommandLineArgumentRelation(string argument, string value, bool required, string separator) : base(argument, value, required)
        {
            this.Separator = separator;
        }

        public string Separator { get; set; }
    }
}

