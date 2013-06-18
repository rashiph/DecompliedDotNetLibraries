namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("ED4BAE22-2F3C-419a-B487-CF869E716B95")]
    public interface IVsaScriptScope : IJSVsaItem
    {
        IVsaScriptScope Parent { get; }
        IJSVsaItem AddItem(string itemName, JSVsaItemType type);
        IJSVsaItem GetItem(string itemName);
        void RemoveItem(string itemName);
        void RemoveItem(IJSVsaItem item);
        int GetItemCount();
        IJSVsaItem GetItemAtIndex(int index);
        void RemoveItemAtIndex(int index);
        object GetObject();
        IJSVsaItem CreateDynamicItem(string itemName, JSVsaItemType type);
    }
}

