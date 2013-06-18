namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal static class WebServiceActivityHelpers
    {
        private static string FindLeastCommonParent(IEnumerable<string> source, IEnumerable<string> dest)
        {
            IEnumerator enumerator = source.GetEnumerator();
            IEnumerator enumerator2 = dest.GetEnumerator();
            string current = null;
            while (enumerator.MoveNext() && enumerator2.MoveNext())
            {
                if (enumerator.Current.Equals(enumerator2.Current))
                {
                    current = (string) enumerator.Current;
                }
                else
                {
                    return current;
                }
            }
            return current;
        }

        private static IEnumerable<string> GetActivityPath(Activity activity)
        {
            if (activity != null)
            {
                foreach (string iteratorVariable0 in GetActivityPath(activity.Parent))
                {
                    yield return iteratorVariable0;
                }
                yield return activity.QualifiedName;
            }
        }

        private static IEnumerable GetContainedActivities(CompositeActivity activity)
        {
            if (activity.Enabled)
            {
                foreach (Activity iteratorVariable0 in activity.Activities)
                {
                    if ((iteratorVariable0 is CompositeActivity) && !Helpers.IsCustomActivity((CompositeActivity) iteratorVariable0))
                    {
                        IEnumerator enumerator = GetContainedActivities((CompositeActivity) iteratorVariable0).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Activity current = (Activity) enumerator.Current;
                            if (current.Enabled)
                            {
                                yield return current;
                            }
                        }
                    }
                    else if (iteratorVariable0.Enabled)
                    {
                        yield return iteratorVariable0;
                    }
                }
            }
        }

        internal static void GetParameterInfo(MethodInfo methodInfo, out List<ParameterInfo> inParameters, out List<ParameterInfo> outParameters)
        {
            inParameters = new List<ParameterInfo>();
            outParameters = new List<ParameterInfo>();
            foreach (ParameterInfo info in methodInfo.GetParameters())
            {
                if ((info.IsOut || info.IsRetval) || info.ParameterType.IsByRef)
                {
                    outParameters.Add(info);
                }
                if (!info.IsOut && !info.IsRetval)
                {
                    inParameters.Add(info);
                }
            }
            if (methodInfo.ReturnType != typeof(void))
            {
                outParameters.Add(methodInfo.ReturnParameter);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static IEnumerable GetPreceedingActivities(Activity startActivity)
        {
            return GetPreceedingActivities(startActivity, false);
        }

        internal static IEnumerable GetPreceedingActivities(Activity startActivity, bool crossOverLoop)
        {
            Activity activity = null;
            Stack<Activity> iteratorVariable1 = new Stack<Activity>();
            iteratorVariable1.Push(startActivity);
            while ((activity = iteratorVariable1.Pop()) != null)
            {
                if ((activity is CompositeActivity) && Helpers.IsCustomActivity((CompositeActivity) activity))
                {
                    break;
                }
                if (activity.Parent != null)
                {
                    foreach (Activity iteratorVariable2 in activity.Parent.Activities)
                    {
                        if (((iteratorVariable2 == activity) && (((activity.Parent is ParallelActivity) && !Helpers.IsFrameworkActivity(activity)) || ((activity.Parent is StateActivity) && !Helpers.IsFrameworkActivity(activity)))) || (((activity.Parent is IfElseActivity) && !Helpers.IsFrameworkActivity(activity)) || ((activity.Parent is ListenActivity) && !Helpers.IsFrameworkActivity(activity))))
                        {
                            continue;
                        }
                        StateActivity parent = activity.Parent as StateActivity;
                        if (parent != null)
                        {
                            StateActivity state = StateMachineHelpers.FindEnclosingState(startActivity);
                            if (StateMachineHelpers.IsInitialState(state))
                            {
                                break;
                            }
                            yield return parent;
                        }
                        if (iteratorVariable2 == activity)
                        {
                            break;
                        }
                        if (iteratorVariable2.Enabled)
                        {
                            if (((iteratorVariable2 is CompositeActivity) && !Helpers.IsCustomActivity((CompositeActivity) iteratorVariable2)) && (crossOverLoop || !IsLoopActivity(iteratorVariable2)))
                            {
                                IEnumerator enumerator = GetContainedActivities((CompositeActivity) iteratorVariable2).GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    Activity current = (Activity) enumerator.Current;
                                    yield return current;
                                }
                                continue;
                            }
                            yield return iteratorVariable2;
                        }
                    }
                }
                if (!crossOverLoop && IsLoopActivity(activity.Parent))
                {
                    break;
                }
                iteratorVariable1.Push(activity.Parent);
            }
        }

        internal static IEnumerable GetSucceedingActivities(Activity startActivity)
        {
            Activity iteratorVariable0 = null;
            Stack<Activity> iteratorVariable1 = new Stack<Activity>();
            iteratorVariable1.Push(startActivity);
            while ((iteratorVariable0 = iteratorVariable1.Pop()) != null)
            {
                if ((iteratorVariable0 is CompositeActivity) && Helpers.IsCustomActivity((CompositeActivity) iteratorVariable0))
                {
                    break;
                }
                if (iteratorVariable0.Parent != null)
                {
                    bool iteratorVariable2 = false;
                    foreach (Activity iteratorVariable3 in iteratorVariable0.Parent.Activities)
                    {
                        if (iteratorVariable3 == iteratorVariable0)
                        {
                            iteratorVariable2 = true;
                            continue;
                        }
                        if (iteratorVariable2 && iteratorVariable3.Enabled)
                        {
                            if ((iteratorVariable3 is CompositeActivity) && !Helpers.IsCustomActivity((CompositeActivity) iteratorVariable3))
                            {
                                IEnumerator enumerator = GetContainedActivities((CompositeActivity) iteratorVariable3).GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    Activity current = (Activity) enumerator.Current;
                                    yield return current;
                                }
                                continue;
                            }
                            yield return iteratorVariable3;
                        }
                    }
                }
                iteratorVariable1.Push(iteratorVariable0.Parent);
            }
        }

        internal static bool IsInsideLoop(Activity webServiceActivity, Activity searchBoundary)
        {
            IEnumerable<string> activityPath = GetActivityPath(searchBoundary);
            IEnumerable<string> dest = GetActivityPath(webServiceActivity);
            string str = FindLeastCommonParent(activityPath, dest);
            for (Activity activity = webServiceActivity; (activity.Parent != null) && (activity.Parent.QualifiedName != str); activity = activity.Parent)
            {
                if (IsLoopActivity(activity))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsLoopActivity(Activity activity)
        {
            return (((activity is WhileActivity) || (activity is ReplicatorActivity)) || (activity is ConditionedActivityGroup));
        }

        internal static ValidationErrorCollection ValidateParameterTypes(MethodInfo methodInfo)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            if (methodInfo != null)
            {
                foreach (ParameterInfo info in methodInfo.GetParameters())
                {
                    if (info.ParameterType == null)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_ParameterTypeNotFound", new object[] { methodInfo.Name, info.Name }), 0x571));
                    }
                }
                if ((methodInfo.ReturnType != typeof(void)) && (methodInfo.ReturnParameter.ParameterType == null))
                {
                    errors.Add(new ValidationError(SR.GetString("Error_ReturnTypeNotFound", new object[] { methodInfo.Name }), 0x572));
                }
            }
            return errors;
        }




    }
}

