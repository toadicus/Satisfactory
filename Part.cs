using System;
using u8 = System.Byte;

public struct Part {
	#region STATIC
	public static bool TryGetGeneration(Part part, out u8 gen) {
		Recipe rcp;
		if (Recipe.TryFindRecipeFor(part.name, out rcp)) {
			gen = rcp.gen;
			return true;
		}

		gen = u8.MaxValue;
		return false;
	}

	public static u8 GetGeneration(Part part) {
		u8 gen;
		if (TryGetGeneration(part, out gen)) {
			return gen;
		}
		throw new Exception("No recipe exists for part named {0}".Format(part.name));
	}
	#endregion
	public string name { get; private set; }
	public string plural { get; private set; }
	public double rate { get; private set; }

	public Part(string name, double rate, string plural = "s") {
		this.name = name;
		this.rate = rate;
		this.plural = plural;
	}

	public Part Copy(double newRate = double.NaN) {
		if (newRate is double.NaN) {
			newRate = this.rate;
		}

		return new Part(this.name, newRate, this.plural);
	}

	public override string ToString() {
		return string.Format("{0:G4} {1}", this.rate, this.name);
	}

	public static Part operator +(Part lhs, Part rhs) {
		if (lhs.name != rhs.name) {
			throw new ArgumentException(string.Format("Parts may only be added to or subtracted from other parts of the same resource (got {0} and {1}).", lhs.name, rhs.name));
		}
		//if (this.name != rhs.name)
		//{
		//	return new CompoundPart(this, rhs);
		//}
		else {
			return new Part(lhs.name, lhs.rate + rhs.rate);
		}
	}

	public static Part operator -(Part un) {
		return new Part(un.name, -un.rate);
	}

	public static Part operator -(Part lhs, Part rhs) {
		return lhs + -rhs;
	}

	public static Part operator *(Part lhs, double rhs) {
		return new Part(lhs.name, lhs.rate * rhs);
	}

	public static Part operator /(Part lhs, double rhs) {
		return new Part(lhs.name, lhs.rate / rhs);
	}
}
