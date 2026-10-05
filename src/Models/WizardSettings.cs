namespace CadLayerTools
{
    public class SourceSettings
    {
        public bool Saved;
        public object LinkId, CadId;
    }
    public class CreationSettings
    {
        public object TypeId, SystemId, LevelId, LayerId, MarkerId, WorksetId;
        public string Length="1", Endpoint="0.1", Minimum="100", Comments="";
        public string Offset;
    }
}
