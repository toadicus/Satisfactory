using System;
using System.Collections.Generic;
using System.Linq;
using static FuzzyCompare;
using static Utils;
using u8 = System.Byte;


public class Production {
	#region static
	public static Production CalcMinProductionFor(Production prod, double margin = FUZZY_MARGIN) {
		u8 iters = 0;
		while (prod.HasNegativeNet(margin)) {
			if (++iters > 10)
				throw new Exception("This is running away.");

			Production iterProd = new Production();

			foreach (Part part in prod.Net.Values) {
				if (AlmostGte(part.rate, 0d))
					continue;

				Recipe rcp;

				if (Recipe.TryFindRecipeFor(part.name, out rcp))
					iterProd.Add(rcp.GetNProductionByIndex(-part.rate, 0));
			}

			prod.Add(iterProd);
		}

		return prod;
	}

	public static Production CalcMinProductionFor(Recipe rcp) {
		return CalcMinProductionFor(rcp.GetProduction());
	}

	public static Production CalcMinProductionFor(Recipe[] rcps) {
		Production prod = new Production();

		foreach (Recipe rcp in rcps) {
			prod.Add(rcp.GetProduction());
		}

		return CalcMinProductionFor(prod);
	}

	public static Production CalcMinProductionFor(Part part) {
		Production prod = new Production();

		prod.AddDemand(part);

		return CalcMinProductionFor(prod);
	}

	public static bool LeftHasDemandNotInRightGross(Production lhs, Production rhs) {
		return FindMissingCosts(lhs, rhs).Count > 0;
	}

	public static List<Part> FindMissingCosts(Production lhs, Production rhs, Dictionary<string, bool> ignores = null) {
		List<Part> missingCosts = new List<Part>();

		foreach (Part dmnd in lhs.Demands.Values) {
			bool found = false;
			foreach (Part prod in rhs.Gross.Values) {
				found = dmnd.name == prod.name;

				if (found)
					break;
			}

			if (!found && (ignores is null || !ignores.ContainsKey(dmnd.name))) {
				missingCosts.Add(dmnd);
			}
		}

		return missingCosts;
	}
	#endregion

	public Dictionary<string, Part> Gross { get; private set; }
	public Dictionary<string, Part> Demands { get; private set; }

	public Dictionary<string, Part> Net { get; private set; }

	public List<double> PowerInputs { get; private set; }
	public List<double> PowerOutputs { get; private set; }

	public double NetPower {
		get {
			double powerIn = PowerInputs?.Sum() ?? 0d;
			double powerOut = PowerOutputs?.Sum() ?? 0d;

			return powerIn - powerOut;
		}
	}

	public void Add(Production rhs) {
		foreach (Part part in rhs.Gross.Values) {
			this.AddProduction(part);
		}

		foreach (Part part in rhs.Demands.Values) {
			this.AddDemand(part);
		}

		this.PowerInputs.AddRange(rhs.PowerInputs);
		this.PowerOutputs.AddRange(rhs.PowerOutputs);
	}

	public void AddBuildings(List<Building> bldgs) {
		foreach (Building bldg in bldgs) {
			this.Add(bldg.GetProduction());
		}
	}

	public void AddBuildings(Building[] bldgs) {
		foreach (Building bldg in bldgs) {
			this.Add(bldg.GetProduction());
		}
	}

	public void AddProduction(Part part) {
		string name = part.name;

		if (this.Gross.ContainsKey(name)) {
			this.Gross[name] += part;
		}
		else {
			this.Gross[name] = part;
		}

		if (this.Net.ContainsKey(name)) {
			this.Net[name] += part;
		}
		else {
			this.Net[name] = part;
		}
	}

	public void AddProduction(IEnumerable<Part> parts) {
		foreach (Part part in parts) {
			this.AddProduction(part);
		}
	}

	public void AddDemand(Part part) {
		string name = part.name;
		if (this.Demands.ContainsKey(name)) {
			this.Demands[name] += part;
		}
		else {
			this.Demands[name] = part;
		}

		if (this.Net.ContainsKey(name)) {
			this.Net[name] -= part;
		}
		else {
			this.Net[name] = -part;
		}
	}

	public void AddDemand(IEnumerable<Part> parts) {
		foreach (Part part in parts) {
			this.AddDemand(part);
		}
	}

	public void Clear() {
		this.Gross.Clear();
		this.Demands.Clear();
		this.Net.Clear();

		this.PowerInputs.Clear();
		this.PowerOutputs.Clear();
	}

	public bool HasNegativeNet(double margin = FUZZY_MARGIN) {
		foreach (Part part in this.Net.Values) {
			if (AlmostLt(part.rate, 0, margin))
				return true;
		}
		return false;
	}

	public void PrintGross() {
		foreach (Part part in this.Gross.Values) {
			print(part);
		}
	}

	public void PrintDemands() {
		foreach (Part part in this.Demands.Values) {
			print(part);
		}
	}

	public void PrintNet(bool printZeroes = false) {
		foreach (Part part in this.Net.Values) {
			if (printZeroes || AlmostNe(part.rate, 0d))
				print(part);
		}
	}

	public void PrintPower() {
		print("***Power Production***: {0:G4} MW".Format(this.PowerInputs.Sum()));
		print("***Power Consumption***: {0:G4} MW".Format(this.PowerOutputs.Sum()));
		print("***Power Balance***: {0:G4} MW".Format(this.NetPower));
	}

	public void PrintAll(bool printZeroes = false) {
		print("***Gross Production:***");
		this.PrintGross();

		print("\n***Demands:***");
		this.PrintDemands();

		print("\n***Net Production:***");
		this.PrintNet(printZeroes);

		print("\n");
		this.PrintPower();
	}

	public Production(Dictionary<string, Part> prod, Dictionary<string, Part> dems) {
		this.PowerInputs = new List<double>();
		this.PowerOutputs = new List<double>();

		this.Gross = prod;
		this.Demands = dems;

		this.Net = new Dictionary<string, Part>(prod);

		foreach ((string name, Part part) in dems) {
			if (this.Net.ContainsKey(name)) {
				this.Net[name] -= part;
			}
			else {
				this.Net[name] = -part;
			}
		}
	}

	public Production(Part[] prod, Part[] dems) : this() {
		foreach (Part part in prod) {
			this.AddProduction(part);
		}

		foreach (Part part in dems) {
			this.AddDemand(part);
		}
	}

	public Production() {
		this.Gross = new Dictionary<string, Part>();
		this.Demands = new Dictionary<string, Part>();
		this.Net = new Dictionary<string, Part>();

		this.PowerInputs = new List<double>();
		this.PowerOutputs = new List<double>();
	}

	#region OPERATORS
	public static implicit operator Production(ValueTuple<Part[], Part[]> tuple) {
		return new Production(tuple.Item1, tuple.Item2);
	}

	public static implicit operator ValueTuple<Part[], Part[]>(Production ptuple) {
		return (ptuple.Gross.Values.ToArray(), ptuple.Demands.Values.ToArray());
	}
	#endregion
}
