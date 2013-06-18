namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public class PropertyRelation
    {
        public PropertyRelation()
        {
        }

        public PropertyRelation(string argument, string value, bool required)
        {
            this.Argument = argument;
            this.Value = value;
            this.Required = required;
        }

        public string Argument { get; set; }

        public bool Required { get; set; }

        public string Value { get; set; }
    }
}

