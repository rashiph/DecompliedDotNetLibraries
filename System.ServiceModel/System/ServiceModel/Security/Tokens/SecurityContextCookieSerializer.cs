namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityContextCookieSerializer
    {
        private const int SupportedPersistanceVersion = 1;
        private SecurityStateEncoder securityStateEncoder;
        private IList<Type> knownTypes;
        public SecurityContextCookieSerializer(SecurityStateEncoder securityStateEncoder, IList<Type> knownTypes)
        {
            if (securityStateEncoder == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityStateEncoder");
            }
            this.securityStateEncoder = securityStateEncoder;
            this.knownTypes = knownTypes ?? new List<Type>();
        }

        private SecurityContextSecurityToken DeserializeContext(byte[] serializedContext, byte[] cookieBlob, string id, XmlDictionaryReaderQuotas quotas)
        {
            List<IAuthorizationPolicy> list3;
            SctClaimDictionary instance = SctClaimDictionary.Instance;
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(serializedContext, 0, serializedContext.Length, instance, quotas, null, null);
            int num = -1;
            UniqueId contextId = null;
            DateTime minUtcDateTime = System.ServiceModel.Security.SecurityUtils.MinUtcDateTime;
            DateTime maxUtcDateTime = System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
            byte[] key = null;
            string str = null;
            UniqueId keyGeneration = null;
            DateTime keyEffectiveTime = System.ServiceModel.Security.SecurityUtils.MinUtcDateTime;
            DateTime keyExpirationTime = System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
            List<ClaimSet> claimSets = null;
            IList<IIdentity> identities = null;
            bool isCookieMode = true;
            reader.ReadFullStartElement(instance.SecurityContextSecurityToken, instance.EmptyString);
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(instance.Version, instance.EmptyString))
                {
                    num = reader.ReadElementContentAsInt();
                }
                else
                {
                    if (reader.IsStartElement(instance.ContextId, instance.EmptyString))
                    {
                        contextId = reader.ReadElementContentAsUniqueId();
                        continue;
                    }
                    if (reader.IsStartElement(instance.Id, instance.EmptyString))
                    {
                        str = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (reader.IsStartElement(instance.EffectiveTime, instance.EmptyString))
                    {
                        minUtcDateTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                        continue;
                    }
                    if (reader.IsStartElement(instance.ExpiryTime, instance.EmptyString))
                    {
                        maxUtcDateTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                        continue;
                    }
                    if (reader.IsStartElement(instance.Key, instance.EmptyString))
                    {
                        key = reader.ReadElementContentAsBase64();
                        continue;
                    }
                    if (reader.IsStartElement(instance.KeyGeneration, instance.EmptyString))
                    {
                        keyGeneration = reader.ReadElementContentAsUniqueId();
                        continue;
                    }
                    if (reader.IsStartElement(instance.KeyEffectiveTime, instance.EmptyString))
                    {
                        keyEffectiveTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                        continue;
                    }
                    if (reader.IsStartElement(instance.KeyExpiryTime, instance.EmptyString))
                    {
                        keyExpirationTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                        continue;
                    }
                    if (reader.IsStartElement(instance.Identities, instance.EmptyString))
                    {
                        identities = SctClaimSerializer.DeserializeIdentities(reader, instance, DataContractSerializerDefaults.CreateSerializer(typeof(IIdentity), this.knownTypes, 0x7fffffff));
                        continue;
                    }
                    if (reader.IsStartElement(instance.ClaimSets, instance.EmptyString))
                    {
                        reader.ReadStartElement();
                        DataContractSerializer serializer = DataContractSerializerDefaults.CreateSerializer(typeof(ClaimSet), this.knownTypes, 0x7fffffff);
                        DataContractSerializer claimSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(Claim), this.knownTypes, 0x7fffffff);
                        claimSets = new List<ClaimSet>(1);
                        while (reader.IsStartElement())
                        {
                            claimSets.Add(SctClaimSerializer.DeserializeClaimSet(reader, instance, serializer, claimSerializer));
                        }
                        reader.ReadEndElement();
                        continue;
                    }
                    if (reader.IsStartElement(instance.IsCookieMode, instance.EmptyString))
                    {
                        isCookieMode = reader.ReadElementString() == "1";
                    }
                    else
                    {
                        OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieXmlParseError"));
                    }
                }
            }
            reader.ReadEndElement();
            if (num != 1)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SerializedTokenVersionUnsupported", new object[] { num })));
            }
            if (contextId == null)
            {
                OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieValueMissingOrIncorrect", new object[] { "ContextId" }));
            }
            if ((key == null) || (key.Length == 0))
            {
                OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieValueMissingOrIncorrect", new object[] { "Key" }));
            }
            if (str != id)
            {
                OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieValueMissingOrIncorrect", new object[] { "Id" }));
            }
            if (claimSets != null)
            {
                list3 = new List<IAuthorizationPolicy>(1) {
                    new SctUnconditionalPolicy(identities, claimSets, maxUtcDateTime)
                };
            }
            else
            {
                list3 = null;
            }
            return new SecurityContextSecurityToken(contextId, str, key, minUtcDateTime, maxUtcDateTime, (list3 != null) ? list3.AsReadOnly() : null, isCookieMode, cookieBlob, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        public byte[] CreateCookieFromSecurityContext(UniqueId contextId, string id, byte[] key, DateTime tokenEffectiveTime, DateTime tokenExpirationTime, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (contextId == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            if (key == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, SctClaimDictionary.Instance, null);
            SctClaimDictionary instance = SctClaimDictionary.Instance;
            writer.WriteStartElement(instance.SecurityContextSecurityToken, instance.EmptyString);
            writer.WriteStartElement(instance.Version, instance.EmptyString);
            writer.WriteValue(1);
            writer.WriteEndElement();
            if (id != null)
            {
                writer.WriteElementString(instance.Id, instance.EmptyString, id);
            }
            XmlHelper.WriteElementStringAsUniqueId(writer, instance.ContextId, instance.EmptyString, contextId);
            writer.WriteStartElement(instance.Key, instance.EmptyString);
            writer.WriteBase64(key, 0, key.Length);
            writer.WriteEndElement();
            if (keyGeneration != null)
            {
                XmlHelper.WriteElementStringAsUniqueId(writer, instance.KeyGeneration, instance.EmptyString, keyGeneration);
            }
            XmlHelper.WriteElementContentAsInt64(writer, instance.EffectiveTime, instance.EmptyString, tokenEffectiveTime.ToUniversalTime().Ticks);
            XmlHelper.WriteElementContentAsInt64(writer, instance.ExpiryTime, instance.EmptyString, tokenExpirationTime.ToUniversalTime().Ticks);
            XmlHelper.WriteElementContentAsInt64(writer, instance.KeyEffectiveTime, instance.EmptyString, keyEffectiveTime.ToUniversalTime().Ticks);
            XmlHelper.WriteElementContentAsInt64(writer, instance.KeyExpiryTime, instance.EmptyString, keyExpirationTime.ToUniversalTime().Ticks);
            AuthorizationContext authContext = null;
            if (authorizationPolicies != null)
            {
                authContext = AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies);
            }
            if ((authContext != null) && (authContext.ClaimSets.Count != 0))
            {
                DataContractSerializer serializer = DataContractSerializerDefaults.CreateSerializer(typeof(IIdentity), this.knownTypes, 0x7fffffff);
                DataContractSerializer serializer2 = DataContractSerializerDefaults.CreateSerializer(typeof(ClaimSet), this.knownTypes, 0x7fffffff);
                DataContractSerializer claimSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(Claim), this.knownTypes, 0x7fffffff);
                SctClaimSerializer.SerializeIdentities(authContext, instance, writer, serializer);
                writer.WriteStartElement(instance.ClaimSets, instance.EmptyString);
                for (int i = 0; i < authContext.ClaimSets.Count; i++)
                {
                    SctClaimSerializer.SerializeClaimSet(authContext.ClaimSets[i], instance, writer, serializer2, claimSerializer);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
            byte[] data = stream.ToArray();
            return this.securityStateEncoder.EncodeSecurityState(data);
        }

        public SecurityContextSecurityToken CreateSecurityContextFromCookie(byte[] encodedCookie, UniqueId contextId, UniqueId generation, string id, XmlDictionaryReaderQuotas quotas)
        {
            byte[] serializedContext = null;
            try
            {
                serializedContext = this.securityStateEncoder.DecodeSecurityState(encodedCookie);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieBlobDecodeFailure"), exception);
            }
            SecurityContextSecurityToken token = this.DeserializeContext(serializedContext, encodedCookie, id, quotas);
            if (token.ContextId != contextId)
            {
                OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieValueMissingOrIncorrect", new object[] { "ContextId" }));
            }
            if (token.KeyGeneration != generation)
            {
                OnInvalidCookieFailure(System.ServiceModel.SR.GetString("SctCookieValueMissingOrIncorrect", new object[] { "KeyGeneration" }));
            }
            return token;
        }

        internal static void OnInvalidCookieFailure(string reason)
        {
            OnInvalidCookieFailure(reason, null);
        }

        internal static void OnInvalidCookieFailure(string reason, Exception e)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidSecurityContextCookie", new object[] { reason }), e));
        }
        private class SctUnconditionalPolicy : IAuthorizationPolicy, IAuthorizationComponent
        {
            private IList<ClaimSet> claimSets;
            private DateTime expirationTime;
            private SecurityUniqueId id = SecurityUniqueId.Create();
            private IList<IIdentity> identities;

            public SctUnconditionalPolicy(IList<IIdentity> identities, IList<ClaimSet> claimSets, DateTime expirationTime)
            {
                this.identities = identities;
                this.claimSets = claimSets;
                this.expirationTime = expirationTime;
            }

            public bool Evaluate(EvaluationContext evaluationContext, ref object state)
            {
                for (int i = 0; i < this.claimSets.Count; i++)
                {
                    evaluationContext.AddClaimSet(this, this.claimSets[i]);
                }
                if (this.identities != null)
                {
                    object obj2;
                    if (!evaluationContext.Properties.TryGetValue("Identities", out obj2))
                    {
                        evaluationContext.Properties.Add("Identities", this.identities);
                    }
                    else
                    {
                        List<IIdentity> list = obj2 as List<IIdentity>;
                        if (list != null)
                        {
                            list.AddRange(this.identities);
                        }
                    }
                }
                evaluationContext.RecordExpirationTime(this.expirationTime);
                return true;
            }

            public string Id
            {
                get
                {
                    return this.id.Value;
                }
            }

            public ClaimSet Issuer
            {
                get
                {
                    return ClaimSet.System;
                }
            }
        }
    }
}

