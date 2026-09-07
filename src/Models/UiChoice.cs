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
/// <summary>Lựa chọn dropdown: Name để hiển thị, Id giữ định danh phần tử/layer.</summary>
public class UiChoice
    {
        public object Id {get;set;}
        public string Name {get;set;}
        public override string ToString()=>Name;
    }
}
