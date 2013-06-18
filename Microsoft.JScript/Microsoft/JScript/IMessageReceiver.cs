namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("F062C7FB-53BF-4f0d-B0F6-D66C5948E63F")]
    public interface IMessageReceiver
    {
        void Message(string strValue);
    }
}

