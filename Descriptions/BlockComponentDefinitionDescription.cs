using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Descriptions
{
    public class BlockComponentDefinitionDescription : SQLQueryData
    {

        public string TypeId { get; set; }
        public string SubtypeId { get; set; }
        public string Name { get; set; }
        public float Mass { get; set; }
        public float Volume { get; set; }
        public float MaxIntegrity { get; set; }
        public float MaxStackAmount { get; set; }
        public float Health { get; set; }
        public string Description { get; set; }

        public override string GetQuery()
        {
            return
$@"INSERT INTO definition_components ([type_id], [subtype_id], [name], [mass], [volume], [max_integrity], [max_stack_amount], [health], [description], [timestamp])
VALUES ('{TypeId}', '{SubtypeId}', '{Name}', {Mass}, {Volume}, {MaxIntegrity}, {MaxStackAmount}, {Health}, '{Description}', '{Tools.DateTimeFormated}');";
        }
    }
}