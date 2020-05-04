﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Utils;

using u8 = System.Byte;

public class Recipe {
	#region STATIC
	public static Dictionary<string, Recipe> IndexByName { get; private set; }
	public static Dictionary<string, List<Recipe>> IndexByProvides { get; private set; }
	public static List<Recipe> List { get; private set; }

	static Recipe() {
		Recipe.IndexByName = new Dictionary<string, Recipe>();
		Recipe.IndexByProvides = new Dictionary<string, List<Recipe>>();
		List = new List<Recipe>();

		RuntimeHelpers.RunClassConstructor(typeof(RecipeDefs).TypeHandle);
	}

	public static Recipe New(string name, List<Part> production, List<Part> demands, string plural = "s") {
		return new Recipe(name, production, demands, plural);
	}

	public static Recipe New(string name, Part[] production, Part[] demands = null, string plural = "s") {
		List<Part> prodParts = new List<Part>();
		List<Part> demandParts = new List<Part>();

		foreach (Part p in production) {
			prodParts.Add(new Part(p.name, p.rate, p.plural));
		}

		if (!(demands is null)) {
			foreach (Part p in demands) {
				demandParts.Add(new Part(p.name, p.rate, p.plural));
			}
		}

		return new Recipe(name, prodParts, demandParts, plural);
	}

	public static Recipe New(string name, ValueTuple<Part, Part> production, ValueTuple<Part, Part> demands, string plural = "s") {
		return Recipe.New(name, new Part[] { production.Item1, production.Item2 }, new Part[] { demands.Item1, demands.Item2 }, plural);
	}

	public static Recipe New(ValueTuple<Part, Part> production, ValueTuple<Part, Part> demands, string plural = "s") {
		return Recipe.New(production.Item1.name, production, demands, plural);
	}

	public static Recipe New(ValueTuple<Part, Part> production, Part demand, string plural = "s") {
		return Recipe.New(production.Item1.name, new Part[] { production.Item1, production.Item2 }, new Part[] { demand }, plural);
	}

	public static Recipe New(string name, double rate, Part[] demands = null, string plural = "s") {
		List<Part> prodParts = new List<Part>() { new Part(name, rate, plural) };
		List<Part> demandParts = new List<Part>();

		if (!(demands is null)) {
			foreach (Part p in demands) {
				demandParts.Add(new Part(p.name, p.rate, p.plural));
			}
		}

		return new Recipe(name, prodParts, demandParts, plural);
	}

	public static Recipe New(string name, double rate, Part demand, string plural = "s") {
		List<Part> prodParts = new List<Part>() { new Part(name, rate, plural) };
		List<Part> demandParts = new List<Part>() { demand };

		return new Recipe(name, prodParts, demandParts, plural);
	}

	public static Recipe New(string name, double rate, ITuple demands = null, string plural = "s") {
		Part[] demandArray = new Part[demands.Length];

		for (u8 idx = 0; idx < demands.Length; idx++) {
			demandArray[idx] = (Part)demands[idx];
		}

		return Recipe.New(name, rate, demandArray, plural);
	}

	public static Recipe New(string name, double rate, string plural = "s") {
		List<Part> prodParts = new List<Part>() { new Part(name, rate, plural) };
		List<Part> demandParts = new List<Part>();

		return new Recipe(name, prodParts, demandParts, plural);
	}

	public static Recipe Get(string name) {
		return IndexByName[name];
	}

	public static Recipe CopyAtMultiplier(Recipe recipe, double multiplier) {
		List<Part> newProd = new List<Part>();
		List<Part> newDems = new List<Part>();

		foreach (Part part in recipe.production) {
			newProd.Add(part * multiplier);
		}

		foreach (Part part in recipe.demands) {
			newDems.Add(part * multiplier);
		}

		return new Recipe(string.Format("{0}×{1:.3g}", recipe.name, multiplier), newProd, newDems, recipe.plural);
	}

	public static Production Dive(Recipe rcp, Production prod = null) {
		prod ??= new Production();

		prod.Add(rcp.GetProduction());

		foreach (Part part in prod.Net.Values) {
			if (part.rate >= 0)
				continue;

			// WIP
		}

		return prod;
	}
	#endregion
	public string name { get; }
	public string plural { get; }

	public List<Part> production { get; }
	public List<Part> demands { get; }
	public List<string> provides { get; }

	public u8 gen { get; }

	[Obsolete("Recipe.part is a legacy method and should be avoided.")]
	public Part part {
		get {
			// Returns the first part, because this is how it worked before.
			return this.production[0];
		}
	}

	public Production GetProduction() {
		return this.GetProductionAtMultiplier(1);
	}

	public Production GetProductionAtMultiplier(double multiplier = 1d) {
		Part[] prod = new Part[this.production.Count];
		Part[] dems = new Part[this.demands.Count];

		for (u8 idx = 0; idx < this.production.Count; idx++) {
			prod[idx] = this.production[idx] * multiplier;
		}

		for (u8 idx = 0; idx < this.demands.Count; idx++) {
			dems[idx] = this.demands[idx] * multiplier;
		}

		return new Production(prod, dems);
	}

	public Production GetNProductionByIndex(double req, u8 idx) {
		double rate = req / this.production[idx].rate;

		return GetProductionAtMultiplier(rate);
	}

	public bool Provides(Part part) {
		return provides.Contains(part.name);
	}

	public bool Provides(string partName) {
		return provides.Contains(partName);
	}

	public override string ToString() {
		return string.Format("{0} ({1})",
			string.Join(", ", this.production.ConvertAll(p => p.ToString())),
			string.Join(", ", this.demands.ConvertAll(p => p.ToString()))
			);
	}

	protected u8 CalculateGeneration() {
		u8 g = 1;

		u8 sub = 0;
		foreach (Part part in this.demands) {
			sub = max(sub, Recipe.Get(part.name).gen);
		}

		g += sub;

		return g;
	}

	#region CONSTRUCTOR
	private Recipe(string name, List<Part> production, List<Part> demands, string plural) {
		if (IndexByName.ContainsKey(name)) {
			throw new ArgumentException(string.Format("A part named {0} already exists! ({1})", name, IndexByName[name]));
		}

		this.name = name;
		this.production = production;
		this.demands = demands;
		this.provides = new List<string>();
		this.plural = plural;

		foreach (Part part in production) {
			this.provides.Add(part.name);

			if (!IndexByProvides.ContainsKey(part.name)) {
				IndexByProvides[part.name] = new List<Recipe>();
			}
			IndexByProvides[part.name].Add(this);
			IndexByName[part.name] = this;
		}

		this.gen = this.CalculateGeneration();

		List.Add(this);
	}
	#endregion
}
