namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    public sealed class HttpRuntimeSection : ConfigurationSection
    {
        private int _MaxQueryStringLength;
        private int _MaxRequestLengthBytes = -1;
        private int _MaxUrlLength;
        private static readonly ConfigurationProperty _propApartmentThreading = new ConfigurationProperty("apartmentThreading", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAppRequestQueueLimit = new ConfigurationProperty("appRequestQueueLimit", typeof(int), 0x1388, null, StdValidatorsAndConverters.NonZeroPositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDelayNotificationTimeout = new ConfigurationProperty("delayNotificationTimeout", typeof(TimeSpan), TimeSpan.FromSeconds(0.0), StdValidatorsAndConverters.TimeSpanSecondsConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnable = new ConfigurationProperty("enable", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableHeaderChecking = new ConfigurationProperty("enableHeaderChecking", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableKernelOutputCache = new ConfigurationProperty("enableKernelOutputCache", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableVersionHeader = new ConfigurationProperty("enableVersionHeader", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEncoderType = new ConfigurationProperty("encoderType", typeof(string), "System.Web.Util.HttpEncoder", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propExecutionTimeout = new ConfigurationProperty("executionTimeout", typeof(TimeSpan), TimeSpan.FromSeconds(110.0), StdValidatorsAndConverters.TimeSpanSecondsConverter, StdValidatorsAndConverters.PositiveTimeSpanValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxQueryStringLength = new ConfigurationProperty("maxQueryStringLength", typeof(int), 0x800, null, new IntegerValidator(0, 0x1fffff), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxRequestLength = new ConfigurationProperty("maxRequestLength", typeof(int), 0x1000, null, StdValidatorsAndConverters.NonZeroPositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxUrlLength = new ConfigurationProperty("maxUrlLength", typeof(int), 260, null, new IntegerValidator(0, 0x1fffff), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxWaitChangeNotification = new ConfigurationProperty("maxWaitChangeNotification", typeof(int), 0, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinFreeThreads = new ConfigurationProperty("minFreeThreads", typeof(int), 8, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinLocalRequestFreeThreads = new ConfigurationProperty("minLocalRequestFreeThreads", typeof(int), 4, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRelaxedUrlToFileSystemMapping = new ConfigurationProperty("relaxedUrlToFileSystemMapping", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestLengthDiskThreshold = new ConfigurationProperty("requestLengthDiskThreshold", typeof(int), 80, null, StdValidatorsAndConverters.NonZeroPositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestPathInvalidCharacters = new ConfigurationProperty("requestPathInvalidCharacters", typeof(string), @"<,>,*,%,&,:,\,?", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestValidationMode = new ConfigurationProperty("requestValidationMode", typeof(Version), DefaultRequestValidationMode, StdValidatorsAndConverters.VersionConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestValidationType = new ConfigurationProperty("requestValidationType", typeof(string), "System.Web.Util.RequestValidator", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequireRootedSaveAsPath = new ConfigurationProperty("requireRootedSaveAsPath", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSendCacheControlHeader = new ConfigurationProperty("sendCacheControlHeader", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propShutdownTimeout = new ConfigurationProperty("shutdownTimeout", typeof(TimeSpan), TimeSpan.FromSeconds(90.0), StdValidatorsAndConverters.TimeSpanSecondsConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUseFullyQualifiedRedirectUrl = new ConfigurationProperty("useFullyQualifiedRedirectUrl", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propWaitChangeNotification = new ConfigurationProperty("waitChangeNotification", typeof(int), 0, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private int _RequestLengthDiskThresholdBytes = -1;
        private char[] _RequestPathInvalidCharactersArray;
        private Version _requestValidationMode;
        internal const int DefaultAppRequestQueueLimit = 100;
        internal const int DefaultDelayNotificationTimeout = 0;
        internal const bool DefaultEnableKernelOutputCache = true;
        internal const string DefaultEncoderType = "System.Web.Util.HttpEncoder";
        internal const int DefaultExecutionTimeout = 110;
        internal const int DefaultMaxQueryStringLength = 0x800;
        internal const int DefaultMaxRequestLength = 0x400000;
        internal const int DefaultMaxUrlLength = 260;
        internal const int DefaultMaxWaitChangeNotification = 0;
        internal const int DefaultMinFreeThreads = 8;
        internal const int DefaultMinLocalRequestFreeThreads = 4;
        internal const bool DefaultRelaxedUrlToFileSystemMapping = false;
        internal const int DefaultRequestLengthDiskThreshold = 0x14000;
        internal const string DefaultRequestPathInvalidCharacters = @"<,>,*,%,&,:,\,?";
        internal static readonly Version DefaultRequestValidationMode = VersionUtil.Framework40;
        internal const string DefaultRequestValidationModeString = "4.0";
        internal const string DefaultRequestValidationType = "System.Web.Util.RequestValidator";
        internal const bool DefaultRequireRootedSaveAsPath = true;
        internal const bool DefaultSendCacheControlHeader = true;
        internal const int DefaultShutdownTimeout = 90;
        internal const int DefaultWaitChangeNotification = 0;
        private bool enableVersionHeaderCache = true;
        private bool enableVersionHeaderCached;
        private TimeSpan executionTimeoutCache;
        private bool executionTimeoutCached;
        private static string s_versionHeader = null;
        private bool sendCacheControlHeaderCache;
        private bool sendCacheControlHeaderCached;

        static HttpRuntimeSection()
        {
            _properties.Add(_propExecutionTimeout);
            _properties.Add(_propMaxRequestLength);
            _properties.Add(_propRequestLengthDiskThreshold);
            _properties.Add(_propUseFullyQualifiedRedirectUrl);
            _properties.Add(_propMinFreeThreads);
            _properties.Add(_propMinLocalRequestFreeThreads);
            _properties.Add(_propAppRequestQueueLimit);
            _properties.Add(_propEnableKernelOutputCache);
            _properties.Add(_propEnableVersionHeader);
            _properties.Add(_propRequireRootedSaveAsPath);
            _properties.Add(_propEnable);
            _properties.Add(_propShutdownTimeout);
            _properties.Add(_propDelayNotificationTimeout);
            _properties.Add(_propWaitChangeNotification);
            _properties.Add(_propMaxWaitChangeNotification);
            _properties.Add(_propEnableHeaderChecking);
            _properties.Add(_propSendCacheControlHeader);
            _properties.Add(_propApartmentThreading);
            _properties.Add(_propEncoderType);
            _properties.Add(_propRequestValidationMode);
            _properties.Add(_propRequestValidationType);
            _properties.Add(_propRequestPathInvalidCharacters);
            _properties.Add(_propMaxUrlLength);
            _properties.Add(_propMaxQueryStringLength);
            _properties.Add(_propRelaxedUrlToFileSystemMapping);
        }

        private int BytesFromKilobytes(int kilobytes)
        {
            long num = kilobytes * 0x400L;
            if (num >= 0x7fffffffL)
            {
                return 0x7fffffff;
            }
            return (int) num;
        }

        private static char[] DecodeAndThenSplitString(string invalidCharString)
        {
            if (string.IsNullOrEmpty(invalidCharString))
            {
                return new char[0];
            }
            string[] strArray = HttpUtility.UrlDecode(invalidCharString, Encoding.UTF8).Split(new char[] { ',' });
            char[] chArray = new char[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                string str = strArray[i].Trim();
                if (str.Length == 1)
                {
                    chArray[i] = str[0];
                }
                else
                {
                    return null;
                }
            }
            return chArray;
        }

        private static char[] SplitStringAndThenDecode(string invalidCharString)
        {
            if (string.IsNullOrEmpty(invalidCharString))
            {
                return new char[0];
            }
            string[] strArray = invalidCharString.Split(new char[] { ',' });
            char[] chArray = new char[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                string str = HttpUtility.UrlDecode(strArray[i], Encoding.UTF8).Trim();
                if (str.Length == 1)
                {
                    chArray[i] = str[0];
                }
                else
                {
                    return null;
                }
            }
            return chArray;
        }

        [ConfigurationProperty("apartmentThreading", DefaultValue=false)]
        public bool ApartmentThreading
        {
            get
            {
                return (bool) base[_propApartmentThreading];
            }
            set
            {
                base[_propApartmentThreading] = value;
            }
        }

        [ConfigurationProperty("appRequestQueueLimit", DefaultValue=0x1388), IntegerValidator(MinValue=1)]
        public int AppRequestQueueLimit
        {
            get
            {
                return (int) base[_propAppRequestQueueLimit];
            }
            set
            {
                base[_propAppRequestQueueLimit] = value;
            }
        }

        [ConfigurationProperty("delayNotificationTimeout", DefaultValue="00:00:00"), TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan DelayNotificationTimeout
        {
            get
            {
                return (TimeSpan) base[_propDelayNotificationTimeout];
            }
            set
            {
                base[_propDelayNotificationTimeout] = value;
            }
        }

        [ConfigurationProperty("enable", DefaultValue=true)]
        public bool Enable
        {
            get
            {
                return (bool) base[_propEnable];
            }
            set
            {
                base[_propEnable] = value;
            }
        }

        [ConfigurationProperty("enableHeaderChecking", DefaultValue=true)]
        public bool EnableHeaderChecking
        {
            get
            {
                return (bool) base[_propEnableHeaderChecking];
            }
            set
            {
                base[_propEnableHeaderChecking] = value;
            }
        }

        [ConfigurationProperty("enableKernelOutputCache", DefaultValue=true)]
        public bool EnableKernelOutputCache
        {
            get
            {
                return (bool) base[_propEnableKernelOutputCache];
            }
            set
            {
                base[_propEnableKernelOutputCache] = value;
            }
        }

        [ConfigurationProperty("enableVersionHeader", DefaultValue=true)]
        public bool EnableVersionHeader
        {
            get
            {
                if (!this.enableVersionHeaderCached)
                {
                    this.enableVersionHeaderCache = (bool) base[_propEnableVersionHeader];
                    this.enableVersionHeaderCached = true;
                }
                return this.enableVersionHeaderCache;
            }
            set
            {
                base[_propEnableVersionHeader] = value;
                this.enableVersionHeaderCache = value;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("encoderType", DefaultValue="System.Web.Util.HttpEncoder")]
        public string EncoderType
        {
            get
            {
                return (string) base[_propEncoderType];
            }
            set
            {
                base[_propEncoderType] = value;
            }
        }

        [ConfigurationProperty("executionTimeout", DefaultValue="00:01:50"), TimeSpanValidator(MinValueString="00:00:00", MaxValueString="10675199.02:48:05.4775807"), TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan ExecutionTimeout
        {
            get
            {
                if (!this.executionTimeoutCached)
                {
                    this.executionTimeoutCache = (TimeSpan) base[_propExecutionTimeout];
                    this.executionTimeoutCached = true;
                }
                return this.executionTimeoutCache;
            }
            set
            {
                base[_propExecutionTimeout] = value;
                this.executionTimeoutCache = value;
            }
        }

        [ConfigurationProperty("maxQueryStringLength", DefaultValue=0x800), IntegerValidator(MinValue=0)]
        public int MaxQueryStringLength
        {
            get
            {
                if (this._MaxQueryStringLength == 0)
                {
                    this._MaxQueryStringLength = (int) base[_propMaxQueryStringLength];
                }
                return this._MaxQueryStringLength;
            }
            set
            {
                this._MaxQueryStringLength = value;
                base[_propMaxQueryStringLength] = value;
            }
        }

        [ConfigurationProperty("maxRequestLength", DefaultValue=0x1000), IntegerValidator(MinValue=0)]
        public int MaxRequestLength
        {
            get
            {
                return (int) base[_propMaxRequestLength];
            }
            set
            {
                if (value < this.RequestLengthDiskThreshold)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_max_request_length_smaller_than_max_request_length_disk_threshold"), base.ElementInformation.Properties[_propMaxRequestLength.Name].Source, base.ElementInformation.Properties[_propMaxRequestLength.Name].LineNumber);
                }
                base[_propMaxRequestLength] = value;
            }
        }

        internal int MaxRequestLengthBytes
        {
            get
            {
                if (this._MaxRequestLengthBytes < 0)
                {
                    this._MaxRequestLengthBytes = this.BytesFromKilobytes(this.MaxRequestLength);
                }
                return this._MaxRequestLengthBytes;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("maxUrlLength", DefaultValue=260)]
        public int MaxUrlLength
        {
            get
            {
                if (this._MaxUrlLength == 0)
                {
                    this._MaxUrlLength = (int) base[_propMaxUrlLength];
                }
                return this._MaxUrlLength;
            }
            set
            {
                this._MaxUrlLength = value;
                base[_propMaxUrlLength] = value;
            }
        }

        [ConfigurationProperty("maxWaitChangeNotification", DefaultValue=0), IntegerValidator(MinValue=0)]
        public int MaxWaitChangeNotification
        {
            get
            {
                return (int) base[_propMaxWaitChangeNotification];
            }
            set
            {
                base[_propMaxWaitChangeNotification] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("minFreeThreads", DefaultValue=8)]
        public int MinFreeThreads
        {
            get
            {
                return (int) base[_propMinFreeThreads];
            }
            set
            {
                base[_propMinFreeThreads] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("minLocalRequestFreeThreads", DefaultValue=4)]
        public int MinLocalRequestFreeThreads
        {
            get
            {
                return (int) base[_propMinLocalRequestFreeThreads];
            }
            set
            {
                base[_propMinLocalRequestFreeThreads] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("relaxedUrlToFileSystemMapping", DefaultValue=false)]
        public bool RelaxedUrlToFileSystemMapping
        {
            get
            {
                return (bool) base[_propRelaxedUrlToFileSystemMapping];
            }
            set
            {
                base[_propRelaxedUrlToFileSystemMapping] = value;
            }
        }

        [ConfigurationProperty("requestLengthDiskThreshold", DefaultValue=80), IntegerValidator(MinValue=1)]
        public int RequestLengthDiskThreshold
        {
            get
            {
                return (int) base[_propRequestLengthDiskThreshold];
            }
            set
            {
                if (value > this.MaxRequestLength)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_max_request_length_disk_threshold_exceeds_max_request_length"), base.ElementInformation.Properties[_propRequestLengthDiskThreshold.Name].Source, base.ElementInformation.Properties[_propRequestLengthDiskThreshold.Name].LineNumber);
                }
                base[_propRequestLengthDiskThreshold] = value;
            }
        }

        internal int RequestLengthDiskThresholdBytes
        {
            get
            {
                if (this._RequestLengthDiskThresholdBytes < 0)
                {
                    this._RequestLengthDiskThresholdBytes = this.BytesFromKilobytes(this.RequestLengthDiskThreshold);
                }
                return this._RequestLengthDiskThresholdBytes;
            }
        }

        [ConfigurationProperty("requestPathInvalidCharacters", DefaultValue=@"<,>,*,%,&,:,\,?")]
        public string RequestPathInvalidCharacters
        {
            get
            {
                return (string) base[_propRequestPathInvalidCharacters];
            }
            set
            {
                base[_propRequestPathInvalidCharacters] = value;
                this._RequestPathInvalidCharactersArray = null;
            }
        }

        internal char[] RequestPathInvalidCharactersArray
        {
            get
            {
                if (this._RequestPathInvalidCharactersArray == null)
                {
                    this._RequestPathInvalidCharactersArray = DecodeAndThenSplitString(this.RequestPathInvalidCharacters);
                    if (this._RequestPathInvalidCharactersArray == null)
                    {
                        this._RequestPathInvalidCharactersArray = SplitStringAndThenDecode(this.RequestPathInvalidCharacters);
                    }
                    if (this._RequestPathInvalidCharactersArray == null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_property_generic"), base.ElementInformation.Properties[_propRequestPathInvalidCharacters.Name].Source, base.ElementInformation.Properties[_propRequestPathInvalidCharacters.Name].LineNumber);
                    }
                }
                return this._RequestPathInvalidCharactersArray;
            }
        }

        [TypeConverter(typeof(VersionConverter)), ConfigurationProperty("requestValidationMode", DefaultValue="4.0")]
        public Version RequestValidationMode
        {
            get
            {
                if (this._requestValidationMode == null)
                {
                    this._requestValidationMode = (Version) base[_propRequestValidationMode];
                }
                return this._requestValidationMode;
            }
            set
            {
                this._requestValidationMode = value;
                base[_propRequestValidationMode] = value;
            }
        }

        [ConfigurationProperty("requestValidationType", DefaultValue="System.Web.Util.RequestValidator"), StringValidator(MinLength=1)]
        public string RequestValidationType
        {
            get
            {
                return (string) base[_propRequestValidationType];
            }
            set
            {
                base[_propRequestValidationType] = value;
            }
        }

        [ConfigurationProperty("requireRootedSaveAsPath", DefaultValue=true)]
        public bool RequireRootedSaveAsPath
        {
            get
            {
                return (bool) base[_propRequireRootedSaveAsPath];
            }
            set
            {
                base[_propRequireRootedSaveAsPath] = value;
            }
        }

        [ConfigurationProperty("sendCacheControlHeader", DefaultValue=true)]
        public bool SendCacheControlHeader
        {
            get
            {
                if (!this.sendCacheControlHeaderCached)
                {
                    this.sendCacheControlHeaderCache = (bool) base[_propSendCacheControlHeader];
                    this.sendCacheControlHeaderCached = true;
                }
                return this.sendCacheControlHeaderCache;
            }
            set
            {
                base[_propSendCacheControlHeader] = value;
                this.sendCacheControlHeaderCache = value;
            }
        }

        [ConfigurationProperty("shutdownTimeout", DefaultValue="00:01:30"), TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan ShutdownTimeout
        {
            get
            {
                return (TimeSpan) base[_propShutdownTimeout];
            }
            set
            {
                base[_propShutdownTimeout] = value;
            }
        }

        [ConfigurationProperty("useFullyQualifiedRedirectUrl", DefaultValue=false)]
        public bool UseFullyQualifiedRedirectUrl
        {
            get
            {
                return (bool) base[_propUseFullyQualifiedRedirectUrl];
            }
            set
            {
                base[_propUseFullyQualifiedRedirectUrl] = value;
            }
        }

        internal string VersionHeader
        {
            get
            {
                if (!this.EnableVersionHeader)
                {
                    return null;
                }
                if (s_versionHeader == null)
                {
                    string str = null;
                    try
                    {
                        string systemWebVersion = VersionInfo.SystemWebVersion;
                        int length = systemWebVersion.LastIndexOf('.');
                        if (length > 0)
                        {
                            str = systemWebVersion.Substring(0, length);
                        }
                    }
                    catch
                    {
                    }
                    if (str == null)
                    {
                        str = string.Empty;
                    }
                    s_versionHeader = str;
                }
                return s_versionHeader;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("waitChangeNotification", DefaultValue=0)]
        public int WaitChangeNotification
        {
            get
            {
                return (int) base[_propWaitChangeNotification];
            }
            set
            {
                base[_propWaitChangeNotification] = value;
            }
        }
    }
}

