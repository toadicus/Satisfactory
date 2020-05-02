using System;
using static Utils;
using static RecipeDefs;

public static class BuildingDefs {
	public static BldgPlan miner = BldgPlan.New("Miner", 5, (p1(portable_miner, 1), p1(iron_plate, 10), p1(concrete, 10)));
	public static BldgPlan miner2 = BldgPlan.New("Miner Mk 2", 5, (p1(portable_miner, 2), p1(encased_beam, 10), p1(steel_pipe, 20), p1(modular_frame, 10)), rateMultiplier:  2);

	public static BldgPlan smelter = BldgPlan.New("Smelter", 4, (p1(iron_rod, 5), p1(wire, 8)));
	public static BldgPlan foundry = BldgPlan.New("Foundry", 16, (p1(modular_frame, 10), p1(motor, 10), p1(concrete, 20)));

	public static BldgPlan constructor = BldgPlan.New("Constructor", 4, (p1(reinf_plate, 2), p1(cable, 8)));
	public static BldgPlan assembler = BldgPlan.New("Assembler", 15, (p1(reinf_plate, 8), p1(rotor, 4), p1(cable, 10)));
	public static BldgPlan manufacturer = BldgPlan.New("Manufacturer", 55, (p1(motor, 5), p1(heavy_modular_frame, 10), p1(cable, 50), p1(plastic, 50)));

	public static BldgPlan water_extractor = BldgPlan.New("Water Extractor", 20, (p1(copper_sheet, 20), p1(reinf_plate, 10), p1(rotor, 10)));
	public static BldgPlan oil_extractor = BldgPlan.New("Oil Extractor", 40, (p1(motor, 15), p1(encased_beam, 20), p1(cable, 60)));
	public static BldgPlan refinery = BldgPlan.New("Refinery", 30, (p1(motor, 10), p1(encased_beam, 10), p1(steel_pipe, 30), p1(copper_sheet, 20)));
}
