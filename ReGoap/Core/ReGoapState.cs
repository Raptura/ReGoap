using System;
using System.Collections.Generic;

namespace ReGoap.Core
{
    public class ReGoapState<T, W>
    {
        // can change to object
        private Dictionary<T, StructValue > values;
        private readonly Dictionary<T, StructValue> bufferA;
        private readonly Dictionary<T, StructValue> bufferB;

        public static int DefaultSize = 20;

        private ReGoapState()
        {
            bufferA = new Dictionary<T, StructValue>(DefaultSize);
            bufferB = new Dictionary<T, StructValue>(DefaultSize);
            values = bufferA;
        }

        private void Init(ReGoapState<T, W> old)
        {
            values.Clear();
            if (old != null)
            {
                lock (old.values)
                {
                    foreach (var pair in old.values)
                    {
                        values[pair.Key] = pair.Value; //just copy is enough, we're using struct here
                    }
                }
            }
        }

        //public static ReGoapState<T, W> operator +(ReGoapState<T, W> a, ReGoapState<T, W> b)
        //{
        //    ReGoapState<T, W> result;
        //    lock (a.values)
        //    {
        //        result = Instantiate(a);
        //    }
        //    lock (b.values)
        //    {
        //        foreach (var pair in b.values)
        //            result.values[pair.Key] = pair.Value;
        //        return result;
        //    }
        //}

        public void AddFromState(ReGoapState<T, W> b)
        {
            lock (values) lock (b.values)
                {
                    foreach (var pair in b.values)
                    {
                        StructValue thisValue;
                        if( values.TryGetValue(pair.Key, out thisValue) )
                        {
                            values[pair.Key] = thisValue.MergeWith(pair.Value);
                        }
                        else
                        {
                            values[pair.Key] = pair.Value;
                        }
                        
                    }
                }
        }

        public int Count
        {
            get { return values.Count; }
        }

        ///// <summary>
        ///// If this and other share the same key
        ///// </summary>
        //public bool HasAny(ReGoapState<T, W> other)
        //{
        //    lock (values) lock (other.values)
        //    {
        //        foreach (var pair in other.values)
        //        {
        //            StructValue thisValue;
        //            values.TryGetValue(pair.Key, out thisValue);
        //            if (Equals(thisValue, pair.Value))
        //                return true;
        //        }
        //        return false;
        //    }
        //}

        /// <summary>
        /// if anything belongs to self that is also in goal, is better than curState, then return true 
        /// </summary>
        public bool HasAnyGoodForGoal(ReGoapState<T, W> curState, ReGoapState<T, W> goal)
        {
            lock(values)
            lock (goal.values)
            lock (curState)
            {
                foreach(var pair in goal.values)
                {
                    StructValue thisValue;
                    StructValue curValue;
                    if (values.TryGetValue(pair.Key, out thisValue))
                    {
                        if (!curState.values.TryGetValue(pair.Key, out curValue))
                            return true;
                        if (thisValue.IsBetter(curValue, pair.Value))
                            return true;
                    }
                }
                return false;
            }
        }


        //public bool HasAnyConflict(ReGoapState<T, W> other) // used only in backward for now
        //{
        //    lock (values) lock (other.values)
        //        {
        //            foreach (var pair in other.values)
        //            {
        //                W thisValue;
        //                values.TryGetValue(pair.Key, out thisValue);
        //                var otherValue = pair.Value;
        //                if (otherValue == null || Equals(otherValue, false))
        //                    continue;
        //                if (thisValue != null && !Equals(otherValue, thisValue))
        //                    return true;
        //            }
        //            return false;
        //        }
        //}

        /// <summary>
        /// For non-arithmetic values,
        ///     if a child node's precond overwrite one of the goal's value, then it's conflict
        ///     
        /// REASON: (non-arithmetic value)
        ///     if a child node modifies the precond's value, then the ancestor node's precond is ignored by children nodes
        /// </summary>
        public bool HasAnyConflictPrecond(ReGoapState<T, W> effects, ReGoapState<T, W> precond) // used only in backward for now //effect, precond
        {
            lock (values) lock (precond.values)
                {
                    foreach (var pair in precond.values)
                    {
                        T key = pair.Key;
                        StructValue precondValue = pair.Value;

                        if (precondValue.tp != StructValue.EValueType.Other)
                            continue;

                        StructValue thisValue;
                        StructValue effectValue;

                        values.TryGetValue(key, out thisValue);
                        effects.values.TryGetValue(key, out effectValue);

                        if (
                             thisValue.Inited && !thisValue.IsFulfilledBy(precondValue) &&
                             (!effectValue.Inited || !thisValue.IsFulfilledBy(effectValue))
                           )
                        {
                            return true;
                        }
                    }
                    return false;
                }
        }

        /// <summary>
        /// For non-arithmetic values,
        ///     if an effect's value invalidates a goal's value, then this effect is unacceptable
        ///     
        /// REASON: (non-arithmetic values)
        ///     if a child node changes a goal's value to invalid, the ancestor node cannot set it back
        ///     it's okay for a child to leave the goal's value as invalid, but it cannot set it from valid to invalid, or from invalid to invalid
        /// </summary>
        public bool HasAnyConflictEffect(ReGoapState<T,W> effect)
        {
            lock (values) lock (effect.values)
                {
                    foreach(var pair in effect.values)
                    {
                        T key = pair.Key;
                        StructValue effectValue = pair.Value;

                        if (effectValue.tp != StructValue.EValueType.Other)
                            continue;

                        StructValue goalValue;
                        if( values.TryGetValue(key, out goalValue) )
                        {
                            if (!goalValue.IsFulfilledBy(effectValue))
                                return true;
                        }
                    }
                    return false;
                }
        }

        /// <summary>
        /// 
        /// if all arithmetic values don't make the better towards goal
        ///     return true;
        /// else 
        ///     return false;
        /// 
        /// </summary>
        public bool IsNotHelpfulAtAll(ReGoapState<T,W> effect, ReGoapState<T,W> precond, ReGoapState<T,W> curState)
        {
            lock(values) lock(effect.values)
                {
                    bool nonHelpful = true;
                    foreach(var pair in effect.values)
                    {
                        T key = pair.Key;
                        StructValue effectValue = pair.Value;
                        StructValue curStateValue;

                        if (effectValue.tp == StructValue.EValueType.Arithmetic)
                        {
                            StructValue goalValue;
                            curState.values.TryGetValue(key, out curStateValue);

                            if (values.TryGetValue(key, out goalValue))
                            {// if a goal is satisfied already, then this effect is not useful (not really, but this helps to prune the search tree)
                                if (goalValue.IsFulfilledBy(effectValue))
                                {
                                    var defValue = StructValue.CopyCreate(ref goalValue, 0);
                                    if (! goalValue.IsFulfilledBy(defValue)) //if the goal has just been satified by this effect, okay
                                    { //e.g: for non-neg int, goalValue > 0
                                        nonHelpful = false;
                                        break;
                                    }
                                }
                                else if (effectValue.IsBetter(curStateValue, goalValue)) //not fulfill the target, but make it better towards target
                                {
                                    nonHelpful = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            StructValue goalValue;
                            if( values.TryGetValue(key, out goalValue) )
                            {
                                if( goalValue.IsFulfilledBy(effectValue) )
                                {
                                    curState.values.TryGetValue(key, out curStateValue);
                                    if( !curStateValue.Inited || !goalValue.IsFulfilledBy(curStateValue) )
                                    {
                                        nonHelpful = false;
                                        break;
                                    }
                                }
                            }
                            
                        }                        
                    }
                    return nonHelpful;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MissingDifference(ReGoapState<T, W> curState, int stopAt = int.MaxValue)
        {
            lock (values)
            {
                var count = 0;
                foreach (var pair in values)
                {
                    StructValue precondValue = pair.Value;
                    StructValue curValue;
                    if (
                        !curState.values.TryGetValue(pair.Key, out curValue)   //current-state doesn't have corresponding entry for this precond
                        || !precondValue.IsFulfilledBy(curValue)  //current-state doesn't fullfil this precond
                       )
                    {
                        count++;
                        if (count >= stopAt)
                            break;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// write into 'difference', anything in 'this' and not fulfilled by 'other'
        /// </summary>
        public int MissingDifference(ReGoapState<T, W> other, ref ReGoapState<T, W> difference, int stopAt = int.MaxValue)
        {
            lock (values)
            {
                var count = 0;
                foreach (var pair in values)
                {
                    T key = pair.Key;
                    StructValue thisValue = pair.Value;
                    StructValue otherValue;
                    bool hasOtherValue = other.values.TryGetValue(key, out otherValue);
                    if( 
                        ! hasOtherValue   //not exist in other
                        || ! thisValue.IsFulfilledBy(otherValue)  //not fulfilled by other
                      )
                    {
                        count++;
                        if (difference != null)
                        {
                            if (hasOtherValue)
                                difference.values[key] = thisValue.DiffWith(otherValue);
                            else
                                difference.values[key] = thisValue;
                        }
                        if (count >= stopAt)
                            break;
                    }
                }
                return count;
            }
        }

        // keep only missing differences in values
        public int ReplaceWithMissingDifference(ReGoapState<T, W> other, int stopAt = int.MaxValue, Func<KeyValuePair<T, W>, W, bool> predicate = null, bool test = false)
        {
            lock (values)
            {
                var count = 0;
                var buffer = values;
                values = values == bufferA ? bufferB : bufferA;
                values.Clear();
                foreach (var pair in buffer)
                {
                    T key = pair.Key;
                    StructValue thisValue = pair.Value;
                    StructValue otherValue;
                    bool hasOtherValue = other.values.TryGetValue(key, out otherValue);
                    if( 
                        ! hasOtherValue //not exist in other
                        || ! thisValue.IsFulfilledBy(otherValue) //not fulfilled by other
                      )
                    {
                        count++;
                        if (hasOtherValue)
                            values[key] = thisValue.DiffWith(otherValue);
                        else
                            values[key] = thisValue;
                        if (count >= stopAt)
                            break;
                    }
                }
                return count;
            }
        }

        public ReGoapState<T, W> Clone()
        {
            return Instantiate(this);
        }


        #region StateFactory
        private static Stack<ReGoapState<T, W>> cachedStates;

        public static void Warmup(int count)
        {
            cachedStates = new Stack<ReGoapState<T, W>>(count);
            for (int i = 0; i < count; i++)
            {
                cachedStates.Push(new ReGoapState<T, W>());
            }
        }

        public void Recycle()
        {
            lock (cachedStates)
            {
                cachedStates.Push(this);
            }
        }

        public static ReGoapState<T, W> Instantiate(ReGoapState<T, W> old = null)
        {
            ReGoapState<T, W> state;
            if (cachedStates == null)
            {
                cachedStates = new Stack<ReGoapState<T, W>>();
            }
            lock (cachedStates)
            {
                state = cachedStates.Count > 0 ? cachedStates.Pop() : new ReGoapState<T, W>();
            }
            state.Init(old);
            return state;
        }
        #endregion

        public override string ToString()
        {
            lock (values)
            {
                var result = "";
                foreach (var pair in values)
                    result += string.Format("'{0}': {1}, ", pair.Key, pair.Value.v);
                return result;
            }
        }

        public W Get(T key)
        {
            lock (values)
            {
                if (!values.ContainsKey(key))
                    return default(W);
                return (W)values[key].v;
            }
        }

        public void Set(T key, W value)
        {
            lock (values)
            {
                values[key] = StructValue.Create(value);
            }
        }

        public StructValue GetStructValue(T key)
        {
            lock(values)
            {
                StructValue st;
                values.TryGetValue(key, out st);
                return st;
            }
        }

        public StructValue ForceGetStructValueInt(T key, int def)
        {
            lock(values)
            {
                StructValue st;
                if( ! values.TryGetValue(key, out st) )
                {
                    st = StructValue.CreateIntArithmetic(def);
                    values.Add(key, st);
                }
                return st;
            }
        }

        public StructValue ForceGetStructValueFloat(T key, float def)
        {
            lock (values)
            {
                StructValue st;
                if (!values.TryGetValue(key, out st))
                {
                    st = StructValue.CreateFloatArithmetic(def);
                    values.Add(key, st);
                }
                return st;
            }
        }

        public StructValue ForceGetStructValueObject(T key, object def)
        {
            lock (values)
            {
                StructValue st;
                if (!values.TryGetValue(key, out st))
                {
                    st = StructValue.Create(def);
                    values.Add(key, st);
                }
                return st;
            }
        }
        public void SetStructValue(T key, StructValue st)
        {
            lock(values)
            {
                values[key] = st;
            }
        }

        public void Remove(T key)
        {
            lock (values)
            {
                values.Remove(key);
            }
        }

        public Dictionary<T, StructValue> GetValues()
        {
            lock (values)
                return values;
        }

        public bool HasKey(T key)
        {
            lock (values)
                return values.ContainsKey(key);
        }

        public void Clear()
        {
            lock (values)
                values.Clear();
        }


        
    }


    /// <summary>
    /// contains the value & optional merge & match ops
    /// </summary>
    public struct StructValue
    {
        public enum EValueType { Arithmetic, Other }

        public EValueType tp;
        public object v;
        public Func<StructValue, StructValue, StructValue> mergeOp; //used by AddFromState
        public Func<StructValue, StructValue, StructValue> diffOp; //used by MissingDifference
        public Func<StructValue, StructValue, bool> isFulfilledByOP; //used for precondition check
        public Func<StructValue, StructValue, StructValue, bool> isBetterOp; // <this, that, target>, this op return true iff 'this' is nearer to 'target' than 'that'

        public bool Inited { get { return mergeOp != null; } }

        public void Invalidate()
        {
            mergeOp = null;
            diffOp = null;
            isFulfilledByOP = null;
            isBetterOp = null;
            v = null;
        }

        public static StructValue Create(object v)
        {
            return Create(v, S_ReplaceMergeOP, S_KeepDiffOP, S_EqualIsFulFilledByOP, S_DefIsBetterOP, EValueType.Other);
        }
        public static StructValue CreateIntArithmetic(int v, bool neg = false)
        {
            if (!neg)
                return Create(v, S_IntAdd_MergeOP, S_IntSub_DiffOp, S_IntLessEqual_IsFulfilledByOP, S_IntBigger_IsBetterOP, EValueType.Arithmetic);
            else
                return Create(v, S_IntAdd_MergeOP, S_IntSub_DiffOp, S_IntGreaterEqual_IsFulfilledByOP, S_IntSmaller_IsBetterOP, EValueType.Arithmetic);
        }
        public static StructValue CreateFloatArithmetic(float v, bool neg = false)
        {
            if (!neg)
                return Create(v, S_FloatAdd_MergeOP, S_FloatSub_DiffOp, S_FloatLessEqual_IsFulfilledByOP, S_FloatBigger_IsBetterOP, EValueType.Arithmetic);
            else
                return Create(v, S_FloatAdd_MergeOP, S_FloatSub_DiffOp, S_FloatGreaterEqual_IsFulfilledByOP, S_FloatSmaller_IsBetterOP, EValueType.Arithmetic);
        }
        public static StructValue Create(object v,
            Func<StructValue, StructValue, StructValue> mergeOp,
            Func<StructValue, StructValue, StructValue> diffOp,
            Func<StructValue, StructValue, bool> isFulFilledByOP,
            Func<StructValue, StructValue, StructValue, bool> isBetterOp,
            EValueType tp
            )
        {
            var o = new StructValue();
            o.v = v;
            o.mergeOp = mergeOp;
            o.diffOp = diffOp;
            o.isFulfilledByOP = isFulFilledByOP;
            o.isBetterOp = isBetterOp;
            o.tp = tp;
            return o;
        }

        public static StructValue CopyCreate(ref StructValue proto, object v)
        {
            return Create(v, proto.mergeOp, proto.diffOp, proto.isFulfilledByOP, proto.isBetterOp, proto.tp);
        }


        public override string ToString()
        {
            return v.ToString();
        }
        //public static implicit operator W(StructValue st)
        //{
        //    return (W)st.v;
        //}

        public StructValue MergeWith(StructValue other)
        {
            var newOne = mergeOp(this, other);
            return newOne;
        }

        public StructValue DiffWith(StructValue other)
        {
            var newOne = diffOp(this, other);
            return newOne;
        }

        public bool IsFulfilledBy(StructValue other)
        {
            return isFulfilledByOP(this, other);
        }

        public bool IsBetter(StructValue other, StructValue target)
        {
            return isBetterOp(this, other, target);
        }

        #region "default OPs"
        public static StructValue ReplaceMergeOP(StructValue self, StructValue other)
        {
            return other;
        }

        public static StructValue KeepDiffOp(StructValue self, StructValue other)
        {
            return self;
        }

        public static bool EqualMatchOP(StructValue self, StructValue other)
        {
            return Equals(self.v, other.v);
        }

        public static bool DefaultIsBetterOP(StructValue self, StructValue other, StructValue target)
        {
            return Equals(self.v, target.v);
        }

        public static readonly Func<StructValue, StructValue, StructValue> S_ReplaceMergeOP = ReplaceMergeOP;
        public static readonly Func<StructValue, StructValue, StructValue> S_KeepDiffOP = KeepDiffOp;
        public static readonly Func<StructValue, StructValue, bool> S_EqualIsFulFilledByOP = EqualMatchOP;
        public static readonly Func<StructValue, StructValue, StructValue, bool> S_DefIsBetterOP = DefaultIsBetterOP;
        #endregion "default OPs"

        #region "Arithmetic OPs"

        public static StructValue IntAdd_MergeOP(StructValue self, StructValue other)
        {
            int v = Convert.ToInt32(self.v) + Convert.ToInt32(other.v);
            StructValue newOne = StructValue.CopyCreate(ref self, v);
            return newOne;
        }
        public static StructValue IntSub_DiffOp(StructValue self, StructValue other)
        {
            int v = Convert.ToInt32(self.v) - Convert.ToInt32(other.v);
            StructValue newOne = StructValue.CopyCreate(ref self, v);
            return newOne;
        }
        public static bool IntLessEqual_IsFulfilledByOP(StructValue goal, StructValue other)
        {
            return Convert.ToInt32(goal.v) <= Convert.ToInt32(other.v);
        }
        public static bool IntGreaterEqual_IsFulfilledByOP(StructValue goal, StructValue other)
        {
            return Convert.ToInt32(goal.v) >= Convert.ToInt32(other.v);
        }
        public static bool IntBigger_IsBetterOP(StructValue effect, StructValue curState, StructValue target)
        {
            int vSelf = Convert.ToInt32(effect.v);
            return vSelf > 0;
        }
        public static bool IntSmaller_IsBetterOP(StructValue effect, StructValue curState, StructValue target)
        {
            int vEff = Convert.ToInt32(effect.v);
            return vEff < 0;
        }

        public static readonly Func<StructValue, StructValue, StructValue> S_IntAdd_MergeOP = IntAdd_MergeOP;
        public static readonly Func<StructValue, StructValue, StructValue> S_IntSub_DiffOp = IntSub_DiffOp;
        public static readonly Func<StructValue, StructValue, bool> S_IntLessEqual_IsFulfilledByOP = IntLessEqual_IsFulfilledByOP;
        public static readonly Func<StructValue, StructValue, bool> S_IntGreaterEqual_IsFulfilledByOP = IntGreaterEqual_IsFulfilledByOP;
        public static readonly Func<StructValue, StructValue, StructValue, bool> S_IntBigger_IsBetterOP = IntBigger_IsBetterOP;
        public static readonly Func<StructValue, StructValue, StructValue, bool> S_IntSmaller_IsBetterOP = IntSmaller_IsBetterOP;

        public static StructValue FloatAdd_MergeOP(StructValue self, StructValue other)
        {
            float v = Convert.ToSingle(self.v) + Convert.ToSingle(other.v);
            StructValue newOne = StructValue.CopyCreate(ref self, v);
            return newOne;
        }

        public static StructValue FloatSub_DiffOp(StructValue self, StructValue other)
        {
            float v = Convert.ToSingle(self.v) - Convert.ToSingle(other.v);
            StructValue newOne = StructValue.CopyCreate(ref self, v);
            return newOne;
        }

        public static bool FloatLessEqual_IsFulfilledByOP(StructValue goal, StructValue other)
        {
            try
            {
                return Convert.ToSingle(goal.v) <= Convert.ToSingle(other.v);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
                return false;
            }
        }

        public static bool FloatGreaterEqual_IsFulfilledByOP(StructValue goal, StructValue other)
        {
            try
            {
                return Convert.ToSingle(goal.v) >= Convert.ToSingle(other.v);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                return false;
            }
        }

        public static bool FloatBigger_IsBetterOP(StructValue effect, StructValue curState, StructValue target)
        {
            float vEffect = Convert.ToSingle(effect.v);
            return vEffect > 0;
        }

        public static bool FloatSmaller_IsBetterOP(StructValue effect, StructValue curState, StructValue target)
        {
            float vEffect = Convert.ToSingle(effect.v);
            return vEffect < 0;
        }

        public static readonly Func<StructValue, StructValue, StructValue> S_FloatAdd_MergeOP = FloatAdd_MergeOP;
        public static readonly Func<StructValue, StructValue, StructValue> S_FloatSub_DiffOp = FloatSub_DiffOp;
        public static readonly Func<StructValue, StructValue, bool> S_FloatLessEqual_IsFulfilledByOP = FloatLessEqual_IsFulfilledByOP;
        public static readonly Func<StructValue, StructValue, bool> S_FloatGreaterEqual_IsFulfilledByOP = FloatGreaterEqual_IsFulfilledByOP;
        public static readonly Func<StructValue, StructValue, StructValue, bool> S_FloatBigger_IsBetterOP = FloatBigger_IsBetterOP;
        public static readonly Func<StructValue, StructValue, StructValue, bool> S_FloatSmaller_IsBetterOP = FloatSmaller_IsBetterOP;

        #endregion "Arithmetic OPs"
    }

}