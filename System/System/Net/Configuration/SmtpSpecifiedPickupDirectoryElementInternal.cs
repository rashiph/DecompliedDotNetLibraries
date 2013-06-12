namespace System.Net.Configuration
{
    using System;

    internal sealed class SmtpSpecifiedPickupDirectoryElementInternal
    {
        private string pickupDirectoryLocation;

        internal SmtpSpecifiedPickupDirectoryElementInternal(SmtpSpecifiedPickupDirectoryElement element)
        {
            this.pickupDirectoryLocation = element.PickupDirectoryLocation;
        }

        internal string PickupDirectoryLocation
        {
            get
            {
                return this.pickupDirectoryLocation;
            }
        }
    }
}

