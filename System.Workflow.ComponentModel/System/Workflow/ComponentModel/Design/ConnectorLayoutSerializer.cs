namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    public class ConnectorLayoutSerializer : WorkflowMarkupSerializer
    {
        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Connector connector = null;
            IReferenceService service = serializationManager.GetService(typeof(IReferenceService)) as IReferenceService;
            FreeformActivityDesigner designer = serializationManager.Context[typeof(FreeformActivityDesigner)] as FreeformActivityDesigner;
            if ((designer != null) && (service != null))
            {
                ConnectionPoint source = null;
                ConnectionPoint target = null;
                try
                {
                    Dictionary<string, string> connectorConstructionArguments = this.GetConnectorConstructionArguments(serializationManager, type);
                    if ((connectorConstructionArguments.ContainsKey("SourceActivity") && connectorConstructionArguments.ContainsKey("SourceConnectionIndex")) && connectorConstructionArguments.ContainsKey("SourceConnectionEdge"))
                    {
                        ActivityDesigner associatedDesigner = ActivityDesigner.GetDesigner(service.GetReference(connectorConstructionArguments["SourceActivity"]) as Activity);
                        DesignerEdges designerEdge = (DesignerEdges) Enum.Parse(typeof(DesignerEdges), connectorConstructionArguments["SourceConnectionEdge"]);
                        int connectionIndex = Convert.ToInt32(connectorConstructionArguments["SourceConnectionIndex"], CultureInfo.InvariantCulture);
                        if (((associatedDesigner != null) && (designerEdge != DesignerEdges.None)) && (connectionIndex >= 0))
                        {
                            source = new ConnectionPoint(associatedDesigner, designerEdge, connectionIndex);
                        }
                    }
                    if ((connectorConstructionArguments.ContainsKey("TargetActivity") && connectorConstructionArguments.ContainsKey("TargetConnectionIndex")) && connectorConstructionArguments.ContainsKey("TargetConnectionEdge"))
                    {
                        ActivityDesigner designer3 = ActivityDesigner.GetDesigner(service.GetReference(connectorConstructionArguments["TargetActivity"]) as Activity);
                        DesignerEdges edges2 = (DesignerEdges) Enum.Parse(typeof(DesignerEdges), connectorConstructionArguments["TargetConnectionEdge"]);
                        int num2 = Convert.ToInt32(connectorConstructionArguments["TargetConnectionIndex"], CultureInfo.InvariantCulture);
                        if (((designer3 != null) && (edges2 != DesignerEdges.None)) && (num2 >= 0))
                        {
                            target = new ConnectionPoint(designer3, edges2, num2);
                        }
                    }
                }
                catch
                {
                }
                if ((source != null) && (target != null))
                {
                    connector = designer.AddConnector(source, target);
                }
            }
            return connector;
        }

        protected Dictionary<string, string> GetConnectorConstructionArguments(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if ((reader != null) && (reader.NodeType == XmlNodeType.Element))
            {
                while (reader.MoveToNextAttribute())
                {
                    string localName = reader.LocalName;
                    if (!dictionary.ContainsKey(localName))
                    {
                        reader.ReadAttributeValue();
                        dictionary.Add(localName, reader.Value);
                    }
                }
                reader.MoveToElement();
            }
            return dictionary;
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            List<PropertyInfo> list = new List<PropertyInfo>(base.GetProperties(serializationManager, obj)) {
                typeof(Connector).GetProperty("SourceActivity", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(Connector).GetProperty("SourceConnectionIndex", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(Connector).GetProperty("SourceConnectionEdge", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(Connector).GetProperty("TargetActivity", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(Connector).GetProperty("TargetConnectionIndex", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(Connector).GetProperty("TargetConnectionEdge", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(Connector).GetProperty("Segments", BindingFlags.NonPublic | BindingFlags.Instance)
            };
            return list.ToArray();
        }

        protected override void OnAfterDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnAfterDeserialize(serializationManager, obj);
            Connector connector = obj as Connector;
            if (connector != null)
            {
                connector.SetConnectorModified(true);
            }
        }
    }
}

