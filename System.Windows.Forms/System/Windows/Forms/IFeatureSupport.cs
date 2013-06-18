namespace System.Windows.Forms
{
    using System;

    public interface IFeatureSupport
    {
        Version GetVersionPresent(object feature);
        bool IsPresent(object feature);
        bool IsPresent(object feature, Version minimumVersion);
    }
}

