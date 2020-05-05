using System;
using System.Collections.Generic;
using System.Linq;
using static Utils;
using static FuzzyCompare;

using u8 = System.Byte;


public class Production
{
	#region static
	public static Production CalcMinProductionFor(Production prod) {
		while (prod.HasNegativeNet()) {
			Production iterProd = new Production();

			foreach (Part part in prod.Net.Values) {
				if (AlmostGte(part.rate, 0d))
					continue;

				// TODO: Add support for recipes with multiple parts.
				iterProd.Add(Recipe.Get(part.name).GetNProductionByIndex(-part.rate, 0));
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
	#endregion

	public Dictionary<string, Part> Gross { get; private set; }
	public Dictionary<string, Part> Demands { get; private set; }

	public Dictionary<string, Part> Net { get; private set; }

	public void Add(Production rhs) {
		foreach (Part part in rhs.Gross.Values) {
			this.AddProduction(part);
		}

		foreach (Part part in rhs.Demands.Values) {
			this.AddDemand(part);
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

	public bool HasNegativeNet() {
		foreach (Part part in this.Net.Values) {
			if (AlmostLt(part.rate, 0))
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

	public Production(Dictionary<string, Part> prod, Dictionary<string, Part> dems)
	{
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

	public Production(Part[] prod, Part[] dems) {
		this.Gross = new Dictionary<string, Part>();
		this.Demands = new Dictionary<string, Part>();
		this.Net = new Dictionary<string, Part>();

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
	}

	#region OPERATORS
	public static implicit operator Production(ValueTuple<Part[], Part[]> tuple) {
		return new Production(tuple.Item1, tuple.Item2);
	}

	public static implicit operator ValueTuple<Part[], Part[]> (Production ptuple) {
		return (ptuple.Gross.Values.ToArray(), ptuple.Demands.Values.ToArray());
	}
	#endregion
}
