using System.Collections.Generic;
using System.Runtime.Serialization;
using massive_moose.services.models.drawing;
using System;

namespace massive_moose.services.models.literally
{
    [DataContract(Name = "drawing")]
    public class Drawing
    {
        [DataMember(Name="shapes")]
        public List<Shape> Shapes { get; set; } 

        [DataMember(Name="imageSize")]
        public Size ImageSize { get; set; }

        [DataMember(Name="colors")]
        public DrawingColors Colors { get; set; }
    }

    [DataContract(Name = "colors")]
    public class DrawingColors
    {
        [DataMember(Name="primary")]
        public string PrimaryHslaString { get; set; }
        public Hsla PrimaryHsla { get { return ShapeData.ToHsla(PrimaryHslaString); } }
        public ColorRGB Primary { get { return ColorRGB.FromHSLA(PrimaryHsla.Hue, PrimaryHsla.Saturation, PrimaryHsla.Lightness, PrimaryHsla.Alpha); } }

        [DataMember(Name = "secondary")]
        public string SecondaryHslaString { get; set; }
        public Hsla SecondaryHsla { get { return ShapeData.ToHsla(SecondaryHslaString); } }
        public ColorRGB Secondary { get { return ColorRGB.FromHSLA(SecondaryHsla.Hue, SecondaryHsla.Saturation, SecondaryHsla.Lightness, SecondaryHsla.Alpha); } }

        [DataMember(Name = "background")]
        public string BackgroundHslaString { get; set; }
        public Hsla BackgroundHsla { get { return ShapeData.ToHsla(BackgroundHslaString); } }
        public ColorRGB Background { get { return ColorRGB.FromHSLA(BackgroundHsla.Hue, BackgroundHsla.Saturation, BackgroundHsla.Lightness, BackgroundHsla.Alpha); } }
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
        public Hsla ColorHsla { get { return ToHsla(ColorHSLAString); } }
        public ColorRGB Color { get {  return ColorRGB.FromHSLA(ColorHsla.Hue,ColorHsla.Saturation, ColorHsla.Lightness, ColorHsla.Alpha);} }

        [DataMember(Name="pointColor")]
        public string PointColorHSLAString { get; set; }
        public Hsla PointColorHsla { get { return ToHsla(PointColorHSLAString); } }
        public ColorRGB PointColor { get {  return ColorRGB.FromHSLA(PointColorHsla.Hue, PointColorHsla.Saturation, PointColorHsla.Lightness, PointColorHsla.Alpha);} }

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
        public Hsla StrokeColorHsla { get { return ToHsla(StrokeColorHSLAString); } }
        public ColorRGB StrokeColor {  get
        {
            return ColorRGB.FromHSLA(StrokeColorHsla.Hue, StrokeColorHsla.Saturation, StrokeColorHsla.Lightness,
                StrokeColorHsla.Alpha);
        } }

        [DataMember(Name = "fillColor")]
        public string FillColorHSLAString { get; set; }
        public Hsla FillColorHsla { get { return ToHsla(FillColorHSLAString); } }
        public ColorRGB FillColor { get
        {
            return ColorRGB.FromHSLA(FillColorHsla.Hue, FillColorHsla.Saturation, FillColorHsla.Lightness,
                FillColorHsla.Alpha);
        } }

        [DataMember(Name="isClosed")]
        public bool IsClosed { get; set; }

        public static Hsla ToHsla(string input)
        {
            if (input == "transparent") return new Hsla() {Alpha=0,Saturation=0,Lightness=0,Hue=0};
            var parts = input.Replace("hsla","").Trim('(',')').Split(',');
            var hsla = new Hsla();
            double h, s, l;
            float a;

            double.TryParse(parts[0], out h);
            double.TryParse(parts[1].Trim('%'), out s);
            double.TryParse(parts[2].Trim('%'), out l);
            float.TryParse(parts[3], out a);

            return new Hsla() {Hue=h,Saturation=s/100,Lightness=l/100,Alpha=a};
        }
    }

    public struct Hsla
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Lightness { get; set; }
        public double Alpha { get; set; }
    }

    public class ColorRGB
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public ColorRGB()
        {
            R = 255;
            G = 255;
            B = 255;
            A = 255;
        }

        public ColorRGB(Color value)
        {
            this.R = value.R;
            this.G = value.G;
            this.B = value.B;
            this.A = value.A;
        }



        // Given H,S,L in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public static ColorRGB FromHSL(double H, double S, double L)
        {
            return FromHSLA(H, S, L, 1.0);
        }

        // Given H,S,L,A in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public static ColorRGB FromHSLA(double H, double S, double L, double A)
        {
            double v;
            double r, g, b;
            if (A > 1.0)
                A = 1.0;

            r = L;   // default to gray
            g = L;
            b = L;
            v = (L <= 0.5) ? (L * (1.0 + S)) : (L + S - L * S);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = L + L - v;
                sv = (v - m) / v;
                H /= 360;
                H *= 6.0;
                sextant = (int)H;
                fract = H - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            ColorRGB rgb = new ColorRGB();
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);
            rgb.A = Convert.ToByte(A * 255.0f);

            System.Diagnostics.Debug.WriteLine("H:{0} S:{1} L:{2} A:{3} -> R:{4} G:{5} B:{6} A:{7}", H, S, L, A, rgb.R, rgb.G, rgb.B, rgb.A);

            return rgb;
        }
    }
}
