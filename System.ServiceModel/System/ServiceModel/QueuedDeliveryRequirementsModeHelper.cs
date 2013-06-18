namespace System.ServiceModel
{
    using System;

    internal static class QueuedDeliveryRequirementsModeHelper
    {
        public static bool IsDefined(QueuedDeliveryRequirementsMode x)
        {
            if ((x != QueuedDeliveryRequirementsMode.Allowed) && (x != QueuedDeliveryRequirementsMode.Required))
            {
                return (x == QueuedDeliveryRequirementsMode.NotAllowed);
            }
            return true;
        }
    }
}

