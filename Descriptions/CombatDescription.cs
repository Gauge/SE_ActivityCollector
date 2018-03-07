using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class CombatDescription : ISQLQueryData
    {
        public int SessionId { get; set; }
        public long AttackerGridId { get; set; }
        public string AttackerGridName { get; set; }
        public long AttackerGridOwner { get; set; }
        public long AttackerEntityId { get; set; }
        public string AttackerEntityType { get; set; }
        public string AttackerEntitySubtype { get; set; }
        public string AttackerEntityObjectType { get; set; }
        public string AttackerEntityName { get; set; }
        public long AttackerEntityOwner { get; set; }

        public long VictimGridId { get; set; }
        public string VictimGridName { get; set; }
        public long VictimGridOwner { get; set; }
        public long VictimEntityId { get; set; }
        public string VictimEntityType { get; set; }
        public string VictimEntitySubtype { get; set; }
        public string VictimEntityObjectType { get; set; }
        public string VictimEntityName { get; set; }
        public long VictimEntityOwner { get; set; }

        public string DamageType { get; set; }
        public float Damage { get; set; }
        public bool TargetEntityFunctional { get; set; }
        public bool TargetEntityDestroyed { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            return string.Format(@"
        	INSERT INTO [dbo].[combatlog] (
                [session_id], 
                [attacker_grid_id], [attacker_grid_name], [attacker_grid_owner], 
                [attacker_entity_id], [attacker_entity_type], [attacker_entity_subtype], [attacker_entity_object_type], [attacker_entity_name], [attacker_entity_owner],
                [victim_grid_id], [victim_grid_name], [victim_grid_owner], 
                [victim_entity_id], [victim_entity_type], [victim_entity_subtype], [victim_entity_object_type], [victim_entity_name], [victim_entity_owner], 
                [damage_type], [damage], [target_entity_functional], [target_entity_destroyed], [timestamp]
            )
	        VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', {20}, '{21}', '{22}', '{23}');", 
            SessionId, 
            AttackerGridId, AttackerGridName, AttackerGridOwner, 
            AttackerEntityId, AttackerEntityType, AttackerEntitySubtype, AttackerEntityObjectType, AttackerEntityName, AttackerEntityOwner, 
            VictimGridId, VictimGridName, VictimGridOwner, 
            VictimEntityId, VictimEntityType, VictimEntitySubtype, VictimEntityObjectType, VictimEntityName, VictimEntityOwner,
            DamageType, Damage, TargetEntityFunctional, TargetEntityDestroyed, Timestamp);
        }
    }
}
