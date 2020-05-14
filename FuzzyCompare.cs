using System;

public static class FuzzyCompare {
	public const double FUZZY_MARGIN = 1e-6d;

	public static bool AlmostEq(double lhs, double rhs, double margin = FUZZY_MARGIN) {
		return abs(lhs - rhs) < margin;
	}

	public static bool AlmostNe(double lhs, double rhs, double margin = FUZZY_MARGIN) {
		return !AlmostEq(lhs, rhs);
	}

	public static bool AlmostLte(double lhs, double rhs, double margin = FUZZY_MARGIN) {
		return lhs <= rhs + margin;
	}

	public static bool AlmostLt(double lhs, double rhs, double margin = FUZZY_MARGIN) {
		return AlmostLte(lhs, rhs, margin) && !AlmostEq(lhs, rhs, margin);
	}

	public static bool AlmostGte(double lhs, double rhs, double margin = FUZZY_MARGIN) {
		return lhs + margin >= rhs;
	}

	public static bool AlmostGt(double lhs, double rhs, double margin = FUZZY_MARGIN) {
		return AlmostGte(lhs, rhs, margin) && !AlmostEq(lhs, rhs, margin);
	}

	public static double FuzzyCeiling(double num, double margin = FUZZY_MARGIN) {
		double inum = (int)num;

		if (AlmostGt(num, inum, margin)) {
			return inum + 1d;
		}
		return inum;
	}

	public static double abs(double n) {
		return Math.Abs(n);
	}
}
