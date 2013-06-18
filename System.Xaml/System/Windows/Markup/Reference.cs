namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    [ContentProperty("Name")]
    public class Reference : MarkupExtension
    {
        public Reference()
        {
        }

        public Reference(string name)
        {
            this.Name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            IXamlNameResolver service = serviceProvider.GetService(typeof(IXamlNameResolver)) as IXamlNameResolver;
            if (service == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MissingNameResolver"));
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MustHaveName"));
            }
            object fixupToken = service.Resolve(this.Name);
            if (fixupToken == null)
            {
                string[] names = new string[] { this.Name };
                fixupToken = service.GetFixupToken(names, true);
            }
            return fixupToken;
        }

        [ConstructorArgument("name")]
        public string Name { get; set; }
    }
}

