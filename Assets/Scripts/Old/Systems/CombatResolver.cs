namespace GGJ2026
{
    public static class CombatResolver
    {
        public static int Strength(FactionColor c) => c switch
        {
            FactionColor.Blue => 5,
            FactionColor.Red => 3,
            FactionColor.White => 1,
            
            FactionColor.Green => 0, // 中立：不杀不被杀
            _ => 0
        };
    }

}