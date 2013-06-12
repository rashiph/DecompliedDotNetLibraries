namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ParseChildren(true, "Transformers"), TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class WebPartConnection
    {
        private string _consumerConnectionPointID;
        private string _consumerID;
        private bool _deleted;
        private string _id;
        private bool _isActive;
        private bool _isShared = true;
        private bool _isStatic = true;
        private string _providerConnectionPointID;
        private string _providerID;
        private WebPartTransformerCollection _transformers;
        private WebPartManager _webPartManager;

        internal void Activate()
        {
            this.Transformers.SetReadOnly();
            WebPart provider = this.Provider;
            WebPart consumer = this.Consumer;
            Control control = provider.ToControl();
            Control control2 = consumer.ToControl();
            System.Web.UI.WebControls.WebParts.ProviderConnectionPoint providerConnectionPoint = this.ProviderConnectionPoint;
            if (!providerConnectionPoint.GetEnabled(control))
            {
                consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_DisabledConnectionPoint", new object[] { providerConnectionPoint.DisplayName, provider.DisplayTitle }));
            }
            else
            {
                System.Web.UI.WebControls.WebParts.ConsumerConnectionPoint consumerConnectionPoint = this.ConsumerConnectionPoint;
                if (!consumerConnectionPoint.GetEnabled(control2))
                {
                    consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_DisabledConnectionPoint", new object[] { consumerConnectionPoint.DisplayName, consumer.DisplayTitle }));
                }
                else if (!provider.IsClosed && !consumer.IsClosed)
                {
                    WebPartTransformer transformer = this.Transformer;
                    if (transformer == null)
                    {
                        if (providerConnectionPoint.InterfaceType == consumerConnectionPoint.InterfaceType)
                        {
                            ConnectionInterfaceCollection secondaryInterfaces = providerConnectionPoint.GetSecondaryInterfaces(control);
                            if (consumerConnectionPoint.SupportsConnection(control2, secondaryInterfaces))
                            {
                                object data = providerConnectionPoint.GetObject(control);
                                consumerConnectionPoint.SetObject(control2, data);
                                this._isActive = true;
                            }
                            else
                            {
                                consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_IncompatibleSecondaryInterfaces", new string[] { consumerConnectionPoint.DisplayName, consumer.DisplayTitle, providerConnectionPoint.DisplayName, provider.DisplayTitle }));
                            }
                        }
                        else
                        {
                            consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_NoCommonInterface", new string[] { providerConnectionPoint.DisplayName, provider.DisplayTitle, consumerConnectionPoint.DisplayName, consumer.DisplayTitle }));
                        }
                    }
                    else
                    {
                        Type type = transformer.GetType();
                        if (!this._webPartManager.AvailableTransformers.Contains(type))
                        {
                            string str;
                            if ((this._webPartManager.Context != null) && this._webPartManager.Context.IsCustomErrorEnabled)
                            {
                                str = System.Web.SR.GetString("WebPartConnection_TransformerNotAvailable");
                            }
                            else
                            {
                                str = System.Web.SR.GetString("WebPartConnection_TransformerNotAvailableWithType", new object[] { type.FullName });
                            }
                            consumer.SetConnectErrorMessage(str);
                        }
                        Type consumerType = WebPartTransformerAttribute.GetConsumerType(type);
                        Type providerType = WebPartTransformerAttribute.GetProviderType(type);
                        if ((providerConnectionPoint.InterfaceType == consumerType) && (providerType == consumerConnectionPoint.InterfaceType))
                        {
                            if (consumerConnectionPoint.SupportsConnection(control2, ConnectionInterfaceCollection.Empty))
                            {
                                object providerData = providerConnectionPoint.GetObject(control);
                                object obj4 = transformer.Transform(providerData);
                                consumerConnectionPoint.SetObject(control2, obj4);
                                this._isActive = true;
                            }
                            else
                            {
                                consumer.SetConnectErrorMessage(System.Web.SR.GetString("WebPartConnection_ConsumerRequiresSecondaryInterfaces", new object[] { consumerConnectionPoint.DisplayName, consumer.DisplayTitle }));
                            }
                        }
                        else if (providerConnectionPoint.InterfaceType != consumerType)
                        {
                            string str2;
                            if ((this._webPartManager.Context != null) && this._webPartManager.Context.IsCustomErrorEnabled)
                            {
                                str2 = System.Web.SR.GetString("WebPartConnection_IncompatibleProviderTransformer", new object[] { providerConnectionPoint.DisplayName, provider.DisplayTitle });
                            }
                            else
                            {
                                str2 = System.Web.SR.GetString("WebPartConnection_IncompatibleProviderTransformerWithType", new object[] { providerConnectionPoint.DisplayName, provider.DisplayTitle, type.FullName });
                            }
                            consumer.SetConnectErrorMessage(str2);
                        }
                        else
                        {
                            string str3;
                            if ((this._webPartManager.Context != null) && this._webPartManager.Context.IsCustomErrorEnabled)
                            {
                                str3 = System.Web.SR.GetString("WebPartConnection_IncompatibleConsumerTransformer", new object[] { consumerConnectionPoint.DisplayName, consumer.DisplayTitle });
                            }
                            else
                            {
                                str3 = System.Web.SR.GetString("WebPartConnection_IncompatibleConsumerTransformerWithType", new object[] { type.FullName, consumerConnectionPoint.DisplayName, consumer.DisplayTitle });
                            }
                            consumer.SetConnectErrorMessage(str3);
                        }
                    }
                }
            }
        }

        internal bool ConflictsWith(WebPartConnection otherConnection)
        {
            if (!this.ConflictsWithConsumer(otherConnection))
            {
                return this.ConflictsWithProvider(otherConnection);
            }
            return true;
        }

        internal bool ConflictsWithConsumer(WebPartConnection otherConnection)
        {
            return ((!this.ConsumerConnectionPoint.AllowsMultipleConnections && (this.Consumer == otherConnection.Consumer)) && (this.ConsumerConnectionPoint == otherConnection.ConsumerConnectionPoint));
        }

        internal bool ConflictsWithProvider(WebPartConnection otherConnection)
        {
            return ((!this.ProviderConnectionPoint.AllowsMultipleConnections && (this.Provider == otherConnection.Provider)) && (this.ProviderConnectionPoint == otherConnection.ProviderConnectionPoint));
        }

        internal void SetIsShared(bool isShared)
        {
            this._isShared = isShared;
        }

        internal void SetIsStatic(bool isStatic)
        {
            this._isStatic = isStatic;
        }

        internal void SetTransformer(WebPartTransformer transformer)
        {
            if (this.Transformers.Count == 0)
            {
                this.Transformers.Add(transformer);
            }
            else
            {
                this.Transformers[0] = transformer;
            }
        }

        internal void SetWebPartManager(WebPartManager webPartManager)
        {
            this._webPartManager = webPartManager;
        }

        public override string ToString()
        {
            return base.GetType().Name;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPart Consumer
        {
            get
            {
                string consumerID = this.ConsumerID;
                if (consumerID.Length == 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_ConsumerIDNotSet"));
                }
                if (this._webPartManager != null)
                {
                    return this._webPartManager.WebParts[consumerID];
                }
                return null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.WebControls.WebParts.ConsumerConnectionPoint ConsumerConnectionPoint
        {
            get
            {
                WebPart consumer = this.Consumer;
                if ((consumer != null) && (this._webPartManager != null))
                {
                    return this._webPartManager.GetConsumerConnectionPoint(consumer, this.ConsumerConnectionPointID);
                }
                return null;
            }
        }

        [DefaultValue("default")]
        public string ConsumerConnectionPointID
        {
            get
            {
                if (string.IsNullOrEmpty(this._consumerConnectionPointID))
                {
                    return ConnectionPoint.DefaultID;
                }
                return this._consumerConnectionPointID;
            }
            set
            {
                this._consumerConnectionPointID = value;
            }
        }

        [DefaultValue("")]
        public string ConsumerID
        {
            get
            {
                if (this._consumerID == null)
                {
                    return string.Empty;
                }
                return this._consumerID;
            }
            set
            {
                this._consumerID = value;
            }
        }

        internal bool Deleted
        {
            get
            {
                return this._deleted;
            }
            set
            {
                this._deleted = value;
            }
        }

        [DefaultValue("")]
        public string ID
        {
            get
            {
                if (this._id == null)
                {
                    return string.Empty;
                }
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsShared
        {
            get
            {
                return this._isShared;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsStatic
        {
            get
            {
                return this._isStatic;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WebPart Provider
        {
            get
            {
                string providerID = this.ProviderID;
                if (providerID.Length == 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartConnection_ProviderIDNotSet"));
                }
                if (this._webPartManager != null)
                {
                    return this._webPartManager.WebParts[providerID];
                }
                return null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.WebControls.WebParts.ProviderConnectionPoint ProviderConnectionPoint
        {
            get
            {
                WebPart provider = this.Provider;
                if ((provider != null) && (this._webPartManager != null))
                {
                    return this._webPartManager.GetProviderConnectionPoint(provider, this.ProviderConnectionPointID);
                }
                return null;
            }
        }

        [DefaultValue("default")]
        public string ProviderConnectionPointID
        {
            get
            {
                if (string.IsNullOrEmpty(this._providerConnectionPointID))
                {
                    return ConnectionPoint.DefaultID;
                }
                return this._providerConnectionPointID;
            }
            set
            {
                this._providerConnectionPointID = value;
            }
        }

        [DefaultValue("")]
        public string ProviderID
        {
            get
            {
                if (this._providerID == null)
                {
                    return string.Empty;
                }
                return this._providerID;
            }
            set
            {
                this._providerID = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebPartTransformer Transformer
        {
            get
            {
                if ((this._transformers != null) && (this._transformers.Count != 0))
                {
                    return this._transformers[0];
                }
                return null;
            }
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public WebPartTransformerCollection Transformers
        {
            get
            {
                if (this._transformers == null)
                {
                    this._transformers = new WebPartTransformerCollection();
                }
                return this._transformers;
            }
        }
    }
}

