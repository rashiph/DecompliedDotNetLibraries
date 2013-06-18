namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    [ContentProperty("Nodes")]
    public sealed class Flowchart : NativeActivity
    {
        private Collection<FlowNode> allNodes = new Collection<FlowNode>();
        private Variable<int> currentNode = new Variable<int>();
        private Collection<FlowNode> nodes;
        private CompletionCallback<bool> onDecisionCompleted;
        private CompletionCallback onStepCompleted;
        private Collection<Variable> variables;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetVariablesCollection(this.Variables);
            metadata.AddImplementationVariable(this.currentNode);
            this.GatherAllNodes(metadata);
            HashSet<Activity> children = new HashSet<Activity>();
            for (int i = 0; i < this.allNodes.Count; i++)
            {
                this.allNodes[i].GetChildActivities(children);
            }
            List<Activity> list = new List<Activity>(children.Count);
            foreach (Activity activity in children)
            {
                list.Add(activity);
            }
            metadata.SetChildrenCollection(new Collection<Activity>(list));
        }

        private void DepthFirstVisitNodes(Func<FlowNode, bool> visitNodeCallback, FlowNode start)
        {
            List<FlowNode> connections = new List<FlowNode>();
            Stack<FlowNode> stack = new Stack<FlowNode>();
            if (start != null)
            {
                stack.Push(start);
                while (stack.Count > 0)
                {
                    FlowNode arg = stack.Pop();
                    if ((arg != null) && visitNodeCallback(arg))
                    {
                        connections.Clear();
                        arg.GetConnectedNodes(connections);
                        for (int i = 0; i < connections.Count; i++)
                        {
                            stack.Push(connections[i]);
                        }
                    }
                }
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.StartNode != null)
            {
                if (TD.FlowchartStartIsEnabled())
                {
                    TD.FlowchartStart(base.DisplayName);
                }
                this.ExecuteNodeChain(context, this.StartNode, null);
            }
            else if (TD.FlowchartEmptyIsEnabled())
            {
                TD.FlowchartEmpty(base.DisplayName);
            }
        }

        private void ExecuteNodeChain(NativeActivityContext context, FlowNode node, System.Activities.ActivityInstance completedInstance)
        {
            if (node == null)
            {
                if (context.IsCancellationRequested && (completedInstance.State != ActivityInstanceState.Closed))
                {
                    context.MarkCanceled();
                }
            }
            else if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
            else
            {
                FlowNode node2 = node;
                do
                {
                    FlowNode node3;
                    if (this.ExecuteSingleNode(context, node2, out node3))
                    {
                        node2 = node3;
                    }
                    else
                    {
                        this.currentNode.Set(context, node2.Index);
                        node2 = null;
                    }
                }
                while (node2 != null);
            }
        }

        private bool ExecuteSingleNode(NativeActivityContext context, FlowNode node, out FlowNode nextNode)
        {
            FlowStep step = node as FlowStep;
            if (step != null)
            {
                if (this.onStepCompleted == null)
                {
                    this.onStepCompleted = new CompletionCallback(this.OnStepCompleted);
                }
                return step.Execute(context, this.onStepCompleted, out nextNode);
            }
            nextNode = null;
            FlowDecision decision = node as FlowDecision;
            if (decision != null)
            {
                if (this.onDecisionCompleted == null)
                {
                    this.onDecisionCompleted = new CompletionCallback<bool>(this.OnDecisionCompleted);
                }
                return decision.Execute(context, this.onDecisionCompleted);
            }
            IFlowSwitch switch2 = node as IFlowSwitch;
            return switch2.Execute(context, this);
        }

        private void GatherAllNodes(NativeActivityMetadata metadata)
        {
            Func<FlowNode, bool> visitNodeCallback = null;
            this.allNodes.Clear();
            if ((this.StartNode == null) && (this.Nodes.Count > 0))
            {
                metadata.AddValidationError(System.Activities.SR.FlowchartMissingStartNode(base.DisplayName));
            }
            else
            {
                if (visitNodeCallback == null)
                {
                    visitNodeCallback = n => this.VisitNode(n, metadata);
                }
                this.DepthFirstVisitNodes(visitNodeCallback, this.StartNode);
            }
        }

        private FlowNode GetCurrentNode(NativeActivityContext context)
        {
            int num = this.currentNode.Get(context);
            return this.allNodes[num];
        }

        private void OnDecisionCompleted(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, bool result)
        {
            FlowDecision currentNode = this.GetCurrentNode(context) as FlowDecision;
            FlowNode node = result ? currentNode.True : currentNode.False;
            this.ExecuteNodeChain(context, node, completedInstance);
        }

        private void OnStepCompleted(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            FlowStep currentNode = this.GetCurrentNode(context) as FlowStep;
            FlowNode next = currentNode.Next;
            this.ExecuteNodeChain(context, next, completedInstance);
        }

        internal void OnSwitchCompleted<T>(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, T result)
        {
            FlowNode nextNode = (this.GetCurrentNode(context) as IFlowSwitch).GetNextNode(result);
            this.ExecuteNodeChain(context, nextNode, completedInstance);
        }

        private bool VisitNode(FlowNode node, NativeActivityMetadata metadata)
        {
            if (node.Open(this, metadata))
            {
                node.Index = this.allNodes.Count;
                this.allNodes.Add(node);
                return true;
            }
            return false;
        }

        [DependsOn("StartNode")]
        public Collection<FlowNode> Nodes
        {
            get
            {
                if (this.nodes == null)
                {
                    ValidatingCollection<FlowNode> validatings = new ValidatingCollection<FlowNode> {
                        OnAddValidationCallback = delegate (FlowNode item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.nodes = validatings;
                }
                return this.nodes;
            }
        }

        [DependsOn("Variables")]
        public FlowNode StartNode { get; set; }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    ValidatingCollection<Variable> validatings = new ValidatingCollection<Variable> {
                        OnAddValidationCallback = delegate (Variable item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }
    }
}

