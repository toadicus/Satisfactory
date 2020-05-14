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
				//refinery.Build(alumina_sln, 1.02),
				//refinery.Build(alumina_sln, 1.02),
				//refinery.Build(alumina_sln, 1.02),
				//refinery.Build(alumina_sln, 1.02),
				//refinery.Build(alumina_sln, .81),
				//refinery.Build(alumina_sln, .81),
				//refinery.Build(alumina_sln, .81),
				//refinery.Build(alumina_sln, .81),
				//refinery.Build(aluminum_scrap, .67),
				//refinery.Build(aluminum_scrap, .67)
			};

			(var result, var prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts: true, maxOCRate: 1.1, rcpMarginFactor: .02);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}
	}
}
