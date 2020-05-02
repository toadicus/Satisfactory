using System;
using u8 = System.Byte;

public static class Utils {
    public static Part p(string name, double rate, string plural = "s") {
        return new Part(name, rate, plural);
    }

    public static Part p(Part part, double rate) {
        return part.Copy(rate);
    }

    [Obsolete("This is a naive way to pull parts from recipes; should stop using it soon.")]
    public static Part p(Recipe rcp, double rate) {
        // DEPRECATED
        return p1(rcp, rate);
    }

    public static Part p1(Recipe rcp, double rate) {
        return rcp.production[0];
    }

    public static Part p2(Recipe rcp, double rate) {
        return rcp.production[1];
    }

    public static double max(double one, double two) {
        if (two > one)
            return two;
        return one;
    }

    public static u8 max(u8 one, u8 two) {
        if (two > one)
            return two;
        return one;
    }
}