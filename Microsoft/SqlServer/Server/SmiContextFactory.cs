namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Security.Permissions;

    internal sealed class SmiContextFactory
    {
        private readonly ulong[] __supportedSmiVersions = new ulong[] { 100L, 210L };
        private readonly short _buildNum;
        private readonly SmiEventSink_Default _eventSinkForGetCurrentContext;
        private readonly byte _majorVersion;
        private readonly byte _minorVersion;
        private readonly ulong _negotiatedSmiVersion;
        private readonly string _serverVersion;
        private readonly SmiLink _smiLink;
        public static readonly SmiContextFactory Instance = new SmiContextFactory();
        internal const ulong KatmaiVersion = 210L;
        internal const ulong LatestVersion = 210L;
        internal const ulong YukonVersion = 100L;

        private SmiContextFactory()
        {
            if (InOutOfProcHelper.InProc)
            {
                Type aType = Type.GetType("Microsoft.SqlServer.Server.InProcLink, SqlAccess, PublicKeyToken=89845dcd8080cc91");
                if (null == aType)
                {
                    throw SQL.ContextUnavailableOutOfProc();
                }
                FieldInfo staticField = this.GetStaticField(aType, "Instance");
                if (staticField == null)
                {
                    throw SQL.ContextUnavailableOutOfProc();
                }
                this._smiLink = (SmiLink) this.GetValue(staticField);
                FieldInfo fieldInfo = this.GetStaticField(aType, "BuildVersion");
                if (fieldInfo != null)
                {
                    uint num2 = (uint) this.GetValue(fieldInfo);
                    this._majorVersion = (byte) (num2 >> 0x18);
                    this._minorVersion = (byte) ((num2 >> 0x10) & 0xff);
                    this._buildNum = (short) (num2 & 0xffff);
                    this._serverVersion = string.Format(null, "{0:00}.{1:00}.{2:0000}", new object[] { this._majorVersion, (short) this._minorVersion, this._buildNum });
                }
                else
                {
                    this._serverVersion = string.Empty;
                }
                this._negotiatedSmiVersion = this._smiLink.NegotiateVersion(210L);
                bool flag = false;
                for (int i = 0; !flag && (i < this.__supportedSmiVersions.Length); i++)
                {
                    if (this.__supportedSmiVersions[i] == this._negotiatedSmiVersion)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    this._smiLink = null;
                }
                this._eventSinkForGetCurrentContext = new SmiEventSink_Default();
            }
        }

        internal SmiContext GetCurrentContext()
        {
            if (this._smiLink == null)
            {
                throw SQL.ContextUnavailableOutOfProc();
            }
            object currentContext = this._smiLink.GetCurrentContext(this._eventSinkForGetCurrentContext);
            this._eventSinkForGetCurrentContext.ProcessMessagesAndThrow();
            if (currentContext == null)
            {
                throw SQL.ContextUnavailableWhileInProc();
            }
            return (SmiContext) currentContext;
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private FieldInfo GetStaticField(Type aType, string fieldName)
        {
            return aType.GetField(fieldName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Static);
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private object GetValue(FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(null);
        }

        internal ulong NegotiatedSmiVersion
        {
            get
            {
                if (this._smiLink == null)
                {
                    throw SQL.ContextUnavailableOutOfProc();
                }
                return this._negotiatedSmiVersion;
            }
        }

        internal string ServerVersion
        {
            get
            {
                if (this._smiLink == null)
                {
                    throw SQL.ContextUnavailableOutOfProc();
                }
                return this._serverVersion;
            }
        }
    }
}

