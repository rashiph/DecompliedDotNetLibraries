namespace System.Net.Mail
{
    using System;

    [Flags]
    public enum DeliveryNotificationOptions
    {
        Delay = 4,
        Never = 0x8000000,
        None = 0,
        OnFailure = 2,
        OnSuccess = 1
    }
}

