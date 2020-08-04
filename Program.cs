using System.Collections.Generic;
using static Utils;
using ConfigParser;
using System;
using Satisfactory.Schema;
using System.Linq;
using System.Text;

namespace Satisfactory {
	struct Conditions {
		public const bool IGNORE_COSTS_DEFAULT = false;
		public const bool IGNORE_POWER_DEFAULT = false;
		public const bool ALLOW_PREFER_DEFAULT = true;
		public const double MIN_OCRATE_DEFAULT = 0.5d;
		public const double MAX_OCRATE_DEFAULT = 1.0d;
		public const double MARGIN_FACTOR_DEFAULT = 0.01d;

		public bool ignoreCosts;
		public bool ignorePower;
		public bool allowDemandPreference;
		public BldgPlan.OCRateBounds ocBounds;
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

					bool ignoreCosts = Conditions.IGNORE_COSTS_DEFAULT;
					bool ignorePower = Conditions.IGNORE_POWER_DEFAULT;
					bool allowDemandPreference = Conditions.ALLOW_PREFER_DEFAULT;
					double minOCRate = Conditions.MIN_OCRATE_DEFAULT;
					double maxOCRate = Conditions.MAX_OCRATE_DEFAULT;
					double rcpMarginFactor = Conditions.MARGIN_FACTOR_DEFAULT;
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
								minOCRate = node.GetValue(TestSpec.MIN_OC_KEY, minOCRate);
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
						ocBounds = new BldgPlan.OCRateBounds(maxOCRate, minOCRate),
						rcpMarginFactor = rcpMarginFactor,
						partTargets = partTargets,
						ignoreRecipes = ignoreRecipes ?? new List<string> { }
					});
				}
            }

			return conditions;
        }

		static void Test(Production prod, List<Building> bldgs, bool ignoreCosts, bool ignorePower, bool allowDemandPreference, BldgPlan.OCRateBounds ocBounds, double rcpMarginFactor) {
			double maxOCRate = ocBounds.maxOCRate;

			print("\nTesting at max clock rate of {0:P0}\n".Format(maxOCRate));

			(bldgs, prod) = BldgPlan.ProcessBuildings(prod, bldgs, ocBounds, ignoreCosts, ignorePower, allowDemandPreference, rcpMarginFactor);

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
			Production prod = new Production();
			List<Building> bldgs = new List<Building>();

			foreach (Part target in conditions.partTargets) {
				Part part;

                if (double.IsNaN(target.rate)) {
                    Recipe rcp;

                    if (Recipe.TryFindRecipeFor(target.name, out rcp)) {
                        part = target.Copy(rcp.GetProdRateOfPart(target));
                    }
                    else {
                        part = target.Copy(0);
                    }
                }
                else {
					part = target.Copy();
                }

				prod.AddDemand(part);
            }

			Test(prod, bldgs, conditions.ignoreCosts, conditions.ignorePower, conditions.allowDemandPreference, conditions.ocBounds, conditions.rcpMarginFactor);
        }

		class CSVTable {
			public class CSVRow {
				List<object> cells;
				CSVTable host;
				int rowIdx;

				public void SetNumColumns(int num, string defaultString = "") {
					if (cells.Count == num)
						return;

					if (cells.Count < num) {
						while (cells.Count < num) {
							cells.Add(defaultString);
                        }
                    }
					else {
						while (num < cells.Count) {
							cells.RemoveAt(cells.Count - 1);
                        }
                    }
                }

				public void AddValue(object value) {
					this.cells.Add(value);
                }

				public void AddValueByColumnHeader(string header, object value) {
					if (!this.host.columnsByName.ContainsKey(header)) {
						this.host.AddColumn(header);
                    }

					int idx = this.host.columnsByName[header];

					this.cells[idx] = value;
                }

                public override string ToString() {
					return string.Join(',', this.cells);
                }

				public CSVRow(CSVTable host, int rowIdx, params object[] cells) {
					this.host = host;
					this.rowIdx = rowIdx;
					this.cells = new List<object> { cells };
                }
            }

			Dictionary<string, int> columnsByName = new Dictionary<string, int>();
			List<string> columnHeaders = new List<string>();
			List<CSVRow> rows = new List<CSVRow>();

			public void AddColumn(string header) {
				int idx = columnHeaders.Count;

				if (columnsByName.ContainsKey(header)) {
					header = "{0}_{1}".Format(header, columnsByName.Count(kvp => (kvp.Key == header)));
                }

				columnHeaders.Add(header);
				columnsByName[header] = idx;

				foreach (CSVRow row in this.rows) {
					row.SetNumColumns(columnHeaders.Count);
                }
			}

			public CSVRow AddRow(params object[] cells) {
				int idx = this.rows.Count;

				CSVRow row = new CSVRow(this, idx, cells);
				row.SetNumColumns(this.columnHeaders.Count);

				this.rows.Add(row);

				return row;
            }

            public override String ToString() {
				StringBuilder sb = new StringBuilder();

				sb.Append(string.Join(',', this.columnHeaders));
				sb.Append('\n');

				foreach (CSVRow row in this.rows) {
					sb.Append(row.ToString());
					sb.Append('\n');
                }

				return sb.ToString();
            }
        }

		static void BuildCSVs() {
			CSVTable rcpTable = new CSVTable();

			string rcpHeader = "Recipe";

			rcpTable.AddColumn(rcpHeader);

			foreach (Recipe rcp in Recipe.List) {
				CSVTable.CSVRow row = rcpTable.AddRow();
				row.AddValueByColumnHeader(rcpHeader, rcp.name);

				foreach (Part part in rcp.production) {
					row.AddValueByColumnHeader(part.name, part.rate);
				}

				foreach (Part part in rcp.demands) {
					row.AddValueByColumnHeader(part.name, -part.rate);
				}
			}

			print(rcpTable);

			CSVTable bldgTable = new CSVTable();

			string bldgHeader = "Building";

			foreach (BldgPlan plan in BldgPlan.List) {
				CSVTable.CSVRow row = bldgTable.AddRow();

				row.AddValueByColumnHeader(bldgHeader, plan.Name);

				if (plan is GenrPlan) { }
				else {
					foreach (Recipe rcp in plan.BuildList) {
						row.AddValue(rcp.name);
					}
				}
			}

			print(bldgTable);
		}

		static void ParseGameDocs(string path) {
			const string PartClassKey = "Class'/Script/FactoryGame.FGItemDescriptor'";
			const string RecipeClassKey = "Class'/Script/FactoryGame.FGRecipe'";
			const string DisplayNameKey = "mDisplayName";
			const string IngredientsKey = "mIngredients";
			const string ProducedInKey = "mProducedIn";
			const string ProductKey = "mProduct";

			using var docsStream = System.IO.File.OpenText(path);
			string docsString = docsStream.ReadToEnd();
			var docsJson = System.Text.Json.JsonDocument.Parse(docsString);

			var docsLen = docsJson.RootElement.GetArrayLength();

            for (UInt16 idx = 0; idx < docsLen; idx++) {
				var jsonItem = docsJson.RootElement[idx];

				print(jsonItem.GetProperty("NativeClass").ToString());

				switch (jsonItem.GetProperty("NativeClass").ToString()) {
					case RecipeClassKey:
						var jsonRcpArray = jsonItem.GetProperty("Classes");

						var rcpCount = jsonRcpArray.GetArrayLength();

						for (UInt16 rIdx = 0; rIdx < rcpCount; rIdx++) {
							var jsonRcp = jsonRcpArray[rIdx];

							foreach (var item in jsonRcp.EnumerateObject()) {
								print(item.ToString());
							}
							print();
						}
						break;
					case PartClassKey:
						var jsonPartArray = jsonItem.GetProperty("Classes");

						var partCount = jsonPartArray.GetArrayLength();

						for (UInt16 rIdx = 0; rIdx < partCount; rIdx++) {
							var jsonPart = jsonPartArray[rIdx];

							foreach (var item in jsonPart.EnumerateObject()) {
								print(item.ToString());
							}
							print();
						}
						break;
					default:
						break;
				}
			}
        }

		static void Main(string[] args) {
			// ParseGameDocs("G:\\Epic Games\\SatisfactoryExperimental\\CommunityResources\\Docs\\Docs.json");

			Recipe.LoadAllFromPath("Data/recipe_list.cfg");
			BldgPlan.LoadAllFromPath("Data/bldg_plans.cfg");

			foreach (Conditions conditions in LoadTestSpecsFromPath("TestSpec.cfg")) {
				Test(conditions);
			}
		}
	}
}
