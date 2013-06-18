namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Recovery;
    using System;
    using System.IO;
    using System.Runtime;

    internal class ProtocolInformationWriter
    {
        private ProtocolState state;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProtocolInformationWriter(ProtocolState state)
        {
            this.state = state;
        }

        public byte[] GetProtocolInformation()
        {
            MemoryStream mem = new MemoryStream();
            this.WriteProtocolInformation(mem);
            return mem.ToArray();
        }

        private ProtocolInformationFlags GetProtocolInformationFlags()
        {
            ProtocolInformationFlags flags = (ProtocolInformationFlags) 0;
            if (this.state.Config.PortConfiguration.SupportingTokensEnabled)
            {
                flags = (ProtocolInformationFlags) ((byte) (flags | ProtocolInformationFlags.IssuedTokensEnabled));
            }
            if (this.state.TransactionManager.Settings.NetworkInboundAccess)
            {
                flags = (ProtocolInformationFlags) ((byte) (flags | ProtocolInformationFlags.NetworkInboundAccess));
            }
            if (this.state.TransactionManager.Settings.NetworkOutboundAccess)
            {
                flags = (ProtocolInformationFlags) ((byte) (flags | ProtocolInformationFlags.NetworkOutboundAccess));
            }
            if (this.state.TransactionManager.Settings.NetworkClientAccess)
            {
                flags = (ProtocolInformationFlags) ((byte) (flags | ProtocolInformationFlags.NetworkClientAccess));
            }
            if (this.state.TransactionManager.Settings.IsClustered)
            {
                flags = (ProtocolInformationFlags) ((byte) (flags | ProtocolInformationFlags.IsClustered));
            }
            return flags;
        }

        private void WriteProtocolInformation(MemoryStream mem)
        {
            ProtocolInformationFlags protocolInformationFlags = this.GetProtocolInformationFlags();
            mem.WriteByte(1);
            mem.WriteByte(2);
            mem.WriteByte((byte) protocolInformationFlags);
            SerializationUtils.WriteInt(mem, this.state.Config.PortConfiguration.HttpsPort);
            SerializationUtils.WriteTimeout(mem, this.state.Config.MaxTimeout);
            SerializationUtils.WriteString(mem, this.state.Config.PortConfiguration.HostName);
            SerializationUtils.WriteString(mem, this.state.Config.PortConfiguration.BasePath);
            SerializationUtils.WriteString(mem, Environment.MachineName);
            SerializationUtils.WriteUShort(mem, 3);
        }
    }
}

