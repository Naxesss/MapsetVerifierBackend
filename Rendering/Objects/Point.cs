using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MapsetVerifierBackend.Rendering.Objects
{
    public class Point
    {
        /*
         *  Note: Capitalization of variables matter for serialization; they must be lowercased.
        */

        public double x { get; set; }
        public double y { get; set; }

        public Point(Vector2 vec)
        {
            x = vec.X;
            y = vec.Y;
        }
    }
}
