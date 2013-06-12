namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;

    internal class BuildResultCodeCompileUnit : BuildResult
    {
        private string _cacheKey;
        private System.CodeDom.CodeCompileUnit _codeCompileUnit;
        private Type _codeDomProviderType;
        private System.CodeDom.Compiler.CompilerParameters _compilerParameters;
        private IDictionary _linePragmasTable;
        private const string fileNameAttribute = "CCUpreservationFileName";

        internal BuildResultCodeCompileUnit()
        {
        }

        internal BuildResultCodeCompileUnit(Type codeDomProviderType, System.CodeDom.CodeCompileUnit codeCompileUnit, System.CodeDom.Compiler.CompilerParameters compilerParameters, IDictionary linePragmasTable)
        {
            this._codeDomProviderType = codeDomProviderType;
            this._codeCompileUnit = codeCompileUnit;
            this._compilerParameters = compilerParameters;
            this._linePragmasTable = linePragmasTable;
        }

        protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner)
        {
            base.ComputeHashCode(hashCodeCombiner);
            CompilationSection compilationConfig = MTConfigUtil.GetCompilationConfig(base.VirtualPath);
            hashCodeCombiner.AddObject(compilationConfig.RecompilationHash);
            PagesSection pagesConfig = MTConfigUtil.GetPagesConfig(base.VirtualPath);
            hashCodeCombiner.AddObject(Util.GetRecompilationHash(pagesConfig));
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultCodeCompileUnit;
        }

        private string GetPreservationFileName()
        {
            return (this._cacheKey + ".ccu");
        }

        internal override void GetPreservedAttributes(PreservationFileReader pfr)
        {
            base.GetPreservedAttributes(pfr);
            string attribute = pfr.GetAttribute("CCUpreservationFileName");
            using (FileStream stream = File.Open(Path.Combine(HttpRuntime.CodegenDirInternal, attribute), FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                this._codeCompileUnit = formatter.Deserialize(stream) as System.CodeDom.CodeCompileUnit;
                this._codeDomProviderType = (Type) formatter.Deserialize(stream);
                this._compilerParameters = (System.CodeDom.Compiler.CompilerParameters) formatter.Deserialize(stream);
                this._linePragmasTable = formatter.Deserialize(stream) as IDictionary;
            }
        }

        internal override void RemoveOutOfDateResources(PreservationFileReader pfr)
        {
            string attribute = pfr.GetAttribute("CCUpreservationFileName");
            File.Delete(Path.Combine(HttpRuntime.CodegenDirInternal, attribute));
        }

        internal void SetCacheKey(string cacheKey)
        {
            this._cacheKey = cacheKey;
        }

        internal override void SetPreservedAttributes(PreservationFileWriter pfw)
        {
            base.SetPreservedAttributes(pfw);
            string preservationFileName = this.GetPreservationFileName();
            pfw.SetAttribute("CCUpreservationFileName", preservationFileName);
            using (FileStream stream = File.Open(Path.Combine(HttpRuntime.CodegenDirInternal, preservationFileName), FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                if (this._codeCompileUnit != null)
                {
                    formatter.Serialize(stream, this._codeCompileUnit);
                }
                else
                {
                    formatter.Serialize(stream, new object());
                }
                formatter.Serialize(stream, this._codeDomProviderType);
                formatter.Serialize(stream, this._compilerParameters);
                if (this._linePragmasTable != null)
                {
                    formatter.Serialize(stream, this._linePragmasTable);
                }
                else
                {
                    formatter.Serialize(stream, new object());
                }
            }
        }

        internal override bool CacheToDisk
        {
            get
            {
                return true;
            }
        }

        internal System.CodeDom.CodeCompileUnit CodeCompileUnit
        {
            get
            {
                return this._codeCompileUnit;
            }
        }

        internal Type CodeDomProviderType
        {
            get
            {
                return this._codeDomProviderType;
            }
        }

        internal System.CodeDom.Compiler.CompilerParameters CompilerParameters
        {
            get
            {
                return this._compilerParameters;
            }
        }

        internal IDictionary LinePragmasTable
        {
            get
            {
                return this._linePragmasTable;
            }
        }
    }
}

