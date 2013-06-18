namespace System.Workflow.ComponentModel.Design
{
    using System;

    internal interface IConnectableDesigner
    {
        bool CanConnect(ConnectionPoint source, ConnectionPoint target);
        void OnConnected(ConnectionPoint source, ConnectionPoint target);
    }
}

