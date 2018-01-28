using System;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    [RequireComponent(typeof(ResourcesBag))]
    public class CraftRecipeAction : ReGoapAction<string, object>
    {
        public ScriptableObject RawRecipe;
        private IRecipe recipe;
        private ResourcesBag resourcesBag;

        protected override void Awake()
        {
            base.Awake();
            recipe = RawRecipe as IRecipe;
            if (recipe == null)
                throw new UnityException("[CraftRecipeAction] The rawRecipe ScriptableObject must implement IRecipe.");
            resourcesBag = GetComponent<ResourcesBag>();

            // could implement a more flexible system that handles dynamic resources's count
            foreach (var pair in recipe.GetNeededResources())
            {
                preconditions.SetStructValue("hasResource" + pair.Key, StructValue.CreateFloatArithmetic(pair.Value));
                effects.SetStructValue("hasResource" + pair.Key, StructValue.CreateFloatArithmetic(-pair.Value));
            }
            // false preconditions are not supported
            //preconditions.Set("hasResource" + recipe.GetCraftedResource(), false); // do not permit duplicates in the bag
            effects.SetStructValue("hasResource" + recipe.GetCraftedResource(), StructValue.CreateFloatArithmetic(1));
        }

        public override ReGoapState<string, object> GetPreconditions(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
        {
            preconditions.Set("isAtPosition", agent.GetMemory().GetWorldState().Get("nearestWorkstationPosition") as Vector3?);
            return preconditions;
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, IReGoapActionSettings<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            var wstate = agent.GetMemory().GetWorldState();
            var workstation = wstate.Get("nearestWorkstation") as Workstation;
            if (workstation != null && workstation.CraftResource(resourcesBag, recipe))
            {
                var rName = recipe.GetCraftedResource();
                ReGoapLogger.Log("[CraftRecipeAction] crafted recipe " + rName);
                var newV = wstate.ForceGetStructValueFloat("hasResource" + rName, 0).MergeWith(StructValue.CreateFloatArithmetic(1f)); //acquire one resource
                wstate.SetStructValue("hasResource" + rName, newV);

                foreach (var pair in recipe.GetNeededResources())
                { //remove raw materials
                    newV = wstate.ForceGetStructValueFloat("hasResource" + pair.Key, 0).MergeWith(StructValue.CreateFloatArithmetic(-pair.Value));
                    wstate.SetStructValue("hasResource" + pair.Key, newV);
                }

                done(this);
            }
            else
            {
                fail(this);
            }
        }

        public override string ToString()
        {
            return string.Format("GoapAction('{0}', '{1}')", Name, recipe.GetCraftedResource());
        }
    }
}
