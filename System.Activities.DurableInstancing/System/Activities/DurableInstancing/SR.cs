namespace System.Activities.DurableInstancing
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal class SR
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private SR()
        {
        }

        internal static string CanNotDefineNullForAPromotion(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CanNotDefineNullForAPromotion", Culture), new object[] { param0, param1 });
        }

        internal static string CannotPromoteAsSqlVariant(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotPromoteAsSqlVariant", Culture), new object[] { param0, param1 });
        }

        internal static string CannotPromoteXNameTwiceInPromotion(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotPromoteXNameTwiceInPromotion", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidInstanceLocksRecoveryPeriod(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidInstanceLocksRecoveryPeriod", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidLockRenewalPeriod(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidLockRenewalPeriod", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidRunnableInstancesDetectionPeriod(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidRunnableInstancesDetectionPeriod", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidWorkflowHostTypeValue(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidWorkflowHostTypeValue", Culture), new object[] { param0 });
        }

        internal static string NoPromotionsDefined(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("NoPromotionsDefined", Culture), new object[] { param0 });
        }

        internal static string PromotionAlreadyDefined(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PromotionAlreadyDefined", Culture), new object[] { param0 });
        }

        internal static string PromotionTooManyDefined(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("PromotionTooManyDefined", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string TimeoutOnSqlOperation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutOnSqlOperation", Culture), new object[] { param0 });
        }

        internal static string UnknownCompressionOption(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnknownCompressionOption", Culture), new object[] { param0 });
        }

        internal static string UnknownSprocResult(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnknownSprocResult", Culture), new object[] { param0 });
        }

        internal static string CleanupInProgress
        {
            get
            {
                return ResourceManager.GetString("CleanupInProgress", Culture);
            }
        }

        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string HostLockExpired
        {
            get
            {
                return ResourceManager.GetString("HostLockExpired", Culture);
            }
        }

        internal static string HostLockNotFound
        {
            get
            {
                return ResourceManager.GetString("HostLockNotFound", Culture);
            }
        }

        internal static string InstanceKeyMetadataChangesNotSupported
        {
            get
            {
                return ResourceManager.GetString("InstanceKeyMetadataChangesNotSupported", Culture);
            }
        }

        internal static string InstanceStoreReadOnly
        {
            get
            {
                return ResourceManager.GetString("InstanceStoreReadOnly", Culture);
            }
        }

        internal static string MultipleLockOwnersNotSupported
        {
            get
            {
                return ResourceManager.GetString("MultipleLockOwnersNotSupported", Culture);
            }
        }

        internal static string NonWASActivationNotSupported
        {
            get
            {
                return ResourceManager.GetString("NonWASActivationNotSupported", Culture);
            }
        }

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Activities.DurableInstancing.SR", typeof(System.Activities.DurableInstancing.SR).Assembly);
                    resourceManager = manager;
                }
                return resourceManager;
            }
        }

        internal static string TryLoadRequiresWorkflowType
        {
            get
            {
                return ResourceManager.GetString("TryLoadRequiresWorkflowType", Culture);
            }
        }
    }
}

