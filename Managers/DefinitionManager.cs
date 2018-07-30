using Sandbox.Definitions;
using ActivityCollectorPlugin.Descriptions;
using Sandbox.ModAPI;

namespace ActivityCollectorPlugin.Managers
{
    class DefinitionManager : IManager
    {
        public bool IsInitialized { get; set; } = false;

        public void Run()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                MyAPIGateway.Session.OnSessionReady += SessionReady;
            }
        }

        private void SessionReady()
        {
            MyAPIGateway.Session.OnSessionReady -= SessionReady;

            Analytics.Start("ComponentDefinitions");
            foreach (MyComponentDefinition c in MyDefinitionManager.Static.GetDefinitionsOfType<MyComponentDefinition>())
            {
                SQLQueryData.WriteToDatabase(new BlockComponentDefinitionDescription()
                {
                    TypeId = c.Id.TypeId.ToString(),
                    SubtypeId = c.Id.SubtypeId.ToString(),
                    Name = c.DisplayNameText,
                    Mass = c.Mass,
                    Volume = c.Volume,
                    MaxIntegrity = c.MaxIntegrity,
                    MaxStackAmount = (float)c.MaxStackAmount,
                    Health = c.Health,
                    Description = c.DescriptionText,
                });
            }
            Analytics.Stop("ComponentDefinitions");

            Analytics.Start("CubeBlockDefinitions");
            foreach (MyCubeBlockDefinition cube in MyDefinitionManager.Static.GetDefinitionsOfType<MyCubeBlockDefinition>())
            {
                SQLQueryData.WriteToDatabase(new BlockDefinitionDescription()
                {
                    TypeId = cube.Id.TypeId.ToString(),
                    SubTypeId = cube.Id.SubtypeId.ToString(),
                    Name = cube.DisplayNameText,
                    CubeSize = cube.CubeSize.ToString(),
                    MaxIntegrity = cube.MaxIntegrity,
                    CriticalIntegrityRatio = cube.CriticalIntegrityRatio,
                    GeneralDamageMultiplier = cube.GeneralDamageMultiplier,
                    DisassembleRatio = cube.DisassembleRatio,
                    DeformationRatio = cube.DeformationRatio,
                    UsesDeformation = cube.UsesDeformation,
                    Mass = cube.Mass,
                    PCU = cube.PCU,
                    IsAirTight = cube.IsAirTight,
                    SizeX = cube.Size.X,
                    SizeY = cube.Size.Y,
                    SizeZ = cube.Size.Z,
                    ModelOffestX = cube.ModelOffset.X,
                    ModelOffestY = cube.ModelOffset.Y,
                    ModelOffestZ = cube.ModelOffset.Z,
                    Components = cube.Components,
                    Description = cube.DescriptionText
                });
            }
            Analytics.Stop("CubeBlockDefinitions");
        }
    }
}
