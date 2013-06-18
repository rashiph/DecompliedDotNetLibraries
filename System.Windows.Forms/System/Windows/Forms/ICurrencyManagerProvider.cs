namespace System.Windows.Forms
{
    using System;

    [System.Windows.Forms.SRDescription("ICurrencyManagerProviderDescr")]
    public interface ICurrencyManagerProvider
    {
        System.Windows.Forms.CurrencyManager GetRelatedCurrencyManager(string dataMember);

        System.Windows.Forms.CurrencyManager CurrencyManager { get; }
    }
}

