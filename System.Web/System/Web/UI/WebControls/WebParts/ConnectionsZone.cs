namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SupportsEventValidation, Designer("System.Web.UI.Design.WebControls.WebParts.ConnectionsZoneDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ConnectionsZone : ToolZone
    {
        private ArrayList _availableTransformers;
        private WebPartVerb _cancelVerb;
        private WebPartVerb _closeVerb;
        private WebPartVerb _configureVerb;
        private IDictionary _connectDropDownLists;
        private IDictionary _connectionPointInfo;
        private WebPartVerb _connectVerb;
        private WebPartVerb _disconnectVerb;
        private bool _displayErrorMessage;
        private ConnectionsZoneMode _mode;
        private string _pendingConnectionID;
        private string _pendingConnectionPointID;
        private ConnectionType _pendingConnectionType;
        private WebPart _pendingConsumer;
        private ConsumerConnectionPoint _pendingConsumerConnectionPoint;
        private string _pendingConsumerID;
        private WebPart _pendingProvider;
        private ProviderConnectionPoint _pendingProviderConnectionPoint;
        private string _pendingSelectedValue;
        private WebPartTransformer _pendingTransformer;
        private Control _pendingTransformerConfigurationControl;
        private string _pendingTransformerConfigurationControlTypeName;
        private const int baseIndex = 0;
        private const string cancelEventArgument = "cancel";
        private const int cancelVerbIndex = 1;
        private const string closeEventArgument = "close";
        private const int closeVerbIndex = 2;
        private const string configureEventArgument = "edit";
        private const int configureVerbIndex = 3;
        private const string connectConsumerEventArgument = "connectconsumer";
        private const string connectEventArgument = "connect";
        private const string connectProviderEventArgument = "connectprovider";
        private const int connectVerbIndex = 4;
        private const string consumerEventArgument = "consumer";
        private const string consumerListIdPrefix = "_consumerlist_";
        private const int controlStateArrayLength = 8;
        private const string disconnectEventArgument = "disconnect";
        private const int disconnectVerbIndex = 5;
        private const int modeIndex = 1;
        private const int pendingConnectionIDIndex = 7;
        private const int pendingConnectionPointIDIndex = 2;
        private const int pendingConnectionTypeIndex = 3;
        private const int pendingConsumerIDIndex = 5;
        private const int pendingSelectedValueIndex = 4;
        private const int pendingTransformerTypeNameIndex = 6;
        private const string providerEventArgument = "provider";
        private const string providerListIdPrefix = "_providerlist_";
        private const int viewStateArrayLength = 6;

        public ConnectionsZone() : base(WebPartManager.ConnectDisplayMode)
        {
            this._mode = ConnectionsZoneMode.ExistingConnections;
            this._pendingConnectionPointID = string.Empty;
            this._pendingConnectionType = ConnectionType.None;
            this._pendingSelectedValue = null;
            this._pendingConsumerID = string.Empty;
        }

        private void ClearPendingConnection()
        {
            this._pendingConnectionType = ConnectionType.None;
            this._pendingConnectionPointID = string.Empty;
            this._pendingSelectedValue = null;
            this._pendingConsumerID = string.Empty;
            this._pendingConsumer = null;
            this._pendingConsumerConnectionPoint = null;
            this._pendingProvider = null;
            this._pendingProviderConnectionPoint = null;
            this._pendingTransformerConfigurationControlTypeName = null;
            this._pendingConnectionID = null;
        }

        protected override void Close()
        {
            if (this.WebPartToConnect != null)
            {
                base.WebPartManager.EndWebPartConnecting();
            }
        }

        private void ConnectConsumer(string consumerConnectionPointID)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            if ((webPartToConnect == null) || webPartToConnect.IsClosed)
            {
                this.DisplayConnectionError();
            }
            else
            {
                ConsumerConnectionPoint consumerConnectionPoint = base.WebPartManager.GetConsumerConnectionPoint(webPartToConnect, consumerConnectionPointID);
                if (consumerConnectionPoint == null)
                {
                    this.DisplayConnectionError();
                }
                else
                {
                    this.EnsureChildControls();
                    if (((this._connectDropDownLists == null) || !this._connectDropDownLists.Contains(consumerConnectionPoint)) || ((this._connectionPointInfo == null) || !this._connectionPointInfo.Contains(consumerConnectionPoint)))
                    {
                        this.DisplayConnectionError();
                    }
                    else
                    {
                        DropDownList list = (DropDownList) this._connectDropDownLists[consumerConnectionPoint];
                        string str = this.Page.Request.Form[list.UniqueID];
                        if (!string.IsNullOrEmpty(str))
                        {
                            IDictionary dictionary = (IDictionary) this._connectionPointInfo[consumerConnectionPoint];
                            if ((dictionary == null) || !dictionary.Contains(str))
                            {
                                this.DisplayConnectionError();
                            }
                            else
                            {
                                ProviderInfo info = (ProviderInfo) dictionary[str];
                                Type transformerType = info.TransformerType;
                                if (transformerType != null)
                                {
                                    WebPartTransformer transformer = (WebPartTransformer) WebPartUtil.CreateObjectFromType(transformerType);
                                    if (this.GetConfigurationControl(transformer) == null)
                                    {
                                        if (base.WebPartManager.CanConnectWebParts(info.WebPart, info.ConnectionPoint, webPartToConnect, consumerConnectionPoint, transformer))
                                        {
                                            base.WebPartManager.ConnectWebParts(info.WebPart, info.ConnectionPoint, webPartToConnect, consumerConnectionPoint, transformer);
                                        }
                                        else
                                        {
                                            this.DisplayConnectionError();
                                        }
                                        this.Reset();
                                    }
                                    else
                                    {
                                        this._pendingConnectionType = ConnectionType.Consumer;
                                        this._pendingConnectionPointID = consumerConnectionPointID;
                                        this._pendingSelectedValue = str;
                                        this._mode = ConnectionsZoneMode.ConfiguringTransformer;
                                        base.ChildControlsCreated = false;
                                    }
                                }
                                else
                                {
                                    if (base.WebPartManager.CanConnectWebParts(info.WebPart, info.ConnectionPoint, webPartToConnect, consumerConnectionPoint))
                                    {
                                        base.WebPartManager.ConnectWebParts(info.WebPart, info.ConnectionPoint, webPartToConnect, consumerConnectionPoint);
                                    }
                                    else
                                    {
                                        this.DisplayConnectionError();
                                    }
                                    this.Reset();
                                }
                                list.SelectedValue = null;
                            }
                        }
                    }
                }
            }
        }

        private void ConnectProvider(string providerConnectionPointID)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            if ((webPartToConnect == null) || webPartToConnect.IsClosed)
            {
                this.DisplayConnectionError();
            }
            else
            {
                ProviderConnectionPoint providerConnectionPoint = base.WebPartManager.GetProviderConnectionPoint(webPartToConnect, providerConnectionPointID);
                if (providerConnectionPoint == null)
                {
                    this.DisplayConnectionError();
                }
                else
                {
                    this.EnsureChildControls();
                    if (((this._connectDropDownLists == null) || !this._connectDropDownLists.Contains(providerConnectionPoint)) || ((this._connectionPointInfo == null) || !this._connectionPointInfo.Contains(providerConnectionPoint)))
                    {
                        this.DisplayConnectionError();
                    }
                    else
                    {
                        DropDownList list = (DropDownList) this._connectDropDownLists[providerConnectionPoint];
                        string str = this.Page.Request.Form[list.UniqueID];
                        if (!string.IsNullOrEmpty(str))
                        {
                            IDictionary dictionary = (IDictionary) this._connectionPointInfo[providerConnectionPoint];
                            if ((dictionary == null) || !dictionary.Contains(str))
                            {
                                this.DisplayConnectionError();
                            }
                            else
                            {
                                ConsumerInfo info = (ConsumerInfo) dictionary[str];
                                Type transformerType = info.TransformerType;
                                if (transformerType != null)
                                {
                                    WebPartTransformer transformer = (WebPartTransformer) WebPartUtil.CreateObjectFromType(transformerType);
                                    if (this.GetConfigurationControl(transformer) == null)
                                    {
                                        if (base.WebPartManager.CanConnectWebParts(webPartToConnect, providerConnectionPoint, info.WebPart, info.ConnectionPoint, transformer))
                                        {
                                            base.WebPartManager.ConnectWebParts(webPartToConnect, providerConnectionPoint, info.WebPart, info.ConnectionPoint, transformer);
                                        }
                                        else
                                        {
                                            this.DisplayConnectionError();
                                        }
                                        this.Reset();
                                    }
                                    else
                                    {
                                        this._pendingConnectionType = ConnectionType.Provider;
                                        this._pendingConnectionPointID = providerConnectionPointID;
                                        this._pendingSelectedValue = str;
                                        this._mode = ConnectionsZoneMode.ConfiguringTransformer;
                                        base.ChildControlsCreated = false;
                                    }
                                }
                                else
                                {
                                    if (base.WebPartManager.CanConnectWebParts(webPartToConnect, providerConnectionPoint, info.WebPart, info.ConnectionPoint))
                                    {
                                        base.WebPartManager.ConnectWebParts(webPartToConnect, providerConnectionPoint, info.WebPart, info.ConnectionPoint);
                                    }
                                    else
                                    {
                                        this.DisplayConnectionError();
                                    }
                                    this.Reset();
                                }
                                list.SelectedValue = null;
                            }
                        }
                    }
                }
            }
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this._connectDropDownLists = new HybridDictionary();
            this._connectionPointInfo = new HybridDictionary();
            this._pendingTransformerConfigurationControl = null;
            WebPart webPartToConnect = this.WebPartToConnect;
            if ((webPartToConnect != null) && !webPartToConnect.IsClosed)
            {
                WebPartManager webPartManager = base.WebPartManager;
                foreach (ProviderConnectionPoint point in base.WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect))
                {
                    DropDownList child = new DropDownList {
                        ID = "_providerlist_" + point.ID,
                        EnableViewState = false
                    };
                    this._connectDropDownLists[point] = child;
                    this.Controls.Add(child);
                }
                foreach (ConsumerConnectionPoint point2 in base.WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect))
                {
                    DropDownList list2 = new DropDownList {
                        ID = "_consumerlist_" + point2.ID,
                        EnableViewState = false
                    };
                    this._connectDropDownLists[point2] = list2;
                    this.Controls.Add(list2);
                }
                this.SetDropDownProperties();
                if (this._pendingConnectionType == ConnectionType.Consumer)
                {
                    if (this.EnsurePendingData())
                    {
                        this._pendingProvider.ToControl();
                        this._pendingConsumer.ToControl();
                        if (this._pendingSelectedValue != null)
                        {
                            IDictionary dictionary = (IDictionary) this._connectionPointInfo[this._pendingConsumerConnectionPoint];
                            ProviderInfo info = (ProviderInfo) dictionary[this._pendingSelectedValue];
                            this._pendingTransformer = (WebPartTransformer) WebPartUtil.CreateObjectFromType(info.TransformerType);
                        }
                        this._pendingTransformerConfigurationControl = this.GetConfigurationControl(this._pendingTransformer);
                        if (this._pendingTransformerConfigurationControl != null)
                        {
                            ((ITransformerConfigurationControl) this._pendingTransformerConfigurationControl).Cancelled += new EventHandler(this.OnConfigurationControlCancelled);
                            ((ITransformerConfigurationControl) this._pendingTransformerConfigurationControl).Succeeded += new EventHandler(this.OnConfigurationControlSucceeded);
                            this.Controls.Add(this._pendingTransformerConfigurationControl);
                        }
                    }
                }
                else if ((this._pendingConnectionType == ConnectionType.Provider) && this.EnsurePendingData())
                {
                    this._pendingProvider.ToControl();
                    this._pendingConsumer.ToControl();
                    IDictionary dictionary2 = (IDictionary) this._connectionPointInfo[this._pendingProviderConnectionPoint];
                    ConsumerInfo info2 = (ConsumerInfo) dictionary2[this._pendingSelectedValue];
                    this._pendingTransformer = (WebPartTransformer) WebPartUtil.CreateObjectFromType(info2.TransformerType);
                    this._pendingTransformerConfigurationControl = this.GetConfigurationControl(this._pendingTransformer);
                    if (this._pendingTransformerConfigurationControl != null)
                    {
                        ((ITransformerConfigurationControl) this._pendingTransformerConfigurationControl).Cancelled += new EventHandler(this.OnConfigurationControlCancelled);
                        ((ITransformerConfigurationControl) this._pendingTransformerConfigurationControl).Succeeded += new EventHandler(this.OnConfigurationControlSucceeded);
                        this.Controls.Add(this._pendingTransformerConfigurationControl);
                    }
                }
                this.SetTransformerConfigurationControlProperties();
            }
        }

        private void Disconnect(string connectionID)
        {
            WebPartConnection connection = base.WebPartManager.Connections[connectionID];
            if (connection != null)
            {
                if ((connection.Provider != this.WebPartToConnect) && (connection.Consumer != this.WebPartToConnect))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ConnectionsZone_DisconnectInvalid"));
                }
                base.WebPartManager.DisconnectWebParts(connection);
            }
        }

        private void DisplayConnectionError()
        {
            this._displayErrorMessage = true;
            this.Reset();
        }

        private bool EnsurePendingData()
        {
            if (this.WebPartToConnect == null)
            {
                this.ClearPendingConnection();
                this._mode = ConnectionsZoneMode.ExistingConnections;
                return false;
            }
            if ((this._pendingConsumer != null) && (((this._pendingConsumerConnectionPoint == null) || (this._pendingProvider == null)) || (this._pendingProviderConnectionPoint == null)))
            {
                this.DisplayConnectionError();
                return false;
            }
            if (this._pendingConnectionType == ConnectionType.Provider)
            {
                this._pendingProvider = this.WebPartToConnect;
                this._pendingProviderConnectionPoint = base.WebPartManager.GetProviderConnectionPoint(this.WebPartToConnect, this._pendingConnectionPointID);
                if (this._pendingProviderConnectionPoint == null)
                {
                    this.DisplayConnectionError();
                    return false;
                }
                IDictionary dictionary = (IDictionary) this._connectionPointInfo[this._pendingProviderConnectionPoint];
                ConsumerInfo info = null;
                if (dictionary != null)
                {
                    info = (ConsumerInfo) dictionary[this._pendingSelectedValue];
                }
                if (info == null)
                {
                    this.DisplayConnectionError();
                    return false;
                }
                this._pendingConsumer = info.WebPart;
                this._pendingConsumerConnectionPoint = info.ConnectionPoint;
                return true;
            }
            string str = this._pendingConsumerID;
            if (this._pendingConnectionType == ConnectionType.Consumer)
            {
                if (!string.IsNullOrEmpty(this._pendingConnectionID))
                {
                    WebPartConnection connection = base.WebPartManager.Connections[this._pendingConnectionID];
                    if (connection != null)
                    {
                        this._pendingConnectionPointID = connection.ConsumerConnectionPointID;
                        this._pendingConsumer = connection.Consumer;
                        this._pendingConsumerConnectionPoint = connection.ConsumerConnectionPoint;
                        this._pendingConsumerID = connection.Consumer.ID;
                        this._pendingProvider = connection.Provider;
                        this._pendingProviderConnectionPoint = connection.ProviderConnectionPoint;
                        this._pendingTransformer = connection.Transformer;
                        this._pendingSelectedValue = null;
                        this._pendingConnectionType = ConnectionType.Consumer;
                        return true;
                    }
                    this.DisplayConnectionError();
                    return false;
                }
                if (string.IsNullOrEmpty(str))
                {
                    this._pendingConsumer = this.WebPartToConnect;
                }
                else
                {
                    this._pendingConsumer = base.WebPartManager.WebParts[str];
                }
                this._pendingConsumerConnectionPoint = base.WebPartManager.GetConsumerConnectionPoint(this._pendingConsumer, this._pendingConnectionPointID);
                if (this._pendingConsumerConnectionPoint == null)
                {
                    this.DisplayConnectionError();
                    return false;
                }
                if (!string.IsNullOrEmpty(this._pendingSelectedValue))
                {
                    IDictionary dictionary2 = (IDictionary) this._connectionPointInfo[this._pendingConsumerConnectionPoint];
                    ProviderInfo info2 = null;
                    if (dictionary2 != null)
                    {
                        info2 = (ProviderInfo) dictionary2[this._pendingSelectedValue];
                    }
                    if (info2 == null)
                    {
                        this.DisplayConnectionError();
                        return false;
                    }
                    this._pendingProvider = info2.WebPart;
                    this._pendingProviderConnectionPoint = info2.ConnectionPoint;
                }
                return true;
            }
            this.ClearPendingConnection();
            return false;
        }

        private Control GetConfigurationControl(WebPartTransformer transformer)
        {
            Control control = transformer.CreateConfigurationControl();
            if (control == null)
            {
                return null;
            }
            if (!(control is ITransformerConfigurationControl))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ConnectionsZone_MustImplementITransformerConfigurationControl"));
            }
            string assemblyQualifiedName = control.GetType().AssemblyQualifiedName;
            if ((this._pendingTransformerConfigurationControlTypeName != null) && (this._pendingTransformerConfigurationControlTypeName != assemblyQualifiedName))
            {
                this.DisplayConnectionError();
                return null;
            }
            this._pendingTransformerConfigurationControlTypeName = assemblyQualifiedName;
            return control;
        }

        private string GetDisplayTitle(WebPart part, ConnectionPoint connectionPoint, bool isConsumer)
        {
            if (part == null)
            {
                return System.Web.SR.GetString("Part_Unknown");
            }
            int num = isConsumer ? base.WebPartManager.GetConsumerConnectionPoints(part).Count : base.WebPartManager.GetProviderConnectionPoints(part).Count;
            if (num == 1)
            {
                return part.DisplayTitle;
            }
            return (part.DisplayTitle + " (" + ((connectionPoint != null) ? connectionPoint.DisplayName : System.Web.SR.GetString("Part_Unknown")) + ")");
        }

        private IDictionary GetValidConsumers(WebPart provider, ProviderConnectionPoint providerConnectionPoint, WebPartCollection webParts)
        {
            HybridDictionary dictionary = new HybridDictionary();
            if ((((providerConnectionPoint != null) && (provider != null)) && provider.AllowConnect) && (providerConnectionPoint.AllowsMultipleConnections || !base.WebPartManager.IsProviderConnected(provider, providerConnectionPoint)))
            {
                foreach (WebPart part in webParts)
                {
                    if ((part.AllowConnect && (part != provider)) && !part.IsClosed)
                    {
                        foreach (ConsumerConnectionPoint point in base.WebPartManager.GetConsumerConnectionPoints(part))
                        {
                            if (base.WebPartManager.CanConnectWebParts(provider, providerConnectionPoint, part, point))
                            {
                                dictionary.Add(part.ID + '$' + point.ID, new ConsumerInfo(part, point));
                            }
                            else
                            {
                                foreach (WebPartTransformer transformer in this.AvailableTransformers)
                                {
                                    if (base.WebPartManager.CanConnectWebParts(provider, providerConnectionPoint, part, point, transformer))
                                    {
                                        dictionary.Add(part.ID + '$' + point.ID, new ConsumerInfo(part, point, transformer.GetType()));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        private IDictionary GetValidProviders(WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint, WebPartCollection webParts)
        {
            HybridDictionary dictionary = new HybridDictionary();
            if ((((consumerConnectionPoint != null) && (consumer != null)) && consumer.AllowConnect) && (consumerConnectionPoint.AllowsMultipleConnections || !base.WebPartManager.IsConsumerConnected(consumer, consumerConnectionPoint)))
            {
                foreach (WebPart part in webParts)
                {
                    if ((part.AllowConnect && (part != consumer)) && !part.IsClosed)
                    {
                        foreach (ProviderConnectionPoint point in base.WebPartManager.GetProviderConnectionPoints(part))
                        {
                            if (base.WebPartManager.CanConnectWebParts(part, point, consumer, consumerConnectionPoint))
                            {
                                dictionary.Add(part.ID + '$' + point.ID, new ProviderInfo(part, point));
                            }
                            else
                            {
                                foreach (WebPartTransformer transformer in this.AvailableTransformers)
                                {
                                    if (base.WebPartManager.CanConnectWebParts(part, point, consumer, consumerConnectionPoint, transformer))
                                    {
                                        dictionary.Add(part.ID + '$' + point.ID, new ProviderInfo(part, point, transformer.GetType()));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        private bool HasConfigurationControl(WebPartTransformer transformer)
        {
            return (transformer.CreateConfigurationControl() != null);
        }

        protected internal override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 8)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_ControlState"));
                }
                base.LoadControlState(objArray[0]);
                if (objArray[1] != null)
                {
                    this._mode = (ConnectionsZoneMode) objArray[1];
                }
                if (objArray[2] != null)
                {
                    this._pendingConnectionPointID = (string) objArray[2];
                }
                if (objArray[3] != null)
                {
                    this._pendingConnectionType = (ConnectionType) objArray[3];
                }
                if (objArray[4] != null)
                {
                    this._pendingSelectedValue = (string) objArray[4];
                }
                if (objArray[5] != null)
                {
                    this._pendingConsumerID = (string) objArray[5];
                }
                if (objArray[6] != null)
                {
                    this._pendingTransformerConfigurationControlTypeName = (string) objArray[6];
                }
                if (objArray[7] != null)
                {
                    this._pendingConnectionID = (string) objArray[7];
                }
            }
            else
            {
                base.LoadControlState(null);
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 6)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.CancelVerb).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.CloseVerb).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.ConfigureVerb).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.ConnectVerb).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.DisconnectVerb).LoadViewState(objArray[5]);
                }
            }
        }

        private void OnConfigurationControlCancelled(object sender, EventArgs e)
        {
            this.Reset();
        }

        private void OnConfigurationControlSucceeded(object sender, EventArgs e)
        {
            this.EnsurePendingData();
            if ((this._pendingConnectionType == ConnectionType.Consumer) && !string.IsNullOrEmpty(this._pendingConnectionID))
            {
                base.WebPartManager.Personalization.SetDirty();
            }
            else if (base.WebPartManager.CanConnectWebParts(this._pendingProvider, this._pendingProviderConnectionPoint, this._pendingConsumer, this._pendingConsumerConnectionPoint, this._pendingTransformer))
            {
                base.WebPartManager.ConnectWebParts(this._pendingProvider, this._pendingProviderConnectionPoint, this._pendingConsumer, this._pendingConsumerConnectionPoint, this._pendingTransformer);
            }
            else
            {
                this.DisplayConnectionError();
            }
            this.Reset();
        }

        protected override void OnDisplayModeChanged(object sender, WebPartDisplayModeEventArgs e)
        {
            this.Reset();
            base.OnDisplayModeChanged(sender, e);
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                this.Page.RegisterRequiresControlState(this);
                this.Page.PreRenderComplete += new EventHandler(this.OnPagePreRenderComplete);
            }
        }

        private void OnPagePreRenderComplete(object sender, EventArgs e)
        {
            this.SetTransformerConfigurationControlProperties();
        }

        protected override void OnSelectedWebPartChanged(object sender, WebPartEventArgs e)
        {
            if ((base.WebPartManager != null) && (base.WebPartManager.DisplayMode == WebPartManager.ConnectDisplayMode))
            {
                this.Reset();
            }
            base.OnSelectedWebPartChanged(sender, e);
        }

        protected override void RaisePostBackEvent(string eventArgument)
        {
            if (this.WebPartToConnect == null)
            {
                this.ClearPendingConnection();
                this._mode = ConnectionsZoneMode.ExistingConnections;
            }
            else
            {
                string[] strArray = eventArgument.Split(new char[] { '$' });
                if ((strArray.Length == 2) && string.Equals(strArray[0], "disconnect", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.DisconnectVerb.Visible && this.DisconnectVerb.Enabled)
                    {
                        string connectionID = strArray[1];
                        this.Disconnect(connectionID);
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                    }
                }
                else if ((strArray.Length == 3) && string.Equals(strArray[0], "connect", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.ConnectVerb.Visible && this.ConnectVerb.Enabled)
                    {
                        string providerConnectionPointID = strArray[2];
                        if (string.Equals(strArray[1], "provider", StringComparison.OrdinalIgnoreCase))
                        {
                            this.ConnectProvider(providerConnectionPointID);
                        }
                        else
                        {
                            this.ConnectConsumer(providerConnectionPointID);
                        }
                    }
                }
                else if ((strArray.Length == 2) && string.Equals(strArray[0], "edit", StringComparison.OrdinalIgnoreCase))
                {
                    this._pendingConnectionID = strArray[1];
                    this._pendingConnectionType = ConnectionType.Consumer;
                    this._mode = ConnectionsZoneMode.ConfiguringTransformer;
                }
                else if (string.Equals(eventArgument, "connectconsumer", StringComparison.OrdinalIgnoreCase))
                {
                    this._mode = ConnectionsZoneMode.ConnectToConsumer;
                }
                else if (string.Equals(eventArgument, "connectprovider", StringComparison.OrdinalIgnoreCase))
                {
                    this._mode = ConnectionsZoneMode.ConnectToProvider;
                }
                else if (string.Equals(eventArgument, "close", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.CloseVerb.Visible && this.CloseVerb.Enabled)
                    {
                        this.Close();
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                    }
                }
                else if (string.Equals(eventArgument, "cancel", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.CancelVerb.Visible && this.CancelVerb.Enabled)
                    {
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                    }
                }
                else
                {
                    base.RaisePostBackEvent(eventArgument);
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.SetDropDownProperties();
            base.Render(writer);
        }

        private void RenderAddVerbs(HtmlTextWriter writer)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            WebPartCollection webParts = null;
            if (base.WebPartManager != null)
            {
                webParts = base.WebPartManager.WebParts;
            }
            if ((webPartToConnect != null) || base.DesignMode)
            {
                bool designMode = base.DesignMode;
                if (!designMode && (base.WebPartManager != null))
                {
                    foreach (ProviderConnectionPoint point in base.WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect))
                    {
                        if (this.GetValidConsumers(webPartToConnect, point, webParts).Count != 0)
                        {
                            designMode = true;
                            break;
                        }
                    }
                }
                if (designMode)
                {
                    ZoneLinkButton button = new ZoneLinkButton(this, "connectconsumer") {
                        Text = this.ConnectToConsumerText
                    };
                    button.ApplyStyle(base.VerbStyle);
                    button.Page = this.Page;
                    button.RenderControl(writer);
                    writer.WriteBreak();
                }
                bool flag2 = base.DesignMode;
                if (!flag2 && (base.WebPartManager != null))
                {
                    foreach (ConsumerConnectionPoint point2 in base.WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect))
                    {
                        if (this.GetValidProviders(webPartToConnect, point2, webParts).Count != 0)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
                if (flag2)
                {
                    ZoneLinkButton button2 = new ZoneLinkButton(this, "connectprovider") {
                        Text = this.ConnectToProviderText
                    };
                    button2.ApplyStyle(base.VerbStyle);
                    button2.Page = this.Page;
                    button2.RenderControl(writer);
                    writer.WriteBreak();
                }
                if (flag2 || designMode)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    writer.RenderEndTag();
                }
            }
        }

        protected override void RenderBody(HtmlTextWriter writer)
        {
            if (((this.PartChromeType == System.Web.UI.WebControls.WebParts.PartChromeType.Default) || (this.PartChromeType == System.Web.UI.WebControls.WebParts.PartChromeType.BorderOnly)) || (this.PartChromeType == System.Web.UI.WebControls.WebParts.PartChromeType.TitleAndBorder))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "Black");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "1px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "Solid");
            }
            base.RenderBodyTableBeginTag(writer);
            this.RenderErrorMessage(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            switch (this._mode)
            {
                case ConnectionsZoneMode.ConnectToConsumer:
                    this.RenderConnectToConsumersDropDowns(writer);
                    break;

                case ConnectionsZoneMode.ConnectToProvider:
                    this.RenderConnectToProvidersDropDowns(writer);
                    break;

                case ConnectionsZoneMode.ConfiguringTransformer:
                    if (this._pendingTransformerConfigurationControl != null)
                    {
                        this.RenderTransformerConfigurationHeader(writer);
                        this._pendingTransformerConfigurationControl.RenderControl(writer);
                    }
                    break;

                default:
                    this.RenderAddVerbs(writer);
                    this.RenderExistingConnections(writer);
                    break;
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
            WebZone.RenderBodyTableEndTag(writer);
        }

        private void RenderConnectToConsumersDropDowns(HtmlTextWriter writer)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            if (webPartToConnect != null)
            {
                ProviderConnectionPointCollection enabledProviderConnectionPoints = base.WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect);
                bool flag = true;
                Label label = new Label {
                    Page = this.Page,
                    AssociatedControlInControlTree = false
                };
                foreach (ProviderConnectionPoint point in enabledProviderConnectionPoints)
                {
                    DropDownList list = (DropDownList) this._connectDropDownLists[point];
                    if ((list != null) && list.Enabled)
                    {
                        if (flag)
                        {
                            string connectToConsumerTitle = this.ConnectToConsumerTitle;
                            if (!string.IsNullOrEmpty(connectToConsumerTitle))
                            {
                                label.Text = connectToConsumerTitle;
                                label.ApplyStyle(base.LabelStyle);
                                label.AssociatedControlID = string.Empty;
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            string connectToConsumerInstructionText = this.ConnectToConsumerInstructionText;
                            if (!string.IsNullOrEmpty(connectToConsumerInstructionText))
                            {
                                writer.WriteBreak();
                                label.Text = connectToConsumerInstructionText;
                                label.ApplyStyle(base.InstructionTextStyle);
                                label.AssociatedControlID = string.Empty;
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            flag = false;
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                        writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                        writer.RenderBeginTag(HtmlTextWriterTag.Table);
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        label.ApplyStyle(base.LabelStyle);
                        label.Text = this.SendText;
                        label.AssociatedControlID = string.Empty;
                        label.RenderControl(writer);
                        writer.RenderEndTag();
                        base.LabelStyle.AddAttributesToRender(writer, this);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.WriteEncodedText(point.DisplayName);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        label.Text = this.SendToText;
                        label.AssociatedControlID = list.ClientID;
                        label.RenderControl(writer);
                        writer.RenderEndTag();
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        list.ApplyStyle(base.EditUIStyle);
                        list.RenderControl(writer);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        WebPartVerb connectVerb = this.ConnectVerb;
                        connectVerb.EventArgument = string.Join('$'.ToString(CultureInfo.InvariantCulture), new string[] { "connect", "provider", point.ID });
                        this.RenderVerb(writer, connectVerb);
                        writer.RenderEndTag();
                    }
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                WebPartVerb cancelVerb = this.CancelVerb;
                cancelVerb.EventArgument = "cancel";
                this.RenderVerb(writer, cancelVerb);
                writer.RenderEndTag();
            }
        }

        private void RenderConnectToProvidersDropDowns(HtmlTextWriter writer)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            if (webPartToConnect != null)
            {
                ConsumerConnectionPointCollection enabledConsumerConnectionPoints = base.WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect);
                bool flag = true;
                Label label = new Label {
                    Page = this.Page,
                    AssociatedControlInControlTree = false
                };
                foreach (ConsumerConnectionPoint point in enabledConsumerConnectionPoints)
                {
                    DropDownList list = (DropDownList) this._connectDropDownLists[point];
                    if ((list != null) && list.Enabled)
                    {
                        if (flag)
                        {
                            string connectToProviderTitle = this.ConnectToProviderTitle;
                            if (!string.IsNullOrEmpty(connectToProviderTitle))
                            {
                                label.Text = connectToProviderTitle;
                                label.ApplyStyle(base.LabelStyle);
                                label.AssociatedControlID = string.Empty;
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            string connectToProviderInstructionText = this.ConnectToProviderInstructionText;
                            if (!string.IsNullOrEmpty(connectToProviderInstructionText))
                            {
                                writer.WriteBreak();
                                label.Text = connectToProviderInstructionText;
                                label.ApplyStyle(base.InstructionTextStyle);
                                label.AssociatedControlID = string.Empty;
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            flag = false;
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                        writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                        writer.RenderBeginTag(HtmlTextWriterTag.Table);
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        label.ApplyStyle(base.LabelStyle);
                        label.Text = this.GetText;
                        label.AssociatedControlID = string.Empty;
                        label.RenderControl(writer);
                        writer.RenderEndTag();
                        base.LabelStyle.AddAttributesToRender(writer, this);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.WriteEncodedText(point.DisplayName);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        label.Text = this.GetFromText;
                        label.AssociatedControlID = list.ClientID;
                        label.RenderControl(writer);
                        writer.RenderEndTag();
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        list.ApplyStyle(base.EditUIStyle);
                        list.RenderControl(writer);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        WebPartVerb connectVerb = this.ConnectVerb;
                        connectVerb.EventArgument = string.Join('$'.ToString(CultureInfo.InvariantCulture), new string[] { "connect", "consumer", point.ID });
                        this.RenderVerb(writer, connectVerb);
                        writer.RenderEndTag();
                    }
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                WebPartVerb cancelVerb = this.CancelVerb;
                cancelVerb.EventArgument = "cancel";
                this.RenderVerb(writer, cancelVerb);
                writer.RenderEndTag();
            }
        }

        private void RenderErrorMessage(HtmlTextWriter writer)
        {
            if (this._displayErrorMessage)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                TableCell cell = new TableCell();
                cell.ApplyStyle(base.ErrorStyle);
                cell.Text = this.NewConnectionErrorMessage;
                cell.RenderControl(writer);
                writer.RenderEndTag();
            }
        }

        private void RenderExistingConnection(HtmlTextWriter writer, string connectionPointName, string partTitle, string disconnectEventArg, string editEventArg, bool consumer, bool isActive)
        {
            Label label = new Label {
                Page = this.Page
            };
            label.ApplyStyle(base.LabelStyle);
            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            label.Text = consumer ? this.SendText : this.GetText;
            label.RenderControl(writer);
            writer.RenderEndTag();
            base.LabelStyle.AddAttributesToRender(writer, this);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.WriteEncodedText(connectionPointName);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            label.Text = consumer ? this.SendToText : this.GetFromText;
            label.RenderControl(writer);
            writer.RenderEndTag();
            base.LabelStyle.AddAttributesToRender(writer, this);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.WriteEncodedText(partTitle);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            WebPartVerb disconnectVerb = this.DisconnectVerb;
            disconnectVerb.EventArgument = disconnectEventArg;
            this.RenderVerb(writer, disconnectVerb);
            if (this.VerbButtonType == ButtonType.Link)
            {
                writer.Write("&nbsp;");
            }
            if (isActive)
            {
                WebPartVerb configureVerb = this.ConfigureVerb;
                if (editEventArg == null)
                {
                    configureVerb.Enabled = false;
                }
                else
                {
                    configureVerb.Enabled = true;
                    configureVerb.EventArgument = editEventArg;
                }
                this.RenderVerb(writer, configureVerb);
            }
            else
            {
                writer.WriteBreak();
                label.ApplyStyle(base.ErrorStyle);
                label.Text = this.ExistingConnectionErrorMessage;
                label.RenderControl(writer);
            }
            writer.RenderEndTag();
        }

        private void RenderExistingConnections(HtmlTextWriter writer)
        {
            WebPartManager webPartManager = base.WebPartManager;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            if (webPartManager != null)
            {
                WebPart webPartToConnect = this.WebPartToConnect;
                WebPartConnectionCollection connections = webPartManager.Connections;
                foreach (WebPartConnection connection in connections)
                {
                    if (connection.Provider == webPartToConnect)
                    {
                        if (!flag)
                        {
                            this.RenderInstructionTitle(writer);
                            this.RenderInstructionText(writer);
                            flag = true;
                        }
                        if (!flag2)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                            base.LabelStyle.AddAttributesToRender(writer, this);
                            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                            writer.Write(this.ConsumersTitle);
                            writer.RenderEndTag();
                            string consumersInstructionText = this.ConsumersInstructionText;
                            if (!string.IsNullOrEmpty(consumersInstructionText))
                            {
                                writer.WriteBreak();
                                Label label = new Label {
                                    Text = consumersInstructionText,
                                    Page = this.Page
                                };
                                label.ApplyStyle(base.InstructionTextStyle);
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            flag2 = true;
                        }
                        this.RenderExistingConsumerConnection(writer, connection);
                    }
                }
                if (flag2)
                {
                    writer.RenderEndTag();
                }
                foreach (WebPartConnection connection2 in connections)
                {
                    if (connection2.Consumer == webPartToConnect)
                    {
                        if (!flag)
                        {
                            this.RenderInstructionTitle(writer);
                            this.RenderInstructionText(writer);
                            flag = true;
                        }
                        if (!flag3)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                            base.LabelStyle.AddAttributesToRender(writer, this);
                            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                            writer.Write(this.ProvidersTitle);
                            writer.RenderEndTag();
                            string providersInstructionText = this.ProvidersInstructionText;
                            if (!string.IsNullOrEmpty(providersInstructionText))
                            {
                                writer.WriteBreak();
                                Label label2 = new Label {
                                    Text = providersInstructionText,
                                    Page = this.Page
                                };
                                label2.ApplyStyle(base.InstructionTextStyle);
                                label2.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            flag3 = true;
                        }
                        this.RenderExistingProviderConnection(writer, connection2);
                    }
                }
            }
            if (flag3)
            {
                writer.RenderEndTag();
            }
            if (flag)
            {
                writer.WriteBreak();
            }
            else
            {
                this.RenderNoExistingConnection(writer);
            }
        }

        private void RenderExistingConsumerConnection(HtmlTextWriter writer, WebPartConnection connection)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            ProviderConnectionPoint providerConnectionPoint = base.WebPartManager.GetProviderConnectionPoint(webPartToConnect, connection.ProviderConnectionPointID);
            WebPart consumer = connection.Consumer;
            ConsumerConnectionPoint consumerConnectionPoint = connection.ConsumerConnectionPoint;
            string partTitle = this.GetDisplayTitle(consumer, consumerConnectionPoint, true);
            string editEventArg = null;
            WebPartTransformer transformer = connection.Transformer;
            if ((transformer != null) && this.HasConfigurationControl(transformer))
            {
                editEventArg = "edit" + '$'.ToString(CultureInfo.InvariantCulture) + connection.ID;
            }
            bool isActive = (((providerConnectionPoint != null) && (consumerConnectionPoint != null)) && ((connection.Provider != null) && (connection.Consumer != null))) && connection.IsActive;
            char ch2 = '$';
            this.RenderExistingConnection(writer, (providerConnectionPoint != null) ? providerConnectionPoint.DisplayName : System.Web.SR.GetString("Part_Unknown"), partTitle, string.Join(ch2.ToString(CultureInfo.InvariantCulture), new string[] { "disconnect", connection.ID }), editEventArg, true, isActive);
        }

        private void RenderExistingProviderConnection(HtmlTextWriter writer, WebPartConnection connection)
        {
            WebPart webPartToConnect = this.WebPartToConnect;
            ConsumerConnectionPoint consumerConnectionPoint = base.WebPartManager.GetConsumerConnectionPoint(webPartToConnect, connection.ConsumerConnectionPointID);
            WebPart provider = connection.Provider;
            ProviderConnectionPoint providerConnectionPoint = connection.ProviderConnectionPoint;
            string partTitle = this.GetDisplayTitle(provider, providerConnectionPoint, false);
            string editEventArg = null;
            WebPartTransformer transformer = connection.Transformer;
            if ((transformer != null) && this.HasConfigurationControl(transformer))
            {
                editEventArg = "edit" + '$'.ToString(CultureInfo.InvariantCulture) + connection.ID;
            }
            bool isActive = (((providerConnectionPoint != null) && (consumerConnectionPoint != null)) && ((connection.Provider != null) && (connection.Consumer != null))) && connection.IsActive;
            char ch2 = '$';
            this.RenderExistingConnection(writer, (consumerConnectionPoint != null) ? consumerConnectionPoint.DisplayName : System.Web.SR.GetString("Part_Unknown"), partTitle, string.Join(ch2.ToString(CultureInfo.InvariantCulture), new string[] { "disconnect", connection.ID }), editEventArg, false, isActive);
        }

        private void RenderInstructionText(HtmlTextWriter writer)
        {
            string instructionText = this.InstructionText;
            if (!string.IsNullOrEmpty(instructionText))
            {
                Label label = new Label {
                    Text = instructionText,
                    Page = this.Page
                };
                label.ApplyStyle(base.InstructionTextStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        private void RenderInstructionTitle(HtmlTextWriter writer)
        {
            if ((this.PartChromeType != System.Web.UI.WebControls.WebParts.PartChromeType.None) && (this.PartChromeType != System.Web.UI.WebControls.WebParts.PartChromeType.BorderOnly))
            {
                string instructionTitle = this.InstructionTitle;
                if (!string.IsNullOrEmpty(instructionTitle))
                {
                    Label label = new Label();
                    if (this.WebPartToConnect != null)
                    {
                        label.Text = string.Format(CultureInfo.CurrentCulture, instructionTitle, new object[] { this.WebPartToConnect.DisplayTitle });
                    }
                    else
                    {
                        label.Text = instructionTitle;
                    }
                    label.Page = this.Page;
                    label.ApplyStyle(base.LabelStyle);
                    label.RenderControl(writer);
                    writer.WriteBreak();
                }
            }
        }

        private void RenderNoExistingConnection(HtmlTextWriter writer)
        {
            string noExistingConnectionTitle = this.NoExistingConnectionTitle;
            if (!string.IsNullOrEmpty(noExistingConnectionTitle))
            {
                Label label = new Label {
                    Text = noExistingConnectionTitle,
                    Page = this.Page
                };
                label.ApplyStyle(base.LabelStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
            string noExistingConnectionInstructionText = this.NoExistingConnectionInstructionText;
            if (!string.IsNullOrEmpty(noExistingConnectionInstructionText))
            {
                Label label2 = new Label {
                    Text = noExistingConnectionInstructionText,
                    Page = this.Page
                };
                label2.ApplyStyle(base.InstructionTextStyle);
                label2.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        private void RenderTransformerConfigurationHeader(HtmlTextWriter writer)
        {
            if (this.EnsurePendingData())
            {
                string text = null;
                string displayTitle = null;
                bool flag = this._pendingConsumer == this.WebPartToConnect;
                if ((this._pendingConnectionType == ConnectionType.Consumer) && flag)
                {
                    displayTitle = this._pendingProvider.DisplayTitle;
                    text = this._pendingConsumerConnectionPoint.DisplayName;
                }
                else
                {
                    displayTitle = this._pendingConsumer.DisplayTitle;
                    text = this._pendingProviderConnectionPoint.DisplayName;
                }
                Label label = new Label {
                    Page = this.Page
                };
                label.ApplyStyle(base.LabelStyle);
                label.Text = flag ? this.ConnectToProviderTitle : this.ConnectToConsumerTitle;
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
                label.ApplyStyle(base.InstructionTextStyle);
                label.Text = flag ? this.ConnectToProviderInstructionText : this.ConnectToConsumerInstructionText;
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                label.ApplyStyle(base.LabelStyle);
                label.Text = flag ? this.GetText : this.SendText;
                label.RenderControl(writer);
                writer.RenderEndTag();
                base.LabelStyle.AddAttributesToRender(writer, this);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.WriteEncodedText(text);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                label.Text = flag ? this.GetFromText : this.SendToText;
                label.RenderControl(writer);
                writer.RenderEndTag();
                base.LabelStyle.AddAttributesToRender(writer, this);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.WriteEncodedText(displayTitle);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.WriteBreak();
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
                writer.WriteBreak();
                label.ApplyStyle(base.LabelStyle);
                label.Text = this.ConfigureConnectionTitle;
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        protected override void RenderVerbs(HtmlTextWriter writer)
        {
            base.RenderVerbsInternal(writer, new WebPartVerb[] { this.CloseVerb });
        }

        private void Reset()
        {
            this.ClearPendingConnection();
            base.ChildControlsCreated = false;
            this._mode = ConnectionsZoneMode.ExistingConnections;
        }

        protected internal override object SaveControlState()
        {
            object obj2 = base.SaveControlState();
            if ((this._mode == ConnectionsZoneMode.ExistingConnections) && (obj2 == null))
            {
                return null;
            }
            return new object[] { obj2, this._mode, this._pendingConnectionPointID, this._pendingConnectionType, this._pendingSelectedValue, this._pendingConsumerID, this._pendingTransformerConfigurationControlTypeName, this._pendingConnectionID };
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._cancelVerb != null) ? ((IStateManager) this._cancelVerb).SaveViewState() : null, (this._closeVerb != null) ? ((IStateManager) this._closeVerb).SaveViewState() : null, (this._configureVerb != null) ? ((IStateManager) this._configureVerb).SaveViewState() : null, (this._connectVerb != null) ? ((IStateManager) this._connectVerb).SaveViewState() : null, (this._disconnectVerb != null) ? ((IStateManager) this._disconnectVerb).SaveViewState() : null };
            for (int i = 0; i < 6; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        private void SelectValueInList(ListControl list, string value)
        {
            if (list == null)
            {
                this.DisplayConnectionError();
            }
            else
            {
                ListItem item = list.Items.FindByValue(value);
                if (item != null)
                {
                    item.Selected = true;
                }
                else
                {
                    this.DisplayConnectionError();
                }
            }
        }

        private void SetDropDownProperties()
        {
            bool flag = false;
            WebPart webPartToConnect = this.WebPartToConnect;
            if ((webPartToConnect != null) && !webPartToConnect.IsClosed)
            {
                WebPartCollection webParts = base.WebPartManager.WebParts;
                foreach (ProviderConnectionPoint point in base.WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect))
                {
                    DropDownList list = (DropDownList) this._connectDropDownLists[point];
                    if (list != null)
                    {
                        list.Items.Clear();
                        list.SelectedIndex = 0;
                        IDictionary dictionary = this.GetValidConsumers(webPartToConnect, point, webParts);
                        if (dictionary.Count == 0)
                        {
                            list.Enabled = false;
                            list.Items.Add(new ListItem(System.Web.SR.GetString("ConnectionsZone_NoConsumers"), string.Empty));
                        }
                        else
                        {
                            list.Enabled = true;
                            list.Items.Add(new ListItem());
                            this._connectionPointInfo[point] = dictionary;
                            WebPartConnection connection = point.AllowsMultipleConnections ? null : base.WebPartManager.GetConnectionForProvider(webPartToConnect, point);
                            WebPart consumer = null;
                            ConsumerConnectionPoint consumerConnectionPoint = null;
                            if (connection != null)
                            {
                                consumer = connection.Consumer;
                                consumerConnectionPoint = connection.ConsumerConnectionPoint;
                                list.Enabled = false;
                            }
                            else
                            {
                                flag = true;
                            }
                            foreach (DictionaryEntry entry in dictionary)
                            {
                                ConsumerInfo info = (ConsumerInfo) entry.Value;
                                ListItem item = new ListItem {
                                    Text = this.GetDisplayTitle(info.WebPart, info.ConnectionPoint, true),
                                    Value = (string) entry.Key
                                };
                                if (((connection != null) && (info.WebPart == consumer)) && (info.ConnectionPoint == consumerConnectionPoint))
                                {
                                    item.Selected = true;
                                }
                                list.Items.Add(item);
                            }
                        }
                    }
                }
                foreach (ConsumerConnectionPoint point3 in base.WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect))
                {
                    DropDownList list2 = (DropDownList) this._connectDropDownLists[point3];
                    if (list2 != null)
                    {
                        list2.Items.Clear();
                        list2.SelectedIndex = 0;
                        IDictionary dictionary2 = this.GetValidProviders(webPartToConnect, point3, webParts);
                        if (dictionary2.Count == 0)
                        {
                            list2.Enabled = false;
                            list2.Items.Add(new ListItem(System.Web.SR.GetString("ConnectionsZone_NoProviders"), string.Empty));
                        }
                        else
                        {
                            list2.Enabled = true;
                            list2.Items.Add(new ListItem());
                            this._connectionPointInfo[point3] = dictionary2;
                            WebPartConnection connection2 = point3.AllowsMultipleConnections ? null : base.WebPartManager.GetConnectionForConsumer(webPartToConnect, point3);
                            WebPart provider = null;
                            ProviderConnectionPoint providerConnectionPoint = null;
                            if (connection2 != null)
                            {
                                provider = connection2.Provider;
                                providerConnectionPoint = connection2.ProviderConnectionPoint;
                                list2.Enabled = false;
                            }
                            else
                            {
                                flag = true;
                            }
                            foreach (DictionaryEntry entry2 in dictionary2)
                            {
                                ProviderInfo info2 = (ProviderInfo) entry2.Value;
                                ListItem item2 = new ListItem {
                                    Text = this.GetDisplayTitle(info2.WebPart, info2.ConnectionPoint, false),
                                    Value = (string) entry2.Key
                                };
                                if (((connection2 != null) && (info2.WebPart == provider)) && (info2.ConnectionPoint == providerConnectionPoint))
                                {
                                    item2.Selected = true;
                                }
                                list2.Items.Add(item2);
                            }
                        }
                    }
                }
                if (((this._pendingConnectionType == ConnectionType.Consumer) && (this._pendingSelectedValue != null)) && (this._pendingSelectedValue.Length > 0))
                {
                    this.EnsurePendingData();
                    if (this._pendingConsumerConnectionPoint == null)
                    {
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                        return;
                    }
                    DropDownList list3 = (DropDownList) this._connectDropDownLists[this._pendingConsumerConnectionPoint];
                    if (list3 == null)
                    {
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                        return;
                    }
                    this.SelectValueInList(list3, this._pendingSelectedValue);
                }
                else if (this._pendingConnectionType == ConnectionType.Provider)
                {
                    this.EnsurePendingData();
                    if (this._pendingProviderConnectionPoint == null)
                    {
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                        return;
                    }
                    DropDownList list4 = (DropDownList) this._connectDropDownLists[this._pendingProviderConnectionPoint];
                    if (list4 == null)
                    {
                        this._mode = ConnectionsZoneMode.ExistingConnections;
                        return;
                    }
                    this.SelectValueInList(list4, this._pendingSelectedValue);
                }
                if (!flag && ((this._mode == ConnectionsZoneMode.ConnectToConsumer) || (this._mode == ConnectionsZoneMode.ConnectToProvider)))
                {
                    this._mode = ConnectionsZoneMode.ExistingConnections;
                }
            }
        }

        private void SetTransformerConfigurationControlProperties()
        {
            if (this.EnsurePendingData())
            {
                Control control = this._pendingProvider.ToControl();
                Control control2 = this._pendingConsumer.ToControl();
                object providerData = this._pendingProviderConnectionPoint.GetObject(control);
                object data = this._pendingTransformer.Transform(providerData);
                this._pendingConsumerConnectionPoint.SetObject(control2, data);
                if (((this._pendingConnectionType == ConnectionType.Consumer) && (string.IsNullOrEmpty(this._pendingConnectionID) || this._pendingConsumerConnectionPoint.AllowsMultipleConnections)) || (this._pendingConnectionType == ConnectionType.Provider))
                {
                    this._pendingConsumerConnectionPoint.SetObject(control2, null);
                }
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._cancelVerb != null)
            {
                ((IStateManager) this._cancelVerb).TrackViewState();
            }
            if (this._closeVerb != null)
            {
                ((IStateManager) this._closeVerb).TrackViewState();
            }
            if (this._configureVerb != null)
            {
                ((IStateManager) this._configureVerb).TrackViewState();
            }
            if (this._connectVerb != null)
            {
                ((IStateManager) this._connectVerb).TrackViewState();
            }
            if (this._disconnectVerb != null)
            {
                ((IStateManager) this._disconnectVerb).TrackViewState();
            }
        }

        private ArrayList AvailableTransformers
        {
            get
            {
                if (this._availableTransformers == null)
                {
                    this._availableTransformers = new ArrayList();
                    foreach (Type type in base.WebPartManager.AvailableTransformers)
                    {
                        this._availableTransformers.Add(WebPartUtil.CreateObjectFromType(type));
                    }
                }
                return this._availableTransformers;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebCategory("Verbs"), WebSysDescription("ConnectionsZone_CancelVerb"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public virtual WebPartVerb CancelVerb
        {
            get
            {
                if (this._cancelVerb == null)
                {
                    this._cancelVerb = new WebPartConnectionsCancelVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._cancelVerb).TrackViewState();
                    }
                }
                return this._cancelVerb;
            }
        }

        [WebSysDescription("ConnectionsZone_CloseVerb"), NotifyParentProperty(true), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual WebPartVerb CloseVerb
        {
            get
            {
                if (this._closeVerb == null)
                {
                    this._closeVerb = new WebPartConnectionsCloseVerb();
                    this._closeVerb.EventArgument = "close";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._closeVerb).TrackViewState();
                    }
                }
                return this._closeVerb;
            }
        }

        [WebCategory("Appearance"), WebSysDefaultValue("ConnectionsZone_ConfigureConnectionTitle"), WebSysDescription("ConnectionsZone_ConfigureConnectionTitleDescription")]
        public virtual string ConfigureConnectionTitle
        {
            get
            {
                string str = (string) this.ViewState["ConfigureConnectionTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConfigureConnectionTitle");
            }
            set
            {
                this.ViewState["ConfigureConnectionTitle"] = value;
            }
        }

        [NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebSysDescription("ConnectionsZone_ConfigureVerb"), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs")]
        public virtual WebPartVerb ConfigureVerb
        {
            get
            {
                if (this._configureVerb == null)
                {
                    this._configureVerb = new WebPartConnectionsConfigureVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._configureVerb).TrackViewState();
                    }
                }
                return this._configureVerb;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_ConnectToConsumerInstructionText"), WebSysDescription("ConnectionsZone_ConnectToConsumerInstructionTextDescription"), WebCategory("Appearance")]
        public virtual string ConnectToConsumerInstructionText
        {
            get
            {
                string str = (string) this.ViewState["ConnectToConsumerInstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConnectToConsumerInstructionText");
            }
            set
            {
                this.ViewState["ConnectToConsumerInstructionText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ConnectionsZone_ConnectToConsumerTextDescription"), WebSysDefaultValue("ConnectionsZone_ConnectToConsumerText")]
        public virtual string ConnectToConsumerText
        {
            get
            {
                string str = (string) this.ViewState["ConnectToConsumerText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConnectToConsumerText");
            }
            set
            {
                this.ViewState["ConnectToConsumerText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDefaultValue("ConnectionsZone_ConnectToConsumerTitle"), WebSysDescription("ConnectionsZone_ConnectToConsumerTitleDescription")]
        public virtual string ConnectToConsumerTitle
        {
            get
            {
                string str = (string) this.ViewState["ConnectToConsumerTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConnectToConsumerTitle");
            }
            set
            {
                this.ViewState["ConnectToConsumerTitle"] = value;
            }
        }

        [WebSysDescription("ConnectionsZone_ConnectToProviderInstructionTextDescription"), WebCategory("Appearance"), WebSysDefaultValue("ConnectionsZone_ConnectToProviderInstructionText")]
        public virtual string ConnectToProviderInstructionText
        {
            get
            {
                string str = (string) this.ViewState["ConnectToProviderInstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConnectToProviderInstructionText");
            }
            set
            {
                this.ViewState["ConnectToProviderInstructionText"] = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_ConnectToProviderText"), WebCategory("Appearance"), WebSysDescription("ConnectionsZone_ConnectToProviderTextDescription")]
        public virtual string ConnectToProviderText
        {
            get
            {
                string str = (string) this.ViewState["ConnectToProviderText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConnectToProviderText");
            }
            set
            {
                this.ViewState["ConnectToProviderText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDefaultValue("ConnectionsZone_ConnectToProviderTitle"), WebSysDescription("ConnectionsZone_ConnectToProviderTitleDescription")]
        public virtual string ConnectToProviderTitle
        {
            get
            {
                string str = (string) this.ViewState["ConnectToProviderTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConnectToProviderTitle");
            }
            set
            {
                this.ViewState["ConnectToProviderTitle"] = value;
            }
        }

        [NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebSysDescription("ConnectionsZone_ConnectVerb"), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs")]
        public virtual WebPartVerb ConnectVerb
        {
            get
            {
                if (this._connectVerb == null)
                {
                    this._connectVerb = new WebPartConnectionsConnectVerb();
                    this._connectVerb.EventArgument = "connect";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._connectVerb).TrackViewState();
                    }
                }
                return this._connectVerb;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_ConsumersInstructionText"), WebCategory("Appearance"), WebSysDescription("ConnectionsZone_ConsumersInstructionTextDescription")]
        public virtual string ConsumersInstructionText
        {
            get
            {
                string str = (string) this.ViewState["ConsumersInstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConsumersInstructionText");
            }
            set
            {
                this.ViewState["ConsumersInstructionText"] = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_ConsumersTitle"), WebSysDescription("ConnectionsZone_ConsumersTitleDescription"), WebCategory("Appearance")]
        public virtual string ConsumersTitle
        {
            get
            {
                string str = (string) this.ViewState["ConsumersTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ConsumersTitle");
            }
            set
            {
                this.ViewState["ConsumersTitle"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs"), WebSysDescription("ConnectionsZone_DisconnectVerb"), DefaultValue((string) null)]
        public virtual WebPartVerb DisconnectVerb
        {
            get
            {
                if (this._disconnectVerb == null)
                {
                    this._disconnectVerb = new WebPartConnectionsDisconnectVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._disconnectVerb).TrackViewState();
                    }
                }
                return this._disconnectVerb;
            }
        }

        protected override bool Display
        {
            get
            {
                return (base.Display && (this.WebPartToConnect != null));
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string EmptyZoneText
        {
            get
            {
                return base.EmptyZoneText;
            }
            set
            {
                base.EmptyZoneText = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_WarningConnectionDisabled"), WebSysDescription("ConnectionsZone_WarningMessage"), WebCategory("Appearance")]
        public virtual string ExistingConnectionErrorMessage
        {
            get
            {
                string str = (string) this.ViewState["ExistingConnectionErrorMessage"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_WarningConnectionDisabled");
            }
            set
            {
                this.ViewState["ExistingConnectionErrorMessage"] = value;
            }
        }

        [WebSysDescription("ConnectionsZone_GetFromTextDescription"), WebSysDefaultValue("ConnectionsZone_GetFromText"), WebCategory("Appearance")]
        public virtual string GetFromText
        {
            get
            {
                string str = (string) this.ViewState["GetFromText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_GetFromText");
            }
            set
            {
                this.ViewState["GetFromText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ConnectionsZone_GetDescription"), WebSysDefaultValue("ConnectionsZone_Get")]
        public virtual string GetText
        {
            get
            {
                string str = (string) this.ViewState["GetText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_Get");
            }
            set
            {
                this.ViewState["GetText"] = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_HeaderText"), WebCategory("Appearance"), WebSysDescription("ConnectionsZone_HeaderTextDescription")]
        public override string HeaderText
        {
            get
            {
                string str = (string) this.ViewState["HeaderText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_HeaderText");
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        [WebSysDescription("ConnectionsZone_InstructionTextDescription"), WebSysDefaultValue("ConnectionsZone_InstructionText"), WebCategory("Appearance")]
        public override string InstructionText
        {
            get
            {
                string str = (string) this.ViewState["InstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_InstructionText");
            }
            set
            {
                this.ViewState["InstructionText"] = value;
            }
        }

        [WebSysDescription("ConnectionsZone_InstructionTitleDescription"), WebSysDefaultValue("ConnectionsZone_InstructionTitle"), WebCategory("Appearance")]
        public virtual string InstructionTitle
        {
            get
            {
                string str = (string) this.ViewState["InstructionTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_InstructionTitle");
            }
            set
            {
                this.ViewState["InstructionTitle"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ConnectionsZone_ErrorMessage"), WebSysDefaultValue("ConnectionsZone_ErrorCantContinueConnectionCreation")]
        public virtual string NewConnectionErrorMessage
        {
            get
            {
                string str = (string) this.ViewState["NewConnectionErrorMessage"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ErrorCantContinueConnectionCreation");
            }
            set
            {
                this.ViewState["NewConnectionErrorMessage"] = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_NoExistingConnectionInstructionText"), WebCategory("Appearance"), WebSysDescription("ConnectionsZone_NoExistingConnectionInstructionTextDescription")]
        public virtual string NoExistingConnectionInstructionText
        {
            get
            {
                string str = (string) this.ViewState["NoExistingConnectionInstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_NoExistingConnectionInstructionText");
            }
            set
            {
                this.ViewState["NoExistingConnectionInstructionText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDefaultValue("ConnectionsZone_NoExistingConnectionTitle"), WebSysDescription("ConnectionsZone_NoExistingConnectionTitleDescription")]
        public virtual string NoExistingConnectionTitle
        {
            get
            {
                string str = (string) this.ViewState["NoExistingConnectionTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_NoExistingConnectionTitle");
            }
            set
            {
                this.ViewState["NoExistingConnectionTitle"] = value;
            }
        }

        [Browsable(false), Themeable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Web.UI.WebControls.WebParts.PartChromeType PartChromeType
        {
            get
            {
                return base.PartChromeType;
            }
            set
            {
                base.PartChromeType = value;
            }
        }

        [WebSysDescription("ConnectionsZone_ProvidersInstructionTextDescription"), WebSysDefaultValue("ConnectionsZone_ProvidersInstructionText"), WebCategory("Appearance")]
        public virtual string ProvidersInstructionText
        {
            get
            {
                string str = (string) this.ViewState["ProvidersInstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ProvidersInstructionText");
            }
            set
            {
                this.ViewState["ProvidersInstructionText"] = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_ProvidersTitle"), WebSysDescription("ConnectionsZone_ProvidersTitleDescription"), WebCategory("Appearance")]
        public virtual string ProvidersTitle
        {
            get
            {
                string str = (string) this.ViewState["ProvidersTitle"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_ProvidersTitle");
            }
            set
            {
                this.ViewState["ProvidersTitle"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ConnectionsZone_SendTextDescription"), WebSysDefaultValue("ConnectionsZone_SendText")]
        public virtual string SendText
        {
            get
            {
                string str = (string) this.ViewState["SendText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_SendText");
            }
            set
            {
                this.ViewState["SendText"] = value;
            }
        }

        [WebSysDefaultValue("ConnectionsZone_SendToText"), WebCategory("Appearance"), WebSysDescription("ConnectionsZone_SendToTextDescription")]
        public virtual string SendToText
        {
            get
            {
                string str = (string) this.ViewState["SendToText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("ConnectionsZone_SendToText");
            }
            set
            {
                this.ViewState["SendToText"] = value;
            }
        }

        protected WebPart WebPartToConnect
        {
            get
            {
                if ((base.WebPartManager != null) && (base.WebPartManager.DisplayMode == WebPartManager.ConnectDisplayMode))
                {
                    return base.WebPartManager.SelectedWebPart;
                }
                return null;
            }
        }

        private abstract class ConnectionPointInfo
        {
            private Type _transformerType;
            private System.Web.UI.WebControls.WebParts.WebPart _webPart;

            protected ConnectionPointInfo(System.Web.UI.WebControls.WebParts.WebPart webPart)
            {
                this._webPart = webPart;
            }

            protected ConnectionPointInfo(System.Web.UI.WebControls.WebParts.WebPart webPart, Type transformerType) : this(webPart)
            {
                this._transformerType = transformerType;
            }

            public Type TransformerType
            {
                get
                {
                    return this._transformerType;
                }
            }

            public System.Web.UI.WebControls.WebParts.WebPart WebPart
            {
                get
                {
                    return this._webPart;
                }
            }
        }

        private enum ConnectionsZoneMode
        {
            ExistingConnections,
            ConnectToConsumer,
            ConnectToProvider,
            ConfiguringTransformer
        }

        private enum ConnectionType
        {
            None,
            Consumer,
            Provider
        }

        private sealed class ConsumerInfo : ConnectionsZone.ConnectionPointInfo
        {
            private ConsumerConnectionPoint _connectionPoint;

            public ConsumerInfo(WebPart webPart, ConsumerConnectionPoint connectionPoint) : base(webPart)
            {
                this._connectionPoint = connectionPoint;
            }

            public ConsumerInfo(WebPart webPart, ConsumerConnectionPoint connectionPoint, Type transformerType) : base(webPart, transformerType)
            {
                this._connectionPoint = connectionPoint;
            }

            public ConsumerConnectionPoint ConnectionPoint
            {
                get
                {
                    return this._connectionPoint;
                }
            }
        }

        private sealed class ProviderInfo : ConnectionsZone.ConnectionPointInfo
        {
            private ProviderConnectionPoint _connectionPoint;

            public ProviderInfo(WebPart webPart, ProviderConnectionPoint connectionPoint) : base(webPart)
            {
                this._connectionPoint = connectionPoint;
            }

            public ProviderInfo(WebPart webPart, ProviderConnectionPoint connectionPoint, Type transformerType) : base(webPart, transformerType)
            {
                this._connectionPoint = connectionPoint;
            }

            public ProviderConnectionPoint ConnectionPoint
            {
                get
                {
                    return this._connectionPoint;
                }
            }
        }
    }
}

