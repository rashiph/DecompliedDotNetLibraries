namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    public class XamlSetMarkupExtensionEventArgs : XamlSetValueEventArgs
    {
        public XamlSetMarkupExtensionEventArgs(XamlMember member, System.Windows.Markup.MarkupExtension value, IServiceProvider serviceProvider) : base(member, value)
        {
            this.ServiceProvider = serviceProvider;
        }

        internal XamlSetMarkupExtensionEventArgs(XamlMember member, System.Windows.Markup.MarkupExtension value, IServiceProvider serviceProvider, object targetObject) : this(member, value, serviceProvider)
        {
            this.TargetObject = targetObject;
        }

        public override void CallBase()
        {
            if (this.CurrentType != null)
            {
                XamlType baseType = this.CurrentType.BaseType;
                if (baseType != null)
                {
                    this.CurrentType = baseType;
                    if (baseType.SetMarkupExtensionHandler != null)
                    {
                        baseType.SetMarkupExtensionHandler(this.TargetObject, this);
                    }
                }
            }
        }

        internal XamlType CurrentType { get; set; }

        public System.Windows.Markup.MarkupExtension MarkupExtension
        {
            get
            {
                return (base.Value as System.Windows.Markup.MarkupExtension);
            }
        }

        public IServiceProvider ServiceProvider { get; private set; }

        internal object TargetObject { get; private set; }
    }
}

