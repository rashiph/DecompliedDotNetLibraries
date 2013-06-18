namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChannelRequirements
    {
        public bool usesInput;
        public bool usesReply;
        public bool usesOutput;
        public bool usesRequest;
        public SessionMode sessionMode;
        public static void ComputeContractRequirements(ContractDescription contractDescription, out ChannelRequirements requirements)
        {
            requirements = new ChannelRequirements();
            requirements.usesInput = false;
            requirements.usesReply = false;
            requirements.usesOutput = false;
            requirements.usesRequest = false;
            requirements.sessionMode = contractDescription.SessionMode;
            for (int i = 0; i < contractDescription.Operations.Count; i++)
            {
                OperationDescription description = contractDescription.Operations[i];
                bool isOneWay = description.IsOneWay;
                if (!description.IsServerInitiated())
                {
                    if (isOneWay)
                    {
                        requirements.usesInput = true;
                    }
                    else
                    {
                        requirements.usesReply = true;
                    }
                }
                else if (isOneWay)
                {
                    requirements.usesOutput = true;
                }
                else
                {
                    requirements.usesRequest = true;
                }
            }
        }

        public static System.Type[] ComputeRequiredChannels(ref ChannelRequirements requirements)
        {
            if (requirements.usesOutput || requirements.usesRequest)
            {
                switch (requirements.sessionMode)
                {
                    case SessionMode.Allowed:
                        return new System.Type[] { typeof(IDuplexChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.Required:
                        return new System.Type[] { typeof(IDuplexSessionChannel) };

                    case SessionMode.NotAllowed:
                        return new System.Type[] { typeof(IDuplexChannel) };
                }
            }
            else if (requirements.usesInput && requirements.usesReply)
            {
                switch (requirements.sessionMode)
                {
                    case SessionMode.Allowed:
                        return new System.Type[] { typeof(IRequestChannel), typeof(IRequestSessionChannel), typeof(IDuplexChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.Required:
                        return new System.Type[] { typeof(IRequestSessionChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.NotAllowed:
                        return new System.Type[] { typeof(IRequestChannel), typeof(IDuplexChannel) };
                }
            }
            else if (requirements.usesInput)
            {
                switch (requirements.sessionMode)
                {
                    case SessionMode.Allowed:
                        return new System.Type[] { typeof(IOutputChannel), typeof(IOutputSessionChannel), typeof(IRequestChannel), typeof(IRequestSessionChannel), typeof(IDuplexChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.Required:
                        return new System.Type[] { typeof(IOutputSessionChannel), typeof(IRequestSessionChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.NotAllowed:
                        return new System.Type[] { typeof(IOutputChannel), typeof(IRequestChannel), typeof(IDuplexChannel) };
                }
            }
            else if (requirements.usesReply)
            {
                switch (requirements.sessionMode)
                {
                    case SessionMode.Allowed:
                        return new System.Type[] { typeof(IRequestChannel), typeof(IRequestSessionChannel), typeof(IDuplexChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.Required:
                        return new System.Type[] { typeof(IRequestSessionChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.NotAllowed:
                        return new System.Type[] { typeof(IRequestChannel), typeof(IDuplexChannel) };
                }
            }
            else
            {
                switch (requirements.sessionMode)
                {
                    case SessionMode.Allowed:
                        return new System.Type[] { typeof(IOutputSessionChannel), typeof(IOutputChannel), typeof(IRequestSessionChannel), typeof(IRequestChannel), typeof(IDuplexChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.Required:
                        return new System.Type[] { typeof(IOutputSessionChannel), typeof(IRequestSessionChannel), typeof(IDuplexSessionChannel) };

                    case SessionMode.NotAllowed:
                        return new System.Type[] { typeof(IOutputChannel), typeof(IRequestChannel), typeof(IDuplexChannel) };
                }
            }
            return null;
        }

        public static bool IsSessionful(System.Type channelType)
        {
            if ((!(channelType == typeof(IDuplexSessionChannel)) && !(channelType == typeof(IOutputSessionChannel))) && (!(channelType == typeof(IInputSessionChannel)) && !(channelType == typeof(IReplySessionChannel))))
            {
                return (channelType == typeof(IRequestSessionChannel));
            }
            return true;
        }

        public static bool IsOneWay(System.Type channelType)
        {
            if ((!(channelType == typeof(IOutputChannel)) && !(channelType == typeof(IInputChannel))) && !(channelType == typeof(IInputSessionChannel)))
            {
                return (channelType == typeof(IOutputSessionChannel));
            }
            return true;
        }

        public static bool IsRequestReply(System.Type channelType)
        {
            if ((!(channelType == typeof(IRequestChannel)) && !(channelType == typeof(IReplyChannel))) && !(channelType == typeof(IReplySessionChannel)))
            {
                return (channelType == typeof(IRequestSessionChannel));
            }
            return true;
        }

        public static bool IsDuplex(System.Type channelType)
        {
            if (!(channelType == typeof(IDuplexChannel)))
            {
                return (channelType == typeof(IDuplexSessionChannel));
            }
            return true;
        }

        public static Exception CantCreateListenerException(IEnumerable<System.Type> supportedChannels, IEnumerable<System.Type> requiredChannels, string bindingName)
        {
            string contractChannelTypesString = "";
            string bindingChannelTypesString = "";
            Exception exception = BindingContractMismatchException(supportedChannels, requiredChannels, bindingName, ref contractChannelTypesString, ref bindingChannelTypesString);
            if (exception == null)
            {
                exception = new InvalidOperationException(System.ServiceModel.SR.GetString("EndpointListenerRequirementsCannotBeMetBy3", new object[] { bindingName, contractChannelTypesString, bindingChannelTypesString }));
            }
            return exception;
        }

        public static Exception CantCreateChannelException(IEnumerable<System.Type> supportedChannels, IEnumerable<System.Type> requiredChannels, string bindingName)
        {
            string contractChannelTypesString = "";
            string bindingChannelTypesString = "";
            Exception exception = BindingContractMismatchException(supportedChannels, requiredChannels, bindingName, ref contractChannelTypesString, ref bindingChannelTypesString);
            if (exception == null)
            {
                exception = new InvalidOperationException(System.ServiceModel.SR.GetString("CouldnTCreateChannelForType2", new object[] { bindingName, contractChannelTypesString }));
            }
            return exception;
        }

        public static Exception BindingContractMismatchException(IEnumerable<System.Type> supportedChannels, IEnumerable<System.Type> requiredChannels, string bindingName, ref string contractChannelTypesString, ref string bindingChannelTypesString)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            bool flag5 = true;
            bool flag6 = true;
            foreach (System.Type type in requiredChannels)
            {
                if (builder.Length > 0)
                {
                    builder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                    builder.Append(" ");
                }
                string str = type.ToString();
                builder.Append(str.Substring(str.LastIndexOf('.') + 1));
                if (!IsOneWay(type))
                {
                    flag = false;
                }
                if (!IsRequestReply(type))
                {
                    flag2 = false;
                }
                if (!IsDuplex(type))
                {
                    flag3 = false;
                }
                if (!IsRequestReply(type) && !IsDuplex(type))
                {
                    flag4 = false;
                }
                if (!IsSessionful(type))
                {
                    flag5 = false;
                }
                else
                {
                    flag6 = false;
                }
            }
            StringBuilder builder2 = new StringBuilder();
            bool flag7 = false;
            bool flag8 = false;
            bool flag9 = false;
            bool flag10 = false;
            bool flag11 = false;
            bool flag12 = false;
            foreach (System.Type type2 in supportedChannels)
            {
                flag12 = true;
                if (builder2.Length > 0)
                {
                    builder2.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                    builder2.Append(" ");
                }
                string str2 = type2.ToString();
                builder2.Append(str2.Substring(str2.LastIndexOf('.') + 1));
                if (IsOneWay(type2))
                {
                    flag7 = true;
                }
                if (IsRequestReply(type2))
                {
                    flag8 = true;
                }
                if (IsDuplex(type2))
                {
                    flag9 = true;
                }
                if (IsSessionful(type2))
                {
                    flag10 = true;
                }
                else
                {
                    flag11 = true;
                }
            }
            bool flag13 = flag8 || flag9;
            if (!flag12)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportAnyChannelTypes1", new object[] { bindingName }));
            }
            if (flag5 && !flag10)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportSessionButContractRequires1", new object[] { bindingName }));
            }
            if (flag6 && !flag11)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesntSupportDatagramButContractRequires", new object[] { bindingName }));
            }
            if (flag3 && !flag9)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportDuplexButContractRequires1", new object[] { bindingName }));
            }
            if (flag2 && !flag8)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportRequestReplyButContract1", new object[] { bindingName }));
            }
            if (flag && !flag7)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportOneWayButContractRequires1", new object[] { bindingName }));
            }
            if (flag4 && !flag13)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportTwoWayButContractRequires1", new object[] { bindingName }));
            }
            contractChannelTypesString = builder.ToString();
            bindingChannelTypesString = builder2.ToString();
            return null;
        }
    }
}

