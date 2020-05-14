using System.Collections.Generic;
using static BuildingDefs;
using static RecipeDefs;
using static Utils;

namespace Satisfactory {
	class Program {
		static void Test(Building[] b, double maxOCRate = 1.0d) {
			print("\nTesting at max clock rate of {0:P0}\n".Format(maxOCRate));
			List<Building> bldgs = new List<Building>(b);

			Production prod;
			(bldgs, prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts: true, maxOCRate: maxOCRate, rcpMarginFactor: .02);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}

		static void Main(string[] args) {
			var b = new Building[] {
				foundry.Build(aluminum_ingot),
				foundry.Build(aluminum_ingot),
			};

			Test(b);
			Test(b, 1.05);
			Test(b, 1.1);
			Test(b, 1.5);
			Test(b, 2);
			Test(b, 2.5);
		}
	}
}
