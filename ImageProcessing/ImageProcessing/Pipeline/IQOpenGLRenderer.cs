using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessing.Effects;
using ImageProcessing.Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ImageProcessing.Pipeline
{
    public class IQOpenGLRenderer
    {
        private static string TAG 						  			= "IQOpenGLRenderer";
	    private int m_SurfaceTextureID = 0;
        private System.Drawing.Bitmap m_BmpSrc = null;
        public System.Drawing.Bitmap m_BmpDst = null;
        private IQEffect_Copy m_IQEffect_Copy = null;
        private IQEffect_BitmapImageTexture m_IQEffect_BitmapImageTexture = null;
        private IQEffect_LOG m_IQEffect_LOG = null;
        private IQEffect_ABF m_IQEffect_ABF = null;
        private EffectSettingsParameters[] m_EffectSettingsParameters = null;
        private RendererOptions[] m_RendererOptions = null;
        private RendererStats[] m_RendererStats = null;
        private int[] m_SourceTexture = null;
        private Dictionary<string, EffectInitParameters> m_effectInitParametersDict = new Dictionary<string, EffectInitParameters>();
        private int m_iScreenWidth = 0;
        private int m_iScreenHeight = 0;

        private GLControl m_glControl;
        private Models.Effects m_effect; 

        public IQOpenGLRenderer(int iScreenWidth, int iScreenHeight)
        {
            GlHelper.resetValuesToDefault();

            m_iScreenWidth = iScreenWidth;

            m_iScreenHeight = iScreenHeight;

            m_EffectSettingsParameters = new EffectSettingsParameters[1];

            m_EffectSettingsParameters[0] = new EffectSettingsParameters();

            m_RendererOptions = new RendererOptions[1];

            m_RendererOptions[0] = new RendererOptions();

            m_RendererStats = new RendererStats[1];

            m_RendererStats[0] = new RendererStats();
        }

        private void createSourceTexture()
        {
            m_SourceTexture = new int[1];

            m_SourceTexture[0] = Gl.createTexture();

            if (m_SourceTexture[0] == 0)
            {
                Console.WriteLine("{0}, {1}", TAG, "Could not generate a new OpenGL texture object.");

                IQUtil.IQAssert();
            }
            m_SurfaceTextureID = m_SourceTexture[0];
        }

        public void setEffect(Models.Effects effect)
        {
            m_effect = effect;
        }

        public void init()
        {
            m_glControl = new GLControl(new GraphicsMode(DisplayDevice.Default.BitsPerPixel, 16, 0, 4, 0, 2, false));
            m_glControl.MakeCurrent();
            createSourceTexture();
        }

        public void DrawFrame()
        {
            initEffects();

            Console.WriteLine("Render Frame...");

            Gl.clearColor(0.0f, 0.0f, 0.0f, 0.0f);

            Gl.clear(Gl.DEPTH_BUFFER_BIT | Gl.COLOR_BUFFER_BIT);

            {
                loadTextureFromImage();

                if (m_effect == Models.Effects.ABF)
                {
                    int[] src = new int[1] { m_SurfaceTextureID };
                    IQFrameBuffer[] dst = new IQFrameBuffer[1] { m_IQEffect_LOG.m_DefaultFrameBufferProp[0].m_IQFrameBuffer };
                    m_IQEffect_LOG.RenderFrame(src, dst);

                    src = new int[2] { m_SurfaceTextureID, m_IQEffect_LOG.m_DefaultFrameBufferProp[0].m_IQFrameBuffer.texture };
                    dst = new IQFrameBuffer[1] { m_IQEffect_ABF.m_DefaultFrameBufferProp[0].m_IQFrameBuffer };
                    m_IQEffect_ABF.RenderFrame(src, dst);

                    m_BmpDst = m_IQEffect_ABF.m_DefaultFrameBufferProp[0].m_IQFrameBuffer.saveFrameBufferToBmp();
                    m_BmpDst.Save("C:\\temp\\gpu.png", System.Drawing.Imaging.ImageFormat.Png);
                }
                else if (m_effect == Models.Effects.NO_EFFECT)
                {
                    int[] src = new int[1] { m_SurfaceTextureID };
                    IQFrameBuffer[] dst = new IQFrameBuffer[1] { m_IQEffect_BitmapImageTexture.m_DefaultFrameBufferProp[0].m_IQFrameBuffer };
                    m_IQEffect_BitmapImageTexture.RenderFrame(src, dst);

                    m_BmpDst = m_IQEffect_BitmapImageTexture.m_DefaultFrameBufferProp[0].m_IQFrameBuffer.saveFrameBufferToBmp();
                    m_BmpDst.Save("C:\\temp\\gpu.png", System.Drawing.Imaging.ImageFormat.Png);
                }

            }
        }

        private void initEffects()
        {
            if (m_effect == Models.Effects.ABF)
            {
                if (m_IQEffect_ABF == null)
                {
                    EffectInitParameters effectInitParameters = new EffectInitParameters();
                    effectInitParameters.iScreenWidth = m_iScreenWidth;
                    effectInitParameters.iScreenHeight = m_iScreenHeight;
                    effectInitParameters.szShaderVersion = "1.0";
                    effectInitParameters.frameBufferType = FrameBufferType.FrameBufferType_UnsignedByte;

                    m_effectInitParametersDict.Add("IQEffect_ABF", effectInitParameters);
                    m_IQEffect_ABF = new IQEffect_ABF(effectInitParameters);

                    IQUtil.IQAssert(m_IQEffect_ABF != null);

                    if (m_IQEffect_ABF.loadResources() == false)
                    {
                        IQUtil.IQAssert();

                        return;
                    }

                    m_IQEffect_ABF.initializeVariables();
                }
                else
                {
                    if(m_effectInitParametersDict.TryGetValue("IQEffect_ABF", out EffectInitParameters value))
                    {
                        if (value.iScreenWidth != m_iScreenWidth || value.iScreenHeight != m_iScreenHeight)
                        {
                            // update dictionary
                            value.iScreenHeight = m_iScreenHeight;
                            value.iScreenWidth = m_iScreenWidth;
                            m_effectInitParametersDict["IQEffect_ABF"] = value;

                            // recreate the frame buffer
                            m_IQEffect_ABF.resetFrameBuffer(m_iScreenWidth, m_iScreenHeight);
                        }
                        else
                        {
                            Console.WriteLine("ABF frame buffer exist");
                        }
                    }
                    
                }

                if (m_IQEffect_LOG == null)
                {
                    EffectInitParameters effectInitParameters = new EffectInitParameters();
                    effectInitParameters.iScreenWidth = m_iScreenWidth;
                    effectInitParameters.iScreenHeight = m_iScreenHeight;
                    effectInitParameters.szShaderVersion = "1.0";
                    effectInitParameters.frameBufferType = FrameBufferType.FrameBufferType_Float;

                    m_effectInitParametersDict.Add("IQEffect_LOG", effectInitParameters);

                    m_IQEffect_LOG = new IQEffect_LOG(effectInitParameters);

                    IQUtil.IQAssert(m_IQEffect_LOG != null);

                    if (m_IQEffect_LOG.loadResources() == false)
                    {
                        IQUtil.IQAssert();

                        return;
                    }

                    m_IQEffect_LOG.initializeVariables();
                }
                else
                {
                    if (m_effectInitParametersDict.TryGetValue("IQEffect_LOG", out EffectInitParameters value))
                    {
                        if (value.iScreenWidth != m_iScreenWidth || value.iScreenHeight != m_iScreenHeight)
                        {
                            // update dictionary
                            value.iScreenHeight = m_iScreenHeight;
                            value.iScreenWidth = m_iScreenWidth;
                            m_effectInitParametersDict["IQEffect_LOG"] = value;

                            // recreate the frame buffer
                            m_IQEffect_LOG.resetFrameBuffer(m_iScreenWidth, m_iScreenHeight);
                        }
                        else
                        {
                            Console.WriteLine("LOG frame buffer exist");
                        }
                    }
                }
            }
            else if (m_effect == Models.Effects.NO_EFFECT)
            {
                if (m_IQEffect_BitmapImageTexture == null)
                {
                    EffectInitParameters effectInitParameters = new EffectInitParameters();
                    effectInitParameters.iScreenWidth = m_iScreenWidth;
                    effectInitParameters.iScreenHeight = m_iScreenHeight;
                    effectInitParameters.szShaderVersion = "1.0";
                    effectInitParameters.frameBufferType = FrameBufferType.FrameBufferType_UnsignedByte;

                    m_effectInitParametersDict.Add("IQEffect_BitmapImageTexture", effectInitParameters);

                    m_IQEffect_BitmapImageTexture = new IQEffect_BitmapImageTexture(effectInitParameters);

                    IQUtil.IQAssert(m_IQEffect_BitmapImageTexture != null);

                    if (m_IQEffect_BitmapImageTexture.loadResources() == false)
                    {
                        IQUtil.IQAssert();

                        return;
                    }

                    m_IQEffect_BitmapImageTexture.initializeVariables();
                }
                else
                {
                    if (m_effectInitParametersDict.TryGetValue("IQEffect_BitmapImageTexture", out EffectInitParameters value))
                    {
                        if (value.iScreenWidth != m_iScreenWidth || value.iScreenHeight != m_iScreenHeight)
                        {
                            // update dictionary
                            value.iScreenHeight = m_iScreenHeight;
                            value.iScreenWidth = m_iScreenWidth;
                            m_effectInitParametersDict["IQEffect_BitmapImageTexture"] = value;

                            // recreate the frame buffer
                            m_IQEffect_BitmapImageTexture.resetFrameBuffer(m_iScreenWidth, m_iScreenHeight);
                        }
                        else
                        {
                            Console.WriteLine("BitmapImageTexture frame buffer exist");
                        }
                    }
                }
            }
        }

        public void setBitmap(System.Drawing.Bitmap bmp)
        {
            m_BmpSrc = bmp;
            setScreenSize(bmp.Width, bmp.Height);
        }

        private void setScreenSize(int w, int h)
        {
            m_iScreenHeight = h;
            m_iScreenWidth = w;
        }

        private void loadTextureFromImage()
        {
            System.Drawing.Bitmap bitmap = m_BmpSrc;
            m_BmpSrc.Save("C:\\temp\\gpu_src.png", System.Drawing.Imaging.ImageFormat.Png);
            if (bitmap == null)
            {
                Console.WriteLine("{0}, {1}", TAG, "Bitmap could not be decoded");

                IQUtil.IQAssert();
            }

            Gl.bindTexture(Gl.TEXTURE_2D, m_SourceTexture[0]);

            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MIN_FILTER, (int)Gl.LINEAR_MIPMAP_LINEAR);

            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MAG_FILTER, (int)Gl.LINEAR);

            //TODO: FIX TEXIMAGE2D
            //Gl.texImage2D(Gl.TEXTURE_2D, 0, bitmap, 0);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.texImage2D(Gl.TEXTURE_2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Rgba, Gl.UNSIGNED_BYTE, data.Scan0);
            bitmap.UnlockBits(data);

            Gl.generateMipmap(Gl.MIPMAPTEXTURE_2D);

            Gl.bindTexture( Gl.TEXTURE_2D, 0);
        }

        public EffectSettingsParameters[] getEffectSettingsParameters()
        {

            return m_EffectSettingsParameters;

        }

        public RendererOptions[] getRendererOptions()
        {
            return m_RendererOptions;
        }

        public RendererStats[] getRendererStats()
        {
            return m_RendererStats;
        }
    }
}
