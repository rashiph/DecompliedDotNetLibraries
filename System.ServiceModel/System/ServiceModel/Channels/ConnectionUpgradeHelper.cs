namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;

    internal class ConnectionUpgradeHelper
    {
        public static IAsyncResult BeginDecodeFramingFault(ClientFramingDecoder decoder, IConnection connection, Uri via, string contentType, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
        {
            return new DecodeFailedUpgradeAsyncResult(decoder, connection, via, contentType, ref timeoutHelper, callback, state);
        }

        public static IAsyncResult BeginInitiateUpgrade(IDefaultCommunicationTimeouts timeouts, EndpointAddress remoteAddress, IConnection connection, ClientFramingDecoder decoder, StreamUpgradeInitiator upgradeInitiator, string contentType, WindowsIdentity identityToImpersonate, TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
        {
            return new InitiateUpgradeAsyncResult(timeouts, remoteAddress, connection, decoder, upgradeInitiator, contentType, identityToImpersonate, timeoutHelper, callback, state);
        }

        public static void DecodeFramingFault(ClientFramingDecoder decoder, IConnection connection, Uri via, string contentType, ref TimeoutHelper timeoutHelper)
        {
            ValidateReadingFaultString(decoder);
            int offset = 0;
            byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(0x100);
            int size = connection.Read(buffer, offset, buffer.Length, timeoutHelper.RemainingTime());
            while (size > 0)
            {
                int num3 = decoder.Decode(buffer, offset, size);
                offset += num3;
                size -= num3;
                if (decoder.CurrentState == ClientFramingDecoderState.Fault)
                {
                    ConnectionUtilities.CloseNoThrow(connection, timeoutHelper.RemainingTime());
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(FaultStringDecoder.GetFaultException(decoder.Fault, via.ToString(), contentType));
                }
                if (decoder.CurrentState != ClientFramingDecoderState.ReadingFaultString)
                {
                    throw Fx.AssertAndThrow("invalid framing client state machine");
                }
                if (size == 0)
                {
                    offset = 0;
                    size = connection.Read(buffer, offset, buffer.Length, timeoutHelper.RemainingTime());
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(decoder.CreatePrematureEOFException());
        }

        public static void EndDecodeFramingFault(IAsyncResult result)
        {
            DecodeFailedUpgradeAsyncResult.End(result);
        }

        public static IConnection EndInitiateUpgrade(IAsyncResult result)
        {
            return InitiateUpgradeAsyncResult.End(result);
        }

        public static bool InitiateUpgrade(StreamUpgradeInitiator upgradeInitiator, ref IConnection connection, ClientFramingDecoder decoder, IDefaultCommunicationTimeouts defaultTimeouts, ref TimeoutHelper timeoutHelper)
        {
            for (string str = upgradeInitiator.GetNextUpgrade(); str != null; str = upgradeInitiator.GetNextUpgrade())
            {
                EncodedUpgrade upgrade = new EncodedUpgrade(str);
                connection.Write(upgrade.EncodedBytes, 0, upgrade.EncodedBytes.Length, true, timeoutHelper.RemainingTime());
                byte[] buffer = new byte[1];
                int count = connection.Read(buffer, 0, buffer.Length, timeoutHelper.RemainingTime());
                if (!ValidateUpgradeResponse(buffer, count, decoder))
                {
                    return false;
                }
                ConnectionStream stream = new ConnectionStream(connection, defaultTimeouts);
                Stream stream2 = upgradeInitiator.InitiateUpgrade(stream);
                connection = new StreamConnection(stream2, stream);
            }
            return true;
        }

        public static bool ValidatePreambleResponse(byte[] buffer, int count, ClientFramingDecoder decoder, Uri via)
        {
            if (count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("ServerRejectedSessionPreamble", new object[] { via }), decoder.CreatePrematureEOFException()));
            }
            while (decoder.Decode(buffer, 0, count) == 0)
            {
            }
            if (decoder.CurrentState != ClientFramingDecoderState.Start)
            {
                return false;
            }
            return true;
        }

        private static void ValidateReadingFaultString(ClientFramingDecoder decoder)
        {
            if (decoder.CurrentState != ClientFramingDecoderState.ReadingFaultString)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ServerRejectedUpgradeRequest")));
            }
        }

        private static bool ValidateUpgradeResponse(byte[] buffer, int count, ClientFramingDecoder decoder)
        {
            if (count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ServerRejectedUpgradeRequest"), decoder.CreatePrematureEOFException()));
            }
            while (decoder.Decode(buffer, 0, count) == 0)
            {
            }
            if (decoder.CurrentState != ClientFramingDecoderState.UpgradeResponse)
            {
                return false;
            }
            return true;
        }

        private class DecodeFailedUpgradeAsyncResult : AsyncResult
        {
            private IConnection connection;
            private string contentType;
            private ClientFramingDecoder decoder;
            private static WaitCallback onReadFaultData = new WaitCallback(ConnectionUpgradeHelper.DecodeFailedUpgradeAsyncResult.OnReadFaultData);
            private TimeoutHelper timeoutHelper;
            private Uri via;

            public DecodeFailedUpgradeAsyncResult(ClientFramingDecoder decoder, IConnection connection, Uri via, string contentType, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
            {
                ConnectionUpgradeHelper.ValidateReadingFaultString(decoder);
                this.decoder = decoder;
                this.connection = connection;
                this.via = via;
                this.contentType = contentType;
                this.timeoutHelper = timeoutHelper;
                if (connection.BeginRead(0, Math.Min(0x100, connection.AsyncReadBufferSize), timeoutHelper.RemainingTime(), onReadFaultData, this) != AsyncReadResult.Queued)
                {
                    this.CompleteReadFaultData();
                }
            }

            private void CompleteReadFaultData()
            {
                int offset = 0;
                int size = this.connection.EndRead();
                while (size > 0)
                {
                    int num3 = this.decoder.Decode(this.connection.AsyncReadBuffer, offset, size);
                    offset += num3;
                    size -= num3;
                    if (this.decoder.CurrentState == ClientFramingDecoderState.Fault)
                    {
                        ConnectionUtilities.CloseNoThrow(this.connection, this.timeoutHelper.RemainingTime());
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(FaultStringDecoder.GetFaultException(this.decoder.Fault, this.via.ToString(), this.contentType));
                    }
                    if (this.decoder.CurrentState != ClientFramingDecoderState.ReadingFaultString)
                    {
                        throw Fx.AssertAndThrow("invalid framing client state machine");
                    }
                    if (size == 0)
                    {
                        offset = 0;
                        if (this.connection.BeginRead(0, Math.Min(0x100, this.connection.AsyncReadBufferSize), this.timeoutHelper.RemainingTime(), onReadFaultData, this) == AsyncReadResult.Queued)
                        {
                            return;
                        }
                        size = this.connection.EndRead();
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ConnectionUpgradeHelper.DecodeFailedUpgradeAsyncResult>(result);
            }

            private static void OnReadFaultData(object state)
            {
                ConnectionUpgradeHelper.DecodeFailedUpgradeAsyncResult result = (ConnectionUpgradeHelper.DecodeFailedUpgradeAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.CompleteReadFaultData();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    result.Complete(false, exception);
                }
            }
        }

        private class InitiateUpgradeAsyncResult : AsyncResult
        {
            private IConnection connection;
            private ConnectionStream connectionStream;
            private string contentType;
            private ClientFramingDecoder decoder;
            private IDefaultCommunicationTimeouts defaultTimeouts;
            private WindowsIdentity identityToImpersonate;
            private static AsyncCallback onFailedUpgrade;
            private static AsyncCallback onInitiateUpgrade = Fx.ThunkCallback(new AsyncCallback(ConnectionUpgradeHelper.InitiateUpgradeAsyncResult.OnInitiateUpgrade));
            private static WaitCallback onReadUpgradeResponse = new WaitCallback(ConnectionUpgradeHelper.InitiateUpgradeAsyncResult.OnReadUpgradeResponse);
            private static AsyncCallback onWriteUpgradeBytes = Fx.ThunkCallback(new AsyncCallback(ConnectionUpgradeHelper.InitiateUpgradeAsyncResult.OnWriteUpgradeBytes));
            private EndpointAddress remoteAddress;
            private TimeoutHelper timeoutHelper;
            private StreamUpgradeInitiator upgradeInitiator;

            public InitiateUpgradeAsyncResult(IDefaultCommunicationTimeouts timeouts, EndpointAddress remoteAddress, IConnection connection, ClientFramingDecoder decoder, StreamUpgradeInitiator upgradeInitiator, string contentType, WindowsIdentity identityToImpersonate, TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
            {
                this.defaultTimeouts = timeouts;
                this.decoder = decoder;
                this.upgradeInitiator = upgradeInitiator;
                this.contentType = contentType;
                this.timeoutHelper = timeoutHelper;
                this.connection = connection;
                this.remoteAddress = remoteAddress;
                this.identityToImpersonate = identityToImpersonate;
                if (this.Begin())
                {
                    base.Complete(true);
                }
            }

            private bool Begin()
            {
                for (string str = this.upgradeInitiator.GetNextUpgrade(); str != null; str = this.upgradeInitiator.GetNextUpgrade())
                {
                    EncodedUpgrade upgrade = new EncodedUpgrade(str);
                    IAsyncResult result = this.connection.BeginWrite(upgrade.EncodedBytes, 0, upgrade.EncodedBytes.Length, true, this.timeoutHelper.RemainingTime(), onWriteUpgradeBytes, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    if (!this.CompleteWriteUpgradeBytes(result))
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool CompleteReadUpgradeResponse()
            {
                int count = this.connection.EndRead();
                if (!ConnectionUpgradeHelper.ValidateUpgradeResponse(this.connection.AsyncReadBuffer, count, this.decoder))
                {
                    if (onFailedUpgrade == null)
                    {
                        onFailedUpgrade = Fx.ThunkCallback(new AsyncCallback(ConnectionUpgradeHelper.InitiateUpgradeAsyncResult.OnFailedUpgrade));
                    }
                    IAsyncResult result = ConnectionUpgradeHelper.BeginDecodeFramingFault(this.decoder, this.connection, this.remoteAddress.Uri, this.contentType, ref this.timeoutHelper, onFailedUpgrade, this);
                    if (result.CompletedSynchronously)
                    {
                        ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                    }
                    return result.CompletedSynchronously;
                }
                this.connectionStream = new ConnectionStream(this.connection, this.defaultTimeouts);
                IAsyncResult result2 = null;
                WindowsImpersonationContext context = (this.identityToImpersonate == null) ? null : this.identityToImpersonate.Impersonate();
                try
                {
                    using (context)
                    {
                        result2 = this.upgradeInitiator.BeginInitiateUpgrade(this.connectionStream, onInitiateUpgrade, this);
                    }
                }
                catch
                {
                    throw;
                }
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                this.CompleteUpgrade(result2);
                return true;
            }

            private void CompleteUpgrade(IAsyncResult result)
            {
                Stream stream = this.upgradeInitiator.EndInitiateUpgrade(result);
                this.connection = new StreamConnection(stream, this.connectionStream);
            }

            private bool CompleteWriteUpgradeBytes(IAsyncResult result)
            {
                this.connection.EndWrite(result);
                if (this.connection.BeginRead(0, ServerSessionEncoder.UpgradeResponseBytes.Length, this.timeoutHelper.RemainingTime(), onReadUpgradeResponse, this) == AsyncReadResult.Queued)
                {
                    return false;
                }
                return this.CompleteReadUpgradeResponse();
            }

            public static IConnection End(IAsyncResult result)
            {
                return AsyncResult.End<ConnectionUpgradeHelper.InitiateUpgradeAsyncResult>(result).connection;
            }

            private static void OnFailedUpgrade(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectionUpgradeHelper.InitiateUpgradeAsyncResult asyncState = (ConnectionUpgradeHelper.InitiateUpgradeAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }

            private static void OnInitiateUpgrade(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ConnectionUpgradeHelper.InitiateUpgradeAsyncResult asyncState = (ConnectionUpgradeHelper.InitiateUpgradeAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.CompleteUpgrade(result);
                        flag = asyncState.Begin();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnReadUpgradeResponse(object state)
            {
                ConnectionUpgradeHelper.InitiateUpgradeAsyncResult result = (ConnectionUpgradeHelper.InitiateUpgradeAsyncResult) state;
                Exception exception = null;
                bool flag = false;
                try
                {
                    if (result.CompleteReadUpgradeResponse())
                    {
                        flag = result.Begin();
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception2;
                }
                if (flag)
                {
                    result.Complete(false, exception);
                }
            }

            private static void OnWriteUpgradeBytes(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectionUpgradeHelper.InitiateUpgradeAsyncResult asyncState = (ConnectionUpgradeHelper.InitiateUpgradeAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        if (asyncState.CompleteWriteUpgradeBytes(result))
                        {
                            flag = asyncState.Begin();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }
    }
}

