using System;

namespace FrontEnd.RayTrace
{
    class Primitive
    {
        public virtual bool Intersect(ref Intersection intersection)
        {
            return false;
        }
    }
}