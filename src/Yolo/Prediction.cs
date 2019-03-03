using System;
using System.Collections.Generic;
using System.Text;

namespace Yolo
{
    public class Prediction
    {
        public string Name { get; set; }
        public float Probability { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
