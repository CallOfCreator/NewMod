namespace NewMod
{
    /// <summary>
    /// Represents the different types of missions available to players.
    /// </summary>
    public enum MissionType
    {
        /// <summary>
        /// Mission to kill the most wanted target.
        /// </summary>
        KillMostWanted,

        /// <summary>
        /// Mission to drain a player's energy using Energy Thief abilities.
        /// </summary>
        DrainEnergy,

        /// <summary>
        /// Mission to disguise as a random player and create fake dead bodies using Prankster abilities.
        /// </summary>
        CreateFakeBodies,

        /// <summary>
        /// Mission to revive a dead player using Necromancer powers and kill them again.
        /// </summary>
        ReviveAndKill
    }
}
