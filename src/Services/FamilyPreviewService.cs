using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
namespace CadLayerTools
{
    internal static class FamilyPreviewService
    {
        public static ImageSource Read(Document doc,object id)
        {
            try
            {
                var symbol=doc.GetElement((ElementId)id) as FamilySymbol;
                using(var bitmap=symbol?.GetPreviewImage(new Size(320,240)))
                {
                    if(bitmap==null)return null;
                    using(var stream=new MemoryStream())
                    {
                        bitmap.Save(stream,ImageFormat.Png);stream.Position=0;
                        var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.StreamSource=stream;image.EndInit();image.Freeze();return image;
                    }
                }
            }
            catch(Exception){return null;}
        }
    }
}
