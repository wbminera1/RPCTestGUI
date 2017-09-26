using Mono.GameMath;
using System;

namespace FrontEnd.RayTrace
{
    class Sphere : Primitive
    {
        Vector3 m_Pos;
        float m_Radius;
        public Sphere(Vector3 pos, float radius)
        {
            m_Pos = pos;
            m_Radius = radius;
        }

        public override bool Intersect(ref Intersection intersection)
        {

            {
                Vector3 deltap = intersection.m_Ray.Origin - m_Pos;
                float a = Vector3.Dot(intersection.m_Ray.Direction, intersection.m_Ray.Direction);
                float b = Vector3.Dot(deltap, intersection.m_Ray.Direction) * 2.0f;
                float c = Vector3.Dot(deltap, deltap) - (m_Radius * m_Radius);

                float d = b * b - 4.0f * a * c;
                if (d < 0)
                {
                    return false;
                }

                d = (float)Math.Sqrt((double)d);

                float q;
                if (b < 0)
                {
                    q = (-b - d) * 0.5f;
                }
                else
                {
                    q = (-b + d) * 0.5f;
                }

                float r1 = q / a;
                float r2 = c / q;

                if (r1 > r2)
                {
                    float tmp = r1;
                    r1 = r2;
                    r2 = tmp;
                }

                float distance = r1;
                if (distance < 0)
                {
                    distance = r2;
                }

                if (distance < 0 /*|| isnan(distance)*/)
                {
                    //return Intersection(); // No intersection.
                    return false;
                }

                intersection.m_Distance = distance;
                intersection.m_Hit = intersection.m_Ray.Origin + (intersection.m_Ray.Direction * distance);
                intersection.m_Normal = Vector3.Normalize(intersection.m_Hit - m_Pos);

                //normal = material->modifyNormal(normal, point);

                // Normal needs to be flipped if this is a refractive ray.
                //                if (ray.direction.dot(normal) > 0)
                //                {
                //                    normal = normal * -1;
                //                }

                //return Intersection(ray, point, distance, normal, ray.material, material, this);

            }
            return true;
        }

        public static void Test()
        {
            Sphere sph = new Sphere(new Vector3(0, 0, 2), 1);
            Intersection intersection = new Intersection();
            intersection.m_Ray = new Ray(new Vector3(0), new Vector3(0, 0, 1.0f));

            bool res = sph.Intersect(ref intersection);
        }
    }
}