using System;

namespace ActivityCollectorPlugin.Descriptions
{
    public enum FactionState { Create, Edit, Remove }

    public class FactionNameDescription : SQLQueryData
    {
        public long FactionId { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        public override string GetQuery()
        {
            return $@"INSERT INTO faction_names ([faction_id], [tag], [name], [description], [timestamp])
VALUES ('{FactionId}', '{Tag}', '{Name}', '{Description}', '{Tools.format(Timestamp)}')";
        }
    }
}
