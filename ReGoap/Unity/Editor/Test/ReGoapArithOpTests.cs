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
        public void TestPlan1()
        {
            TestPlan1(GetPlanner());
        }

        [Test]
        public void TestPlan2()
        {
            TestPlan2(GetPlanner());
        }

        [Test]
        public void TestNegativePlan1()
        {
            var planner = GetPlanner();
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "CollectRes",
                new Dictionary<string, object> {  },
                new Dictionary<string, object> { { "NFloatRisk", 10f }, { "IntGold", 10 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ReduceRisk",
                new Dictionary<string, object> { { "IntGold", -10 } },
                new Dictionary<string, object> { { "NFloatRisk", -20f }, { "IntGold", -10 } },
                5);

            var goal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GetGold",
                new Dictionary<string, object> { { "NFloatRisk", 10f } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("NFloatRisk", StructValue.CreateFloatArithmetic(50f));
            memory.SetStructValue("IntGold", StructValue.CreateFloatArithmetic(10));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.debugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(goal));
            Assert.That(plan.GetPlan().Count, Is.EqualTo(5));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestNegativePlan2()
        {
            var planner = GetPlanner();
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "CollectRes",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "NFloatRisk", 10f }, { "IntGold", 10 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ReduceRisk",
                new Dictionary<string, object> { { "IntGold", -10 } },
                new Dictionary<string, object> { { "NFloatRisk", -20f }, { "IntGold", -10 }},
                5);

            var goal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GetGold",
                new Dictionary<string, object> { { "NFloatRisk", 10f } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("NFloatRisk", StructValue.CreateFloatArithmetic(50f));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.debugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(goal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestImpossiblePlanNonDynamicActions()
        {
            var planner = new ReGoapPlanner<string, object>(
                new ReGoapPlannerSettings { PlanningEarlyExit = false, UsingDynamicActions = false, MaxIterations=100, MaxNodesToExpand=20 }
            );

            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "BuyFood",
                new Dictionary<string, object> { { "IntGold", 10 } },
                new Dictionary<string, object> { { "IntGold", -10 }, { "IntFood", 1 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoMine",
                new Dictionary<string, object> { {"IntFood", 1 } },
                new Dictionary<string, object> { { "IntGold", 5 }, { "IntFood", -1} },
                5);

            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GetGold",
                new Dictionary<string, object> { { "IntGold", 30 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("IntGold", StructValue.CreateIntArithmetic(10));
            memory.SetStructValue("IntFood", StructValue.CreateIntArithmetic(3));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.Null);
        }

        [Test]
        public void TestImpossiblePlanDynamicActions()
        {
            var planner = new ReGoapPlanner<string, object>(
                new ReGoapPlannerSettings { PlanningEarlyExit = false, UsingDynamicActions = true, MaxIterations = 100, MaxNodesToExpand = 20 }
            );

            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "BuyFood",
                new Dictionary<string, object> { { "IntGold", 10 } },
                new Dictionary<string, object> { { "IntGold", -10 }, { "IntFood", 1 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoMine",
                new Dictionary<string, object> { { "IntFood", 1 } },
                new Dictionary<string, object> { { "IntGold", 5 }, { "IntFood", -1 } },
                5);

            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GetGold",
                new Dictionary<string, object> { { "IntGold", 30 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("IntGold", StructValue.CreateIntArithmetic(10));
            memory.SetStructValue("IntFood", StructValue.CreateIntArithmetic(3));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.Null);
        }

        public void TestPlan1(IGoapPlanner<string, object> planner)
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "BuyFood",
                new Dictionary<string, object> { { "IntGold", 10 } },
                new Dictionary<string, object> { { "IntGold", -10 }, { "IntFood", 1 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoMine",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "IntGold", 10 } },
                5);

            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Mine",
                new Dictionary<string, object> { { "IntGold", 10 }, { "IntFood", 1 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("IntGold", StructValue.CreateIntArithmetic(10));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.debugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        public void TestPlan2(IGoapPlanner<string, object> planner)
        {
            var gameObject = new GameObject();

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
            agent.debugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestPlan3()
        {
            var planner = GetPlanner();
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "BuyFood",
                new Dictionary<string, object> { { "IntGold", 5 } },
                new Dictionary<string, object> { { "IntGold", -5 }, { "IntFood", 2 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoMine",
                new Dictionary<string, object> { { "IntFood", 2 } },
                new Dictionary<string, object> { { "IntFood", -2 }, { "IntGold", 20 } },
                5);

            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Mine",
                new Dictionary<string, object> { { "IntGold", 40 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetStructValue("IntGold", StructValue.CreateIntArithmetic(20));
            memory.SetStructValue("IntFood", StructValue.CreateIntArithmetic(20));

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

    }
}