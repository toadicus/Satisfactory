using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Utils;
using u8 = System.Byte;

public class Recipe {
	#region STATIC
	public static Dictionary<string, Recipe> IndexByName { get; private set; }
	public static Dictionary<string, List<Recipe>> IndexByPart { get; private set; }
	public static List<Recipe> List { get; private set; }

	static Recipe() {
		Recipe.IndexByName = new Dictionary<string, Recipe>();
		Recipe.IndexByPart = new Dictionary<string, List<Recipe>>();
		List = new List<Recipe>();

		RuntimeHelpers.RunClassConstructor(typeof(RecipeDefs).TypeHandle);
	}

	public static Recipe New(string name, List<Part> production, List<Part> demands, string plural = "s") {
		return new Recipe(name, production, demands, plural);
	}

	public static Recipe New(ValueTuple<Part, Part> production, ValueTuple<Part, Part> demands, string plural = "s") {
		return Recipe.New(production.Item1.name, production, demands, plural);
	}

	public static Recipe New(string name, ValueTuple<Part, Part> production, ValueTuple<Part, Part> demands, string plural = "s") {
		return Recipe.New(name, new List<Part> { production.Item1, production.Item2 }, new List<Part> { demands.Item1, demands.Item2 }, plural);
	}

	public static Recipe New(string name, Part production, ValueTuple<Part, Part> demands, string plural = "s") {
		return Recipe.New(name, new List<Part> { production }, new List<Part> { demands.Item1, demands.Item2 }, plural);
	}

	public static Recipe New(string name, Part production, Part demand, string plural = "s") {
		return Recipe.New(name, new List<Part> { production }, new List<Part> { demand }, plural);
	}

	public static Recipe New(ValueTuple<Part, Part> production, Part demand, string plural = "s") {
		return Recipe.New(production.Item1.name, new List<Part> { production.Item1, production.Item2 }, new List<Part> { demand }, plural);
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

	public static Recipe CopyAtMultiplier(Recipe recipe) {
		List<Part> newProd = new List<Part>();
		List<Part> newDems = new List<Part>();

		return new Recipe(recipe.name, newProd, newDems, recipe.plural);
	}

	// WIP: Needs more heuristics.
	public static bool TryFindRecipeFor(string name, out Recipe rcp) {
		if (Recipe.IndexByName.ContainsKey(name)) {
			rcp = Recipe.IndexByName[name];
		}
		else {
			// There's no Recipe by this name; let's look for other matches that can be built.
			Recipe bestRcp = null;

			double rate = 0d;

			// HACK: This just finds the way to make the most of a thing in a single recipe.
			foreach (Recipe rRcp in Recipe.IndexByPart[name]) {
				if (BldgPlan.IndexByRecipe.ContainsKey(rRcp.name)) {
					double rRate = rRcp.GetProductionOf(name).rate;
					if (rRate > rate) {
						rate = rRate;
						bestRcp = rRcp;
					}
				}
			}

			rcp = bestRcp;
		}

		return rcp != null;
	}

	public static Recipe FindRecipeFor(string name) {
		Recipe rcp;
		if (TryFindRecipeFor(name, out rcp)) {
			return rcp;
		}

		throw new ArgumentOutOfRangeException("No recipe producing {0} exists.".Format(name));
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

	public double GetRateOfPart(Part part) {
		foreach (Part p in this.production) {
			if (part.name == p.name) {
				return p.rate;
			}
		}

		throw new ArgumentOutOfRangeException("Recipe {0} does not provide part {1}".Format(this.name, part.name));
	}

	public bool TryGetIndexOf(string name, out u8 idx) {
		u8 _idx;
		for (_idx = 0; _idx < this.production.Count; _idx++) {
			if (this.production[_idx].name == name) {
				idx = _idx;
				return true;
			}
		}

		idx = u8.MaxValue;
		return false;
	}

	public u8 GetIndexOf(string name) {
		u8 res;
		if (TryGetIndexOf(name, out res)) {
			return res;
		}
		else {
			throw new ArgumentOutOfRangeException("Recipe {0} does not provide part named {1}".Format(this.name, name));
		}
	}

	public u8 GetIndexOf(Part part) {
		return this.GetIndexOf(part.name);
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

	public Production GetNProductionOfPart(double req, Part part) {
		double rate = part.rate / GetRateOfPart(part);

		return GetProductionAtMultiplier(rate);
	}

	public Part GetProductionOf(string name) {
		foreach (Part p in this.production) {
			if (p.name == name) {
				return p;
			}
		}

		throw new ArgumentOutOfRangeException("Recipe {0} does not produce a part named {1}.".Format(this.name, name));
	}

	public bool Provides(Part part) {
		return provides.Contains(part.name);
	}

	public bool Provides(string partName) {
		return provides.Contains(partName);
	}

	public override string ToString() {
		return string.Join(", ", this.production.ConvertAll(p => p.ToString()));
	}

	protected u8 CalculateGeneration() {
		u8 g = 1;

		u8 sub = 0;
		foreach (Part part in this.demands) {
			Recipe rcp;
			if (TryFindRecipeFor(part.name, out rcp)) {
				sub = max(sub, rcp.gen);
			}
			else {
				sub = 0;
			}
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

			if (!IndexByPart.ContainsKey(part.name)) {
				IndexByPart[part.name] = new List<Recipe>();
			}
			IndexByPart[part.name].Add(this);
		}

		IndexByName[name] = this;

		this.gen = this.CalculateGeneration();

		List.Add(this);
	}
	#endregion
}
