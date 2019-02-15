using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace ActivityCollectorPlugin
{
    public class Tools
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

        //public static string getBlockId(int x, int y, int z)
        //{
        //    return $"{x}|{y}|{z}";
        //}

        public static string GetBlockId(Vector3I pos)
        {
            return format(pos);
        }

        public static string PrepString(string data)
        {
            return data.Replace("'", "''");
        }

        //public static float Round(float value)
        //{
        //    return (float)Math.Round(value, 3);
        //}

        public static double Round(double value)
        {
            return Math.Round(value, 4);
        }

        public static Vector3 Round(Vector3 v)
        {
            return new Vector3(Round(v.X), Round(v.Y), Round(v.Z));
        }

        public static Vector3D Round(Vector3D v)
        {
            return new Vector3D(Round(v.X), Round(v.Y), Round(v.Z));
        }

        public static string format(Vector3 v)
        {
            return format(v.X, v.Y, v.Z);
        }

        //public static string formatNoRounding(Vector3 v)
        //{
        //    return $"{v.X}:{v.Y}:{v.Z}";
        //}

        public static string format(Vector3D v)
        {
            return format(v.X, v.Y, v.Z);
        }

        public static string format(Vector3I v)
        {
            return format(v.X, v.Y, v.Z);//$"{v.X}:{v.Y}:{v.Z}";
        }

        public static string format(double x, double y, double z)
        {
            return $"{Round(x)}:{Round(y)}:{Round(z)}"; //$"{x}:{y}:{z}"; 
        }

    }
}
