﻿using static Utils;
#pragma warning disable 0618
public static class RecipeDefs {
	public static Recipe iron_ore = Recipe.New("Iron Ore", 60, plural: "");
	public static Recipe limestone = Recipe.New("Limestone", 60, plural: "");
	public static Recipe copper_ore = Recipe.New("Copper Ore", 60, plural: "");
	public static Recipe coal = Recipe.New("Coal", 60, plural: "");
	public static Recipe caterium_ore = Recipe.New("Caterium Ore", 60, plural: "");
	public static Recipe raw_quartz = Recipe.New("Raw Quartz", 60, plural: "");
	public static Recipe bauxite = Recipe.New("Bauxite", 60, plural: "");

	public static Recipe concrete = Recipe.New("Concrete", 15, p(limestone, 45), plural: "");
	public static Recipe iron_ingot = Recipe.New("Iron Ingot", 30, p(iron_ore, 30));
	public static Recipe copper_ingot = Recipe.New("Copper Ingot", 30, p(copper_ore, 30));
	public static Recipe steel_ingot = Recipe.New("Steel Ingot", 45, (p(iron_ore, 45), p(coal, 45)));
	public static Recipe caterium_ingot = Recipe.New("Caterium Ingot", 15, p(caterium_ore, 45));

	public static Recipe iron_rod = Recipe.New("Iron Rod", 15, p(iron_ingot, 15));
	public static Recipe iron_plate = Recipe.New("Iron Plate", 20, p(iron_ingot, 30));

	public static Recipe screw = Recipe.New("Screw", 40, p(iron_rod, 10));

	public static Recipe reinf_plate = Recipe.New("Reinforced Iron Plate", 5, (p(iron_plate, 30), p(screw, 60)));
	public static Recipe rotor = Recipe.New("Rotor", 4, (p(iron_rod, 20), p(screw, 100)));

	public static Recipe modular_frame = Recipe.New("Modular Frame", 2, (p(reinf_plate, 3), p(iron_rod, 12)));

	public static Recipe wire = Recipe.New("Wire", 30, p(copper_ingot, 15));
	public static Recipe copper_sheet = Recipe.New("Copper Sheet", 10, p(copper_ingot, 20));
	public static Recipe cable = Recipe.New("Cable", 30, p(wire, 60));

	public static Recipe quickwire = Recipe.New("Quickwire", 60, p(caterium_ingot, 12));

	public static Recipe steel_pipe = Recipe.New("Steel Pipe", 20, p(steel_ingot, 30));
	public static Recipe steel_beam = Recipe.New("Steel Beam", 15, p(steel_ingot, 60));

	public static Recipe stator = Recipe.New("Stator", 5, (p(steel_pipe, 15), p(wire, 40)));
	public static Recipe encased_beam = Recipe.New("Encased Industrial Beam", 6, (p(steel_beam, 24), p(concrete, 30)));

	public static Recipe motor = Recipe.New("Motor", 5, (p(rotor, 10), p(stator, 10)));
	public static Recipe heavy_modular_frame = Recipe.New("Heavy Modular Frame", 2, (p(modular_frame, 10), p(steel_pipe, 30), p(encased_beam, 10), p(screw, 200)));

	public static Recipe vers_framework = Recipe.New("Versatile Framework", 5, (p(modular_frame, 2.5), p(steel_beam, 30)));

	public static Recipe crude_oil = Recipe.New("Crude Oil", 120, plural: "");
	public static Recipe water = Recipe.New("Water", 120, plural: "");
	// public static Recipe heavy_residue = Recipe.New("Heavy Oil Residue", 1, plural: "");
	public static Recipe polymer_resin = Recipe.New("Polymer Resin", 1, plural: "");

	public static Recipe plastic = Recipe.New((p("Plastic", 20), p("Heavy Oil Residue", 10)), p(crude_oil, 30), plural: "");
	public static Recipe rubber = Recipe.New((p("Rubber", 20), p("Heavy Oil Residue", 20)), p(crude_oil, 30), plural: "");
	public static Recipe residual_rubber = Recipe.New("Residual Rubber", p(rubber, 20), (p(polymer_resin, 40), p(water, 40)), plural: "");

	public static Recipe canister = Recipe.New("Empty Canister", 60, p(plastic, 30));

	public static Recipe fuel = Recipe.New((p("Fuel", 40), p(polymer_resin, 30)), p(crude_oil, 60), plural: "");
	public static Recipe residual_fuel = Recipe.New("Residual Fuel", 40, p("Heavy Oil Residue", 60), plural: "");
	public static Recipe packaged_fuel = Recipe.New("Packaged Fuel", 40, (p(fuel, 40), p(canister, 40)), plural: "");
	public static Recipe petroleum_coke = Recipe.New("Petroleum Coke", 120, p("Heavy Oil Residue", 40), plural: "");

	public static Recipe circuit_board = Recipe.New("Circuit Board", 7.5, (p(copper_sheet, 15), p(plastic, 30)));
	public static Recipe computer = Recipe.New("Computer", 2.5, (p(circuit_board, 25), p(cable, 22.5), p(plastic, 45), p(screw, 130)));

	public static Recipe smart_plating = Recipe.New("Smart Plating", 2, (p(reinf_plate, 2), p(rotor, 2)));
	public static Recipe plastic_plating = Recipe.New("Plastic Smart Plating", 5, (p(reinf_plate, 2.5), p(rotor, 2.5), p(plastic, 7.5)));
	public static Recipe automated_wiring = Recipe.New("Automated Wiring", 2.5, (p(stator, 2.5), p(cable, 50)));
	public static Recipe adaptive_control_unit = Recipe.New("Adaptive Control Unit", 1, (p(automated_wiring, 7.5), p(circuit_board, 5), p(heavy_modular_frame, 1), p(computer, 1)));

	public static Recipe modular_engine = Recipe.New("Modular Engine", 1, (p(motor, 2), p(rubber, 15), p(smart_plating, 2)));

	public static Recipe portable_miner = Recipe.New("Portable Miner", 1, (p(iron_plate, 3), p(iron_rod, 3)));

	public static Recipe quartz_crystal = Recipe.New("Quartz Crystal", 22.5, p(raw_quartz, 37.5));
	public static Recipe silica = Recipe.New("Silica", 37.5, p(raw_quartz, 22.5), plural: "");
	public static Recipe crystal_oscillator = Recipe.New("Crystal Oscillator", 2, (p(quartz_crystal, 36/2), p(cable, 28/2), p(reinf_plate, 5/2)));
	public static Recipe ai_limiter = Recipe.New("A.I. Limiter", 5, (p(copper_sheet, 25), p(quickwire, 100)));

	public static Recipe alumina_sln = Recipe.New((p("Alumina Solution", 80), p(silica, 20)), (p(bauxite, 70), p(water, 100)), plural: "");
	public static Recipe aluminum_scrap = Recipe.New((p("Aluminum Scrap", 360), p(water, 60)), (p(alumina_sln, 240), p(petroleum_coke, 60)), plural: "");
	public static Recipe aluminum_ingot = Recipe.New("Aluminum Ingot", 80, (p(aluminum_scrap, 240), p(silica, 140)));
	public static Recipe pure_aluminum_ingot = Recipe.New("Pure Aluminum Ingot", p(aluminum_ingot, 36), p(aluminum_scrap, 144));

	public static Recipe alclad_alum_sheet = Recipe.New("Alclad Aluminum Sheet", 30, (p("Aluminum Ingot", 60), p(copper_ingot, 22.5)));

	public static Recipe heat_sink = Recipe.New("Heat Sink", 10, (p(alclad_alum_sheet, 8*10/2), p(rubber, 14*10/2)));
	public static Recipe radio_control_unit = Recipe.New("Radio Control Unit", 2.5, (p(heat_sink, 4*2.5), p(rubber, 16*2.5), p(crystal_oscillator, 2.5), p(computer, 2.5)));

	public static Recipe turbo_motor = Recipe.New("Turbo Motor", 1.875, (p(heat_sink, 4*1.875), p(radio_control_unit, 2*1.875), p(motor, 4*1.875), p(rubber, 24*1.875)));

	public static Recipe uranium = Recipe.New("Uranium", 120, plural: "");
	public static Recipe sulfur = Recipe.New("Sulfur", 120, plural: "");

	public static Recipe sulfuric_acid = Recipe.New("Sulfuric Acid", 100, (p(sulfur, 50), p(water, 50)), plural: "");
	public static Recipe uranium_pellet = Recipe.New("Uranium Pellet", (p("Uranium Pellet", 50), p("Sulfuric Acid", 20)), (p(uranium, 50), p(sulfuric_acid, 80)));

	public static Recipe encased_uranium_cell = Recipe.New("Encased Uranium Cell", 10, (p(uranium_pellet, 40), p(concrete, 9)));
	public static Recipe electro_control_rod = Recipe.New("Electromagnetic COntrol Rod", 4, (p(stator, 6), p(ai_limiter, 4)));

	public static Recipe nuclear_fuel_rod = Recipe.New("Nuclear Fuel Rod", 0.4, (p(encased_uranium_cell, 25*.4), p(encased_beam, 3*.4), p(electro_control_rod, 5*.4)));
}
#pragma warning restore 0618
