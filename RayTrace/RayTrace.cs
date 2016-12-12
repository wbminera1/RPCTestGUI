using Mono.GameMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FrontEnd.RayTrace
{
    class RGBImage
    {
        byte[] m_Pixels;
        int m_Width;
        int m_Height;
        int m_BPP;
        int m_BPL;

        public RGBImage(int width, int height, int bpp)
        {
            m_Width = width;
            m_Height = height;
            m_BPP = bpp;
            m_BPL = m_BPP * m_Width;
            m_Pixels = new byte[m_Height * m_BPL];
        }

        public void SetPixel(int x, int y, int color)
        {
            if (ClampPixel(x, y))
            {
                int offset = y * m_BPL + x * m_BPP;
                for (int i = 0; i < m_BPP; ++i)
                {
                    m_Pixels[offset] = (byte)(color & 0xFF);
                    color >>= 8;
                    ++offset;
                }
            }
        }

        bool ClampPixel(int x, int y)
        {
            return x >= 0 && y >= 0 && x < m_Width && y < m_Height;
        }

        public byte[] GetRawData()
        {
            return m_Pixels;
        }

        public BitmapSource RawToBitmap()
        {
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Red);
            colors.Add(System.Windows.Media.Colors.Blue);
            colors.Add(System.Windows.Media.Colors.Green);
            BitmapPalette myPalette = new BitmapPalette(colors);

            BitmapSource image = BitmapSource.Create(
                m_Width,
                m_Height,
                96,
                96,
                System.Windows.Media.PixelFormats.Bgr32,
                myPalette,
                m_Pixels,
                m_BPL);

            return image;

        }
    }

    class Sampler
    {
        int m_Width;
        int m_Height;

        public Sampler(int width, int height)
        {
            m_Width = width;
            m_Height = height;
        }

        public Vector2[] GetSamples()
        {
            float hwidth = 0.5f * (float)m_Width;
            float hheight = 0.5f * (float)m_Height;
            Vector2[] samples = new Vector2[m_Width * m_Height];
            for (int y = 0; y < m_Height; ++y)
                for (int x = 0; x < m_Width; ++x)
                {
                    float offx = ((float)x - hwidth) / hwidth;
                    float offy = ((float)y - hheight) / hheight;
                    samples[m_Width * y + x] = new Vector2(offx, offy);
                }
            return samples;
        }
    }

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
    class Intersection
    {
        public Ray m_Ray;
        public Vector3 m_Hit;
        public Vector3 m_Normal;
    }

    class Primitive
    {
        public virtual bool Intersect(ref Intersection intersection)
        {
            return false;
        }
    }

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

                float disc = b * b - 4.0f * a * c;
                if (disc < 0)
                {
                    //return Intersection(); // No intersection.
                    return false;
                }

                disc = (float) Math.Sqrt((double)disc);

                float q;
                if (b < 0)
                {
                    q = (-b - disc) * 0.5f;
                }
                else
                {
                    q = (-b + disc) * 0.5f;
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
            Sphere sph = new Sphere(new Vector3(0,0,2), 1);
            Intersection intersection = new Intersection();
            intersection.m_Ray = new Ray(new Vector3(0), new Vector3(0, 0, 1.0f));

            bool res = sph.Intersect(ref intersection);
        }
    }

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
            foreach (Primitive p in m_Primitives)
            {
                Intersection intersection = new Intersection();
                intersection.m_Ray = ray;
                if(p.Intersect(ref intersection))
                {
                    color = new Vector3(1.0f, 0.0f, 0.0f);
                    return true;
                }
            }
            return false;
        }
    }

    class RayTrace
    {
        public RGBImage Trace(int width, int height)
        {
            RGBImage image = new RGBImage(width, height, 4);
            Sampler sampler = new Sampler(width, height);
            Scene scene = new Scene();
            scene.Add(new Sphere(new Vector3(0,0,2.0f), 0.5f));
            Vector2[] samples = sampler.GetSamples();
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Vector2 sample = samples[width * y + x];
                    Ray ray = new Ray(new Vector3(0), new Vector3(sample, 1.0f));
                    Vector3 color;
                    if(scene.Trace(ray, out color))
                    {
                        color = Vector3.Clamp(color, new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                        color = Vector3.Multiply(color, 255);
                        int r = (int)color.X;
                        int g = (int)color.Y;
                        int b = (int)color.Z;
                        image.SetPixel(x, y, (r << 16) | (g << 8)  | b);
                    }
                }
            }
            return image;
        }
    }
}
