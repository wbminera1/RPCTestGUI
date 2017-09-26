using Mono.GameMath;
using System;

namespace FrontEnd.RayTrace
{

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
            float aspectRatio = (float)m_Width / (float)m_Height;
            float hwidth = 0.5f * (float)m_Width;
            float hheight = 0.5f * (float)m_Height;
            Vector2[] samples = new Vector2[m_Width * m_Height];
            for (int y = 0; y < m_Height; ++y)
                for (int x = 0; x < m_Width; ++x)
                {
                    float offx = ((float)x - hwidth) * aspectRatio / hwidth;
                    float offy = ((float)y - hheight) / hheight;
                    samples[m_Width * y + x] = new Vector2(offx, offy);
                }
            return samples;
        }
    }

}