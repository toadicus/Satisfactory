using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Utils;
using u8 = System.Byte;

public class Building {

}

public class BldgPlan
{
	public static List<BldgPlan> List;
	public static Dictionary<string, BldgPlan> IndexByRecipe;

	static BldgPlan() {
		List = new List<BldgPlan>();
		IndexByRecipe = new Dictionary<string, BldgPlan>();
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

	public string Name { get; protected set; }
	public double BasePower { get; protected set; }
	public List<Recipe> BuildList { get; protected set; }
	public List<Part> Costs { get; protected set; }
	public double RateMultiplier { get; protected set; }

	protected BldgPlan(string name, double basePower, List<Part> costs, List<Recipe> buildList, double rateMultiplier) {
		this.Name = name;
		this.BasePower = basePower;
		this.Costs = costs;
		this.BuildList = BuildList;
		this.RateMultiplier = rateMultiplier;
	}
}
