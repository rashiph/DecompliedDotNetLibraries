namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Text")]
    public sealed class WriteLine : CodeActivity
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Text", typeof(string), ArgumentDirection.In);
            metadata.Bind(this.Text, argument);
            RuntimeArgument argument2 = new RuntimeArgument("TextWriter", typeof(System.IO.TextWriter), ArgumentDirection.In);
            metadata.Bind(this.TextWriter, argument2);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument, argument2 });
        }

        protected override void Execute(CodeActivityContext context)
        {
            System.IO.TextWriter writer = this.TextWriter.Get(context);
            if (writer == null)
            {
                writer = context.GetExtension<System.IO.TextWriter>() ?? Console.Out;
            }
            writer.WriteLine(this.Text.Get(context));
        }

        [DefaultValue((string) null)]
        public InArgument<string> Text { get; set; }

        [DefaultValue((string) null)]
        public InArgument<System.IO.TextWriter> TextWriter { get; set; }
    }
}

