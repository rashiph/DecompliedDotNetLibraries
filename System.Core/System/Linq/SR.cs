namespace System.Linq
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string Argument_AdjustmentRulesAmbiguousOverlap = "Argument_AdjustmentRulesAmbiguousOverlap";
        internal const string Argument_AdjustmentRulesInvalidOverlap = "Argument_AdjustmentRulesInvalidOverlap";
        internal const string Argument_AdjustmentRulesNoNulls = "Argument_AdjustmentRulesNoNulls";
        internal const string Argument_AdjustmentRulesOutOfOrder = "Argument_AdjustmentRulesOutOfOrder";
        internal const string Argument_AdjustmentRulesrDaylightSavingTimeOverlap = "Argument_AdjustmentRulesrDaylightSavingTimeOverlap";
        internal const string Argument_AdjustmentRulesrDaylightSavingTimeOverlapNonRuleRange = "Argument_AdjustmentRulesrDaylightSavingTimeOverlapNonRuleRange";
        internal const string Argument_ConvertMismatch = "Argument_ConvertMismatch";
        internal const string Argument_DateTimeHasTicks = "Argument_DateTimeHasTicks";
        internal const string Argument_DateTimeHasTimeOfDay = "Argument_DateTimeHasTimeOfDay";
        internal const string Argument_DateTimeIsInvalid = "Argument_DateTimeIsInvalid";
        internal const string Argument_DateTimeIsNotAmbiguous = "Argument_DateTimeIsNotAmbiguous";
        internal const string Argument_DateTimeKindMustBeUnspecified = "Argument_DateTimeKindMustBeUnspecified";
        internal const string Argument_DateTimeOffsetIsNotAmbiguous = "Argument_DateTimeOffsetIsNotAmbiguous";
        internal const string Argument_InvalidId = "Argument_InvalidId";
        internal const string Argument_InvalidREG_TZI_FORMAT = "Argument_InvalidREG_TZI_FORMAT";
        internal const string Argument_InvalidSerializedString = "Argument_InvalidSerializedString";
        internal const string Argument_OutOfOrderDateTimes = "Argument_OutOfOrderDateTimes";
        internal const string Argument_TimeSpanHasSeconds = "Argument_TimeSpanHasSeconds";
        internal const string Argument_TimeZoneInfoBadTZif = "Argument_TimeZoneInfoBadTZif";
        internal const string Argument_TimeZoneInfoInvalidTZif = "Argument_TimeZoneInfoInvalidTZif";
        internal const string Argument_TransitionTimesAreIdentical = "Argument_TransitionTimesAreIdentical";
        internal const string ArgumentArrayHasTooManyElements = "ArgumentArrayHasTooManyElements";
        internal const string ArgumentNotIEnumerableGeneric = "ArgumentNotIEnumerableGeneric";
        internal const string ArgumentNotLambda = "ArgumentNotLambda";
        internal const string ArgumentNotSequence = "ArgumentNotSequence";
        internal const string ArgumentNotValid = "ArgumentNotValid";
        internal const string ArgumentOutOfRange_DateTimeBadTicks = "ArgumentOutOfRange_DateTimeBadTicks";
        internal const string ArgumentOutOfRange_DayOfWeek = "ArgumentOutOfRange_DayOfWeek";
        internal const string ArgumentOutOfRange_DayParam = "ArgumentOutOfRange_DayParam";
        internal const string ArgumentOutOfRange_MonthParam = "ArgumentOutOfRange_MonthParam";
        internal const string ArgumentOutOfRange_UtcOffset = "ArgumentOutOfRange_UtcOffset";
        internal const string ArgumentOutOfRange_UtcOffsetAndDaylightDelta = "ArgumentOutOfRange_UtcOffsetAndDaylightDelta";
        internal const string ArgumentOutOfRange_Week = "ArgumentOutOfRange_Week";
        internal const string EmptyEnumerable = "EmptyEnumerable";
        internal const string IncompatibleElementTypes = "IncompatibleElementTypes";
        internal const string InvalidTimeZone_InvalidRegistryData = "InvalidTimeZone_InvalidRegistryData";
        internal const string InvalidTimeZone_InvalidWin32APIData = "InvalidTimeZone_InvalidWin32APIData";
        private static System.Linq.SR loader;
        internal const string MoreThanOneElement = "MoreThanOneElement";
        internal const string MoreThanOneMatch = "MoreThanOneMatch";
        internal const string NoArgumentMatchingMethodsInQueryable = "NoArgumentMatchingMethodsInQueryable";
        internal const string NoElements = "NoElements";
        internal const string NoMatch = "NoMatch";
        internal const string NoMethodOnType = "NoMethodOnType";
        internal const string NoMethodOnTypeMatchingArguments = "NoMethodOnTypeMatchingArguments";
        internal const string NoNameMatchingMethodsInQueryable = "NoNameMatchingMethodsInQueryable";
        internal const string OwningTeam = "OwningTeam";
        internal const string ParallelEnumerable_BinaryOpMustUseAsParallel = "ParallelEnumerable_BinaryOpMustUseAsParallel";
        internal const string ParallelEnumerable_ToArray_DimensionRequired = "ParallelEnumerable_ToArray_DimensionRequired";
        internal const string ParallelEnumerable_WithCancellation_TokenSourceDisposed = "ParallelEnumerable_WithCancellation_TokenSourceDisposed";
        internal const string ParallelEnumerable_WithMergeOptions_InvalidOptions = "ParallelEnumerable_WithMergeOptions_InvalidOptions";
        internal const string ParallelEnumerable_WithQueryExecutionMode_InvalidMode = "ParallelEnumerable_WithQueryExecutionMode_InvalidMode";
        internal const string ParallelPartitionable_IncorretElementCount = "ParallelPartitionable_IncorretElementCount";
        internal const string ParallelPartitionable_NullElement = "ParallelPartitionable_NullElement";
        internal const string ParallelPartitionable_NullReturn = "ParallelPartitionable_NullReturn";
        internal const string ParallelQuery_DuplicateDOP = "ParallelQuery_DuplicateDOP";
        internal const string ParallelQuery_DuplicateExecutionMode = "ParallelQuery_DuplicateExecutionMode";
        internal const string ParallelQuery_DuplicateMergeOptions = "ParallelQuery_DuplicateMergeOptions";
        internal const string ParallelQuery_DuplicateTaskScheduler = "ParallelQuery_DuplicateTaskScheduler";
        internal const string ParallelQuery_DuplicateWithCancellation = "ParallelQuery_DuplicateWithCancellation";
        internal const string ParallelQuery_InvalidAsOrderedCall = "ParallelQuery_InvalidAsOrderedCall";
        internal const string ParallelQuery_InvalidNonGenericAsOrderedCall = "ParallelQuery_InvalidNonGenericAsOrderedCall";
        internal const string ParallelQuery_PartitionerNotOrderable = "ParallelQuery_PartitionerNotOrderable";
        internal const string PartitionerQueryOperator_NullPartition = "PartitionerQueryOperator_NullPartition";
        internal const string PartitionerQueryOperator_NullPartitionList = "PartitionerQueryOperator_NullPartitionList";
        internal const string PartitionerQueryOperator_WrongNumberOfPartitions = "PartitionerQueryOperator_WrongNumberOfPartitions";
        internal const string PLINQ_CommonEnumerator_Current_NotStarted = "PLINQ_CommonEnumerator_Current_NotStarted";
        internal const string PLINQ_DisposeRequested = "PLINQ_DisposeRequested";
        internal const string PLINQ_EnumerationPreviouslyFailed = "PLINQ_EnumerationPreviouslyFailed";
        internal const string PLINQ_ExternalCancellationRequested = "PLINQ_ExternalCancellationRequested";
        private ResourceManager resources;
        internal const string Security_CannotReadRegistryData = "Security_CannotReadRegistryData";
        internal const string Serialization_CorruptField = "Serialization_CorruptField";
        internal const string Serialization_InvalidEscapeSequence = "Serialization_InvalidEscapeSequence";
        internal const string TimeZoneNotFound_MissingRegistryData = "TimeZoneNotFound_MissingRegistryData";

        internal SR()
        {
            this.resources = new ResourceManager("System.Linq", base.GetType().Assembly);
        }

        private static System.Linq.SR GetLoader()
        {
            if (loader == null)
            {
                System.Linq.SR sr = new System.Linq.SR();
                Interlocked.CompareExchange<System.Linq.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.Linq.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.Linq.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.Linq.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

