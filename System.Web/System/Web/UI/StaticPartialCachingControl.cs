namespace System.Web.UI
{
    using System;

    public class StaticPartialCachingControl : BasePartialCachingControl
    {
        private BuildMethod _buildMethod;

        public StaticPartialCachingControl(string ctrlID, string guid, int duration, string varyByParams, string varyByControls, string varyByCustom, BuildMethod buildMethod) : this(ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, null, buildMethod, null)
        {
        }

        public StaticPartialCachingControl(string ctrlID, string guid, int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, BuildMethod buildMethod) : this(ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, sqlDependency, buildMethod, null)
        {
        }

        public StaticPartialCachingControl(string ctrlID, string guid, int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, BuildMethod buildMethod, string providerName)
        {
            base._ctrlID = ctrlID;
            base.Duration = new TimeSpan(0, 0, duration);
            base.SetVaryByParamsCollectionFromString(varyByParams);
            if (varyByControls != null)
            {
                base._varyByControlsCollection = varyByControls.Split(new char[] { ';' });
            }
            base._varyByCustom = varyByCustom;
            base._guid = guid;
            this._buildMethod = buildMethod;
            base._sqlDependency = sqlDependency;
            base._provider = providerName;
        }

        public static void BuildCachedControl(Control parent, string ctrlID, string guid, int duration, string varyByParams, string varyByControls, string varyByCustom, BuildMethod buildMethod)
        {
            BuildCachedControl(parent, ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, null, buildMethod, null);
        }

        public static void BuildCachedControl(Control parent, string ctrlID, string guid, int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, BuildMethod buildMethod)
        {
            BuildCachedControl(parent, ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, sqlDependency, buildMethod, null);
        }

        public static void BuildCachedControl(Control parent, string ctrlID, string guid, int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, BuildMethod buildMethod, string providerName)
        {
            StaticPartialCachingControl control = new StaticPartialCachingControl(ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, sqlDependency, buildMethod, providerName);
            ((IParserAccessor) parent).AddParsedSubObject(control);
        }

        internal override Control CreateCachedControl()
        {
            return this._buildMethod();
        }
    }
}

