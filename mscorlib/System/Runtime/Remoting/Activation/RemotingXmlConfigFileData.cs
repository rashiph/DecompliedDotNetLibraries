namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Remoting;

    internal class RemotingXmlConfigFileData
    {
        internal string ApplicationName;
        internal ArrayList ChannelEntries = new ArrayList();
        internal CustomErrorsEntry CustomErrors;
        internal ArrayList InteropXmlElementEntries = new ArrayList();
        internal ArrayList InteropXmlTypeEntries = new ArrayList();
        internal LifetimeEntry Lifetime;
        internal static bool LoadTypes;
        internal ArrayList PreLoadEntries = new ArrayList();
        internal ArrayList RemoteAppEntries = new ArrayList();
        internal ArrayList ServerActivatedEntries = new ArrayList();
        internal ArrayList ServerWellKnownEntries = new ArrayList();
        internal bool UrlObjRefMode = RemotingConfigHandler.UrlObjRefMode;

        internal void AddInteropXmlElementEntry(string xmlElementName, string xmlElementNamespace, string urtTypeName, string urtAssemblyName)
        {
            this.TryToLoadTypeIfApplicable(urtTypeName, urtAssemblyName);
            InteropXmlElementEntry entry = new InteropXmlElementEntry(xmlElementName, xmlElementNamespace, urtTypeName, urtAssemblyName);
            this.InteropXmlElementEntries.Add(entry);
        }

        internal void AddInteropXmlTypeEntry(string xmlTypeName, string xmlTypeNamespace, string urtTypeName, string urtAssemblyName)
        {
            this.TryToLoadTypeIfApplicable(urtTypeName, urtAssemblyName);
            InteropXmlTypeEntry entry = new InteropXmlTypeEntry(xmlTypeName, xmlTypeNamespace, urtTypeName, urtAssemblyName);
            this.InteropXmlTypeEntries.Add(entry);
        }

        internal void AddPreLoadEntry(string typeName, string assemblyName)
        {
            this.TryToLoadTypeIfApplicable(typeName, assemblyName);
            PreLoadEntry entry = new PreLoadEntry(typeName, assemblyName);
            this.PreLoadEntries.Add(entry);
        }

        internal RemoteAppEntry AddRemoteAppEntry(string appUri)
        {
            RemoteAppEntry entry = new RemoteAppEntry(appUri);
            this.RemoteAppEntries.Add(entry);
            return entry;
        }

        internal void AddServerActivatedEntry(string typeName, string assemName, ArrayList contextAttributes)
        {
            this.TryToLoadTypeIfApplicable(typeName, assemName);
            TypeEntry entry = new TypeEntry(typeName, assemName, contextAttributes);
            this.ServerActivatedEntries.Add(entry);
        }

        internal ServerWellKnownEntry AddServerWellKnownEntry(string typeName, string assemName, ArrayList contextAttributes, string objURI, WellKnownObjectMode objMode)
        {
            this.TryToLoadTypeIfApplicable(typeName, assemName);
            ServerWellKnownEntry entry = new ServerWellKnownEntry(typeName, assemName, contextAttributes, objURI, objMode);
            this.ServerWellKnownEntries.Add(entry);
            return entry;
        }

        private void TryToLoadTypeIfApplicable(string typeName, string assemblyName)
        {
            if (LoadTypes)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_AssemblyLoadFailed", new object[] { assemblyName }));
                }
                if (assembly.GetType(typeName, false, false) == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_BadType", new object[] { typeName }));
                }
            }
        }

        internal class ChannelEntry
        {
            internal string AssemblyName;
            internal ArrayList ClientSinkProviders = new ArrayList();
            internal bool DelayLoad;
            internal Hashtable Properties;
            internal ArrayList ServerSinkProviders = new ArrayList();
            internal string TypeName;

            internal ChannelEntry(string typeName, string assemblyName, Hashtable properties)
            {
                this.TypeName = typeName;
                this.AssemblyName = assemblyName;
                this.Properties = properties;
            }
        }

        internal class ClientWellKnownEntry
        {
            internal string AssemblyName;
            internal string TypeName;
            internal string Url;

            internal ClientWellKnownEntry(string typeName, string assemName, string url)
            {
                this.TypeName = typeName;
                this.AssemblyName = assemName;
                this.Url = url;
            }
        }

        internal class ContextAttributeEntry
        {
            internal string AssemblyName;
            internal Hashtable Properties;
            internal string TypeName;

            internal ContextAttributeEntry(string typeName, string assemName, Hashtable properties)
            {
                this.TypeName = typeName;
                this.AssemblyName = assemName;
                this.Properties = properties;
            }
        }

        internal class CustomErrorsEntry
        {
            internal CustomErrorsModes Mode;

            internal CustomErrorsEntry(CustomErrorsModes mode)
            {
                this.Mode = mode;
            }
        }

        internal class InteropXmlElementEntry
        {
            internal string UrtAssemblyName;
            internal string UrtTypeName;
            internal string XmlElementName;
            internal string XmlElementNamespace;

            internal InteropXmlElementEntry(string xmlElementName, string xmlElementNamespace, string urtTypeName, string urtAssemblyName)
            {
                this.XmlElementName = xmlElementName;
                this.XmlElementNamespace = xmlElementNamespace;
                this.UrtTypeName = urtTypeName;
                this.UrtAssemblyName = urtAssemblyName;
            }
        }

        internal class InteropXmlTypeEntry
        {
            internal string UrtAssemblyName;
            internal string UrtTypeName;
            internal string XmlTypeName;
            internal string XmlTypeNamespace;

            internal InteropXmlTypeEntry(string xmlTypeName, string xmlTypeNamespace, string urtTypeName, string urtAssemblyName)
            {
                this.XmlTypeName = xmlTypeName;
                this.XmlTypeNamespace = xmlTypeNamespace;
                this.UrtTypeName = urtTypeName;
                this.UrtAssemblyName = urtAssemblyName;
            }
        }

        internal class LifetimeEntry
        {
            private TimeSpan _leaseManagerPollTime;
            private TimeSpan _leaseTime;
            private TimeSpan _renewOnCallTime;
            private TimeSpan _sponsorshipTimeout;
            internal bool IsLeaseManagerPollTimeSet;
            internal bool IsLeaseTimeSet;
            internal bool IsRenewOnCallTimeSet;
            internal bool IsSponsorshipTimeoutSet;

            internal TimeSpan LeaseManagerPollTime
            {
                get
                {
                    return this._leaseManagerPollTime;
                }
                set
                {
                    this._leaseManagerPollTime = value;
                    this.IsLeaseManagerPollTimeSet = true;
                }
            }

            internal TimeSpan LeaseTime
            {
                get
                {
                    return this._leaseTime;
                }
                set
                {
                    this._leaseTime = value;
                    this.IsLeaseTimeSet = true;
                }
            }

            internal TimeSpan RenewOnCallTime
            {
                get
                {
                    return this._renewOnCallTime;
                }
                set
                {
                    this._renewOnCallTime = value;
                    this.IsRenewOnCallTimeSet = true;
                }
            }

            internal TimeSpan SponsorshipTimeout
            {
                get
                {
                    return this._sponsorshipTimeout;
                }
                set
                {
                    this._sponsorshipTimeout = value;
                    this.IsSponsorshipTimeoutSet = true;
                }
            }
        }

        internal class PreLoadEntry
        {
            internal string AssemblyName;
            internal string TypeName;

            public PreLoadEntry(string typeName, string assemblyName)
            {
                this.TypeName = typeName;
                this.AssemblyName = assemblyName;
            }
        }

        internal class RemoteAppEntry
        {
            internal ArrayList ActivatedObjects = new ArrayList();
            internal string AppUri;
            internal ArrayList WellKnownObjects = new ArrayList();

            internal RemoteAppEntry(string appUri)
            {
                this.AppUri = appUri;
            }

            internal void AddActivatedEntry(string typeName, string assemName, ArrayList contextAttributes)
            {
                RemotingXmlConfigFileData.TypeEntry entry = new RemotingXmlConfigFileData.TypeEntry(typeName, assemName, contextAttributes);
                this.ActivatedObjects.Add(entry);
            }

            internal void AddWellKnownEntry(string typeName, string assemName, string url)
            {
                RemotingXmlConfigFileData.ClientWellKnownEntry entry = new RemotingXmlConfigFileData.ClientWellKnownEntry(typeName, assemName, url);
                this.WellKnownObjects.Add(entry);
            }
        }

        internal class ServerWellKnownEntry : RemotingXmlConfigFileData.TypeEntry
        {
            internal WellKnownObjectMode ObjectMode;
            internal string ObjectURI;

            internal ServerWellKnownEntry(string typeName, string assemName, ArrayList contextAttributes, string objURI, WellKnownObjectMode objMode) : base(typeName, assemName, contextAttributes)
            {
                this.ObjectURI = objURI;
                this.ObjectMode = objMode;
            }
        }

        internal class SinkProviderEntry
        {
            internal string AssemblyName;
            internal bool IsFormatter;
            internal Hashtable Properties;
            internal ArrayList ProviderData = new ArrayList();
            internal string TypeName;

            internal SinkProviderEntry(string typeName, string assemName, Hashtable properties, bool isFormatter)
            {
                this.TypeName = typeName;
                this.AssemblyName = assemName;
                this.Properties = properties;
                this.IsFormatter = isFormatter;
            }
        }

        internal class TypeEntry
        {
            internal string AssemblyName;
            internal ArrayList ContextAttributes;
            internal string TypeName;

            internal TypeEntry(string typeName, string assemName, ArrayList contextAttributes)
            {
                this.TypeName = typeName;
                this.AssemblyName = assemName;
                this.ContextAttributes = contextAttributes;
            }
        }
    }
}

