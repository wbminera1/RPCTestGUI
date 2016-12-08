using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FrontEnd
{
    class Draw
    {
        byte[] m_Pixels;
        int m_Width;
        int m_Height;
        int m_BPP;
        int m_BPL;

        public void Init(int width, int height, int bpp)
        {
            m_Width = width;
            m_Height = height;
            m_BPP = bpp;
            m_BPL = m_BPP * m_Width;
            m_Pixels = new byte[m_Height * m_BPL];
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
                PixelFormats.Bgr32,
                myPalette,
                m_Pixels,
                m_BPL);

            return image;

        }

        public void SetPixel(int x, int y, UInt32 color)
        {
            if(ClampPixel(x,y))
            {
                int offset = y * m_BPL + x * m_BPP;
                for(int i = 0; i < m_BPP; ++i)
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

        public void Line(int x1, int y1, int x2, int y2, uint color)
        {
            if(x1 > x2)
            {
                int tmp = x1;  x1 = x2; x2 = tmp;
                tmp = y1; y1 = y2; y2 = tmp;
            }
            int deltax = x2 - x1;
            int deltay = y2 - y1;
            if(deltay < 0)
            {
                deltay = -deltay;
            }
            int counter = deltax;
            int deltaMax = deltax;
            if (deltax < deltay)
            {
                counter = deltay;
                deltaMax = deltay;
            }
            if(deltaMax == 0)
            {
                SetPixel(x1, y1, color);
                return;
            }
            deltax = (deltax << 16) / deltaMax;
            deltay = (deltay << 16) / deltaMax;
            int counterx = deltax >> 1;
            int countery = deltay >> 1;

            for (;counter >= 0; --counter)
            {
                if(y2 > y1)
                {
                    SetPixel(x1 + (counterx >> 16), y1 + (countery >> 16), color);
                }
                else
                {
                    SetPixel(x1 + (counterx >> 16), y1 - (countery >> 16), color);
                }
                counterx += deltax;
                countery += deltay;

            }
        }

        public void TestSetPixel()
        {
            Random rnd = new Random();
            for(int i = 0; i < 256; ++i)
            {
                int x = rnd.Next(0, m_Width - 1);
                int y = rnd.Next(0, m_Height - 1);
                uint r = (uint)rnd.Next(0, 255);
                uint g = (uint)rnd.Next(0, 255);
                uint b = (uint)rnd.Next(0, 255);
                SetPixel(x, y, r << 16 | g << 8 | b);
            }
        }
        public void TestLine()
        {
            Line(0, 0, 255, 0, 0x7FFFFF);
            Line(0, 255, 255, 255, 0x7FFFFF);
            Line(0, 0, 0, 255, 0x7FFFFF);
            Line(255, 0, 255, 255, 0x7FFFFF);

            Line(0, 0, 255, 255, 0xFFFFFF);
            Line(0, 255, 255, 0, 0xFFFFFF);
            Line(0, 0, 127, 255, 0xFFFFFF);
            /*
            Random rnd = new Random();
            for (int i = 0; i < 256; ++i)
            {
                int x1 = rnd.Next(0, m_Width - 1);
                int y1 = rnd.Next(0, m_Height - 1);
                int x2 = rnd.Next(0, m_Width - 1);
                int y2 = rnd.Next(0, m_Height - 1);
                uint r = (uint)rnd.Next(0, 255);
                uint g = (uint)rnd.Next(0, 255);
                uint b = (uint)rnd.Next(0, 255);
                Line(x1, y1, x2, y2, r << 16 | g << 8 | b);
            }
            */
        }
    }
}
