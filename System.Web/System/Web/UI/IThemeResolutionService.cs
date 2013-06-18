namespace System.Web.UI
{
    public interface IThemeResolutionService
    {
        ThemeProvider[] GetAllThemeProviders();
        ThemeProvider GetStylesheetThemeProvider();
        ThemeProvider GetThemeProvider();
    }
}

