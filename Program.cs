using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using static RecipeDefs;

namespace Satisfactory {
    class Program {
        static void Main(string[] args) {
            var rcps = Recipe.List.OrderBy(r => r.gen).ToArray();

            foreach (var rcp in rcps) {
                Console.WriteLine(rcp);
            }
        }
    }
}
