using ActivityCollectorPlugin.Descriptions;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace ActivityCollectorPlugin.Managers
{
    public class MovementManager : IManager
    {
        public bool IsInitialized => true;

        private int interval = 0;

        private List<MyEntity> entities = new List<MyEntity>();
        private Dictionary<long, MovementDescription> entitiesMovement = new Dictionary<long, MovementDescription>();

        public void AddEntity(IMyEntity ent)
        {
            entities.Add((MyEntity)ent);
            entitiesMovement.Add(ent.EntityId, null);
        }

        public void RemoveEntity(IMyEntity ent)
        {
            entities.Remove((MyEntity)ent);
            entitiesMovement.Remove(ent.EntityId);
        }

        public void Run()
        {
            if (interval == ActivityCollector.Config.Data.EntityMovementInterval)
            {
                interval = 0;

                foreach (MyEntity ent in entities)
                {
                    try
                    {
                        MovementDescription last = entitiesMovement[ent.EntityId];
                        bool writeToDB = false;

                        if (ent.Physics != null)
                        {
                            MovementDescription current = new MovementDescription()
                            {
                                EntityId = ent.EntityId,
                                Position = ent.PositionComp.GetPosition(),
                                LinearAcceleration = Tools.Round(ent.Physics.LinearAcceleration),
                                LinearVelocity = ent.Physics.LinearVelocity,
                                AngularAcceleration = ent.Physics.AngularAcceleration,
                                AngularVelocity = ent.Physics.AngularVelocity,
                                ForwardVector = ent.PositionComp.WorldMatrix.Forward
                            };

                            if (last == null)
                            {
                                writeToDB = true;
                            }
                            else
                            {
                                current.LinearRateOfChange = current.LinearAcceleration - last.LinearAcceleration;

                                if (Tools.Round(current.LinearRateOfChange) != Vector3.Zero  /*current.LinearAcceleration != last.LinearAcceleration || current.AngularAcceleration != last.AngularAcceleration */)
                                {
                                    writeToDB = true;
                                }
                            }

                            if (writeToDB)
                            {
                                SQLQueryData.WriteToDatabase(current);
                                entitiesMovement[ent.EntityId] = current;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ActivityCollector.Log.Error(e.ToString());
                    }
                }
            }

            interval++;
        }
    }
}
