using System;
using System.Collections.Generic;
using System.Linq;
using static Utils;

using u8 = System.Byte;
using u16 = System.UInt16;

public class Building {
	public static Dictionary<Tuple<string, Recipe>, int> CountLikeBuildings(IEnumerable<Building> bldgs, out u16 powerShardCount) {
		Dictionary<Tuple<string, Recipe>, int> result = new Dictionary<Tuple<string, Recipe>, int>();
		powerShardCount = 0;

		foreach (Building bldg in bldgs) {
			if (bldg.OCRate > 1) {
				powerShardCount += (u16)Math.Ceiling((bldg.OCRate - 1) / 0.5);
			}

			Tuple<string, Recipe> key = new Tuple<string, Recipe>(bldg.LongString(), bldg.Assignment);

			if (!result.ContainsKey(key)) {
				result[key] = 0;
			}
			result[key] += 1;
		}

		return result;
	}

	public static Dictionary<Tuple<string, Recipe>, int> CountLikeBuildings(IEnumerable<Building> bldgs) {
		u16 _;
		return CountLikeBuildings(bldgs, out _);
	}

	public static void PrintLikeBuildings(IEnumerable<Building> bldgs, bool printShardCount = true) {
		u16 powerShardCount;

		var likeBuildings = CountLikeBuildings(bldgs, out powerShardCount);

		var sortedKeys = likeBuildings.Keys.ToList();
		sortedKeys.Sort((lhs, rhs) => {
			if (lhs is null || lhs.Item2 is null)
				return 1;
			if (rhs is null || rhs.Item2 is null)
				return -1;
			return lhs.Item2.gen.CompareTo(rhs.Item2.gen);
		});

		print("***Buildings Summary:***");
		foreach (var key in sortedKeys) {
			var val = likeBuildings[key];
			print("{0} {1}".Format(val, key.Item1));
		}
		print("{0} Total Buildings{1}".Format(
			likeBuildings.Values.Sum(),
			printShardCount ? " (Needs {0} Power Shards)".Format(powerShardCount) : ""
			)
		);
	}

	public static Production SummarizeCosts(IEnumerable<Building> bldgs) {
		Production costProd = new Production();

		foreach (Building bldg in bldgs) {
			costProd.AddDemand(bldg.Plan.Costs);
		}

		return costProd;
	}

	public static void PrintCostSummary(IEnumerable<Building> bldgs, Production prod = null) {
		Production costs = SummarizeCosts(bldgs);

		print("***Cost Summary:***");
		costs.PrintDemands();

		if (prod != null) {
			print("\n***Missing Costs:***");
			print(string.Join("\n", Production.FindMissingCosts(costs, prod)));
		}
	}

	public static Production GetProdOfRecipe(IEnumerable<Building> bldgs, string rcpName) {
		Production prod = new Production();
		foreach (Building bldg in bldgs) {
			if (bldg.Assignment != null && bldg.Assignment.name == rcpName) {
				prod.Add(bldg.GetProduction());
			}
		}

		return prod;
	}

	public static double GetGrossRateOfPartByRecipe(IEnumerable<Building> bldgs, string rcpName, string partName) {
		Production prod = GetProdOfRecipe(bldgs, rcpName);

		if (prod.Gross.ContainsKey(partName)) {
			return prod.Gross[partName].rate;
		}
		else {
			return 0d;
		}
	}

	public string Name { get; protected set; }
	public double Power { get; protected set; }
	public Recipe Assignment { get; set; }

	public double RateMultiplier {
		get {
			return this.Plan.RateMultiplier;
		}
	}

	public virtual BldgPlan Plan { get; protected set; }


	protected double ocrate;
	public virtual double OCRate {
		get => this.ocrate;
		set {
			if (value < 0)
				throw new ArgumentOutOfRangeException("Building.OCRate must be non-negative.");
			if (value > 2.5)
				throw new ArgumentOutOfRangeException("Building.OCRate cannot be higher than 2.5 (got {0}).".Format(value));

			this.ocrate = Math.Ceiling(value * 100) / 100;
			this.Power = Plan.BasePower * Math.Pow(value, 1.6);
		}
	}

	public virtual void SetOCRateForNofIndex(double rate, int index) {
		double baseRate = Assignment.production[index].rate;

		double percent = Math.Ceiling(rate * 100d / baseRate);

		this.OCRate = percent / 100d;
	}

	public virtual void SetOCRateForNofPart(double rate, Part part) {
		double baseRate = Assignment.GetProdRateOfPart(part);

		double percent = Math.Ceiling(rate * 100d / baseRate);

		this.OCRate = percent / 100d;
	}

	public virtual void SetOCRateForPartTarget(Part part) {
		double baseRate = Assignment.GetProdRateOfPart(part);

		double percent = Math.Ceiling(part.rate * 100d / baseRate);

		this.OCRate = percent / 100d;
	}

	public virtual void SetOCRateForNofFirst(double rate) {
		this.SetOCRateForNofIndex(rate, 0);
	}

	public virtual Production GetProduction() {
		Production prod = Assignment.GetProductionAtMultiplier(this.OCRate * this.RateMultiplier);

		prod.PowerOutputs.Add(this.Power);

		return prod;
	}

	public override string ToString() {
		return "{0} ({1}) @ {2:P0} ({3} MW)".Format(this.Name, this.Assignment.name, this.OCRate, this.Power);
	}

	public virtual string LongString() {
		Production prod = this.GetProduction();

		return "{0} ({1}: {2}) @ {3:P0} ({4:G4} MW, demands: {5})".Format(this.Name, this.Assignment.name, string.Join(", ", prod.Gross.Values), this.OCRate, this.Power, string.Join(", ", prod.Demands.Values));
	}

	public Building(string name, BldgPlan plan, Recipe assignment = null, double ocrate = 1d) {
		if (assignment != null && !plan.BuildList.Contains(assignment)) {
			throw new ArgumentException("A building cannot be built for a recipe it can not produce ({0} does not build {1}".Format(name, assignment.name));
		}
		this.Name = name;
		this.Plan = plan;
		this.Power = plan.BasePower;
		this.Assignment = assignment;
		this.OCRate = ocrate;
	}
}
