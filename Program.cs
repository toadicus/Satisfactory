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
			(bldgs, prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts: true, ignorePower: true, maxOCRate: maxOCRate, rcpMarginFactor: .02);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}

		static void Main(string[] args) {
			var b = new Building[] {
				foundry.Build(turbo_motor),
				manufacturer.Build(nuclear_fuel_rod),
				assembler.Build(encased_beam),
				constructor.Build(iron_plate),
				constructor.Build(iron_rod),
				constructor.Build(steel_pipe),
				constructor.Build(copper_sheet),
				constructor.Build(cable),
				constructor.Build(wire)
			};

			Test(b, 1.05);
		}
	}
}
