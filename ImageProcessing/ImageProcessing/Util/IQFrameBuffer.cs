using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ImageProcessing.Util
{
    public class IQFrameBuffer
    {
        public static string TAG = "IQFrameBuffer";

        public int texture;
        public int buffer;
        public int width;
        public int height;
        public FrameBufferType frameBufferType;

        public IQFrameBuffer(int iWidth, int iHeight, FrameBufferType frameBufferType)
        {
            this.width = iWidth;
            this.height = iHeight;
            this.frameBufferType = frameBufferType;

            createFramebuffer();
        }

        private void createFramebuffer()
        {
            buffer = Gl.createFramebuffer();

            IQUtil.IQAssert(buffer != 0);

            Gl.bindFramebuffer(Gl.FRAMEBUFFER, buffer);

            texture = createTexture(width, height);

            Gl.framebufferTexture2D(Gl.FRAMEBUFFER, Gl.COLOR_ATTACHMENT0, Gl.TEXTURE_2D, texture, 0);

            FramebufferErrorCode status = Gl.checkFramebufferStatus(Gl.FRAMEBUFFER);

            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                IQUtil.IQAssert();
            }
        }

        private int createTexture(int width, int height)
        {
            int texture = Gl.createTexture();

            IQUtil.IQAssert(texture != 0);

            if (frameBufferType == FrameBufferType.FrameBufferType_UnsignedByte)
            {
                Gl.bindTexture(Gl.TEXTURE_2D, texture);
                Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_S, (int)Gl.CLAMP_TO_EDGE);
                Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_T, (int)Gl.CLAMP_TO_EDGE);
                Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MIN_FILTER, (int)Gl.NEAREST);
                Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MAG_FILTER, (int)Gl.NEAREST);

                Gl.texImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                Gl.bindTexture(Gl.TEXTURE_2D, 0);
            }
            else if (frameBufferType == FrameBufferType.FrameBufferType_Float)
            {
                Gl.bindTexture(Gl.TEXTURE_2D, texture);
                Gl.texImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Gl.bindTexture(Gl.TEXTURE_2D, 0);
            }
            else
            {
                IQUtil.IQAssert();
            }

            return texture;
        }

        public System.Drawing.Bitmap saveFrameBufferToBmp()
        {
            Gl.bindFramebuffer(Gl.FRAMEBUFFER, buffer);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height);
            System.Drawing.Imaging.BitmapData bits = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), 
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.readPixels(0, 0, width, height, Gl.RGBA, Gl.UNSIGNED_BYTE, bits.Scan0);
            bitmap.UnlockBits(bits);

            Gl.bindFramebuffer(Gl.FRAMEBUFFER, 0);
            return bitmap;
        }

        private byte[] writeInt(int value)
        {
            byte[] b = new byte[4];

            b[0] = (byte)(value & 0x000000FF);

            b[1] = (byte)((value & 0x0000FF00) >> 8);

            b[2] = (byte)((value & 0x00FF0000) >> 16);

            b[3] = (byte)((value & 0xFF000000) >> 24);

            return b;
        }

        private double[] exp2Table = {
                                            2.168404E-19, 4.336809E-19, 8.673617E-19, 1.734723E-18,
                                            3.469447E-18, 6.938894E-18, 1.387779E-17, 2.775558E-17,
                                            5.551115E-17, 1.110223E-16, 2.220446E-16, 4.440892E-16,
                                            8.881784E-16, 1.776357E-15, 3.552714E-15, 7.105427E-15,
                                            1.421085E-14, 2.842171E-14, 5.684342E-14, 1.136868E-13,
                                            2.273737E-13, 4.547474E-13, 9.094947E-13, 1.818989E-12,
                                            3.637979E-12, 7.275958E-12, 1.455192E-11, 2.910383E-11,
                                            5.820766E-11, 1.164153E-10, 2.328306E-10, 4.656613E-10,
                                            9.313226E-10, 1.862645E-09, 3.725290E-09, 7.450581E-09,
                                            1.490116E-08, 2.980232E-08, 5.960464E-08, 1.192093E-07,
                                            2.384186E-07, 4.768372E-07, 9.536743E-07, 1.907349E-06,
                                            3.814697E-06, 7.629395E-06, 1.525879E-05, 3.051758E-05,
                                            6.103516E-05, 1.220703E-04, 2.441406E-04, 4.882812E-04,
                                            9.765625E-04, 1.953125E-03, 3.906250E-03, 7.812500E-03,
                                            1.562500E-02, 3.125000E-02, 6.250000E-02, 1.250000E-01,
                                            2.500000E-01, 5.000000E-01, 1.000000E+00, 2.000000E+00,
                                            4.000000E+00, 8.000000E+00, 1.600000E+01, 3.200000E+01,
                                            6.400000E+01, 1.280000E+02, 2.560000E+02, 5.120000E+02,
                                            1.024000E+03, 2.048000E+03, 4.096000E+03, 8.192000E+03,
                                            1.638400E+04, 3.276800E+04, 6.553600E+04, 1.310720E+05,
                                            2.621440E+05, 5.242880E+05, 1.048576E+06, 2.097152E+06,
                                            4.194304E+06, 8.388608E+06, 1.677722E+07, 3.355443E+07,
                                            6.710886E+07, 1.342177E+08, 2.684355E+08, 5.368709E+08,
                                            1.073742E+09, 2.147484E+09, 4.294967E+09, 8.589935E+09,
                                            1.717987E+10, 3.435974E+10, 6.871948E+10, 1.374390E+11,
                                            2.748779E+11, 5.497558E+11, 1.099512E+12, 2.199023E+12,
                                            4.398047E+12, 8.796093E+12, 1.759219E+13, 3.518437E+13,
                                            7.036874E+13, 1.407375E+14, 2.814750E+14, 5.629500E+14,
                                            1.125900E+15, 2.251800E+15, 4.503600E+15, 9.007199E+15,
                                            1.801440E+16, 3.602880E+16, 7.205759E+16, 1.441152E+17,
                                            2.882304E+17, 5.764608E+17, 1.152922E+18, 2.305843E+18
                                        };

        private byte[] encodeFloatArray(double v)
        {
            byte[] c = new byte[4];

            if (v < 0.0)
            {
                c[0] += 64;
                v = -v;
            }

            double f = 0.0;
            double e = (double)Math.Ceiling(Math.Log(v));
            double m = (double)(v * Math.Exp(-e));
            if (e < 0.0)
            {
                e = -e;
                c[0] += 128;
            }

            c[0] += (byte)e;
            m *= 255.0f;
            f = Math.Floor(m);
            c[1] = (byte)(f);
            m -= f;
            m *= 255.0f;
            f = (float)Math.Floor(m);
            c[2] = (byte)f;
            m -= f;
            m *= 255.0f;
            c[3] = (byte)Math.Floor(m);


            for (int i = 0; i < 4; i++)
            {
                c[i] = (byte)(c[i] * 3.921569E-03);
            }

            return c;
        }



        private double[] decodeFloatArray(byte[] input)
        {
            byte e, i_sign;
            int i4 = 0;
            double m = 0.0f;
            double[] output = new double[input.Length / 4];
            int len = output.Length;

            for (int i = 0; i < len; i += 1)
            {
                i4 = i * 4;

                m = (input[i4 + 1] & 0xFF) * 3.921569E-03
                        + (input[i4 + 2] & 0xFF) * 1.537870E-05
                        + (input[i4 + 3] & 0xFF) * 6.030863E-08;
                e = input[i4 + 0];
                i_sign = 0;

                if ((e & 0x80) == 0x80)
                {
                    i_sign = 1;
                    //e &= ~0x80;
                    e = (byte)(e & (~0x80));
                }

                if ((e & 0x40) == 0x40)
                {
                    m = -m;
                    //e &= ~0x40;
                    e = (byte)(e & (~0x40));
                }

                if (i_sign == 1)
                {
                    //e *= -1;
                    e = (byte)(e * -1);
                }


                if (e < -62)
                {
                    //
                    // Not sure why we are getting less than -62, this goes out of array index.
                    //
                    int a = -62;
                    e = (byte)(a);
                }

                output[i] = m * exp2Table[e + 62];
            }

            return output;
        }
    }
}
