using System;
using System.Collections.Generic;
using System.Linq;
using static Utils;

using u8 = System.Byte;


public class Production
{
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

	public void PrintNet() {
		foreach (Part part in this.Net.Values) {
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
