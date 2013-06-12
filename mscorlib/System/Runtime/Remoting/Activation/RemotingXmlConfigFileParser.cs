namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;

    internal static class RemotingXmlConfigFileParser
    {
        private static Hashtable _channelTemplates = CreateSyncCaseInsensitiveHashtable();
        private static Hashtable _clientChannelSinkTemplates = CreateSyncCaseInsensitiveHashtable();
        private static Hashtable _serverChannelSinkTemplates = CreateSyncCaseInsensitiveHashtable();

        private static bool CheckAssemblyNameForVersionInfo(string assemName)
        {
            if (assemName == null)
            {
                return false;
            }
            return (assemName.IndexOf(',') != -1);
        }

        private static Hashtable CreateCaseInsensitiveHashtable()
        {
            return new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        }

        private static Hashtable CreateSyncCaseInsensitiveHashtable()
        {
            return Hashtable.Synchronized(CreateCaseInsensitiveHashtable());
        }

        public static RemotingXmlConfigFileData ParseConfigFile(string filename)
        {
            ConfigTreeParser parser = new ConfigTreeParser();
            return ParseConfigNode(parser.Parse(filename, "/configuration/system.runtime.remoting"));
        }

        private static RemotingXmlConfigFileData ParseConfigNode(ConfigNode rootNode)
        {
            RemotingXmlConfigFileData configData = new RemotingXmlConfigFileData();
            if (rootNode == null)
            {
                return null;
            }
            foreach (DictionaryEntry entry in rootNode.Attributes)
            {
                string str2 = entry.Key.ToString();
                if (str2 != null)
                {
                    bool flag1 = str2 == "version";
                }
            }
            ConfigNode child = null;
            ConfigNode node2 = null;
            ConfigNode node3 = null;
            ConfigNode node4 = null;
            ConfigNode node5 = null;
            foreach (ConfigNode node6 in rootNode.Children)
            {
                string name = node6.Name;
                if (name != null)
                {
                    if (!(name == "application"))
                    {
                        if (name == "channels")
                        {
                            goto Label_00EB;
                        }
                        if (name == "channelSinkProviders")
                        {
                            goto Label_00FE;
                        }
                        if (name == "debug")
                        {
                            goto Label_0111;
                        }
                        if (name == "customErrors")
                        {
                            goto Label_0124;
                        }
                    }
                    else
                    {
                        if (child != null)
                        {
                            ReportUniqueSectionError(rootNode, child, configData);
                        }
                        child = node6;
                    }
                }
                continue;
            Label_00EB:
                if (node2 != null)
                {
                    ReportUniqueSectionError(rootNode, node2, configData);
                }
                node2 = node6;
                continue;
            Label_00FE:
                if (node3 != null)
                {
                    ReportUniqueSectionError(rootNode, node3, configData);
                }
                node3 = node6;
                continue;
            Label_0111:
                if (node4 != null)
                {
                    ReportUniqueSectionError(rootNode, node4, configData);
                }
                node4 = node6;
                continue;
            Label_0124:
                if (node5 != null)
                {
                    ReportUniqueSectionError(rootNode, node5, configData);
                }
                node5 = node6;
            }
            if (node4 != null)
            {
                ProcessDebugNode(node4, configData);
            }
            if (node3 != null)
            {
                ProcessChannelSinkProviderTemplates(node3, configData);
            }
            if (node2 != null)
            {
                ProcessChannelTemplates(node2, configData);
            }
            if (child != null)
            {
                ProcessApplicationNode(child, configData);
            }
            if (node5 != null)
            {
                ProcessCustomErrorsNode(node5, configData);
            }
            return configData;
        }

        public static RemotingXmlConfigFileData ParseDefaultConfiguration()
        {
            ConfigNode parent = new ConfigNode("system.runtime.remoting", null);
            ConfigNode item = new ConfigNode("application", parent);
            parent.Children.Add(item);
            ConfigNode node4 = new ConfigNode("channels", item);
            item.Children.Add(node4);
            ConfigNode node = new ConfigNode("channel", item) {
                Attributes = { new DictionaryEntry("ref", "http client"), new DictionaryEntry("displayName", "http client (delay loaded)"), new DictionaryEntry("delayLoadAsClientChannel", "true") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", item) {
                Attributes = { new DictionaryEntry("ref", "tcp client"), new DictionaryEntry("displayName", "tcp client (delay loaded)"), new DictionaryEntry("delayLoadAsClientChannel", "true") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", item) {
                Attributes = { new DictionaryEntry("ref", "ipc client"), new DictionaryEntry("displayName", "ipc client (delay loaded)"), new DictionaryEntry("delayLoadAsClientChannel", "true") }
            };
            node4.Children.Add(node);
            node4 = new ConfigNode("channels", parent);
            parent.Children.Add(node4);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "http"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "http client"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpClientChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "http server"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpServerChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "tcp"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "tcp client"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpClientChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "tcp server"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpServerChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "ipc"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "ipc client"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcClientChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            node = new ConfigNode("channel", node4) {
                Attributes = { new DictionaryEntry("id", "ipc server"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcServerChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node4.Children.Add(node);
            ConfigNode node5 = new ConfigNode("channelSinkProviders", parent);
            parent.Children.Add(node5);
            ConfigNode node6 = new ConfigNode("clientProviders", node5);
            node5.Children.Add(node6);
            node = new ConfigNode("formatter", node6) {
                Attributes = { new DictionaryEntry("id", "soap"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.SoapClientFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node6.Children.Add(node);
            node = new ConfigNode("formatter", node6) {
                Attributes = { new DictionaryEntry("id", "binary"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node6.Children.Add(node);
            ConfigNode node7 = new ConfigNode("serverProviders", node5);
            node5.Children.Add(node7);
            node = new ConfigNode("formatter", node7) {
                Attributes = { new DictionaryEntry("id", "soap"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node7.Children.Add(node);
            node = new ConfigNode("formatter", node7) {
                Attributes = { new DictionaryEntry("id", "binary"), new DictionaryEntry("type", "System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node7.Children.Add(node);
            node = new ConfigNode("provider", node7) {
                Attributes = { new DictionaryEntry("id", "wsdl"), new DictionaryEntry("type", "System.Runtime.Remoting.MetadataServices.SdlChannelSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") }
            };
            node7.Children.Add(node);
            return ParseConfigNode(parent);
        }

        private static TimeSpan ParseTime(string time, RemotingXmlConfigFileData configData)
        {
            string str = time;
            string str2 = "s";
            int length = 0;
            char c = ' ';
            if (time.Length > 0)
            {
                c = time[time.Length - 1];
            }
            TimeSpan span = TimeSpan.FromSeconds(0.0);
            try
            {
                if (!char.IsDigit(c))
                {
                    if (time.Length == 0)
                    {
                        ReportInvalidTimeFormatError(str, configData);
                    }
                    time = time.ToLower(CultureInfo.InvariantCulture);
                    length = 1;
                    if (time.EndsWith("ms", StringComparison.Ordinal))
                    {
                        length = 2;
                    }
                    str2 = time.Substring(time.Length - length, length);
                }
                int num2 = int.Parse(time.Substring(0, time.Length - length), CultureInfo.InvariantCulture);
                switch (str2)
                {
                    case "d":
                        return TimeSpan.FromDays((double) num2);

                    case "h":
                        return TimeSpan.FromHours((double) num2);

                    case "m":
                        return TimeSpan.FromMinutes((double) num2);

                    case "s":
                        return TimeSpan.FromSeconds((double) num2);

                    case "ms":
                        return TimeSpan.FromMilliseconds((double) num2);
                }
                ReportInvalidTimeFormatError(str, configData);
            }
            catch (Exception)
            {
                ReportInvalidTimeFormatError(str, configData);
            }
            return span;
        }

        private static void ProcessApplicationNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                if (entry.Key.ToString().Equals("name"))
                {
                    configData.ApplicationName = (string) entry.Value;
                }
            }
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "channels"))
                    {
                        if (name == "client")
                        {
                            goto Label_00D1;
                        }
                        if (name == "lifetime")
                        {
                            goto Label_00DA;
                        }
                        if (name == "service")
                        {
                            goto Label_00E4;
                        }
                        if (name == "soapInterop")
                        {
                            goto Label_00ED;
                        }
                    }
                    else
                    {
                        ProcessChannelsNode(node2, configData);
                    }
                }
                continue;
            Label_00D1:
                ProcessClientNode(node2, configData);
                continue;
            Label_00DA:
                ProcessLifetimeNode(node, node2, configData);
                continue;
            Label_00E4:
                ProcessServiceNode(node2, configData);
                continue;
            Label_00ED:
                ProcessSoapInteropNode(node2, configData);
            }
        }

        private static void ProcessChannelProviderTemplates(ConfigNode node, RemotingXmlConfigFileData configData, bool isServer)
        {
            foreach (ConfigNode node2 in node.Children)
            {
                ProcessSinkProviderNode(node2, configData, true, isServer);
            }
        }

        private static RemotingXmlConfigFileData.ChannelEntry ProcessChannelsChannelNode(ConfigNode node, RemotingXmlConfigFileData configData, bool isTemplate)
        {
            string str = null;
            string typeName = null;
            string assemName = null;
            Hashtable properties = CreateCaseInsensitiveHashtable();
            bool flag = false;
            RemotingXmlConfigFileData.ChannelEntry entry = null;
            foreach (DictionaryEntry entry2 in node.Attributes)
            {
                string key = (string) entry2.Key;
                string str5 = key;
                if (str5 == null)
                {
                    goto Label_019A;
                }
                if (str5 != "displayName")
                {
                    if (!(str5 == "id"))
                    {
                        if (str5 == "ref")
                        {
                            goto Label_00C4;
                        }
                        if (str5 == "type")
                        {
                            goto Label_0169;
                        }
                        if (str5 == "delayLoadAsClientChannel")
                        {
                            goto Label_0180;
                        }
                        goto Label_019A;
                    }
                    if (!isTemplate)
                    {
                        ReportNonTemplateIdAttributeError(node, configData);
                    }
                    else
                    {
                        str = ((string) entry2.Value).ToLower(CultureInfo.InvariantCulture);
                    }
                }
                continue;
            Label_00C4:
                if (isTemplate)
                {
                    ReportTemplateCannotReferenceTemplateError(node, configData);
                }
                else
                {
                    entry = (RemotingXmlConfigFileData.ChannelEntry) _channelTemplates[entry2.Value];
                    if (entry == null)
                    {
                        ReportUnableToResolveTemplateReferenceError(node, entry2.Value.ToString(), configData);
                    }
                    else
                    {
                        typeName = entry.TypeName;
                        assemName = entry.AssemblyName;
                        foreach (DictionaryEntry entry3 in entry.Properties)
                        {
                            properties[entry3.Key] = entry3.Value;
                        }
                    }
                }
                continue;
            Label_0169:
                RemotingConfigHandler.ParseType((string) entry2.Value, out typeName, out assemName);
                continue;
            Label_0180:
                flag = Convert.ToBoolean((string) entry2.Value, CultureInfo.InvariantCulture);
                continue;
            Label_019A:
                properties[key] = entry2.Value;
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            RemotingXmlConfigFileData.ChannelEntry channelEntry = new RemotingXmlConfigFileData.ChannelEntry(typeName, assemName, properties) {
                DelayLoad = flag
            };
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "clientProviders"))
                    {
                        if (name == "serverProviders")
                        {
                            goto Label_0239;
                        }
                    }
                    else
                    {
                        ProcessSinkProviderNodes(node2, channelEntry, configData, false);
                    }
                }
                continue;
            Label_0239:
                ProcessSinkProviderNodes(node2, channelEntry, configData, true);
            }
            if (entry != null)
            {
                if (channelEntry.ClientSinkProviders.Count == 0)
                {
                    channelEntry.ClientSinkProviders = entry.ClientSinkProviders;
                }
                if (channelEntry.ServerSinkProviders.Count == 0)
                {
                    channelEntry.ServerSinkProviders = entry.ServerSinkProviders;
                }
            }
            if (isTemplate)
            {
                _channelTemplates[str] = channelEntry;
                return null;
            }
            return channelEntry;
        }

        private static void ProcessChannelSinkProviderTemplates(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "clientProviders"))
                    {
                        if (name == "serverProviders")
                        {
                            goto Label_0046;
                        }
                    }
                    else
                    {
                        ProcessChannelProviderTemplates(node2, configData, false);
                    }
                }
                continue;
            Label_0046:
                ProcessChannelProviderTemplates(node2, configData, true);
            }
        }

        private static void ProcessChannelsNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (ConfigNode node2 in node.Children)
            {
                if (node2.Name.Equals("channel"))
                {
                    RemotingXmlConfigFileData.ChannelEntry entry = ProcessChannelsChannelNode(node2, configData, false);
                    configData.ChannelEntries.Add(entry);
                }
            }
        }

        private static void ProcessChannelTemplates(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (ConfigNode node2 in node.Children)
            {
                string str;
                if (((str = node2.Name) != null) && (str == "channel"))
                {
                    ProcessChannelsChannelNode(node2, configData, true);
                }
            }
        }

        private static void ProcessClientActivatedNode(ConfigNode node, RemotingXmlConfigFileData configData, RemotingXmlConfigFileData.RemoteAppEntry remoteApp)
        {
            string typeName = null;
            string assemName = null;
            ArrayList contextAttributes = new ArrayList();
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str4;
                if (((str4 = entry.Key.ToString()) != null) && (str4 == "type"))
                {
                    RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                }
            }
            foreach (ConfigNode node2 in node.Children)
            {
                string str5;
                if (((str5 = node2.Name) != null) && (str5 == "contextAttribute"))
                {
                    contextAttributes.Add(ProcessContextAttributeNode(node2, configData));
                }
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            remoteApp.AddActivatedEntry(typeName, assemName, contextAttributes);
        }

        private static void ProcessClientNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string appUri = null;
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str3 = entry.Key.ToString();
                if (str3 != null)
                {
                    if (!(str3 == "url"))
                    {
                        if (str3 == "displayName")
                        {
                        }
                    }
                    else
                    {
                        appUri = (string) entry.Value;
                    }
                }
            }
            RemotingXmlConfigFileData.RemoteAppEntry remoteApp = configData.AddRemoteAppEntry(appUri);
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "wellknown"))
                    {
                        if (name == "activated")
                        {
                            goto Label_00C5;
                        }
                    }
                    else
                    {
                        ProcessClientWellKnownNode(node2, configData, remoteApp);
                    }
                }
                continue;
            Label_00C5:
                ProcessClientActivatedNode(node2, configData, remoteApp);
            }
            if ((remoteApp.ActivatedObjects.Count > 0) && (appUri == null))
            {
                ReportMissingAttributeError(node, "url", configData);
            }
        }

        private static void ProcessClientWellKnownNode(ConfigNode node, RemotingXmlConfigFileData configData, RemotingXmlConfigFileData.RemoteAppEntry remoteApp)
        {
            string typeName = null;
            string assemName = null;
            string url = null;
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str5;
                if (((str5 = entry.Key.ToString()) != null) && (str5 != "displayName"))
                {
                    if (!(str5 == "type"))
                    {
                        if (str5 == "url")
                        {
                            goto Label_0075;
                        }
                    }
                    else
                    {
                        RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                    }
                }
                continue;
            Label_0075:
                url = (string) entry.Value;
            }
            if (url == null)
            {
                ReportMissingAttributeError("WellKnown client", "url", configData);
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            if (CheckAssemblyNameForVersionInfo(assemName))
            {
                ReportAssemblyVersionInfoPresent(assemName, "client wellknown", configData);
            }
            remoteApp.AddWellKnownEntry(typeName, assemName, url);
        }

        private static RemotingXmlConfigFileData.ContextAttributeEntry ProcessContextAttributeNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string typeName = null;
            string assemName = null;
            Hashtable properties = CreateCaseInsensitiveHashtable();
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str4;
                string str3 = ((string) entry.Key).ToLower(CultureInfo.InvariantCulture);
                if (((str4 = str3) != null) && (str4 == "type"))
                {
                    RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                }
                else
                {
                    properties[str3] = entry.Value;
                }
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            return new RemotingXmlConfigFileData.ContextAttributeEntry(typeName, assemName, properties);
        }

        private static void ProcessCustomErrorsNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                if (entry.Key.ToString().Equals("mode"))
                {
                    string strA = (string) entry.Value;
                    CustomErrorsModes on = CustomErrorsModes.On;
                    if (string.Compare(strA, "on", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        on = CustomErrorsModes.On;
                    }
                    else if (string.Compare(strA, "off", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        on = CustomErrorsModes.Off;
                    }
                    else if (string.Compare(strA, "remoteonly", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        on = CustomErrorsModes.RemoteOnly;
                    }
                    else
                    {
                        ReportUnknownValueError(node, strA, configData);
                    }
                    configData.CustomErrors = new RemotingXmlConfigFileData.CustomErrorsEntry(on);
                }
            }
        }

        private static void ProcessDebugNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str2;
                if (((str2 = entry.Key.ToString()) != null) && (str2 == "loadTypes"))
                {
                    RemotingXmlConfigFileData.LoadTypes = Convert.ToBoolean((string) entry.Value, CultureInfo.InvariantCulture);
                }
            }
        }

        private static void ProcessInteropXmlElementNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string typeName = null;
            string assemName = null;
            string str3 = null;
            string str4 = null;
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str6 = entry.Key.ToString();
                if (str6 != null)
                {
                    if (!(str6 == "xml"))
                    {
                        if (str6 == "clr")
                        {
                            goto Label_006A;
                        }
                    }
                    else
                    {
                        RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                    }
                }
                continue;
            Label_006A:
                RemotingConfigHandler.ParseType((string) entry.Value, out str3, out str4);
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingXmlTypeAttributeError(node, "xml", configData);
            }
            if ((str3 == null) || (str4 == null))
            {
                ReportMissingTypeAttributeError(node, "clr", configData);
            }
            configData.AddInteropXmlElementEntry(typeName, assemName, str3, str4);
        }

        private static void ProcessInteropXmlTypeNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string typeName = null;
            string assemName = null;
            string str3 = null;
            string str4 = null;
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str6 = entry.Key.ToString();
                if (str6 != null)
                {
                    if (!(str6 == "xml"))
                    {
                        if (str6 == "clr")
                        {
                            goto Label_006A;
                        }
                    }
                    else
                    {
                        RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                    }
                }
                continue;
            Label_006A:
                RemotingConfigHandler.ParseType((string) entry.Value, out str3, out str4);
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingXmlTypeAttributeError(node, "xml", configData);
            }
            if ((str3 == null) || (str4 == null))
            {
                ReportMissingTypeAttributeError(node, "clr", configData);
            }
            configData.AddInteropXmlTypeEntry(typeName, assemName, str3, str4);
        }

        private static void ProcessLifetimeNode(ConfigNode parentNode, ConfigNode node, RemotingXmlConfigFileData configData)
        {
            if (configData.Lifetime != null)
            {
                ReportUniqueSectionError(node, parentNode, configData);
            }
            configData.Lifetime = new RemotingXmlConfigFileData.LifetimeEntry();
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str2 = entry.Key.ToString();
                if (str2 != null)
                {
                    if (!(str2 == "leaseTime"))
                    {
                        if (str2 == "sponsorshipTimeout")
                        {
                            goto Label_009E;
                        }
                        if (str2 == "renewOnCallTime")
                        {
                            goto Label_00BD;
                        }
                        if (str2 == "leaseManagerPollTime")
                        {
                            goto Label_00DC;
                        }
                    }
                    else
                    {
                        configData.Lifetime.LeaseTime = ParseTime((string) entry.Value, configData);
                    }
                }
                continue;
            Label_009E:
                configData.Lifetime.SponsorshipTimeout = ParseTime((string) entry.Value, configData);
                continue;
            Label_00BD:
                configData.Lifetime.RenewOnCallTime = ParseTime((string) entry.Value, configData);
                continue;
            Label_00DC:
                configData.Lifetime.LeaseManagerPollTime = ParseTime((string) entry.Value, configData);
            }
        }

        private static void ProcessPreLoadNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string typeName = null;
            string assemName = null;
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str4 = entry.Key.ToString();
                if (str4 != null)
                {
                    if (!(str4 == "type"))
                    {
                        if (str4 == "assembly")
                        {
                            goto Label_0063;
                        }
                    }
                    else
                    {
                        RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                    }
                }
                continue;
            Label_0063:
                assemName = (string) entry.Value;
            }
            if (assemName == null)
            {
                ReportError(Environment.GetResourceString("Remoting_Config_PreloadRequiresTypeOrAssembly"), configData);
            }
            configData.AddPreLoadEntry(typeName, assemName);
        }

        private static void ProcessServiceActivatedNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string typeName = null;
            string assemName = null;
            ArrayList contextAttributes = new ArrayList();
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str4;
                if (((str4 = entry.Key.ToString()) != null) && (str4 == "type"))
                {
                    RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
                }
            }
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "contextAttribute"))
                    {
                        if (name == "lifetime")
                        {
                        }
                    }
                    else
                    {
                        contextAttributes.Add(ProcessContextAttributeNode(node2, configData));
                    }
                }
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            if (CheckAssemblyNameForVersionInfo(assemName))
            {
                ReportAssemblyVersionInfoPresent(assemName, "service activated", configData);
            }
            configData.AddServerActivatedEntry(typeName, assemName, contextAttributes);
        }

        private static void ProcessServiceNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "wellknown"))
                    {
                        if (name == "activated")
                        {
                            goto Label_0045;
                        }
                    }
                    else
                    {
                        ProcessServiceWellKnownNode(node2, configData);
                    }
                }
                continue;
            Label_0045:
                ProcessServiceActivatedNode(node2, configData);
            }
        }

        private static void ProcessServiceWellKnownNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            string typeName = null;
            string assemName = null;
            ArrayList contextAttributes = new ArrayList();
            string objURI = null;
            WellKnownObjectMode singleton = WellKnownObjectMode.Singleton;
            bool flag = false;
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str6;
                if (((str6 = entry.Key.ToString()) != null) && (str6 != "displayName"))
                {
                    if (!(str6 == "mode"))
                    {
                        if (str6 == "objectUri")
                        {
                            goto Label_00BE;
                        }
                        if (str6 == "type")
                        {
                            goto Label_00CD;
                        }
                    }
                    else
                    {
                        string strA = (string) entry.Value;
                        flag = true;
                        if (string.CompareOrdinal(strA, "Singleton") == 0)
                        {
                            singleton = WellKnownObjectMode.Singleton;
                        }
                        else if (string.CompareOrdinal(strA, "SingleCall") == 0)
                        {
                            singleton = WellKnownObjectMode.SingleCall;
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                }
                continue;
            Label_00BE:
                objURI = (string) entry.Value;
                continue;
            Label_00CD:
                RemotingConfigHandler.ParseType((string) entry.Value, out typeName, out assemName);
            }
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "contextAttribute"))
                    {
                        if (name == "lifetime")
                        {
                        }
                    }
                    else
                    {
                        contextAttributes.Add(ProcessContextAttributeNode(node2, configData));
                    }
                }
            }
            if (!flag)
            {
                ReportError(Environment.GetResourceString("Remoting_Config_MissingWellKnownModeAttribute"), configData);
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            if (objURI == null)
            {
                objURI = typeName + ".soap";
            }
            configData.AddServerWellKnownEntry(typeName, assemName, contextAttributes, objURI, singleton);
        }

        private static SinkProviderData ProcessSinkProviderData(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            SinkProviderData data = new SinkProviderData(node.Name);
            foreach (ConfigNode node2 in node.Children)
            {
                SinkProviderData data2 = ProcessSinkProviderData(node2, configData);
                data.Children.Add(data2);
            }
            foreach (DictionaryEntry entry in node.Attributes)
            {
                data.Properties[entry.Key] = entry.Value;
            }
            return data;
        }

        private static RemotingXmlConfigFileData.SinkProviderEntry ProcessSinkProviderNode(ConfigNode node, RemotingXmlConfigFileData configData, bool isTemplate, bool isServer)
        {
            bool isFormatter = false;
            string name = node.Name;
            if (name.Equals("formatter"))
            {
                isFormatter = true;
            }
            else if (name.Equals("provider"))
            {
                isFormatter = false;
            }
            else
            {
                ReportError(Environment.GetResourceString("Remoting_Config_ProviderNeedsElementName"), configData);
            }
            string str2 = null;
            string typeName = null;
            string assemName = null;
            Hashtable properties = CreateCaseInsensitiveHashtable();
            RemotingXmlConfigFileData.SinkProviderEntry entry = null;
            foreach (DictionaryEntry entry2 in node.Attributes)
            {
                string key = (string) entry2.Key;
                string str6 = key;
                if (str6 == null)
                {
                    goto Label_01AD;
                }
                if (!(str6 == "id"))
                {
                    if (str6 == "ref")
                    {
                        goto Label_00D2;
                    }
                    if (str6 == "type")
                    {
                        goto Label_0196;
                    }
                    goto Label_01AD;
                }
                if (!isTemplate)
                {
                    ReportNonTemplateIdAttributeError(node, configData);
                }
                else
                {
                    str2 = (string) entry2.Value;
                }
                continue;
            Label_00D2:
                if (isTemplate)
                {
                    ReportTemplateCannotReferenceTemplateError(node, configData);
                }
                else
                {
                    if (isServer)
                    {
                        entry = (RemotingXmlConfigFileData.SinkProviderEntry) _serverChannelSinkTemplates[entry2.Value];
                    }
                    else
                    {
                        entry = (RemotingXmlConfigFileData.SinkProviderEntry) _clientChannelSinkTemplates[entry2.Value];
                    }
                    if (entry == null)
                    {
                        ReportUnableToResolveTemplateReferenceError(node, entry2.Value.ToString(), configData);
                    }
                    else
                    {
                        typeName = entry.TypeName;
                        assemName = entry.AssemblyName;
                        foreach (DictionaryEntry entry3 in entry.Properties)
                        {
                            properties[entry3.Key] = entry3.Value;
                        }
                    }
                }
                continue;
            Label_0196:
                RemotingConfigHandler.ParseType((string) entry2.Value, out typeName, out assemName);
                continue;
            Label_01AD:
                properties[key] = entry2.Value;
            }
            if ((typeName == null) || (assemName == null))
            {
                ReportMissingTypeAttributeError(node, "type", configData);
            }
            RemotingXmlConfigFileData.SinkProviderEntry entry4 = new RemotingXmlConfigFileData.SinkProviderEntry(typeName, assemName, properties, isFormatter);
            foreach (ConfigNode node2 in node.Children)
            {
                SinkProviderData data = ProcessSinkProviderData(node2, configData);
                entry4.ProviderData.Add(data);
            }
            if ((entry != null) && (entry4.ProviderData.Count == 0))
            {
                entry4.ProviderData = entry.ProviderData;
            }
            if (!isTemplate)
            {
                return entry4;
            }
            if (isServer)
            {
                _serverChannelSinkTemplates[str2] = entry4;
            }
            else
            {
                _clientChannelSinkTemplates[str2] = entry4;
            }
            return null;
        }

        private static void ProcessSinkProviderNodes(ConfigNode node, RemotingXmlConfigFileData.ChannelEntry channelEntry, RemotingXmlConfigFileData configData, bool isServer)
        {
            foreach (ConfigNode node2 in node.Children)
            {
                RemotingXmlConfigFileData.SinkProviderEntry entry = ProcessSinkProviderNode(node2, configData, false, isServer);
                if (isServer)
                {
                    channelEntry.ServerSinkProviders.Add(entry);
                }
                else
                {
                    channelEntry.ClientSinkProviders.Add(entry);
                }
            }
        }

        private static void ProcessSoapInteropNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                string str2;
                if (((str2 = entry.Key.ToString()) != null) && (str2 == "urlObjRef"))
                {
                    configData.UrlObjRefMode = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                }
            }
            foreach (ConfigNode node2 in node.Children)
            {
                string name = node2.Name;
                if (name != null)
                {
                    if (!(name == "preLoad"))
                    {
                        if (name == "interopXmlElement")
                        {
                            goto Label_00BE;
                        }
                        if (name == "interopXmlType")
                        {
                            goto Label_00C7;
                        }
                    }
                    else
                    {
                        ProcessPreLoadNode(node2, configData);
                    }
                }
                continue;
            Label_00BE:
                ProcessInteropXmlElementNode(node2, configData);
                continue;
            Label_00C7:
                ProcessInteropXmlTypeNode(node2, configData);
            }
        }

        private static void ReportAssemblyVersionInfoPresent(string assemName, string entryDescription, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_VersionPresent"), new object[] { assemName, entryDescription }), configData);
        }

        private static void ReportError(string errorStr, RemotingXmlConfigFileData configData)
        {
            throw new RemotingException(errorStr);
        }

        private static void ReportInvalidTimeFormatError(string time, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidTimeFormat"), new object[] { time }), configData);
        }

        private static void ReportMissingAttributeError(ConfigNode node, string attributeName, RemotingXmlConfigFileData configData)
        {
            ReportMissingAttributeError(node.Name, attributeName, configData);
        }

        private static void ReportMissingAttributeError(string nodeDescription, string attributeName, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_RequiredXmlAttribute"), new object[] { nodeDescription, attributeName }), configData);
        }

        private static void ReportMissingTypeAttributeError(ConfigNode node, string attributeName, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_MissingTypeAttribute"), new object[] { node.Name, attributeName }), configData);
        }

        private static void ReportMissingXmlTypeAttributeError(ConfigNode node, string attributeName, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_MissingXmlTypeAttribute"), new object[] { node.Name, attributeName }), configData);
        }

        private static void ReportNonTemplateIdAttributeError(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_NonTemplateIdAttribute"), new object[] { node.Name }), configData);
        }

        private static void ReportTemplateCannotReferenceTemplateError(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_TemplateCannotReferenceTemplate"), new object[] { node.Name }), configData);
        }

        private static void ReportUnableToResolveTemplateReferenceError(ConfigNode node, string referenceName, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_UnableToResolveTemplate"), new object[] { node.Name, referenceName }), configData);
        }

        private static void ReportUniqueSectionError(ConfigNode parent, ConfigNode child, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_NodeMustBeUnique"), new object[] { child.Name, parent.Name }), configData);
        }

        private static void ReportUnknownValueError(ConfigNode node, string value, RemotingXmlConfigFileData configData)
        {
            ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_UnknownValue"), new object[] { node.Name, value }), configData);
        }
    }
}

