using System.Collections.Generic;
using static BuildingDefs;
using static RecipeDefs;
using static Utils;

namespace Satisfactory {
	class Program {
		static void Main(string[] args) {
			List<Building> bldgs = new List<Building> {
				foundry.Build(aluminum_ingot),
				foundry.Build(aluminum_ingot),
			};

			(var result, var prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts: true, maxOCRate: 1.05, rcpMarginFactor: .02);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}
	}
}
