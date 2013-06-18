namespace System.Activities
{
    using System.Collections.ObjectModel;

    internal interface IDynamicActivity
    {
        KeyedCollection<string, DynamicActivityProperty> Properties { get; }
    }
}

