using System;
using System.Linq;

namespace Satisfactory {
	class Program {
		static void Main(string[] args) {
			var rcpsByGen = Recipe.List.OrderBy(r => r.gen).ToArray();

			foreach (var rcp in rcpsByGen) {
				Console.WriteLine(rcp);
			}
		}
	}
}
