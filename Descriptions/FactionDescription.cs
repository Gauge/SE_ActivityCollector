using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum FactionState { Create, Edit, Remove }

    public class FactionDescription : ISQLQueryData
    {
        public long FactionId { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public FactionState State { get; set; }

        public string GetQuery()
        {
            if (State == FactionState.Create)
            {
                return $@"
INSERT INTO factions ([faction_id], [iteration_id], [tag], [name], [description], [creation_date])
    VALUES ('{FactionId}', '{ActivityCollectorPlugin.CurrentIteration}', '{Tag}', '{Helper.prepString(Name)}', '{Helper.prepString(Description)}', '{Helper.format(CreationDate)}');";
            }
            else if (State == FactionState.Edit)
            {
                return $@"
UPDATE factions
SET [tag] = '{Tag}', [name] = '{Name}', [description] = '{Description}'
WHERE [faction_id] = '{FactionId}' AND [iteration_id] = '{ActivityCollectorPlugin.CurrentIteration}';";
            }
            else
            {
                return $@"
UPDATE factions
SET [termination_date] = '{Helper.format(TerminationDate)}'
WHERE [faction_id] = '{FactionId}' AND [iteration_id] = '{ActivityCollectorPlugin.CurrentIteration}';";
            }
        }
    }
}
