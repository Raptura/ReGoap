using System;
using System.Collections.Generic;
using ReGoap.Core;

namespace ReGoap.Planner
{
    public class ReGoapNode<T, W> : INode<ReGoapState<T, W>>
    {
        private float cost;
        private IGoapPlanner<T, W> planner;
        private ReGoapNode<T, W> parent;
        private IReGoapAction<T, W> action;
        private IReGoapActionSettings<T, W> actionSettings;
        private ReGoapState<T, W> state;
        private ReGoapState<T, W> goal;
        private float g;
        private float h;

        //private float heuristicMultiplier = 1;

        private readonly List<INode<ReGoapState<T, W>>> expandList;

        private readonly List<T> tmpKeys = new List<T>();

        //public int AstarID { get; set; }
        public string Name { get { return action == null ? "NoAction" : action.GetName(); } }
        public string GoalString { get { return goal.ToString(); } }
        public string EffectString { get { return action != null ? action.GetEffects(goal).ToString() : ""; } }
        public string PrecondString { get { return action != null ? action.GetPreconditions(goal).ToString() : ""; } }

        private ReGoapNode()
        {
            expandList = new List<INode<ReGoapState<T, W>>>();
        }

        private void Init(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action)
        {
            expandList.Clear();
            tmpKeys.Clear();

            this.planner = planner;
            this.parent = parent;
            this.action = action;
            if (action != null)
                actionSettings = action.GetSettings(planner.GetCurrentAgent(), newGoal);

            if (parent != null)
            {
                state = parent.GetState().Clone();
                // g(node)
                g = parent.GetPathCost();
            }
            else
            {
                state = planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            }

            var nextAction = parent == null ? null : parent.action;
            if (action != null)
            {
                // create a new instance of the goal based on the paren't goal
                goal = ReGoapState<T, W>.Instantiate();
                var tmpGoal = ReGoapState<T, W>.Instantiate(newGoal);

                var preconditions = action.GetPreconditions(tmpGoal, nextAction);
                var effects = action.GetEffects(tmpGoal, nextAction);
                // adding the action's effects to the current node's state
                state.AddFromState(effects);
                // addding the action's cost to the node's total cost
                g += action.GetCost(tmpGoal, nextAction);

                //// add all preconditions of the current action to the goal
                //tmpGoal.AddFromState(preconditions);
                //// removes from goal all the conditions that are now fulfilled in the node's state
                //tmpGoal.ReplaceWithMissingDifference(state);
                ////goal.ReplaceWithMissingDifference(effects);

                // collect all keys from goal & precondition, unique-ed
                foreach(var pr in tmpGoal.GetValues())
                {
                    var k = pr.Key;
                    if( !tmpKeys.Contains(k))
                        tmpKeys.Add(k);
                }
                foreach(var pr in preconditions.GetValues())
                {
                    var k = pr.Key;
                    if (!tmpKeys.Contains(k))
                        tmpKeys.Add(k);
                }

                // process each keys
                foreach(var k in tmpKeys)
                {
                    StructValue goalValue, effectValue, precondValue, stateValue, protoValue;
                    tmpGoal.GetValues().TryGetValue(k, out goalValue);
                    effects.GetValues().TryGetValue(k, out effectValue);
                    preconditions.GetValues().TryGetValue(k, out precondValue);
                    state.GetValues().TryGetValue(k, out stateValue);

                    StructValue.EValueType valueType;
                    _GetValueType(ref goalValue, ref effectValue, ref precondValue, ref stateValue, out valueType, out protoValue);
                    if( valueType == StructValue.EValueType.Arithmetic )
                    {

                        //_EnsureArithStructValueInited(ref goalValue, ref protoValue);
                        _EnsureArithStructValueInited(ref effectValue, ref protoValue);
                        _EnsureArithStructValueInited(ref precondValue, ref protoValue);
                        _EnsureArithStructValueInited(ref stateValue, ref protoValue);
                        if (!goalValue.Inited)
                            goalValue = StructValue.CopyCreate(ref stateValue, -(Convert.ToSingle(stateValue.v) - Convert.ToSingle(effectValue.v)) );

                        float fGoal = Convert.ToSingle(goalValue.v);
                        float fEffect = Convert.ToSingle(effectValue.v);
                        float fPrecond = Convert.ToSingle(precondValue.v);
                        float fState = Convert.ToSingle(stateValue.v);

                        float finalV = Math.Max(
                            fGoal - fEffect,
                            Math.Min(fPrecond, fPrecond - fState)
                        );

                        var sv = StructValue.CopyCreate(ref protoValue, finalV);

                        goal.SetStructValue(k, sv);
                    }
                    else if(valueType == StructValue.EValueType.Other)
                    {
                        //ReplaceWithMissingDifference
                        if (stateValue.Inited && goalValue.Inited && goalValue.IsFulfilledBy(stateValue))
                            goalValue.Invalidate();

                        // AddFromPrecond 
                        // 1. if the precond is satisfied by the memory start state, then discard
                        // 2. else this newly added goal from precond, should not be removed due to fulfilled by curStateValue
                        if (precondValue.Inited)
                        {
                            bool preCondfulfilledByMem = false;
                            var startMemoryState = planner.GetCurrentAgent().GetMemory().GetWorldState();
                            StructValue startMemoryValue;
                            if(startMemoryState.GetValues().TryGetValue(k, out startMemoryValue))
                            {
                                if( startMemoryValue.Inited && precondValue.IsFulfilledBy(startMemoryValue) )
                                {
                                    preCondfulfilledByMem = true; 
                                }
                            }

                            if( !preCondfulfilledByMem )
                            {
                                if (goalValue.Inited)
                                    goalValue = goalValue.MergeWith(precondValue);
                                else
                                    goalValue = precondValue;
                            }
                            
                        }

                        if (goalValue.Inited)
                            goal.SetStructValue(k, goalValue);

                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Unexpected StructValue type: " + valueType);
                    }

                }// foreach (var k in tmpKeys)

                tmpGoal.Recycle();

            }
            else
            {
                var diff = ReGoapState<T, W>.Instantiate();
                newGoal.MissingDifference(state, ref diff);
                goal = diff;

            }

            h = _CalculateH();

            // f(node) = g(node) + h(node)
            cost = g + h * planner.GetSettings().HeuristicMultiplier;
        }

        private float _CalculateH()
        {
            float h = 0;
            foreach(var pr in goal.GetValues())
            {
                var pairValue = pr.Value;
                if (pairValue.tp == StructValue.EValueType.Other)
                {
                    ++h;
                }
                else if( pairValue.tp == StructValue.EValueType.Arithmetic)
                {
                    float goalValue = Convert.ToSingle(pairValue.v);
                    var defValue = StructValue.CopyCreate(ref pairValue, 0);
                    if( ! pairValue.IsFulfilledBy(defValue) )
                        h += Math.Abs(goalValue);
                }
            }
            return h;
        }

        private void _EnsureArithStructValueInited(ref StructValue sv, ref StructValue proto)
        {
            if( !sv.Inited )
            {
                sv = StructValue.CopyCreate(ref proto, 0);
            }
        }

        private void _GetValueType(ref StructValue goalValue, ref StructValue effectValue, ref StructValue precondValue, ref StructValue stateValue, 
            out StructValue.EValueType valueType,
            out StructValue protoValue)
        {
            if (goalValue.Inited)
            {
                valueType = goalValue.tp;
                protoValue = goalValue;
            }
            else if (effectValue.Inited)
            {
                valueType = effectValue.tp;
                protoValue = effectValue;
            }
            else if (precondValue.Inited)
            {
                valueType = precondValue.tp;
                protoValue = precondValue;
            }
            else if (stateValue.Inited)
            {
                valueType = stateValue.tp;
                protoValue = stateValue;
            }
            else
            {
                UnityEngine.Debug.LogError("ReGoapNode._GetValueType: failed to find inited value");
                valueType = StructValue.EValueType.Other;
                protoValue = StructValue.Create(null);
            }
            
        }

        #region NodeFactory
        private static Stack<ReGoapNode<T, W>> cachedNodes;

        public static void Warmup(int count)
        {
            cachedNodes = new Stack<ReGoapNode<T, W>>(count);
            for (int i = 0; i < count; i++)
            {
                cachedNodes.Push(new ReGoapNode<T, W>());
            }
        }

        public void Recycle()
        {
            state.Recycle();
            state = null;
            goal.Recycle();
            goal = null;
            lock (cachedNodes)
            {
                cachedNodes.Push(this);
            }
        }

        public static ReGoapNode<T, W> Instantiate(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action)
        {
            ReGoapNode<T, W> node;
            if (cachedNodes == null)
            {
                cachedNodes = new Stack<ReGoapNode<T, W>>();
            }
            lock (cachedNodes)
            {
                node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new ReGoapNode<T, W>();
            }
            node.Init(planner, newGoal, parent, action);
            return node;
        }
        #endregion

        public float GetPathCost()
        {
            return g;
        }

        public float GetHeuristicCost()
        {
            return h;
        }

        public ReGoapState<T, W> GetState()
        {
            return state;
        }

        public List<INode<ReGoapState<T, W>>> Expand()
        {
            expandList.Clear();

            var agent = planner.GetCurrentAgent();
            var actions = agent.GetActionsSet();
            for (var index = actions.Count - 1; index >= 0; index--)
            {
                var possibleAction = actions[index];
                possibleAction.Precalculations(agent, goal);
                var precond = possibleAction.GetPreconditions(goal, action);
                var effects = possibleAction.GetEffects(goal, action);

                if (effects.HasAnyGoodForGoal(state, goal) && // any effect is the current goal
                    !goal.HasAnyConflictPrecond(effects, precond) && // no precondition is conflicting with the goal (non-arithmetic)
                    !goal.HasAnyConflictEffect(effects) && //no effect is conflicting with goal (non-arithmetic)
                    !goal.IsNotHelpfulAtAll(effects, precond, state) && //(arithmetic)
                    possibleAction.CheckProceduralCondition(agent, goal, parent != null ? parent.action : null))
                {
                    var newGoal = goal;
                    var newNode = Instantiate(planner, newGoal, this, possibleAction);
                    expandList.Add(newNode);
                    //Utilities.ReGoapLogger.Log(string.Format("   oooo Expanded node: action: {0}\n\t effect {1}\n\t precond {2}\n\t goal {3}", possibleAction.GetName(), effects, precond, newNode.GoalString));
                }
                else
                {
                    //Utilities.ReGoapLogger.Log(string.Format("   xxxx Expanded node: action: {0}\n\t effect {1}\n\t precond {2}", possibleAction.GetName(), effects, precond));
                }
            }
            return expandList;
        }


        private IReGoapAction<T, W> GetAction()
        {
            return action;
        }

        public Queue<ReGoapActionState<T, W>> CalculatePath()
        {
            var result = new Queue<ReGoapActionState<T, W>>();
            CalculatePath(ref result);
            return result;
        }

        public void CalculatePath(ref Queue<ReGoapActionState<T, W>> result)
        {
            var node = this;
            while (node.GetParent() != null)
            {
                result.Enqueue(new ReGoapActionState<T, W>(node.action, node.actionSettings));
                node = (ReGoapNode<T, W>)node.GetParent();
            }
        }

        public int CompareTo(INode<ReGoapState<T, W>> other)
        {
            return cost.CompareTo(other.GetCost());
        }

        public float GetCost()
        {
            return cost;
        }

        public INode<ReGoapState<T, W>> GetParent()
        {
            return parent;
        }

        public bool IsGoal(ReGoapState<T, W> goal)
        {
            return h <= 0;
        }

        public float Priority { get; set; }
        public long InsertionIndex { get; set; }
        public int QueueIndex { get; set; }
    }
}