using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessing.Util;

namespace ImageProcessing.Effects
{
    public class IQEffect_LOG : IQEffect
    {
        public IQEffect_LOG(EffectInitParameters effectInitParameters) : base(effectInitParameters)
        {
            m_szEffectTag = "IQEffect_LOG";
        }

        public override bool initializeVariables()
        {
            return false;
        }

        public override bool loadResources()
        {
            bool bResult = true;

            bResult = base.loadResources("vertexshader2d_v1_0.vsh", "log_v1_0.frag", m_iVertexShaderId, m_iFragmentShaderId);

            IQUtil.IQAssert(bResult);

            return bResult;
        }

        public override void RenderFrame(int[] src, IQFrameBuffer[] dst)
        {
            int program = m_Program.getProgramID();

            GlHelper.setProgram(program);

            Gl.uniform1i(Gl.getUniformLocation(program, "texture0"), 0);

            int yFlip = Gl.getUniformLocation(program, "u_yflip");
            Gl.uniform1f(yFlip, 1);

            Gl.uniform4f(Gl.getUniformLocation(program, "SrcSampleSteps"), m_iScreenWidth, m_iScreenHeight, 1.0 / (m_iScreenWidth), 1.0 / (m_iScreenHeight));
            Gl.uniform4f(Gl.getUniformLocation(program, "DstSampleSteps"), m_iScreenWidth, m_iScreenHeight, 1.0 / (m_iScreenWidth), 1.0 / (m_iScreenHeight));
            Gl.uniform4f(Gl.getUniformLocation(program, "channelSelect"), 1.0f, 1.0f, 1.0f, 1.0f);

            GlHelper.setupViewPort(0, 0, dst[0].width, dst[0].height);

            GlHelper.bindFrameBuffer(dst[0].buffer);

            GlHelper.bindTexture(0, src[0]);

            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_S, (int)Gl.CLAMP_TO_EDGE);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_T, (int)Gl.CLAMP_TO_EDGE);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MAG_FILTER, (int)Gl.NEAREST);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MIN_FILTER, (int)Gl.NEAREST);

            Gl.drawArrays(Gl.TRIANGLES, 0, 6);

            GlHelper.bindTexture(0, 0);

            if (m_bSaveSingleFrameToDisk)
            {
                //dst[0].saveFrameBufferToImage(m_SaveAsImageType, m_szSaveAsFileNamePrefix,
                //                       m_szSaveAsFileNameDirectory, m_szSaveAsFileExtension,
                //                       m_szSaveAsFileTag);

                m_bSaveSingleFrameToDisk = false;
            }

            GlHelper.bindFrameBuffer(0);
        }
    }
}
