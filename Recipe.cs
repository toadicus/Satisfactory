﻿using ConfigParser;
using Satisfactory.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Cache;
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
		Recipe rcp;
		if (TryGetRecipeByName(name, out rcp)) {
			return rcp;
        }
		throw new KeyNotFoundException("No recipe named {0} exists.".Format(name));
	}

	public static bool TryGetRecipeByName(string name, out Recipe rcp) {
		if (IndexByName.ContainsKey(name)) {
			rcp = IndexByName[name];
			return true;
        } else {
			rcp = null;
			return false;
        }
    }

	public static Recipe CopyAtMultiplier(Recipe recipe) {
		List<Part> newProd = new List<Part>();
		List<Part> newDems = new List<Part>();

		return new Recipe(recipe.name, newProd, newDems, recipe.plural);
	}

	// WIP: Needs more heuristics.
	public static bool TryFindRecipeFor(string name, out Recipe rcp, List<Part> preferDemands = null, double rcpMarginFactor = FuzzyCompare.FUZZY_MARGIN) {
		rcp = null;

		if (Recipe.IndexByName.ContainsKey(name)) {
			rcp = Recipe.IndexByName[name];
        }

		if (Recipe.IndexByPart.ContainsKey(name) && (!Recipe.IndexByName.ContainsKey(name) || Recipe.IndexByPart[name].Count > 1)) {
			Recipe bestRcp = null;

			double rate = 0d;

			foreach (Recipe rRcp in Recipe.IndexByPart[name]) {
				double rRate = rRcp.GetProductionOf(name).rate;
				BldgPlan plan;
				if (!BldgPlan.TryGetPlanFor(rRcp, out plan)) {
					continue;
                }

				if (preferDemands != null) {
					bool quit = false;
					foreach (Part preference in preferDemands) {
						foreach (Part demand in rRcp.demands) {
							if (demand.name == preference.name) {
								rRate *= 1 + preference.rate / demand.rate;
								quit = true;
								break;
                            }
                        }
						if (quit)
							break;
                    }
                }
				if (rRate > rate) {
					rate = rRate;
					bestRcp = rRcp;
				}
			}

			if (rate > 0)
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

	public static void RemoveRecipeByName(string name) {
		for (UInt16 idx = 0; idx < Recipe.List.Count; idx++) {
			Recipe rcp = Recipe.List[idx];
			if (rcp.name == name) {
				Recipe.List.RemoveAt(idx);
				if (Recipe.IndexByName.ContainsKey(name))
					Recipe.IndexByName.Remove(name);

				foreach (Part part in rcp.production) {
					if (Recipe.IndexByPart.ContainsKey(part.name)) {
						Recipe.IndexByPart[part.name].Remove(rcp);

						if (Recipe.IndexByPart[part.name].Count < 1) {
							Recipe.IndexByPart.Remove(part.name);
                        }
                    }
                }

				break;
            }
        }
    }

	public static NodeDefinition GetNodeDefinition(Recipe rcp) {
		NodeDefinition node = new NodeDefinition(RecipeSpec.RECIPE_NODE);

		node.AddValue(RecipeSpec.NAME_KEY, rcp.name);
		node.AddValue(RecipeSpec.PLURAL_KEY, rcp.plural);

		NodeDefinition prodNode = new NodeDefinition(RecipeSpec.PRODUCTION_NODE);
		foreach (Part part in rcp.production) {
			prodNode.AddNode(Part.GetNodeDefinition(part));
        }
		node.AddNode(prodNode);

		NodeDefinition dmndNode = new NodeDefinition(RecipeSpec.DEMANDS_NODE);
		foreach (Part part in rcp.demands) {
			dmndNode.AddNode(Part.GetNodeDefinition(part));
        }
		node.AddNode(dmndNode);

		return node;
    }


	public static Recipe GetRecipeFromNode(NodeDefinition node) {
		string name = node.GetValue(RecipeSpec.NAME_KEY);
		string plural = node.GetValue(RecipeSpec.PLURAL_KEY);

		NodeDefinition prodNode = node.GetFirstNodeByName(RecipeSpec.PRODUCTION_NODE);
		List<Part> production = new List<Part>();
		foreach (NodeDefinition partNode in prodNode.GetNodesByName(RecipeSpec.PART_NODE)) {
			Part prod = Part.GetPartFromNode(partNode);
			production.Add(prod);
        }

		NodeDefinition dmndNode = node.GetFirstNodeByName(RecipeSpec.DEMANDS_NODE);
		List<Part> demands = new List<Part>();
		var dmndNodes = dmndNode.GetNodesByName(RecipeSpec.PART_NODE);
		if (dmndNodes != null) {
			foreach (NodeDefinition partNode in dmndNode.GetNodesByName(RecipeSpec.PART_NODE)) {
				Part dmnd = Part.GetPartFromNode(partNode);
				demands.Add(dmnd);
			}
		}

		return new Recipe(name, production, demands, plural, true);
	}

	public static void LoadAllFromPath(string path) {
		var nodes = ConfigFile.LoadFromPath(path);

		foreach (NodeDefinition node in nodes) {
			Recipe rcp = Recipe.GetRecipeFromNode(node);
		}

		foreach (Recipe rcp in Recipe.List) {
			rcp.gen = rcp.CalculateGeneration();
        }
	}
	#endregion
	public string name { get; }
	public string plural { get; }

	public List<Part> production { get; }
	public List<Part> demands { get; }
	public List<string> provides { get; }

	public u8 gen { get; protected set; }

	[Obsolete("Recipe.part is a legacy method and should be avoided.")]
	public Part part {
		get {
			// Returns the first part, because this is how it worked before.
			return this.production[0];
		}
	}

	public double GetProdRateOfPart(Part part) {
		return GetProdRateByName(part.name);
	}

	public double GetProdRateByName(string partName) {
		foreach (Part p in this.production) {
			if (p.name == partName) {
				return p.rate;
            }
        }

		throw new KeyNotFoundException("Recipe {0} does not provide part {1}".Format(this.name, partName));
    }

	public double GetDemandRateByName(string demName) {
		foreach (Part d in this.demands) {
			if (d.name == demName) {
				return d.rate;
            }
        }

		throw new KeyNotFoundException("Recipe {0} does not demand part {1}".Format(this.name, demName));
    }

	public double GetDemandRateOfPart(Part part) {
		return GetDemandRateByName(part.name);

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
		double mult = req / this.production[idx].rate;

		return GetProductionAtMultiplier(mult);
	}

	public Production GetNProductionOfPart(double req, Part part) {
		double mult = req / GetProdRateOfPart(part);

		return GetProductionAtMultiplier(mult);
	}

	public Production GetNProductionByDemand(double req, Part part) {
		double mult = req / GetDemandRateOfPart(part);

		return GetProductionAtMultiplier(mult);
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

	public bool HasDemand(string name) {
		return this.demands.Any(p => p.name == name);
    }

	public bool HasDemand(Part part) {
		return this.HasDemand(part.name);
    }

	public override string ToString() {
		return string.Join(", ", this.production.ConvertAll(p => p.ToString()));
	}

	public u8 CalculateGeneration() {
		if (this.gen > 0)
			return this.gen;

		u8 g = 1;

		u8 sub = 0;
		foreach (Part part in this.demands) {
			if (Recipe.TryFindRecipeFor(part.name, out Recipe rcp)) {
				sub = max(rcp.CalculateGeneration());
			}
		}

		g += sub;

		return g;
	}

	#region CONSTRUCTOR
	private Recipe(string name, List<Part> production, List<Part> demands, string plural, bool deferGeneration = false) {
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

		if (!deferGeneration)
			this.gen = this.CalculateGeneration();

		List.Add(this);
	}
	#endregion
}
