namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal static class ThemeDirectoryCompiler
    {
        internal const string skinExtension = ".skin";

        private static void AddThemeFilesToBuildProvider(VirtualDirectory vdir, PageThemeBuildProvider themeBuildProvider, bool topLevel)
        {
            foreach (VirtualFileBase base2 in vdir.Children)
            {
                if (base2.IsDirectory)
                {
                    AddThemeFilesToBuildProvider(base2 as VirtualDirectory, themeBuildProvider, false);
                }
                else
                {
                    string extension = Path.GetExtension(base2.Name);
                    if (StringUtil.EqualsIgnoreCase(extension, ".skin") && topLevel)
                    {
                        themeBuildProvider.AddSkinFile(base2.VirtualPathObject);
                    }
                    else if (StringUtil.EqualsIgnoreCase(extension, ".css"))
                    {
                        themeBuildProvider.AddCssFile(base2.VirtualPathObject);
                    }
                }
            }
        }

        internal static VirtualPath GetAppThemeVirtualDir(string themeName)
        {
            return HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombineWithDir("App_Themes/" + themeName);
        }

        internal static VirtualPath GetGlobalThemeVirtualDir(string themeName)
        {
            return BuildManager.ScriptVirtualDir.SimpleCombineWithDir("Themes/" + themeName);
        }

        private static BuildResultCompiledType GetThemeBuildResultType(string themeName)
        {
            string cacheKey = null;
            string str = "Theme_" + Util.MakeValidTypeNameFromString(themeName);
            BuildResultCompiledType buildResultFromCache = (BuildResultCompiledType) BuildManager.GetBuildResultFromCache(str);
            if (buildResultFromCache == null)
            {
                cacheKey = "GlobalTheme_" + themeName;
                buildResultFromCache = (BuildResultCompiledType) BuildManager.GetBuildResultFromCache(cacheKey);
            }
            if (buildResultFromCache == null)
            {
                bool gotLock = false;
                try
                {
                    CompilationLock.GetLock(ref gotLock);
                    buildResultFromCache = (BuildResultCompiledType) BuildManager.GetBuildResultFromCache(str);
                    if (buildResultFromCache == null)
                    {
                        buildResultFromCache = (BuildResultCompiledType) BuildManager.GetBuildResultFromCache(cacheKey);
                    }
                    if (buildResultFromCache != null)
                    {
                        return buildResultFromCache;
                    }
                    VirtualPath virtualDirPath = null;
                    VirtualPath appThemeVirtualDir = GetAppThemeVirtualDir(themeName);
                    PageThemeBuildProvider themeBuildProvider = null;
                    VirtualPath configPath = appThemeVirtualDir;
                    string str3 = str;
                    if (appThemeVirtualDir.DirectoryExists())
                    {
                        themeBuildProvider = new PageThemeBuildProvider(appThemeVirtualDir);
                    }
                    else
                    {
                        virtualDirPath = GetGlobalThemeVirtualDir(themeName);
                        if (!virtualDirPath.DirectoryExists())
                        {
                            throw new HttpException(System.Web.SR.GetString("Page_theme_not_found", new object[] { themeName }));
                        }
                        configPath = virtualDirPath;
                        str3 = cacheKey;
                        themeBuildProvider = new GlobalPageThemeBuildProvider(virtualDirPath);
                    }
                    DateTime utcNow = DateTime.UtcNow;
                    AddThemeFilesToBuildProvider(configPath.GetDirectory(), themeBuildProvider, true);
                    BuildProvidersCompiler compiler = new BuildProvidersCompiler(configPath, themeBuildProvider.AssemblyNamePrefix + BuildManager.GenerateRandomAssemblyName(themeName));
                    compiler.SetBuildProviders(new SingleObjectCollection(themeBuildProvider));
                    CompilerResults results = compiler.PerformBuild();
                    buildResultFromCache = (BuildResultCompiledType) themeBuildProvider.GetBuildResult(results);
                    BuildManager.CacheBuildResult(str3, buildResultFromCache, utcNow);
                }
                finally
                {
                    if (gotLock)
                    {
                        CompilationLock.ReleaseLock();
                    }
                }
            }
            return buildResultFromCache;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static BuildResultCompiledType GetThemeBuildResultType(HttpContext context, string themeName)
        {
            using (new ApplicationImpersonationContext())
            {
                return GetThemeBuildResultType(themeName);
            }
        }
    }
}

