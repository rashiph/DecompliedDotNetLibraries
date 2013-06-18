namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Xml;

    internal class DataSourceXmlSerializer
    {
        private string nameSpace = "urn:schemas-microsoft-com:xml-msdatasource";
        private static Hashtable nameToType;
        private Queue objectNeedBeInitialized = new Queue();
        private static Hashtable propertySerializationInfoHash;

        internal DataSourceXmlSerializer()
        {
        }

        private object CreateObject(string tagName)
        {
            if (tagName == "DbTable")
            {
                tagName = "TableAdapter";
            }
            if (!this.NameToType.Contains(tagName))
            {
                throw new DataSourceSerializationException(System.Design.SR.GetString("DTDS_CouldNotDeserializeXmlElement", new object[] { tagName }));
            }
            Type type = (Type) this.NameToType[tagName];
            return Activator.CreateInstance(type);
        }

        internal object Deserialize(XmlElement xmlElement)
        {
            object obj2 = this.CreateObject(xmlElement.LocalName);
            if (obj2 is IDataSourceXmlSerializable)
            {
                ((IDataSourceXmlSerializable) obj2).ReadXml(xmlElement, this);
            }
            else
            {
                this.DeserializeBody(xmlElement, obj2);
            }
            if (obj2 is IDataSourceInitAfterLoading)
            {
                this.objectNeedBeInitialized.Enqueue(obj2);
            }
            return obj2;
        }

        internal void DeserializeBody(XmlElement xmlElement, object obj)
        {
            object innerText;
            PropertySerializationInfo serializationInfo = this.GetSerializationInfo(obj.GetType());
            IDataSourceXmlSpecialOwner owner = obj as IDataSourceXmlSpecialOwner;
            foreach (XmlSerializableProperty property in serializationInfo.AttributeProperties)
            {
                DataSourceXmlAttributeAttribute serializationAttribute = property.SerializationAttribute as DataSourceXmlAttributeAttribute;
                if (serializationAttribute != null)
                {
                    XmlAttribute xmlNode = xmlElement.Attributes[property.Name];
                    if (xmlNode != null)
                    {
                        PropertyDescriptor propertyDescriptor = property.PropertyDescriptor;
                        if (serializationAttribute.SpecialWay)
                        {
                            owner.ReadSpecialItem(propertyDescriptor.Name, xmlNode, this);
                        }
                        else
                        {
                            Type propertyType = property.PropertyType;
                            if (propertyType == typeof(string))
                            {
                                innerText = xmlNode.InnerText;
                            }
                            else
                            {
                                innerText = TypeDescriptor.GetConverter(propertyType).ConvertFromString(xmlNode.InnerText);
                            }
                            if (innerText != null)
                            {
                                propertyDescriptor.SetValue(obj, innerText);
                            }
                        }
                    }
                }
            }
            foreach (XmlNode node in xmlElement.ChildNodes)
            {
                XmlElement element = node as XmlElement;
                if (element != null)
                {
                    XmlSerializableProperty serializablePropertyWithElementName = serializationInfo.GetSerializablePropertyWithElementName(element.LocalName);
                    if (serializablePropertyWithElementName != null)
                    {
                        PropertyDescriptor descriptor2 = serializablePropertyWithElementName.PropertyDescriptor;
                        DataSourceXmlSerializationAttribute attribute3 = serializablePropertyWithElementName.SerializationAttribute;
                        if (attribute3 is DataSourceXmlElementAttribute)
                        {
                            DataSourceXmlElementAttribute attribute1 = (DataSourceXmlElementAttribute) attribute3;
                            if (attribute3.SpecialWay)
                            {
                                owner.ReadSpecialItem(descriptor2.Name, element, this);
                            }
                            else if (this.NameToType.Contains(element.LocalName))
                            {
                                object obj3 = this.Deserialize(element);
                                descriptor2.SetValue(obj, obj3);
                            }
                            else
                            {
                                Type type = serializablePropertyWithElementName.PropertyType;
                                try
                                {
                                    if (type == typeof(string))
                                    {
                                        innerText = element.InnerText;
                                    }
                                    else
                                    {
                                        innerText = TypeDescriptor.GetConverter(type).ConvertFromString(element.InnerText);
                                    }
                                    descriptor2.SetValue(obj, innerText);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        else
                        {
                            DataSourceXmlSubItemAttribute attribute4 = (DataSourceXmlSubItemAttribute) attribute3;
                            if (typeof(IList).IsAssignableFrom(descriptor2.PropertyType))
                            {
                                IList list = descriptor2.GetValue(obj) as IList;
                                foreach (XmlNode node2 in element.ChildNodes)
                                {
                                    XmlElement element2 = node2 as XmlElement;
                                    if (element2 != null)
                                    {
                                        object obj4 = this.Deserialize(element2);
                                        list.Add(obj4);
                                    }
                                }
                            }
                            else
                            {
                                for (XmlNode node3 = element.FirstChild; node3 != null; node3 = node3.NextSibling)
                                {
                                    if (node3 is XmlElement)
                                    {
                                        object obj5 = this.Deserialize((XmlElement) node3);
                                        descriptor2.SetValue(obj, obj5);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private PropertySerializationInfo GetSerializationInfo(Type type)
        {
            if (propertySerializationInfoHash == null)
            {
                propertySerializationInfoHash = new Hashtable();
            }
            if (propertySerializationInfoHash.Contains(type))
            {
                return (PropertySerializationInfo) propertySerializationInfoHash[type];
            }
            PropertySerializationInfo info = new PropertySerializationInfo(type);
            propertySerializationInfoHash.Add(type, info);
            return info;
        }

        internal void InitializeObjects()
        {
            int count = this.objectNeedBeInitialized.Count;
            while (count-- > 0)
            {
                ((IDataSourceInitAfterLoading) this.objectNeedBeInitialized.Dequeue()).InitializeAfterLoading();
            }
        }

        internal void Serialize(XmlWriter xmlWriter, object obj)
        {
            if (obj is IDataSourceXmlSerializable)
            {
                ((IDataSourceXmlSerializable) obj).WriteXml(xmlWriter, this);
            }
            else
            {
                Type componentType = obj.GetType();
                string localName = null;
                DataSourceXmlClassAttribute attribute = TypeDescriptor.GetAttributes(componentType)[typeof(DataSourceXmlClassAttribute)] as DataSourceXmlClassAttribute;
                if (attribute != null)
                {
                    localName = attribute.Name;
                }
                if (localName == null)
                {
                    localName = componentType.Name;
                }
                xmlWriter.WriteStartElement(string.Empty, localName, this.nameSpace);
                this.SerializeBody(xmlWriter, obj);
                xmlWriter.WriteFullEndElement();
            }
        }

        internal void SerializeBody(XmlWriter xmlWriter, object obj)
        {
            PropertyDescriptorCollection properties;
            if (obj is ICustomTypeDescriptor)
            {
                properties = ((ICustomTypeDescriptor) obj).GetProperties();
            }
            else
            {
                properties = TypeDescriptor.GetProperties(obj);
            }
            properties = properties.Sort();
            ArrayList list = new ArrayList();
            IDataSourceXmlSpecialOwner owner = obj as IDataSourceXmlSpecialOwner;
            foreach (PropertyDescriptor descriptor in properties)
            {
                DataSourceXmlSerializationAttribute attribute = (DataSourceXmlSerializationAttribute) descriptor.Attributes[typeof(DataSourceXmlSerializationAttribute)];
                if (attribute != null)
                {
                    if (attribute is DataSourceXmlAttributeAttribute)
                    {
                        DataSourceXmlAttributeAttribute attribute2 = (DataSourceXmlAttributeAttribute) attribute;
                        object obj2 = descriptor.GetValue(obj);
                        if (obj2 != null)
                        {
                            string name = attribute2.Name;
                            if (name == null)
                            {
                                name = descriptor.Name;
                            }
                            if (attribute2.SpecialWay)
                            {
                                xmlWriter.WriteStartAttribute(string.Empty, name, string.Empty);
                                owner.WriteSpecialItem(descriptor.Name, xmlWriter, this);
                                xmlWriter.WriteEndAttribute();
                            }
                            else
                            {
                                xmlWriter.WriteAttributeString(name, obj2.ToString());
                            }
                        }
                    }
                    else
                    {
                        list.Add(descriptor);
                    }
                }
            }
            foreach (PropertyDescriptor descriptor2 in list)
            {
                object obj3 = descriptor2.GetValue(obj);
                if (obj3 != null)
                {
                    DataSourceXmlSerializationAttribute attribute3 = (DataSourceXmlSerializationAttribute) descriptor2.Attributes[typeof(DataSourceXmlSerializationAttribute)];
                    string localName = attribute3.Name;
                    if (localName == null)
                    {
                        localName = descriptor2.Name;
                    }
                    if (attribute3 is DataSourceXmlElementAttribute)
                    {
                        DataSourceXmlElementAttribute attribute1 = (DataSourceXmlElementAttribute) attribute3;
                        if (attribute3.SpecialWay)
                        {
                            xmlWriter.WriteStartElement(string.Empty, localName, this.nameSpace);
                            owner.WriteSpecialItem(descriptor2.Name, xmlWriter, this);
                            xmlWriter.WriteFullEndElement();
                        }
                        else if (this.NameToType.Contains(localName))
                        {
                            this.Serialize(xmlWriter, obj3);
                        }
                        else
                        {
                            xmlWriter.WriteElementString(localName, obj3.ToString());
                        }
                    }
                    else
                    {
                        DataSourceXmlSubItemAttribute attribute4 = (DataSourceXmlSubItemAttribute) attribute3;
                        xmlWriter.WriteStartElement(string.Empty, localName, this.nameSpace);
                        if (obj3 is ICollection)
                        {
                            foreach (object obj4 in (ICollection) obj3)
                            {
                                this.Serialize(xmlWriter, obj4);
                            }
                        }
                        else
                        {
                            this.Serialize(xmlWriter, obj3);
                        }
                        xmlWriter.WriteFullEndElement();
                    }
                }
            }
        }

        private Hashtable NameToType
        {
            get
            {
                if (nameToType == null)
                {
                    nameToType = new Hashtable();
                    nameToType.Add("DbSource", typeof(DbSource));
                    nameToType.Add("Connection", typeof(DesignConnection));
                    nameToType.Add("TableAdapter", typeof(DesignTable));
                    nameToType.Add("DbCommand", typeof(DbSourceCommand));
                    nameToType.Add("Parameter", typeof(DesignParameter));
                }
                return nameToType;
            }
        }

        private class PropertySerializationInfo
        {
            internal DataSourceXmlSerializer.XmlSerializableProperty[] AttributeProperties;
            private Hashtable elementProperties;

            internal PropertySerializationInfo(Type type)
            {
                ArrayList list = new ArrayList();
                this.elementProperties = new Hashtable();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(type))
                {
                    DataSourceXmlSerializationAttribute serializationAttribute = (DataSourceXmlSerializationAttribute) descriptor.Attributes[typeof(DataSourceXmlSerializationAttribute)];
                    if (serializationAttribute != null)
                    {
                        DataSourceXmlSerializer.XmlSerializableProperty property = new DataSourceXmlSerializer.XmlSerializableProperty(serializationAttribute, descriptor);
                        if (serializationAttribute is DataSourceXmlAttributeAttribute)
                        {
                            list.Add(property);
                        }
                        else
                        {
                            this.elementProperties.Add(property.Name, property);
                        }
                    }
                }
                this.AttributeProperties = (DataSourceXmlSerializer.XmlSerializableProperty[]) list.ToArray(typeof(DataSourceXmlSerializer.XmlSerializableProperty));
            }

            internal DataSourceXmlSerializer.XmlSerializableProperty GetSerializablePropertyWithElementName(string name)
            {
                if (this.elementProperties.Contains(name))
                {
                    return (DataSourceXmlSerializer.XmlSerializableProperty) this.elementProperties[name];
                }
                return null;
            }
        }

        private class XmlSerializableProperty
        {
            internal string Name;
            internal System.ComponentModel.PropertyDescriptor PropertyDescriptor;
            internal Type PropertyType;
            internal DataSourceXmlSerializationAttribute SerializationAttribute;

            internal XmlSerializableProperty(DataSourceXmlSerializationAttribute serializationAttribute, System.ComponentModel.PropertyDescriptor propertyDescriptor)
            {
                this.Name = serializationAttribute.Name;
                if (this.Name == null)
                {
                    this.Name = propertyDescriptor.Name;
                }
                this.SerializationAttribute = serializationAttribute;
                this.PropertyDescriptor = propertyDescriptor;
                this.PropertyType = serializationAttribute.ItemType;
                if (this.PropertyType == null)
                {
                    this.PropertyType = propertyDescriptor.PropertyType;
                }
            }
        }
    }
}

