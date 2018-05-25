using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace ActivityCollectorPlugin
{
    public class Helper
    {
        public static ulong GetPlayerSteamId(IMyCharacter character)
        {
            IMyPlayer player = MyAPIGateway.Players.GetPlayerControllingEntity(character);

            if (player == null)
            {
                player = MyAPIGateway.Players.GetPlayerControllingEntity(character.Parent);
            }

            return player != null ? player.SteamUserId : ulong.MinValue;
        }

        public static long GetPlayerIdentityId(IMyCharacter character)
        {
            IMyPlayer player = MyAPIGateway.Players.GetPlayerControllingEntity(character);

            if (player == null)
            {
                player = MyAPIGateway.Players.GetPlayerControllingEntity(character.Parent);
            }

            return player != null ? player.IdentityId : 0;
        }

        public static IMyPlayer GetPlayer(IMyCharacter character)
        {
            IMyPlayer player = MyAPIGateway.Players.GetPlayerControllingEntity(character);

            if (player == null)
            {
                player = MyAPIGateway.Players.GetPlayerControllingEntity(character.Parent);
            }

            return player;
        }

        public static IMyCharacter[] GetGridPilots(IMyCubeGrid grid)
        {
            List<IMyCharacter> pilots = new List<IMyCharacter>();
            List<IMySlimBlock> controls = new List<IMySlimBlock>();
            grid.GetBlocks(controls, x => x.FatBlock is IMyShipController);

            foreach (IMySlimBlock control in controls)
            {
                if (((IMyShipController)control.FatBlock).Pilot != null)
                {
                    pilots.Add(((IMyShipController)control.FatBlock).Pilot);
                }
            }

            return pilots.ToArray();
        }

        public static IMyEntity RaycastEntity(Vector3D start, Vector3D end)
        {
            IHitInfo hit;
            if (!MyAPIGateway.Physics.CastRay(start, end, out hit))
                return null;

            if (!(hit.HitEntity is IMyCubeGrid))
                return null;

            var grid = (IMyCubeGrid)hit.HitEntity;
            Vector3I? hitPos = grid.RayCastBlocks(start, end);
            if (hitPos.HasValue)
            {
                IMySlimBlock block = grid.GetCubeBlock(hitPos.Value);
                return block?.FatBlock as IMyTerminalBlock;
            }

            return grid;
        }

        public static DateTime DateTime => DateTime.UtcNow;
        public static string DateTimeFormated => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

        public static string format(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        public static string getBlockId(int x, int y, int z)
        {
            return $"{x}|{y}|{z}";
        }

        public static string getBlockId(Vector3I pos)
        {
            return getBlockId(pos.X, pos.Y, pos.Z);
        }
    }
}
