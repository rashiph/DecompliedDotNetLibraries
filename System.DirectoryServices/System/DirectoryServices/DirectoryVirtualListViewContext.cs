namespace System.DirectoryServices
{
    using System;

    public class DirectoryVirtualListViewContext
    {
        internal byte[] context;

        public DirectoryVirtualListViewContext() : this(new byte[0])
        {
        }

        internal DirectoryVirtualListViewContext(byte[] context)
        {
            if (context == null)
            {
                this.context = new byte[0];
            }
            else
            {
                this.context = new byte[context.Length];
                for (int i = 0; i < context.Length; i++)
                {
                    this.context[i] = context[i];
                }
            }
        }

        public DirectoryVirtualListViewContext Copy()
        {
            return new DirectoryVirtualListViewContext(this.context);
        }
    }
}

