namespace System.Web.Services.Description
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Serialization;

    internal class WebReferenceOptionsSerializationWriter : XmlSerializationWriter
    {
        protected override void InitCallbacks()
        {
        }

        private string Write1_CodeGenerationOptions(CodeGenerationOptions v)
        {
            switch (v)
            {
                case CodeGenerationOptions.GenerateProperties:
                    return "properties";

                case CodeGenerationOptions.GenerateNewAsync:
                    return "newAsync";

                case CodeGenerationOptions.GenerateOldAsync:
                    return "oldAsync";

                case CodeGenerationOptions.GenerateOrder:
                    return "order";

                case CodeGenerationOptions.EnableDataBinding:
                    return "enableDataBinding";
            }
            return XmlSerializationWriter.FromEnum((long) v, new string[] { "properties", "newAsync", "oldAsync", "order", "enableDataBinding" }, new long[] { 1L, 2L, 4L, 8L, 0x10L }, "System.Xml.Serialization.CodeGenerationOptions");
        }

        private string Write2_ServiceDescriptionImportStyle(ServiceDescriptionImportStyle v)
        {
            switch (v)
            {
                case ServiceDescriptionImportStyle.Client:
                    return "client";

                case ServiceDescriptionImportStyle.Server:
                    return "server";

                case ServiceDescriptionImportStyle.ServerInterface:
                    return "serverInterface";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Web.Services.Description.ServiceDescriptionImportStyle");
        }

        private void Write4_WebReferenceOptions(string n, string ns, WebReferenceOptions o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(WebReferenceOptions)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o);
                if (needType)
                {
                    base.WriteXsiType("webReferenceOptions", "http://microsoft.com/webReference/");
                }
                if (o.CodeGenerationOptions != CodeGenerationOptions.GenerateOldAsync)
                {
                    base.WriteElementString("codeGenerationOptions", "http://microsoft.com/webReference/", this.Write1_CodeGenerationOptions(o.CodeGenerationOptions));
                }
                StringCollection schemaImporterExtensions = o.SchemaImporterExtensions;
                if (schemaImporterExtensions != null)
                {
                    base.WriteStartElement("schemaImporterExtensions", "http://microsoft.com/webReference/");
                    for (int i = 0; i < schemaImporterExtensions.Count; i++)
                    {
                        base.WriteNullableStringLiteral("type", "http://microsoft.com/webReference/", schemaImporterExtensions[i]);
                    }
                    base.WriteEndElement();
                }
                if (o.Style != ServiceDescriptionImportStyle.Client)
                {
                    base.WriteElementString("style", "http://microsoft.com/webReference/", this.Write2_ServiceDescriptionImportStyle(o.Style));
                }
                base.WriteElementStringRaw("verbose", "http://microsoft.com/webReference/", XmlConvert.ToString(o.Verbose));
                base.WriteEndElement(o);
            }
        }

        internal void Write5_webReferenceOptions(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("webReferenceOptions", "http://microsoft.com/webReference/");
            }
            else
            {
                base.TopLevelElement();
                this.Write4_WebReferenceOptions("webReferenceOptions", "http://microsoft.com/webReference/", (WebReferenceOptions) o, true, false);
            }
        }
    }
}

