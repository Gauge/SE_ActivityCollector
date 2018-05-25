using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public class CombatDescription : ISQLQueryData
    {
        public int SessionId { get; set; }
        public long AttackerGridId { get; set; }
        public string AttackerGridBlockId { get; set; } 
        public long AttackerEntityId { get; set; }

        public long VictimGridId { get; set; }
        public string VictimGridBlockId { get; set; }
        public long VictimEntityId { get; set; }

        public string Type { get; set; }
        public float Damage { get; set; }
        public float Integrity { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetQuery()
        {
            return $@"
INSERT INTO [dbo].[combatlog] (
    [session_id], 
    [attacker_grid_id], [attacker_grid_block_id], [attacker_entity_id],
    [victim_grid_id], [victim_grid_block_id], [victim_entity_id],
    [type], [damage], [integrity], [timestamp]
)
VALUES ('{ActivityCollectorPlugin.CurrentSession}', '{AttackerGridId}', '{AttackerGridBlockId}', '{AttackerEntityId}', '{VictimGridId}', '{VictimGridBlockId}', '{VictimEntityId}', '{Type}', '{Damage}', '{Integrity}', '{Helper.format(Timestamp)}');";
        }
    }
}
