using System;
using static Utils;

public class Building {
	public string Name { get; protected set; }
	public double Power { get; protected set; }
	public Recipe Assignment { get; set; }

	public BldgPlan Plan { get; protected set; }


	protected double ocrate;
	public double OCRate {
		get {
			return ocrate;
		}
		set {
			if (value < 0)
				throw new ArgumentOutOfRangeException("Building.OCRate must be non-negative.");
			this.ocrate = value;
			this.Power = Plan.BasePower * Math.Pow(value, 1.6);
		}
	}

	public Production GetProduction() {
		return Assignment.GetProductionAtMultiplier(this.OCRate);
	}

	public override string ToString() {
		return "{0} ({1}) @ {2:.0%} ({3} MW)".Format(this.Name, this.Assignment, this.OCRate, this.Power);
	}

	public Building(string name, BldgPlan plan, Recipe assignment = null, double ocrate = 1d) {
		this.Name = name;
		this.Plan = plan;
		this.Power = plan.BasePower;
		this.Assignment = assignment;
		this.OCRate = ocrate;
	}
}
