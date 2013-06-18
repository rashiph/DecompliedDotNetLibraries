namespace System.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("SectionRecord {ConfigKey}")]
    internal class SectionRecord
    {
        private string _configKey;
        private SectionInput _fileInput;
        private SafeBitVector32 _flags;
        private List<SectionInput> _indirectLocationInputs;
        private List<SectionInput> _locationInputs;
        private object _result;
        private object _resultRuntimeObject;
        private const int Flag_AddUpdate = 0x10000;
        private const int Flag_ChildrenLockWithoutFileInput = 0x40;
        private const int Flag_IndirectLocationInputLockApplied = 0x20;
        private const int Flag_IsResultTrustedWithoutAptca = 4;
        private const int Flag_LocationInputLockApplied = 0x10;
        private const int Flag_LockChildren = 2;
        private const int Flag_Locked = 1;
        private const int Flag_RequirePermission = 8;
        private static object s_unevaluated = new object();

        internal SectionRecord(string configKey)
        {
            this._configKey = configKey;
            this._result = s_unevaluated;
            this._resultRuntimeObject = s_unevaluated;
        }

        internal void AddFileInput(SectionInput sectionInput)
        {
            this._fileInput = sectionInput;
            if (!sectionInput.HasErrors && (sectionInput.SectionXmlInfo.OverrideModeSetting.OverrideMode != OverrideMode.Inherit))
            {
                this._flags[0x40] = this.LockChildren;
                this.ChangeLockSettings(OverrideMode.Inherit, sectionInput.SectionXmlInfo.OverrideModeSetting.OverrideMode);
            }
        }

        internal void AddIndirectLocationInput(SectionInput sectionInput)
        {
            this.AddLocationInputImpl(sectionInput, true);
        }

        internal void AddLocationInput(SectionInput sectionInput)
        {
            this.AddLocationInputImpl(sectionInput, false);
        }

        private void AddLocationInputImpl(SectionInput sectionInput, bool isIndirectLocation)
        {
            List<SectionInput> list = isIndirectLocation ? this._indirectLocationInputs : this._locationInputs;
            int num = isIndirectLocation ? 0x20 : 0x10;
            if (list == null)
            {
                list = new List<SectionInput>(1);
                if (isIndirectLocation)
                {
                    this._indirectLocationInputs = list;
                }
                else
                {
                    this._locationInputs = list;
                }
            }
            list.Insert(0, sectionInput);
            if (!sectionInput.HasErrors && !this._flags[num])
            {
                OverrideMode overrideMode = sectionInput.SectionXmlInfo.OverrideModeSetting.OverrideMode;
                if (overrideMode != OverrideMode.Inherit)
                {
                    this.ChangeLockSettings(overrideMode, overrideMode);
                    this._flags[num] = true;
                }
            }
        }

        internal void ChangeLockSettings(OverrideMode forSelf, OverrideMode forChildren)
        {
            if (forSelf != OverrideMode.Inherit)
            {
                this._flags[1] = forSelf == OverrideMode.Deny;
                this._flags[2] = forSelf == OverrideMode.Deny;
            }
            if (forChildren != OverrideMode.Inherit)
            {
                this._flags[2] = (forSelf == OverrideMode.Deny) || (forChildren == OverrideMode.Deny);
            }
        }

        internal void ClearRawXml()
        {
            if (this.HasLocationInputs)
            {
                foreach (SectionInput input in this.LocationInputs)
                {
                    input.SectionXmlInfo.RawXml = null;
                }
            }
            if (this.HasIndirectLocationInputs)
            {
                foreach (SectionInput input2 in this.IndirectLocationInputs)
                {
                    input2.SectionXmlInfo.RawXml = null;
                }
            }
            if (this.HasFileInput)
            {
                this.FileInput.SectionXmlInfo.RawXml = null;
            }
        }

        internal void ClearResult()
        {
            if (this._fileInput != null)
            {
                this._fileInput.ClearResult();
            }
            if (this._locationInputs != null)
            {
                foreach (SectionInput input in this._locationInputs)
                {
                    input.ClearResult();
                }
            }
            this._result = s_unevaluated;
            this._resultRuntimeObject = s_unevaluated;
        }

        private List<ConfigurationException> GetAllErrors()
        {
            List<ConfigurationException> errors = null;
            if (this.HasLocationInputs)
            {
                foreach (SectionInput input in this.LocationInputs)
                {
                    ErrorsHelper.AddErrors(ref errors, input.Errors);
                }
            }
            if (this.HasIndirectLocationInputs)
            {
                foreach (SectionInput input2 in this.IndirectLocationInputs)
                {
                    ErrorsHelper.AddErrors(ref errors, input2.Errors);
                }
            }
            if (this.HasFileInput)
            {
                ErrorsHelper.AddErrors(ref errors, this.FileInput.Errors);
            }
            return errors;
        }

        internal void RemoveFileInput()
        {
            if (this._fileInput != null)
            {
                this._fileInput = null;
                this._flags[2] = this.Locked;
            }
        }

        internal void ThrowOnErrors()
        {
            if (this.HasErrors)
            {
                throw new ConfigurationErrorsException(this.GetAllErrors());
            }
        }

        internal bool AddUpdate
        {
            get
            {
                return this._flags[0x10000];
            }
            set
            {
                this._flags[0x10000] = value;
            }
        }

        internal string ConfigKey
        {
            get
            {
                return this._configKey;
            }
        }

        internal SectionInput FileInput
        {
            get
            {
                return this._fileInput;
            }
        }

        internal bool HasErrors
        {
            get
            {
                if (this.HasLocationInputs)
                {
                    foreach (SectionInput input in this.LocationInputs)
                    {
                        if (input.HasErrors)
                        {
                            return true;
                        }
                    }
                }
                if (this.HasIndirectLocationInputs)
                {
                    foreach (SectionInput input2 in this.IndirectLocationInputs)
                    {
                        if (input2.HasErrors)
                        {
                            return true;
                        }
                    }
                }
                return (this.HasFileInput && this.FileInput.HasErrors);
            }
        }

        internal bool HasFileInput
        {
            get
            {
                return (this._fileInput != null);
            }
        }

        internal bool HasIndirectLocationInputs
        {
            get
            {
                return ((this._indirectLocationInputs != null) && (this._indirectLocationInputs.Count > 0));
            }
        }

        internal bool HasInput
        {
            get
            {
                if (!this.HasLocationInputs && !this.HasFileInput)
                {
                    return this.HasIndirectLocationInputs;
                }
                return true;
            }
        }

        internal bool HasLocationInputs
        {
            get
            {
                return ((this._locationInputs != null) && (this._locationInputs.Count > 0));
            }
        }

        internal bool HasResult
        {
            get
            {
                return (this._result != s_unevaluated);
            }
        }

        internal bool HasResultRuntimeObject
        {
            get
            {
                return (this._resultRuntimeObject != s_unevaluated);
            }
        }

        internal List<SectionInput> IndirectLocationInputs
        {
            get
            {
                return this._indirectLocationInputs;
            }
        }

        internal bool IsResultTrustedWithoutAptca
        {
            get
            {
                return this._flags[4];
            }
            set
            {
                this._flags[4] = value;
            }
        }

        internal SectionInput LastIndirectLocationInput
        {
            get
            {
                if (this.HasIndirectLocationInputs)
                {
                    return this._indirectLocationInputs[this._indirectLocationInputs.Count - 1];
                }
                return null;
            }
        }

        internal SectionInput LastLocationInput
        {
            get
            {
                if (this.HasLocationInputs)
                {
                    return this._locationInputs[this._locationInputs.Count - 1];
                }
                return null;
            }
        }

        internal List<SectionInput> LocationInputs
        {
            get
            {
                return this._locationInputs;
            }
        }

        internal bool LockChildren
        {
            get
            {
                return this._flags[2];
            }
        }

        internal bool LockChildrenWithoutFileInput
        {
            get
            {
                bool lockChildren = this.LockChildren;
                if (this.HasFileInput)
                {
                    lockChildren = this._flags[0x40];
                }
                return lockChildren;
            }
        }

        internal bool Locked
        {
            get
            {
                return this._flags[1];
            }
        }

        internal bool RequirePermission
        {
            get
            {
                return this._flags[8];
            }
            set
            {
                this._flags[8] = value;
            }
        }

        internal object Result
        {
            get
            {
                return this._result;
            }
            set
            {
                this._result = value;
            }
        }

        internal object ResultRuntimeObject
        {
            get
            {
                return this._resultRuntimeObject;
            }
            set
            {
                this._resultRuntimeObject = value;
            }
        }
    }
}

