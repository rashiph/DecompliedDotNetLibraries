namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml;

    public class FuncDeferringLoader : XamlDeferringLoader
    {
        public override object Load(XamlReader xamlReader, IServiceProvider context)
        {
            IXamlObjectWriterFactory service = context.GetService(typeof(IXamlObjectWriterFactory)) as IXamlObjectWriterFactory;
            IProvideValueTarget target = context.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            Type propertyType = null;
            PropertyInfo targetProperty = target.TargetProperty as PropertyInfo;
            if (targetProperty != null)
            {
                propertyType = targetProperty.PropertyType;
            }
            object firstArgument = Activator.CreateInstance(typeof(FuncFactory).MakeGenericType(propertyType.GetGenericArguments()), new object[] { service, xamlReader });
            return Delegate.CreateDelegate(propertyType, firstArgument, firstArgument.GetType().GetMethod("Evaluate"));
        }

        public override XamlReader Save(object value, IServiceProvider serviceProvider)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.SavingActivityToXamlNotSupported));
        }

        private abstract class FuncFactory
        {
            protected FuncFactory()
            {
            }

            public XamlNodeList Nodes { get; set; }
        }

        private class FuncFactory<T> : FuncDeferringLoader.FuncFactory
        {
            private IXamlObjectWriterFactory objectWriterFactory;

            public FuncFactory(IXamlObjectWriterFactory objectWriterFactory, XamlReader reader)
            {
                this.objectWriterFactory = objectWriterFactory;
                base.Nodes = new XamlNodeList(reader.SchemaContext);
                XamlServices.Transform(reader, base.Nodes.Writer);
            }

            public T Evaluate()
            {
                XamlObjectWriter xamlObjectWriter = this.objectWriterFactory.GetXamlObjectWriter(new XamlObjectWriterSettings());
                XamlServices.Transform(base.Nodes.GetReader(), xamlObjectWriter);
                return (T) xamlObjectWriter.Result;
            }
        }
    }
}

