namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.UI;

    public class BrowserCapabilitiesFactory : BrowserCapabilitiesFactoryBase
    {
        private bool BlackberryProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"BlackBerry(?'deviceName'\w+)/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)"))
            {
                return false;
            }
            capabilities["layoutEngine"] = "BlackBerry";
            capabilities["browser"] = "BlackBerry";
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["type"] = worker["BlackBerry${major}"];
            capabilities["mobileDeviceModel"] = worker["${deviceName}"];
            capabilities["isMobileDevice"] = "true";
            capabilities["version"] = worker["${version}"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.3";
            capabilities["w3cdomversion"] = "1.0";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsDivNoWrap"] = "false";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            capabilities["canInitiateVoiceCall"] = "true";
            browserCaps.AddBrowser("BlackBerry");
            this.BlackberryProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.BlackberryProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void BlackberryProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void BlackberryProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool ChromeProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Chrome/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)"))
            {
                return false;
            }
            worker.ProcessRegex(browserCaps[string.Empty], @"AppleWebKit/(?'layoutVersion'\d+)");
            capabilities["layoutEngine"] = "WebKit";
            capabilities["layoutEngineVersion"] = worker["${layoutVersion}"];
            capabilities["browser"] = "Chrome";
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["type"] = worker["Chrome${major}"];
            capabilities["version"] = worker["${version}"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.7";
            capabilities["w3cdomversion"] = "1.0";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsDivNoWrap"] = "false";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            browserCaps.AddBrowser("Chrome");
            this.ChromeProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.ChromeProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void ChromeProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void ChromeProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        public override void ConfigureBrowserCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            this.DefaultProcess(headers, browserCaps);
            if (base.IsBrowserUnknown(browserCaps))
            {
                this.DefaultDefaultProcess(headers, browserCaps);
            }
        }

        private bool CpuProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = headers["UA-CPU"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "(?'cpu'.+)"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["cpu"] = worker["${cpu}"];
            this.CpuProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.CpuProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void CpuProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void CpuProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool CrawlerProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "crawler|Crawler|Googlebot|msnbot|bingbot"))
            {
                return false;
            }
            capabilities["crawler"] = "true";
            this.CrawlerProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.CrawlerProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void CrawlerProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void CrawlerProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool DefaultDefaultProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            capabilities["ecmascriptversion"] = "0.0";
            capabilities["javascript"] = "false";
            capabilities["jscriptversion"] = "0.0";
            bool ignoreApplicationBrowsers = true;
            if (!this.DefaultWmlProcess(headers, browserCaps) && !this.DefaultXhtmlmpProcess(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.DefaultDefaultProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void DefaultDefaultProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool DefaultProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            capabilities["activexcontrols"] = "false";
            capabilities["aol"] = "false";
            capabilities["backgroundsounds"] = "false";
            capabilities["beta"] = "false";
            capabilities["browser"] = "Unknown";
            capabilities["canCombineFormsInDeck"] = "true";
            capabilities["canInitiateVoiceCall"] = "false";
            capabilities["canRenderAfterInputOrSelectElement"] = "true";
            capabilities["canRenderEmptySelects"] = "true";
            capabilities["canRenderInputAndSelectElementsTogether"] = "true";
            capabilities["canRenderMixedSelects"] = "true";
            capabilities["canRenderOneventAndPrevElementsTogether"] = "true";
            capabilities["canRenderPostBackCards"] = "true";
            capabilities["canRenderSetvarZeroWithMultiSelectionList"] = "true";
            capabilities["canSendMail"] = "true";
            capabilities["cdf"] = "false";
            capabilities["cookies"] = "true";
            capabilities["crawler"] = "false";
            capabilities["defaultSubmitButtonLimit"] = "1";
            capabilities["ecmascriptversion"] = "0.0";
            capabilities["frames"] = "false";
            capabilities["gatewayMajorVersion"] = "0";
            capabilities["gatewayMinorVersion"] = "0";
            capabilities["gatewayVersion"] = "None";
            capabilities["hasBackButton"] = "true";
            capabilities["hidesRightAlignedMultiselectScrollbars"] = "false";
            capabilities["inputType"] = "telephoneKeypad";
            capabilities["isColor"] = "false";
            capabilities["isMobileDevice"] = "false";
            capabilities["javaapplets"] = "false";
            capabilities["jscriptversion"] = "0.0";
            capabilities["javascript"] = "false";
            capabilities["majorversion"] = "0";
            capabilities["maximumHrefLength"] = "10000";
            capabilities["maximumRenderedPageSize"] = "2000";
            capabilities["maximumSoftkeyLabelLength"] = "5";
            capabilities["minorversion"] = "0";
            capabilities["mobileDeviceManufacturer"] = "Unknown";
            capabilities["mobileDeviceModel"] = "Unknown";
            capabilities["msdomversion"] = "0.0";
            capabilities["numberOfSoftkeys"] = "0";
            capabilities["platform"] = "Unknown";
            capabilities["preferredImageMime"] = "image/gif";
            capabilities["preferredRenderingMime"] = "text/html";
            capabilities["preferredRenderingType"] = "html32";
            capabilities["rendersBreakBeforeWmlSelectAndInput"] = "false";
            capabilities["rendersBreaksAfterHtmlLists"] = "true";
            capabilities["rendersBreaksAfterWmlAnchor"] = "false";
            capabilities["rendersBreaksAfterWmlInput"] = "false";
            capabilities["rendersWmlDoAcceptsInline"] = "true";
            capabilities["rendersWmlSelectsAsMenuCards"] = "false";
            capabilities["requiredMetaTagNameValue"] = "";
            capabilities["requiresAbsolutePostbackUrl"] = "false";
            capabilities["requiresAdaptiveErrorReporting"] = "false";
            capabilities["requiresAttributeColonSubstitution"] = "false";
            capabilities["requiresContentTypeMetaTag"] = "false";
            capabilities["requiresControlStateInSession"] = "false";
            capabilities["requiresDBCSCharacter"] = "false";
            capabilities["requiresFullyQualifiedRedirectUrl"] = "false";
            capabilities["requiresLeadingPageBreak"] = "false";
            capabilities["requiresNoBreakInFormatting"] = "false";
            capabilities["requiresOutputOptimization"] = "false";
            capabilities["requiresPhoneNumbersAsPlainText"] = "false";
            capabilities["requiresPostRedirectionHandling"] = "false";
            capabilities["requiresSpecialViewStateEncoding"] = "false";
            capabilities["requiresUniqueFilePathSuffix"] = "false";
            capabilities["requiresUniqueHtmlCheckboxNames"] = "false";
            capabilities["requiresUniqueHtmlInputNames"] = "false";
            capabilities["requiresUrlEncodedPostfieldValues"] = "false";
            capabilities["requiresXhtmlCssSuppression"] = "false";
            capabilities["screenBitDepth"] = "1";
            capabilities["supportsAccesskeyAttribute"] = "false";
            capabilities["supportsBodyColor"] = "true";
            capabilities["supportsBold"] = "false";
            capabilities["supportsCallback"] = "false";
            capabilities["supportsCacheControlMetaTag"] = "true";
            capabilities["supportsCss"] = "false";
            capabilities["supportsDivAlign"] = "true";
            capabilities["supportsDivNoWrap"] = "false";
            capabilities["supportsEmptyStringInCookieValue"] = "true";
            capabilities["supportsFileUpload"] = "false";
            capabilities["supportsFontColor"] = "true";
            capabilities["supportsFontName"] = "false";
            capabilities["supportsFontSize"] = "false";
            capabilities["supportsImageSubmit"] = "false";
            capabilities["supportsIModeSymbols"] = "false";
            capabilities["supportsInputIStyle"] = "false";
            capabilities["supportsInputMode"] = "false";
            capabilities["supportsItalic"] = "false";
            capabilities["supportsJPhoneMultiMediaAttributes"] = "false";
            capabilities["supportsJPhoneSymbols"] = "false";
            capabilities["SupportsMaintainScrollPositionOnPostback"] = "false";
            capabilities["supportsMultilineTextBoxDisplay"] = "false";
            capabilities["supportsQueryStringInFormAction"] = "true";
            capabilities["supportsRedirectWithCookie"] = "true";
            capabilities["supportsSelectMultiple"] = "true";
            capabilities["supportsUncheck"] = "true";
            capabilities["supportsVCard"] = "false";
            capabilities["tables"] = "false";
            capabilities["tagwriter"] = "System.Web.UI.Html32TextWriter";
            capabilities["type"] = "Unknown";
            capabilities["vbscript"] = "false";
            capabilities["version"] = "0.0";
            capabilities["w3cdomversion"] = "0.0";
            capabilities["win16"] = "false";
            capabilities["win32"] = "false";
            browserCaps.AddBrowser("Default");
            this.DefaultProcessGateways(headers, browserCaps);
            this.CrawlerProcess(headers, browserCaps);
            this.PlatformProcess(headers, browserCaps);
            this.WinProcess(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if ((!this.BlackberryProcess(headers, browserCaps) && !this.OperaProcess(headers, browserCaps)) && (!this.GenericdownlevelProcess(headers, browserCaps) && !this.MozillaProcess(headers, browserCaps)))
            {
                ignoreApplicationBrowsers = false;
            }
            this.DefaultProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void DefaultProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void DefaultProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool DefaultWmlProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = headers["Accept"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"text/vnd\.wap\.wml|text/hdml"))
            {
                return false;
            }
            target = headers["Accept"];
            if (worker.ProcessRegex(target, @"application/xhtml\+xml; profile|application/vnd\.wap\.xhtml\+xml"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["preferredRenderingMime"] = "text/vnd.wap.wml";
            capabilities["preferredRenderingType"] = "wml11";
            bool ignoreApplicationBrowsers = false;
            this.DefaultWmlProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void DefaultWmlProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool DefaultXhtmlmpProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = headers["Accept"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"application/xhtml\+xml; profile|application/vnd\.wap\.xhtml\+xml"))
            {
                return false;
            }
            target = headers["Accept"];
            if (worker.ProcessRegex(target, "text/hdml"))
            {
                return false;
            }
            target = headers["Accept"];
            if (worker.ProcessRegex(target, @"text/vnd\.wap\.wml"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["preferredRenderingMime"] = "text/html";
            capabilities["preferredRenderingType"] = "xhtml-mp";
            browserCaps.HtmlTextWriter = "System.Web.UI.XhtmlTextWriter";
            bool ignoreApplicationBrowsers = false;
            this.DefaultXhtmlmpProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void DefaultXhtmlmpProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Firefox35Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            string target = (string) browserCaps.Capabilities["minorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "^[5-9]"))
            {
                return false;
            }
            browserCaps.AddBrowser("Firefox35");
            this.Firefox35ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Firefox35ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Firefox35ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Firefox35ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Firefox3Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["majorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"[3-9]|[1-9]\d+"))
            {
                return false;
            }
            capabilities["javascriptversion"] = "1.8";
            capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
            browserCaps.AddBrowser("Firefox3");
            this.Firefox3ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Firefox35Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.Firefox3ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Firefox3ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Firefox3ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool FirefoxProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Firefox\/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)"))
            {
                return false;
            }
            worker.ProcessRegex(browserCaps[string.Empty], @"Gecko/(?'layoutVersion'\d+)");
            capabilities["browser"] = "Firefox";
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["version"] = worker["${version}"];
            capabilities["type"] = worker["Firefox${major}"];
            capabilities["layoutEngine"] = "Gecko";
            capabilities["layoutEngineVersion"] = worker["${layoutVersion}"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.5";
            capabilities["w3cdomversion"] = "1.0";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsDivNoWrap"] = "false";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            browserCaps.AddBrowser("Firefox");
            this.FirefoxProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Firefox3Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.FirefoxProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void FirefoxProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void FirefoxProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool GenericdownlevelProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "^Generic Downlevel$"))
            {
                return false;
            }
            capabilities["cookies"] = "false";
            capabilities["ecmascriptversion"] = "1.0";
            capabilities["tables"] = "true";
            capabilities["type"] = "Downlevel";
            browserCaps.Adapters["System.Web.UI.WebControls.Menu, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"] = "System.Web.UI.WebControls.Adapters.MenuAdapter";
            browserCaps.AddBrowser("GenericDownlevel");
            this.GenericdownlevelProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.GenericdownlevelProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void GenericdownlevelProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void GenericdownlevelProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Ie6to9Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["majorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"[6-9]|[1-9]\d+"))
            {
                return false;
            }
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["jscriptversion"] = "5.6";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.5";
            capabilities["msdomversion"] = worker["${majorversion}.${minorversion}"];
            capabilities["w3cdomversion"] = "1.0";
            capabilities["ExchangeOmaSupported"] = "true";
            capabilities["activexcontrols"] = "true";
            capabilities["backgroundsounds"] = "true";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
            capabilities["supportsVCard"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            capabilities["supportsAccessKeyAttribute"] = "true";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["vbscript"] = "true";
            browserCaps.AddBrowser("IE6to9");
            this.Ie6to9ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Ie7Process(headers, browserCaps) && !this.Ie8Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.Ie6to9ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Ie6to9ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Ie6to9ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Ie7Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["majorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "7"))
            {
                return false;
            }
            capabilities["jscriptversion"] = "5.7";
            browserCaps.AddBrowser("IE7");
            this.Ie7ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Ie7ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Ie7ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Ie7ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Ie8Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["majorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "8"))
            {
                return false;
            }
            capabilities["jscriptversion"] = "6.0";
            browserCaps.AddBrowser("IE8");
            this.Ie8ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Ie8ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Ie8ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Ie8ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool IebetaProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["letters"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "^([bB]|ab)"))
            {
                return false;
            }
            capabilities["beta"] = "true";
            this.IebetaProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.IebetaProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void IebetaProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void IebetaProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool IemobileProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"IEMobile.(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)"))
            {
                return false;
            }
            worker.ProcessRegex(browserCaps[string.Empty], @"MSIE (?'msieMajorVersion'\d+)");
            capabilities["layoutEngine"] = "Trident";
            capabilities["browser"] = "IEMobile";
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["type"] = worker["IEMobile${msieMajorVersion}"];
            capabilities["isMobileDevice"] = "true";
            capabilities["version"] = worker["${version}"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["jscriptversion"] = "5.6";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.5";
            capabilities["msdomversion"] = worker["${majorversion}.${minorversion}"];
            capabilities["w3cdomversion"] = "1.0";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsDivNoWrap"] = "false";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            capabilities["vbscript"] = "true";
            capabilities["inputType"] = "virtualKeyboard";
            capabilities["numberOfSoftkeys"] = "2";
            browserCaps.AddBrowser("IEMobile");
            this.IemobileProcessGateways(headers, browserCaps);
            this.MonoProcess(headers, browserCaps);
            this.PixelsProcess(headers, browserCaps);
            this.OsProcess(headers, browserCaps);
            this.CpuProcess(headers, browserCaps);
            this.VoiceProcess(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.IemobileProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void IemobileProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void IemobileProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool IeProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"MSIE (?'version'(?'major'\d+)(\.(?'minor'\d+)?)(?'letters'\w*))(?'extra'[^)]*)"))
            {
                return false;
            }
            target = browserCaps[string.Empty];
            if (worker.ProcessRegex(target, "IEMobile"))
            {
                return false;
            }
            worker.ProcessRegex(browserCaps[string.Empty], @"Trident/(?'layoutVersion'\d+)");
            capabilities["browser"] = "IE";
            capabilities["layoutEngine"] = "Trident";
            capabilities["layoutEngineVersion"] = worker["${layoutVersion}"];
            capabilities["extra"] = worker["${extra}"];
            capabilities["isColor"] = "true";
            capabilities["letters"] = worker["${letters}"];
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["screenBitDepth"] = "8";
            capabilities["type"] = worker["IE${major}"];
            capabilities["version"] = worker["${version}"];
            browserCaps.AddBrowser("IE");
            this.IeProcessGateways(headers, browserCaps);
            this.IebetaProcess(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Ie6to9Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.IeProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void IeProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void IeProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool IphoneProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "iPhone"))
            {
                return false;
            }
            capabilities["mobileDeviceModel"] = "IPhone";
            capabilities["mobileDeviceManufacturer"] = "Apple";
            capabilities["isMobileDevice"] = "true";
            capabilities["canInitiateVoiceCall"] = "true";
            this.IphoneProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.IphoneProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void IphoneProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void IphoneProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool IpodProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "iPod"))
            {
                return false;
            }
            capabilities["mobileDeviceModel"] = "IPod";
            capabilities["mobileDeviceManufacturer"] = "Apple";
            capabilities["isMobileDevice"] = "true";
            this.IpodProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.IpodProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void IpodProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void IpodProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool MonoProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string str = headers["UA-COLOR"];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            str = headers["UA-COLOR"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(str, @"mono(?'colorDepth'\d+)"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["isColor"] = "false";
            capabilities["screenBitDepth"] = worker["${colorDepth}"];
            this.MonoProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.MonoProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void MonoProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void MonoProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool MozillaProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Mozilla"))
            {
                return false;
            }
            capabilities["browser"] = "Mozilla";
            capabilities["cookies"] = "false";
            capabilities["inputType"] = "keyboard";
            capabilities["isColor"] = "true";
            capabilities["isMobileDevice"] = "false";
            capabilities["maximumRenderedPageSize"] = "300000";
            capabilities["screenBitDepth"] = "8";
            capabilities["supportsBold"] = "true";
            capabilities["supportsCss"] = "true";
            capabilities["supportsDivNoWrap"] = "true";
            capabilities["supportsFontName"] = "true";
            capabilities["supportsFontSize"] = "true";
            capabilities["supportsImageSubmit"] = "true";
            capabilities["supportsItalic"] = "true";
            capabilities["type"] = "Mozilla";
            browserCaps.AddBrowser("Mozilla");
            this.MozillaProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (((!this.IeProcess(headers, browserCaps) && !this.ChromeProcess(headers, browserCaps)) && (!this.FirefoxProcess(headers, browserCaps) && !this.IemobileProcess(headers, browserCaps))) && !this.SafariProcess(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.MozillaProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void MozillaProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void MozillaProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Opera10Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Opera/10\.|Version/10\."))
            {
                return false;
            }
            capabilities["version"] = "10.00";
            capabilities["majorversion"] = "10";
            capabilities["minorversion"] = "00";
            capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
            browserCaps.AddBrowser("Opera10");
            this.Opera10ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Opera10ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Opera10ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Opera10ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Opera8to9Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["majorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "[8-9]"))
            {
                return false;
            }
            target = (string) capabilities["Version"];
            if (worker.ProcessRegex(target, "9.80"))
            {
                return false;
            }
            capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
            browserCaps.AddBrowser("Opera8to9");
            this.Opera8to9ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Opera8to9ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Opera8to9ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Opera8to9ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool OperaProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Opera[ /](?'version'(?'major'\d+)(\.(?'minor'\d+)?)(?'letters'\w*))"))
            {
                return false;
            }
            worker.ProcessRegex(browserCaps[string.Empty], @"Presto/(?'layoutVersion'\d+)");
            capabilities["browser"] = "Opera";
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["type"] = worker["Opera${major}"];
            capabilities["version"] = worker["${version}"];
            capabilities["layoutEngine"] = "Presto";
            capabilities["layoutEngineVersion"] = worker["${layoutVersion}"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.5";
            capabilities["letters"] = worker["${letters}"];
            capabilities["w3cdomversion"] = "1.0";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            capabilities["inputType"] = "keyboard";
            capabilities["isColor"] = "true";
            capabilities["isMobileDevice"] = "false";
            capabilities["maximumRenderedPageSize"] = "300000";
            capabilities["screenBitDepth"] = "8";
            capabilities["supportsBold"] = "true";
            capabilities["supportsCss"] = "true";
            capabilities["supportsDivNoWrap"] = "true";
            capabilities["supportsFontName"] = "true";
            capabilities["supportsFontSize"] = "true";
            capabilities["supportsImageSubmit"] = "true";
            capabilities["supportsItalic"] = "true";
            browserCaps.AddBrowser("Opera");
            this.OperaProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Opera8to9Process(headers, browserCaps) && !this.Opera10Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.OperaProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void OperaProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void OperaProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool OsProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = headers["UA-OS"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "(?'os'.+)"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["platform"] = worker["${os}"];
            this.OSProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.OSProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void OSProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void OSProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PixelsProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string str = headers["UA-PIXELS"];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            str = headers["UA-PIXELS"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(str, @"(?'screenWidth'\d+)x(?'screenHeight'\d+)"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["screenPixelsHeight"] = worker["${screenHeight}"];
            capabilities["screenPixelsWidth"] = worker["${screenWidth}"];
            this.PixelsProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.PixelsProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PixelsProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PixelsProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Platformmac68kProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Mac(_68(000|K)|intosh.*68K)"))
            {
                return false;
            }
            capabilities["platform"] = "Mac68K";
            this.Platformmac68kProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Platformmac68kProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Platformmac68kProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Platformmac68kProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformmacppcProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Mac(_PowerPC|intosh.*PPC|_PPC)|PPC Mac"))
            {
                return false;
            }
            capabilities["platform"] = "MacPPC";
            this.PlatformmacppcProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.PlatformmacppcProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformmacppcProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformmacppcProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string str = browserCaps[string.Empty];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            this.PlatformProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if ((((!this.PlatformwinntProcess(headers, browserCaps) && !this.Platformwin2000bProcess(headers, browserCaps)) && (!this.Platformwin95Process(headers, browserCaps) && !this.Platformwin98Process(headers, browserCaps))) && ((!this.Platformwin16Process(headers, browserCaps) && !this.PlatformwinceProcess(headers, browserCaps)) && (!this.Platformmac68kProcess(headers, browserCaps) && !this.PlatformmacppcProcess(headers, browserCaps)))) && (!this.PlatformunixProcess(headers, browserCaps) && !this.PlatformwebtvProcess(headers, browserCaps)))
            {
                ignoreApplicationBrowsers = false;
            }
            this.PlatformProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformunixProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "X11"))
            {
                return false;
            }
            capabilities["platform"] = "UNIX";
            this.PlatformunixProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.PlatformunixProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformunixProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformunixProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformwebtvProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "WebTV"))
            {
                return false;
            }
            capabilities["platform"] = "WebTV";
            this.PlatformwebtvProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.PlatformwebtvProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformwebtvProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformwebtvProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Platformwin16Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Win(dows 3\.1|16)"))
            {
                return false;
            }
            capabilities["platform"] = "Win16";
            this.Platformwin16ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Platformwin16ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Platformwin16ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Platformwin16ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Platformwin2000aProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Windows NT 5\.0"))
            {
                return false;
            }
            capabilities["platform"] = "Win2000";
            this.Platformwin2000aProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Platformwin2000aProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Platformwin2000aProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Platformwin2000aProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Platformwin2000bProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Windows 2000"))
            {
                return false;
            }
            capabilities["platform"] = "Win2000";
            this.Platformwin2000bProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Platformwin2000bProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Platformwin2000bProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Platformwin2000bProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Platformwin95Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Win(dows )?95"))
            {
                return false;
            }
            capabilities["platform"] = "Win95";
            this.Platformwin95ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Platformwin95ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Platformwin95ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Platformwin95ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Platformwin98Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Win(dows )?98"))
            {
                return false;
            }
            capabilities["platform"] = "Win98";
            this.Platformwin98ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Platformwin98ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Platformwin98ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Platformwin98ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformwinceProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Win(dows )?CE"))
            {
                return false;
            }
            capabilities["platform"] = "WinCE";
            this.PlatformwinceProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.PlatformwinceProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformwinceProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformwinceProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformwinntProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Windows NT|WinNT|Windows XP"))
            {
                return false;
            }
            target = browserCaps[string.Empty];
            if (worker.ProcessRegex(target, "WinCE|Windows CE"))
            {
                return false;
            }
            capabilities["platform"] = "WinNT";
            this.PlatformwinntProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.PlatformwinxpProcess(headers, browserCaps) && !this.Platformwin2000aProcess(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.PlatformwinntProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformwinntProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformwinntProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool PlatformwinxpProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Windows (NT 5\.1|XP)"))
            {
                return false;
            }
            capabilities["platform"] = "WinXP";
            this.PlatformwinxpProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.PlatformwinxpProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void PlatformwinxpProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void PlatformwinxpProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected override void PopulateBrowserElements(IDictionary dictionary)
        {
            base.PopulateBrowserElements(dictionary);
            dictionary["Default"] = new Triplet(null, string.Empty, 0);
            dictionary["BlackBerry"] = new Triplet("Default", string.Empty, 1);
            dictionary["Opera"] = new Triplet("Default", string.Empty, 1);
            dictionary["Opera8to9"] = new Triplet("Opera", string.Empty, 2);
            dictionary["Opera10"] = new Triplet("Opera", string.Empty, 2);
            dictionary["GenericDownlevel"] = new Triplet("Default", string.Empty, 1);
            dictionary["Mozilla"] = new Triplet("Default", string.Empty, 1);
            dictionary["IE"] = new Triplet("Mozilla", string.Empty, 2);
            dictionary["IE6to9"] = new Triplet("Ie", string.Empty, 3);
            dictionary["IE7"] = new Triplet("Ie6to9", string.Empty, 4);
            dictionary["IE8"] = new Triplet("Ie6to9", string.Empty, 4);
            dictionary["Chrome"] = new Triplet("Mozilla", string.Empty, 2);
            dictionary["Firefox"] = new Triplet("Mozilla", string.Empty, 2);
            dictionary["Firefox3"] = new Triplet("Firefox", string.Empty, 3);
            dictionary["Firefox35"] = new Triplet("Firefox3", string.Empty, 4);
            dictionary["IEMobile"] = new Triplet("Mozilla", string.Empty, 2);
            dictionary["Safari"] = new Triplet("Mozilla", string.Empty, 2);
            dictionary["Safari3to4"] = new Triplet("Safari", string.Empty, 3);
            dictionary["Safari4"] = new Triplet("Safari3to4", string.Empty, 4);
        }

        protected override void PopulateMatchedHeaders(IDictionary dictionary)
        {
            base.PopulateMatchedHeaders(dictionary);
            dictionary[""] = null;
            dictionary["UA-COLOR"] = null;
            dictionary["UA-PIXELS"] = null;
            dictionary["UA-OS"] = null;
            dictionary["UA-CPU"] = null;
            dictionary["UA-VOICE"] = null;
            dictionary["Accept"] = null;
        }

        private bool Safari3to4Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"Version/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)"))
            {
                return false;
            }
            capabilities["version"] = worker["${version}"];
            capabilities["majorversion"] = worker["${major}"];
            capabilities["minorversion"] = worker["${minor}"];
            capabilities["type"] = worker["Safari${major}"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.6";
            capabilities["w3cdomversion"] = "1.0";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsDivNoWrap"] = "false";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            browserCaps.AddBrowser("Safari3to4");
            this.Safari3to4ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Safari4Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.Safari3to4ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Safari3to4ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Safari3to4ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Safari4Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = (string) capabilities["majorversion"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "4"))
            {
                return false;
            }
            capabilities["javascriptversion"] = "1.7";
            browserCaps.AddBrowser("Safari4");
            this.Safari4ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Safari4ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Safari4ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Safari4ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool SafariProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Safari"))
            {
                return false;
            }
            target = browserCaps[string.Empty];
            if (worker.ProcessRegex(target, "Chrome"))
            {
                return false;
            }
            worker.ProcessRegex(browserCaps[string.Empty], @"AppleWebKit/(?'layoutVersion'\d+)");
            capabilities["layoutEngine"] = "WebKit";
            capabilities["layoutEngineVersion"] = worker["${layoutVersion}"];
            capabilities["browser"] = "Safari";
            capabilities["type"] = "Safari";
            browserCaps.AddBrowser("Safari");
            this.SafariProcessGateways(headers, browserCaps);
            this.IphoneProcess(headers, browserCaps);
            this.IpodProcess(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Safari3to4Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.SafariProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void SafariProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void SafariProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool VoiceProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string str = headers["UA-VOICE"];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            str = headers["UA-VOICE"];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(str, "(?i:TRUE)"))
            {
                return false;
            }
            browserCaps.DisableOptimizedCacheKey();
            capabilities["canInitiateVoiceCall"] = "true";
            this.VoiceProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.VoiceProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void VoiceProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void VoiceProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Win16Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, @"16bit|Win(dows 3\.1|16)"))
            {
                return false;
            }
            capabilities["win16"] = "true";
            this.Win16ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Win16ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Win16ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Win16ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool Win32Process(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string target = browserCaps[string.Empty];
            RegexWorker worker = new RegexWorker(browserCaps);
            if (!worker.ProcessRegex(target, "Win(dows )?(9[58]|NT|32)"))
            {
                return false;
            }
            capabilities["win32"] = "true";
            this.Win32ProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = false;
            this.Win32ProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void Win32ProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void Win32ProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        private bool WinProcess(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
            IDictionary capabilities = browserCaps.Capabilities;
            string str = browserCaps[string.Empty];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            this.WinProcessGateways(headers, browserCaps);
            bool ignoreApplicationBrowsers = true;
            if (!this.Win32Process(headers, browserCaps) && !this.Win16Process(headers, browserCaps))
            {
                ignoreApplicationBrowsers = false;
            }
            this.WinProcessBrowsers(ignoreApplicationBrowsers, headers, browserCaps);
            return true;
        }

        protected virtual void WinProcessBrowsers(bool ignoreApplicationBrowsers, NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        protected virtual void WinProcessGateways(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }
    }
}

