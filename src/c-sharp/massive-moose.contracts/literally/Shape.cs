using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace massive_moose.contracts.literally
{
    [DataContract(Name = "drawing")]
    public class Drawing
    {
        [DataMember(Name="shapes")]
        public List<Shape> Shapes { get; set; } 

        [DataMember(Name="imageSize")]
        public Size ImageSize { get; set; }
    }

    [DataContract(Name = "size")]
    public class Size
    {
        [DataMember(Name ="width")]
        public double Width { get; set; }

        [DataMember(Name ="height")]
        public double Height { get; set; }
    }

    [DataContract(Name="shape")]
    public class Shape
    {
        [DataMember(Name="className")]
        public string ClassName { get; set; }

        [DataMember(Name="data")]
        public ShapeData Data { get; set; }

        [DataMember(Name="Id")]
        public Guid Id { get; set; }
    }

    [DataContract(Name="data")]
    public class ShapeData
    {
        [DataMember(Name="order")]
        public int Order { get; set; }

        [DataMember(Name = "tailSize")]
        public int TailSize { get;set; }

        [DataMember(Name = "smooth")]
        public bool Smooth { get; set; }

        [DataMember(Name = "pointCoordinatePairs")]
        public List<double[]> PointCoordinatePairs { get; set; }

        [DataMember(Name= "smoothedPointCoordinatePairs")]
        public List<double[]> SmoothedPointCoordinatePairs { get; set; }

        [DataMember(Name="pointSize")]
        public double PointSize { get; set; }

        [DataMember(Name = "color")]
        public string ColorHSLAString { get; set; }
        public Hsla Color { get { return ToHsla(ColorHSLAString); } }

        [DataMember(Name="pointColor")]
        public string PointColorHSLAString { get; set; }
        public Hsla PointColor { get { return ToHsla(PointColorHSLAString); } }

        [DataMember(Name="x1")]
        public double X1 { get; set; }

        [DataMember(Name="y1")]
        public double Y1 { get; set; }

        [DataMember(Name="x2")]
        public double X2 { get; set; }

        [DataMember(Name="y2")]
        public double Y2 { get; set; }

        [DataMember(Name="strokeWidth")]
        public double StrokeWidth { get; set; }

        [DataMember(Name="capStyle")]
        public string CapStyle { get; set; }

        [DataMember(Name="dash")]
        public double[] Dash { get; set; }
        
        [DataMember(Name= "endCapShapes")]
        public string[] EndCapShapes { get; set; }

        [DataMember(Name="text")]
        public string Text { get; set; }
        
        [DataMember(Name="font")]
        public string Font { get; set; }

        [DataMember(Name="forcedWidth")]
        public double ForcedWidth { get; set; }

        [DataMember(Name = "forcedHeight")]
        public double ForcedHeight { get; set; }

        [DataMember(Name="x")]
        public double X { get; set; }

        [DataMember(Name="y")]
        public double Y { get; set; }

        [DataMember(Name="v")]
        public double V { get; set; }

        [DataMember(Name="width")]
        public double Width { get; set; }
        
        [DataMember(Name="height")]
        public double Height { get; set; }

        [DataMember(Name= "strokeColor")]
        public string StrokeColorHSLAString { get; set; }
        public Hsla StrokeColor { get { return ToHsla(StrokeColorHSLAString); } }

        [DataMember(Name = "fillColor")]
        public string FillColorHSLAString { get; set; }
        public Hsla FillColor { get { return ToHsla(FillColorHSLAString); } }

        [DataMember(Name="isClosed")]
        public bool IsClosed { get; set; }

        private Hsla ToHsla(string input)
        {
            var parts = input.Replace("hsla","").Trim('(',')').Split(',');
            var hsla = new Hsla();
            double h, s, l;
            float a;

            double.TryParse(parts[0], out h);
            double.TryParse(parts[1].Trim('%'), out s);
            double.TryParse(parts[2].Trim('%'), out l);
            float.TryParse(parts[3], out a);

            return new Hsla() {Hue=h,Saturation=s,Lightness=l/100,Alpha=a};
        }
    }

    public struct Hsla
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Lightness { get; set; }
        public float Alpha { get; set; }
    }
}
