namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel;
    using System.Text;
    using System.Xml.Linq;

    public sealed class CorrelationKey : InstanceKey
    {
        private static readonly XNamespace CorrelationNamespace = XNamespace.Get("urn:microsoft-com:correlation");
        private static readonly ReadOnlyDictionary<string, string> emptyDictionary = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(0));
        private string name;

        private CorrelationKey(string keyString, XNamespace provider) : base(GenerateKey(keyString), dictionary)
        {
            Dictionary<XName, InstanceValue> dictionary = new Dictionary<XName, InstanceValue>(2);
            dictionary.Add(provider.GetName("KeyString"), new InstanceValue(keyString, InstanceValueOptions.Optional));
            dictionary.Add(WorkflowNamespace.KeyProvider, new InstanceValue(provider.NamespaceName, InstanceValueOptions.Optional));
            this.KeyString = keyString;
        }

        public CorrelationKey(IDictionary<string, string> keyData, XName scopeName, XNamespace provider) : this((keyData == null) ? emptyDictionary : ((ReadOnlyDictionary<string, string>) ReadOnlyDictionary<string, string>.Create(keyData)), (scopeName != null) ? scopeName.ToString() : null, provider ?? CorrelationNamespace)
        {
            this.ScopeName = scopeName;
        }

        private CorrelationKey(ReadOnlyDictionary<string, string> keyData, string scopeName, XNamespace provider) : this(GenerateKeyString(keyData, scopeName, provider.NamespaceName), provider)
        {
            this.KeyData = keyData;
            this.Provider = provider;
        }

        private static Guid GenerateKey(string keyString)
        {
            return new Guid(HashHelper.ComputeHash(Encoding.Unicode.GetBytes(keyString)));
        }

        private static string GenerateKeyString(ReadOnlyDictionary<string, string> keyData, string scopeName, string provider)
        {
            if (string.IsNullOrEmpty(scopeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("scopeName", System.ServiceModel.SR.GetString("ScopeNameMustBeSpecified"));
            }
            if (provider.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("provider", System.ServiceModel.SR.GetString("ProviderCannotBeEmptyString"));
            }
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            SortedList<string, string> list = new SortedList<string, string>(keyData, StringComparer.Ordinal);
            builder2.Append(list.Count.ToString(NumberFormatInfo.InvariantInfo));
            builder2.Append('.');
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('&');
                }
                builder.Append(list.Keys[i]);
                builder.Append('=');
                builder.Append(list.Values[i]);
                builder2.Append(list.Keys[i].Length.ToString(NumberFormatInfo.InvariantInfo));
                builder2.Append('.');
                builder2.Append(list.Values[i].Length.ToString(NumberFormatInfo.InvariantInfo));
                builder2.Append('.');
            }
            if (list.Count > 0)
            {
                builder.Append(',');
            }
            builder.Append(scopeName);
            builder.Append(',');
            builder.Append(provider);
            builder2.Append(scopeName.Length.ToString(NumberFormatInfo.InvariantInfo));
            builder2.Append('.');
            builder2.Append(provider.Length.ToString(NumberFormatInfo.InvariantInfo));
            builder.Append('|');
            builder.Append(builder2);
            return builder.ToString();
        }

        public IDictionary<string, string> KeyData { get; private set; }

        public string KeyString { get; private set; }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (!base.IsValid)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotSetNameOnTheInvalidKey")));
                }
                this.name = value;
            }
        }

        public XNamespace Provider { get; private set; }

        public XName ScopeName { get; private set; }
    }
}

