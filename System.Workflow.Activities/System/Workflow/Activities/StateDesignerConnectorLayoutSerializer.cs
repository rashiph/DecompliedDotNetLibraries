namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    internal class StateDesignerConnectorLayoutSerializer : ConnectorLayoutSerializer
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
            StateDesignerConnector connector = null;
            IReferenceService service = serializationManager.GetService(typeof(IReferenceService)) as IReferenceService;
            FreeformActivityDesigner designer = serializationManager.Context[typeof(FreeformActivityDesigner)] as FreeformActivityDesigner;
            if ((designer != null) && (service != null))
            {
                StateDesigner.DesignerLayoutConnectionPoint source = null;
                ConnectionPoint target = null;
                StateDesigner.TransitionInfo transitionInfo = null;
                StateDesigner rootStateDesigner = null;
                try
                {
                    Dictionary<string, string> connectorConstructionArguments = base.GetConnectorConstructionArguments(serializationManager, type);
                    if ((connectorConstructionArguments.ContainsKey("EventHandlerName") && connectorConstructionArguments.ContainsKey("SetStateName")) && connectorConstructionArguments.ContainsKey("TargetStateName"))
                    {
                        CompositeActivity reference = (CompositeActivity) service.GetReference(connectorConstructionArguments["EventHandlerName"]);
                        SetStateActivity setState = (SetStateActivity) service.GetReference(connectorConstructionArguments["SetStateName"]);
                        StateActivity activity3 = (StateActivity) service.GetReference(connectorConstructionArguments["TargetStateName"]);
                        transitionInfo = new StateDesigner.TransitionInfo(setState, reference) {
                            TargetState = activity3
                        };
                    }
                    if ((connectorConstructionArguments.ContainsKey("SourceActivity") && connectorConstructionArguments.ContainsKey("SourceConnectionIndex")) && (connectorConstructionArguments.ContainsKey("SourceConnectionEdge") && connectorConstructionArguments.ContainsKey("EventHandlerName")))
                    {
                        StateDesigner associatedDesigner = (StateDesigner) StateDesigner.GetDesigner(service.GetReference(connectorConstructionArguments["SourceActivity"]) as Activity);
                        CompositeActivity eventHandler = (CompositeActivity) service.GetReference(connectorConstructionArguments["EventHandlerName"]);
                        rootStateDesigner = associatedDesigner.RootStateDesigner;
                        DesignerEdges designerEdges = (DesignerEdges) Enum.Parse(typeof(DesignerEdges), connectorConstructionArguments["SourceConnectionEdge"]);
                        int connectionIndex = Convert.ToInt32(connectorConstructionArguments["SourceConnectionIndex"], CultureInfo.InvariantCulture);
                        if (((associatedDesigner != null) && (eventHandler != null)) && ((designerEdges != DesignerEdges.None) && (connectionIndex >= 0)))
                        {
                            source = new StateDesigner.DesignerLayoutConnectionPoint(associatedDesigner, connectionIndex, eventHandler, designerEdges);
                        }
                    }
                    if ((connectorConstructionArguments.ContainsKey("TargetActivity") && connectorConstructionArguments.ContainsKey("TargetConnectionIndex")) && connectorConstructionArguments.ContainsKey("TargetConnectionEdge"))
                    {
                        ActivityDesigner designer4 = StateDesigner.GetDesigner(service.GetReference(connectorConstructionArguments["TargetActivity"]) as Activity);
                        DesignerEdges designerEdge = (DesignerEdges) Enum.Parse(typeof(DesignerEdges), connectorConstructionArguments["TargetConnectionEdge"]);
                        int num2 = Convert.ToInt32(connectorConstructionArguments["TargetConnectionIndex"], CultureInfo.InvariantCulture);
                        if (((designer4 != null) && (designerEdge != DesignerEdges.None)) && (num2 >= 0))
                        {
                            target = new ConnectionPoint(designer4, designerEdge, num2);
                        }
                    }
                }
                catch
                {
                }
                if (((transitionInfo == null) || (source == null)) || (target == null))
                {
                    return connector;
                }
                connector = rootStateDesigner.FindConnector(transitionInfo);
                if (connector == null)
                {
                    rootStateDesigner.AddingSetState = false;
                    try
                    {
                        return (designer.AddConnector(source, target) as StateDesignerConnector);
                    }
                    finally
                    {
                        rootStateDesigner.AddingSetState = true;
                    }
                }
                connector.Source = source;
                connector.Target = target;
                connector.ClearConnectorSegments();
            }
            return connector;
        }

        protected override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
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
                typeof(StateDesignerConnector).GetProperty("SetStateName", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(StateDesignerConnector).GetProperty("SourceStateName", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(StateDesignerConnector).GetProperty("TargetStateName", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(StateDesignerConnector).GetProperty("EventHandlerName", BindingFlags.NonPublic | BindingFlags.Instance)
            };
            return list.ToArray();
        }
    }
}

