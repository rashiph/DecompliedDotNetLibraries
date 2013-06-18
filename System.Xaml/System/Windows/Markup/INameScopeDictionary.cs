namespace System.Windows.Markup
{
    using System.Collections;
    using System.Collections.Generic;

    public interface INameScopeDictionary : INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
    }
}

