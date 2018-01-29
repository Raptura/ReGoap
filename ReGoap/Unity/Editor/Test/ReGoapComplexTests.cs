using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Unity.Test;
using NUnit.Framework;
using UnityEngine;

using ReGoap.Utilities;

namespace ReGoap.Unity.Editor.Test
{
    public class ReGoapComplexTests
    {
        ReGoapLogger.DebugLevel _level;

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
                new ReGoapPlannerSettings { PlanningEarlyExit = false, UsingDynamicActions = true, MaxIterations = 10000, MaxNodesToExpand = 10000 }
            );
        }

        [Test]
        public void TestGatherGotoGather()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "Gather",
                new Dictionary<string, object> { { "At", "Farm" } },
                new Dictionary<string, object> { { "IntFood", 1 } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "Goto",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Farm" } }, 2);

            var theGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GGG",
                new Dictionary<string, object> { { "IntFood", 2 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.debugPlan = true;

            var planner = GetPlanner();
            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(theGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestCollectResource1()
        {
            var planner = GetPlanner();

            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherTree",
                new Dictionary<string, object> { { "At", "Tree" } },
                new Dictionary<string, object> { { "IntTree", 1 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherOre",
                new Dictionary<string, object> { { "At", "Ore" } } ,
                new Dictionary<string, object> { { "IntOre", 1 } },
                5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToTree",
                new Dictionary<string, object> {  },
                new Dictionary<string, object> { { "At", "Tree" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToOre",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Ore" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToBench",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Bench" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MakeTool",
                new Dictionary<string, object> { { "At", "Bench"}, { "IntWood", 1}, {"IntOre", 2 } },
                new Dictionary<string, object> { { "IntTool", 1}, { "IntWood", -1}, { "IntOre", -2} },
                6);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MakeWood",
                new Dictionary<string, object> { { "At", "Bench" }, { "IntTree", 2 } },
                new Dictionary<string, object> { { "IntWood", 1 }, { "IntTree", -2 } },
                6);


            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "MakeTool",
                new Dictionary<string, object> { { "IntTool", 1 } } );

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.shouldDebugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestCollectResource2()
        {
            var planner = GetPlanner();

            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherTree",
                new Dictionary<string, object> { { "At", "Tree" } },
                new Dictionary<string, object> { { "IntTree", 1 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherOre",
                new Dictionary<string, object> { { "At", "Ore" } },
                new Dictionary<string, object> { { "IntOre", 1 } },
                5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToTree",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Tree" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToOre",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Ore" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToBench",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Bench" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MakeTool",
                new Dictionary<string, object> { { "At", "Bench" }, { "IntWood", 2 }, { "IntOre", 2 } },
                new Dictionary<string, object> { { "IntTool", 1 }, { "IntWood", -2 }, { "IntOre", -2 } },
                6);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MakeWood",
                new Dictionary<string, object> { { "At", "Bench" }, { "IntTree", 2 } },
                new Dictionary<string, object> { { "IntWood", 1 }, { "IntTree", -2 } },
                6);


            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "MakeTool",
                new Dictionary<string, object> { { "IntTool", 1 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.shouldDebugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }

        [Test]
        public void TestCollectResource3()
        {
            var planner = GetPlanner();

            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherTree",
                new Dictionary<string, object> { { "At", "Tree" } },
                new Dictionary<string, object> { { "IntTree", 1 } },
                3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherOre",
                new Dictionary<string, object> { { "At", "Ore" } },
                new Dictionary<string, object> { { "IntOre", 1 } },
                5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToTree",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Tree" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToOre",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Ore" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToBench",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Bench" } },
                7);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MakeTool",
                new Dictionary<string, object> { { "At", "Bench" }, { "IntWood", 2 }, { "IntOre", 2 } },
                new Dictionary<string, object> { { "IntTool", 1 }, { "IntWood", -2 }, { "IntOre", -2 } },
                6);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MakeWood",
                new Dictionary<string, object> { { "At", "Bench" }, { "IntTree", 2 } },
                new Dictionary<string, object> { { "IntWood", 2 }, { "IntTree", -2 } },
                6);


            var miningGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "MakeTool",
                new Dictionary<string, object> { { "IntTool", 2 } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();
            agent.shouldDebugPlan = true;

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(miningGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
        }
    }
}
