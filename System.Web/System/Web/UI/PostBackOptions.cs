namespace System.Web.UI
{
    using System;
    using System.ComponentModel;

    public sealed class PostBackOptions
    {
        private string _actionUrl;
        private string _argument;
        private bool _autoPostBack;
        private bool _clientSubmit;
        private bool _performValidation;
        private bool _requiresJavaScriptProtocol;
        private Control _targetControl;
        private bool _trackFocus;
        private string _validationGroup;

        public PostBackOptions(Control targetControl) : this(targetControl, null, null, false, false, false, true, false, null)
        {
        }

        public PostBackOptions(Control targetControl, string argument) : this(targetControl, argument, null, false, false, false, true, false, null)
        {
        }

        public PostBackOptions(Control targetControl, string argument, string actionUrl, bool autoPostBack, bool requiresJavaScriptProtocol, bool trackFocus, bool clientSubmit, bool performValidation, string validationGroup)
        {
            this._clientSubmit = true;
            if (targetControl == null)
            {
                throw new ArgumentNullException("targetControl");
            }
            this._actionUrl = actionUrl;
            this._argument = argument;
            this._autoPostBack = autoPostBack;
            this._clientSubmit = clientSubmit;
            this._requiresJavaScriptProtocol = requiresJavaScriptProtocol;
            this._performValidation = performValidation;
            this._trackFocus = trackFocus;
            this._targetControl = targetControl;
            this._validationGroup = validationGroup;
        }

        [DefaultValue("")]
        public string ActionUrl
        {
            get
            {
                return this._actionUrl;
            }
            set
            {
                this._actionUrl = value;
            }
        }

        [DefaultValue("")]
        public string Argument
        {
            get
            {
                return this._argument;
            }
            set
            {
                this._argument = value;
            }
        }

        [DefaultValue(false)]
        public bool AutoPostBack
        {
            get
            {
                return this._autoPostBack;
            }
            set
            {
                this._autoPostBack = value;
            }
        }

        [DefaultValue(true)]
        public bool ClientSubmit
        {
            get
            {
                return this._clientSubmit;
            }
            set
            {
                this._clientSubmit = value;
            }
        }

        [DefaultValue(false)]
        public bool PerformValidation
        {
            get
            {
                return this._performValidation;
            }
            set
            {
                this._performValidation = value;
            }
        }

        [DefaultValue(true)]
        public bool RequiresJavaScriptProtocol
        {
            get
            {
                return this._requiresJavaScriptProtocol;
            }
            set
            {
                this._requiresJavaScriptProtocol = value;
            }
        }

        [DefaultValue((string) null)]
        public Control TargetControl
        {
            get
            {
                return this._targetControl;
            }
        }

        [DefaultValue(false)]
        public bool TrackFocus
        {
            get
            {
                return this._trackFocus;
            }
            set
            {
                this._trackFocus = value;
            }
        }

        [DefaultValue("")]
        public string ValidationGroup
        {
            get
            {
                return this._validationGroup;
            }
            set
            {
                this._validationGroup = value;
            }
        }
    }
}

