namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime;

    internal class PinningHelper
    {
        [ForceTokenStabilization]
        public byte m_data;
    }
}

