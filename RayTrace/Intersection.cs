using Mono.GameMath;
using System;

namespace FrontEnd.RayTrace
{
    class Intersection
    {
        public Ray m_Ray;
        public Vector3 m_Hit;
        public Vector3 m_Normal;
        public float m_Distance;
        public Primitive m_Target;
    }
}