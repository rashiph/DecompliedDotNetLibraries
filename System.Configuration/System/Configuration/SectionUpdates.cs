namespace System.Configuration
{
    using System;
    using System.Collections;

    internal class SectionUpdates
    {
        private int _cMoved;
        private int _cUnretrieved;
        private Hashtable _groups;
        private bool _isNew;
        private string _name;
        private Update _sectionGroupUpdate;
        private Hashtable _sections;

        internal SectionUpdates(string name)
        {
            this._name = name;
            this._groups = new Hashtable();
            this._sections = new Hashtable();
        }

        internal void AddSection(Update update)
        {
            SectionUpdates updates = this.FindSectionUpdates(update.ConfigKey, false);
            updates._sections.Add(update.ConfigKey, update);
            updates._cUnretrieved++;
            if (update.Moved)
            {
                updates._cMoved++;
            }
        }

        internal void AddSectionGroup(Update update)
        {
            this.FindSectionUpdates(update.ConfigKey, true)._sectionGroupUpdate = update;
        }

        internal void CompleteUpdates()
        {
            bool flag = true;
            foreach (SectionUpdates updates in this._groups.Values)
            {
                updates.CompleteUpdates();
                if (!updates.IsNew)
                {
                    flag = false;
                }
            }
            this._isNew = flag && (this._cMoved == this._sections.Count);
        }

        private SectionUpdates FindSectionUpdates(string configKey, bool isGroup)
        {
            string str;
            if (isGroup)
            {
                str = configKey;
            }
            else
            {
                string str2;
                BaseConfigurationRecord.SplitConfigKey(configKey, out str, out str2);
            }
            SectionUpdates updates = this;
            if (str.Length != 0)
            {
                foreach (string str3 in str.Split(BaseConfigurationRecord.ConfigPathSeparatorParams))
                {
                    SectionUpdates updates2 = (SectionUpdates) updates._groups[str3];
                    if (updates2 == null)
                    {
                        updates2 = new SectionUpdates(str3);
                        updates._groups[str3] = updates2;
                    }
                    updates = updates2;
                }
            }
            return updates;
        }

        internal DeclarationUpdate GetDeclarationUpdate(string configKey)
        {
            return (DeclarationUpdate) this.GetUpdate(configKey);
        }

        internal DefinitionUpdate GetDefinitionUpdate(string configKey)
        {
            return (DefinitionUpdate) this.GetUpdate(configKey);
        }

        internal string[] GetMovedSectionNames()
        {
            if (this._cMoved == 0)
            {
                return null;
            }
            string[] array = new string[this._cMoved];
            int index = 0;
            foreach (Update update in this._sections.Values)
            {
                if (update.Moved && !update.Retrieved)
                {
                    array[index] = update.ConfigKey;
                    index++;
                }
            }
            Array.Sort<string>(array);
            return array;
        }

        internal string[] GetNewGroupNames()
        {
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry entry in this._groups)
            {
                string key = (string) entry.Key;
                SectionUpdates updates = (SectionUpdates) entry.Value;
                if (updates.IsNew && updates.HasUnretrievedSections())
                {
                    list.Add(key);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            string[] array = new string[list.Count];
            list.CopyTo(array);
            Array.Sort<string>(array);
            return array;
        }

        internal DeclarationUpdate GetSectionGroupUpdate()
        {
            if ((this._sectionGroupUpdate != null) && !this._sectionGroupUpdate.Retrieved)
            {
                this._sectionGroupUpdate.Retrieved = true;
                return (DeclarationUpdate) this._sectionGroupUpdate;
            }
            return null;
        }

        internal SectionUpdates GetSectionUpdatesForGroup(string group)
        {
            return (SectionUpdates) this._groups[group];
        }

        internal string[] GetUnretrievedGroupNames()
        {
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry entry in this._groups)
            {
                string key = (string) entry.Key;
                SectionUpdates updates = (SectionUpdates) entry.Value;
                if (updates.HasUnretrievedSections())
                {
                    list.Add(key);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            string[] array = new string[list.Count];
            list.CopyTo(array);
            Array.Sort<string>(array);
            return array;
        }

        internal string[] GetUnretrievedSectionNames()
        {
            if (this._cUnretrieved == 0)
            {
                return null;
            }
            string[] array = new string[this._cUnretrieved];
            int index = 0;
            foreach (Update update in this._sections.Values)
            {
                if (!update.Retrieved)
                {
                    array[index] = update.ConfigKey;
                    index++;
                }
            }
            Array.Sort<string>(array);
            return array;
        }

        private Update GetUpdate(string configKey)
        {
            Update update = (Update) this._sections[configKey];
            if (update != null)
            {
                if (update.Retrieved)
                {
                    return null;
                }
                update.Retrieved = true;
                this._cUnretrieved--;
                if (update.Moved)
                {
                    this._cMoved--;
                }
            }
            return update;
        }

        internal bool HasNewSectionGroups()
        {
            foreach (SectionUpdates updates in this._groups.Values)
            {
                if (updates.IsNew)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasUnretrievedSections()
        {
            if ((this._cUnretrieved > 0) || ((this._sectionGroupUpdate != null) && !this._sectionGroupUpdate.Retrieved))
            {
                return true;
            }
            foreach (SectionUpdates updates in this._groups.Values)
            {
                if (updates.HasUnretrievedSections())
                {
                    return true;
                }
            }
            return false;
        }

        internal void MarkAsRetrieved()
        {
            this._cUnretrieved = 0;
            foreach (SectionUpdates updates in this._groups.Values)
            {
                updates.MarkAsRetrieved();
            }
            if (this._sectionGroupUpdate != null)
            {
                this._sectionGroupUpdate.Retrieved = true;
            }
        }

        internal void MarkGroupAsRetrieved(string groupName)
        {
            SectionUpdates updates = this._groups[groupName] as SectionUpdates;
            if (updates != null)
            {
                updates.MarkAsRetrieved();
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return ((this._groups.Count == 0) && (this._sections.Count == 0));
            }
        }

        internal bool IsNew
        {
            get
            {
                return this._isNew;
            }
            set
            {
                this._isNew = value;
            }
        }
    }
}

