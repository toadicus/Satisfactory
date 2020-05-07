using System;
using System.Linq;
using static RecipeDefs;
using static BuildingDefs;
using static Utils;
using System.Collections.Generic;

namespace Satisfactory {
	class Program {
		static void Main(string[] args) {
			List<Building> bldgs = new List<Building> {
				foundry.Build(aluminum_ingot),
				foundry.Build(aluminum_ingot),
				foundry.Build(aluminum_ingot),
			};

			(var result, var prod) = BldgPlan.ProcessBuildings(bldgs, ignoreCosts: true);

			Building.PrintLikeBuildings(bldgs);
			print();

			Building.PrintCostSummary(bldgs);
			print();

			prod.PrintAll();
		}
	}
}
