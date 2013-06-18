namespace System.Web.UI
{
    using System;

    public interface IStateFormatter
    {
        object Deserialize(string serializedState);
        string Serialize(object state);
    }
}

