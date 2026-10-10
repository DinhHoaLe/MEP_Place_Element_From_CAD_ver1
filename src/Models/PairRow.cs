using System;
using System.ComponentModel;
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
public class PairRow : INotifyPropertyChanged
    {
        double progress;string status="Ready";
        public double Progress {get=>progress;set{progress=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("Progress"));}}
        public string ProgressStatus {get=>status;set{status=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("ProgressStatus"));}}
        public event PropertyChangedEventHandler PropertyChanged;
        public int A {get;set;} public int B {get;set;}
        public double Length {get;set;} public double Diameter {get;set;}
        public double Delta {get;set;}
        public string Start {get;set;} public string End {get;set;}
    }
}
