namespace System.Web.UI
{
    using System;

    public interface IFilterResolutionService
    {
        int CompareFilters(string filter1, string filter2);
        bool EvaluateFilter(string filterName);
    }
}

