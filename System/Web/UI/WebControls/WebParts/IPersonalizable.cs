namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public interface IPersonalizable
    {
        void Load(PersonalizationDictionary state);
        void Save(PersonalizationDictionary state);

        bool IsDirty { get; }
    }
}

