using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;


namespace CadLayerTools
{
/// <summary>Dữ liệu một hàng bảng WPF: số thứ tự biên, chiều dài tim, Diameter và tọa độ.</summary>
public class PairRow
    {
        public int A {get;set;} public int B {get;set;}
        public double Length {get;set;} public double Diameter {get;set;}
        public double Delta {get;set;}
        public string Start {get;set;} public string End {get;set;}
    }
}
