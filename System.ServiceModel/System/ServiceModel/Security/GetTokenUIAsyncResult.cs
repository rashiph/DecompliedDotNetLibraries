namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class GetTokenUIAsyncResult : AsyncResult
    {
        private Binding binding;
        private static AsyncCallback callback = Fx.ThunkCallback(new AsyncCallback(GetTokenUIAsyncResult.Callback));
        private ClientCredentials credentials;
        private IClientChannel proxy;
        private Uri relyingPartyIssuer;
        private bool requiresInfoCard;

        internal GetTokenUIAsyncResult(Binding binding, IClientChannel channel, ClientCredentials credentials, AsyncCallback callback, object state) : base(callback, state)
        {
            this.credentials = credentials;
            this.proxy = channel;
            this.binding = binding;
            this.CallBegin(true);
        }

        private static void Callback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                GetTokenUIAsyncResult asyncState = (GetTokenUIAsyncResult) result.AsyncState;
                Exception exception = null;
                asyncState.CallEnd(result, out exception);
                asyncState.CallComplete(false, exception);
            }
        }

        private void CallBegin(bool completedSynchronously)
        {
            IAsyncResult result = null;
            Exception exception = null;
            try
            {
                CardSpacePolicyElement[] elementArray;
                SecurityTokenManager clientCredentialsTokenManager = this.credentials.CreateSecurityTokenManager();
                this.requiresInfoCard = InfoCardHelper.IsInfocardRequired(this.binding, this.credentials, clientCredentialsTokenManager, this.proxy.RemoteAddress, out elementArray, out this.relyingPartyIssuer);
                MessageSecurityVersion bindingSecurityVersionOrDefault = InfoCardHelper.GetBindingSecurityVersionOrDefault(this.binding);
                WSSecurityTokenSerializer defaultInstance = WSSecurityTokenSerializer.DefaultInstance;
                result = this.credentials.GetInfoCardTokenCallback.BeginInvoke(this.requiresInfoCard, elementArray, clientCredentialsTokenManager.CreateSecurityTokenSerializer(bindingSecurityVersionOrDefault.SecurityTokenVersion), callback, this);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                exception = exception2;
            }
            if (exception == null)
            {
                if (!result.CompletedSynchronously)
                {
                    return;
                }
                this.CallEnd(result, out exception);
            }
            if (exception == null)
            {
                this.CallComplete(completedSynchronously, null);
            }
        }

        private void CallComplete(bool completedSynchronously, Exception exception)
        {
            base.Complete(completedSynchronously, exception);
        }

        private void CallEnd(IAsyncResult result, out Exception exception)
        {
            try
            {
                SecurityToken token = this.credentials.GetInfoCardTokenCallback.EndInvoke(result);
                ChannelParameterCollection property = this.proxy.GetProperty<ChannelParameterCollection>();
                if (property != null)
                {
                    property.Add(new InfoCardChannelParameter(token, this.relyingPartyIssuer, this.requiresInfoCard));
                }
                exception = null;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                exception = exception2;
            }
        }

        internal static void End(IAsyncResult result)
        {
            AsyncResult.End<GetTokenUIAsyncResult>(result);
        }
    }
}

