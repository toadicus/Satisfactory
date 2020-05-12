using System;
using System.Collections.Generic;
using static Utils;

public class Building {
	public static Dictionary<string, int> CountLikeBuildings(IEnumerable<Building> bldgs) {
		Dictionary<string, int> result = new Dictionary<string, int>();

		foreach (Building bldg in bldgs) {
			string key = bldg.LongString();

			if (!result.ContainsKey(key)) {
				result[key] = 0;
			}
			result[key] += 1;
		}

		return result;
	}

	public static void PrintLikeBuildings(IEnumerable<Building> bldgs) {
		var likeBuildings = CountLikeBuildings(bldgs);

		print("***Buildings Summary:***");
		foreach ((var key, var val) in likeBuildings) {
			print("{0} {1}".Format(val, key));
		}
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
		double baseRate = Assignment.GetRateOfPart(part);

		double percent = Math.Ceiling(rate * 100d / baseRate);

		this.OCRate = percent / 100d;
	}

	public virtual void SetOCRateForPartTarget(Part part) {
		double baseRate = Assignment.GetRateOfPart(part);

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

		return "{0} ({1}) @ {2:P0} ({3:G4} MW, demands: {4})".Format(this.Name, string.Join(", ", prod.Gross.Values), this.OCRate, this.Power, string.Join(", ", prod.Demands.Values));
	}

	public Building(string name, BldgPlan plan, Recipe assignment = null, double ocrate = 1d) {
		this.Name = name;
		this.Plan = plan;
		this.Power = plan.BasePower;
		this.Assignment = assignment;
		this.OCRate = ocrate;
	}
}
