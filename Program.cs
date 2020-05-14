using System.Collections.Generic;
using static BuildingDefs;
using static RecipeDefs;
using static Utils;

namespace Satisfactory {
	class Program {
		static void Main(string[] args) {
			var b = new Building[] {
				foundry.Build(aluminum_ingot),
				foundry.Build(aluminum_ingot),
			};

			List<Building> bldgs = new List<Building>(b);

			Production prod;
			(bldgs, prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts: true, maxOCRate: 1.1, rcpMarginFactor: .015);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}
	}
}
