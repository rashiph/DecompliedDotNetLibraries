namespace System.Activities
{
    using System;
    using System.Activities.Validation;
    using System.Runtime.CompilerServices;

    internal class ProcessActivityTreeOptions
    {
        private static ProcessActivityTreeOptions finishCachingSubtreeOptionsWithCreateEmptyBindings;
        private static ProcessActivityTreeOptions finishCachingSubtreeOptionsWithoutCreateEmptyBindings;
        private static ProcessActivityTreeOptions fullCachingOptions;
        private static ProcessActivityTreeOptions singleLevelValidationOptions;
        private static ProcessActivityTreeOptions validationOptions;

        private ProcessActivityTreeOptions()
        {
        }

        public static ProcessActivityTreeOptions GetFinishCachingSubtreeOptions(ProcessActivityTreeOptions originalOptions)
        {
            if (originalOptions.CreateEmptyBindings)
            {
                return FinishCachingSubtreeOptionsWithCreateEmptyBindings;
            }
            return FinishCachingSubtreeOptionsWithoutCreateEmptyBindings;
        }

        public static ProcessActivityTreeOptions GetValidationOptions(ValidationSettings settings)
        {
            if (settings.SingleLevel)
            {
                return SingleLevelValidationOptions;
            }
            return ValidationOptions;
        }

        public bool CreateEmptyBindings { get; private set; }

        private static ProcessActivityTreeOptions FinishCachingSubtreeOptionsWithCreateEmptyBindings
        {
            get
            {
                if (finishCachingSubtreeOptionsWithCreateEmptyBindings == null)
                {
                    ProcessActivityTreeOptions options = new ProcessActivityTreeOptions {
                        SkipConstraints = true,
                        CreateEmptyBindings = true,
                        StoreTempViolations = true
                    };
                    finishCachingSubtreeOptionsWithCreateEmptyBindings = options;
                }
                return finishCachingSubtreeOptionsWithCreateEmptyBindings;
            }
        }

        private static ProcessActivityTreeOptions FinishCachingSubtreeOptionsWithoutCreateEmptyBindings
        {
            get
            {
                if (finishCachingSubtreeOptionsWithoutCreateEmptyBindings == null)
                {
                    ProcessActivityTreeOptions options = new ProcessActivityTreeOptions {
                        SkipConstraints = true,
                        StoreTempViolations = true
                    };
                    finishCachingSubtreeOptionsWithoutCreateEmptyBindings = options;
                }
                return finishCachingSubtreeOptionsWithoutCreateEmptyBindings;
            }
        }

        public static ProcessActivityTreeOptions FullCachingOptions
        {
            get
            {
                if (fullCachingOptions == null)
                {
                    ProcessActivityTreeOptions options = new ProcessActivityTreeOptions {
                        SkipIfCached = true,
                        CreateEmptyBindings = true,
                        OnlyCallCallbackForDeclarations = true
                    };
                    fullCachingOptions = options;
                }
                return fullCachingOptions;
            }
        }

        public bool IsRuntimeReadyOptions
        {
            get
            {
                return (!this.SkipPrivateChildren && this.CreateEmptyBindings);
            }
        }

        public bool OnlyCallCallbackForDeclarations { get; private set; }

        public bool OnlyVisitSingleLevel { get; private set; }

        private static ProcessActivityTreeOptions SingleLevelValidationOptions
        {
            get
            {
                if (singleLevelValidationOptions == null)
                {
                    ProcessActivityTreeOptions options = new ProcessActivityTreeOptions {
                        SkipPrivateChildren = false,
                        CreateEmptyBindings = false,
                        OnlyVisitSingleLevel = true
                    };
                    singleLevelValidationOptions = options;
                }
                return singleLevelValidationOptions;
            }
        }

        public bool SkipConstraints { get; private set; }

        public bool SkipIfCached { get; private set; }

        public bool SkipPrivateChildren { get; private set; }

        public bool StoreTempViolations { get; private set; }

        public static ProcessActivityTreeOptions ValidationOptions
        {
            get
            {
                if (validationOptions == null)
                {
                    ProcessActivityTreeOptions options = new ProcessActivityTreeOptions {
                        SkipPrivateChildren = false,
                        CreateEmptyBindings = false
                    };
                    validationOptions = options;
                }
                return validationOptions;
            }
        }
    }
}

