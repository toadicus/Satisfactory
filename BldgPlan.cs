using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using u8 = System.Byte;

public class Building {
	public string Name { get; protected set; }
	public BldgPlan Plan { get; protected set; }
	public Recipe Assignment { get; set; }

	protected double ocrate;
	public double OCRate { get {
			return ocrate;
		}
		set {
			if (value < 0)
				throw new ArgumentOutOfRangeException("Building.OCRate must be non-negative.");
			this.ocrate = value;
			this.Power = Plan.BasePower * Math.Pow(value, 1.6);
		}
	}

	public double Power { get; protected set; }
	public Part[] GetProduction() {
		return Assignment.production.ToArray();
	}

	public Building(string name, BldgPlan plan, Recipe assignment = null, double ocrate = 1d) {
		this.Name = name;
		this.Plan = plan;
		this.Power = plan.BasePower;
		this.Assignment = assignment;
		this.OCRate = ocrate;
	}
}

public class BldgPlan {
	public static List<BldgPlan> List;
	public static Dictionary<string, BldgPlan> IndexByRecipe;

	static BldgPlan() {
		List = new List<BldgPlan>();
		IndexByRecipe = new Dictionary<string, BldgPlan>();

		RuntimeHelpers.RunClassConstructor(typeof(BuildingDefs).TypeHandle);
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

	protected List<Recipe> buildList;
	public List<Recipe> BuildList {
		get {
			return buildList;
		}
		set {
			buildList = value;
		}
	}

	public List<Part> Costs { get; protected set; }
	public double RateMultiplier { get; protected set; }

	public override System.String ToString() {
		return string.Format("Plan for {0} ({1} MW, costs: {2})\nBuilds:\n\t{3}", this.Name, this.BasePower, string.Join(", ", this.Costs), string.Join(",\n\t", this.BuildList));
	}

	protected BldgPlan(string name, double basePower, List<Part> costs, List<Recipe> buildList, double rateMultiplier) {
		this.Name = name;
		this.BasePower = basePower;
		this.Costs = costs;
		this.BuildList = BuildList;
		this.RateMultiplier = rateMultiplier;

		BldgPlan.List.Add(this);
	}
}
