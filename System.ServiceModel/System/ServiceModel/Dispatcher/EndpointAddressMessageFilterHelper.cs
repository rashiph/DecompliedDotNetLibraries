namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    internal class EndpointAddressMessageFilterHelper
    {
        private EndpointAddress address;
        private Dictionary<string, EndpointAddressProcessor.HeaderBit[]> headerLookup;
        private byte[] mask;
        private WeakReference processorPool;
        private Dictionary<EndpointAddressProcessor.QName, int> qnameLookup;
        private int size;

        internal EndpointAddressMessageFilterHelper(EndpointAddress address)
        {
            this.address = address;
            if (this.address.Headers.Count > 0)
            {
                this.CreateMask();
                this.processorPool = new WeakReference(null);
            }
            else
            {
                this.qnameLookup = null;
                this.headerLookup = null;
                this.size = 0;
                this.mask = null;
            }
        }

        private void CreateMask()
        {
            int num = 0;
            this.qnameLookup = new Dictionary<EndpointAddressProcessor.QName, int>(EndpointAddressProcessor.QNameComparer);
            this.headerLookup = new Dictionary<string, EndpointAddressProcessor.HeaderBit[]>();
            StringBuilder builder = null;
            for (int i = 0; i < this.address.Headers.Count; i++)
            {
                EndpointAddressProcessor.HeaderBit[] bitArray;
                if (builder == null)
                {
                    builder = new StringBuilder();
                }
                else
                {
                    builder.Remove(0, builder.Length);
                }
                string comparableForm = this.address.Headers[i].GetComparableForm(builder);
                if (this.headerLookup.TryGetValue(comparableForm, out bitArray))
                {
                    Array.Resize<EndpointAddressProcessor.HeaderBit>(ref bitArray, bitArray.Length + 1);
                    bitArray[bitArray.Length - 1] = new EndpointAddressProcessor.HeaderBit(num++);
                    this.headerLookup[comparableForm] = bitArray;
                }
                else
                {
                    EndpointAddressProcessor.QName name;
                    EndpointAddressProcessor.HeaderBit[] bitArray2 = new EndpointAddressProcessor.HeaderBit[] { new EndpointAddressProcessor.HeaderBit(num++) };
                    this.headerLookup.Add(comparableForm, bitArray2);
                    AddressHeader header = this.address.Headers[i];
                    name.name = header.Name;
                    name.ns = header.Namespace;
                    this.qnameLookup[name] = 1;
                }
            }
            if (num == 0)
            {
                this.size = 0;
            }
            else
            {
                this.size = ((num - 1) / 8) + 1;
            }
            if (this.size > 0)
            {
                this.mask = new byte[this.size];
                for (int j = 0; j < (this.size - 1); j++)
                {
                    this.mask[j] = 0xff;
                }
                if ((num % 8) == 0)
                {
                    this.mask[this.size - 1] = 0xff;
                }
                else
                {
                    this.mask[this.size - 1] = (byte) ((((int) 1) << (num % 8)) - 1);
                }
            }
        }

        private EndpointAddressProcessor CreateProcessor(int length)
        {
            if (this.processorPool.Target != null)
            {
                lock (this.processorPool)
                {
                    object target = this.processorPool.Target;
                    if (target != null)
                    {
                        EndpointAddressProcessor processor = (EndpointAddressProcessor) target;
                        this.processorPool.Target = processor.Next;
                        processor.Next = null;
                        processor.Clear(length);
                        return processor;
                    }
                }
            }
            return new EndpointAddressProcessor(length);
        }

        internal bool Match(Message message)
        {
            if (this.size == 0)
            {
                return true;
            }
            EndpointAddressProcessor context = this.CreateProcessor(this.size);
            context.ProcessHeaders(message, this.qnameLookup, this.headerLookup);
            bool flag = context.TestExact(this.mask);
            this.ReleaseProcessor(context);
            return flag;
        }

        private void ReleaseProcessor(EndpointAddressProcessor context)
        {
            lock (this.processorPool)
            {
                context.Next = this.processorPool.Target as EndpointAddressProcessor;
                this.processorPool.Target = context;
            }
        }

        internal Dictionary<string, EndpointAddressProcessor.HeaderBit[]> HeaderLookup
        {
            get
            {
                if (this.headerLookup == null)
                {
                    this.headerLookup = new Dictionary<string, EndpointAddressProcessor.HeaderBit[]>();
                }
                return this.headerLookup;
            }
        }
    }
}

