namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;

    public interface IPersistUIState
    {
        void LoadViewState(BinaryReader reader);
        void SaveViewState(BinaryWriter writer);
    }
}

