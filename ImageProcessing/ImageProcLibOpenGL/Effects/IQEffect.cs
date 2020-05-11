using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcLibOpenGL.GPU;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ImageProcLibOpenGL.Effects
{
    public enum Effects
    {
        NO_EFFECT,
        ABF
    }

    public abstract class IQEffect
    {
        protected int m_iScreenWidth = 0;
        protected int m_iScreenHeight = 0;
        protected ProgramShader m_Program = null;
        protected string m_szShaderVersion = "";
        protected string m_szFragmentShaderName = "";
        protected string m_szVertexShaderName = "";
        protected int m_iFragmentShaderId = 0;
        protected int m_iVertexShaderId = 0;
        protected float m_fAspectRatio = 1.0f;
        protected bool m_bSaveSingleFrameToDisk = false;
        protected string m_szSaveAsFileNamePrefix = "";
        protected string m_szSaveAsFileNameDirectory = "";
        protected string m_szSaveAsFileExtension = "";

        public FrameBufferProp[] m_DefaultFrameBufferProp = null;
        public string m_szEffectTag = "";


        public class IQRectSize
        {
            public int m_iWidth = 0;
            public int m_iHeight = 0;
        }

        public IQEffect(EffectInitParameters effectInitParameters)
        {
            IQUtil.IQAssert(effectInitParameters.iScreenHeight > 0);
            IQUtil.IQAssert(effectInitParameters.iScreenWidth > 0);

            m_iScreenWidth = effectInitParameters.iScreenWidth;
            m_iScreenHeight = effectInitParameters.iScreenHeight;
            m_szShaderVersion = effectInitParameters.szShaderVersion;

            m_DefaultFrameBufferProp = new FrameBufferProp[1];
            m_DefaultFrameBufferProp[0] = new FrameBufferProp();
            m_DefaultFrameBufferProp[0].width = effectInitParameters.iScreenWidth;
            m_DefaultFrameBufferProp[0].height = effectInitParameters.iScreenHeight;
            m_DefaultFrameBufferProp[0].frameBufferType = effectInitParameters.frameBufferType;
        }

        public abstract void updateSettingParameters(EffectSettingsParameters settings);

        public abstract bool loadResources();

        public bool loadResources(string szVertexShaderName, string szFragmentShaderName, int iVertexShaderResourceID, int ifragmentShaderResourceID)
        {
            bool bResult = true;

            m_Program = new ProgramShader(szVertexShaderName, szFragmentShaderName, iVertexShaderResourceID, ifragmentShaderResourceID);

            IQUtil.IQAssert(m_Program != null);

            bResult = m_Program.initProgram();

            IQUtil.IQAssert(bResult);

            if (bResult)
            {
                m_Program.setupPositionTexcoord();

                if (m_DefaultFrameBufferProp[0].width != 0 && m_DefaultFrameBufferProp[0].height != 0)
                {
                    m_DefaultFrameBufferProp[0].m_IQFrameBuffer = new IQFrameBuffer(m_DefaultFrameBufferProp[0].width, m_DefaultFrameBufferProp[0].height, m_DefaultFrameBufferProp[0].frameBufferType);

                    IQUtil.IQAssert(m_DefaultFrameBufferProp[0].m_IQFrameBuffer != null);
                }

                GlHelper.setProgram(m_Program.getProgramID());
                
            }

            return bResult;
        }

        public void resetFrameBuffer(int width, int height)
        {
            if (m_iScreenWidth != width || m_iScreenHeight != height)
            {
                m_iScreenWidth = width;
                m_iScreenHeight = height;

                m_DefaultFrameBufferProp[0].width = width;
                m_DefaultFrameBufferProp[0].height = height;

                Gl.deleteTexture(m_DefaultFrameBufferProp[0].m_IQFrameBuffer.texture);
                Gl.deleteFrameBuffer(m_DefaultFrameBufferProp[0].m_IQFrameBuffer.buffer);

                m_DefaultFrameBufferProp[0].m_IQFrameBuffer = new IQFrameBuffer(m_DefaultFrameBufferProp[0].width, m_DefaultFrameBufferProp[0].height, m_DefaultFrameBufferProp[0].frameBufferType);

                IQUtil.IQAssert(m_DefaultFrameBufferProp[0].m_IQFrameBuffer != null);
            }
        }

        public abstract bool initializeVariables();

        public void setupViewPort(IQPreViewPort[] preViewPort, IQFrameBuffer[] frameBuffer)
        {
            int dstWidth = frameBuffer == null ? m_iScreenWidth : frameBuffer[0].width;
            int dstHeight = frameBuffer == null ? m_iScreenHeight : frameBuffer[0].height;

            //if (preViewPort[0].width != dstWidth || preViewPort[0].height != dstHeight) 
            {
                GlHelper.setupViewPort(0, 0, dstWidth, dstHeight);
                preViewPort[0].width = dstWidth;
                preViewPort[0].height = dstHeight;
            }

        }

        public abstract void RenderFrame(int[] src, IQFrameBuffer[] dst);

        public float getAspectRatio()
        {
            IQUtil.IQAssert(m_iScreenWidth != 0);
            IQUtil.IQAssert(m_iScreenHeight != 0);

            m_fAspectRatio = (float)m_iScreenWidth / (float)m_iScreenHeight;

            return m_fAspectRatio;
        }

        public IQRectSize getScreenDestinationSizeWithAspectRatio()
        {
            IQRectSize dstScreenSize = new IQRectSize();

            dstScreenSize.m_iHeight = 0;

            dstScreenSize.m_iWidth = 0;

            float fAspectRatio = getAspectRatio();

            float fAspectRatio2 = (float)((float)m_iScreenWidth / (float)m_iScreenHeight);

            if (fAspectRatio <= fAspectRatio2)
            {
                dstScreenSize.m_iHeight = m_iScreenHeight;

                dstScreenSize.m_iWidth = (int)(fAspectRatio * ((float)m_iScreenHeight));
            }
            else
            {
                dstScreenSize.m_iWidth = m_iScreenWidth;

                dstScreenSize.m_iHeight = (int)((float)m_iScreenWidth / fAspectRatio);
            }

            return dstScreenSize;
        }

        public void setSaveSingleFrameToDisk(bool bSaveSingleFrameToDisk)
        {
            m_bSaveSingleFrameToDisk = bSaveSingleFrameToDisk;
        }

        //public void setSaveAsImageType(SaveAsImageType saveAsImageType)
        //{
            //m_SaveAsImageType = saveAsImageType;
        //}

        public void setSaveAsFileNamePrefix(string szSaveAsFileNamePrefix)
        {
            m_szSaveAsFileNamePrefix = szSaveAsFileNamePrefix;
        }

        public void setSaveAsFileNameDirectory(string szSaveAsFileNameDirectory)
        {
            m_szSaveAsFileNameDirectory = szSaveAsFileNameDirectory;
        }

        public void setSaveAsFileExtension(string szSaveAsFileExtension)
        {
            m_szSaveAsFileExtension = szSaveAsFileExtension;
        }
    }
}
