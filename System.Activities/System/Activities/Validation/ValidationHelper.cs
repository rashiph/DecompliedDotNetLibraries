namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal static class ValidationHelper
    {
        private static bool CheckIfArgumentIsBound(RuntimeArgument argument, IDictionary<string, object> inputs)
        {
            return !CheckIfArgumentIsNotBound(argument, inputs);
        }

        private static bool CheckIfArgumentIsNotBound(RuntimeArgument argument, IDictionary<string, object> inputs)
        {
            if (((argument.Owner != null) && (argument.Owner.Parent == null)) && ArgumentDirectionHelper.IsOut(argument.Direction))
            {
                return false;
            }
            if ((argument.BoundArgument != null) && (argument.BoundArgument.Expression != null))
            {
                return false;
            }
            if ((inputs != null) && inputs.ContainsKey(argument.Name))
            {
                return false;
            }
            return true;
        }

        public static bool GatherAndValidateOverloads(Activity activity, out Dictionary<string, List<RuntimeArgument>> overloadGroups, out List<RuntimeArgument> requiredArgumentsNotInOverloadGroups, out OverloadGroupEquivalenceInfo equivalenceInfo, ref IList<ValidationError> validationErrors)
        {
            overloadGroups = null;
            requiredArgumentsNotInOverloadGroups = null;
            foreach (RuntimeArgument argument in activity.RuntimeArguments)
            {
                if (!argument.OverloadGroupNames.IsNullOrEmpty())
                {
                    foreach (string str in argument.OverloadGroupNames)
                    {
                        if (overloadGroups == null)
                        {
                            overloadGroups = new Dictionary<string, List<RuntimeArgument>>();
                        }
                        List<RuntimeArgument> list = null;
                        if (!overloadGroups.TryGetValue(str, out list))
                        {
                            list = new List<RuntimeArgument>();
                            overloadGroups.Add(str, list);
                        }
                        list.Add(argument);
                    }
                }
                else if (argument.IsRequired)
                {
                    if (requiredArgumentsNotInOverloadGroups == null)
                    {
                        requiredArgumentsNotInOverloadGroups = new List<RuntimeArgument>();
                    }
                    requiredArgumentsNotInOverloadGroups.Add(argument);
                }
            }
            equivalenceInfo = GetOverloadGroupEquivalence(overloadGroups);
            return ValidateOverloadGroupDefinitions(activity, equivalenceInfo, overloadGroups, ref validationErrors);
        }

        private static OverloadGroupEquivalenceInfo GetOverloadGroupEquivalence(Dictionary<string, List<RuntimeArgument>> groupDefinitions)
        {
            OverloadGroupEquivalenceInfo info = new OverloadGroupEquivalenceInfo();
            if (!groupDefinitions.IsNullOrEmpty())
            {
                string[] array = new string[groupDefinitions.Count];
                groupDefinitions.Keys.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    string str = array[i];
                    HashSet<RuntimeArgument> set = new HashSet<RuntimeArgument>(groupDefinitions[str]);
                    for (int j = i + 1; j < array.Length; j++)
                    {
                        string str2 = array[j];
                        HashSet<RuntimeArgument> other = new HashSet<RuntimeArgument>(groupDefinitions[str2]);
                        if (set.IsProperSupersetOf(other))
                        {
                            info.SetAsSuperset(str, str2);
                        }
                        else if (set.IsProperSubsetOf(other))
                        {
                            info.SetAsSuperset(str2, str);
                        }
                        else if (set.SetEquals(other))
                        {
                            info.SetAsEquivalent(str, str2);
                        }
                        else if (set.Overlaps(other))
                        {
                            info.SetAsOverlapping(str, str2);
                        }
                        else
                        {
                            info.SetAsDisjoint(str, str2);
                        }
                    }
                }
            }
            return info;
        }

        public static void ValidateArguments(Activity activity, OverloadGroupEquivalenceInfo equivalenceInfo, Dictionary<string, List<RuntimeArgument>> overloadGroups, List<RuntimeArgument> requiredArgumentsNotInOverloadGroups, IDictionary<string, object> inputs, ref IList<ValidationError> validationErrors)
        {
            Func<RuntimeArgument, bool> func3 = null;
            Func<RuntimeArgument, bool> func4 = null;
            Predicate<RuntimeArgument> predicate2 = null;
            if (!requiredArgumentsNotInOverloadGroups.IsNullOrEmpty())
            {
                foreach (RuntimeArgument argument in requiredArgumentsNotInOverloadGroups)
                {
                    if (CheckIfArgumentIsNotBound(argument, inputs))
                    {
                        ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.RequiredArgumentValueNotSupplied(argument.Name), false, argument.Name, activity));
                    }
                }
            }
            if (!overloadGroups.IsNullOrEmpty())
            {
                Func<string, bool> func = null;
                Func<string, bool> func2 = null;
                Dictionary<string, bool> configurationResults = new Dictionary<string, bool>();
                string key = string.Empty;
                int num = 0;
                int num2 = 0;
                foreach (KeyValuePair<string, List<RuntimeArgument>> pair in overloadGroups)
                {
                    string str2 = pair.Key;
                    configurationResults.Add(str2, false);
                    IEnumerable<RuntimeArgument> source = from a in pair.Value
                        where a.IsRequired
                        select a;
                    if (source.Count<RuntimeArgument>() > 0)
                    {
                        if (func3 == null)
                        {
                            func3 = localArgument => CheckIfArgumentIsBound(localArgument, inputs);
                        }
                        if (source.All<RuntimeArgument>(func3))
                        {
                            configurationResults[str2] = true;
                            key = str2;
                            num++;
                        }
                    }
                    else
                    {
                        num2++;
                        if (func4 == null)
                        {
                            func4 = localArgument => CheckIfArgumentIsBound(localArgument, inputs);
                        }
                        if ((from a in pair.Value
                            where !a.IsRequired
                            select a).Any<RuntimeArgument>(func4))
                        {
                            configurationResults[str2] = true;
                            key = str2;
                            num++;
                        }
                    }
                }
                switch (num)
                {
                    case 0:
                        if (num2 == 0)
                        {
                            ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.NoOverloadGroupsAreConfigured, false, activity));
                            return;
                        }
                        return;

                    case 1:
                    {
                        HashSet<RuntimeArgument> second = new HashSet<RuntimeArgument>(overloadGroups[key]);
                        if (predicate2 == null)
                        {
                            predicate2 = localArgument => CheckIfArgumentIsBound(localArgument, inputs);
                        }
                        Predicate<RuntimeArgument> match = predicate2;
                        List<string> list = null;
                        if (!equivalenceInfo.DisjointGroupsDictionary.IsNullOrEmpty())
                        {
                            equivalenceInfo.DisjointGroupsDictionary.TryGetValue(key, out list);
                        }
                        List<string> list2 = null;
                        if (!equivalenceInfo.OverlappingGroupsDictionary.IsNullOrEmpty())
                        {
                            equivalenceInfo.OverlappingGroupsDictionary.TryGetValue(key, out list2);
                        }
                        if (func == null)
                        {
                            func = k => !configurationResults[k];
                        }
                        foreach (string str3 in configurationResults.Keys.Where<string>(func))
                        {
                            if ((list != null) && list.Contains(str3))
                            {
                                foreach (RuntimeArgument argument2 in overloadGroups[str3].FindAll(match))
                                {
                                    ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ExtraOverloadGroupPropertiesConfigured(key, argument2.Name, str3), false, activity));
                                }
                            }
                            else if ((list2 != null) && list2.Contains(str3))
                            {
                                HashSet<RuntimeArgument> first = new HashSet<RuntimeArgument>(overloadGroups[str3]);
                                IEnumerable<RuntimeArgument> enumerable3 = first.Intersect<RuntimeArgument>(second);
                                foreach (RuntimeArgument argument3 in first.Except<RuntimeArgument>(enumerable3).ToList<RuntimeArgument>().FindAll(match))
                                {
                                    ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ExtraOverloadGroupPropertiesConfigured(key, argument3.Name, str3), false, activity));
                                }
                            }
                        }
                        return;
                    }
                }
                if (func2 == null)
                {
                    func2 = k => configurationResults[k];
                }
                IEnumerable<string> c = configurationResults.Keys.Where<string>(func2).OrderBy<string, string>(k => k, StringComparer.Ordinal);
                ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.MultipleOverloadGroupsConfigured(c.AsCommaSeparatedValues()), false, activity));
            }
        }

        private static bool ValidateOverloadGroupDefinitions(Activity activity, OverloadGroupEquivalenceInfo equivalenceInfo, Dictionary<string, List<RuntimeArgument>> overloadGroups, ref IList<ValidationError> validationErrors)
        {
            bool flag = true;
            if (!equivalenceInfo.EquivalentGroupsDictionary.IsNullOrEmpty())
            {
                Hashtable hashtable = new Hashtable(equivalenceInfo.EquivalentGroupsDictionary.Count);
                foreach (KeyValuePair<string, List<string>> pair in equivalenceInfo.EquivalentGroupsDictionary)
                {
                    if (!hashtable.Contains(pair.Key))
                    {
                        string[] array = new string[pair.Value.Count + 1];
                        array[0] = pair.Key;
                        pair.Value.CopyTo(array, 1);
                        IEnumerable<string> c = array.OrderBy<string, string>(s => s, StringComparer.Ordinal);
                        ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.OverloadGroupsAreEquivalent(c.AsCommaSeparatedValues()), false, activity));
                        flag = false;
                        for (int i = 0; i < array.Length; i++)
                        {
                            hashtable.Add(array[i], null);
                        }
                    }
                }
                return flag;
            }
            if (!equivalenceInfo.SupersetOfGroupsDictionary.IsNullOrEmpty())
            {
                foreach (KeyValuePair<string, List<string>> pair2 in equivalenceInfo.SupersetOfGroupsDictionary)
                {
                    IList<string> list = pair2.Value.OrderBy<string, string>(s => s, StringComparer.Ordinal).ToList<string>();
                    string[] strArray2 = new string[list.Count];
                    int num2 = 0;
                    foreach (string str in list)
                    {
                        if (overloadGroups[str].Any<RuntimeArgument>(a => a.IsRequired))
                        {
                            strArray2[num2++] = str;
                        }
                    }
                    if (num2 > 0)
                    {
                        ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.OverloadGroupHasSubsets(pair2.Key, strArray2.AsCommaSeparatedValues()), false, activity));
                        flag = false;
                    }
                }
            }
            return flag;
        }

        public class OverloadGroupEquivalenceInfo
        {
            private Dictionary<string, List<string>> disjointGroupsDictionary;
            private Dictionary<string, List<string>> equivalentGroupsDictionary;
            private Dictionary<string, List<string>> overlappingGroupsDictionary;
            private Dictionary<string, List<string>> supersetOfGroupsDictionary;

            private void AddToDictionary(ref Dictionary<string, List<string>> dictionary, string dictionaryKey, string listEntry)
            {
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, List<string>>();
                }
                List<string> list = null;
                if (!dictionary.TryGetValue(dictionaryKey, out list))
                {
                    list = new List<string> {
                        listEntry
                    };
                    dictionary.Add(dictionaryKey, list);
                }
                else
                {
                    list.Add(listEntry);
                }
            }

            public void SetAsDisjoint(string group1, string group2)
            {
                this.AddToDictionary(ref this.disjointGroupsDictionary, group1, group2);
                this.AddToDictionary(ref this.disjointGroupsDictionary, group2, group1);
            }

            public void SetAsEquivalent(string group1, string group2)
            {
                this.AddToDictionary(ref this.equivalentGroupsDictionary, group1, group2);
                this.AddToDictionary(ref this.equivalentGroupsDictionary, group2, group1);
            }

            public void SetAsOverlapping(string group1, string group2)
            {
                this.AddToDictionary(ref this.overlappingGroupsDictionary, group1, group2);
                this.AddToDictionary(ref this.overlappingGroupsDictionary, group2, group1);
            }

            public void SetAsSuperset(string group1, string group2)
            {
                this.AddToDictionary(ref this.supersetOfGroupsDictionary, group1, group2);
            }

            public Dictionary<string, List<string>> DisjointGroupsDictionary
            {
                get
                {
                    return this.disjointGroupsDictionary;
                }
            }

            public Dictionary<string, List<string>> EquivalentGroupsDictionary
            {
                get
                {
                    return this.equivalentGroupsDictionary;
                }
            }

            public Dictionary<string, List<string>> OverlappingGroupsDictionary
            {
                get
                {
                    return this.overlappingGroupsDictionary;
                }
            }

            public Dictionary<string, List<string>> SupersetOfGroupsDictionary
            {
                get
                {
                    return this.supersetOfGroupsDictionary;
                }
            }
        }
    }
}

