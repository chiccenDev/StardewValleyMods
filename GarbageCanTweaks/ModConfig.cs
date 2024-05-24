namespace GarbageCanTweaks
{
    public class ModConfig
    {

        public bool EnableMod { get; set; } = true;
        public bool EnableBirthday { get; set; } = true;
        public float BirthdayChance { get; set; } = 0.75f;
        public float LootChance { get; set; } = 0.2f;
        public bool Debug { get; set; } = false;

    }
}
