namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Xml.Serialization;

    internal class WebReferenceOptionsSerializationReader : XmlSerializationReader
    {
        private Hashtable _CodeGenerationOptionsValues;
        private string id1_webReferenceOptions;
        private string id2_Item;
        private string id3_codeGenerationOptions;
        private string id4_schemaImporterExtensions;
        private string id5_type;
        private string id6_style;
        private string id7_verbose;

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id2_Item = base.Reader.NameTable.Add("http://microsoft.com/webReference/");
            this.id5_type = base.Reader.NameTable.Add("type");
            this.id4_schemaImporterExtensions = base.Reader.NameTable.Add("schemaImporterExtensions");
            this.id3_codeGenerationOptions = base.Reader.NameTable.Add("codeGenerationOptions");
            this.id6_style = base.Reader.NameTable.Add("style");
            this.id7_verbose = base.Reader.NameTable.Add("verbose");
            this.id1_webReferenceOptions = base.Reader.NameTable.Add("webReferenceOptions");
        }

        private CodeGenerationOptions Read1_CodeGenerationOptions(string s)
        {
            return (CodeGenerationOptions) ((int) XmlSerializationReader.ToEnum(s, this.CodeGenerationOptionsValues, "System.Xml.Serialization.CodeGenerationOptions"));
        }

        private ServiceDescriptionImportStyle Read2_ServiceDescriptionImportStyle(string s)
        {
            switch (s)
            {
                case "client":
                    return ServiceDescriptionImportStyle.Client;

                case "server":
                    return ServiceDescriptionImportStyle.Server;

                case "serverInterface":
                    return ServiceDescriptionImportStyle.ServerInterface;
            }
            throw base.CreateUnknownConstantException(s, typeof(ServiceDescriptionImportStyle));
        }

        private WebReferenceOptions Read4_WebReferenceOptions(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id1_webReferenceOptions) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            WebReferenceOptions o = new WebReferenceOptions();
            StringCollection schemaImporterExtensions = o.SchemaImporterExtensions;
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id3_codeGenerationOptions)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (base.Reader.IsEmptyElement)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            o.CodeGenerationOptions = this.Read1_CodeGenerationOptions(base.Reader.ReadElementString());
                        }
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id4_schemaImporterExtensions) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            StringCollection strings = o.SchemaImporterExtensions;
                            if ((strings == null) || base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num3 = 0;
                                int num4 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id5_type) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            if (base.ReadNull())
                                            {
                                                strings.Add(null);
                                            }
                                            else
                                            {
                                                strings.Add(base.Reader.ReadElementString());
                                            }
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://microsoft.com/webReference/:type");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://microsoft.com/webReference/:type");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num3, ref num4);
                                }
                                base.ReadEndElement();
                            }
                        }
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id6_style)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (base.Reader.IsEmptyElement)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            o.Style = this.Read2_ServiceDescriptionImportStyle(base.Reader.ReadElementString());
                        }
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id7_verbose)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Verbose = XmlConvert.ToBoolean(base.Reader.ReadElementString());
                        flagArray[3] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://microsoft.com/webReference/:codeGenerationOptions, http://microsoft.com/webReference/:schemaImporterExtensions, http://microsoft.com/webReference/:style, http://microsoft.com/webReference/:verbose");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://microsoft.com/webReference/:codeGenerationOptions, http://microsoft.com/webReference/:schemaImporterExtensions, http://microsoft.com/webReference/:style, http://microsoft.com/webReference/:verbose");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        internal object Read5_webReferenceOptions()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id1_webReferenceOptions) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read4_WebReferenceOptions(true, true);
            }
            base.UnknownNode(null, "http://microsoft.com/webReference/:webReferenceOptions");
            return null;
        }

        internal Hashtable CodeGenerationOptionsValues
        {
            get
            {
                if (this._CodeGenerationOptionsValues == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("properties", 1L);
                    hashtable.Add("newAsync", 2L);
                    hashtable.Add("oldAsync", 4L);
                    hashtable.Add("order", 8L);
                    hashtable.Add("enableDataBinding", 0x10L);
                    this._CodeGenerationOptionsValues = hashtable;
                }
                return this._CodeGenerationOptionsValues;
            }
        }
    }
}

