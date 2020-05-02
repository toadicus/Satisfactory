using System;

public struct Part {
	public string name { get; private set; }
	public string plural { get; private set; }
	public double rate { get; private set; }

	public Part(string name, double rate, string plural = "s") {
		this.name = name;
		this.rate = rate;
		this.plural = plural;
	}

	public Part Copy(double newRate = double.NaN) {
		if (newRate == double.NaN) {
			newRate = this.rate;
		}

		return new Part(this.name, newRate, this.plural);
	}

	public override string ToString() {
		return string.Format("{0} {1}", this.rate, this.name);
	}

	public string LongString() {
		// TODO: Implement long string for parts once recipe lookup is available. 
		return string.Format("{0} {1}", this.rate, this.name);
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
