using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using massive_moose.services;
using massive_moose.services.models.drawing;
using massive_moose.services.models.literally;

namespace massive_moose.drawing.console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: massive-moose.drawing.console [silverlight|literally] [filename]");
                Console.WriteLine("input file must be a json formatted representation of stuff to draw");
                Environment.Exit(0);
            }

            var type = (FormatType)Enum.Parse(typeof(FormatType), args[0]);
            string filename = args[1];

            switch (type)
            {
                case FormatType.silverlight:
                    RenderSilverlight(System.IO.File.ReadAllText(filename));
                    break;
                case FormatType.literally:
                    RenderLiterally(System.IO.File.ReadAllText(filename));
                    break;
            }
        }

        private static void RenderLiterally(string input)
        {
            var drawing = JsonConvert.DeserializeObject<Drawing>(input);
            foreach (var shape in drawing.Shapes)
            {
                Console.WriteLine("shape type: {0}:", shape.ClassName);
                Console.WriteLine("pointsize: {0}", shape.Data.PointSize);
                Console.WriteLine("color: {0}", shape.Data.PointColorHSLAString);
                if (shape.Data != null)
                {
                    if (shape.Data.PointCoordinatePairs != null)
                    {
                        Console.WriteLine("points:");
                        foreach (var point in shape.Data.PointCoordinatePairs)
                        {
                            Console.WriteLine("{0},{1}", point[0], point[1]);
                        }
                    }
                    Console.WriteLine("smoothed points:");
                    if (shape.Data.SmoothedPointCoordinatePairs != null)
                    {
                        foreach (var point in shape.Data.SmoothedPointCoordinatePairs)
                        {
                            Console.WriteLine("{0},{1}", point[0], point[1]);
                        }
                    }
                }
            }

            var canvas = new LiterallyMapper().ToCanvas(drawing);
            var imageData = new BrickRenderer().Render(canvas);
            SaveImage(imageData);
        }

        private static void RenderSilverlight(string input)
        {
            Canvas canvas = JsonConvert.DeserializeObject<Canvas>(input);

            var imageData = new BrickRenderer().Render(canvas);

            SaveImage(imageData);
        }

        private static void SaveImage(byte[] imageData)
        {
            string outputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"images\brick.png");
            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            using (var file = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                file.Write(imageData, 0, imageData.Length);
                file.Flush();
                file.Close();
            }
        }

        private static void RenderSilverlight()
        {
            throw new NotImplementedException();
        }
    }

    public enum FormatType
    {
        silverlight,
        literally
    }
}
