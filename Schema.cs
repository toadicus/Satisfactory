using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Satisfactory.Schema {
    public static class TestSpec {
        public const string NAME_KEY = "name";
        public const string RATE_KEY = "rate";
        public const string IGN_COSTS_KEY = "ignoreCosts";
        public const string IGN_POWER_KEY = "ignorePower";
        public const string MIN_OC_KEY = "minOCRate";
        public const string MAX_OC_KEY = "maxOCRate";
        public const string MARGIN_FACTOR_KEY = "rcpMarginFactor";
        public const string IGNORE_RECIPES_KEY = "ignoreRecipes";
        public const string ALLOW_DEM_PREF_KEY = "allowDemandPreference";

        public const string PART_TGT_NODE = "PART_TARGET";
        public const string CONDITIONS_NODE = "CONDITIONS";
    }

    public static class PartSpec {
        public const string NAME_KEY = "name";
        public const string PLURAL_KEY = "plural";
        public const string RATE_KEY = "rate";

        public const string PART_NODE = "PART";
    }

    public static class RecipeSpec {
        public const string NAME_KEY = "name";
        public const string PLURAL_KEY = "plural";
        public const string RATE_KEY = "rate";

        public const string PART_NODE = "PART";
        public const string RECIPE_NODE = "RECIPE";
        public const string PRODUCTION_NODE = "PRODUCTION";
        public const string DEMANDS_NODE = "DEMANDS";
    }

    public static class PlanSpec {
        public const string NAME_KEY = "name";
        public const string BASE_POWER_KEY = "basePower";
        public const string RATE_MULT_KEY = "rateMultiplier";
        public const string BUILD_LIST_KEY = "buildList";

        public const string COSTS_NODE = "COSTS";
        public const string PLAN_NODE = "PLAN";
        public const string GENR_NODE = "GENR";
    }
}
