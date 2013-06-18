namespace System.Web.Compilation
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    internal class BuildResultCompiledType : BuildResultCompiledAssemblyBase, ITypedWebObjectFactory, IWebObjectFactory
    {
        private Type _builtType;
        private InstantiateObject _instObj;
        private bool _triedToGetInstObj;

        internal BuildResultCompiledType()
        {
        }

        internal BuildResultCompiledType(Type t)
        {
            this._builtType = t;
        }

        protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner)
        {
            base.ComputeHashCode(hashCodeCombiner);
            if (base.VirtualPath != null)
            {
                Assembly localResourcesAssembly = BuildManager.GetLocalResourcesAssembly(base.VirtualPath.Parent);
                if (localResourcesAssembly != null)
                {
                    hashCodeCombiner.AddFile(localResourcesAssembly.Location);
                }
            }
        }

        public object CreateInstance()
        {
            if (!this._triedToGetInstObj)
            {
                this._instObj = ObjectFactoryCodeDomTreeGenerator.GetFastObjectCreationDelegate(this.ResultType);
                this._triedToGetInstObj = true;
            }
            if (this._instObj == null)
            {
                return HttpRuntime.CreatePublicInstance(this.ResultType);
            }
            return this._instObj();
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultCompiledType;
        }

        internal override void GetPreservedAttributes(PreservationFileReader pfr)
        {
            base.GetPreservedAttributes(pfr);
            Assembly preservedAssembly = BuildResultCompiledAssemblyBase.GetPreservedAssembly(pfr);
            string attribute = pfr.GetAttribute("type");
            this.ResultType = preservedAssembly.GetType(attribute, true);
        }

        internal override void SetPreservedAttributes(PreservationFileWriter pfw)
        {
            base.SetPreservedAttributes(pfw);
            pfw.SetAttribute("type", this.FullResultTypeName);
        }

        internal static bool UsesDelayLoadType(BuildResult result)
        {
            BuildResultCompiledType type = result as BuildResultCompiledType;
            return ((type != null) && type.IsDelayLoadType);
        }

        private string FullResultTypeName
        {
            get
            {
                DelayLoadType resultType = this.ResultType as DelayLoadType;
                if (resultType != null)
                {
                    return resultType.TypeName;
                }
                return this.ResultType.FullName;
            }
        }

        internal override bool HasResultAssembly
        {
            get
            {
                return (this._builtType != null);
            }
        }

        public virtual Type InstantiatedType
        {
            get
            {
                return this.ResultType;
            }
        }

        internal bool IsDelayLoadType
        {
            get
            {
                return (this.ResultType is DelayLoadType);
            }
        }

        protected override bool IsGacAssembly
        {
            get
            {
                if (this.IsDelayLoadType)
                {
                    return false;
                }
                return base.IsGacAssembly;
            }
        }

        internal override Assembly ResultAssembly
        {
            get
            {
                return this._builtType.Assembly;
            }
            set
            {
            }
        }

        internal Type ResultType
        {
            get
            {
                return this._builtType;
            }
            set
            {
                this._builtType = value;
            }
        }

        protected override string ShortAssemblyName
        {
            get
            {
                DelayLoadType resultType = this.ResultType as DelayLoadType;
                if (resultType != null)
                {
                    return resultType.AssemblyName;
                }
                return base.ShortAssemblyName;
            }
        }
    }
}

