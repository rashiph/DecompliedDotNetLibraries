namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Runtime;

    internal class StateContainer
    {
        private Microsoft.Transactions.Wsat.StateMachines.State completionAborted;
        private Microsoft.Transactions.Wsat.StateMachines.State completionAborting;
        private Microsoft.Transactions.Wsat.StateMachines.State completionActive;
        private Microsoft.Transactions.Wsat.StateMachines.State completionCommitted;
        private Microsoft.Transactions.Wsat.StateMachines.State completionCommitting;
        private Microsoft.Transactions.Wsat.StateMachines.State completionCreated;
        private Microsoft.Transactions.Wsat.StateMachines.State completionCreating;
        private Microsoft.Transactions.Wsat.StateMachines.State completionInitializationFailed;
        private Microsoft.Transactions.Wsat.StateMachines.State completionInitializing;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorAborted;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorActive;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorAwaitingEndOfRecovery;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorCommitted;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorCommitting;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorEnlisted;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorEnlisting;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorFailedRecovery;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorForgotten;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorInitializationFailed;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorInitializing;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorPrepared;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorPreparing;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorReadOnlyInDoubt;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorRecovered;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorRecovering;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorRegisteringBoth;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorRegisteringDurable;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorRegisteringVolatile;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorVolatileActive;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorVolatilePreparing;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorVolatilePreparingRegistered;
        private Microsoft.Transactions.Wsat.StateMachines.State coordinatorVolatilePreparingRegistering;
        private Microsoft.Transactions.Wsat.StateMachines.State durableAborted;
        private Microsoft.Transactions.Wsat.StateMachines.State durableActive;
        private Microsoft.Transactions.Wsat.StateMachines.State durableCommitted;
        private Microsoft.Transactions.Wsat.StateMachines.State durableCommitting;
        private Microsoft.Transactions.Wsat.StateMachines.State durableFailedRecovery;
        private Microsoft.Transactions.Wsat.StateMachines.State durableInDoubt;
        private Microsoft.Transactions.Wsat.StateMachines.State durableInitializationFailed;
        private Microsoft.Transactions.Wsat.StateMachines.State durablePrepared;
        private Microsoft.Transactions.Wsat.StateMachines.State durablePreparing;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRecovering;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRecoveryAwaitingCommit;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRecoveryAwaitingRollback;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRecoveryReceivedCommit;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRecoveryReceivedRollback;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRegistering;
        private Microsoft.Transactions.Wsat.StateMachines.State durableRejoined;
        private Microsoft.Transactions.Wsat.StateMachines.State durableUnregistered;
        private Microsoft.Transactions.Wsat.StateMachines.State subordinateActive;
        private Microsoft.Transactions.Wsat.StateMachines.State subordinateFinished;
        private Microsoft.Transactions.Wsat.StateMachines.State subordinateInitializing;
        private Microsoft.Transactions.Wsat.StateMachines.State subordinateRegistering;
        private Microsoft.Transactions.Wsat.StateMachines.State transactionContextActive;
        private Microsoft.Transactions.Wsat.StateMachines.State transactionContextFinished;
        private Microsoft.Transactions.Wsat.StateMachines.State transactionContextInitializing;
        private Microsoft.Transactions.Wsat.StateMachines.State transactionContextInitializingCoordinator;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileAborted;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileAborting;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileCommitted;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileCommitting;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileInDoubt;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileInitializationFailed;
        private Microsoft.Transactions.Wsat.StateMachines.State volatilePhaseOneUnregistered;
        private Microsoft.Transactions.Wsat.StateMachines.State volatilePhaseZeroActive;
        private Microsoft.Transactions.Wsat.StateMachines.State volatilePhaseZeroUnregistered;
        private Microsoft.Transactions.Wsat.StateMachines.State volatilePrepared;
        private Microsoft.Transactions.Wsat.StateMachines.State volatilePrePrepared;
        private Microsoft.Transactions.Wsat.StateMachines.State volatilePrePreparing;
        private Microsoft.Transactions.Wsat.StateMachines.State volatileRegistering;

        public StateContainer(ProtocolState state)
        {
            this.coordinatorInitializing = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorInitializing(state);
            this.coordinatorEnlisting = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorEnlisting(state);
            this.coordinatorEnlisted = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorEnlisted(state);
            this.coordinatorRegisteringBoth = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorRegisteringBoth(state);
            this.coordinatorRegisteringDurable = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorRegisteringDurable(state);
            this.coordinatorRegisteringVolatile = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorRegisteringVolatile(state);
            this.coordinatorVolatileActive = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorVolatileActive(state);
            this.coordinatorVolatilePreparing = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorVolatilePreparing(state);
            this.coordinatorVolatilePreparingRegistering = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorVolatilePreparingRegistering(state);
            this.coordinatorVolatilePreparingRegistered = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorVolatilePreparingRegistered(state);
            this.coordinatorActive = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorActive(state);
            this.coordinatorPreparing = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorPreparing(state);
            this.coordinatorPrepared = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorPrepared(state);
            this.coordinatorCommitting = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorCommitting(state);
            this.coordinatorRecovering = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorRecovering(state);
            this.coordinatorRecovered = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorRecovered(state);
            this.coordinatorAwaitingEndOfRecovery = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorAwaitingEndOfRecovery(state);
            this.coordinatorFailedRecovery = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorFailedRecovery(state);
            this.coordinatorCommitted = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorCommitted(state);
            this.coordinatorAborted = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorAborted(state);
            this.coordinatorForgotten = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorForgotten(state);
            this.coordinatorReadOnlyInDoubt = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorReadOnlyInDoubt(state);
            this.coordinatorInitializationFailed = new Microsoft.Transactions.Wsat.StateMachines.CoordinatorInitializationFailed(state);
            this.completionInitializing = new Microsoft.Transactions.Wsat.StateMachines.CompletionInitializing(state);
            this.completionCreating = new Microsoft.Transactions.Wsat.StateMachines.CompletionCreating(state);
            this.completionCreated = new Microsoft.Transactions.Wsat.StateMachines.CompletionCreated(state);
            this.completionActive = new Microsoft.Transactions.Wsat.StateMachines.CompletionActive(state);
            this.completionCommitting = new Microsoft.Transactions.Wsat.StateMachines.CompletionCommitting(state);
            this.completionAborting = new Microsoft.Transactions.Wsat.StateMachines.CompletionAborting(state);
            this.completionCommitted = new Microsoft.Transactions.Wsat.StateMachines.CompletionCommitted(state);
            this.completionAborted = new Microsoft.Transactions.Wsat.StateMachines.CompletionAborted(state);
            this.completionInitializationFailed = new Microsoft.Transactions.Wsat.StateMachines.CompletionInitializationFailed(state);
            this.subordinateInitializing = new Microsoft.Transactions.Wsat.StateMachines.SubordinateInitializing(state);
            this.subordinateRegistering = new Microsoft.Transactions.Wsat.StateMachines.SubordinateRegistering(state);
            this.subordinateActive = new Microsoft.Transactions.Wsat.StateMachines.SubordinateActive(state);
            this.subordinateFinished = new Microsoft.Transactions.Wsat.StateMachines.SubordinateFinished(state);
            this.durableRegistering = new Microsoft.Transactions.Wsat.StateMachines.DurableRegistering(state);
            this.durableActive = new Microsoft.Transactions.Wsat.StateMachines.DurableActive(state);
            this.durableUnregistered = new Microsoft.Transactions.Wsat.StateMachines.DurableUnregistered(state);
            this.durablePreparing = new Microsoft.Transactions.Wsat.StateMachines.DurablePreparing(state);
            this.durablePrepared = new Microsoft.Transactions.Wsat.StateMachines.DurablePrepared(state);
            this.durableCommitting = new Microsoft.Transactions.Wsat.StateMachines.DurableCommitting(state);
            this.durableRecovering = new Microsoft.Transactions.Wsat.StateMachines.DurableRecovering(state);
            this.durableRejoined = new Microsoft.Transactions.Wsat.StateMachines.DurableRejoined(state);
            this.durableRecoveryAwaitingCommit = new Microsoft.Transactions.Wsat.StateMachines.DurableRecoveryAwaitingCommit(state);
            this.durableRecoveryReceivedCommit = new Microsoft.Transactions.Wsat.StateMachines.DurableRecoveryReceivedCommit(state);
            this.durableRecoveryAwaitingRollback = new Microsoft.Transactions.Wsat.StateMachines.DurableRecoveryAwaitingRollback(state);
            this.durableRecoveryReceivedRollback = new Microsoft.Transactions.Wsat.StateMachines.DurableRecoveryReceivedRollback(state);
            this.durableFailedRecovery = new Microsoft.Transactions.Wsat.StateMachines.DurableFailedRecovery(state);
            this.durableCommitted = new Microsoft.Transactions.Wsat.StateMachines.DurableCommitted(state);
            this.durableAborted = new Microsoft.Transactions.Wsat.StateMachines.DurableAborted(state);
            this.durableInDoubt = new Microsoft.Transactions.Wsat.StateMachines.DurableInDoubt(state);
            this.durableInitializationFailed = new Microsoft.Transactions.Wsat.StateMachines.DurableInitializationFailed(state);
            this.volatileRegistering = new Microsoft.Transactions.Wsat.StateMachines.VolatileRegistering(state);
            this.volatilePhaseZeroActive = new Microsoft.Transactions.Wsat.StateMachines.VolatilePhaseZeroActive(state);
            this.volatilePhaseZeroUnregistered = new Microsoft.Transactions.Wsat.StateMachines.VolatilePhaseZeroUnregistered(state);
            this.volatilePhaseOneUnregistered = new Microsoft.Transactions.Wsat.StateMachines.VolatilePhaseOneUnregistered(state);
            this.volatilePrePreparing = new Microsoft.Transactions.Wsat.StateMachines.VolatilePrePreparing(state);
            this.volatilePrePrepared = new Microsoft.Transactions.Wsat.StateMachines.VolatilePrePrepared(state);
            this.volatilePrepared = new Microsoft.Transactions.Wsat.StateMachines.VolatilePrepared(state);
            this.volatileCommitting = new Microsoft.Transactions.Wsat.StateMachines.VolatileCommitting(state);
            this.volatileAborting = new Microsoft.Transactions.Wsat.StateMachines.VolatileAborting(state);
            this.volatileCommitted = new Microsoft.Transactions.Wsat.StateMachines.VolatileCommitted(state);
            this.volatileAborted = new Microsoft.Transactions.Wsat.StateMachines.VolatileAborted(state);
            this.volatileInDoubt = new Microsoft.Transactions.Wsat.StateMachines.VolatileInDoubt(state);
            this.volatileInitializationFailed = new Microsoft.Transactions.Wsat.StateMachines.VolatileInitializationFailed(state);
            this.transactionContextInitializing = new Microsoft.Transactions.Wsat.StateMachines.TransactionContextInitializing(state);
            this.transactionContextInitializingCoordinator = new Microsoft.Transactions.Wsat.StateMachines.TransactionContextInitializingCoordinator(state);
            this.transactionContextActive = new Microsoft.Transactions.Wsat.StateMachines.TransactionContextActive(state);
            this.transactionContextFinished = new Microsoft.Transactions.Wsat.StateMachines.TransactionContextFinished(state);
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionAborted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionAborted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionAborting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionAborting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionCommitted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionCommitted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionCommitting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionCommitting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionCreated
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionCreated;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionCreating
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionCreating;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionInitializationFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionInitializationFailed;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CompletionInitializing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completionInitializing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorAborted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorAborted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorAwaitingEndOfRecovery
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorAwaitingEndOfRecovery;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorCommitted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorCommitted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorCommitting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorCommitting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorEnlisted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorEnlisted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorEnlisting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorEnlisting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorFailedRecovery
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorFailedRecovery;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorForgotten
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorForgotten;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorInitializationFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorInitializationFailed;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorInitializing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorInitializing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorPrepared
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorPrepared;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorPreparing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorPreparing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorReadOnlyInDoubt
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorReadOnlyInDoubt;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorRecovered
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorRecovered;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorRecovering
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorRecovering;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorRegisteringBoth
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorRegisteringBoth;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorRegisteringDurable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorRegisteringDurable;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorRegisteringVolatile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorRegisteringVolatile;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorVolatileActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorVolatileActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorVolatilePreparing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorVolatilePreparing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorVolatilePreparingRegistered
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorVolatilePreparingRegistered;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State CoordinatorVolatilePreparingRegistering
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorVolatilePreparingRegistering;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableAborted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableAborted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableCommitted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableCommitted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableCommitting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableCommitting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableFailedRecovery
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableFailedRecovery;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableInDoubt
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableInDoubt;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableInitializationFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableInitializationFailed;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurablePrepared
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durablePrepared;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurablePreparing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durablePreparing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRecovering
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRecovering;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRecoveryAwaitingCommit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRecoveryAwaitingCommit;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRecoveryAwaitingRollback
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRecoveryAwaitingRollback;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRecoveryReceivedCommit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRecoveryReceivedCommit;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRecoveryReceivedRollback
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRecoveryReceivedRollback;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRegistering
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRegistering;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableRejoined
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableRejoined;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State DurableUnregistered
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.durableUnregistered;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State SubordinateActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subordinateActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State SubordinateFinished
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subordinateFinished;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State SubordinateInitializing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subordinateInitializing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State SubordinateRegistering
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subordinateRegistering;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State TransactionContextActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionContextActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State TransactionContextFinished
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionContextFinished;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State TransactionContextInitializing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionContextInitializing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State TransactionContextInitializingCoordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionContextInitializingCoordinator;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileAborted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileAborted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileAborting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileAborting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileCommitted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileCommitted;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileCommitting
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileCommitting;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileInDoubt
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileInDoubt;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileInitializationFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileInitializationFailed;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatilePhaseOneUnregistered
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatilePhaseOneUnregistered;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatilePhaseZeroActive
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatilePhaseZeroActive;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatilePhaseZeroUnregistered
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatilePhaseZeroUnregistered;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatilePrepared
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatilePrepared;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatilePrePrepared
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatilePrePrepared;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatilePrePreparing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatilePrePreparing;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State VolatileRegistering
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileRegistering;
            }
        }
    }
}

