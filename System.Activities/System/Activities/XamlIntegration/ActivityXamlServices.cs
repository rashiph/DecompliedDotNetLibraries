namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.IO;
    using System.Reflection;
    using System.Xaml;
    using System.Xml;

    public static class ActivityXamlServices
    {
        private static readonly XamlSchemaContext dynamicActivityReaderSchemaContext = new DynamicActivityReaderSchemaContext();

        public static XamlReader CreateBuilderReader(XamlReader innerReader)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }
            return new DynamicActivityXamlReader(true, innerReader, null);
        }

        public static XamlReader CreateBuilderReader(XamlReader innerReader, XamlSchemaContext schemaContext)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }
            if (schemaContext == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaContext");
            }
            return new DynamicActivityXamlReader(true, innerReader, schemaContext);
        }

        public static XamlWriter CreateBuilderWriter(XamlWriter innerWriter)
        {
            if (innerWriter == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerWriter");
            }
            return new ActivityBuilderXamlWriter(innerWriter);
        }

        public static XamlReader CreateReader(Stream stream)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }
            return CreateReader(new XamlXmlReader(XmlReader.Create(stream), dynamicActivityReaderSchemaContext), dynamicActivityReaderSchemaContext);
        }

        public static XamlReader CreateReader(XamlReader innerReader)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }
            return new DynamicActivityXamlReader(innerReader);
        }

        public static XamlReader CreateReader(XamlReader innerReader, XamlSchemaContext schemaContext)
        {
            if (innerReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerReader");
            }
            if (schemaContext == null)
            {
                throw FxTrace.Exception.ArgumentNull("schemaContext");
            }
            return new DynamicActivityXamlReader(innerReader, schemaContext);
        }

        public static Activity Load(Stream stream)
        {
            if (stream == null)
            {
                throw FxTrace.Exception.ArgumentNull("stream");
            }
            using (XmlReader reader = XmlReader.Create(stream))
            {
                return Load(reader);
            }
        }

        public static Activity Load(TextReader textReader)
        {
            if (textReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("textReader");
            }
            using (XmlReader reader = XmlReader.Create(textReader))
            {
                return Load(reader);
            }
        }

        public static Activity Load(string fileName)
        {
            if (fileName == null)
            {
                throw FxTrace.Exception.ArgumentNull("fileName");
            }
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                return Load(reader);
            }
        }

        public static Activity Load(XamlReader xamlReader)
        {
            if (xamlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xamlReader");
            }
            DynamicActivityXamlReader reader = new DynamicActivityXamlReader(xamlReader);
            object obj2 = XamlServices.Load(reader);
            Activity activity = obj2 as Activity;
            if (activity == null)
            {
                throw FxTrace.Exception.Argument("reader", System.Activities.SR.ActivityXamlServicesRequiresActivity((obj2 != null) ? obj2.GetType().FullName : string.Empty));
            }
            return activity;
        }

        public static Activity Load(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xmlReader");
            }
            using (XamlXmlReader reader = new XamlXmlReader(xmlReader, dynamicActivityReaderSchemaContext))
            {
                return Load(reader);
            }
        }

        private class DynamicActivityReaderSchemaContext : XamlSchemaContext
        {
            private const string serviceModelActivitiesDll = "System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            private const string serviceModelDll = "System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            private static bool serviceModelLoaded;
            private const string serviceModelNamespace = "http://schemas.microsoft.com/netfx/2009/xaml/servicemodel";

            public DynamicActivityReaderSchemaContext() : base(new XamlSchemaContextSettings())
            {
            }

            protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
            {
                XamlType type = base.GetXamlType(xamlNamespace, name, typeArguments);
                if (((type == null) && (xamlNamespace == "http://schemas.microsoft.com/netfx/2009/xaml/servicemodel")) && !serviceModelLoaded)
                {
                    Assembly.Load("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    Assembly.Load("System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    serviceModelLoaded = true;
                    type = base.GetXamlType(xamlNamespace, name, typeArguments);
                }
                return type;
            }
        }
    }
}

