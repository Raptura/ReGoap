using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Unity.Test;
using NUnit.Framework;
using UnityEngine;

namespace ReGoap.Unity.Editor.Test
{
    public class ReGoapArithOpTests
    {
        [OneTimeSetUp]
        public void Init()
        {
        }

        [OneTimeTearDown]
        public void Dispose()
        {
        }

        IGoapPlanner<string, object> GetPlanner()
        {
            // not using early exit to have precise results, probably wouldn't care in a game for performance reasons
            return new ReGoapPlanner<string, object>(
                new ReGoapPlannerSettings { PlanningEarlyExit = false, UsingDynamicActions = true }
            );
        }

        [Test]
        public void TestReGoapStateAddOperator()
        {
            var state = ReGoapState<string, object>.Instantiate();
            state.Set("var0", true);
            state.SetStructValue("var1", StructValue.CreateIntArithmetic(10));
            state.SetStructValue("var2", StructValue.CreateFloatArithmetic(100f));
            var otherState = ReGoapState<string, object>.Instantiate();
            otherState.SetStructValue("var1", StructValue.CreateIntArithmetic(20)); // 2nd one replaces the first
            otherState.SetStructValue("var2", StructValue.CreateFloatArithmetic(-20f));
            otherState.Set("var3", 10.1f);
            Assert.That(state.Count, Is.EqualTo(3));
            state.AddFromState(otherState);
            Assert.That(otherState.Count, Is.EqualTo(3));
            Assert.That(state.Count, Is.EqualTo(4));
            Assert.That(state.Get("var0"), Is.EqualTo(true));
            Assert.That(state.Get("var1"), Is.EqualTo(30));
            Assert.That(state.Get("var2"), Is.EqualTo(80f));
            Assert.That(state.Get("var3"), Is.EqualTo(10.1f));
        }

        [Test]
        public void TestSimpleChainedPlan()
        {
            TestSimpleChainedPlan(GetPlanner());
        }

        //[Test]
        //public void TestConflictingActionPlan()
        //{
        //    var gameObject = new GameObject();

        //    ReGoapTestsHelper.GetCustomAction(gameObject, "JumpIntoWater",
        //        new Dictionary<string, object> { { "isAtPosition", 0 } },
        //        new Dictionary<string, object> { { "isAtPosition", 1 }, { "isSwimming", true } }, 1);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "GoSwimming",
        //        new Dictionary<string, object> { },
        //        new Dictionary<string, object> { { "isAtPosition", 0 } }, 2);

        //    var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Swim",
        //        new Dictionary<string, object> { { "isSwimming", true } });

        //    var memory = gameObject.AddComponent<ReGoapTestMemory>();
        //    memory.Init();

        //    var agent = gameObject.AddComponent<ReGoapTestAgent>();
        //    agent.Init();

        //    var plan = GetPlanner().Plan(agent, null, null, null);

        //    Assert.That(plan, Is.EqualTo(hasAxeGoal));
        //    // validate plan actions
        //    ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        //}

        public void TestSimpleChainedPlan(IGoapPlanner<string, object> planner)
        {
            var gameObject = new GameObject("SimpleChainedPlan");

            ReGoapTestsHelper.GetCustomAction(gameObject, "BuyFood",
                new Dictionary<string, object> { { "IntGold", 5} },
                new Dictionary<string, object> { { "IntGold", -5}, { "IntFood", 2} },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoMine",
                new Dictionary<string, object> { { "IntFood", 2} },
                new Dictionary<string, object> { { "IntFood", -2}, { "IntGold", 20 } },
                5);
            
            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Mine",
                new Dictionary<string, object> { { "IntGold", 40} });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("IntGold", StructValue.CreateIntArithmetic(20));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        //public void TestTwoPhaseChainedPlan(IGoapPlanner<string, object> planner)
        //{
        //    var gameObject = new GameObject();

        //    ReGoapTestsHelper.GetCustomAction(gameObject, "CCAction",
        //        new Dictionary<string, object> { { "hasWeaponEquipped", true }, { "isNearEnemy", true } },
        //        new Dictionary<string, object> { { "killedEnemy", true } }, 4);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "EquipAxe",
        //        new Dictionary<string, object> { { "hasAxe", true } },
        //        new Dictionary<string, object> { { "hasWeaponEquipped", true } }, 1);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "GoToEnemy",
        //        new Dictionary<string, object> { { "hasTarget", true } },
        //        new Dictionary<string, object> { { "isNearEnemy", true } }, 3);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
        //        new Dictionary<string, object> { { "hasWood", true }, { "hasSteel", true } },
        //        new Dictionary<string, object> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
        //        new Dictionary<string, object> { },
        //        new Dictionary<string, object> { { "hasRawWood", true } }, 2);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
        //        new Dictionary<string, object> { { "hasRawWood", true } },
        //        new Dictionary<string, object> { { "hasWood", true }, { "hasRawWood", false } }, 5);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre", new Dictionary<string, object> { },
        //        new Dictionary<string, object> { { "hasOre", true } }, 10);
        //    ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
        //        new Dictionary<string, object> { { "hasOre", true } },
        //        new Dictionary<string, object> { { "hasSteel", true }, { "hasOre", false } }, 10);

        //    var readyToFightGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "ReadyToFightGoal",
        //        new Dictionary<string, object> { { "hasWeaponEquipped", true } }, 2);
        //    ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
        //        new Dictionary<string, object> { { "hasAxe", true } });
        //    var killEnemyGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "KillEnemyGoal",
        //        new Dictionary<string, object> { { "killedEnemy", true } }, 3);

        //    var memory = gameObject.AddComponent<ReGoapTestMemory>();
        //    memory.Init();

        //    var agent = gameObject.AddComponent<ReGoapTestAgent>();
        //    agent.Init();

        //    // first plan should create axe and equip it, through 'ReadyToFightGoal', since 'hasTarget' is false (memory should handle this)
        //    var plan = planner.Plan(agent, null, null, null);

        //    Assert.That(plan, Is.EqualTo(readyToFightGoal));
        //    // we apply manually the effects, but in reality the actions should do this themselves 
        //    //  and the memory should understand what happened 
        //    //  (e.g. equip weapon action? memory should set 'hasWeaponEquipped' to true if the action equipped something)
        //    // validate plan actions
        //    ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);

        //    // now we tell the memory that we see the enemy
        //    memory.SetValue("hasTarget", true);
        //    // now the planning should choose KillEnemyGoal
        //    plan = planner.Plan(agent, null, null, null);

        //    Assert.That(plan, Is.EqualTo(killEnemyGoal));
        //    ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        //}
    }
}