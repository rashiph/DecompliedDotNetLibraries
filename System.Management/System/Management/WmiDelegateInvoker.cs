namespace System.Management
{
    using System;

    internal class WmiDelegateInvoker
    {
        internal object sender;

        internal WmiDelegateInvoker(object sender)
        {
            this.sender = sender;
        }

        internal void FireEventToDelegates(MulticastDelegate md, ManagementEventArgs args)
        {
            try
            {
                if (md != null)
                {
                    foreach (Delegate delegate2 in md.GetInvocationList())
                    {
                        try
                        {
                            delegate2.DynamicInvoke(new object[] { this.sender, args });
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}

