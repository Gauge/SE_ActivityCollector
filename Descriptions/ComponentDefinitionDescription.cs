using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Descriptions
{
    public class ComponentDefinitionDescription : ISQLQueryData
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

        public string GetQuery()
        {
            return
$@"IF NOT EXISTS (SELECT * FROM definition_components WHERE type_id = '{TypeId}' AND subtype_id = '{SubtypeId}' AND name = '{Name}' AND mass = {Mass} AND volume = {Volume} AND max_integrity = {MaxIntegrity} AND max_stack_amount = {MaxStackAmount} AND health = {Health} AND CONVERT(VARCHAR, description) = '{Description}')
BEGIN
INSERT INTO definition_components ([session_id], [type_id], [subtype_id], [name], [mass], [volume], [max_integrity], [max_stack_amount], [health], [description], [timestamp])
VALUES ({ActivityCollectorPlugin.CurrentSession}, '{TypeId}', '{SubtypeId}', '{Name}', {Mass}, {Volume}, {MaxIntegrity}, {MaxStackAmount}, {Health}, '{Description}', '{Helper.DateTimeFormated}')
END;";
        }
    }
}