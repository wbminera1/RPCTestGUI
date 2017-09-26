using Mono.GameMath;
using System;

namespace FrontEnd.RayTrace
{
    class Ray
    {
        Vector3 m_Origin;
        Vector3 m_Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            m_Origin = origin;
            m_Direction = direction;
        }

        public Vector3 Origin
        {
            get
            {
                return m_Origin;
            }

            set
            {
                m_Origin = value;
            }
        }

        public Vector3 Direction
        {
            get
            {
                return m_Direction;
            }

            set
            {
                m_Direction = value;
            }
        }
    }
}