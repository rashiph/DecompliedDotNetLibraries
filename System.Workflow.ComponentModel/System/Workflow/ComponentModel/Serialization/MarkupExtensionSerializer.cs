namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    using System.Text;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    internal class MarkupExtensionSerializer : WorkflowMarkupSerializer
    {
        private const string CompactFormatCharacters = "=,\"'{}\\";
        private const string CompactFormatEnd = "}";
        private const string CompactFormatNameValueSeperator = "=";
        private const string CompactFormatPropertySeperator = ",";
        private const string CompactFormatStart = "{";
        private const string CompactFormatTypeSeperator = " ";

        protected internal sealed override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return true;
        }

        private string CreateEscapedValue(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            StringBuilder builder = new StringBuilder(0x40);
            int length = value.Length;
            for (int i = 0; i < length; i++)
            {
                if ("=,\"'{}\\".IndexOf(value[i]) != -1)
                {
                    builder.Append(@"\");
                }
                builder.Append(value[i]);
            }
            return builder.ToString();
        }

        protected virtual InstanceDescriptor GetInstanceDescriptor(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            MarkupExtension extension = value as MarkupExtension;
            if (extension == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(MarkupExtension).FullName }), "value");
            }
            return new InstanceDescriptor(extension.GetType().GetConstructor(new Type[0]), null);
        }

        protected internal sealed override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            writer.WriteString("{");
            string prefix = string.Empty;
            XmlQualifiedName xmlQualifiedName = serializationManager.GetXmlQualifiedName(value.GetType(), out prefix);
            writer.WriteQualifiedName(xmlQualifiedName.Name, xmlQualifiedName.Namespace);
            int num = 0;
            Dictionary<string, string> dictionary = null;
            InstanceDescriptor instanceDescriptor = this.GetInstanceDescriptor(serializationManager, value);
            if (instanceDescriptor != null)
            {
                ConstructorInfo memberInfo = instanceDescriptor.MemberInfo as ConstructorInfo;
                if (memberInfo != null)
                {
                    ParameterInfo[] parameters = memberInfo.GetParameters();
                    if ((parameters != null) && (parameters.Length == instanceDescriptor.Arguments.Count))
                    {
                        int index = 0;
                        foreach (object obj2 in instanceDescriptor.Arguments)
                        {
                            if (dictionary == null)
                            {
                                dictionary = new Dictionary<string, string>();
                            }
                            if (obj2 != null)
                            {
                                dictionary.Add(parameters[index].Name, parameters[index++].Name);
                                if (num++ > 0)
                                {
                                    writer.WriteString(",");
                                }
                                else
                                {
                                    writer.WriteString(" ");
                                }
                                if (obj2.GetType() == typeof(string))
                                {
                                    writer.WriteString(this.CreateEscapedValue(obj2 as string));
                                }
                                else if (obj2 is Type)
                                {
                                    Type type = obj2 as Type;
                                    if (type.Assembly != null)
                                    {
                                        string str2 = string.Empty;
                                        XmlQualifiedName name2 = serializationManager.GetXmlQualifiedName(type, out str2);
                                        writer.WriteQualifiedName(XmlConvert.EncodeName(name2.Name), name2.Namespace);
                                    }
                                    else
                                    {
                                        writer.WriteString(type.FullName);
                                    }
                                }
                                else
                                {
                                    string text = base.SerializeToString(serializationManager, obj2);
                                    if (text != null)
                                    {
                                        writer.WriteString(text);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            List<PropertyInfo> list = new List<PropertyInfo>();
            list.AddRange(this.GetProperties(serializationManager, value));
            list.AddRange(serializationManager.GetExtendedProperties(value));
            foreach (PropertyInfo info2 in list)
            {
                if (((Helpers.GetSerializationVisibility(info2) != DesignerSerializationVisibility.Hidden) && info2.CanRead) && (info2.GetValue(value, null) != null))
                {
                    WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(info2.PropertyType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                    if (serializer == null)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailable", new object[] { info2.PropertyType.FullName })));
                    }
                    else
                    {
                        if (dictionary != null)
                        {
                            object[] customAttributes = info2.GetCustomAttributes(typeof(ConstructorArgumentAttribute), false);
                            if ((customAttributes.Length > 0) && dictionary.ContainsKey((customAttributes[0] as ConstructorArgumentAttribute).ArgumentName))
                            {
                                continue;
                            }
                        }
                        serializationManager.Context.Push(info2);
                        try
                        {
                            object obj3 = info2.GetValue(value, null);
                            if (serializer.ShouldSerializeValue(serializationManager, obj3))
                            {
                                if (serializer.CanSerializeToString(serializationManager, obj3))
                                {
                                    if (num++ > 0)
                                    {
                                        writer.WriteString(",");
                                    }
                                    else
                                    {
                                        writer.WriteString(" ");
                                    }
                                    writer.WriteString(info2.Name);
                                    writer.WriteString("=");
                                    if (obj3.GetType() == typeof(string))
                                    {
                                        writer.WriteString(this.CreateEscapedValue(obj3 as string));
                                    }
                                    else
                                    {
                                        string str4 = serializer.SerializeToString(serializationManager, obj3);
                                        if (str4 != null)
                                        {
                                            writer.WriteString(str4);
                                        }
                                    }
                                }
                                else
                                {
                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNoSerializeLogic", new object[] { info2.Name, value.GetType().FullName })));
                                }
                            }
                        }
                        catch
                        {
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNoSerializeLogic", new object[] { info2.Name, value.GetType().FullName })));
                        }
                        finally
                        {
                            serializationManager.Context.Pop();
                        }
                    }
                }
            }
            writer.WriteString("}");
            return string.Empty;
        }
    }
}

