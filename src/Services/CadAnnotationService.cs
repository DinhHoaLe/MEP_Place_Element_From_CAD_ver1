using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
namespace CadLayerTools
{
    internal sealed class CadAnnotationData
    {
        public string SourceName {get;set;}public string SourcePath {get;set;}public string SourceHash {get;set;}
        public List<CadAnnotationBlock> Blocks {get;set;}
    }
    internal sealed class CadAnnotationBlock
    {
        public string Layer {get;set;}public double[] Origin {get;set;}public List<CadAnnotationText> Texts {get;set;}
    }
    internal sealed class CadAnnotationText
    {
        public string Value {get;set;}public double[] Anchor {get;set;}public double Height {get;set;}public bool Centered {get;set;}
    }
    internal sealed class CadAnnotationService
    {
        readonly CadAnnotationData data;
        internal CadAnnotationService(CadAnnotationData data){this.data=data;}
        public static CadAnnotationService Load(Document doc,ImportInstance cad)
        {
            try
            {
                var name=doc.GetElement(cad.GetTypeId())?.Name??cad.Name;
                var directory=new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                for(int i=0;i<4&&directory!=null;i++)directory=directory.Parent;
                if(directory==null)return null;
                var cache=Path.Combine(directory.FullName,"work","cad-annotations");if(!Directory.Exists(cache))return null;
                foreach(var file in Directory.GetFiles(cache,"*.json"))
                {
                    var annotation=JsonConvert.DeserializeObject<CadAnnotationData>(File.ReadAllText(file));
                    if(annotation?.Blocks==null||string.IsNullOrWhiteSpace(annotation.SourceName)||name.IndexOf(annotation.SourceName,StringComparison.OrdinalIgnoreCase)<0)continue;
                    if(string.IsNullOrWhiteSpace(annotation.SourcePath)||!File.Exists(annotation.SourcePath))continue;
                    using(var sha=SHA256.Create())using(var stream=File.OpenRead(annotation.SourcePath))
                        if(!string.Equals(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-",""),annotation.SourceHash,StringComparison.OrdinalIgnoreCase))continue;
                    // For linked CAD, also require the exact source path; imported CAD has no external reference.
                    try
                    {
                        var reference=ExternalFileUtils.GetExternalFileReference(doc,cad.GetTypeId());
                        if(reference!=null)
                        {
                            var source=ModelPathUtils.ConvertModelPathToUserVisiblePath(reference.GetAbsolutePath());
                            if(!string.Equals(Path.GetFullPath(source),Path.GetFullPath(annotation.SourcePath),StringComparison.OrdinalIgnoreCase))continue;
                        }
                    }
                    catch(Autodesk.Revit.Exceptions.ArgumentException) { }
                    return new CadAnnotationService(annotation);
                }
            }
            catch(IOException) { }
            catch(UnauthorizedAccessException) { }
            catch(JsonException) { }
            return null;
        }
        public void Apply(EquipmentPoint point,Transform cadToHost,string layer)
        {
            if(cadToHost==null)return;
            var matches=data.Blocks.Where(b=>string.Equals(b.Layer,layer,StringComparison.OrdinalIgnoreCase)&&b.Origin?.Length==3)
                .Where(b=>{var p=cadToHost.OfPoint(new XYZ(b.Origin[0]/304.8,b.Origin[1]/304.8,b.Origin[2]/304.8));return Math.Pow(p.X*304.8-point.X,2)+Math.Pow(p.Y*304.8-point.Y,2)<1;}).ToList();
            if(matches.Count!=1)return;
            foreach(var text in matches[0].Texts??new List<CadAnnotationText>())
            {
                if(text.Anchor?.Length!=3||string.IsNullOrEmpty(text.Value)||text.Height<=0)continue;
                var anchor=cadToHost.OfPoint(new XYZ(text.Anchor[0]/304.8,text.Anchor[1]/304.8,text.Anchor[2]/304.8));
                point.PreviewTexts.Add(new CadPreviewText {Value=text.Value,X=anchor.X*304.8-point.X,Y=anchor.Y*304.8-point.Y,Height=text.Height*cadToHost.BasisY.GetLength(),Angle=Math.Atan2(cadToHost.BasisX.Y,cadToHost.BasisX.X)*180/Math.PI,Centered=text.Centered});
            }
            if(point.PreviewTexts.Count>0)point.PreviewInfo+=$" {point.PreviewTexts.Count} verified DWG attributes.";
        }
    }
}