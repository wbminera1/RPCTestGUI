using Mono.GameMath;
using System;
using System.Collections.Generic;

namespace FrontEnd.RayTrace
{
    class Scene
    {
        List<Primitive> m_Primitives = new List<Primitive>();

        public void Add(Primitive primitive)
        {
            m_Primitives.Add(primitive);
        }

        public bool Trace(Ray ray, out Vector3 color)
        {
            color = new Vector3(0.0f, 0.0f, 0.0f);
            Intersection closest = null;
            foreach (Primitive p in m_Primitives)
            {
                Intersection intersection = new Intersection();
                intersection.m_Ray = ray;
                if (p.Intersect(ref intersection))
                {
                    if (closest == null || closest.m_Distance > intersection.m_Distance)
                    {
                        closest = intersection;
                    }
                }
            }
            if (closest != null)
            {
                color = new Vector3(1.0f / (closest.m_Distance + 0.01f), 0.0f, 0.0f);
                return true;
            }
            return false;
        }
    }
}