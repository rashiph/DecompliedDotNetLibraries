namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class DrainOnCloseStream : DetectEofStream
    {
        public DrainOnCloseStream(MaxMessageSizeStream innerStream) : base(innerStream)
        {
        }

        public override void Close()
        {
            if (!base.IsAtEof)
            {
                byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(0x800);
                while (!base.IsAtEof)
                {
                    base.Read(buffer, 0, buffer.Length);
                }
            }
            base.Close();
        }
    }
}

