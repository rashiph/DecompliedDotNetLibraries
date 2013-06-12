namespace System.Configuration
{
    using System;

    public interface IPersistComponentSettings
    {
        void LoadComponentSettings();
        void ResetComponentSettings();
        void SaveComponentSettings();

        bool SaveSettings { get; set; }

        string SettingsKey { get; set; }
    }
}

