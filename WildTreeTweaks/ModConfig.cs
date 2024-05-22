namespace WildTreeTweaks
{
    public class ModConfig
    {

        public bool EnableMod { get; set; } = true; // enable the mod
        public bool OnlyOnFarm { get; set; } = false; // only overwrite farm trees
        public bool GrowInWinter { get; set; } = true; // just like FTT, grow in winter DONE?
        public bool MossFromMature { get; set; } = true; // whether they can grow moss from maturity (growthStage 4) or default (growthStage 14)
        public float Health { get; set; } = 10f; // starting health DONE?
        public float GrowthChance { get; set; } = 0.2f; // DONE?
        public float SeedChance { get; set; } = 0.05f; // chance for daily seed from shake DONE?
        public float SeedSpreadChance { get; set; } = 0.15f; // DONE?
        public bool Debug { get; set; } = false;

    }
}
