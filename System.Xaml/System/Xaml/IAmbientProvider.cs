namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    public interface IAmbientProvider
    {
        IEnumerable<object> GetAllAmbientValues(params XamlType[] types);
        IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties);
        IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties);
        object GetFirstAmbientValue(params XamlType[] types);
        AmbientPropertyValue GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties);
    }
}

