namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Net.Mime;

    internal class SendMailAsyncResult : LazyAsyncResult
    {
        private SmtpConnection connection;
        private string deliveryNotify;
        private ArrayList failedRecipientExceptions;
        private string from;
        private static AsyncCallback sendDataCompleted = new AsyncCallback(SendMailAsyncResult.SendDataCompleted);
        private static AsyncCallback sendMailFromCompleted = new AsyncCallback(SendMailAsyncResult.SendMailFromCompleted);
        private static AsyncCallback sendToCollectionCompleted = new AsyncCallback(SendMailAsyncResult.SendToCollectionCompleted);
        private static AsyncCallback sendToCompleted = new AsyncCallback(SendMailAsyncResult.SendToCompleted);
        private Stream stream;
        private string to;
        private MailAddressCollection toCollection;
        private int toIndex;

        internal SendMailAsyncResult(SmtpConnection connection, string from, MailAddressCollection toCollection, string deliveryNotify, AsyncCallback callback, object state) : base(null, state, callback)
        {
            this.failedRecipientExceptions = new ArrayList();
            this.toCollection = toCollection;
            this.connection = connection;
            this.from = from;
            this.deliveryNotify = deliveryNotify;
        }

        internal static MailWriter End(IAsyncResult result)
        {
            SendMailAsyncResult result2 = (SendMailAsyncResult) result;
            object obj2 = result2.InternalWaitForCompletion();
            if (obj2 is Exception)
            {
                throw ((Exception) obj2);
            }
            return new MailWriter(result2.stream);
        }

        internal void Send()
        {
            this.SendMailFrom();
        }

        private void SendData()
        {
            IAsyncResult result = DataCommand.BeginSend(this.connection, sendDataCompleted, this);
            if (result.CompletedSynchronously)
            {
                DataCommand.EndSend(result);
                this.stream = this.connection.GetClosableStream();
                if (this.failedRecipientExceptions.Count > 1)
                {
                    base.InvokeCallback(new SmtpFailedRecipientsException(this.failedRecipientExceptions, this.failedRecipientExceptions.Count == this.toCollection.Count));
                }
                else if (this.failedRecipientExceptions.Count == 1)
                {
                    base.InvokeCallback(this.failedRecipientExceptions[0]);
                }
                else
                {
                    base.InvokeCallback();
                }
            }
        }

        private static void SendDataCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult asyncState = (SendMailAsyncResult) result.AsyncState;
                try
                {
                    DataCommand.EndSend(result);
                    asyncState.stream = asyncState.connection.GetClosableStream();
                    if (asyncState.failedRecipientExceptions.Count > 1)
                    {
                        asyncState.InvokeCallback(new SmtpFailedRecipientsException(asyncState.failedRecipientExceptions, asyncState.failedRecipientExceptions.Count == asyncState.toCollection.Count));
                    }
                    else if (asyncState.failedRecipientExceptions.Count == 1)
                    {
                        asyncState.InvokeCallback(asyncState.failedRecipientExceptions[0]);
                    }
                    else
                    {
                        asyncState.InvokeCallback();
                    }
                }
                catch (Exception exception)
                {
                    asyncState.InvokeCallback(exception);
                }
            }
        }

        private void SendMailFrom()
        {
            IAsyncResult result = MailCommand.BeginSend(this.connection, SmtpCommands.Mail, this.from, sendMailFromCompleted, this);
            if (result.CompletedSynchronously)
            {
                MailCommand.EndSend(result);
                this.SendTo();
            }
        }

        private static void SendMailFromCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult asyncState = (SendMailAsyncResult) result.AsyncState;
                try
                {
                    MailCommand.EndSend(result);
                    asyncState.SendTo();
                }
                catch (Exception exception)
                {
                    asyncState.InvokeCallback(exception);
                }
            }
        }

        private void SendTo()
        {
            if (this.to != null)
            {
                IAsyncResult result = RecipientCommand.BeginSend(this.connection, (this.deliveryNotify != null) ? (this.to + this.deliveryNotify) : this.to, sendToCompleted, this);
                if (result.CompletedSynchronously)
                {
                    string str;
                    if (!RecipientCommand.EndSend(result, out str))
                    {
                        throw new SmtpFailedRecipientException(this.connection.Reader.StatusCode, this.to, str);
                    }
                    this.SendData();
                }
            }
            else if (this.SendToCollection())
            {
                this.SendData();
            }
        }

        private bool SendToCollection()
        {
            while (this.toIndex < this.toCollection.Count)
            {
                string str;
                MultiAsyncResult result = (MultiAsyncResult) RecipientCommand.BeginSend(this.connection, this.toCollection[this.toIndex++].SmtpAddress + this.deliveryNotify, sendToCollectionCompleted, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                if (!RecipientCommand.EndSend(result, out str))
                {
                    this.failedRecipientExceptions.Add(new SmtpFailedRecipientException(this.connection.Reader.StatusCode, this.toCollection[this.toIndex - 1].SmtpAddress, str));
                }
            }
            return true;
        }

        private static void SendToCollectionCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult asyncState = (SendMailAsyncResult) result.AsyncState;
                try
                {
                    string str;
                    if (!RecipientCommand.EndSend(result, out str))
                    {
                        asyncState.failedRecipientExceptions.Add(new SmtpFailedRecipientException(asyncState.connection.Reader.StatusCode, asyncState.toCollection[asyncState.toIndex - 1].SmtpAddress, str));
                        if (asyncState.failedRecipientExceptions.Count == asyncState.toCollection.Count)
                        {
                            SmtpFailedRecipientException exception = null;
                            if (asyncState.toCollection.Count == 1)
                            {
                                exception = (SmtpFailedRecipientException) asyncState.failedRecipientExceptions[0];
                            }
                            else
                            {
                                exception = new SmtpFailedRecipientsException(asyncState.failedRecipientExceptions, true);
                            }
                            exception.fatal = true;
                            asyncState.InvokeCallback(exception);
                            return;
                        }
                    }
                    if (asyncState.SendToCollection())
                    {
                        asyncState.SendData();
                    }
                }
                catch (Exception exception2)
                {
                    asyncState.InvokeCallback(exception2);
                }
            }
        }

        private static void SendToCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                SendMailAsyncResult asyncState = (SendMailAsyncResult) result.AsyncState;
                try
                {
                    string str;
                    if (RecipientCommand.EndSend(result, out str))
                    {
                        asyncState.SendData();
                    }
                    else
                    {
                        asyncState.InvokeCallback(new SmtpFailedRecipientException(asyncState.connection.Reader.StatusCode, asyncState.to, str));
                    }
                }
                catch (Exception exception)
                {
                    asyncState.InvokeCallback(exception);
                }
            }
        }
    }
}

