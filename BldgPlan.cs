using ConfigParser;
using Satisfactory.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static FuzzyCompare;
using static Utils;

using u8 = System.Byte;

public class BldgPlan {
	#region STATIC
	public static List<BldgPlan> List;
	public static Dictionary<string, List<BldgPlan>> IndexByRecipe;

	static BldgPlan() {
		List = new List<BldgPlan>();
		IndexByRecipe = new Dictionary<string, List<BldgPlan>>();
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

	public static Building[] MakeBuildingsForNofIndex(Recipe rcp, double demandRate, int index, double exGross, int exCount, out double newOCRate, double maxOCRate = 1.0, double rcpMarginFactor = 0) {
		BldgPlan plan = GetPlanFor(rcp);

		double rcpMargin = rcp.production[index].rate * rcpMarginFactor + FUZZY_MARGIN;

		double newGross = demandRate + exGross;
		double rcpRate = rcp.production[index].rate * plan.RateMultiplier;
		double currMaxGross = rcpRate * exCount;

		int count;

		if (AlmostLte(newGross, currMaxGross * maxOCRate, rcpMargin)) {
			newOCRate = newGross / currMaxGross;
			count = 0;
		}
		else if (exCount > 0) {
			demandRate = newGross - currMaxGross * maxOCRate;
			count = (int)FuzzyCeiling(demandRate / (rcpRate * maxOCRate), rcpMarginFactor);
			newOCRate = newGross / (rcpRate * (exCount + count));
		}
		else {
			count = (int)FuzzyCeiling(demandRate / (rcpRate * maxOCRate), rcpMarginFactor);
			newOCRate = demandRate / (rcpRate * count);
		}

		Building[] bldgs = new Building[count];

		for (int idx = 0; idx < count; idx++) {
			Building bldg = plan.Build(rcp, newOCRate);

			bldgs[idx] = bldg;
		}

		return bldgs;
	}


	public static Building[] MakeBuildingsForNofIndex(Recipe rcp, double rate, int index, out double newOCRate, double maxOCRate = 1.0d, double rcpMarginFactor = FUZZY_MARGIN) {
		return MakeBuildingsForNofIndex(rcp, rate, index, 0d, 0, out newOCRate, maxOCRate: maxOCRate, rcpMarginFactor: rcpMarginFactor);
	}

	public static Building[] MakeBuildingsForPartTarget(Recipe rcp, Part target, double exGross, int exCount, out double newOCRate, double maxOCRate = 1.0d, double rcpMarginFactor = FUZZY_MARGIN) {
		u8 idx = rcp.GetIndexOf(target);

		return MakeBuildingsForNofIndex(rcp, target.rate, idx, exGross, exCount, out newOCRate, maxOCRate: maxOCRate, rcpMarginFactor: rcpMarginFactor);
	}

	public static Building[] MakeBuildingsForPartTarget(Recipe rcp, Part target, double maxOCRate = 1.0d) {
		double _;

		return MakeBuildingsForPartTarget(rcp, target, 0, 0, out _, maxOCRate: maxOCRate);
	}


	public static ValueTuple<List<Building>, Production> ProcessBuildings(List<Building> bldgs, bool ignoreCosts = false, bool ignorePower = false, bool allowDemandPreference = true, double maxOCRate = 1.0d, double rcpMarginFactor = 0d) {
		rcpMarginFactor += FUZZY_MARGIN;
		u8 iters = 0;

		Production prod = new Production();
		Production costs;

		List<Building> newBldgs = new List<Building>();
		List<Part> missingCosts = null;
		List<Part> preferDemands = new List<Part>();

		Dictionary<string, bool> partsToIngore = new Dictionary<string, bool>();

		prod.AddBuildings(bldgs);

		var minPartList = Production.CalcMinProductionFor(prod);
		Dictionary<Recipe, Part> priorityRcps = new Dictionary<Recipe, Part>();

		foreach (Part priPart in minPartList.Gross.Values) {
			// Look for parts that are produced as a secondary output for any primary recipes.
			Recipe primary;
			if (AlmostGte(priPart.rate, 0, priPart.rate * rcpMarginFactor) && Recipe.TryFindRecipeFor(priPart.name, out primary)) {
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
            newBldgs.AddRange(MakeBuildingsForPartTarget(rcp, part, maxOCRate: maxOCRate));
        }

        prod.AddBuildings(newBldgs);
		bldgs.AddRange(newBldgs);
		newBldgs.Clear();

		double margin = FUZZY_MARGIN;

		while ((!ignorePower && AlmostLt(prod.NetPower, 0d)) || prod.HasNegativeNet(margin) || (!ignoreCosts && missingCosts != null && missingCosts.Count > 0)) {
			margin = FUZZY_MARGIN;
			if (++iters > 254)
				throw new Exception("This is running away.");

			newBldgs.Clear();

			if (!ignoreCosts) {
				costs = Building.SummarizeCosts(bldgs);
				missingCosts = Production.FindMissingCosts(costs, prod, partsToIngore);

				if (missingCosts != null) {
					foreach (Part cost in missingCosts) {
						Recipe rcp;
						bool recipeExists = Recipe.TryFindRecipeFor(cost.name, out rcp);

						if (recipeExists) {
							newBldgs.Add(MakeBuildingFor(rcp));
						}
						else {
							partsToIngore[cost.name] = true;
						}
					}

					bldgs.AddRange(newBldgs);
					prod.AddBuildings(newBldgs);

					newBldgs.Clear();
				}
			}

			preferDemands.Clear();

			foreach (Part part in prod.Net.Values) {
				Recipe rcp;
				if (!Recipe.TryFindRecipeFor(part.name, out rcp, preferDemands, rcpMarginFactor)) {
					throw new Exception("Critical error: Could not find recipe for part named {0} while balancing production.".Format(part.name));
                }

				double rcpRate = rcp.GetRateOfPart(part);
				double rcpMargin = rcpRate * rcpMarginFactor + FUZZY_MARGIN;

				if (AlmostGte(part.rate, 0, rcpMargin)) {
					if (allowDemandPreference)
						preferDemands.Add(part);
					continue;
				}

				margin = max(margin, rcpMargin);


				double currOCRate = 1d;
				var partBldgs = bldgs.Where(b => !(b.Assignment is null)).Where(b => b.Assignment.name == part.name);
				int exCount = partBldgs.Count();

				if (prod.Gross.ContainsKey(part.name)) {
					double newOCRate;

					// TODO: Make this target the part probably.
					// HACK: We are going back through the whole list of buildings to find the gross rate we're actually targeting.  Production probably needs a redesign.
					newBldgs.AddRange(MakeBuildingsForNofIndex(rcp, -part.rate, 0, Building.GetGrossRateOfPartByRecipe(bldgs, rcp.name, part.name), exCount, out newOCRate, maxOCRate: maxOCRate, rcpMarginFactor: rcpMarginFactor));

					if (exCount > 0) {
						currOCRate = partBldgs.First().OCRate;

						if (newOCRate != currOCRate) {
							foreach (Building partBldg in partBldgs) {
								partBldg.OCRate = newOCRate;
							}
						}
					}
				}
				else {
					newBldgs.AddRange(MakeBuildingsForNofIndex(rcp, -part.rate, 0, out currOCRate, rcpMarginFactor: rcpMarginFactor));
				}
			}

			// HACK: We need a better way to recalculate production when OC rates change.
			bldgs.AddRange(newBldgs);
			prod.Clear();
			prod.AddBuildings(bldgs);
			newBldgs.Clear();

			if (!ignorePower && AlmostLt(prod.NetPower, 0)) {
				Generator[] newGens = GenrPlan.MakeGenrsForPower(-prod.NetPower, "Fuel");

				newBldgs.AddRange(newGens);

				bldgs.AddRange(newBldgs);
				prod.AddBuildings(newBldgs);
			}
		}

		return (bldgs, prod);
	}

	protected static void rebuildIndexByRecipe() {
		IndexByRecipe.Clear();

		foreach (BldgPlan plan in BldgPlan.List) {
			if (plan.BuildList is null)
				continue;

			foreach (Recipe rcp in plan.BuildList) {
				if (!IndexByRecipe.ContainsKey(rcp.name)) {
					IndexByRecipe[rcp.name] = new List<BldgPlan>();
				}

				IndexByRecipe[rcp.name].Add(plan);
			}
		}
	}

	public static NodeDefinition GetNodeDefinition(BldgPlan plan) {
		NodeDefinition node = new NodeDefinition(PlanSpec.PLAN_NODE);

		node.AddValue(PlanSpec.NAME_KEY, plan.Name);
		node.AddValue(PlanSpec.BASE_POWER_KEY, plan.BasePower.ToString("G7"));
		node.AddValue(PlanSpec.RATE_MULT_KEY, plan.RateMultiplier.ToString("G7"));

		if (plan.BuildList != null) {
			foreach (Recipe rcp in plan.BuildList) {
				node.AddValue(PlanSpec.BUILD_LIST_KEY, rcp.name);
			}
		}

		NodeDefinition costsNode = new NodeDefinition(PlanSpec.COSTS_NODE);
		foreach (Part part in plan.Costs) {
			costsNode.AddNode(Part.GetNodeDefinition(part));
        }
		node.AddNode(costsNode);

		return node;
    }

	public static BldgPlan GetPlanFromNode(NodeDefinition node) {
		string name = node.GetValue(PlanSpec.NAME_KEY);
		double basePower = double.Parse(node.GetValue(PlanSpec.BASE_POWER_KEY));
		double rateMultiplier = double.Parse(node.GetValue(PlanSpec.RATE_MULT_KEY));

		var buildListS = node.GetValues(PlanSpec.BUILD_LIST_KEY);

		List<Recipe> buildList = new List<Recipe>();

		if (buildListS != null) {
			foreach (string rcpName in buildListS) {
				Recipe rcp;

				if (Recipe.TryGetRecipeByName(rcpName, out rcp)) {
					buildList.Add(rcp);
				} else {
					error("Skipping recipe named {0} when loading {1}: recipe does not exist.".Format(rcpName, name));
                }
			}
		}

		NodeDefinition costsNode = node.GetFirstNodeByName(PlanSpec.COSTS_NODE);
		var costNodes = costsNode.GetNodesByName(PartSpec.PART_NODE);

		List<Part> costs = new List<Part>();

		foreach (NodeDefinition costNode in costNodes) {
			Part cost = Part.GetPartFromNode(costNode);
			costs.Add(cost);
        }

		return new BldgPlan(name, basePower, costs, buildList, rateMultiplier);
    }

	public static void LoadAllFromPath(string path) {
		var nodes = ConfigFile.LoadFromPath(path);

		foreach (NodeDefinition node in nodes) {
			BldgPlan plan;
			switch (node.NodeName) {
				case PlanSpec.PLAN_NODE:
					plan = BldgPlan.GetPlanFromNode(node);
					break;
				case PlanSpec.GENR_NODE:
					plan = GenrPlan.GetPlanFromNode(node) as BldgPlan;
					break;
				default:
					throw new NotImplementedException("Only building and generator plans are supported at this time.");
			}
		}
	}
	#endregion

	public string Name { get; protected set; }
	public double BasePower { get; protected set; }
	public double RateMultiplier { get; protected set; }


	protected List<Recipe> buildList;
	public IList<Recipe> BuildList {
		get {
			return buildList.AsReadOnly();
		}
		set {
			buildList = value as List<Recipe>;
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
		this.BuildList = buildList;
		this.RateMultiplier = rateMultiplier;

		BldgPlan.List.Add(this);
	}
}
