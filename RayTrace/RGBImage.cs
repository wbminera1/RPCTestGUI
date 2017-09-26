using System;
using System.Collections.Generic;
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

}