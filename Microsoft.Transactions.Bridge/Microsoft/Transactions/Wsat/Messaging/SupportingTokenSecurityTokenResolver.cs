namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel.Transactions;
    using System.Xml;

    internal class SupportingTokenSecurityTokenResolver : SecurityTokenResolver, ISecurityContextSecurityTokenCache
    {
        [ThreadStatic]
        private static SecurityContextSecurityToken currentSct;

        public void AddContext(SecurityContextSecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void ClearContexts()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        private SecurityContextSecurityToken DeriveToken(Guid transactionId, string tokenId)
        {
            byte[] key = CoordinationServiceSecurity.DeriveIssuedTokenKey(transactionId, tokenId);
            DateTime utcNow = DateTime.UtcNow;
            return new SecurityContextSecurityToken(new UniqueId(tokenId), key, utcNow, utcNow + TimeSpan.FromMinutes(5.0));
        }

        public bool FaultInSupportingToken(Message message)
        {
            DebugTrace.TraceEnter(this, "FaultInSupportingToken");
            bool flag = false;
            WsatRegistrationHeader header = this.ReadRegistrationHeader(message);
            if (((header == null) || (header.TransactionId == Guid.Empty)) || string.IsNullOrEmpty(header.TokenId))
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Invalid or absent RegisterInfo in register message");
                }
            }
            else
            {
                currentSct = this.DeriveToken(header.TransactionId, header.TokenId);
                flag = true;
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Created SCT with id {0} for transaction {1}", header.TokenId, header.TransactionId);
                }
            }
            DebugTrace.TraceLeave(this, "FaultInSupportingToken");
            return flag;
        }

        public Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation)
        {
            DebugTrace.TraceEnter(this, "GetContext");
            SecurityContextSecurityToken currentSct = null;
            if (((SupportingTokenSecurityTokenResolver.currentSct != null) && (SupportingTokenSecurityTokenResolver.currentSct.ContextId == contextId)) && (SupportingTokenSecurityTokenResolver.currentSct.KeyGeneration == generation))
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Found SCT with matching id {0}", contextId);
                }
                currentSct = SupportingTokenSecurityTokenResolver.currentSct;
                SupportingTokenSecurityTokenResolver.currentSct = null;
            }
            DebugTrace.TraceLeave(this, "GetContext");
            return currentSct;
        }

        private WsatRegistrationHeader ReadRegistrationHeader(Message message)
        {
            WsatRegistrationHeader header = null;
            try
            {
                header = WsatRegistrationHeader.ReadFrom(message);
            }
            catch (InvalidEnlistmentHeaderException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                if (DebugTrace.Error)
                {
                    DebugTrace.Trace(TraceLevel.Error, "Invalid RegisterInfo header found in register message: {0}", exception.Message);
                }
            }
            return header;
        }

        public void RemoveAllContexts(UniqueId contextId)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void RemoveContext(UniqueId contextId, UniqueId generation)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public bool TryAddContext(SecurityContextSecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void UpdateContextCachingTime(SecurityContextSecurityToken context, DateTime expirationTime)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }
    }
}

