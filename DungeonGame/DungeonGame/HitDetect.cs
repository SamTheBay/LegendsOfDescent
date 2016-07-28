using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    public static class HitDetect
    {

        public static bool HitDetectTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 point)
        {
            float a, b, c;
            a = (v1.X - point.X) * (v2.Y - point.Y) - (v2.X - point.X) * (v1.Y - point.Y);
            b = (v2.X - point.X) * (v3.Y - point.Y) - (v3.X - point.X) * (v2.Y - point.Y);
            c = (v3.X - point.X) * (v1.Y - point.Y) - (v1.X - point.X) * (v3.Y - point.Y);
            return (Math.Sign(a) == Math.Sign(b) && Math.Sign(b) == Math.Sign(c));
        }


        public static bool HitDetectConvexQuadrialteral(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Vector2 point)
        {
            return HitDetectTriangle(v1, v2, v3, point) || HitDetectTriangle(v1, v3, v4, point);
        }

    }
}
