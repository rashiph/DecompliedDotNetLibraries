namespace System.ServiceModel.Channels
{
    using System;

    public interface IReceiveContextSettings
    {
        bool Enabled { get; set; }

        TimeSpan ValidityDuration { get; }
    }
}

