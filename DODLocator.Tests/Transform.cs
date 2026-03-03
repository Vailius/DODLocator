using System.Numerics;

namespace DODLocator.Tests
{
    public struct Transform
    {
        public Vector3 Position;
        public Vector4 Rotation;
        public Vector3 Scale;

        internal static void StartProcess(StructArray<Transform> soa, StructArray<Transform>.RawDataHandler handler)
        {
            soa.ProcessRawData(handler);
        }

        internal static void StartProcessPositions(StructArray<Transform> soa, StructArray<Transform>.DataHandler<Vector3> handler)
        {
            soa.ProcessData(handler, StructFieldsAnalyzer<Transform>.Identifier["Position"]);
        }

        internal static void StartProcessRotations(StructArray<Transform> soa, StructArray<Transform>.DataHandler<Vector4> handler)
        {
            soa.ProcessData(handler, StructFieldsAnalyzer<Transform>.Identifier["Rotation"]);
        }

        internal static void StartProcessScales(StructArray<Transform> soa, StructArray<Transform>.DataHandler<Vector3> handler)
        {
            soa.ProcessData(handler, StructFieldsAnalyzer<Transform>.Identifier["Scale"]);
        }
    }
}