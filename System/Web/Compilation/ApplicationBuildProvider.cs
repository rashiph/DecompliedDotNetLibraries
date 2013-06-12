namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    internal class ApplicationBuildProvider : BaseTemplateBuildProvider
    {
        internal override BuildResultCompiledType CreateBuildResult(Type t)
        {
            BuildResultCompiledGlobalAsaxType type = new BuildResultCompiledGlobalAsaxType(t);
            if ((base.Parser.ApplicationObjects != null) || (base.Parser.SessionObjects != null))
            {
                type.HasAppOrSessionObjects = true;
            }
            return type;
        }

        internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser)
        {
            return new ApplicationFileCodeDomTreeGenerator((ApplicationFileParser) parser);
        }

        protected override TemplateParser CreateParser()
        {
            return new ApplicationFileParser();
        }

        internal static BuildResultCompiledGlobalAsaxType GetGlobalAsaxBuildResult(bool isPrecompiledApp)
        {
            string cacheKey = "App_global.asax";
            BuildResultCompiledGlobalAsaxType buildResultFromCache = BuildManager.GetBuildResultFromCache(cacheKey) as BuildResultCompiledGlobalAsaxType;
            if (buildResultFromCache == null)
            {
                if (isPrecompiledApp)
                {
                    return null;
                }
                VirtualPath globalAsaxVirtualPath = BuildManager.GlobalAsaxVirtualPath;
                if (!globalAsaxVirtualPath.FileExists())
                {
                    return null;
                }
                ApplicationBuildProvider o = new ApplicationBuildProvider();
                o.SetVirtualPath(globalAsaxVirtualPath);
                DateTime utcNow = DateTime.UtcNow;
                BuildProvidersCompiler compiler = new BuildProvidersCompiler(globalAsaxVirtualPath, BuildManager.GenerateRandomAssemblyName("App_global.asax"));
                compiler.SetBuildProviders(new SingleObjectCollection(o));
                CompilerResults results = compiler.PerformBuild();
                buildResultFromCache = (BuildResultCompiledGlobalAsaxType) o.GetBuildResult(results);
                buildResultFromCache.CacheToMemory = false;
                BuildManager.CacheBuildResult(cacheKey, buildResultFromCache, utcNow);
            }
            return buildResultFromCache;
        }
    }
}

