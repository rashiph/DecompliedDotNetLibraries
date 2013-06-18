namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal sealed class InternalTransition
    {
        private Collection<TransitionData> transitionDataList;

        public int InternalTransitionIndex { get; set; }

        public bool IsUnconditional
        {
            get
            {
                return ((this.transitionDataList.Count == 1) && (this.transitionDataList[0].Condition == null));
            }
        }

        public Collection<TransitionData> TransitionDataList
        {
            get
            {
                if (this.transitionDataList == null)
                {
                    this.transitionDataList = new Collection<TransitionData>();
                }
                return this.transitionDataList;
            }
        }

        public Activity Trigger { get; set; }
    }
}

