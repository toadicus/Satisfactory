using System;
using System.Linq;
using static RecipeDefs;
using static BuildingDefs;
using static Utils;

namespace Satisfactory {
	class Program {
		static void Main(string[] args) {
			var rcpsByGen = Recipe.List.OrderBy(r => r.gen).ToArray();

			foreach (var rcp in rcpsByGen) {
				Console.WriteLine(rcp);
			}

			Console.WriteLine("");

			foreach (var bplan in BldgPlan.List) {
				Console.WriteLine(bplan);
				Console.WriteLine("");
			}

			Production production = stator.GetProductionAtMultiplier();

			production.Add(steel_pipe.GetProduction());

			print("***Gross Production:***");
			production.PrintGross();

			print("\n***Demands:***");
			production.PrintDemands();

			print("\n***Net Production:***");
			production.PrintNet();
		}
	}
}
