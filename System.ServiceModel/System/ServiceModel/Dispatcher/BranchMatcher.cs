namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BranchMatcher
    {
        private int resultCount;
        private QueryBranchResultSet resultTable;
        internal BranchMatcher(int resultCount, QueryBranchResultSet resultTable)
        {
            this.resultCount = resultCount;
            this.resultTable = resultTable;
        }

        internal QueryBranchResultSet ResultTable
        {
            get
            {
                return this.resultTable;
            }
        }
        private void InitResults(ProcessingContext context)
        {
            context.PushFrame();
            context.Push(false, this.resultCount);
        }

        internal void InvokeMatches(ProcessingContext context)
        {
            switch (this.resultTable.Count)
            {
                case 0:
                    break;

                case 1:
                    this.InvokeSingleMatch(context);
                    break;

                default:
                    this.InvokeMultiMatch(context);
                    return;
            }
        }

        private void InvokeMultiMatch(ProcessingContext context)
        {
            int counterMarker = context.Processor.CounterMarker;
            BranchContext context2 = new BranchContext(context);
            int count = this.resultTable.Count;
            int num3 = 0;
            while (num3 < count)
            {
                ProcessingContext context3;
                QueryBranchResult result = this.resultTable[num3];
                QueryBranch branch = result.Branch;
                Opcode next = branch.Branch.Next;
                if (next.TestFlag(OpcodeFlags.NoContextCopy))
                {
                    context3 = context;
                }
                else
                {
                    context3 = context2.Create();
                }
                this.InitResults(context3);
                context3.Values[context3.TopArg[result.ValIndex]].Boolean = true;
                while (++num3 < count)
                {
                    result = this.resultTable[num3];
                    if (branch.ID != result.Branch.ID)
                    {
                        break;
                    }
                    context3.Values[context3.TopArg[result.ValIndex]].Boolean = true;
                }
                try
                {
                    context3.EvalCodeBlock(next);
                }
                catch (XPathNavigatorException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(next));
                }
                catch (NavigatorInvalidBodyAccessException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(next));
                }
                context.Processor.CounterMarker = counterMarker;
            }
            context2.Release();
        }

        internal void InvokeNonMatches(ProcessingContext context, QueryBranchTable nonMatchTable)
        {
            int counterMarker = context.Processor.CounterMarker;
            BranchContext context2 = new BranchContext(context);
            int num2 = 0;
            int num3 = 0;
            while ((num3 < this.resultTable.Count) && (num2 < nonMatchTable.Count))
            {
                QueryBranchResult result = this.resultTable[num3];
                int num4 = result.Branch.ID - nonMatchTable[num2].ID;
                if (num4 > 0)
                {
                    ProcessingContext context3 = context2.Create();
                    this.InvokeNonMatch(context3, nonMatchTable[num2]);
                    context.Processor.CounterMarker = counterMarker;
                    num2++;
                }
                else
                {
                    if (num4 == 0)
                    {
                        num2++;
                        continue;
                    }
                    num3++;
                }
            }
            while (num2 < nonMatchTable.Count)
            {
                ProcessingContext context4 = context2.Create();
                this.InvokeNonMatch(context4, nonMatchTable[num2]);
                context.Processor.CounterMarker = counterMarker;
                num2++;
            }
            context2.Release();
        }

        private void InvokeNonMatch(ProcessingContext context, QueryBranch branch)
        {
            context.PushFrame();
            context.Push(false, this.resultCount);
            try
            {
                context.EvalCodeBlock(branch.Branch);
            }
            catch (XPathNavigatorException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(branch.Branch));
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(branch.Branch));
            }
        }

        private void InvokeSingleMatch(ProcessingContext context)
        {
            int counterMarker = context.Processor.CounterMarker;
            QueryBranchResult result = this.resultTable[0];
            this.InitResults(context);
            context.Values[context.TopArg[result.ValIndex]].Boolean = true;
            try
            {
                context.EvalCodeBlock(result.Branch.Branch.Next);
            }
            catch (XPathNavigatorException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(result.Branch.Branch.Next));
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(result.Branch.Branch.Next));
            }
            context.Processor.CounterMarker = counterMarker;
        }

        internal void Release(ProcessingContext context)
        {
            context.Processor.ReleaseResults(this.resultTable);
        }
    }
}

