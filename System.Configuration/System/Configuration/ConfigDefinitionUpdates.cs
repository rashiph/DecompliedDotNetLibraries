namespace System.Configuration
{
    using System;
    using System.Collections;

    internal class ConfigDefinitionUpdates
    {
        private ArrayList _locationUpdatesList = new ArrayList();
        private bool _requireLocationWritten;

        internal ConfigDefinitionUpdates()
        {
        }

        internal DefinitionUpdate AddUpdate(OverrideModeSetting overrideMode, bool inheritInChildApps, bool moved, string updatedXml, SectionRecord sectionRecord)
        {
            LocationUpdates updates = this.FindLocationUpdates(overrideMode, inheritInChildApps);
            if (updates == null)
            {
                updates = new LocationUpdates(overrideMode, inheritInChildApps);
                this._locationUpdatesList.Add(updates);
            }
            DefinitionUpdate update = new DefinitionUpdate(sectionRecord.ConfigKey, moved, updatedXml, sectionRecord);
            updates.SectionUpdates.AddSection(update);
            return update;
        }

        internal void CompleteUpdates()
        {
            foreach (LocationUpdates updates in this._locationUpdatesList)
            {
                updates.CompleteUpdates();
            }
        }

        internal LocationUpdates FindLocationUpdates(OverrideModeSetting overrideMode, bool inheritInChildApps)
        {
            foreach (LocationUpdates updates in this._locationUpdatesList)
            {
                if (OverrideModeSetting.CanUseSameLocationTag(updates.OverrideMode, overrideMode) && (updates.InheritInChildApps == inheritInChildApps))
                {
                    return updates;
                }
            }
            return null;
        }

        internal void FlagLocationWritten()
        {
            this._requireLocationWritten = false;
        }

        internal ArrayList LocationUpdatesList
        {
            get
            {
                return this._locationUpdatesList;
            }
        }

        internal bool RequireLocation
        {
            get
            {
                return this._requireLocationWritten;
            }
            set
            {
                this._requireLocationWritten = value;
            }
        }
    }
}

