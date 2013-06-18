namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class SignatureConfirmations
    {
        private SignatureConfirmation[] confirmations = new SignatureConfirmation[1];
        private bool encrypted;
        private int length = 0;

        public void AddConfirmation(byte[] value, bool encrypted)
        {
            if (this.confirmations.Length == this.length)
            {
                SignatureConfirmation[] destinationArray = new SignatureConfirmation[this.length * 2];
                Array.Copy(this.confirmations, 0, destinationArray, 0, this.length);
                this.confirmations = destinationArray;
            }
            this.confirmations[this.length] = new SignatureConfirmation(value);
            this.length++;
            this.encrypted |= encrypted;
        }

        public void GetConfirmation(int index, out byte[] value, out bool encrypted)
        {
            if ((index < 0) || (index >= this.length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.length })));
            }
            value = this.confirmations[index].value;
            encrypted = this.encrypted;
        }

        public int Count
        {
            get
            {
                return this.length;
            }
        }

        public bool IsMarkedForEncryption
        {
            get
            {
                return this.encrypted;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SignatureConfirmation
        {
            public byte[] value;
            public SignatureConfirmation(byte[] value)
            {
                this.value = value;
            }
        }
    }
}

