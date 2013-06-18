namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    internal sealed class BlobPersonalizationState : PersonalizationState
    {
        private IDictionary _extractedState;
        private bool _isPostRequest;
        private IDictionary _personalizedControls;
        private byte[] _rawUserData;
        private IDictionary _sharedState;
        private IDictionary _userState;
        private const int PersonalizationVersion = 2;
        private const string WebPartManagerPersonalizationID = "__wpm";

        public BlobPersonalizationState(WebPartManager webPartManager) : base(webPartManager)
        {
            this._isPostRequest = webPartManager.Page.Request.HttpVerb == HttpVerb.POST;
        }

        private void ApplyPersonalization(Control control, string personalizationID, bool isWebPartManager, System.Web.UI.WebControls.WebParts.PersonalizationScope extractScope, GenericWebPart genericWebPart)
        {
            if (this._personalizedControls == null)
            {
                this._personalizedControls = new HybridDictionary(false);
            }
            else if (this._personalizedControls.Contains(personalizationID))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_CantApply", new object[] { personalizationID }));
            }
            IDictionary personalizablePropertyEntries = PersonalizableAttribute.GetPersonalizablePropertyEntries(control.GetType());
            if (this.SharedState == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotLoaded"));
            }
            PersonalizationInfo sharedInfo = (PersonalizationInfo) this.SharedState[personalizationID];
            PersonalizationInfo userInfo = null;
            IDictionary dictionary2 = null;
            IDictionary dictionary3 = null;
            PersonalizationDictionary customInitialProperties = null;
            ControlInfo info3 = new ControlInfo {
                _allowSetDirty = false
            };
            this._personalizedControls[personalizationID] = info3;
            if (((sharedInfo != null) && sharedInfo._isStatic) && !sharedInfo.IsMatchingControlType(control))
            {
                sharedInfo = null;
                if (this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared)
                {
                    this.SetControlDirty(control, personalizationID, isWebPartManager, true);
                }
            }
            IPersonalizable personalizable = control as IPersonalizable;
            ITrackingPersonalizable personalizable2 = control as ITrackingPersonalizable;
            WebPart hasDataWebPart = null;
            if (!isWebPartManager)
            {
                if (genericWebPart != null)
                {
                    hasDataWebPart = genericWebPart;
                }
                else
                {
                    hasDataWebPart = (WebPart) control;
                }
            }
            try
            {
                if (personalizable2 != null)
                {
                    personalizable2.BeginLoad();
                }
                if (this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User)
                {
                    if (this.UserState == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotLoaded"));
                    }
                    userInfo = (PersonalizationInfo) this.UserState[personalizationID];
                    if (((userInfo != null) && userInfo._isStatic) && !userInfo.IsMatchingControlType(control))
                    {
                        userInfo = null;
                        this.SetControlDirty(control, personalizationID, isWebPartManager, true);
                    }
                    if (personalizable != null)
                    {
                        PersonalizationDictionary state = this.MergeCustomProperties(sharedInfo, userInfo, isWebPartManager, hasDataWebPart, ref customInitialProperties);
                        if (state != null)
                        {
                            info3._allowSetDirty = true;
                            personalizable.Load(state);
                            info3._allowSetDirty = false;
                        }
                    }
                    if (!isWebPartManager)
                    {
                        IDictionary dictionary6 = null;
                        IDictionary dictionary7 = null;
                        if (sharedInfo != null)
                        {
                            IDictionary propertyState = sharedInfo._properties;
                            if ((propertyState != null) && (propertyState.Count != 0))
                            {
                                hasDataWebPart.SetHasSharedData(true);
                                dictionary6 = SetPersonalizedProperties(control, personalizablePropertyEntries, propertyState, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared);
                            }
                        }
                        dictionary2 = GetPersonalizedProperties(control, personalizablePropertyEntries, null, null, extractScope);
                        if (userInfo != null)
                        {
                            IDictionary dictionary9 = userInfo._properties;
                            if ((dictionary9 != null) && (dictionary9.Count != 0))
                            {
                                hasDataWebPart.SetHasUserData(true);
                                dictionary7 = SetPersonalizedProperties(control, personalizablePropertyEntries, dictionary9, extractScope);
                            }
                            if ((personalizable2 == null) || !personalizable2.TracksChanges)
                            {
                                dictionary3 = dictionary9;
                            }
                        }
                        if ((dictionary6 != null) || (dictionary7 != null))
                        {
                            IVersioningPersonalizable personalizable3 = control as IVersioningPersonalizable;
                            if (personalizable3 != null)
                            {
                                IDictionary unknownProperties = null;
                                if (dictionary6 != null)
                                {
                                    unknownProperties = dictionary6;
                                    if (dictionary7 != null)
                                    {
                                        foreach (DictionaryEntry entry in dictionary7)
                                        {
                                            unknownProperties[entry.Key] = entry.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    unknownProperties = dictionary7;
                                }
                                info3._allowSetDirty = true;
                                personalizable3.Load(unknownProperties);
                                info3._allowSetDirty = false;
                            }
                            else
                            {
                                this.SetControlDirty(control, personalizationID, isWebPartManager, true);
                            }
                        }
                    }
                }
                else
                {
                    if (personalizable != null)
                    {
                        PersonalizationDictionary dictionary11 = this.MergeCustomProperties(sharedInfo, userInfo, isWebPartManager, hasDataWebPart, ref customInitialProperties);
                        if (dictionary11 != null)
                        {
                            info3._allowSetDirty = true;
                            personalizable.Load(dictionary11);
                            info3._allowSetDirty = false;
                        }
                    }
                    if (!isWebPartManager)
                    {
                        IDictionary dictionary12 = null;
                        dictionary2 = GetPersonalizedProperties(control, personalizablePropertyEntries, null, null, extractScope);
                        if (sharedInfo != null)
                        {
                            IDictionary dictionary13 = sharedInfo._properties;
                            if ((dictionary13 != null) && (dictionary13.Count != 0))
                            {
                                hasDataWebPart.SetHasSharedData(true);
                                dictionary12 = SetPersonalizedProperties(control, personalizablePropertyEntries, dictionary13, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared);
                            }
                            if ((personalizable2 == null) || !personalizable2.TracksChanges)
                            {
                                dictionary3 = dictionary13;
                            }
                        }
                        if (dictionary12 != null)
                        {
                            IVersioningPersonalizable personalizable4 = control as IVersioningPersonalizable;
                            if (personalizable4 != null)
                            {
                                info3._allowSetDirty = true;
                                personalizable4.Load(dictionary12);
                                info3._allowSetDirty = false;
                            }
                            else
                            {
                                this.SetControlDirty(control, personalizationID, isWebPartManager, true);
                            }
                        }
                    }
                }
            }
            finally
            {
                info3._allowSetDirty = true;
                if (personalizable2 != null)
                {
                    personalizable2.EndLoad();
                }
            }
            info3._control = control;
            info3._personalizableProperties = personalizablePropertyEntries;
            info3._defaultProperties = dictionary2;
            info3._initialProperties = dictionary3;
            info3._customInitialProperties = customInitialProperties;
        }

        public override void ApplyWebPartManagerPersonalization()
        {
            this.ApplyPersonalization(base.WebPartManager, "__wpm", true, this.PersonalizationScope, null);
        }

        public override void ApplyWebPartPersonalization(WebPart webPart)
        {
            base.ValidateWebPart(webPart);
            if (!(webPart is UnauthorizedWebPart))
            {
                string personalizationID = this.CreatePersonalizationID(webPart, null);
                System.Web.UI.WebControls.WebParts.PersonalizationScope personalizationScope = this.PersonalizationScope;
                if ((personalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User) && !webPart.IsShared)
                {
                    personalizationScope = System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared;
                }
                this.ApplyPersonalization(webPart, personalizationID, false, personalizationScope, null);
                GenericWebPart associatedGenericWebPart = webPart as GenericWebPart;
                if (associatedGenericWebPart != null)
                {
                    Control childControl = associatedGenericWebPart.ChildControl;
                    personalizationID = this.CreatePersonalizationID(childControl, associatedGenericWebPart);
                    this.ApplyPersonalization(childControl, personalizationID, false, personalizationScope, associatedGenericWebPart);
                }
            }
        }

        private bool CompareProperties(IDictionary newProperties, IDictionary oldProperties)
        {
            int count = 0;
            int num2 = 0;
            if (newProperties != null)
            {
                count = newProperties.Count;
            }
            if (oldProperties != null)
            {
                num2 = oldProperties.Count;
            }
            if (count != num2)
            {
                return true;
            }
            if (count != 0)
            {
                foreach (DictionaryEntry entry in newProperties)
                {
                    object key = entry.Key;
                    object objA = entry.Value;
                    if (oldProperties.Contains(key))
                    {
                        object objB = oldProperties[key];
                        if (!object.Equals(objA, objB))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string CreatePersonalizationID(string ID, string genericWebPartID)
        {
            if (!string.IsNullOrEmpty(genericWebPartID))
            {
                return (ID + '$' + genericWebPartID);
            }
            return ID;
        }

        private string CreatePersonalizationID(Control control, WebPart associatedGenericWebPart)
        {
            if (associatedGenericWebPart != null)
            {
                return this.CreatePersonalizationID(control.ID, associatedGenericWebPart.ID);
            }
            return this.CreatePersonalizationID(control.ID, null);
        }

        private static IDictionary DeserializeData(byte[] data)
        {
            IDictionary dictionary = null;
            if ((data != null) && (data.Length > 0))
            {
                Exception innerException = null;
                int num = -1;
                object[] objArray = null;
                int num2 = 0;
                try
                {
                    ObjectStateFormatter formatter = new ObjectStateFormatter(null, false);
                    if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
                    {
                        HttpRuntime.NamedPermissionSet.PermitOnly();
                    }
                    objArray = (object[]) formatter.DeserializeWithAssert(new MemoryStream(data));
                    if ((objArray != null) && (objArray.Length != 0))
                    {
                        num = (int) objArray[num2++];
                    }
                }
                catch (Exception exception2)
                {
                    innerException = exception2;
                }
                switch (num)
                {
                    case 1:
                    case 2:
                        try
                        {
                            int initialSize = (int) objArray[num2++];
                            if (initialSize > 0)
                            {
                                dictionary = new HybridDictionary(initialSize, false);
                            }
                            for (int i = 0; i < initialSize; i++)
                            {
                                string str;
                                bool flag;
                                Type type = null;
                                VirtualPath path = null;
                                object obj2 = objArray[num2++];
                                if (obj2 is string)
                                {
                                    str = (string) obj2;
                                    flag = false;
                                }
                                else
                                {
                                    type = (Type) obj2;
                                    if (type == typeof(UserControl))
                                    {
                                        path = VirtualPath.CreateNonRelativeAllowNull((string) objArray[num2++]);
                                    }
                                    str = (string) objArray[num2++];
                                    flag = true;
                                }
                                IDictionary dictionary2 = null;
                                int num5 = (int) objArray[num2++];
                                if (num5 > 0)
                                {
                                    dictionary2 = new HybridDictionary(num5, false);
                                    for (int j = 0; j < num5; j++)
                                    {
                                        string str2 = ((IndexedString) objArray[num2++]).Value;
                                        object obj3 = objArray[num2++];
                                        dictionary2[str2] = obj3;
                                    }
                                }
                                PersonalizationDictionary dictionary3 = null;
                                int num7 = (int) objArray[num2++];
                                if (num7 > 0)
                                {
                                    dictionary3 = new PersonalizationDictionary(num7);
                                    for (int k = 0; k < num7; k++)
                                    {
                                        string str3 = ((IndexedString) objArray[num2++]).Value;
                                        object obj4 = objArray[num2++];
                                        System.Web.UI.WebControls.WebParts.PersonalizationScope scope = ((bool) objArray[num2++]) ? System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared : System.Web.UI.WebControls.WebParts.PersonalizationScope.User;
                                        bool isSensitive = false;
                                        if (num == 2)
                                        {
                                            isSensitive = (bool) objArray[num2++];
                                        }
                                        dictionary3[str3] = new PersonalizationEntry(obj4, scope, isSensitive);
                                    }
                                }
                                PersonalizationInfo info = new PersonalizationInfo {
                                    _controlID = str,
                                    _controlType = type,
                                    _controlVPath = path,
                                    _isStatic = flag,
                                    _properties = dictionary2,
                                    _customProperties = dictionary3
                                };
                                dictionary[str] = info;
                            }
                        }
                        catch (Exception exception3)
                        {
                            innerException = exception3;
                        }
                        break;
                }
                if ((innerException != null) || ((num != 1) && (num != 2)))
                {
                    throw new ArgumentException(System.Web.SR.GetString("BlobPersonalizationState_DeserializeError"), "data", innerException);
                }
            }
            if (dictionary == null)
            {
                dictionary = new HybridDictionary(false);
            }
            return dictionary;
        }

        private void ExtractPersonalization(Control control, string personalizationID, bool isWebPartManager, System.Web.UI.WebControls.WebParts.PersonalizationScope scope, bool isStatic, GenericWebPart genericWebPart)
        {
            if (this._extractedState == null)
            {
                this._extractedState = new HybridDictionary(false);
            }
            if (this._personalizedControls == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotApplied"));
            }
            ControlInfo info = (ControlInfo) this._personalizedControls[personalizationID];
            if (info == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_CantExtract", new object[] { personalizationID }));
            }
            ITrackingPersonalizable personalizable = control as ITrackingPersonalizable;
            IPersonalizable personalizable2 = control as IPersonalizable;
            IDictionary dictionary = info._initialProperties;
            PersonalizationDictionary dictionary2 = info._customInitialProperties;
            bool flag = false;
            try
            {
                if (personalizable != null)
                {
                    personalizable.BeginSave();
                }
                if (!this.IsPostRequest)
                {
                    if (info._dirty)
                    {
                        if (personalizable2 != null)
                        {
                            PersonalizationDictionary state = new PersonalizationDictionary();
                            personalizable2.Save(state);
                            if ((state.Count != 0) || ((dictionary2 != null) && (dictionary2.Count != 0)))
                            {
                                if (scope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User)
                                {
                                    state.RemoveSharedProperties();
                                }
                                dictionary2 = state;
                            }
                        }
                        if (!isWebPartManager)
                        {
                            dictionary = GetPersonalizedProperties(control, info._personalizableProperties, info._defaultProperties, info._initialProperties, scope);
                        }
                        flag = true;
                    }
                }
                else
                {
                    bool flag2 = true;
                    bool flag3 = true;
                    if (info._dirty)
                    {
                        flag3 = false;
                    }
                    else if (((personalizable != null) && personalizable.TracksChanges) && !info._dirty)
                    {
                        flag2 = false;
                    }
                    if (flag2)
                    {
                        if ((personalizable2 != null) && (info._dirty || personalizable2.IsDirty))
                        {
                            PersonalizationDictionary dictionary4 = new PersonalizationDictionary();
                            personalizable2.Save(dictionary4);
                            if ((dictionary4.Count != 0) || ((dictionary2 != null) && (dictionary2.Count != 0)))
                            {
                                if (dictionary4.Count != 0)
                                {
                                    if (scope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User)
                                    {
                                        dictionary4.RemoveSharedProperties();
                                    }
                                    dictionary2 = dictionary4;
                                }
                                else
                                {
                                    dictionary2 = null;
                                }
                                flag3 = false;
                                flag = true;
                            }
                        }
                        if (!isWebPartManager)
                        {
                            IDictionary newProperties = GetPersonalizedProperties(control, info._personalizableProperties, info._defaultProperties, info._initialProperties, scope);
                            if (flag3 && !this.CompareProperties(newProperties, info._initialProperties))
                            {
                                flag2 = false;
                            }
                            if (flag2)
                            {
                                dictionary = newProperties;
                                flag = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (personalizable != null)
                {
                    personalizable.EndSave();
                }
            }
            PersonalizationInfo info2 = new PersonalizationInfo {
                _controlID = personalizationID
            };
            if (isStatic)
            {
                UserControl control2 = control as UserControl;
                if (control2 != null)
                {
                    info2._controlType = typeof(UserControl);
                    info2._controlVPath = control2.TemplateControlVirtualPath;
                }
                else
                {
                    info2._controlType = control.GetType();
                }
            }
            info2._isStatic = isStatic;
            info2._properties = dictionary;
            info2._customProperties = dictionary2;
            this._extractedState[personalizationID] = info2;
            if (flag)
            {
                base.SetDirty();
            }
            if (((dictionary != null) && (dictionary.Count > 0)) || ((dictionary2 != null) && (dictionary2.Count > 0)))
            {
                WebPart part = null;
                if (!isWebPartManager)
                {
                    if (genericWebPart != null)
                    {
                        part = genericWebPart;
                    }
                    else
                    {
                        part = (WebPart) control;
                    }
                }
                if (part != null)
                {
                    if (this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared)
                    {
                        part.SetHasSharedData(true);
                    }
                    else
                    {
                        part.SetHasUserData(true);
                    }
                }
            }
        }

        public override void ExtractWebPartManagerPersonalization()
        {
            this.ExtractPersonalization(base.WebPartManager, "__wpm", true, this.PersonalizationScope, true, null);
        }

        public override void ExtractWebPartPersonalization(WebPart webPart)
        {
            base.ValidateWebPart(webPart);
            ProxyWebPart part = webPart as ProxyWebPart;
            if (part != null)
            {
                this.RoundTripWebPartPersonalization(part.OriginalID, part.GenericWebPartID);
            }
            else
            {
                System.Web.UI.WebControls.WebParts.PersonalizationScope personalizationScope = this.PersonalizationScope;
                if ((personalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User) && !webPart.IsShared)
                {
                    personalizationScope = System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared;
                }
                bool isStatic = webPart.IsStatic;
                string personalizationID = this.CreatePersonalizationID(webPart, null);
                this.ExtractPersonalization(webPart, personalizationID, false, personalizationScope, isStatic, null);
                GenericWebPart associatedGenericWebPart = webPart as GenericWebPart;
                if (associatedGenericWebPart != null)
                {
                    Control childControl = associatedGenericWebPart.ChildControl;
                    personalizationID = this.CreatePersonalizationID(childControl, associatedGenericWebPart);
                    this.ExtractPersonalization(childControl, personalizationID, false, personalizationScope, isStatic, associatedGenericWebPart);
                }
            }
        }

        public override string GetAuthorizationFilter(string webPartID)
        {
            if (string.IsNullOrEmpty(webPartID))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("webPartID");
            }
            return (this.GetPersonalizedValue(webPartID, "AuthorizationFilter") as string);
        }

        internal static IDictionary GetPersonalizedProperties(Control control, System.Web.UI.WebControls.WebParts.PersonalizationScope scope)
        {
            IDictionary personalizablePropertyEntries = PersonalizableAttribute.GetPersonalizablePropertyEntries(control.GetType());
            return GetPersonalizedProperties(control, personalizablePropertyEntries, null, null, scope);
        }

        private static IDictionary GetPersonalizedProperties(Control control, IDictionary personalizableProperties, IDictionary defaultPropertyState, IDictionary initialPropertyState, System.Web.UI.WebControls.WebParts.PersonalizationScope scope)
        {
            if (personalizableProperties.Count == 0)
            {
                return null;
            }
            bool flag = scope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User;
            IDictionary dictionary = null;
            foreach (DictionaryEntry entry in personalizableProperties)
            {
                PersonalizablePropertyEntry entry2 = (PersonalizablePropertyEntry) entry.Value;
                if (!flag || (entry2.Scope != System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    PropertyInfo propertyInfo = entry2.PropertyInfo;
                    string key = (string) entry.Key;
                    object objA = FastPropertyAccessor.GetProperty(control, key, control.DesignMode);
                    bool flag2 = true;
                    if (((initialPropertyState == null) || !initialPropertyState.Contains(key)) && (defaultPropertyState != null))
                    {
                        object objB = defaultPropertyState[key];
                        if (object.Equals(objA, objB))
                        {
                            flag2 = false;
                        }
                    }
                    if (flag2)
                    {
                        if (dictionary == null)
                        {
                            dictionary = new HybridDictionary(personalizableProperties.Count, false);
                        }
                        dictionary[key] = objA;
                    }
                }
            }
            return dictionary;
        }

        private object GetPersonalizedValue(string personalizationID, string propertyName)
        {
            if (this.SharedState == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotLoaded"));
            }
            PersonalizationInfo info = (PersonalizationInfo) this.SharedState[personalizationID];
            IDictionary dictionary = (info != null) ? info._properties : null;
            if (this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared)
            {
                if (dictionary != null)
                {
                    return dictionary[propertyName];
                }
            }
            else
            {
                if (this.UserState == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotLoaded"));
                }
                PersonalizationInfo info2 = (PersonalizationInfo) this.UserState[personalizationID];
                IDictionary dictionary2 = (info2 != null) ? info2._properties : null;
                if ((dictionary2 != null) && dictionary2.Contains(propertyName))
                {
                    return dictionary2[propertyName];
                }
                if (dictionary != null)
                {
                    return dictionary[propertyName];
                }
            }
            return null;
        }

        public void LoadDataBlobs(byte[] sharedData, byte[] userData)
        {
            this._sharedState = DeserializeData(sharedData);
            this._rawUserData = userData;
        }

        private PersonalizationDictionary MergeCustomProperties(PersonalizationInfo sharedInfo, PersonalizationInfo userInfo, bool isWebPartManager, WebPart hasDataWebPart, ref PersonalizationDictionary customInitialProperties)
        {
            PersonalizationDictionary dictionary = null;
            bool flag = (sharedInfo != null) && (sharedInfo._customProperties != null);
            bool flag2 = (userInfo != null) && (userInfo._customProperties != null);
            if (flag && flag2)
            {
                dictionary = new PersonalizationDictionary();
                foreach (DictionaryEntry entry in sharedInfo._customProperties)
                {
                    dictionary[(string) entry.Key] = (PersonalizationEntry) entry.Value;
                }
                foreach (DictionaryEntry entry2 in userInfo._customProperties)
                {
                    dictionary[(string) entry2.Key] = (PersonalizationEntry) entry2.Value;
                }
            }
            else if (flag)
            {
                dictionary = sharedInfo._customProperties;
            }
            else if (flag2)
            {
                dictionary = userInfo._customProperties;
            }
            if ((this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared) && flag)
            {
                customInitialProperties = sharedInfo._customProperties;
            }
            else if ((this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User) && flag2)
            {
                customInitialProperties = userInfo._customProperties;
            }
            if (flag && !isWebPartManager)
            {
                hasDataWebPart.SetHasSharedData(true);
            }
            if (flag2 && !isWebPartManager)
            {
                hasDataWebPart.SetHasUserData(true);
            }
            return dictionary;
        }

        private void RoundTripWebPartPersonalization(string personalizationID)
        {
            if (this.PersonalizationScope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared)
            {
                if (this.SharedState == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotLoaded"));
                }
                if (this.SharedState.Contains(personalizationID))
                {
                    this._extractedState[personalizationID] = (PersonalizationInfo) this.SharedState[personalizationID];
                }
            }
            else
            {
                if (this.UserState == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotLoaded"));
                }
                if (this.UserState.Contains(personalizationID))
                {
                    this._extractedState[personalizationID] = (PersonalizationInfo) this.UserState[personalizationID];
                }
            }
        }

        private void RoundTripWebPartPersonalization(string ID, string genericWebPartID)
        {
            if (string.IsNullOrEmpty(ID))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("ID");
            }
            string personalizationID = this.CreatePersonalizationID(ID, genericWebPartID);
            this.RoundTripWebPartPersonalization(personalizationID);
            if (!string.IsNullOrEmpty(genericWebPartID))
            {
                string str2 = this.CreatePersonalizationID(genericWebPartID, null);
                this.RoundTripWebPartPersonalization(str2);
            }
        }

        public byte[] SaveDataBlob()
        {
            return SerializeData(this._extractedState);
        }

        private static byte[] SerializeData(IDictionary data)
        {
            byte[] buffer = null;
            if ((data == null) || (data.Count == 0))
            {
                return buffer;
            }
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry entry in data)
            {
                PersonalizationInfo info = (PersonalizationInfo) entry.Value;
                if (((info._properties != null) && (info._properties.Count != 0)) || ((info._customProperties != null) && (info._customProperties.Count != 0)))
                {
                    list.Add(info);
                }
            }
            if (list.Count == 0)
            {
                return buffer;
            }
            ArrayList list2 = new ArrayList();
            list2.Add(2);
            list2.Add(list.Count);
            foreach (PersonalizationInfo info2 in list)
            {
                if (info2._isStatic)
                {
                    list2.Add(info2._controlType);
                    if (info2._controlVPath != null)
                    {
                        list2.Add(info2._controlVPath.AppRelativeVirtualPathString);
                    }
                }
                list2.Add(info2._controlID);
                int count = 0;
                if (info2._properties != null)
                {
                    count = info2._properties.Count;
                }
                list2.Add(count);
                if (count != 0)
                {
                    foreach (DictionaryEntry entry2 in info2._properties)
                    {
                        list2.Add(new IndexedString((string) entry2.Key));
                        list2.Add(entry2.Value);
                    }
                }
                int num2 = 0;
                if (info2._customProperties != null)
                {
                    num2 = info2._customProperties.Count;
                }
                list2.Add(num2);
                if (num2 != 0)
                {
                    foreach (DictionaryEntry entry3 in info2._customProperties)
                    {
                        list2.Add(new IndexedString((string) entry3.Key));
                        PersonalizationEntry entry4 = (PersonalizationEntry) entry3.Value;
                        list2.Add(entry4.Value);
                        list2.Add(entry4.Scope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared);
                        list2.Add(entry4.IsSensitive);
                    }
                }
            }
            if (list2.Count == 0)
            {
                return buffer;
            }
            ObjectStateFormatter formatter = new ObjectStateFormatter(null, false);
            MemoryStream outputStream = new MemoryStream(0x400);
            object[] stateGraph = list2.ToArray();
            if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
            {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }
            formatter.SerializeWithAssert(outputStream, stateGraph);
            return outputStream.ToArray();
        }

        private void SetControlDirty(Control control, string personalizationID, bool isWebPartManager, bool forceSetDirty)
        {
            if (this._personalizedControls == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("BlobPersonalizationState_NotApplied"));
            }
            ControlInfo info = (ControlInfo) this._personalizedControls[personalizationID];
            if ((info != null) && (forceSetDirty || info._allowSetDirty))
            {
                info._dirty = true;
            }
        }

        internal static IDictionary SetPersonalizedProperties(Control control, IDictionary propertyState)
        {
            IDictionary personalizablePropertyEntries = PersonalizableAttribute.GetPersonalizablePropertyEntries(control.GetType());
            return SetPersonalizedProperties(control, personalizablePropertyEntries, propertyState, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared);
        }

        private static IDictionary SetPersonalizedProperties(Control control, IDictionary personalizableProperties, IDictionary propertyState, System.Web.UI.WebControls.WebParts.PersonalizationScope scope)
        {
            if (personalizableProperties.Count == 0)
            {
                return propertyState;
            }
            if ((propertyState == null) || (propertyState.Count == 0))
            {
                return null;
            }
            IDictionary dictionary = null;
            foreach (DictionaryEntry entry in propertyState)
            {
                string key = (string) entry.Key;
                object val = entry.Value;
                PersonalizablePropertyEntry entry2 = (PersonalizablePropertyEntry) personalizableProperties[key];
                bool flag = false;
                if ((entry2 != null) && ((scope == System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared) || (entry2.Scope == System.Web.UI.WebControls.WebParts.PersonalizationScope.User)))
                {
                    PropertyInfo propertyInfo = entry2.PropertyInfo;
                    try
                    {
                        FastPropertyAccessor.SetProperty(control, key, val, control.DesignMode);
                        flag = true;
                    }
                    catch
                    {
                    }
                }
                if (!flag)
                {
                    if (dictionary == null)
                    {
                        dictionary = new HybridDictionary(propertyState.Count, false);
                    }
                    dictionary[key] = val;
                }
            }
            return dictionary;
        }

        public override void SetWebPartDirty(WebPart webPart)
        {
            base.ValidateWebPart(webPart);
            string personalizationID = this.CreatePersonalizationID(webPart, null);
            this.SetControlDirty(webPart, personalizationID, false, false);
            GenericWebPart associatedGenericWebPart = webPart as GenericWebPart;
            if (associatedGenericWebPart != null)
            {
                Control childControl = associatedGenericWebPart.ChildControl;
                personalizationID = this.CreatePersonalizationID(childControl, associatedGenericWebPart);
                this.SetControlDirty(childControl, personalizationID, false, false);
            }
        }

        public override void SetWebPartManagerDirty()
        {
            this.SetControlDirty(base.WebPartManager, "__wpm", true, false);
        }

        public override bool IsEmpty
        {
            get
            {
                if (this._extractedState != null)
                {
                    return (this._extractedState.Count == 0);
                }
                return true;
            }
        }

        private bool IsPostRequest
        {
            get
            {
                return this._isPostRequest;
            }
        }

        private System.Web.UI.WebControls.WebParts.PersonalizationScope PersonalizationScope
        {
            get
            {
                return base.WebPartManager.Personalization.Scope;
            }
        }

        private IDictionary SharedState
        {
            get
            {
                return this._sharedState;
            }
        }

        private IDictionary UserState
        {
            get
            {
                if (this._rawUserData != null)
                {
                    this._userState = DeserializeData(this._rawUserData);
                    this._rawUserData = null;
                }
                if (this._userState == null)
                {
                    this._userState = new HybridDictionary(false);
                }
                return this._userState;
            }
        }

        private sealed class ControlInfo
        {
            public bool _allowSetDirty;
            public Control _control;
            public PersonalizationDictionary _customInitialProperties;
            public IDictionary _defaultProperties;
            public bool _dirty;
            public IDictionary _initialProperties;
            public IDictionary _personalizableProperties;
        }

        private sealed class PersonalizationInfo
        {
            public string _controlID;
            public Type _controlType;
            public VirtualPath _controlVPath;
            public PersonalizationDictionary _customProperties;
            public bool _isStatic;
            public IDictionary _properties;

            public bool IsMatchingControlType(Control c)
            {
                if (c is ProxyWebPart)
                {
                    return true;
                }
                if (this._controlType == null)
                {
                    return false;
                }
                if (!(this._controlType == typeof(UserControl)))
                {
                    return this._controlType.IsAssignableFrom(c.GetType());
                }
                UserControl control = c as UserControl;
                return ((control != null) && (control.TemplateControlVirtualPath == this._controlVPath));
            }
        }

        private enum PersonalizationVersions
        {
            WhidbeyBeta2 = 1,
            WhidbeyRTM = 2
        }
    }
}

