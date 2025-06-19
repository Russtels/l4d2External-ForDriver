// GameConstants.cs (Actualizado)
namespace left4dead2Menu
{
    // --- ENUM PARA DEFINIR LA ZONA DE APUNTADO ---
    // Lo movemos aquí para que sea accesible globalmente en el namespace.
    public enum AimbotTarget
    {
        Head,
        Chest
    }

    internal static class GameConstants
    {
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        public const int SURVIVOR_TEAM = 2;
        public const int INFECTED_TEAM = 3;

        public const int LIFE_STATE_ALIVE = 2;
    }
}