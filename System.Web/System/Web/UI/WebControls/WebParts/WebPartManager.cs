namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    [ParseChildren(true), NonVisualControl, Designer("System.Web.UI.Design.WebControls.WebParts.WebPartManagerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), PersistChildren(false), ViewStateModeById, Bindable(false)]
    public class WebPartManager : Control, INamingContainer, IPersonalizable
    {
        private bool _allowCreateDisplayTitles;
        private bool _allowEventCancellation = true;
        private TransformerTypeCollection _availableTransformers;
        private WebPartDisplayMode _displayMode = BrowseDisplayMode;
        private WebPartDisplayModeCollection _displayModes;
        private IDictionary _displayTitles;
        private WebPartConnectionCollection _dynamicConnections;
        private bool _hasDataChanged;
        private WebPartManagerInternals _internals;
        private PermissionSet _mediumPermissionSet;
        private PermissionSet _minimalPermissionSet;
        private bool _pageInitComplete;
        private IDictionary _partAndChildControlIDs = new HybridDictionary(true);
        private IDictionary _partsForZone;
        private WebPartPersonalization _personalization;
        private PersonalizationDictionary _personalizationState;
        private bool _renderClientScript;
        private WebPart _selectedWebPart;
        private WebPartConnectionCollection _staticConnections;
        private WebPartDisplayModeCollection _supportedDisplayModes;
        private bool? _usePermitOnly;
        private WebPartZoneCollection _webPartZones = new WebPartZoneCollection();
        private IDictionary _zoneIDs = new HybridDictionary(true);
        private const string AuthorizationFilterName = "AuthorizationFilter";
        private static readonly object AuthorizeWebPartEvent = new object();
        private const int baseIndex = 0;
        public static readonly WebPartDisplayMode BrowseDisplayMode = new BrowseWebPartDisplayMode();
        public static readonly WebPartDisplayMode CatalogDisplayMode = new CatalogWebPartDisplayMode();
        private const string CloseProviderWarningDeclaration = "CloseProviderWarningDeclaration";
        public static readonly WebPartDisplayMode ConnectDisplayMode = new ConnectWebPartDisplayMode();
        private static Hashtable ConnectionPointsCache;
        private static readonly object ConnectionsActivatedEvent = new object();
        private static readonly object ConnectionsActivatingEvent = new object();
        private const int controlStateArrayLength = 3;
        private const string DeleteWarningDeclaration = "DeleteWarningDeclaration";
        public static readonly WebPartDisplayMode DesignDisplayMode = new DesignWebPartDisplayMode();
        private static readonly object DisplayModeChangedEvent = new object();
        private static readonly object DisplayModeChangingEvent = new object();
        private const int displayModeIndex = 2;
        private static string[] displayTitleSuffix = new string[] { 
            " [0]", " [1]", " [2]", " [3]", " [4]", " [5]", " [6]", " [7]", " [8]", " [9]", " [10]", " [11]", " [12]", " [13]", " [14]", " [15]", 
            " [16]", " [17]", " [18]", " [19]", " [20]"
         };
        private const string DragOverlayElementHtmlTemplate = "\r\n<div id=\"{0}___Drag\" style=\"display:none; position:absolute; z-index: 32000; filter:alpha(opacity=75)\"></div>";
        private const string DynamicConnectionIDPrefix = "c";
        private const string DynamicWebPartIDPrefix = "wp";
        public static readonly WebPartDisplayMode EditDisplayMode = new EditWebPartDisplayMode();
        internal const string ExportDataElement = "data";
        internal const string ExportErrorMessageElement = "importErrorMessage";
        internal const string ExportGenericPartPropertiesElement = "genericWebPartProperties";
        internal const string ExportIPersonalizableElement = "ipersonalizable";
        internal const string ExportMetaDataElement = "metaData";
        internal const string ExportPartElement = "webPart";
        internal const string ExportPartNamespaceAttribute = "xmlns";
        internal const string ExportPartNamespaceValue = "http://schemas.microsoft.com/WebPart/v3";
        internal const string ExportPropertiesElement = "properties";
        internal const string ExportPropertyElement = "property";
        internal const string ExportPropertyNameAttribute = "name";
        internal const string ExportPropertyNullAttribute = "null";
        internal const string ExportPropertyScopeAttribute = "scope";
        internal const string ExportPropertyTypeAttribute = "type";
        internal const string ExportRootElement = "webParts";
        private const string ExportSensitiveDataWarningDeclaration = "ExportSensitiveDataWarningDeclaration";
        private const string ExportTypeBool = "bool";
        private const string ExportTypeChromeState = "chromestate";
        private const string ExportTypeChromeType = "chrometype";
        private const string ExportTypeColor = "color";
        private const string ExportTypeDateTime = "datetime";
        private const string ExportTypeDirection = "direction";
        private const string ExportTypeDouble = "double";
        internal const string ExportTypeElement = "type";
        private const string ExportTypeExportMode = "exportmode";
        private const string ExportTypeFontSize = "fontsize";
        private const string ExportTypeHelpMode = "helpmode";
        private const string ExportTypeInt = "int";
        internal const string ExportTypeNameAttribute = "name";
        private const string ExportTypeObject = "object";
        private const string ExportTypeSingle = "single";
        private const string ExportTypeString = "string";
        private const string ExportTypeUnit = "unit";
        internal const string ExportUserControlSrcAttribute = "src";
        private const string ImportErrorMessageName = "ImportErrorMessage";
        private static readonly object SelectedWebPartChangedEvent = new object();
        private static readonly object SelectedWebPartChangingEvent = new object();
        private const int selectedWebPartIndex = 1;
        private const string StartupScript = "\r\n<script type=\"text/javascript\">\r\n\r\n__wpm = new WebPartManager();\r\n__wpm.overlayContainerElement = {0};\r\n__wpm.personalizationScopeShared = {1};\r\n\r\nvar zoneElement;\r\nvar zoneObject;\r\n{2}\r\n</script>\r\n";
        private static readonly object WebPartAddedEvent = new object();
        private static readonly object WebPartAddingEvent = new object();
        private static readonly object WebPartClosedEvent = new object();
        private static readonly object WebPartClosingEvent = new object();
        private static readonly object WebPartDeletedEvent = new object();
        private static readonly object WebPartDeletingEvent = new object();
        private static readonly object WebPartMovedEvent = new object();
        private static readonly object WebPartMovingEvent = new object();
        private static readonly object WebPartsConnectedEvent = new object();
        private static readonly object WebPartsConnectingEvent = new object();
        private static readonly object WebPartsDisconnectedEvent = new object();
        private static readonly object WebPartsDisconnectingEvent = new object();
        private const string ZoneEndScript = "\r\n}";
        private const string ZoneIDName = "ZoneID";
        private const string ZoneIndexName = "ZoneIndex";
        private const string ZonePartScript = "\r\n    zoneObject.AddWebPart(document.getElementById('{0}'), {1}, {2});";
        private const string ZoneScript = "\r\nzoneElement = document.getElementById('{0}');\r\nif (zoneElement != null) {{\r\n    zoneObject = __wpm.AddZone(zoneElement, '{1}', {2}, {3}, '{4}');";

        [WebCategory("Action"), WebSysDescription("WebPartManager_AuthorizeWebPart")]
        public event WebPartAuthorizationEventHandler AuthorizeWebPart
        {
            add
            {
                base.Events.AddHandler(AuthorizeWebPartEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(AuthorizeWebPartEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_ConnectionsActivated")]
        public event EventHandler ConnectionsActivated
        {
            add
            {
                base.Events.AddHandler(ConnectionsActivatedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(ConnectionsActivatedEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_ConnectionsActivating")]
        public event EventHandler ConnectionsActivating
        {
            add
            {
                base.Events.AddHandler(ConnectionsActivatingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(ConnectionsActivatingEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_DisplayModeChanged"), WebCategory("Action")]
        public event WebPartDisplayModeEventHandler DisplayModeChanged
        {
            add
            {
                base.Events.AddHandler(DisplayModeChangedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(DisplayModeChangedEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_DisplayModeChanging"), WebCategory("Action")]
        public event WebPartDisplayModeCancelEventHandler DisplayModeChanging
        {
            add
            {
                base.Events.AddHandler(DisplayModeChangingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(DisplayModeChangingEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_SelectedWebPartChanged"), WebCategory("Action")]
        public event WebPartEventHandler SelectedWebPartChanged
        {
            add
            {
                base.Events.AddHandler(SelectedWebPartChangedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(SelectedWebPartChangedEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_SelectedWebPartChanging"), WebCategory("Action")]
        public event WebPartCancelEventHandler SelectedWebPartChanging
        {
            add
            {
                base.Events.AddHandler(SelectedWebPartChangingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(SelectedWebPartChangingEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_WebPartAdded"), WebCategory("Action")]
        public event WebPartEventHandler WebPartAdded
        {
            add
            {
                base.Events.AddHandler(WebPartAddedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartAddedEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_WebPartAdding"), WebCategory("Action")]
        public event WebPartAddingEventHandler WebPartAdding
        {
            add
            {
                base.Events.AddHandler(WebPartAddingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartAddingEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_WebPartClosed")]
        public event WebPartEventHandler WebPartClosed
        {
            add
            {
                base.Events.AddHandler(WebPartClosedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartClosedEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_WebPartClosing")]
        public event WebPartCancelEventHandler WebPartClosing
        {
            add
            {
                base.Events.AddHandler(WebPartClosingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartClosingEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_WebPartDeleted"), WebCategory("Action")]
        public event WebPartEventHandler WebPartDeleted
        {
            add
            {
                base.Events.AddHandler(WebPartDeletedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartDeletedEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_WebPartDeleting")]
        public event WebPartCancelEventHandler WebPartDeleting
        {
            add
            {
                base.Events.AddHandler(WebPartDeletingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartDeletingEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_WebPartMoved"), WebCategory("Action")]
        public event WebPartEventHandler WebPartMoved
        {
            add
            {
                base.Events.AddHandler(WebPartMovedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartMovedEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_WebPartMoving"), WebCategory("Action")]
        public event WebPartMovingEventHandler WebPartMoving
        {
            add
            {
                base.Events.AddHandler(WebPartMovingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartMovingEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_WebPartsConnected")]
        public event WebPartConnectionsEventHandler WebPartsConnected
        {
            add
            {
                base.Events.AddHandler(WebPartsConnectedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartsConnectedEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_WebPartsConnecting")]
        public event WebPartConnectionsCancelEventHandler WebPartsConnecting
        {
            add
            {
                base.Events.AddHandler(WebPartsConnectingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartsConnectingEvent, value);
            }
        }

        [WebSysDescription("WebPartManager_WebPartsDisconnected"), WebCategory("Action")]
        public event WebPartConnectionsEventHandler WebPartsDisconnected
        {
            add
            {
                base.Events.AddHandler(WebPartsDisconnectedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartsDisconnectedEvent, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("WebPartManager_WebPartsDisconnecting")]
        public event WebPartConnectionsCancelEventHandler WebPartsDisconnecting
        {
            add
            {
                base.Events.AddHandler(WebPartsDisconnectingEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartsDisconnectingEvent, value);
            }
        }

        protected virtual void ActivateConnections()
        {
            try
            {
                this._allowEventCancellation = false;
                foreach (WebPartConnection connection in this.ConnectionsToActivate())
                {
                    connection.Activate();
                }
            }
            finally
            {
                this._allowEventCancellation = true;
            }
        }

        private WebPart AddDynamicWebPartToZone(WebPart webPart, WebPartZoneBase zone, int zoneIndex)
        {
            if (!this.IsAuthorized(webPart))
            {
                return null;
            }
            WebPart part = this.CopyWebPart(webPart);
            this.Internals.SetIsStatic(part, false);
            this.Internals.SetIsShared(part, this.Personalization.Scope == PersonalizationScope.Shared);
            this.AddWebPartToZone(part, zone, zoneIndex);
            this.Internals.AddWebPart(part);
            this.Personalization.CopyPersonalizationState(webPart, part);
            this.OnWebPartAdded(new WebPartEventArgs(part));
            return part;
        }

        internal void AddWebPart(WebPart webPart)
        {
            ((WebPartManagerControlCollection) this.Controls).AddWebPart(webPart);
        }

        public WebPart AddWebPart(WebPart webPart, WebPartZoneBase zone, int zoneIndex)
        {
            this.Personalization.EnsureEnabled(true);
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            if (!this._webPartZones.Contains(zone))
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_MustRegister"), "zone");
            }
            if (zoneIndex < 0)
            {
                throw new ArgumentOutOfRangeException("zoneIndex");
            }
            if ((webPart.Zone != null) && !webPart.IsClosed)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_AlreadyInZone"), "webPart");
            }
            WebPartAddingEventArgs e = new WebPartAddingEventArgs(webPart, zone, zoneIndex);
            this.OnWebPartAdding(e);
            if (this._allowEventCancellation && e.Cancel)
            {
                return null;
            }
            if (this.Controls.Contains(webPart))
            {
                WebPart part = webPart;
                this.AddWebPartToZone(webPart, zone, zoneIndex);
                this.OnWebPartAdded(new WebPartEventArgs(part));
                return part;
            }
            return this.AddDynamicWebPartToZone(webPart, zone, zoneIndex);
        }

        private void AddWebPartToDictionary(WebPart webPart)
        {
            if (this._partsForZone != null)
            {
                string zoneID = this.Internals.GetZoneID(webPart);
                if (!string.IsNullOrEmpty(zoneID))
                {
                    SortedList list = (SortedList) this._partsForZone[zoneID];
                    if (list == null)
                    {
                        list = new SortedList(new WebPart.ZoneIndexComparer());
                        this._partsForZone[zoneID] = list;
                    }
                    list.Add(webPart, null);
                }
            }
        }

        private void AddWebPartToZone(WebPart webPart, WebPartZoneBase zone, int zoneIndex)
        {
            int index;
            IList allWebPartsForZone = this.GetAllWebPartsForZone(zone);
            WebPartCollection webPartsForZone = this.GetWebPartsForZone(zone);
            if (zoneIndex < webPartsForZone.Count)
            {
                WebPart part = webPartsForZone[zoneIndex];
                index = allWebPartsForZone.IndexOf(part);
            }
            else
            {
                index = allWebPartsForZone.Count;
            }
            for (int i = 0; i < index; i++)
            {
                WebPart part2 = (WebPart) allWebPartsForZone[i];
                this.Internals.SetZoneIndex(part2, i);
            }
            for (int j = index; j < allWebPartsForZone.Count; j++)
            {
                WebPart part3 = (WebPart) allWebPartsForZone[j];
                this.Internals.SetZoneIndex(part3, j + 1);
            }
            this.Internals.SetZoneIndex(webPart, index);
            this.Internals.SetZoneID(webPart, zone.ID);
            this.Internals.SetIsClosed(webPart, false);
            this._hasDataChanged = true;
            this.AddWebPartToDictionary(webPart);
        }

        public virtual void BeginWebPartConnecting(WebPart webPart)
        {
            this.Personalization.EnsureEnabled(true);
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (webPart.IsClosed)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_CantBeginConnectingClosed"), "webPart");
            }
            if (!this.Controls.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
            if (this.DisplayMode != ConnectDisplayMode)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_MustBeInConnect"));
            }
            if (webPart == this.SelectedWebPart)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_AlreadyInConnect"), "webPart");
            }
            WebPartCancelEventArgs e = new WebPartCancelEventArgs(webPart);
            this.OnSelectedWebPartChanging(e);
            if (!this._allowEventCancellation || !e.Cancel)
            {
                if (this.SelectedWebPart != null)
                {
                    this.EndWebPartConnecting();
                    if (this.SelectedWebPart != null)
                    {
                        return;
                    }
                }
                this.SetSelectedWebPart(webPart);
                this.Internals.CallOnConnectModeChanged(webPart);
                this.OnSelectedWebPartChanged(new WebPartEventArgs(webPart));
            }
        }

        public virtual void BeginWebPartEditing(WebPart webPart)
        {
            this.Personalization.EnsureEnabled(true);
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (webPart.IsClosed)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_CantBeginEditingClosed"), "webPart");
            }
            if (!this.Controls.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
            if (this.DisplayMode != EditDisplayMode)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_MustBeInEdit"));
            }
            if (webPart == this.SelectedWebPart)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_AlreadyInEdit"), "webPart");
            }
            WebPartCancelEventArgs e = new WebPartCancelEventArgs(webPart);
            this.OnSelectedWebPartChanging(e);
            if (!this._allowEventCancellation || !e.Cancel)
            {
                if (this.SelectedWebPart != null)
                {
                    this.EndWebPartEditing();
                    if (this.SelectedWebPart != null)
                    {
                        return;
                    }
                }
                this.SetSelectedWebPart(webPart);
                this.Internals.CallOnEditModeChanged(webPart);
                this.OnSelectedWebPartChanged(new WebPartEventArgs(webPart));
            }
        }

        public bool CanConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint, WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint)
        {
            return this.CanConnectWebParts(provider, providerConnectionPoint, consumer, consumerConnectionPoint, null);
        }

        public virtual bool CanConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint, WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint, WebPartTransformer transformer)
        {
            return this.CanConnectWebPartsCore(provider, providerConnectionPoint, consumer, consumerConnectionPoint, transformer, false);
        }

        private bool CanConnectWebPartsCore(WebPart provider, ProviderConnectionPoint providerConnectionPoint, WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint, WebPartTransformer transformer, bool throwOnError)
        {
            if (!this.Personalization.IsModifiable)
            {
                if (!throwOnError)
                {
                    return false;
                }
                this.Personalization.EnsureEnabled(true);
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!this.Controls.Contains(provider))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "provider");
            }
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }
            if (!this.Controls.Contains(consumer))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "consumer");
            }
            if (providerConnectionPoint == null)
            {
                throw new ArgumentNullException("providerConnectionPoint");
            }
            if (consumerConnectionPoint == null)
            {
                throw new ArgumentNullException("consumerConnectionPoint");
            }
            Control control = provider.ToControl();
            Control control2 = consumer.ToControl();
            if (providerConnectionPoint.ControlType != control.GetType())
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_InvalidConnectionPoint"), "providerConnectionPoint");
            }
            if (consumerConnectionPoint.ControlType != control2.GetType())
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_InvalidConnectionPoint"), "consumerConnectionPoint");
            }
            if (provider == consumer)
            {
                if (throwOnError)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_CantConnectToSelf"));
                }
                return false;
            }
            if (provider.IsClosed)
            {
                if (throwOnError)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_CantConnectClosed", new object[] { provider.ID }));
                }
                return false;
            }
            if (consumer.IsClosed)
            {
                if (throwOnError)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_CantConnectClosed", new object[] { consumer.ID }));
                }
                return false;
            }
            if (!providerConnectionPoint.GetEnabled(control))
            {
                if (throwOnError)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_DisabledConnectionPoint", new object[] { providerConnectionPoint.ID, provider.ID }));
                }
                return false;
            }
            if (!consumerConnectionPoint.GetEnabled(control2))
            {
                if (throwOnError)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_DisabledConnectionPoint", new object[] { consumerConnectionPoint.ID, consumer.ID }));
                }
                return false;
            }
            if (!providerConnectionPoint.AllowsMultipleConnections)
            {
                foreach (WebPartConnection connection in this.Connections)
                {
                    if ((connection.Provider == provider) && (connection.ProviderConnectionPoint == providerConnectionPoint))
                    {
                        if (throwOnError)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_Duplicate", new object[] { providerConnectionPoint.ID, provider.ID }));
                        }
                        return false;
                    }
                }
            }
            if (!consumerConnectionPoint.AllowsMultipleConnections)
            {
                foreach (WebPartConnection connection2 in this.Connections)
                {
                    if ((connection2.Consumer == consumer) && (connection2.ConsumerConnectionPoint == consumerConnectionPoint))
                    {
                        if (throwOnError)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_Duplicate", new object[] { consumerConnectionPoint.ID, consumer.ID }));
                        }
                        return false;
                    }
                }
            }
            if (transformer == null)
            {
                if (providerConnectionPoint.InterfaceType != consumerConnectionPoint.InterfaceType)
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_NoCommonInterface", new string[] { providerConnectionPoint.DisplayName, provider.ID, consumerConnectionPoint.DisplayName, consumer.ID }));
                    }
                    return false;
                }
                ConnectionInterfaceCollection secondaryInterfaces = providerConnectionPoint.GetSecondaryInterfaces(control);
                if (!consumerConnectionPoint.SupportsConnection(control2, secondaryInterfaces))
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_IncompatibleSecondaryInterfaces", new string[] { consumerConnectionPoint.DisplayName, consumer.ID, providerConnectionPoint.DisplayName, provider.ID }));
                    }
                    return false;
                }
            }
            else
            {
                Type type = transformer.GetType();
                if (!this.AvailableTransformers.Contains(type))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_TransformerNotAvailable", new object[] { type.FullName }));
                }
                Type consumerType = WebPartTransformerAttribute.GetConsumerType(type);
                Type providerType = WebPartTransformerAttribute.GetProviderType(type);
                if (providerConnectionPoint.InterfaceType != consumerType)
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_IncompatibleProviderTransformer", new object[] { providerConnectionPoint.DisplayName, provider.ID, type.FullName }));
                    }
                    return false;
                }
                if (providerType != consumerConnectionPoint.InterfaceType)
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_IncompatibleConsumerTransformer", new object[] { type.FullName, consumerConnectionPoint.DisplayName, consumer.ID }));
                    }
                    return false;
                }
                if (!consumerConnectionPoint.SupportsConnection(control2, ConnectionInterfaceCollection.Empty))
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_ConsumerRequiresSecondaryInterfaces", new object[] { consumerConnectionPoint.DisplayName, consumer.ID }));
                    }
                    return false;
                }
            }
            return true;
        }

        protected virtual bool CheckRenderClientScript()
        {
            bool flag = false;
            if (this.EnableClientScript && (this.Page != null))
            {
                HttpBrowserCapabilities browser = this.Page.Request.Browser;
                if (browser.Win32 && (browser.MSDomVersion.CompareTo(new Version(5, 5)) >= 0))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private void CloseOrDeleteWebPart(WebPart webPart, bool delete)
        {
            this.Personalization.EnsureEnabled(true);
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (!this.Controls.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
            if (!delete && webPart.IsClosed)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_AlreadyClosed"), "webPart");
            }
            if (delete)
            {
                if (webPart.IsStatic)
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPartManager_CantDeleteStatic"), "webPart");
                }
                if (webPart.IsShared && (this.Personalization.Scope == PersonalizationScope.User))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPartManager_CantDeleteSharedInUserScope"), "webPart");
                }
            }
            WebPartCancelEventArgs e = new WebPartCancelEventArgs(webPart);
            if (delete)
            {
                this.OnWebPartDeleting(e);
            }
            else
            {
                this.OnWebPartClosing(e);
            }
            if (!this._allowEventCancellation || !e.Cancel)
            {
                if ((this.DisplayMode == ConnectDisplayMode) && (webPart == this.SelectedWebPart))
                {
                    this.EndWebPartConnecting();
                    if (this.SelectedWebPart != null)
                    {
                        return;
                    }
                }
                if ((this.DisplayMode == EditDisplayMode) && (webPart == this.SelectedWebPart))
                {
                    this.EndWebPartEditing();
                    if (this.SelectedWebPart != null)
                    {
                        return;
                    }
                }
                if (delete)
                {
                    this.Internals.CallOnDeleting(webPart);
                }
                else
                {
                    this.Internals.CallOnClosing(webPart);
                }
                if (!webPart.IsClosed)
                {
                    this.RemoveWebPartFromZone(webPart);
                }
                this.DisconnectWebPart(webPart);
                if (delete)
                {
                    this.Internals.RemoveWebPart(webPart);
                    this.OnWebPartDeleted(new WebPartEventArgs(webPart));
                }
                else
                {
                    this.OnWebPartClosed(new WebPartEventArgs(webPart));
                }
            }
        }

        private void CloseOrphanedParts()
        {
            if (this.HasControls())
            {
                try
                {
                    this._allowEventCancellation = false;
                    foreach (WebPart part in this.Controls)
                    {
                        if (part.IsOrphaned)
                        {
                            this.CloseWebPart(part);
                        }
                    }
                }
                finally
                {
                    this._allowEventCancellation = true;
                }
            }
        }

        public void CloseWebPart(WebPart webPart)
        {
            this.CloseOrDeleteWebPart(webPart, false);
        }

        private WebPartConnection[] ConnectionsToActivate()
        {
            ArrayList connectionsToActivate = new ArrayList();
            HybridDictionary connectionIDs = new HybridDictionary(true);
            WebPartConnection[] array = new WebPartConnection[this.StaticConnections.Count + this.DynamicConnections.Count];
            this.StaticConnections.CopyTo(array, 0);
            this.DynamicConnections.CopyTo(array, this.StaticConnections.Count);
            foreach (WebPartConnection connection in array)
            {
                this.ConnectionsToActivateHelper(connection, connectionIDs, connectionsToActivate);
            }
            WebPartConnection[] connectionArray2 = (WebPartConnection[]) connectionsToActivate.ToArray(typeof(WebPartConnection));
            foreach (WebPartConnection connection2 in connectionArray2)
            {
                if (!connection2.IsShared)
                {
                    ArrayList list2 = new ArrayList();
                    foreach (WebPartConnection connection3 in connectionsToActivate)
                    {
                        if (((connection2 != connection3) && connection3.IsShared) && connection2.ConflictsWith(connection3))
                        {
                            list2.Add(connection3);
                        }
                    }
                    foreach (WebPartConnection connection4 in list2)
                    {
                        this.DisconnectWebParts(connection4);
                        connectionsToActivate.Remove(connection4);
                    }
                }
            }
            connectionArray2 = (WebPartConnection[]) connectionsToActivate.ToArray(typeof(WebPartConnection));
            foreach (WebPartConnection connection5 in connectionArray2)
            {
                if (connection5.IsShared && !connection5.IsStatic)
                {
                    ArrayList list3 = new ArrayList();
                    foreach (WebPartConnection connection6 in connectionsToActivate)
                    {
                        if (((connection5 != connection6) && connection6.IsStatic) && connection5.ConflictsWith(connection6))
                        {
                            list3.Add(connection6);
                        }
                    }
                    foreach (WebPartConnection connection7 in list3)
                    {
                        this.DisconnectWebParts(connection7);
                        connectionsToActivate.Remove(connection7);
                    }
                }
            }
            ArrayList list4 = new ArrayList();
            foreach (WebPartConnection connection8 in connectionsToActivate)
            {
                bool flag = false;
                foreach (WebPartConnection connection9 in connectionsToActivate)
                {
                    if (connection8 != connection9)
                    {
                        if (connection8.ConflictsWithConsumer(connection9))
                        {
                            connection8.Consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_Duplicate", new object[] { connection8.ConsumerConnectionPoint.DisplayName, connection8.Consumer.DisplayTitle }));
                            flag = true;
                        }
                        if (connection8.ConflictsWithProvider(connection9))
                        {
                            connection8.Consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_Duplicate", new object[] { connection8.ProviderConnectionPoint.DisplayName, connection8.Provider.DisplayTitle }));
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    list4.Add(connection8);
                }
            }
            this.StaticConnections.SetReadOnly("WebPartManager_StaticConnectionsReadOnly");
            this.DynamicConnections.SetReadOnly("WebPartManager_DynamicConnectionsReadOnly");
            return (WebPartConnection[]) list4.ToArray(typeof(WebPartConnection));
        }

        private void ConnectionsToActivateHelper(WebPartConnection connection, IDictionary connectionIDs, ArrayList connectionsToActivate)
        {
            string iD = connection.ID;
            if (string.IsNullOrEmpty(iD))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_NoID"));
            }
            if (connectionIDs.Contains(iD))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_DuplicateConnectionID", new object[] { iD }));
            }
            connectionIDs.Add(iD, null);
            if (!connection.Deleted)
            {
                WebPart provider = connection.Provider;
                if (provider == null)
                {
                    if (connection.IsStatic)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_NoProvider", new object[] { connection.ProviderID }));
                    }
                    this.DisconnectWebParts(connection);
                }
                else
                {
                    WebPart consumer = connection.Consumer;
                    if (consumer == null)
                    {
                        if (connection.IsStatic)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_NoConsumer", new object[] { connection.ConsumerID }));
                        }
                        this.DisconnectWebParts(connection);
                    }
                    else if (!(provider is ProxyWebPart) && !(consumer is ProxyWebPart))
                    {
                        Control control = provider.ToControl();
                        Control control2 = consumer.ToControl();
                        if (control == control2)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_CantConnectToSelf"));
                        }
                        if (connection.ProviderConnectionPoint == null)
                        {
                            consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_NoProviderConnectionPoint", new object[] { connection.ProviderConnectionPointID, provider.DisplayTitle }));
                        }
                        else if (connection.ConsumerConnectionPoint == null)
                        {
                            consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_NoConsumerConnectionPoint", new object[] { connection.ConsumerConnectionPointID, consumer.DisplayTitle }));
                        }
                        else
                        {
                            connectionsToActivate.Add(connection);
                        }
                    }
                }
            }
        }

        public WebPartConnection ConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint, WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint)
        {
            return this.ConnectWebParts(provider, providerConnectionPoint, consumer, consumerConnectionPoint, null);
        }

        public virtual WebPartConnection ConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint, WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint, WebPartTransformer transformer)
        {
            this.CanConnectWebPartsCore(provider, providerConnectionPoint, consumer, consumerConnectionPoint, transformer, true);
            if (this.DynamicConnections.IsReadOnly)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_ConnectTooLate"));
            }
            WebPartConnectionsCancelEventArgs e = new WebPartConnectionsCancelEventArgs(provider, providerConnectionPoint, consumer, consumerConnectionPoint);
            this.OnWebPartsConnecting(e);
            if (this._allowEventCancellation && e.Cancel)
            {
                return null;
            }
            Control control = provider.ToControl();
            Control control2 = consumer.ToControl();
            WebPartConnection connection = new WebPartConnection {
                ID = this.CreateDynamicConnectionID(),
                ProviderID = control.ID,
                ConsumerID = control2.ID,
                ProviderConnectionPointID = providerConnectionPoint.ID,
                ConsumerConnectionPointID = consumerConnectionPoint.ID
            };
            if (transformer != null)
            {
                this.Internals.SetTransformer(connection, transformer);
            }
            this.Internals.SetIsShared(connection, this.Personalization.Scope == PersonalizationScope.Shared);
            this.Internals.SetIsStatic(connection, false);
            this.DynamicConnections.Add(connection);
            this._hasDataChanged = true;
            this.OnWebPartsConnected(new WebPartConnectionsEventArgs(provider, providerConnectionPoint, consumer, consumerConnectionPoint, connection));
            return connection;
        }

        protected virtual WebPart CopyWebPart(WebPart webPart)
        {
            WebPart part;
            GenericWebPart part2 = webPart as GenericWebPart;
            if (part2 != null)
            {
                Control childControl = part2.ChildControl;
                this.VerifyType(childControl);
                Type type = childControl.GetType();
                Control control = (Control) this.Internals.CreateObjectFromType(type);
                control.ID = this.CreateDynamicWebPartID(type);
                part = this.CreateWebPart(control);
            }
            else
            {
                this.VerifyType(webPart);
                part = (WebPart) this.Internals.CreateObjectFromType(webPart.GetType());
            }
            part.ID = this.CreateDynamicWebPartID(webPart.GetType());
            return part;
        }

        protected virtual TransformerTypeCollection CreateAvailableTransformers()
        {
            TransformerTypeCollection types = new TransformerTypeCollection();
            foreach (Type type in RuntimeConfig.GetConfig().WebParts.Transformers.GetTransformerEntries().Values)
            {
                types.Add(type);
            }
            return types;
        }

        private static ICollection[] CreateConnectionPoints(Type type)
        {
            ArrayList connectionPoints = new ArrayList();
            ArrayList list2 = new ArrayList();
            foreach (MethodInfo info in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                object[] customAttributes = info.GetCustomAttributes(typeof(ConnectionConsumerAttribute), true);
                if (customAttributes.Length == 1)
                {
                    ConsumerConnectionPoint point;
                    ParameterInfo[] parameters = info.GetParameters();
                    Type interfaceType = null;
                    if (parameters.Length == 1)
                    {
                        interfaceType = parameters[0].ParameterType;
                    }
                    if ((!info.IsPublic || (info.ReturnType != typeof(void))) || (interfaceType == null))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_InvalidConsumerSignature", new object[] { info.Name, type.FullName }));
                    }
                    ConnectionConsumerAttribute attribute = customAttributes[0] as ConnectionConsumerAttribute;
                    string displayName = attribute.DisplayName;
                    string iD = attribute.ID;
                    Type connectionPointType = attribute.ConnectionPointType;
                    bool allowsMultipleConnections = attribute.AllowsMultipleConnections;
                    if (connectionPointType == null)
                    {
                        point = new ConsumerConnectionPoint(info, interfaceType, type, displayName, iD, allowsMultipleConnections);
                    }
                    else
                    {
                        object[] args = new object[] { info, interfaceType, type, displayName, iD, allowsMultipleConnections };
                        point = (ConsumerConnectionPoint) Activator.CreateInstance(connectionPointType, args);
                    }
                    connectionPoints.Add(point);
                }
                object[] objArray3 = info.GetCustomAttributes(typeof(ConnectionProviderAttribute), true);
                if (objArray3.Length == 1)
                {
                    ProviderConnectionPoint point2;
                    Type returnType = info.ReturnType;
                    if ((!info.IsPublic || (returnType == typeof(void))) || (info.GetParameters().Length != 0))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_InvalidProviderSignature", new object[] { info.Name, type.FullName }));
                    }
                    ConnectionProviderAttribute attribute2 = objArray3[0] as ConnectionProviderAttribute;
                    string str3 = attribute2.DisplayName;
                    string id = attribute2.ID;
                    Type type5 = attribute2.ConnectionPointType;
                    bool flag2 = attribute2.AllowsMultipleConnections;
                    if (type5 == null)
                    {
                        point2 = new ProviderConnectionPoint(info, returnType, type, str3, id, flag2);
                    }
                    else
                    {
                        object[] objArray4 = new object[] { info, returnType, type, str3, id, flag2 };
                        point2 = (ProviderConnectionPoint) Activator.CreateInstance(type5, objArray4);
                    }
                    list2.Add(point2);
                }
            }
            return new ICollection[] { new ConsumerConnectionPointCollection(connectionPoints), new ProviderConnectionPointCollection(list2) };
        }

        protected sealed override ControlCollection CreateControlCollection()
        {
            return new WebPartManagerControlCollection(this);
        }

        protected virtual WebPartDisplayModeCollection CreateDisplayModes()
        {
            WebPartDisplayModeCollection modes = new WebPartDisplayModeCollection();
            modes.Add(BrowseDisplayMode);
            modes.Add(CatalogDisplayMode);
            modes.Add(DesignDisplayMode);
            modes.Add(EditDisplayMode);
            modes.Add(ConnectDisplayMode);
            return modes;
        }

        private string CreateDisplayTitle(string title, WebPart webPart, int count)
        {
            string str = title;
            if (webPart.Hidden)
            {
                str = System.Web.SR.GetString("WebPart_HiddenFormatString", new object[] { str });
            }
            if (webPart is ErrorWebPart)
            {
                str = System.Web.SR.GetString("WebPart_ErrorFormatString", new object[] { str });
            }
            if (count == 0)
            {
                return str;
            }
            if (count < displayTitleSuffix.Length)
            {
                return (str + displayTitleSuffix[count]);
            }
            return (str + " [" + count.ToString(CultureInfo.CurrentCulture) + "]");
        }

        private IDictionary CreateDisplayTitles()
        {
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            foreach (WebPart part in this.Controls)
            {
                string title = part.Title;
                if (string.IsNullOrEmpty(title))
                {
                    title = System.Web.SR.GetString("Part_Untitled");
                }
                if (part is UnauthorizedWebPart)
                {
                    hashtable[part] = title;
                }
                else
                {
                    ArrayList list = (ArrayList) hashtable2[title];
                    if (list == null)
                    {
                        list = new ArrayList();
                        hashtable2[title] = list;
                        hashtable[part] = this.CreateDisplayTitle(title, part, 0);
                    }
                    else
                    {
                        int count = list.Count;
                        if (count == 1)
                        {
                            WebPart webPart = (WebPart) list[0];
                            hashtable[webPart] = this.CreateDisplayTitle(title, webPart, 1);
                        }
                        hashtable[part] = this.CreateDisplayTitle(title, part, count + 1);
                    }
                    list.Add(part);
                }
            }
            return hashtable;
        }

        protected virtual string CreateDynamicConnectionID()
        {
            int num = Math.Abs(Guid.NewGuid().GetHashCode());
            return ("c" + num.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual string CreateDynamicWebPartID(Type webPartType)
        {
            if (webPartType == null)
            {
                throw new ArgumentNullException("webPartType");
            }
            string str = "wp" + Math.Abs(Guid.NewGuid().GetHashCode()).ToString(CultureInfo.InvariantCulture);
            if ((this.Page != null) && this.Page.Trace.IsEnabled)
            {
                str = str + webPartType.Name;
            }
            return str;
        }

        protected virtual ErrorWebPart CreateErrorWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID, string errorMessage)
        {
            return new ErrorWebPart(originalID, originalTypeName, originalPath, genericWebPartID) { ErrorMessage = errorMessage };
        }

        protected virtual WebPartPersonalization CreatePersonalization()
        {
            return new WebPartPersonalization(this);
        }

        public virtual GenericWebPart CreateWebPart(Control control)
        {
            return CreateWebPartStatic(control);
        }

        internal static GenericWebPart CreateWebPartStatic(Control control)
        {
            GenericWebPart part = new GenericWebPart(control);
            part.CreateChildControls();
            return part;
        }

        public void DeleteWebPart(WebPart webPart)
        {
            this.CloseOrDeleteWebPart(webPart, true);
        }

        protected virtual void DisconnectWebPart(WebPart webPart)
        {
            try
            {
                this._allowEventCancellation = false;
                foreach (WebPartConnection connection in this.Connections)
                {
                    if ((connection.Provider == webPart) || (connection.Consumer == webPart))
                    {
                        this.DisconnectWebParts(connection);
                    }
                }
            }
            finally
            {
                this._allowEventCancellation = true;
            }
        }

        public virtual void DisconnectWebParts(WebPartConnection connection)
        {
            this.Personalization.EnsureEnabled(true);
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            WebPart provider = connection.Provider;
            ProviderConnectionPoint providerConnectionPoint = connection.ProviderConnectionPoint;
            WebPart consumer = connection.Consumer;
            ConsumerConnectionPoint consumerConnectionPoint = connection.ConsumerConnectionPoint;
            WebPartConnectionsCancelEventArgs e = new WebPartConnectionsCancelEventArgs(provider, providerConnectionPoint, consumer, consumerConnectionPoint, connection);
            this.OnWebPartsDisconnecting(e);
            if (!this._allowEventCancellation || !e.Cancel)
            {
                WebPartConnectionsEventArgs args2 = new WebPartConnectionsEventArgs(provider, providerConnectionPoint, consumer, consumerConnectionPoint);
                if (this.StaticConnections.Contains(connection))
                {
                    if (this.StaticConnections.IsReadOnly)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_DisconnectTooLate"));
                    }
                    if (this.Internals.ConnectionDeleted(connection))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_AlreadyDisconnected"));
                    }
                    this.Internals.DeleteConnection(connection);
                    this._hasDataChanged = true;
                    this.OnWebPartsDisconnected(args2);
                }
                else
                {
                    if (!this.DynamicConnections.Contains(connection))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("WebPartManager_UnknownConnection"), "connection");
                    }
                    if (this.DynamicConnections.IsReadOnly)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_DisconnectTooLate"));
                    }
                    if (this.ShouldRemoveConnection(connection))
                    {
                        this.DynamicConnections.Remove(connection);
                    }
                    else
                    {
                        if (this.Internals.ConnectionDeleted(connection))
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_AlreadyDisconnected"));
                        }
                        this.Internals.DeleteConnection(connection);
                    }
                    this._hasDataChanged = true;
                    this.OnWebPartsDisconnected(args2);
                }
            }
        }

        public virtual void EndWebPartConnecting()
        {
            this.Personalization.EnsureEnabled(true);
            WebPart selectedWebPart = this.SelectedWebPart;
            if (selectedWebPart == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_NoSelectedWebPartConnect"));
            }
            WebPartCancelEventArgs e = new WebPartCancelEventArgs(selectedWebPart);
            this.OnSelectedWebPartChanging(e);
            if (!this._allowEventCancellation || !e.Cancel)
            {
                this.SetSelectedWebPart(null);
                this.Internals.CallOnConnectModeChanged(selectedWebPart);
                this.OnSelectedWebPartChanged(new WebPartEventArgs(null));
            }
        }

        public virtual void EndWebPartEditing()
        {
            this.Personalization.EnsureEnabled(true);
            WebPart selectedWebPart = this.SelectedWebPart;
            if (selectedWebPart == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_NoSelectedWebPartEdit"));
            }
            WebPartCancelEventArgs e = new WebPartCancelEventArgs(selectedWebPart);
            this.OnSelectedWebPartChanging(e);
            if (!this._allowEventCancellation || !e.Cancel)
            {
                this.SetSelectedWebPart(null);
                this.Internals.CallOnEditModeChanged(selectedWebPart);
                this.OnSelectedWebPartChanged(new WebPartEventArgs(null));
            }
        }

        private void ExportIPersonalizable(XmlWriter writer, Control control, bool excludeSensitive)
        {
            IPersonalizable personalizable = control as IPersonalizable;
            if (personalizable != null)
            {
                PersonalizationDictionary state = new PersonalizationDictionary();
                personalizable.Save(state);
                if (state.Count > 0)
                {
                    writer.WriteStartElement("ipersonalizable");
                    this.ExportToWriter(state, writer, true, excludeSensitive);
                    writer.WriteEndElement();
                }
            }
        }

        private static void ExportProperty(XmlWriter writer, string name, string value, Type type, PersonalizationScope scope, bool isIPersonalizable)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("type", GetExportName(type));
            if (isIPersonalizable)
            {
                writer.WriteAttributeString("scope", scope.ToString());
            }
            if (value == null)
            {
                writer.WriteAttributeString("null", "true");
            }
            else
            {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
        }

        private void ExportToWriter(IDictionary propBag, XmlWriter writer)
        {
            this.ExportToWriter(propBag, writer, false, false);
        }

        private void ExportToWriter(IDictionary propBag, XmlWriter writer, bool isIPersonalizable, bool excludeSensitive)
        {
            foreach (DictionaryEntry entry in propBag)
            {
                string key = (string) entry.Key;
                switch (key)
                {
                    case "AuthorizationFilter":
                    case "ImportErrorMessage":
                        break;

                    default:
                    {
                        string str2;
                        PropertyInfo propertyInfo = null;
                        object propertyValue = null;
                        Pair pair = entry.Value as Pair;
                        PersonalizationScope user = PersonalizationScope.User;
                        if (!isIPersonalizable && (pair != null))
                        {
                            propertyInfo = (PropertyInfo) pair.First;
                            propertyValue = pair.Second;
                        }
                        else if (isIPersonalizable)
                        {
                            PersonalizationEntry entry2 = entry.Value as PersonalizationEntry;
                            if ((entry2 != null) && ((this.Personalization.Scope == PersonalizationScope.Shared) || (entry2.Scope == PersonalizationScope.User)))
                            {
                                propertyValue = entry2.Value;
                                user = entry2.Scope;
                            }
                            if (excludeSensitive && entry2.IsSensitive)
                            {
                                break;
                            }
                        }
                        Type propertyValueType = (propertyInfo != null) ? propertyInfo.PropertyType : ((propertyValue != null) ? propertyValue.GetType() : typeof(object));
                        if (this.ShouldExportProperty(propertyInfo, propertyValueType, propertyValue, out str2))
                        {
                            ExportProperty(writer, key, str2, propertyValueType, user, isIPersonalizable);
                        }
                        break;
                    }
                }
            }
        }

        public virtual void ExportWebPart(WebPart webPart, XmlWriter writer)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (!this.Controls.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (webPart.ExportMode == WebPartExportMode.None)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_PartNotExportable"), "webPart");
            }
            bool excludeSensitive = (webPart.ExportMode == WebPartExportMode.NonSensitiveData) && (this.Personalization.Scope != PersonalizationScope.Shared);
            writer.WriteStartElement("webParts");
            writer.WriteStartElement("webPart");
            writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/WebPart/v3");
            writer.WriteStartElement("metaData");
            writer.WriteStartElement("type");
            Control control = webPart.ToControl();
            UserControl control2 = control as UserControl;
            if (control2 != null)
            {
                writer.WriteAttributeString("src", control2.AppRelativeVirtualPath);
            }
            else
            {
                writer.WriteAttributeString("name", WebPartUtil.SerializeType(control.GetType()));
            }
            writer.WriteEndElement();
            writer.WriteElementString("importErrorMessage", webPart.ImportErrorMessage);
            writer.WriteEndElement();
            writer.WriteStartElement("data");
            IDictionary propBag = PersonalizableAttribute.GetPersonalizablePropertyValues(webPart, PersonalizationScope.Shared, excludeSensitive);
            writer.WriteStartElement("properties");
            if (webPart is GenericWebPart)
            {
                this.ExportIPersonalizable(writer, control, excludeSensitive);
                IDictionary dictionary2 = PersonalizableAttribute.GetPersonalizablePropertyValues(control, PersonalizationScope.Shared, excludeSensitive);
                this.ExportToWriter(dictionary2, writer);
                writer.WriteEndElement();
                writer.WriteStartElement("genericWebPartProperties");
                this.ExportIPersonalizable(writer, webPart, excludeSensitive);
                this.ExportToWriter(propBag, writer);
            }
            else
            {
                this.ExportIPersonalizable(writer, webPart, excludeSensitive);
                this.ExportToWriter(propBag, writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Focus()
        {
            throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
        }

        private IList GetAllWebPartsForZone(WebPartZoneBase zone)
        {
            if (this._partsForZone == null)
            {
                this._partsForZone = new HybridDictionary(true);
                foreach (WebPart part in this.Controls)
                {
                    if (!part.IsClosed)
                    {
                        string zoneID = this.Internals.GetZoneID(part);
                        if (!string.IsNullOrEmpty(zoneID))
                        {
                            SortedList list = (SortedList) this._partsForZone[zoneID];
                            if (list == null)
                            {
                                list = new SortedList(new WebPart.ZoneIndexComparer());
                                this._partsForZone[zoneID] = list;
                            }
                            list.Add(part, null);
                        }
                    }
                }
            }
            SortedList list2 = (SortedList) this._partsForZone[zone.ID];
            if (list2 == null)
            {
                list2 = new SortedList();
            }
            return list2.GetKeyList();
        }

        internal WebPartConnection GetConnectionForConsumer(WebPart consumer, ConsumerConnectionPoint connectionPoint)
        {
            ConsumerConnectionPoint consumerConnectionPoint = connectionPoint;
            if (connectionPoint == null)
            {
                consumerConnectionPoint = this.GetConsumerConnectionPoint(consumer, null);
            }
            foreach (WebPartConnection connection in this.StaticConnections)
            {
                if ((!this.Internals.ConnectionDeleted(connection) && (connection.Consumer == consumer)) && (this.GetConsumerConnectionPoint(consumer, connection.ConsumerConnectionPointID) == consumerConnectionPoint))
                {
                    return connection;
                }
            }
            foreach (WebPartConnection connection2 in this.DynamicConnections)
            {
                if ((!this.Internals.ConnectionDeleted(connection2) && (connection2.Consumer == consumer)) && (this.GetConsumerConnectionPoint(consumer, connection2.ConsumerConnectionPointID) == consumerConnectionPoint))
                {
                    return connection2;
                }
            }
            return null;
        }

        internal WebPartConnection GetConnectionForProvider(WebPart provider, ProviderConnectionPoint connectionPoint)
        {
            ProviderConnectionPoint providerConnectionPoint = connectionPoint;
            if (connectionPoint == null)
            {
                providerConnectionPoint = this.GetProviderConnectionPoint(provider, null);
            }
            foreach (WebPartConnection connection in this.StaticConnections)
            {
                if ((!this.Internals.ConnectionDeleted(connection) && (connection.Provider == provider)) && (this.GetProviderConnectionPoint(provider, connection.ProviderConnectionPointID) == providerConnectionPoint))
                {
                    return connection;
                }
            }
            foreach (WebPartConnection connection2 in this.DynamicConnections)
            {
                if ((!this.Internals.ConnectionDeleted(connection2) && (connection2.Provider == provider)) && (this.GetProviderConnectionPoint(provider, connection2.ProviderConnectionPointID) == providerConnectionPoint))
                {
                    return connection2;
                }
            }
            return null;
        }

        private static ICollection[] GetConnectionPoints(Type type)
        {
            if (ConnectionPointsCache == null)
            {
                ConnectionPointsCache = Hashtable.Synchronized(new Hashtable());
            }
            ConnectionPointKey key = new ConnectionPointKey(type, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
            ICollection[] isArray = (ICollection[]) ConnectionPointsCache[key];
            if (isArray == null)
            {
                isArray = CreateConnectionPoints(type);
                ConnectionPointsCache[key] = isArray;
            }
            return isArray;
        }

        internal ConsumerConnectionPoint GetConsumerConnectionPoint(WebPart webPart, string connectionPointID)
        {
            ConsumerConnectionPointCollection consumerConnectionPoints = this.GetConsumerConnectionPoints(webPart);
            if ((consumerConnectionPoints != null) && (consumerConnectionPoints.Count > 0))
            {
                return consumerConnectionPoints[connectionPointID];
            }
            return null;
        }

        private static ConsumerConnectionPointCollection GetConsumerConnectionPoints(Type type)
        {
            return (ConsumerConnectionPointCollection) GetConnectionPoints(type)[0];
        }

        public virtual ConsumerConnectionPointCollection GetConsumerConnectionPoints(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            return GetConsumerConnectionPoints(webPart.ToControl().GetType());
        }

        public static WebPartManager GetCurrentWebPartManager(Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            return (page.Items[typeof(WebPartManager)] as WebPartManager);
        }

        protected internal virtual string GetDisplayTitle(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (!this.Controls.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
            if (!this._allowCreateDisplayTitles)
            {
                return string.Empty;
            }
            if (this._displayTitles == null)
            {
                this._displayTitles = this.CreateDisplayTitles();
            }
            return (string) this._displayTitles[webPart];
        }

        private static ICollection GetEnabledConnectionPoints(ICollection connectionPoints, WebPart webPart)
        {
            Control control = webPart.ToControl();
            ArrayList list = new ArrayList();
            foreach (ConnectionPoint point in connectionPoints)
            {
                if (point.GetEnabled(control))
                {
                    list.Add(point);
                }
            }
            return list;
        }

        internal ConsumerConnectionPointCollection GetEnabledConsumerConnectionPoints(WebPart webPart)
        {
            return new ConsumerConnectionPointCollection(GetEnabledConnectionPoints(this.GetConsumerConnectionPoints(webPart), webPart));
        }

        internal ProviderConnectionPointCollection GetEnabledProviderConnectionPoints(WebPart webPart)
        {
            return new ProviderConnectionPointCollection(GetEnabledConnectionPoints(this.GetProviderConnectionPoints(webPart), webPart));
        }

        private static string GetExportName(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            if (type == typeof(int))
            {
                return "int";
            }
            if (type == typeof(bool))
            {
                return "bool";
            }
            if (type == typeof(double))
            {
                return "double";
            }
            if (type == typeof(float))
            {
                return "single";
            }
            if (type == typeof(DateTime))
            {
                return "datetime";
            }
            if (type == typeof(Color))
            {
                return "color";
            }
            if (type == typeof(Unit))
            {
                return "unit";
            }
            if (type == typeof(FontSize))
            {
                return "fontsize";
            }
            if (type == typeof(ContentDirection))
            {
                return "direction";
            }
            if (type == typeof(WebPartHelpMode))
            {
                return "helpmode";
            }
            if (type == typeof(PartChromeState))
            {
                return "chromestate";
            }
            if (type == typeof(PartChromeType))
            {
                return "chrometype";
            }
            if (type == typeof(WebPartExportMode))
            {
                return "exportmode";
            }
            if (type == typeof(object))
            {
                return "object";
            }
            return type.AssemblyQualifiedName;
        }

        private static Type GetExportType(string name)
        {
            switch (name)
            {
                case "string":
                    return typeof(string);

                case "int":
                    return typeof(int);

                case "bool":
                    return typeof(bool);

                case "double":
                    return typeof(double);

                case "single":
                    return typeof(float);

                case "datetime":
                    return typeof(DateTime);

                case "color":
                    return typeof(Color);

                case "unit":
                    return typeof(Unit);

                case "fontsize":
                    return typeof(FontSize);

                case "direction":
                    return typeof(ContentDirection);

                case "helpmode":
                    return typeof(WebPartHelpMode);

                case "chromestate":
                    return typeof(PartChromeState);

                case "chrometype":
                    return typeof(PartChromeType);

                case "exportmode":
                    return typeof(WebPartExportMode);

                case "object":
                    return typeof(object);
            }
            return WebPartUtil.DeserializeType(name, false);
        }

        public string GetExportUrl(WebPart webPart)
        {
            string str = (this.Personalization.Scope == PersonalizationScope.Shared) ? "&scope=shared" : string.Empty;
            string queryStringText = this.Page.Request.QueryStringText;
            return (this.Page.Request.FilePath + "?__WEBPARTEXPORT=true&webPart=" + HttpUtility.UrlEncode(webPart.ID) + (!string.IsNullOrEmpty(queryStringText) ? ("&query=" + HttpUtility.UrlEncode(queryStringText)) : string.Empty) + str);
        }

        public GenericWebPart GetGenericWebPart(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            GenericWebPart parent = control.Parent as GenericWebPart;
            if ((parent != null) && (parent.ChildControl == control))
            {
                return parent;
            }
            foreach (WebPart part2 in this.Controls)
            {
                GenericWebPart part3 = part2 as GenericWebPart;
                if ((part3 != null) && (part3.ChildControl == control))
                {
                    return part3;
                }
            }
            return null;
        }

        internal ProviderConnectionPoint GetProviderConnectionPoint(WebPart webPart, string connectionPointID)
        {
            ProviderConnectionPointCollection providerConnectionPoints = this.GetProviderConnectionPoints(webPart);
            if ((providerConnectionPoints != null) && (providerConnectionPoints.Count > 0))
            {
                return providerConnectionPoints[connectionPointID];
            }
            return null;
        }

        private static ProviderConnectionPointCollection GetProviderConnectionPoints(Type type)
        {
            return (ProviderConnectionPointCollection) GetConnectionPoints(type)[1];
        }

        public virtual ProviderConnectionPointCollection GetProviderConnectionPoints(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            return GetProviderConnectionPoints(webPart.ToControl().GetType());
        }

        internal WebPartCollection GetWebPartsForZone(WebPartZoneBase zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            if (!this._webPartZones.Contains(zone))
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_MustRegister"), "zone");
            }
            IList allWebPartsForZone = this.GetAllWebPartsForZone(zone);
            WebPartCollection parts = new WebPartCollection();
            if (allWebPartsForZone.Count > 0)
            {
                foreach (WebPart part in allWebPartsForZone)
                {
                    if (this.ShouldRenderWebPartInZone(part, zone))
                    {
                        parts.Add(part);
                    }
                }
            }
            return parts;
        }

        private void ImportFromReader(IDictionary personalizableProperties, Control target, XmlReader reader)
        {
            ImportReadTo(reader, "property");
            bool flag = false;
            if (this.UsePermitOnly)
            {
                this.MinimalPermissionSet.PermitOnly();
                flag = true;
            }
            try
            {
                try
                {
                    IDictionary dictionary;
                    if (personalizableProperties != null)
                    {
                        dictionary = new HybridDictionary();
                    }
                    else
                    {
                        dictionary = new PersonalizationDictionary();
                    }
                    while (reader.Name == "property")
                    {
                        string attribute = reader.GetAttribute("name");
                        string str2 = reader.GetAttribute("type");
                        string a = reader.GetAttribute("scope");
                        bool flag2 = string.Equals(reader.GetAttribute("null"), "true", StringComparison.OrdinalIgnoreCase);
                        if (((attribute == "AuthorizationFilter") || (attribute == "ZoneID")) || (attribute == "ZoneIndex"))
                        {
                            reader.ReadElementString();
                            if (!reader.Read())
                            {
                                throw new XmlException();
                            }
                            goto Label_03AA;
                        }
                        string s = reader.ReadElementString();
                        object obj2 = null;
                        bool flag3 = false;
                        PropertyInfo element = null;
                        if (personalizableProperties != null)
                        {
                            PersonalizablePropertyEntry entry = (PersonalizablePropertyEntry) personalizableProperties[attribute];
                            if (entry != null)
                            {
                                element = entry.PropertyInfo;
                                if ((Attribute.GetCustomAttribute(element, typeof(UrlPropertyAttribute), true) is UrlPropertyAttribute) && CrossSiteScriptingValidation.IsDangerousUrl(s))
                                {
                                    throw new InvalidDataException(System.Web.SR.GetString("WebPart_BadUrl", new object[] { s }));
                                }
                            }
                        }
                        Type exportType = null;
                        if (!string.IsNullOrEmpty(str2))
                        {
                            if (this.UsePermitOnly)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                                flag = false;
                                this.MediumPermissionSet.PermitOnly();
                                flag = true;
                            }
                            exportType = GetExportType(str2);
                            if (this.UsePermitOnly)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                                flag = false;
                                this.MinimalPermissionSet.PermitOnly();
                                flag = true;
                            }
                        }
                        if ((element != null) && ((element.PropertyType == exportType) || (exportType == null)))
                        {
                            TypeConverterAttribute attribute2 = Attribute.GetCustomAttribute(element, typeof(TypeConverterAttribute), true) as TypeConverterAttribute;
                            if (attribute2 != null)
                            {
                                if (this.UsePermitOnly)
                                {
                                    CodeAccessPermission.RevertPermitOnly();
                                    flag = false;
                                    this.MediumPermissionSet.PermitOnly();
                                    flag = true;
                                }
                                Type type = WebPartUtil.DeserializeType(attribute2.ConverterTypeName, false);
                                if (this.UsePermitOnly)
                                {
                                    CodeAccessPermission.RevertPermitOnly();
                                    flag = false;
                                    this.MinimalPermissionSet.PermitOnly();
                                    flag = true;
                                }
                                if ((type != null) && type.IsSubclassOf(typeof(TypeConverter)))
                                {
                                    TypeConverter converter = (TypeConverter) this.Internals.CreateObjectFromType(type);
                                    if (Util.CanConvertToFrom(converter, typeof(string)))
                                    {
                                        if (!flag2)
                                        {
                                            obj2 = converter.ConvertFromInvariantString(s);
                                        }
                                        flag3 = true;
                                    }
                                }
                            }
                            if (!flag3)
                            {
                                TypeConverter converter2 = TypeDescriptor.GetConverter(element.PropertyType);
                                if (Util.CanConvertToFrom(converter2, typeof(string)))
                                {
                                    if (!flag2)
                                    {
                                        obj2 = converter2.ConvertFromInvariantString(s);
                                    }
                                    flag3 = true;
                                }
                            }
                        }
                        if (!flag3 && (exportType != null))
                        {
                            if (exportType == typeof(string))
                            {
                                if (!flag2)
                                {
                                    obj2 = s;
                                }
                                flag3 = true;
                            }
                            else
                            {
                                TypeConverter converter3 = TypeDescriptor.GetConverter(exportType);
                                if (Util.CanConvertToFrom(converter3, typeof(string)))
                                {
                                    if (!flag2)
                                    {
                                        obj2 = converter3.ConvertFromInvariantString(s);
                                    }
                                    flag3 = true;
                                }
                            }
                        }
                        if (flag2 && (personalizableProperties == null))
                        {
                            flag3 = true;
                        }
                        if (flag3)
                        {
                            if (personalizableProperties != null)
                            {
                                dictionary.Add(attribute, obj2);
                            }
                            else
                            {
                                PersonalizationScope scope = string.Equals(a, PersonalizationScope.Shared.ToString(), StringComparison.OrdinalIgnoreCase) ? PersonalizationScope.Shared : PersonalizationScope.User;
                                dictionary.Add(attribute, new PersonalizationEntry(obj2, scope));
                            }
                            goto Label_03AA;
                        }
                        throw new HttpException(System.Web.SR.GetString("WebPartManager_ImportInvalidData", new object[] { attribute }));
                    Label_035C:
                        if (((reader.EOF || (reader.Name == "genericWebPartProperties")) || (reader.Name == "properties")) || ((reader.Name == "ipersonalizable") && (reader.NodeType == XmlNodeType.EndElement)))
                        {
                            break;
                        }
                        reader.Skip();
                    Label_03AA:
                        if (reader.Name != "property")
                        {
                            goto Label_035C;
                        }
                    }
                    if (personalizableProperties != null)
                    {
                        IDictionary unknownProperties = BlobPersonalizationState.SetPersonalizedProperties(target, dictionary);
                        if ((unknownProperties != null) && (unknownProperties.Count > 0))
                        {
                            IVersioningPersonalizable personalizable = target as IVersioningPersonalizable;
                            if (personalizable != null)
                            {
                                personalizable.Load(unknownProperties);
                            }
                        }
                    }
                    else
                    {
                        ((IPersonalizable) target).Load((PersonalizationDictionary) dictionary);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void ImportIPersonalizable(XmlReader reader, Control control)
        {
            if (control is IPersonalizable)
            {
                ImportReadTo(reader, "ipersonalizable", "property");
                if (reader.Name == "ipersonalizable")
                {
                    reader.ReadStartElement("ipersonalizable");
                    this.ImportFromReader(null, control, reader);
                }
            }
        }

        private static void ImportReadTo(XmlReader reader, string elementToFind)
        {
            while (reader.Name != elementToFind)
            {
                if (!reader.Read())
                {
                    throw new XmlException();
                }
            }
        }

        private static void ImportReadTo(XmlReader reader, string elementToFindA, string elementToFindB)
        {
            while ((reader.Name != elementToFindA) && (reader.Name != elementToFindB))
            {
                if (!reader.Read())
                {
                    throw new XmlException();
                }
            }
        }

        private static void ImportSkipTo(XmlReader reader, string elementToFind)
        {
            while (reader.Name != elementToFind)
            {
                reader.Skip();
                if (reader.EOF)
                {
                    throw new XmlException();
                }
            }
        }

        public virtual WebPart ImportWebPart(XmlReader reader, out string errorMessage)
        {
            WebPart part2;
            this.Personalization.EnsureEnabled(true);
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            bool flag = false;
            if (this.UsePermitOnly)
            {
                this.MinimalPermissionSet.PermitOnly();
                flag = true;
            }
            string str = string.Empty;
            try
            {
                try
                {
                    Type type;
                    IDictionary personalizablePropertyEntries;
                    reader.MoveToContent();
                    reader.ReadStartElement("webParts");
                    ImportSkipTo(reader, "webPart");
                    string attribute = reader.GetAttribute("xmlns");
                    if (string.IsNullOrEmpty(attribute))
                    {
                        errorMessage = System.Web.SR.GetString("WebPart_ImportErrorNoVersion");
                        return null;
                    }
                    if (!string.Equals(attribute, "http://schemas.microsoft.com/WebPart/v3", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = System.Web.SR.GetString("WebPart_ImportErrorInvalidVersion");
                        return null;
                    }
                    ImportReadTo(reader, "metaData");
                    reader.ReadStartElement("metaData");
                    string str3 = null;
                    string path = null;
                    ImportSkipTo(reader, "type");
                    str3 = reader.GetAttribute("name");
                    path = reader.GetAttribute("src");
                    ImportSkipTo(reader, "importErrorMessage");
                    str = reader.ReadElementString();
                    WebPart part = null;
                    Control control = null;
                    try
                    {
                        bool isShared = this.Personalization.Scope == PersonalizationScope.Shared;
                        if (!string.IsNullOrEmpty(str3))
                        {
                            if (this.UsePermitOnly)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                                flag = false;
                                this.MediumPermissionSet.PermitOnly();
                                flag = true;
                            }
                            type = WebPartUtil.DeserializeType(str3, true);
                            if (this.UsePermitOnly)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                                flag = false;
                                this.MinimalPermissionSet.PermitOnly();
                                flag = true;
                            }
                            if (!this.IsAuthorized(type, null, null, isShared))
                            {
                                errorMessage = System.Web.SR.GetString("WebPartManager_ForbiddenType");
                                return null;
                            }
                            if (!type.IsSubclassOf(typeof(WebPart)))
                            {
                                if (!type.IsSubclassOf(typeof(Control)))
                                {
                                    errorMessage = System.Web.SR.GetString("WebPartManager_TypeMustDeriveFromControl");
                                    return null;
                                }
                                control = (Control) this.Internals.CreateObjectFromType(type);
                                control.ID = this.CreateDynamicWebPartID(type);
                                part = this.CreateWebPart(control);
                            }
                            else
                            {
                                part = (WebPart) this.Internals.CreateObjectFromType(type);
                            }
                        }
                        else
                        {
                            if (!this.IsAuthorized(typeof(UserControl), path, null, isShared))
                            {
                                errorMessage = System.Web.SR.GetString("WebPartManager_ForbiddenType");
                                return null;
                            }
                            if (this.UsePermitOnly)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                                flag = false;
                            }
                            control = this.Page.LoadControl(path);
                            type = control.GetType();
                            if (this.UsePermitOnly)
                            {
                                this.MinimalPermissionSet.PermitOnly();
                                flag = true;
                            }
                            control.ID = this.CreateDynamicWebPartID(type);
                            part = this.CreateWebPart(control);
                        }
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            errorMessage = str;
                        }
                        else
                        {
                            errorMessage = System.Web.SR.GetString("WebPartManager_ErrorLoadingWebPartType");
                        }
                        return null;
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        str = System.Web.SR.GetString("WebPart_DefaultImportErrorMessage");
                    }
                    ImportSkipTo(reader, "data");
                    reader.ReadStartElement("data");
                    ImportSkipTo(reader, "properties");
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement("properties");
                        if (this.UsePermitOnly)
                        {
                            CodeAccessPermission.RevertPermitOnly();
                            flag = false;
                        }
                        this.ImportIPersonalizable(reader, (control != null) ? control : part);
                        if (this.UsePermitOnly)
                        {
                            this.MinimalPermissionSet.PermitOnly();
                            flag = true;
                        }
                    }
                    if (control != null)
                    {
                        if (!reader.IsEmptyElement)
                        {
                            personalizablePropertyEntries = PersonalizableAttribute.GetPersonalizablePropertyEntries(type);
                            while (reader.Name != "property")
                            {
                                reader.Skip();
                                if (reader.EOF)
                                {
                                    errorMessage = null;
                                    return part;
                                }
                            }
                            if (this.UsePermitOnly)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                                flag = false;
                            }
                            this.ImportFromReader(personalizablePropertyEntries, control, reader);
                            if (this.UsePermitOnly)
                            {
                                this.MinimalPermissionSet.PermitOnly();
                                flag = true;
                            }
                        }
                        ImportSkipTo(reader, "genericWebPartProperties");
                        reader.ReadStartElement("genericWebPartProperties");
                        if (this.UsePermitOnly)
                        {
                            CodeAccessPermission.RevertPermitOnly();
                            flag = false;
                        }
                        this.ImportIPersonalizable(reader, part);
                        if (this.UsePermitOnly)
                        {
                            this.MinimalPermissionSet.PermitOnly();
                            flag = true;
                        }
                        personalizablePropertyEntries = PersonalizableAttribute.GetPersonalizablePropertyEntries(part.GetType());
                    }
                    else
                    {
                        personalizablePropertyEntries = PersonalizableAttribute.GetPersonalizablePropertyEntries(type);
                    }
                    while (reader.Name != "property")
                    {
                        reader.Skip();
                        if (reader.EOF)
                        {
                            errorMessage = null;
                            return part;
                        }
                    }
                    if (this.UsePermitOnly)
                    {
                        CodeAccessPermission.RevertPermitOnly();
                        flag = false;
                    }
                    this.ImportFromReader(personalizablePropertyEntries, part, reader);
                    if (this.UsePermitOnly)
                    {
                        this.MinimalPermissionSet.PermitOnly();
                        flag = true;
                    }
                    errorMessage = null;
                    part2 = part;
                }
                catch (XmlException)
                {
                    errorMessage = System.Web.SR.GetString("WebPartManager_ImportInvalidFormat");
                    part2 = null;
                }
                catch (Exception exception)
                {
                    if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                    {
                        errorMessage = (str.Length != 0) ? str : System.Web.SR.GetString("WebPart_DefaultImportErrorMessage");
                    }
                    else
                    {
                        errorMessage = exception.Message;
                    }
                    part2 = null;
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }
            catch
            {
                throw;
            }
            return part2;
        }

        public bool IsAuthorized(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            string authorizationFilter = webPart.AuthorizationFilter;
            if (!string.IsNullOrEmpty(webPart.ID) && this.Personalization.IsEnabled)
            {
                string str3 = this.Personalization.GetAuthorizationFilter(webPart.ID);
                if (str3 != null)
                {
                    authorizationFilter = str3;
                }
            }
            GenericWebPart part = webPart as GenericWebPart;
            if (part == null)
            {
                return this.IsAuthorized(webPart.GetType(), null, authorizationFilter, webPart.IsShared);
            }
            Type type = null;
            string path = null;
            Control childControl = part.ChildControl;
            UserControl control2 = childControl as UserControl;
            if (control2 != null)
            {
                type = typeof(UserControl);
                path = control2.AppRelativeVirtualPath;
            }
            else
            {
                type = childControl.GetType();
            }
            return this.IsAuthorized(type, path, authorizationFilter, webPart.IsShared);
        }

        public virtual bool IsAuthorized(Type type, string path, string authorizationFilter, bool isShared)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type == typeof(UserControl))
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPartManager_PathCannotBeEmpty"));
                }
            }
            else if (!string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_PathMustBeEmpty", new object[] { path }));
            }
            WebPartAuthorizationEventArgs e = new WebPartAuthorizationEventArgs(type, path, authorizationFilter, isShared);
            this.OnAuthorizeWebPart(e);
            return e.IsAuthorized;
        }

        internal bool IsConsumerConnected(WebPart consumer, ConsumerConnectionPoint connectionPoint)
        {
            return (this.GetConnectionForConsumer(consumer, connectionPoint) != null);
        }

        internal bool IsProviderConnected(WebPart provider, ProviderConnectionPoint connectionPoint)
        {
            return (this.GetConnectionForProvider(provider, connectionPoint) != null);
        }

        protected internal override void LoadControlState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadControlState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 3)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_ControlState"));
                }
                base.LoadControlState(objArray[0]);
                if (objArray[1] != null)
                {
                    WebPart webPart = this.WebParts[(string) objArray[1]];
                    if ((webPart == null) || webPart.IsClosed)
                    {
                        this.SetSelectedWebPart(null);
                        this.OnSelectedWebPartChanged(new WebPartEventArgs(null));
                    }
                    else
                    {
                        this.SetSelectedWebPart(webPart);
                    }
                }
                if (objArray[2] != null)
                {
                    string str = (string) objArray[2];
                    WebPartDisplayMode mode = this.SupportedDisplayModes[str];
                    mode.IsEnabled(this);
                    if (mode == null)
                    {
                        this._displayMode = BrowseDisplayMode;
                        this.OnDisplayModeChanged(new WebPartDisplayModeEventArgs(null));
                    }
                    else
                    {
                        this._displayMode = mode;
                    }
                }
            }
        }

        protected virtual void LoadCustomPersonalizationState(PersonalizationDictionary state)
        {
            this._personalizationState = state;
        }

        private void LoadDeletedConnectionState(PersonalizationEntry entry)
        {
            if (entry != null)
            {
                string[] strArray = (string[]) entry.Value;
                if (strArray != null)
                {
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string b = strArray[i];
                        WebPartConnection connection = null;
                        foreach (WebPartConnection connection2 in this.StaticConnections)
                        {
                            if (string.Equals(connection2.ID, b, StringComparison.OrdinalIgnoreCase))
                            {
                                connection = connection2;
                                break;
                            }
                        }
                        if (connection == null)
                        {
                            foreach (WebPartConnection connection3 in this.DynamicConnections)
                            {
                                if (string.Equals(connection3.ID, b, StringComparison.OrdinalIgnoreCase))
                                {
                                    connection = connection3;
                                    break;
                                }
                            }
                        }
                        if (connection != null)
                        {
                            this.Internals.DeleteConnection(connection);
                        }
                        else
                        {
                            this._hasDataChanged = true;
                        }
                    }
                }
            }
        }

        private void LoadDynamicConnections(PersonalizationEntry entry)
        {
            if (entry != null)
            {
                object[] objArray = (object[]) entry.Value;
                if (objArray != null)
                {
                    for (int i = 0; i < objArray.Length; i += 7)
                    {
                        string str = (string) objArray[i];
                        string str2 = (string) objArray[i + 1];
                        string str3 = (string) objArray[i + 2];
                        string str4 = (string) objArray[i + 3];
                        string str5 = (string) objArray[i + 4];
                        WebPartConnection connection = new WebPartConnection {
                            ID = str,
                            ConsumerID = str2,
                            ConsumerConnectionPointID = str3,
                            ProviderID = str4,
                            ProviderConnectionPointID = str5
                        };
                        this.Internals.SetIsShared(connection, entry.Scope == PersonalizationScope.Shared);
                        this.Internals.SetIsStatic(connection, false);
                        Type type = objArray[i + 5] as Type;
                        if (type != null)
                        {
                            if (!type.IsSubclassOf(typeof(WebPartTransformer)))
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerAttribute_NotTransformer", new object[] { type.Name }));
                            }
                            object savedState = objArray[i + 6];
                            WebPartTransformer transformer = (WebPartTransformer) this.Internals.CreateObjectFromType(type);
                            this.Internals.LoadConfigurationState(transformer, savedState);
                            this.Internals.SetTransformer(connection, transformer);
                        }
                        this.DynamicConnections.Add(connection);
                    }
                }
            }
        }

        private void LoadDynamicWebPart(string id, string typeName, string path, string genericWebPartID, bool isShared)
        {
            WebPart webPart = null;
            Type type = WebPartUtil.DeserializeType(typeName, false);
            if (type == null)
            {
                string str;
                if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                {
                    str = System.Web.SR.GetString("WebPartManager_ErrorLoadingWebPartType");
                }
                else
                {
                    str = System.Web.SR.GetString("Invalid_type", new object[] { typeName });
                }
                webPart = this.CreateErrorWebPart(id, typeName, path, genericWebPartID, str);
            }
            else if (type.IsSubclassOf(typeof(WebPart)))
            {
                string authorizationFilter = this.Personalization.GetAuthorizationFilter(id);
                if (this.IsAuthorized(type, null, authorizationFilter, isShared))
                {
                    try
                    {
                        webPart = (WebPart) this.Internals.CreateObjectFromType(type);
                        webPart.ID = id;
                    }
                    catch
                    {
                        string str3;
                        if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                        {
                            str3 = System.Web.SR.GetString("WebPartManager_CantCreateInstance");
                        }
                        else
                        {
                            str3 = System.Web.SR.GetString("WebPartManager_CantCreateInstanceWithType", new object[] { typeName });
                        }
                        webPart = this.CreateErrorWebPart(id, typeName, path, genericWebPartID, str3);
                    }
                }
                else
                {
                    webPart = new UnauthorizedWebPart(id, typeName, path, genericWebPartID);
                }
            }
            else if (type.IsSubclassOf(typeof(Control)))
            {
                string str4 = this.Personalization.GetAuthorizationFilter(genericWebPartID);
                if (this.IsAuthorized(type, path, str4, isShared))
                {
                    Control control = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            control = this.Page.LoadControl(path);
                        }
                        else
                        {
                            control = (Control) this.Internals.CreateObjectFromType(type);
                        }
                        control.ID = id;
                        webPart = this.CreateWebPart(control);
                        webPart.ID = genericWebPartID;
                    }
                    catch
                    {
                        string str5;
                        if ((control == null) && string.IsNullOrEmpty(path))
                        {
                            if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                            {
                                str5 = System.Web.SR.GetString("WebPartManager_CantCreateInstance");
                            }
                            else
                            {
                                str5 = System.Web.SR.GetString("WebPartManager_CantCreateInstanceWithType", new object[] { typeName });
                            }
                        }
                        else if (control == null)
                        {
                            if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                            {
                                str5 = System.Web.SR.GetString("WebPartManager_InvalidPath");
                            }
                            else
                            {
                                str5 = System.Web.SR.GetString("WebPartManager_InvalidPathWithPath", new object[] { path });
                            }
                        }
                        else
                        {
                            str5 = System.Web.SR.GetString("WebPartManager_CantCreateGeneric");
                        }
                        webPart = this.CreateErrorWebPart(id, typeName, path, genericWebPartID, str5);
                    }
                }
                else
                {
                    webPart = new UnauthorizedWebPart(id, typeName, path, genericWebPartID);
                }
            }
            else
            {
                string str6;
                if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                {
                    str6 = System.Web.SR.GetString("WebPartManager_TypeMustDeriveFromControl");
                }
                else
                {
                    str6 = System.Web.SR.GetString("WebPartManager_TypeMustDeriveFromControlWithType", new object[] { typeName });
                }
                webPart = this.CreateErrorWebPart(id, typeName, path, genericWebPartID, str6);
            }
            this.Internals.SetIsStatic(webPart, false);
            this.Internals.SetIsShared(webPart, isShared);
            this.Internals.AddWebPart(webPart);
        }

        private void LoadDynamicWebParts(PersonalizationEntry entry)
        {
            if (entry != null)
            {
                object[] objArray = (object[]) entry.Value;
                if (objArray != null)
                {
                    bool isShared = entry.Scope == PersonalizationScope.Shared;
                    for (int i = 0; i < objArray.Length; i += 4)
                    {
                        string id = (string) objArray[i];
                        string typeName = (string) objArray[i + 1];
                        string path = (string) objArray[i + 2];
                        string genericWebPartID = (string) objArray[i + 3];
                        this.LoadDynamicWebPart(id, typeName, path, genericWebPartID, isShared);
                    }
                }
            }
        }

        private void LoadWebPartState(PersonalizationEntry entry)
        {
            if (entry != null)
            {
                object[] objArray = (object[]) entry.Value;
                if (objArray != null)
                {
                    for (int i = 0; i < objArray.Length; i += 4)
                    {
                        string id = (string) objArray[i];
                        string zoneID = (string) objArray[i + 1];
                        int zoneIndex = (int) objArray[i + 2];
                        bool isClosed = (bool) objArray[i + 3];
                        WebPart webPart = (WebPart) this.FindControl(id);
                        if (webPart != null)
                        {
                            this.Internals.SetZoneID(webPart, zoneID);
                            this.Internals.SetZoneIndex(webPart, zoneIndex);
                            this.Internals.SetIsClosed(webPart, isClosed);
                        }
                        else
                        {
                            this._hasDataChanged = true;
                        }
                    }
                }
            }
        }

        public virtual void MoveWebPart(WebPart webPart, WebPartZoneBase zone, int zoneIndex)
        {
            this.Personalization.EnsureEnabled(true);
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (!this.Controls.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            if (!this._webPartZones.Contains(zone))
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_MustRegister"), "zone");
            }
            if (zoneIndex < 0)
            {
                throw new ArgumentOutOfRangeException("zoneIndex");
            }
            if ((webPart.Zone == null) || webPart.IsClosed)
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_MustBeInZone"), "webPart");
            }
            if ((webPart.Zone != zone) || (webPart.ZoneIndex != zoneIndex))
            {
                WebPartMovingEventArgs e = new WebPartMovingEventArgs(webPart, zone, zoneIndex);
                this.OnWebPartMoving(e);
                if (!this._allowEventCancellation || !e.Cancel)
                {
                    this.RemoveWebPartFromZone(webPart);
                    this.AddWebPartToZone(webPart, zone, zoneIndex);
                    this.OnWebPartMoved(new WebPartEventArgs(webPart));
                }
            }
        }

        protected virtual void OnAuthorizeWebPart(WebPartAuthorizationEventArgs e)
        {
            WebPartAuthorizationEventHandler handler = (WebPartAuthorizationEventHandler) base.Events[AuthorizeWebPartEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionsActivated(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[ConnectionsActivatedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionsActivating(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[ConnectionsActivatingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDisplayModeChanged(WebPartDisplayModeEventArgs e)
        {
            WebPartDisplayModeEventHandler handler = (WebPartDisplayModeEventHandler) base.Events[DisplayModeChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDisplayModeChanging(WebPartDisplayModeCancelEventArgs e)
        {
            WebPartDisplayModeCancelEventHandler handler = (WebPartDisplayModeCancelEventHandler) base.Events[DisplayModeChangingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!base.DesignMode)
            {
                Page page = this.Page;
                if (page != null)
                {
                    if (((WebPartManager) page.Items[typeof(WebPartManager)]) != null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_OnlyOneInstance"));
                    }
                    page.Items[typeof(WebPartManager)] = this;
                    page.InitComplete += new EventHandler(this.OnPageInitComplete);
                    page.LoadComplete += new EventHandler(this.OnPageLoadComplete);
                    page.SaveStateComplete += new EventHandler(this.OnPageSaveStateComplete);
                    page.RegisterRequiresControlState(this);
                    this.Personalization.LoadInternal();
                }
            }
        }

        private void OnPageInitComplete(object sender, EventArgs e)
        {
            if (this._personalizationState != null)
            {
                this.LoadDynamicConnections(this._personalizationState["DynamicConnectionsShared"]);
                this.LoadDynamicConnections(this._personalizationState["DynamicConnectionsUser"]);
                this.LoadDeletedConnectionState(this._personalizationState["DeletedConnectionsShared"]);
                this.LoadDeletedConnectionState(this._personalizationState["DeletedConnectionsUser"]);
                this.LoadDynamicWebParts(this._personalizationState["DynamicWebPartsShared"]);
                this.LoadDynamicWebParts(this._personalizationState["DynamicWebPartsUser"]);
                this.LoadWebPartState(this._personalizationState["WebPartStateShared"]);
                this.LoadWebPartState(this._personalizationState["WebPartStateUser"]);
            }
            this._pageInitComplete = true;
        }

        private void OnPageLoadComplete(object sender, EventArgs e)
        {
            this.CloseOrphanedParts();
            this._allowCreateDisplayTitles = true;
            this.OnConnectionsActivating(EventArgs.Empty);
            this.ActivateConnections();
            this.OnConnectionsActivated(EventArgs.Empty);
        }

        private void OnPageSaveStateComplete(object sender, EventArgs e)
        {
            this.Personalization.ExtractPersonalizationState();
            foreach (WebPart part in this.Controls)
            {
                this.Personalization.ExtractPersonalizationState(part);
            }
            this.Personalization.SaveInternal();
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterStartupScript(this, typeof(WebPartManager), "ExportSensitiveDataWarningDeclaration", "var __wpmExportWarning='" + Util.QuoteJScriptString(this.ExportSensitiveDataWarning) + "';", true);
                this.Page.ClientScript.RegisterStartupScript(this, typeof(WebPartManager), "CloseProviderWarningDeclaration", "var __wpmCloseProviderWarning='" + Util.QuoteJScriptString(this.CloseProviderWarning) + "';", true);
                this.Page.ClientScript.RegisterStartupScript(this, typeof(WebPartManager), "DeleteWarningDeclaration", "var __wpmDeleteWarning='" + Util.QuoteJScriptString(this.DeleteWarning) + "';", true);
                this._renderClientScript = this.CheckRenderClientScript();
                if (this._renderClientScript)
                {
                    this.Page.RegisterPostBackScript();
                    this.RegisterClientScript();
                }
            }
        }

        protected virtual void OnSelectedWebPartChanged(WebPartEventArgs e)
        {
            WebPartEventHandler handler = (WebPartEventHandler) base.Events[SelectedWebPartChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedWebPartChanging(WebPartCancelEventArgs e)
        {
            WebPartCancelEventHandler handler = (WebPartCancelEventHandler) base.Events[SelectedWebPartChangingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (!base.DesignMode)
            {
                Page page = this.Page;
                if (page != null)
                {
                    page.Items.Remove(typeof(WebPartManager));
                }
            }
        }

        protected virtual void OnWebPartAdded(WebPartEventArgs e)
        {
            WebPartEventHandler handler = (WebPartEventHandler) base.Events[WebPartAddedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartAdding(WebPartAddingEventArgs e)
        {
            WebPartAddingEventHandler handler = (WebPartAddingEventHandler) base.Events[WebPartAddingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartClosed(WebPartEventArgs e)
        {
            WebPartEventHandler handler = (WebPartEventHandler) base.Events[WebPartClosedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartClosing(WebPartCancelEventArgs e)
        {
            WebPartCancelEventHandler handler = (WebPartCancelEventHandler) base.Events[WebPartClosingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartDeleted(WebPartEventArgs e)
        {
            WebPartEventHandler handler = (WebPartEventHandler) base.Events[WebPartDeletedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartDeleting(WebPartCancelEventArgs e)
        {
            WebPartCancelEventHandler handler = (WebPartCancelEventHandler) base.Events[WebPartDeletingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartMoved(WebPartEventArgs e)
        {
            WebPartEventHandler handler = (WebPartEventHandler) base.Events[WebPartMovedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartMoving(WebPartMovingEventArgs e)
        {
            WebPartMovingEventHandler handler = (WebPartMovingEventHandler) base.Events[WebPartMovingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsConnected(WebPartConnectionsEventArgs e)
        {
            WebPartConnectionsEventHandler handler = (WebPartConnectionsEventHandler) base.Events[WebPartsConnectedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsConnecting(WebPartConnectionsCancelEventArgs e)
        {
            WebPartConnectionsCancelEventHandler handler = (WebPartConnectionsCancelEventHandler) base.Events[WebPartsConnectingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsDisconnected(WebPartConnectionsEventArgs e)
        {
            WebPartConnectionsEventHandler handler = (WebPartConnectionsEventHandler) base.Events[WebPartsDisconnectedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsDisconnecting(WebPartConnectionsCancelEventArgs e)
        {
            WebPartConnectionsCancelEventHandler handler = (WebPartConnectionsCancelEventHandler) base.Events[WebPartsDisconnectingEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RegisterClientScript()
        {
            this.Page.ClientScript.RegisterClientScriptResource(this, typeof(WebPartManager), "WebParts.js");
            bool allowPageDesign = this.DisplayMode.AllowPageDesign;
            string str = "null";
            if (allowPageDesign)
            {
                str = "document.getElementById('" + this.ClientID + "___Drag')";
            }
            StringBuilder builder = new StringBuilder(0x400);
            foreach (WebPartZoneBase base2 in this._webPartZones)
            {
                string str2 = (base2.LayoutOrientation == Orientation.Vertical) ? "true" : "false";
                string str3 = "false";
                string str4 = "black";
                if (allowPageDesign && base2.AllowLayoutChange)
                {
                    str3 = "true";
                    str4 = ColorTranslator.ToHtml(base2.DragHighlightColor);
                }
                builder.AppendFormat("\r\nzoneElement = document.getElementById('{0}');\r\nif (zoneElement != null) {{\r\n    zoneObject = __wpm.AddZone(zoneElement, '{1}', {2}, {3}, '{4}');", new object[] { base2.ClientID, base2.UniqueID, str2, str3, str4 });
                foreach (WebPart part in this.GetWebPartsForZone(base2))
                {
                    string str5 = "null";
                    string str6 = "false";
                    if (allowPageDesign)
                    {
                        str5 = "document.getElementById('" + part.TitleBarID + "')";
                        if (part.AllowZoneChange)
                        {
                            str6 = "true";
                        }
                    }
                    builder.AppendFormat("\r\n    zoneObject.AddWebPart(document.getElementById('{0}'), {1}, {2});", part.WholePartID, str5, str6);
                }
                builder.Append("\r\n}");
            }
            string script = string.Format(CultureInfo.InvariantCulture, "\r\n<script type=\"text/javascript\">\r\n\r\n__wpm = new WebPartManager();\r\n__wpm.overlayContainerElement = {0};\r\n__wpm.personalizationScopeShared = {1};\r\n\r\nvar zoneElement;\r\nvar zoneObject;\r\n{2}\r\n</script>\r\n", new object[] { str, (this.Personalization.Scope == PersonalizationScope.Shared) ? "true" : "false", builder.ToString() });
            this.Page.ClientScript.RegisterStartupScript(this, typeof(WebPartManager), string.Empty, script, false);
            IScriptManager scriptManager = this.Page.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
            {
                scriptManager.RegisterDispose(this, "WebPartManager_Dispose();");
            }
        }

        internal void RegisterZone(WebZone zone)
        {
            if (this._pageInitComplete)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_RegisterTooLate"));
            }
            string iD = zone.ID;
            if (string.IsNullOrEmpty(iD))
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_NoZoneID"), "zone");
            }
            if (this._zoneIDs.Contains(iD))
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_DuplicateZoneID", new object[] { iD }));
            }
            this._zoneIDs.Add(iD, zone);
            WebPartZoneBase base2 = zone as WebPartZoneBase;
            if (base2 != null)
            {
                if (this._webPartZones.Contains(base2))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPartManager_AlreadyRegistered"), "zone");
                }
                this._webPartZones.Add(base2);
                WebPartCollection initialWebParts = base2.GetInitialWebParts();
                ((WebPartManagerControlCollection) this.Controls).AddWebPartsFromZone(base2, initialWebParts);
            }
            else
            {
                ToolZone zone2 = (ToolZone) zone;
                WebPartDisplayModeCollection displayModes = this.DisplayModes;
                WebPartDisplayModeCollection supportedDisplayModes = this.SupportedDisplayModes;
                foreach (WebPartDisplayMode mode in zone2.AssociatedDisplayModes)
                {
                    if (displayModes.Contains(mode) && !supportedDisplayModes.Contains(mode))
                    {
                        supportedDisplayModes.AddInternal(mode);
                    }
                }
            }
        }

        internal void RemoveWebPart(WebPart webPart)
        {
            ((WebPartManagerControlCollection) this.Controls).RemoveWebPart(webPart);
        }

        private void RemoveWebPartFromDictionary(WebPart webPart)
        {
            if (this._partsForZone != null)
            {
                string zoneID = this.Internals.GetZoneID(webPart);
                if (!string.IsNullOrEmpty(zoneID))
                {
                    SortedList list = (SortedList) this._partsForZone[zoneID];
                    if (list != null)
                    {
                        list.Remove(webPart);
                    }
                }
            }
        }

        private void RemoveWebPartFromZone(WebPart webPart)
        {
            WebPartZoneBase zone = webPart.Zone;
            this.Internals.SetIsClosed(webPart, true);
            this._hasDataChanged = true;
            this.RemoveWebPartFromDictionary(webPart);
            if (zone != null)
            {
                IList allWebPartsForZone = this.GetAllWebPartsForZone(zone);
                for (int i = 0; i < allWebPartsForZone.Count; i++)
                {
                    WebPart part = (WebPart) allWebPartsForZone[i];
                    this.Internals.SetZoneIndex(part, i);
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.DisplayMode.AllowPageDesign)
            {
                string str = string.Format(CultureInfo.InvariantCulture, "\r\n<div id=\"{0}___Drag\" style=\"display:none; position:absolute; z-index: 32000; filter:alpha(opacity=75)\"></div>", new object[] { this.ClientID });
                writer.WriteLine(str);
            }
        }

        protected internal override object SaveControlState()
        {
            object[] objArray = new object[3];
            objArray[0] = base.SaveControlState();
            if (this.SelectedWebPart != null)
            {
                objArray[1] = this.SelectedWebPart.ID;
            }
            if (this._displayMode != BrowseDisplayMode)
            {
                objArray[2] = this._displayMode.Name;
            }
            for (int i = 0; i < 3; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        protected virtual void SaveCustomPersonalizationState(PersonalizationDictionary state)
        {
            PersonalizationScope scope = this.Personalization.Scope;
            int count = this.Controls.Count;
            if (count > 0)
            {
                object[] objArray = new object[count * 4];
                for (int i = 0; i < count; i++)
                {
                    WebPart webPart = (WebPart) this.Controls[i];
                    objArray[4 * i] = webPart.ID;
                    objArray[(4 * i) + 1] = this.Internals.GetZoneID(webPart);
                    objArray[(4 * i) + 2] = webPart.ZoneIndex;
                    objArray[(4 * i) + 3] = webPart.IsClosed;
                }
                if (scope == PersonalizationScope.Shared)
                {
                    state["WebPartStateShared"] = new PersonalizationEntry(objArray, PersonalizationScope.Shared);
                }
                else
                {
                    state["WebPartStateUser"] = new PersonalizationEntry(objArray, PersonalizationScope.User);
                }
            }
            ArrayList list = new ArrayList();
            foreach (WebPart part2 in this.Controls)
            {
                if (!part2.IsStatic && (((scope == PersonalizationScope.User) && !part2.IsShared) || ((scope == PersonalizationScope.Shared) && part2.IsShared)))
                {
                    list.Add(part2);
                }
            }
            int num3 = list.Count;
            if (num3 > 0)
            {
                object[] objArray2 = new object[num3 * 4];
                for (int j = 0; j < num3; j++)
                {
                    string originalID;
                    string originalTypeName;
                    WebPart part3 = (WebPart) list[j];
                    string originalPath = null;
                    string genericWebPartID = null;
                    ProxyWebPart part4 = part3 as ProxyWebPart;
                    if (part4 != null)
                    {
                        originalID = part4.OriginalID;
                        originalTypeName = part4.OriginalTypeName;
                        originalPath = part4.OriginalPath;
                        genericWebPartID = part4.GenericWebPartID;
                    }
                    else
                    {
                        GenericWebPart part5 = part3 as GenericWebPart;
                        if (part5 != null)
                        {
                            Control childControl = part5.ChildControl;
                            UserControl control2 = childControl as UserControl;
                            originalID = childControl.ID;
                            if (control2 != null)
                            {
                                originalTypeName = WebPartUtil.SerializeType(typeof(UserControl));
                                originalPath = control2.AppRelativeVirtualPath;
                            }
                            else
                            {
                                originalTypeName = WebPartUtil.SerializeType(childControl.GetType());
                            }
                            genericWebPartID = part5.ID;
                        }
                        else
                        {
                            originalID = part3.ID;
                            originalTypeName = WebPartUtil.SerializeType(part3.GetType());
                        }
                    }
                    objArray2[4 * j] = originalID;
                    objArray2[(4 * j) + 1] = originalTypeName;
                    if (!string.IsNullOrEmpty(originalPath))
                    {
                        objArray2[(4 * j) + 2] = originalPath;
                    }
                    if (!string.IsNullOrEmpty(genericWebPartID))
                    {
                        objArray2[(4 * j) + 3] = genericWebPartID;
                    }
                }
                if (scope == PersonalizationScope.Shared)
                {
                    state["DynamicWebPartsShared"] = new PersonalizationEntry(objArray2, PersonalizationScope.Shared);
                }
                else
                {
                    state["DynamicWebPartsUser"] = new PersonalizationEntry(objArray2, PersonalizationScope.User);
                }
            }
            ArrayList list2 = new ArrayList();
            foreach (WebPartConnection connection in this.StaticConnections)
            {
                if (this.Internals.ConnectionDeleted(connection))
                {
                    list2.Add(connection);
                }
            }
            foreach (WebPartConnection connection2 in this.DynamicConnections)
            {
                if (this.Internals.ConnectionDeleted(connection2))
                {
                    list2.Add(connection2);
                }
            }
            int num5 = list2.Count;
            if (list2.Count > 0)
            {
                string[] strArray = new string[num5];
                for (int k = 0; k < num5; k++)
                {
                    WebPartConnection connection3 = (WebPartConnection) list2[k];
                    strArray[k] = connection3.ID;
                }
                if (scope == PersonalizationScope.Shared)
                {
                    state["DeletedConnectionsShared"] = new PersonalizationEntry(strArray, PersonalizationScope.Shared);
                }
                else
                {
                    state["DeletedConnectionsUser"] = new PersonalizationEntry(strArray, PersonalizationScope.User);
                }
            }
            ArrayList list3 = new ArrayList();
            foreach (WebPartConnection connection4 in this.DynamicConnections)
            {
                if (((scope == PersonalizationScope.User) && !connection4.IsShared) || ((scope == PersonalizationScope.Shared) && connection4.IsShared))
                {
                    list3.Add(connection4);
                }
            }
            int num7 = list3.Count;
            if (num7 > 0)
            {
                object[] objArray3 = new object[num7 * 7];
                for (int m = 0; m < num7; m++)
                {
                    WebPartConnection connection5 = (WebPartConnection) list3[m];
                    WebPartTransformer transformer = connection5.Transformer;
                    objArray3[7 * m] = connection5.ID;
                    objArray3[(7 * m) + 1] = connection5.ConsumerID;
                    objArray3[(7 * m) + 2] = connection5.ConsumerConnectionPointID;
                    objArray3[(7 * m) + 3] = connection5.ProviderID;
                    objArray3[(7 * m) + 4] = connection5.ProviderConnectionPointID;
                    if (transformer != null)
                    {
                        objArray3[(7 * m) + 5] = transformer.GetType();
                        objArray3[(7 * m) + 6] = this.Internals.SaveConfigurationState(transformer);
                    }
                }
                if (scope == PersonalizationScope.Shared)
                {
                    state["DynamicConnectionsShared"] = new PersonalizationEntry(objArray3, PersonalizationScope.Shared);
                }
                else
                {
                    state["DynamicConnectionsUser"] = new PersonalizationEntry(objArray3, PersonalizationScope.User);
                }
            }
        }

        protected void SetPersonalizationDirty()
        {
            this.Personalization.SetDirty();
        }

        protected void SetSelectedWebPart(WebPart webPart)
        {
            this._selectedWebPart = webPart;
        }

        private bool ShouldExportProperty(PropertyInfo propertyInfo, Type propertyValueType, object propertyValue, out string exportString)
        {
            string str = propertyValue as string;
            if (str != null)
            {
                exportString = str;
                return true;
            }
            TypeConverter converter = null;
            if (propertyInfo != null)
            {
                TypeConverterAttribute attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(TypeConverterAttribute), true) as TypeConverterAttribute;
                if (attribute != null)
                {
                    Type type = WebPartUtil.DeserializeType(attribute.ConverterTypeName, false);
                    if ((type != null) && type.IsSubclassOf(typeof(TypeConverter)))
                    {
                        TypeConverter converter2 = (TypeConverter) this.Internals.CreateObjectFromType(type);
                        if (Util.CanConvertToFrom(converter2, typeof(string)))
                        {
                            converter = converter2;
                        }
                    }
                }
            }
            if (converter == null)
            {
                TypeConverter converter3 = TypeDescriptor.GetConverter(propertyValueType);
                if (Util.CanConvertToFrom(converter3, typeof(string)))
                {
                    converter = converter3;
                }
            }
            if (converter != null)
            {
                if (propertyValue != null)
                {
                    exportString = converter.ConvertToInvariantString(propertyValue);
                    return true;
                }
                exportString = null;
                return true;
            }
            exportString = null;
            return ((propertyInfo == null) && (propertyValue == null));
        }

        private bool ShouldRemoveConnection(WebPartConnection connection)
        {
            if (connection.IsShared && (this.Personalization.Scope == PersonalizationScope.User))
            {
                return false;
            }
            return true;
        }

        private bool ShouldRenderWebPartInZone(WebPart part, WebPartZoneBase zone)
        {
            if (part is UnauthorizedWebPart)
            {
                return false;
            }
            return true;
        }

        void IPersonalizable.Load(PersonalizationDictionary state)
        {
            this.LoadCustomPersonalizationState(state);
        }

        void IPersonalizable.Save(PersonalizationDictionary state)
        {
            this.SaveCustomPersonalizationState(state);
        }

        protected override void TrackViewState()
        {
            this.Personalization.ApplyPersonalizationState();
            base.TrackViewState();
        }

        private void VerifyType(Control control)
        {
            if (!(control is UserControl))
            {
                Type type = control.GetType();
                string typeName = WebPartUtil.SerializeType(type);
                if (WebPartUtil.DeserializeType(typeName, false) != type)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_CantAddControlType", new object[] { typeName }));
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TransformerTypeCollection AvailableTransformers
        {
            get
            {
                if (this._availableTransformers == null)
                {
                    this._availableTransformers = this.CreateAvailableTransformers();
                }
                return this._availableTransformers;
            }
        }

        [WebSysDefaultValue("WebPartManager_DefaultCloseProviderWarning"), WebCategory("Behavior"), WebSysDescription("WebPartManager_CloseProviderWarning")]
        public virtual string CloseProviderWarning
        {
            get
            {
                object obj2 = this.ViewState["CloseProviderWarning"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("WebPartManager_DefaultCloseProviderWarning");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["CloseProviderWarning"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPartConnectionCollection Connections
        {
            get
            {
                WebPartConnectionCollection connections = new WebPartConnectionCollection(this);
                if (this._staticConnections != null)
                {
                    foreach (WebPartConnection connection in this._staticConnections)
                    {
                        if (!this.Internals.ConnectionDeleted(connection))
                        {
                            connections.Add(connection);
                        }
                    }
                }
                if (this._dynamicConnections != null)
                {
                    foreach (WebPartConnection connection2 in this._dynamicConnections)
                    {
                        if (!this.Internals.ConnectionDeleted(connection2))
                        {
                            connections.Add(connection2);
                        }
                    }
                }
                connections.SetReadOnly("WebPartManager_ConnectionsReadOnly");
                return connections;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        [WebSysDefaultValue("WebPartManager_DefaultDeleteWarning"), WebSysDescription("WebPartManager_DeleteWarning"), WebCategory("Behavior")]
        public virtual string DeleteWarning
        {
            get
            {
                object obj2 = this.ViewState["DeleteWarning"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("WebPartManager_DefaultDeleteWarning");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DeleteWarning"] = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual WebPartDisplayMode DisplayMode
        {
            get
            {
                return this._displayMode;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.DisplayMode != value)
                {
                    if (!this.SupportedDisplayModes.Contains(value))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("WebPartManager_InvalidDisplayMode"), "value");
                    }
                    if (!value.IsEnabled(this))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("WebPartManager_DisabledDisplayMode"), "value");
                    }
                    WebPartDisplayModeCancelEventArgs e = new WebPartDisplayModeCancelEventArgs(value);
                    this.OnDisplayModeChanging(e);
                    if (!this._allowEventCancellation || !e.Cancel)
                    {
                        if ((this.DisplayMode == ConnectDisplayMode) && (this.SelectedWebPart != null))
                        {
                            this.EndWebPartConnecting();
                            if (this.SelectedWebPart != null)
                            {
                                return;
                            }
                        }
                        if ((this.DisplayMode == EditDisplayMode) && (this.SelectedWebPart != null))
                        {
                            this.EndWebPartEditing();
                            if (this.SelectedWebPart != null)
                            {
                                return;
                            }
                        }
                        WebPartDisplayModeEventArgs args2 = new WebPartDisplayModeEventArgs(this.DisplayMode);
                        this._displayMode = value;
                        this.OnDisplayModeChanged(args2);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebPartDisplayModeCollection DisplayModes
        {
            get
            {
                if (this._displayModes == null)
                {
                    this._displayModes = this.CreateDisplayModes();
                    this._displayModes.SetReadOnly("WebPartManager_DisplayModesReadOnly");
                }
                return this._displayModes;
            }
        }

        protected internal WebPartConnectionCollection DynamicConnections
        {
            get
            {
                if (this._dynamicConnections == null)
                {
                    this._dynamicConnections = new WebPartConnectionCollection(this);
                }
                return this._dynamicConnections;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("WebPartManager_EnableClientScript"), DefaultValue(true)]
        public virtual bool EnableClientScript
        {
            get
            {
                object obj2 = this.ViewState["EnableClientScript"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["EnableClientScript"] = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(true)]
        public override bool EnableTheming
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("WebPartManager_CantSetEnableTheming"));
            }
        }

        [WebSysDescription("WebPartManager_ExportSensitiveDataWarning"), WebSysDefaultValue("WebPartChrome_ConfirmExportSensitive"), WebCategory("Behavior")]
        public virtual string ExportSensitiveDataWarning
        {
            get
            {
                object obj2 = this.ViewState["ExportSensitiveDataWarning"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("WebPartChrome_ConfirmExportSensitive");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ExportSensitiveDataWarning"] = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected WebPartManagerInternals Internals
        {
            get
            {
                if (this._internals == null)
                {
                    this._internals = new WebPartManagerInternals(this);
                }
                return this._internals;
            }
        }

        protected virtual bool IsCustomPersonalizationStateDirty
        {
            get
            {
                return this._hasDataChanged;
            }
        }

        protected virtual PermissionSet MediumPermissionSet
        {
            get
            {
                if (this._mediumPermissionSet == null)
                {
                    this._mediumPermissionSet = new PermissionSet(PermissionState.None);
                    this._mediumPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    this._mediumPermissionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));
                }
                return this._mediumPermissionSet;
            }
        }

        protected virtual PermissionSet MinimalPermissionSet
        {
            get
            {
                if (this._minimalPermissionSet == null)
                {
                    this._minimalPermissionSet = new PermissionSet(PermissionState.None);
                    this._minimalPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    this._minimalPermissionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal));
                }
                return this._minimalPermissionSet;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebSysDescription("WebPartManager_Personalization"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Behavior")]
        public WebPartPersonalization Personalization
        {
            get
            {
                if (this._personalization == null)
                {
                    this._personalization = this.CreatePersonalization();
                }
                return this._personalization;
            }
        }

        internal bool RenderClientScript
        {
            get
            {
                return this._renderClientScript;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPart SelectedWebPart
        {
            get
            {
                return this._selectedWebPart;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue("")]
        public override string SkinID
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebPartManager_StaticConnections"), WebCategory("Behavior"), DefaultValue((string) null), MergableProperty(false)]
        public WebPartConnectionCollection StaticConnections
        {
            get
            {
                if (this._staticConnections == null)
                {
                    this._staticConnections = new WebPartConnectionCollection(this);
                }
                return this._staticConnections;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPartDisplayModeCollection SupportedDisplayModes
        {
            get
            {
                if (this._supportedDisplayModes == null)
                {
                    this._supportedDisplayModes = new WebPartDisplayModeCollection();
                    foreach (WebPartDisplayMode mode in this.DisplayModes)
                    {
                        if (!mode.AssociatedWithToolZone)
                        {
                            this._supportedDisplayModes.Add(mode);
                        }
                    }
                    this._supportedDisplayModes.SetReadOnly("WebPartManager_DisplayModesReadOnly");
                }
                return this._supportedDisplayModes;
            }
        }

        bool IPersonalizable.IsDirty
        {
            get
            {
                return this.IsCustomPersonalizationStateDirty;
            }
        }

        private bool UsePermitOnly
        {
            get
            {
                if (!this._usePermitOnly.HasValue)
                {
                    this._usePermitOnly = new bool?(RuntimeConfig.GetAppConfig().Trust.LegacyCasModel);
                }
                return this._usePermitOnly.Value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override bool Visible
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("ControlNonVisual", new object[] { base.GetType().Name }));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebPartCollection WebParts
        {
            get
            {
                if (this.HasControls())
                {
                    return new WebPartCollection(this.Controls);
                }
                return new WebPartCollection();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPartZoneCollection Zones
        {
            get
            {
                return this._webPartZones;
            }
        }

        private sealed class BrowseWebPartDisplayMode : WebPartDisplayMode
        {
            public BrowseWebPartDisplayMode() : base("Browse")
            {
            }
        }

        private sealed class CatalogWebPartDisplayMode : WebPartDisplayMode
        {
            public CatalogWebPartDisplayMode() : base("Catalog")
            {
            }

            public override bool AllowPageDesign
            {
                get
                {
                    return true;
                }
            }

            public override bool AssociatedWithToolZone
            {
                get
                {
                    return true;
                }
            }

            public override bool RequiresPersonalization
            {
                get
                {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class ConnectionPointKey
        {
            private CultureInfo _culture;
            private Type _type;
            private CultureInfo _uiCulture;

            public ConnectionPointKey(Type type, CultureInfo culture, CultureInfo uiCulture)
            {
                this._type = type;
                this._culture = culture;
                this._uiCulture = uiCulture;
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }
                WebPartManager.ConnectionPointKey key = obj as WebPartManager.ConnectionPointKey;
                return ((((key != null) && key._type.Equals(this._type)) && key._culture.Equals(this._culture)) && key._uiCulture.Equals(this._uiCulture));
            }

            public override int GetHashCode()
            {
                int hashCode = this._type.GetHashCode();
                int num2 = ((hashCode << 5) + hashCode) ^ this._culture.GetHashCode();
                return (((num2 << 5) + num2) ^ this._uiCulture.GetHashCode());
            }
        }

        private sealed class ConnectWebPartDisplayMode : WebPartDisplayMode
        {
            public ConnectWebPartDisplayMode() : base("Connect")
            {
            }

            public override bool AllowPageDesign
            {
                get
                {
                    return true;
                }
            }

            public override bool AssociatedWithToolZone
            {
                get
                {
                    return true;
                }
            }

            public override bool RequiresPersonalization
            {
                get
                {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class DesignWebPartDisplayMode : WebPartDisplayMode
        {
            public DesignWebPartDisplayMode() : base("Design")
            {
            }

            public override bool AllowPageDesign
            {
                get
                {
                    return true;
                }
            }

            public override bool RequiresPersonalization
            {
                get
                {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class EditWebPartDisplayMode : WebPartDisplayMode
        {
            public EditWebPartDisplayMode() : base("Edit")
            {
            }

            public override bool AllowPageDesign
            {
                get
                {
                    return true;
                }
            }

            public override bool AssociatedWithToolZone
            {
                get
                {
                    return true;
                }
            }

            public override bool RequiresPersonalization
            {
                get
                {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class WebPartManagerControlCollection : ControlCollection
        {
            private WebPartManager _manager;

            public WebPartManagerControlCollection(WebPartManager owner) : base(owner)
            {
                this._manager = owner;
                base.SetCollectionReadOnly("WebPartManager_CannotModify");
            }

            internal void AddWebPart(WebPart webPart)
            {
                string errorMsg = base.SetCollectionReadOnly(null);
                try
                {
                    try
                    {
                        this.AddWebPartHelper(webPart);
                    }
                    finally
                    {
                        base.SetCollectionReadOnly(errorMsg);
                    }
                }
                catch
                {
                    throw;
                }
            }

            private void AddWebPartHelper(WebPart webPart)
            {
                string iD = webPart.ID;
                if (string.IsNullOrEmpty(iD))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_NoWebPartID"));
                }
                if (this._manager._partAndChildControlIDs.Contains(iD))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_DuplicateWebPartID", new object[] { iD }));
                }
                this._manager._partAndChildControlIDs.Add(iD, null);
                GenericWebPart part = webPart as GenericWebPart;
                if (part != null)
                {
                    string str2 = part.ChildControl.ID;
                    if (string.IsNullOrEmpty(str2))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_NoChildControlID"));
                    }
                    if (this._manager._partAndChildControlIDs.Contains(str2))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManager_DuplicateWebPartID", new object[] { str2 }));
                    }
                    this._manager._partAndChildControlIDs.Add(str2, null);
                }
                this._manager.Internals.SetIsStandalone(webPart, false);
                webPart.SetWebPartManager(this._manager);
                this.Add(webPart);
                this._manager._partsForZone = null;
            }

            internal void AddWebPartsFromZone(WebPartZoneBase zone, WebPartCollection webParts)
            {
                if ((webParts != null) && (webParts.Count != 0))
                {
                    string errorMsg = base.SetCollectionReadOnly(null);
                    try
                    {
                        try
                        {
                            string iD = zone.ID;
                            int zoneIndex = 0;
                            foreach (WebPart part in webParts)
                            {
                                this._manager.Internals.SetIsShared(part, true);
                                WebPart webPart = part;
                                if (!this._manager.IsAuthorized(part))
                                {
                                    webPart = new UnauthorizedWebPart(part);
                                }
                                this._manager.Internals.SetIsStatic(webPart, true);
                                this._manager.Internals.SetIsShared(webPart, true);
                                this._manager.Internals.SetZoneID(webPart, iD);
                                this._manager.Internals.SetZoneIndex(webPart, zoneIndex);
                                this.AddWebPartHelper(webPart);
                                zoneIndex++;
                            }
                        }
                        finally
                        {
                            base.SetCollectionReadOnly(errorMsg);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            internal void RemoveWebPart(WebPart webPart)
            {
                string errorMsg = base.SetCollectionReadOnly(null);
                try
                {
                    try
                    {
                        this._manager._partAndChildControlIDs.Remove(webPart.ID);
                        GenericWebPart part = webPart as GenericWebPart;
                        if (part != null)
                        {
                            this._manager._partAndChildControlIDs.Remove(part.ChildControl.ID);
                        }
                        this.Remove(webPart);
                        this._manager._hasDataChanged = true;
                        webPart.SetWebPartManager(null);
                        this._manager.Internals.SetIsStandalone(webPart, true);
                        this._manager._partsForZone = null;
                    }
                    finally
                    {
                        base.SetCollectionReadOnly(errorMsg);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}

