namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    internal sealed class ServiceInfo
    {
        private KeyedByTypeCollection<IServiceBehavior> behaviors;
        private EndpointInfoCollection endpoints;
        private ServiceHostBase service;
        private string serviceName;

        internal ServiceInfo(ServiceHostBase service)
        {
            this.service = service;
            this.behaviors = service.Description.Behaviors;
            this.serviceName = service.Description.Name;
            this.endpoints = new EndpointInfoCollection(service.Description.Endpoints, this.ServiceName);
        }

        public KeyedByTypeCollection<IServiceBehavior> Behaviors
        {
            get
            {
                return this.behaviors;
            }
        }

        public string ConfigurationName
        {
            get
            {
                return this.service.Description.ConfigurationName;
            }
        }

        public string DistinguishedName
        {
            get
            {
                return (this.serviceName + "@" + this.FirstAddress);
            }
        }

        public EndpointInfoCollection Endpoints
        {
            get
            {
                return this.endpoints;
            }
        }

        public string FirstAddress
        {
            get
            {
                string str = "";
                if (this.Service.BaseAddresses.Count > 0)
                {
                    return this.Service.BaseAddresses[0].ToString();
                }
                if (this.Endpoints.Count > 0)
                {
                    Uri address = this.Endpoints[0].Address;
                    if (null != address)
                    {
                        str = address.ToString();
                    }
                }
                return str;
            }
        }

        public string[] Metadata
        {
            get
            {
                string[] array = null;
                ServiceMetadataExtension extension = this.service.Extensions.Find<ServiceMetadataExtension>();
                if (extension != null)
                {
                    Collection<string> collection = new Collection<string>();
                    try
                    {
                        foreach (MetadataSection section in extension.Metadata.MetadataSections)
                        {
                            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                            {
                                if (section.Metadata is System.Web.Services.Description.ServiceDescription)
                                {
                                    ((System.Web.Services.Description.ServiceDescription) section.Metadata).Write(writer);
                                    collection.Add(writer.ToString());
                                }
                                else
                                {
                                    if (section.Metadata is XmlElement)
                                    {
                                        XmlElement metadata = (XmlElement) section.Metadata;
                                        using (XmlWriter writer2 = XmlWriter.Create(writer))
                                        {
                                            metadata.WriteTo(writer2);
                                            collection.Add(writer.ToString());
                                            continue;
                                        }
                                    }
                                    if (section.Metadata is System.Xml.Schema.XmlSchema)
                                    {
                                        ((System.Xml.Schema.XmlSchema) section.Metadata).Write(writer);
                                        collection.Add(writer.ToString());
                                    }
                                    else
                                    {
                                        collection.Add(section.Metadata.ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException exception)
                    {
                        collection.Add(exception.ToString());
                    }
                    array = new string[collection.Count];
                    collection.CopyTo(array, 0);
                }
                return array;
            }
        }

        public string Name
        {
            get
            {
                return this.serviceName;
            }
        }

        public string Namespace
        {
            get
            {
                return this.service.Description.Namespace;
            }
        }

        public ServiceHostBase Service
        {
            get
            {
                return this.service;
            }
        }

        public string ServiceName
        {
            get
            {
                return this.serviceName;
            }
        }

        public CommunicationState State
        {
            get
            {
                return this.Service.State;
            }
        }
    }
}

