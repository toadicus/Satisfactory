using System.Collections.Generic;
using static Utils;
using ConfigParser;
using System;
using Satisfactory.Schema;

namespace Satisfactory {
	struct Conditions {
		public bool ignoreCosts;
		public bool ignorePower;
		public bool allowDemandPreference;
		public double maxOCRate;
		public double rcpMarginFactor;

		public List<Part> partTargets;
		public List<string> ignoreRecipes;
    }

	class Program {
		static List<Conditions> LoadTestSpecsFromPath(string path) {
			List<Conditions> conditions = new List<Conditions>();

			using (var stream = System.IO.File.OpenText(path)) {
				List<NodeDefinition> testNodes = new Parser(new Scanner(stream.ReadToEnd())).Parse();
				foreach (NodeDefinition testNode in testNodes) {
					if (testNode.NodeName != "TEST") {
						throw new NotImplementedException("Test specifications must be given in TEST{} node blocks.");
                    }

					IList<NodeDefinition> nodes = testNode.Nodes;

					List<Part> partTargets = new List<Part>();

					bool ignoreCosts = false;
					bool ignorePower = false;
					bool allowDemandPreference = true;
					double maxOCRate = 1d;
					double rcpMarginFactor = 0.01d;
					List<string> ignoreRecipes = null;

					foreach (NodeDefinition node in nodes) {
						switch (node.NodeName) {
							case TestSpec.PART_TGT_NODE:
								string partName = node.GetValue(TestSpec.NAME_KEY);
								double targetRate = node.GetValue(TestSpec.RATE_KEY, double.NaN);
								Part target = new Part(partName, targetRate);
								partTargets.Add(target);
								break;
							case TestSpec.CONDITIONS_NODE:
								ignoreCosts = node.GetValue(TestSpec.IGN_COSTS_KEY, ignoreCosts);
								ignorePower = node.GetValue(TestSpec.IGN_POWER_KEY, ignorePower);
								allowDemandPreference = node.GetValue(TestSpec.ALLOW_DEM_PREF_KEY, allowDemandPreference);
								maxOCRate = node.GetValue(TestSpec.MAX_OC_KEY, maxOCRate);
								rcpMarginFactor = node.GetValue(TestSpec.MARGIN_FACTOR_KEY, rcpMarginFactor);
								if (node.HasValue(TestSpec.IGNORE_RECIPES_KEY)) {
									ignoreRecipes = node.GetValues(TestSpec.IGNORE_RECIPES_KEY);
                                }
								break;
							default:
								throw new NotImplementedException("Unsupported specification node.");
						}
					}

					conditions.Add(new Conditions() {
						ignoreCosts = ignoreCosts,
						ignorePower = ignorePower,
						allowDemandPreference = allowDemandPreference,
						maxOCRate = maxOCRate,
						rcpMarginFactor = rcpMarginFactor,
						partTargets = partTargets,
						ignoreRecipes = ignoreRecipes ?? new List<string> { }
					});
				}
            }

			return conditions;
        }

		static void Test(List<Building> bldgs, bool ignoreCosts, bool ignorePower, bool allowDemandPreference, double maxOCRate, double rcpMarginFactor) {
			print("\nTesting at max clock rate of {0:P0}\n".Format(maxOCRate));

			Production prod;
			(bldgs, prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts, ignorePower, allowDemandPreference, maxOCRate, rcpMarginFactor);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}

		static void Test(Conditions conditions) {
			foreach (string rcpName in conditions.ignoreRecipes) {
				Recipe.RemoveRecipeByName(rcpName);
			}

			List<Building> bldgs = new List<Building>();

			foreach (Part target in conditions.partTargets) {
				Recipe rcp;

				if (Recipe.TryFindRecipeFor(target.name, out rcp)) {
					double rcpRate;
					int rcpPartIdx;

					for (int idx = 0; idx < rcp.production.Count; idx++) {
						Part part = rcp.production[idx];

						if (part.name == target.name) {
							rcpRate = part.rate;
							rcpPartIdx = idx;

							double rate = double.IsNaN(target.rate) ? rcpRate : target.rate;
							double _;

							Building[] bldg = BldgPlan.MakeBuildingsForNofIndex(rcp, rate, rcpPartIdx, out _, conditions.maxOCRate, conditions.rcpMarginFactor);
							bldgs.AddRange(bldg);
							break;
                        }
                    }
                }
            }

			Test(bldgs, conditions.ignoreCosts, conditions.ignorePower, conditions.allowDemandPreference, conditions.maxOCRate, conditions.rcpMarginFactor);
        }

		static void Main(string[] args) {
			Recipe.LoadAllFromPath("Data/recipe_list.cfg");
			BldgPlan.LoadAllFromPath("Data/bldg_plans.cfg");

			foreach (Conditions conditions in LoadTestSpecsFromPath("TestSpec.cfg")) {
				Test(conditions);
			}
		}
	}
}
