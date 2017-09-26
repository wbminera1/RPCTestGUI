using Mono.GameMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontEnd.RayTrace
{

    class RayTrace
    {
        public RGBImage Trace(int width, int height)
        {
            RGBImage image = new RGBImage(width, height, 4);
            Sampler sampler = new Sampler(width, height);
            Scene scene = new Scene();
            scene.Add(new Sphere(new Vector3(-0.5f, 0.0f, 2.7f), 0.5f));
            scene.Add(new Sphere(new Vector3( 0,      0,  2.5f), 0.5f));
            scene.Add(new Sphere(new Vector3( 0.5f, 0.0f, 2.3f), 0.5f));
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
