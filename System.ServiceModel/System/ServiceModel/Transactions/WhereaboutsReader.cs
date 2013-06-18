namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.Text;

    internal class WhereaboutsReader
    {
        private static Guid GuidWhereaboutsInfo = new Guid("{2adb4462-bd41-11d0-b12e-00c04fc2f3ef}");
        private string hostName;
        private ProtocolInformationReader protocolInfo;
        private const long STmToTmProtocolSize = 8L;

        public WhereaboutsReader(byte[] whereabouts)
        {
            MemoryStream mem = new MemoryStream(whereabouts, 0, whereabouts.Length, false, true);
            this.DeserializeWhereabouts(mem);
        }

        private void DeserializeWhereabouts(MemoryStream mem)
        {
            if (SerializationUtils.ReadGuid(mem) != GuidWhereaboutsInfo)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsSignatureMissing")));
            }
            uint num = SerializationUtils.ReadUInt(mem);
            if ((num * 8L) > (mem.Length - mem.Position))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsImplausibleProtocolCount")));
            }
            for (uint i = 0; i < num; i++)
            {
                this.DeserializeWhereaboutsProtocol(mem);
            }
            if (string.IsNullOrEmpty(this.hostName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsNoHostName")));
            }
        }

        private void DeserializeWhereaboutsProtocol(MemoryStream mem)
        {
            TmProtocol protocol = (TmProtocol) SerializationUtils.ReadInt(mem);
            uint cbTmProtocolData = SerializationUtils.ReadUInt(mem);
            switch (protocol)
            {
                case TmProtocol.TmProtocolMsdtcV2:
                    this.ReadMsdtcV2Protocol(mem, cbTmProtocolData);
                    break;

                case TmProtocol.TmProtocolExtended:
                    this.ReadExtendedProtocol(mem, cbTmProtocolData);
                    break;

                default:
                    SerializationUtils.IncrementPosition(mem, (long) cbTmProtocolData);
                    break;
            }
            SerializationUtils.AlignPosition(mem, 4);
        }

        private void ReadExtendedProtocol(MemoryStream mem, uint cbTmProtocolData)
        {
            Guid guid = SerializationUtils.ReadGuid(mem);
            if ((guid == PluggableProtocol10.ProtocolGuid) || (guid == PluggableProtocol11.ProtocolGuid))
            {
                this.protocolInfo = new ProtocolInformationReader(mem);
            }
            else
            {
                SerializationUtils.IncrementPosition(mem, (long) (cbTmProtocolData - 0x10));
            }
        }

        private void ReadMsdtcV2Protocol(MemoryStream mem, uint cbTmProtocolData)
        {
            if (cbTmProtocolData > 0x20)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsImplausibleHostNameByteCount")));
            }
            byte[] bytes = SerializationUtils.ReadBytes(mem, (int) cbTmProtocolData);
            int index = 0;
            while ((index < (cbTmProtocolData - 1)) && ((bytes[index] != 0) || (bytes[index + 1] != 0)))
            {
                index += 2;
            }
            if (index == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsInvalidHostName")));
            }
            try
            {
                this.hostName = Encoding.Unicode.GetString(bytes, 0, index);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("WhereaboutsInvalidHostName"), exception));
            }
        }

        public string HostName
        {
            get
            {
                return this.hostName;
            }
        }

        public ProtocolInformationReader ProtocolInformation
        {
            get
            {
                return this.protocolInfo;
            }
        }

        private enum TmProtocol
        {
            TmProtocolNone,
            TmProtocolTip,
            TmProtocolMsdtcV1,
            TmProtocolMsdtcV2,
            TmProtocolExtended
        }
    }
}

