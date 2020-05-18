using System.Collections.Generic;
using static RecipeDefs;
using static Utils;

public static class BuildingDefs {
	public static BldgPlan miner = BldgPlan.New("Miner", 5, (p1(portable_miner, 1), p1(iron_plate, 10), p1(concrete, 10)));
	public static BldgPlan miner2 = BldgPlan.New("Miner Mk 2", 5, (p1(portable_miner, 2), p1(encased_beam, 10), p1(steel_pipe, 20), p1(modular_frame, 10)), rateMultiplier: 2);

	public static BldgPlan smelter = BldgPlan.New("Smelter", 4, (p1(iron_rod, 5), p1(wire, 8)));
	public static BldgPlan foundry = BldgPlan.New("Foundry", 16, (p1(modular_frame, 10), p1(motor, 10), p1(concrete, 20)));

	public static BldgPlan constructor = BldgPlan.New("Constructor", 4, (p1(reinf_plate, 2), p1(cable, 8)));
	public static BldgPlan assembler = BldgPlan.New("Assembler", 15, (p1(reinf_plate, 8), p1(rotor, 4), p1(cable, 10)));
	public static BldgPlan manufacturer = BldgPlan.New("Manufacturer", 55, (p1(motor, 5), p1(heavy_modular_frame, 10), p1(cable, 50), p1(plastic, 50)));

	public static BldgPlan water_extractor = BldgPlan.New("Water Extractor", 20, (p1(copper_sheet, 20), p1(reinf_plate, 10), p1(rotor, 10)));
	public static BldgPlan oil_extractor = BldgPlan.New("Oil Extractor", 40, (p1(motor, 15), p1(encased_beam, 20), p1(cable, 60)));
	public static BldgPlan refinery = BldgPlan.New("Refinery", 30, (p1(motor, 10), p1(encased_beam, 10), p1(steel_pipe, 30), p1(copper_sheet, 20)));

	public static GenrPlan fuel_generator = GenrPlan.New("Fuel Generator", 150, p1(fuel, 15), (p1(computer, 5), p1(heavy_modular_frame, 10), p1(motor, 15), p1(rubber, 50), p1(quickwire, 50)));

	static BuildingDefs() {
		miner.BuildList = new List<Recipe> {
			iron_ore,
			caterium_ore,
			copper_ore,
			limestone,
			coal,
			bauxite,
			raw_quartz,
			sulfur,
			uranium
		};

		miner2.BuildList = new List<Recipe>();

		foreach (Recipe rcp in miner.BuildList) {
			miner2.BuildList.AddRange(miner.BuildList);
		}

		smelter.BuildList = new List<Recipe> {
			iron_ingot,
			copper_ingot,
			caterium_ingot,
			pure_aluminum_ingot,
		};

		foundry.BuildList = new List<Recipe> {
			steel_ingot,
			aluminum_ingot,
		};

		constructor.BuildList = new List<Recipe> {
			iron_plate,
			iron_rod,
			wire,
			copper_sheet,
			cable,
			canister,
		//	biomass_leaves,
			concrete,
			screw,
		//	biomass_wood,
		//	power_shard_green,
		//	solid_biofuel,
			steel_beam,
		//	biomass_mycelia,
		//	color_cartridge,
		//	spiked_rebar,
		//	biomass_carapace,
		//	power_shard_yellow,
			quickwire,
		//	power_shard_purple,
			silica,
			quartz_crystal,
		};

		assembler.BuildList = new List<Recipe> {
			reinf_plate,
			rotor,
			modular_frame,
			encased_beam,
			stator,
			motor,
		//	fabric,
			circuit_board,
			ai_limiter,
			steel_pipe,
			automated_wiring,
			smart_plating,
			alclad_alum_sheet,
			heat_sink,
			encased_uranium_cell,
			electro_control_rod
		};

		manufacturer.BuildList = new List<Recipe> {
			heavy_modular_frame,
			computer,
		//	supercomputer,
		//	high_speed_connector,
		//	filter,
			crystal_oscillator,
		//	nobelisk,
		//	beacon,
			radio_control_unit,
			turbo_motor,
			nuclear_fuel_rod
		};

		oil_extractor.BuildList = new List<Recipe> {
			crude_oil
		};

		refinery.BuildList = new List<Recipe> {
			plastic,
			rubber,
			fuel,
			residual_fuel,
			residual_rubber,
			alumina_sln,
			aluminum_scrap,
			petroleum_coke,
			sulfuric_acid,
			uranium_pellet
		};

		water_extractor.BuildList = new List<Recipe> {
			water
		};
	}
}
