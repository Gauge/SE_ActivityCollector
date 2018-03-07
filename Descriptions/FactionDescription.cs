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
                return string.Format(@"
INSERT INTO factions ([faction_id], [iteration_id], [tag], [name], [description], [creation_date])
VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                FactionId, ActivityCollectorPlugin.CurrentIteration, Tag, Name, Description, CreationDate);
            }
            else if (State == FactionState.Edit)
            {
                return string.Format(@"
UPDATE factions
SET [tag] = '{0}', [name] = '{1}', [description] = '{2}'
WHERE [faction_id] = '{3}' AND [iteration_id] = '{4}'
", Tag, Name, Description, FactionId, ActivityCollectorPlugin.CurrentIteration);
            }
            else
            {
                return string.Format(@"
UPDATE factions
SET [termination_date] = '{0}'
WHERE [faction_id] = '{1}' AND [iteration_id] = '{2}'
", TerminationDate, FactionId, ActivityCollectorPlugin.CurrentIteration);
            }
        }
    }
}
