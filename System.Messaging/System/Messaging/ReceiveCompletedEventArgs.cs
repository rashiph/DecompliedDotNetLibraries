﻿namespace System.Messaging
{
    using System;
    using System.Runtime;

    public class ReceiveCompletedEventArgs : EventArgs
    {
        private System.Messaging.Message message;
        private IAsyncResult result;
        private MessageQueue sender;

        internal ReceiveCompletedEventArgs(MessageQueue sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
        }

        public IAsyncResult AsyncResult
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.result;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.result = value;
            }
        }

        public System.Messaging.Message Message
        {
            get
            {
                if (this.message == null)
                {
                    try
                    {
                        this.message = this.sender.EndReceive(this.result);
                    }
                    catch
                    {
                        throw;
                    }
                }
                return this.message;
            }
        }
    }
}

