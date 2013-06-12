namespace System.Windows.Forms
{
    using System;

    public enum AutoCompleteSource
    {
        AllSystemSources = 7,
        AllUrl = 6,
        CustomSource = 0x40,
        FileSystem = 1,
        FileSystemDirectories = 0x20,
        HistoryList = 2,
        ListItems = 0x100,
        None = 0x80,
        RecentlyUsedList = 4
    }
}

