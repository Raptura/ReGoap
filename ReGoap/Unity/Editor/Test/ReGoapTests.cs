using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Unity.Test;
using NUnit.Framework;
using UnityEngine;

using ReGoap.Utilities;

namespace ReGoap.Unity.Editor.Test
{
    public class ReGoapTests
    {
        private ReGoapLogger.DebugLevel _level;

        [OneTimeSetUp]
        public void Init()
        {
            _level = ReGoapLogger.Level;
            ReGoapLogger.Level = ReGoapLogger.DebugLevel.Full;
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            ReGoapLogger.Level = _level;
        }

        IGoapPlanner<string, object> GetPlanner()
        {
            // not using early exit to have precise results, probably wouldn't care in a game for performance reasons
            return new ReGoapPlanner<string, object>(
                new ReGoapPlannerSettings { PlanningEarlyExit = false }
            );
        }

        [Test]
        public void TestGatherGotoGather()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherApple",
                new Dictionary<string, object> { { "At", "Farm"} },
                new Dictionary<string, object> { { "hasApple", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherPeach",
                new Dictionary<string, object> { { "At", "Farm" } },
                new Dictionary<string, object> { { "hasPeach", true } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "Goto",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Farm" } }, 10);

            var theGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GatherAll",
                new Dictionary<string, object> { { "hasApple", true }, { "hasPeach", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.debugPlan = true;

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(theGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestConflictingEffectPlan()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "Mine Ore",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasMoney", true } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "Buy Food",
                new Dictionary<string, object> { { "hasMoney", true } },
                new Dictionary<string, object> { { "hasFood", true }, { "hasMoney", false } }, 2);

            var theGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "PrepareFoodAndMoney",
                new Dictionary<string, object> { { "hasMoney", true }, { "hasFood", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(theGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestReGoapStateMissingDifference()
        {
            var state = ReGoapState<string, object>.Instantiate();
            state.Set("var0", true);
            state.Set("var1", "string");
            state.Set("var2", 1);
            var otherState = ReGoapState<string, object>.Instantiate();
            otherState.Set("var1", "stringDifferent");
            otherState.Set("var2", 1);
            var differences = ReGoapState<string, object>.Instantiate();
            var count = state.MissingDifference(otherState, ref differences);
            Assert.That(count, Is.EqualTo(2));
            Assert.That(differences.Get("var0"), Is.EqualTo(true));
            Assert.That(differences.Get("var1"), Is.EqualTo("string"));
            Assert.That(differences.HasKey("var2"), Is.EqualTo(false));
        }

        [Test]
        public void TestReGoapStateAddOperator()
        {
            var state = ReGoapState<string, object>.Instantiate();
            state.Set("var0", true);
            state.Set("var1", "string");
            state.Set("var2", 1);
            var otherState = ReGoapState<string, object>.Instantiate();
            otherState.Set("var2", "new2"); // 2nd one replaces the first
            otherState.Set("var3", true);
            otherState.Set("var4", 10.1f);
            Assert.That(state.Count, Is.EqualTo(3));
            state.AddFromState(otherState);
            Assert.That(otherState.Count, Is.EqualTo(3));
            Assert.That(state.Count, Is.EqualTo(5)); // var2 on first is replaced by var2 on second
            Assert.That(state.Get("var0"), Is.EqualTo(true));
            Assert.That(state.Get("var1"), Is.EqualTo("string"));
            Assert.That(state.Get("var2"), Is.EqualTo("new2"));
            Assert.That(state.Get("var3"), Is.EqualTo(true));
            Assert.That(state.Get("var4"), Is.EqualTo(10.1f));
        }

        [Test]
        public void TestConflictingPlan()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "JumpIntoWater",
                new Dictionary<string, object> { { "isAtPosition", 0 } },
                new Dictionary<string, object> { { "isAtPosition", 1 }, { "isSwimming", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoSwimming",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "isAtPosition", 0 } }, 2);

            var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Swim",
                new Dictionary<string, object> { { "isSwimming", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(hasAxeGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestConflictingPrecondPlan()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "PutUpAxe",
                new Dictionary<string, object> { { "isAtPosition", "Box" }, { "hasAxe", true } },
                new Dictionary<string, object> { { "PutUpAxe", true }, { "hasAxe", false } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToBox",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "isAtPosition", "Box" } }, 4);
            ReGoapTestsHelper.GetCustomAction(gameObject, "CraftAxe",
                new Dictionary<string, object> { { "isAtPosition", "Workbench" } },
                new Dictionary<string, object> { { "hasAxe", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GotoWorkbench",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "isAtPosition", "Workbench"} }, 4);

            var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Crafter",
                new Dictionary<string, object> { { "PutUpAxe", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(hasAxeGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestSimpleChainedPlan()
        {
            var planner = GetPlanner();
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
                new Dictionary<string, object> { { "hasWood", true }, { "hasSteel", true } },
                new Dictionary<string, object> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasRawWood", true } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
                new Dictionary<string, object> { { "hasRawWood", true } },
                new Dictionary<string, object> { { "hasWood", true }, { "hasRawWood", false } }, 5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasOre", true } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
                new Dictionary<string, object> { { "hasOre", true } },
                new Dictionary<string, object> { { "hasSteel", true }, { "hasOre", false } }, 10);

            var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
                new Dictionary<string, object> { { "hasAxe", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(hasAxeGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestTwoPhaseChainedPlan()
        {
            var planner = GetPlanner();
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "CCAction",
                new Dictionary<string, object> { { "hasWeaponEquipped", true }, { "isNearEnemy", true } },
                new Dictionary<string, object> { { "killedEnemy", true } }, 4);
            ReGoapTestsHelper.GetCustomAction(gameObject, "EquipAxe",
                new Dictionary<string, object> { { "hasAxe", true } },
                new Dictionary<string, object> { { "hasWeaponEquipped", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToEnemy",
                new Dictionary<string, object> { { "hasTarget", true } },
                new Dictionary<string, object> { { "isNearEnemy", true } }, 3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
                new Dictionary<string, object> { { "hasWood", true }, { "hasSteel", true } },
                new Dictionary<string, object> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasRawWood", true } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
                new Dictionary<string, object> { { "hasRawWood", true } },
                new Dictionary<string, object> { { "hasWood", true }, { "hasRawWood", false } }, 5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre", 
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasOre", true } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
                new Dictionary<string, object> { { "hasOre", true } },
                new Dictionary<string, object> { { "hasSteel", true }, { "hasOre", false } }, 10);

            var readyToFightGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "ReadyToFightGoal",
                new Dictionary<string, object> { { "hasWeaponEquipped", true } }, 2);
            ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
                new Dictionary<string, object> { { "hasAxe", true } });
            var killEnemyGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "KillEnemyGoal",
                new Dictionary<string, object> { { "killedEnemy", true } }, 3);

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.debugPlan = true;

            // first plan should create axe and equip it, through 'ReadyToFightGoal', since 'hasTarget' is false (memory should handle this)
            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(readyToFightGoal));
            // we apply manually the effects, but in reality the actions should do this themselves 
            //  and the memory should understand what happened 
            //  (e.g. equip weapon action? memory should set 'hasWeaponEquipped' to true if the action equipped something)
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory); 

            // now we tell the memory that we see the enemy
            memory.SetValue("hasTarget", true);
            // now the planning should choose KillEnemyGoal
            plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(killEnemyGoal));
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }
    }
}