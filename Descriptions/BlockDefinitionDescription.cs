using Sandbox.Definitions;
using System.Text;

namespace ActivityCollectorPlugin.Descriptions
{
    public class BlockDefinitionDescription : SQLQueryData
    {

        public string TypeId { get; set; }
        public string SubTypeId { get; set; }
        public string Name { get; set; }
        public string CubeSize { get; set; }
        public float MaxIntegrity { get; set; }
        public float CriticalIntegrityRatio { get; set; }
        public float GeneralDamageMultiplier { get; set; }
        public float DisassembleRatio { get; set; }
        public float DeformationRatio { get; set; }
        public bool UsesDeformation { get; set; }
        public float Mass { get; set; }
        public int PCU { get; set; }
        public bool IsAirTight { get; set; }
        public float SizeX { get; set; }
        public float SizeY { get; set; }
        public float SizeZ { get; set; }
        public float ModelOffestX { get; set; }
        public float ModelOffestY { get; set; }
        public float ModelOffestZ { get; set; }
        public MyCubeBlockDefinition.Component[] Components { get; set; } = new MyCubeBlockDefinition.Component[0];
        public string Description { get; set; }

        public override string GetQuery()
        {
            StringBuilder final = new StringBuilder();
            //StringBuilder condition = new StringBuilder();
            //StringBuilder values = new StringBuilder();

            final.Append($@"INSERT INTO definition_blocks ([type_id], [subtype_id], [cube_size], [name], [max_integrity], [critical_integrity_ratio], [general_damage_multiplier], [disassemble_ratio], [deformation_ratio], [uses_deformation], [mass], [pcu], [is_air_tight], [size_x], [size_y], [size_z], [model_offset_x], [model_offset_y], [model_offset_z], [description], [timestamp])
VALUES ('{TypeId}', '{SubTypeId}', '{CubeSize}', '{Name}', {MaxIntegrity}, {CriticalIntegrityRatio}, {GeneralDamageMultiplier}, {DisassembleRatio}, {DeformationRatio}, '{UsesDeformation}', {Mass}, {PCU}, '{IsAirTight}', {SizeX}, {SizeY}, {SizeZ}, {ModelOffestX}, {ModelOffestY}, {ModelOffestZ}, '{Description}', '{Tools.DateTimeFormated}');");

            final.Append($@"
INSERT INTO definition_block_components ([block_id], [component_id], [count], [index], [timestamp])
VALUES ");
            int index = 0;
            foreach (var component in Components)
            {
                final.Append($@"((SELECT TOP 1 id FROM definition_blocks WHERE [type_id] = '{TypeId}' AND [subtype_id] = '{SubTypeId}' order by [timestamp] desc), (SELECT TOP 1 id FROM definition_components WHERE [type_id] = '{component.Definition.Id.TypeId.ToString()}' AND [subtype_id] = '{component.Definition.Id.SubtypeId.ToString()}' order by [timestamp] desc), {component.Count}, {index}, '{Tools.DateTimeFormated}'){(index < Components.Length - 1 ? "," : ";")}");
                index++;
            }

            return final.ToString();

//            final.Append($@"IF NOT EXISTS (SELECT * FROM definition_blocks WHERE [type_id] = '{TypeId}' AND [subtype_id] = '{SubTypeId}' AND name = '{Name}' AND cube_size = '{CubeSize}' AND max_integrity = {MaxIntegrity} AND critical_integrity_ratio = {CriticalIntegrityRatio} AND general_damage_multiplier = {GeneralDamageMultiplier} AND deformation_ratio = {DeformationRatio} AND mass = {Mass} AND is_air_tight = '{IsAirTight}' AND size_x = {SizeX} AND size_y = {SizeY} AND size_z = {SizeZ} AND model_offset_x = {ModelOffestX} AND model_offset_y = {ModelOffestY} AND model_offset_z = {ModelOffestZ} AND CONVERT(VARCHAR, description) = '{Description}')
//BEGIN
//INSERT INTO definition_blocks ([type_id], [subtype_id], [cube_size], [name], [max_integrity], [critical_integrity_ratio], [general_damage_multiplier], [deformation_ratio], [mass], [is_air_tight], [size_x], [size_y], [size_z], [model_offset_x], [model_offset_y], [model_offset_z], [description], [timestamp])
//VALUES ('{TypeId}', '{SubTypeId}', '{CubeSize}', '{Name}', {MaxIntegrity}, {CriticalIntegrityRatio}, {GeneralDamageMultiplier}, {DeformationRatio}, {Mass}, '{IsAirTight}', {SizeX}, {SizeY}, {SizeZ}, {ModelOffestX}, {ModelOffestY}, {ModelOffestZ}, '{Description}', '{Tools.DateTimeFormated}')
//END;");

//            condition.Append($"IF (SELECT COUNT(*) FROM definition_blocks WHERE [type_id] = '{TypeId}' AND [subtype_id] = '{SubTypeId}' AND [session_id] = {ActivityCollector.CurrentSession} group by [type_id]) = 1");
//            int index = 0;
//            foreach (var component in Components)
//            {
//                condition.Append($" OR (SELECT COUNT(*) FROM definition_components WHERE [type_id] = '{component.Definition.Id.TypeId.ToString()}' AND [subtype_id] = '{component.Definition.Id.SubtypeId.ToString()}' AND [session_id] = {ActivityCollector.CurrentSession} group by [type_id]) = 1");
//                values.AppendLine($"({ActivityCollector.CurrentSession}, @block_id, (SELECT TOP 1 id FROM definition_components WHERE [type_id] = '{component.Definition.Id.TypeId.ToString()}' AND [subtype_id] = '{component.Definition.Id.SubtypeId.ToString()}' order by [timestamp] desc), {component.Count}, {index}, '{Tools.DateTimeFormated}'){(index < Components.Length - 1 ? "," : "")}");
//                index++;
//            }
//            final.Append($@"{condition}
//BEGIN
//DECLARE @block_id INT;
//SET @block_id = (SELECT TOP 1 id FROM definition_blocks WHERE [type_id] = '{TypeId}' AND [subtype_id] = '{SubTypeId}' AND [session_id] = {ActivityCollector.CurrentSession});	
//INSERT INTO definition_block_components ([session_id], [block_id], [component_id], [count], [index], [timestamp])
//VALUES {values}
//END;");


            //return final.ToString();
        }
    }
}
