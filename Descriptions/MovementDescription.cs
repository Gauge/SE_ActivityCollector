using VRageMath;

namespace ActivityCollectorPlugin.Descriptions
{
    public class MovementDescription : SQLQueryData
    {
        public long EntityId { get; set; }
        public Vector3D Position { get; set; }
        public Vector3 LinearVelocity { get; set; }
        public Vector3 LinearAcceleration { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public Vector3 AngularAcceleration { get; set; }
        public Vector3 LinearRateOfChange { get; set; }
        public Vector3 AngularRateOfChange { get; set; }
        public Vector3 ForwardVector { get; set; }


        public override string GetQuery()
        {
            return $@"INSERT INTO entities_movment ([entity_id], [position], [linear_velocity], [linear_acceleration], [angular_velocity], [angular_acceleration], [forward_vector], [timestamp])
VALUES ('{EntityId}', '{Tools.format(Position)}', '{Tools.format(LinearVelocity)}', '{Tools.format(LinearAcceleration)}', '{Tools.format(AngularVelocity)}', '{Tools.format(AngularAcceleration)}', '{Tools.format(ForwardVector)}', '{Tools.DateTimeFormated}')";
        }
    }
}
