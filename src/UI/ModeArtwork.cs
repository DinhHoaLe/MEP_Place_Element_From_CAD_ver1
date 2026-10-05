using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace CadLayerTools
{
    internal static class ModeArtwork
    {
        public static ImageSource Load(string name)
        {
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("CadLayerTools.Assets."+name+".png"))
            {
                if(stream==null)return null;
                var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.StreamSource=stream;image.EndInit();image.Freeze();return image;
            }
        }
    }
}
