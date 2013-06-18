namespace System.Deployment.Application
{
    using System;

    internal enum ActivationType
    {
        None,
        InstallViaDotApplication,
        InstallViaShortcut,
        InstallViaFileAssociation,
        UpdateViaShortcutOrFA
    }
}

