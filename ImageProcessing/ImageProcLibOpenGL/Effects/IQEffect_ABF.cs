using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcLibOpenGL.GPU;

namespace ImageProcLibOpenGL.Effects
{
    public class IQEffect_ABF : IQEffect
    {
        public float m_fTheta = 0.75f;

        public IQEffect_ABF(EffectInitParameters effectInitParameters) : base(effectInitParameters)
        {
            m_szEffectTag = "IQEffect_ABF";
        }

        public override bool initializeVariables()
        {
            return false;
        }

        public override bool loadResources()
        {
            bool bResult = true;

            bResult = base.loadResources("vertexshader2d_v1_0.vsh", "abf_v1_0.frag", m_iVertexShaderId, m_iFragmentShaderId);

            IQUtil.IQAssert(bResult);

            return bResult;
        }

        public override void updateSettingParameters(EffectSettingsParameters settings)
        {
            m_fTheta = settings.m_ABFTheta;
        }

        public override void RenderFrame(int[] src, IQFrameBuffer[] dst)
        {
            int program = m_Program.getProgramID();
            
            GlHelper.setProgram(program);

            Gl.uniform1i(Gl.getUniformLocation(program, "texture0"), 0);
            Gl.uniform1i(Gl.getUniformLocation(program, "texture1"), 1);

            int yFlip = Gl.getUniformLocation(program, "u_yflip");
            Gl.uniform1f(yFlip, 1);

            Gl.uniform4f(Gl.getUniformLocation(program, "SrcSampleSteps"), m_iScreenWidth, m_iScreenHeight, 1.0 / (m_iScreenWidth), 1.0 / (m_iScreenHeight));
            Gl.uniform4f(Gl.getUniformLocation(program, "DstSampleSteps"), m_iScreenWidth, m_iScreenHeight, 1.0 / (m_iScreenWidth), 1.0 / (m_iScreenHeight));
            Gl.uniform4f(Gl.getUniformLocation(program, "channelSelect"), 1.0f, 1.0f, 1.0f, 1.0f);
            Gl.uniform4f(Gl.getUniformLocation(program, "contrast"), 0.3f, 0.0f, 0.0f, 0.0f);
            Gl.uniform4f(Gl.getUniformLocation(program, "bilateralParams"), 1.1f, 7.0f, m_fTheta, 1.0f);

            GlHelper.setupViewPort(0, 0, dst[0].width, dst[0].height);

            GlHelper.bindFrameBuffer(dst[0].buffer);

            GlHelper.bindTexture(0, src[0]);
            GlHelper.bindTexture(1, src[1]);

            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_S, (int)Gl.CLAMP_TO_EDGE);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_WRAP_T, (int)Gl.CLAMP_TO_EDGE);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MAG_FILTER, (int)Gl.NEAREST);
            Gl.texParameteri(Gl.TEXTURE_2D, Gl.TEXTURE_MIN_FILTER, (int)Gl.NEAREST);

            Gl.drawArrays(Gl.TRIANGLES, 0, 6);

            GlHelper.bindTexture(0, 0);
            GlHelper.bindTexture(1, 0);

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
