﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static FuzzyCompare;
using static RecipeDefs;
using u8 = System.Byte;

public class BldgPlan {
	#region STATIC
	public static List<BldgPlan> List;
	public static Dictionary<string, List<BldgPlan>> IndexByRecipe;

	static BldgPlan() {
		List = new List<BldgPlan>();
		IndexByRecipe = new Dictionary<string, List<BldgPlan>>();

		RuntimeHelpers.RunClassConstructor(typeof(BuildingDefs).TypeHandle);
	}

	public static BldgPlan New(string name, double basePower, ITuple costs, ITuple buildList = null, double rateMultiplier = 1d) {
		List<Part> cList = new List<Part>();
		List<Recipe> bList = new List<Recipe>();

		for (u8 idx = 0; idx < costs.Length; idx++) {
			cList.Add((Part)costs[idx]);
		}

		if (buildList != null) {
			for (u8 idx = 0; idx < buildList.Length; idx++) {
				bList.Add((Recipe)buildList[idx]);
			}
		}

		return new BldgPlan(name, basePower, cList, bList, rateMultiplier);
	}

	public static BldgPlan GetPlanFor(Recipe rcp) {
		BldgPlan plan = null;

		double bestMult = 0d;

		foreach (BldgPlan pot in IndexByRecipe[rcp.name]) {
			if (pot.RateMultiplier > bestMult) {
				bestMult = pot.RateMultiplier;
				plan = pot;
			}
		}
		if (plan is null) {
			throw new Exception("We don't have a building for that.");
		}

		return plan;
	}

	public static Building MakeBuildingFor(Recipe rcp) {
		return GetPlanFor(rcp).Build(rcp);
	}

	public static Building MakeBuildingForNofIndex(Recipe rcp, double rate, int index) {
		Building bldg = GetPlanFor(rcp).Build();

		bldg.SetOCRateForNofIndex(rate, index);

		return bldg;
	}

	public static Building MakeBuildingByRecipeForNofPart(Recipe rcp, Part part) {
		Building bldg = GetPlanFor(rcp).Build(rcp);

		bldg.SetOCRateForPartTarget(part);

		return bldg;
	}

	public static Building[] MakeBuildingsForNofIndex(Recipe rcp, double demandRate, int index, double exGross, int exCount, out double newOCRate) {
		BldgPlan plan = GetPlanFor(rcp);

		double newGross = demandRate + exGross;
		double rcpRate = rcp.production[index].rate * plan.RateMultiplier;
		double currMaxGross = rcpRate * exCount;

		int count;

		if (AlmostLte(newGross, currMaxGross)) {
			newOCRate = newGross / currMaxGross;
			count = 0;
		}
		else {
			demandRate = newGross - currMaxGross;
			count = (int)Math.Ceiling(demandRate / rcpRate);
			newOCRate = newGross / (rcpRate * (exCount + count));
		}

		Building[] bldgs = new Building[count];

		for (int idx = 0; idx < count; idx++) {
			Building bldg = plan.Build(rcp, newOCRate);

			bldgs[idx] = bldg;
		}

		return bldgs;
	}


	public static Building[] MakeBuildingsForNofIndex(Recipe rcp, double rate, int index, out double newOCRate) {
		return MakeBuildingsForNofIndex(rcp, rate, index, 0d, 0, out newOCRate);
	}

	public static Building[] MakeBuildingsForPartTarget(Recipe rcp, Part target, double exGross, int exCount, out double newOCRate) {
		u8 idx = rcp.GetIndexOf(target);

		return MakeBuildingsForNofIndex(rcp, target.rate, idx, exGross, exCount, out newOCRate);
	}

	public static Building[] MakeBuildingsForPartTarget(Recipe rcp, Part target) {
		double _;

		return MakeBuildingsForPartTarget(rcp, target, 0, 0, out _);
	}


	public static ValueTuple<List<Building>, Production> ProcessBuildings(List<Building> bldgs, bool ignoreCosts = false) {
		u8 iters = 0;

		Production prod = new Production();
		Production costs = new Production();

		List<Building> newBldgs = new List<Building>();
		List<Part> missingCosts = null;

		Dictionary<string, bool> partsToIngore = new Dictionary<string, bool>();

		prod.AddBuildings(bldgs);

		var minPartList = Production.CalcMinProductionFor(prod);
		Dictionary<Recipe, Part> priorityRcps = new Dictionary<Recipe, Part>();

		foreach (Part priPart in minPartList.Gross.Values) {
			// Look for parts that are produced as a secondary output for any primary recipes.
			Recipe primary;

			if (Recipe.FindRecipeFor(priPart.name, out primary)) {
				// We found a primary recipe for this part -- time to subloop.
				foreach (Part secPart in minPartList.Gross.Values) {
					if (secPart.name == priPart.name || secPart.name == primary.name)
						continue;

					if (primary.Provides(secPart)) {
						// If the recipe provides a secondary part, set it as a priority, but only up to the amount of primary part we need.
						priorityRcps[primary] = priPart;
					}
				}
			}
		}

		foreach ((Recipe rcp, Part part) in priorityRcps) {
			// If we found any priority recipes, make buildings for them first.
			bldgs.AddRange(MakeBuildingsForPartTarget(rcp, part));
		}

		prod.Clear();
		prod.AddBuildings(bldgs);

		while (AlmostLt(prod.NetPower, 0d) || prod.HasNegativeNet() || (!ignoreCosts && missingCosts != null && missingCosts.Count > 0)) {
			if (++iters > 10)
				throw new Exception("This is running away.");

			newBldgs.Clear();

			if (!ignoreCosts && missingCosts != null) {
				foreach (Part cost in missingCosts) {
					Recipe rcp;
					bool recipeExists = Recipe.FindRecipeFor(cost.name, out rcp);

					if (recipeExists) {
						newBldgs.Add(MakeBuildingFor(rcp));
					}
					else {
						partsToIngore[cost.name] = true;
					}
				}

				bldgs.AddRange(newBldgs);
				prod.Clear();
				prod.AddBuildings(bldgs);
				newBldgs.Clear();
			}

			foreach (Part part in prod.Net.Values) {
				if (AlmostGte(part.rate, 0))
					continue;

				Recipe rcp;
				// TODO: Something with this.
				bool _ = Recipe.FindRecipeFor(part.name, out rcp);

				double currOCRate = 1d;
				var partBldgs = bldgs.Where(b => !(b.Assignment is null)).Where(b => b.Assignment.name == part.name);
				int exCount = partBldgs.Count();

				if (prod.Gross.ContainsKey(part.name) && exCount > 0) {

					currOCRate = partBldgs.First().OCRate;
					double newOCRate;

					newBldgs.AddRange(MakeBuildingsForNofIndex(rcp, -part.rate, 0, prod.Gross[part.name].rate, exCount, out newOCRate));

					if (newOCRate != currOCRate) {
						foreach (Building partBldg in partBldgs) {
							partBldg.OCRate = newOCRate;
						}
					}
				}
				else {
					newBldgs.AddRange(MakeBuildingsForNofIndex(rcp, -part.rate, 0, out currOCRate));
				}
			}

			bldgs.AddRange(newBldgs);
			prod.Clear();
			prod.AddBuildings(bldgs);

			if (AlmostLt(prod.NetPower, 0)) {
				Generator[] newGens = GenrPlan.MakeGenrsForPower(-prod.NetPower, fuel.name);
				bldgs.AddRange(newGens);
			}

			prod.Clear();
			prod.AddBuildings(bldgs);

			costs = Building.SummarizeCosts(bldgs);
			missingCosts = Production.FindMissingCosts(costs, prod, partsToIngore);
		}

		return (bldgs, prod);
	}

	protected static void rebuildIndexByRecipe() {
		IndexByRecipe.Clear();

		foreach (BldgPlan plan in BldgPlan.List) {
			if (plan.buildList is null)
				continue;

			foreach (Recipe rcp in plan.buildList) {
				if (!IndexByRecipe.ContainsKey(rcp.name)) {
					IndexByRecipe[rcp.name] = new List<BldgPlan>();
				}

				IndexByRecipe[rcp.name].Add(plan);
			}
		}
	}
	#endregion

	public string Name { get; protected set; }
	public double BasePower { get; protected set; }
	public double RateMultiplier { get; protected set; }


	protected List<Recipe> buildList;
	public List<Recipe> BuildList {
		get {
			return buildList;
		}
		set {
			buildList = value;
			rebuildIndexByRecipe();
		}
	}

	public List<Part> Costs { get; protected set; }

	public virtual Building Build(Recipe assignment = null, double ocrate = 1d) {
		return new Building(this.Name, this, assignment, ocrate);
	}

	public override System.String ToString() {
		return string.Format("Plan for {0} ({1} MW, costs: {2})\nBuilds:\n\t{3}", this.Name, this.BasePower, string.Join(", ", this.Costs), string.Join(",\n\t", this.BuildList));
	}

	protected BldgPlan(string name, double basePower, List<Part> costs, List<Recipe> buildList, double rateMultiplier) {
		this.Name = name;
		this.BasePower = basePower;
		this.Costs = costs;
		this.BuildList = BuildList;
		this.RateMultiplier = rateMultiplier;

		BldgPlan.List.Add(this);
	}
}
