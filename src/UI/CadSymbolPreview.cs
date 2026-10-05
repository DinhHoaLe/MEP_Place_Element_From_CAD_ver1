using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
namespace CadLayerTools
{
    public class CadSymbolPreview : FrameworkElement
    {
        EquipmentPoint point;
        public EquipmentPoint Point {get=>point;set{point=value;InvalidateVisual();}}
        public CadSymbolPreview() {Height=190;MinWidth=100;ClipToBounds=true;}
        protected override void OnRender(DrawingContext drawing)
        {
            base.OnRender(drawing);
            drawing.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(250,248,245)),new Pen(new SolidColorBrush(Color.FromRgb(221,214,206)),1),new Rect(0,0,ActualWidth,ActualHeight),8,8);
            if(point==null || point.Preview.Count==0)
            {
                var text=new FormattedText(point==null?"Chọn một dòng để xem symbol CAD":"Block này không có nét 2D để xem trước",CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),13,ToolWindow.Muted,1.0);
                drawing.DrawText(text,new Point(18,24));return;
            }
            double minX=Math.Min(0,point.Preview.Min(s=>Math.Min(s.X1,s.X2))),maxX=Math.Max(0,point.Preview.Max(s=>Math.Max(s.X1,s.X2)));
            double minY=Math.Min(0,point.Preview.Min(s=>Math.Min(s.Y1,s.Y2))),maxY=Math.Max(0,point.Preview.Max(s=>Math.Max(s.Y1,s.Y2)));
            double scale=Math.Min(Math.Max(1,ActualWidth-50)/Math.Max(1,maxX-minX),Math.Max(1,ActualHeight-40)/Math.Max(1,maxY-minY));
            Func<double,double,Point> map=(x,y)=>new Point(ActualWidth/2+(x-(minX+maxX)/2)*scale,ActualHeight/2-(y-(minY+maxY)/2)*scale);
            var pen=new Pen(new SolidColorBrush(Color.FromRgb(111,96,68)),1.5);
            foreach(var stroke in point.Preview)drawing.DrawLine(pen,map(stroke.X1,stroke.Y1),map(stroke.X2,stroke.Y2));
            var anchor=map(0,0);var red=new Pen(new SolidColorBrush(Color.FromRgb(217,75,91)),2);
            drawing.DrawLine(red,new Point(anchor.X-6,anchor.Y),new Point(anchor.X+6,anchor.Y));
            drawing.DrawLine(red,new Point(anchor.X,anchor.Y-6),new Point(anchor.X,anchor.Y+6));
        }
    }
}
