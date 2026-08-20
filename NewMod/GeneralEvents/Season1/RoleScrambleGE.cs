using System.Linq;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1
{
    public class RoleScrambleGE : IGeneralEvent
    {
        public string Title => "Role Scramble";
        public string Description => " ROLE ASSIGNMENTS HAVE BEEN CORRUPTED!";
        public LoadableAsset<Sprite> Icon => NewModAsset.RoleScrambleIcon;
        public Color AccentColor => new(0.95f, 0.62f, 0.12f);
        public int OccurrenceChance => 2; // 2%
        public float Duration => 5f;

        public bool CanOccur()
        {
            return Helpers.GetAlivePlayers().Count >= 3;
        }

        public void OnEventStart()
        {
            if (!AmongUsClient.Instance.AmHost)
                return;

            var players = Helpers.GetAlivePlayers().ToArray();

            if (players.Length < 3)
                return;

            var roles = players.Select(player => player.Data.Role.Role).ToArray();

            for (int i = roles.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (roles[i], roles[j]) = (roles[j], roles[i]);
            }

            if (players.Select((player, index) => player.Data.Role.Role == roles[index]).All(same => same))
            {
                var first = roles[0];
                for (int i = 0; i < roles.Length - 1; i++)
                    roles[i] = roles[i + 1];
                roles[^1] = first;
            }

            for (int i = 0; i < players.Length; i++)
                players[i].RpcSetRole(roles[i], true);
        }

        public void OnEventEnd()
        {
        }
    }
}