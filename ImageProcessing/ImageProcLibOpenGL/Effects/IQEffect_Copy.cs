using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcLibOpenGL.GPU;

namespace ImageProcLibOpenGL.Effects
{
    public class IQEffect_Copy : IQEffect
    {
        IQFrameBuffer[] m_IQSaveAsFullScreenFrameBuffer = null;

        IQFrameBuffer[] m_IQNativeImageFullScreenFrameBuffer = null;

        private int m_flip = 0;

        public IQEffect_Copy(EffectInitParameters effectInitParameters) : base(effectInitParameters)
        {
            m_szEffectTag = "IQEffect_Copy";

            m_IQSaveAsFullScreenFrameBuffer = new IQFrameBuffer[1];

            m_IQNativeImageFullScreenFrameBuffer = new IQFrameBuffer[1];
        }

        public override void updateSettingParameters(EffectSettingsParameters settings)
        {
            Console.WriteLine("Not implemented");
        }

        public override bool loadResources()
        {
            bool bResult = true;


            if (m_szShaderVersion.Equals("1.0"))
            {
                m_szFragmentShaderName = "copy_v1_0";
                m_szVertexShaderName = "vertexshader2d_v1_0";
                //m_iFragmentShaderId = R.raw.copy_v1_0;
                //m_iVertexShaderId = R.raw.vertexshader2d_v1_0;
            }
            else
            {
                IQUtil.IQAssert();
            }

            bResult = base.loadResources(m_szVertexShaderName, m_szFragmentShaderName, m_iVertexShaderId, m_iFragmentShaderId);

            IQUtil.IQAssert(bResult);

            return bResult;
        }

        public override void RenderFrame(int[] src, IQFrameBuffer[] dst)
        {

        }

        public void copyToScreen(int src, IQPreViewPort[] preViewPort, bool bIsFullScreen)
        {
            copyToScreenProcess(src, preViewPort, bIsFullScreen);
        }

        public void copyToScreenProcess(int src, IQPreViewPort[] preViewPort, bool bIsFullScreen)
        {
            int program = m_Program.getProgramID();

            GlHelper.setProgram(program);

            //setupPositionTexcoord(gl, program);

            if (m_bSaveSingleFrameToDisk)
            {
                if (bIsFullScreen)
                {
                    if (m_IQSaveAsFullScreenFrameBuffer[0] == null)
                    {
                        IQRectSize iqRect = getScreenDestinationSizeWithAspectRatio();

                        m_IQSaveAsFullScreenFrameBuffer[0] = new IQFrameBuffer(iqRect.m_iWidth, iqRect.m_iHeight, FrameBufferType.FrameBufferType_UnsignedByte);
                    }

                    GlHelper.bindFrameBuffer(m_IQSaveAsFullScreenFrameBuffer[0].buffer);

                }
                else
                {
                    if (m_IQNativeImageFullScreenFrameBuffer[0] == null)
                    {
                        m_IQNativeImageFullScreenFrameBuffer[0] = new IQFrameBuffer(m_iScreenWidth, m_iScreenHeight, FrameBufferType.FrameBufferType_UnsignedByte);
                    }

                    GlHelper.bindFrameBuffer(m_IQNativeImageFullScreenFrameBuffer[0].buffer);

                }
            }



            if (bIsFullScreen)
            {

                IQRectSize iqRect = getScreenDestinationSizeWithAspectRatio();

                if (m_bSaveSingleFrameToDisk)
                {
                    GlHelper.setupViewPort(0, 0, iqRect.m_iWidth, iqRect.m_iHeight);
                }
                else
                {
                    GlHelper.setupViewPort((m_iScreenWidth - iqRect.m_iWidth) / 2, (m_iScreenHeight - iqRect.m_iHeight) / 2, iqRect.m_iWidth, iqRect.m_iHeight);
                }
            }
            else
            {
                GlHelper.setupViewPort(0, 0, m_iScreenWidth, m_iScreenHeight);

            }

            GlHelper.bindTexture(0, src);


            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_S, (int)Gl.CLAMP_TO_EDGE);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_T, (int)Gl.CLAMP_TO_EDGE);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MAG_FILTER, (int)Gl.LINEAR);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MIN_FILTER, (int)Gl.LINEAR);

            // lookup uniforms
            int flip = m_bSaveSingleFrameToDisk ? 1 : -1;

            if (m_flip != flip)
            {
                m_flip = flip;
                int yFlip = Gl.getUniformLocation(program, "u_yflip");
                // set the resolution

                Gl.uniform1f(yFlip, m_flip);

            }


            Gl.drawArrays(Gl.TRIANGLES, 0, 6);

            if (m_bSaveSingleFrameToDisk)
            {
                if (bIsFullScreen)
                {
                    if (m_bSaveSingleFrameToDisk)
                    {
                        // TODO: add save function
                        //m_IQSaveAsFullScreenFrameBuffer[0].saveFrameBufferToImage(m_SaveAsImageType, m_szSaveAsFileNamePrefix,
                        //                                     m_szSaveAsFileNameDirectory, m_szSaveAsFileExtension,
                        //                                     m_szSaveAsFileTag);

                        m_bSaveSingleFrameToDisk = false;
                    }

                }
                else
                {
                    if (m_bSaveSingleFrameToDisk)
                    {
                        //TODO: add save function
                        //m_IQNativeImageFullScreenFrameBuffer[0].saveFrameBufferToImage(m_SaveAsImageType, m_szSaveAsFileNamePrefix,
                        //                                          m_szSaveAsFileNameDirectory, m_szSaveAsFileExtension,
                        //                                          m_szSaveAsFileTag);

                        m_bSaveSingleFrameToDisk = false;
                    }
                }

                //glHelper.bindTexture(dst[0].texture);

                GlHelper.bindFrameBuffer(0);
            }

        }

        public override bool initializeVariables()
        {

            int copyprogram = m_Program.getProgramID();

            GlHelper.setProgram(copyprogram);

            Gl.uniform1i(Gl.getUniformLocation(copyprogram, "u_image"), 0);

            return true;
        }
    }
}
