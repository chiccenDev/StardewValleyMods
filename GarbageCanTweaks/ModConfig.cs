namespace GarbageCanTweaks
{
    public class ModConfig
    {

        public bool EnableMod { get; set; } = true;
        public string LootTable { get; set; } = "default";
        public bool EnableBirthday { get; set; } = true;
        public float BirthdayChance { get; set; } = 0.75f;
        public float LootChance { get; set; } = 1f;
        public bool Debug { get; set; } = false;

    }
}
