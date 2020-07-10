using System;

public class Generator : Building {
	public Part FuelRate { get; protected set; }

	public override Double OCRate {
		get => base.OCRate;
		set {
			if (value < 0)
				throw new ArgumentOutOfRangeException("Building.OCRate must be non-negative.");

			this.ocrate = Math.Ceiling(value * 100) / 100;
			this.Power = Plan.BasePower * Math.Pow(value, 1 / 1.3);
			this.FuelRate = (Plan as GenrPlan).BaseFuelRate * Math.Pow(value, 1 / 1.3);
		}
	}

	public override Production GetProduction() {
		Production prod = new Production();

		prod.PowerInputs.Add(this.Power);
		prod.AddDemand(this.FuelRate);

		return prod;
	}

	public override void SetOCRateForNofFirst(Double rate) {
		this.SetOCRateForNofIndex(rate, 0);
	}

	public override void SetOCRateForNofIndex(Double rate, Int32 index) {
		throw new Exception("Generators do not produce a part, so cannot request an OC rate to produce N parts.");
	}

	public override String ToString() {
		return "{0} @ {1:P0} ({2}, {3})".Format(this.Name, this.ocrate, this.Power, string.Join(", ", this.FuelRate));
	}

	public override String LongString() {
		return "{0} @ {1:P0} ({2}, {3})".Format(this.Name, this.ocrate, this.Power, string.Join(", ", this.FuelRate));
	}
	public Generator(string name, BldgPlan plan, Part baseFuelRate, double ocrate) : base(name, plan, ocrate: ocrate) {
		this.FuelRate = baseFuelRate;
	}
}
