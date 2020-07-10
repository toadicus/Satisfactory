using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConfigParser;
using u8 = System.Byte;

public class GenrPlan : BldgPlan {
    #region STATIC
    public static List<GenrPlan> GenrList = new List<GenrPlan>();
	public static Dictionary<string, GenrPlan> IndexByFuelType = new Dictionary<string, GenrPlan>();

	public static GenrPlan New(String name, Double basePower, Part baseFuelRate, ITuple costs, ITuple buildList = null) {
		List<Part> cList = new List<Part>();

		for (u8 idx = 0; idx < costs.Length; idx++) {
			cList.Add((Part)costs[idx]);
		}

		return new GenrPlan(name, basePower, baseFuelRate, cList);
	}

	public static GenrPlan Get(string fuelName) {
		return IndexByFuelType[fuelName];
	}

	public static Generator[] MakeGenrsForPower(double powerNeed, string fuelTypeName) {
		Generator[] genrList;

		GenrPlan genrPlan = GenrPlan.Get(fuelTypeName);

		u8 count = (u8)Math.Ceiling(powerNeed / genrPlan.BasePower);

		genrList = new Generator[count];

		for (u8 idx = 0; idx < count; idx++) {
			genrList[idx] = genrPlan.Build();
		}

		return genrList;
	}

	public static new GenrPlan GetPlanFromNode(NodeDefinition node) {
		string name = node.GetValue("name");
		double basePower = double.Parse(node.GetValue("basePower"));

		NodeDefinition fuelNode = node.GetFirstNodeByName("FUEL").GetFirstNodeByName("PART");

		Part baseFuelRate = Part.GetPartFromNode(fuelNode);

		NodeDefinition costsNode = node.GetFirstNodeByName("COSTS");
		var costNodes = costsNode.GetNodesByName("PART");

		List<Part> costs = new List<Part>();

		foreach (NodeDefinition costNode in costNodes) {
			Part cost = Part.GetPartFromNode(costNode);
			costs.Add(cost);
		}

		return new GenrPlan(name, basePower, baseFuelRate, costs);
	}
	#endregion

	public Part BaseFuelRate { get; protected set; }

	public Generator Build(Double ocrate = 1) {
		return new Generator(this.Name, this, this.BaseFuelRate, ocrate);
	}

	public override String ToString() {
		return string.Format("Plan for {0} ({3}, {1} MW, costs: {2})", this.Name, this.BasePower, string.Join(", ", this.Costs), this.BaseFuelRate);
	}

	protected GenrPlan(string name, double basePower, Part baseFuelRate, List<Part> costs) : base(name, basePower, costs, null, 1d) {
		this.BaseFuelRate = baseFuelRate;

		GenrList.Add(this);

		if (IndexByFuelType.ContainsKey(baseFuelRate.name))
			throw new ArgumentException("Each fuel type may have only one generator (got new GenrPlan {0} for fuel type {1}).".Format(this.Name, baseFuelRate.name));

		IndexByFuelType[baseFuelRate.name] = this;
	}
}
