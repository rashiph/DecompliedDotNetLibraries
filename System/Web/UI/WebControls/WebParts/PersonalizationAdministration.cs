namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    public static class PersonalizationAdministration
    {
        private const int _defaultPageIndex = 0;
        private const int _defaultPageSize = 0x7fffffff;
        private static readonly object _initializationLock = new object();
        private static bool _initialized;
        private static PersonalizationProvider _provider;
        private static PersonalizationProviderCollection _providers;
        internal static readonly DateTime DefaultInactiveSinceDate = DateTime.MaxValue;

        public static PersonalizationStateInfoCollection FindInactiveUserState(string pathToMatch, string usernameToMatch, DateTime userInactiveSinceDate)
        {
            int num;
            return FindInactiveUserState(pathToMatch, usernameToMatch, userInactiveSinceDate, 0, 0x7fffffff, out num);
        }

        public static PersonalizationStateInfoCollection FindInactiveUserState(string pathToMatch, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            pathToMatch = System.Web.Util.StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            usernameToMatch = System.Web.Util.StringUtil.CheckAndTrimString(usernameToMatch, "usernameToMatch", false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                PathToMatch = pathToMatch,
                UsernameToMatch = usernameToMatch,
                UserInactiveSinceDate = userInactiveSinceDate
            };
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindSharedState(string pathToMatch)
        {
            int num;
            return FindSharedState(pathToMatch, 0, 0x7fffffff, out num);
        }

        public static PersonalizationStateInfoCollection FindSharedState(string pathToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            pathToMatch = System.Web.Util.StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                PathToMatch = pathToMatch
            };
            return FindStatePrivate(PersonalizationScope.Shared, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        private static PersonalizationStateInfoCollection FindStatePrivate(PersonalizationScope scope, PersonalizationStateQuery stateQuery, int pageIndex, int pageSize, out int totalRecords)
        {
            Initialize();
            return _provider.FindState(scope, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindUserState(string pathToMatch, string usernameToMatch)
        {
            int num;
            return FindUserState(pathToMatch, usernameToMatch, 0, 0x7fffffff, out num);
        }

        public static PersonalizationStateInfoCollection FindUserState(string pathToMatch, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            pathToMatch = System.Web.Util.StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            usernameToMatch = System.Web.Util.StringUtil.CheckAndTrimString(usernameToMatch, "usernameToMatch", false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                PathToMatch = pathToMatch,
                UsernameToMatch = usernameToMatch
            };
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllInactiveUserState(DateTime userInactiveSinceDate)
        {
            int num;
            return GetAllInactiveUserState(userInactiveSinceDate, 0, 0x7fffffff, out num);
        }

        public static PersonalizationStateInfoCollection GetAllInactiveUserState(DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                UserInactiveSinceDate = userInactiveSinceDate
            };
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllState(PersonalizationScope scope)
        {
            int num;
            return GetAllState(scope, 0, 0x7fffffff, out num);
        }

        public static PersonalizationStateInfoCollection GetAllState(PersonalizationScope scope, int pageIndex, int pageSize, out int totalRecords)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            return FindStatePrivate(scope, null, pageIndex, pageSize, out totalRecords);
        }

        public static int GetCountOfInactiveUserState(DateTime userInactiveSinceDate)
        {
            return GetCountOfInactiveUserState(null, userInactiveSinceDate);
        }

        public static int GetCountOfInactiveUserState(string pathToMatch, DateTime userInactiveSinceDate)
        {
            pathToMatch = System.Web.Util.StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                PathToMatch = pathToMatch,
                UserInactiveSinceDate = userInactiveSinceDate
            };
            return GetCountOfStatePrivate(PersonalizationScope.User, stateQuery);
        }

        public static int GetCountOfState(PersonalizationScope scope)
        {
            return GetCountOfState(scope, null);
        }

        public static int GetCountOfState(PersonalizationScope scope, string pathToMatch)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            pathToMatch = System.Web.Util.StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                PathToMatch = pathToMatch
            };
            return GetCountOfStatePrivate(scope, stateQuery);
        }

        private static int GetCountOfStatePrivate(PersonalizationScope scope, PersonalizationStateQuery stateQuery)
        {
            Initialize();
            int countOfState = _provider.GetCountOfState(scope, stateQuery);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(countOfState, "GetCountOfState");
            return countOfState;
        }

        public static int GetCountOfUserState(string usernameToMatch)
        {
            usernameToMatch = System.Web.Util.StringUtil.CheckAndTrimString(usernameToMatch, "usernameToMatch", false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery {
                UsernameToMatch = usernameToMatch
            };
            return GetCountOfStatePrivate(PersonalizationScope.User, stateQuery);
        }

        private static void Initialize()
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            if (!_initialized)
            {
                lock (_initializationLock)
                {
                    if (!_initialized)
                    {
                        WebPartsPersonalization personalization = RuntimeConfig.GetAppConfig().WebParts.Personalization;
                        _providers = new PersonalizationProviderCollection();
                        ProvidersHelper.InstantiateProviders(personalization.Providers, _providers, typeof(PersonalizationProvider));
                        _providers.SetReadOnly();
                        _provider = _providers[personalization.DefaultProvider];
                        if (_provider == null)
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_provider_must_exist", new object[] { personalization.DefaultProvider }), personalization.ElementInformation.Properties["defaultProvider"].Source, personalization.ElementInformation.Properties["defaultProvider"].LineNumber);
                        }
                        _initialized = true;
                    }
                }
            }
        }

        public static int ResetAllState(PersonalizationScope scope)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            return ResetStatePrivate(scope, null, null);
        }

        public static int ResetInactiveUserState(DateTime userInactiveSinceDate)
        {
            return ResetInactiveUserStatePrivate(null, userInactiveSinceDate);
        }

        public static int ResetInactiveUserState(string path, DateTime userInactiveSinceDate)
        {
            path = System.Web.Util.StringUtil.CheckAndTrimString(path, "path");
            return ResetInactiveUserStatePrivate(path, userInactiveSinceDate);
        }

        private static int ResetInactiveUserStatePrivate(string path, DateTime userInactiveSinceDate)
        {
            Initialize();
            int returnedValue = _provider.ResetUserState(path, userInactiveSinceDate);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(returnedValue, "ResetUserState");
            return returnedValue;
        }

        public static bool ResetSharedState(string path)
        {
            path = System.Web.Util.StringUtil.CheckAndTrimString(path, "path");
            string[] paths = new string[] { path };
            int num = ResetStatePrivate(PersonalizationScope.Shared, paths, null);
            if (num > 1)
            {
                throw new HttpException(System.Web.SR.GetString("PersonalizationAdmin_UnexpectedResetSharedStateReturnValue", new object[] { num.ToString(CultureInfo.CurrentCulture) }));
            }
            return (num == 1);
        }

        public static int ResetSharedState(string[] paths)
        {
            paths = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(paths, "paths", true, false, -1);
            return ResetStatePrivate(PersonalizationScope.Shared, paths, null);
        }

        public static int ResetState(PersonalizationStateInfoCollection data)
        {
            int num = 0;
            PersonalizationProviderHelper.CheckNullEntries(data, "data");
            StringCollection strings = null;
            foreach (PersonalizationStateInfo info in data)
            {
                UserPersonalizationStateInfo info2 = info as UserPersonalizationStateInfo;
                if (info2 != null)
                {
                    if (ResetUserState(info2.Path, info2.Username))
                    {
                        num++;
                    }
                }
                else
                {
                    if (strings == null)
                    {
                        strings = new StringCollection();
                    }
                    strings.Add(info.Path);
                }
            }
            if (strings != null)
            {
                string[] array = new string[strings.Count];
                strings.CopyTo(array, 0);
                num += ResetStatePrivate(PersonalizationScope.Shared, array, null);
            }
            return num;
        }

        private static int ResetStatePrivate(PersonalizationScope scope, string[] paths, string[] usernames)
        {
            Initialize();
            int returnedValue = _provider.ResetState(scope, paths, usernames);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(returnedValue, "ResetState");
            return returnedValue;
        }

        public static int ResetUserState(string path)
        {
            path = System.Web.Util.StringUtil.CheckAndTrimString(path, "path");
            string[] paths = new string[] { path };
            return ResetStatePrivate(PersonalizationScope.User, paths, null);
        }

        public static int ResetUserState(string[] usernames)
        {
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, "usernames", true, true, -1);
            return ResetStatePrivate(PersonalizationScope.User, null, usernames);
        }

        public static bool ResetUserState(string path, string username)
        {
            path = System.Web.Util.StringUtil.CheckAndTrimString(path, "path");
            username = PersonalizationProviderHelper.CheckAndTrimStringWithoutCommas(username, "username");
            string[] paths = new string[] { path };
            string[] usernames = new string[] { username };
            int num = ResetStatePrivate(PersonalizationScope.User, paths, usernames);
            if (num > 1)
            {
                throw new HttpException(System.Web.SR.GetString("PersonalizationAdmin_UnexpectedResetUserStateReturnValue", new object[] { num.ToString(CultureInfo.CurrentCulture) }));
            }
            return (num == 1);
        }

        public static int ResetUserState(string path, string[] usernames)
        {
            path = System.Web.Util.StringUtil.CheckAndTrimString(path, "path");
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, "usernames", true, true, -1);
            string[] paths = new string[] { path };
            return ResetStatePrivate(PersonalizationScope.User, paths, usernames);
        }

        public static string ApplicationName
        {
            get
            {
                return Provider.ApplicationName;
            }
            set
            {
                Provider.ApplicationName = value;
            }
        }

        public static PersonalizationProvider Provider
        {
            get
            {
                Initialize();
                return _provider;
            }
        }

        public static PersonalizationProviderCollection Providers
        {
            get
            {
                Initialize();
                return _providers;
            }
        }
    }
}

