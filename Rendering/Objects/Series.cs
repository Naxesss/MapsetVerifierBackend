using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MapsetVerifierBackend.Rendering.Objects
{
    public class Series
    {
        public string        Label  { get; }
        public List<Vector2> Points { get; }
        public Color         Color  { get; }

        public Series(string label, List<Vector2> points = null, Color? color = null)
        {
            Label  = label;
            Points = points ?? new List<Vector2>();
            Color  = color ?? Color.FromArgb(255, 255, 255, 255);
        }
    }
}
