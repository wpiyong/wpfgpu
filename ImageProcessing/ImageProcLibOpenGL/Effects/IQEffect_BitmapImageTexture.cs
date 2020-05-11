using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcLibOpenGL.GPU;

namespace ImageProcLibOpenGL.Effects
{
    public class IQEffect_BitmapImageTexture : IQEffect
    {
        public IQEffect_BitmapImageTexture(EffectInitParameters effectInitParameters) : base(effectInitParameters)
        {
            m_szEffectTag = "IQEffect_BitmapImageTexture";
        }

        public override void updateSettingParameters(EffectSettingsParameters settings)
        {
            Console.WriteLine("Not implemented");
        }

        public override bool loadResources()
        {
            bool bResult = true;

            bResult = base.loadResources("vertexshader2d_v1_0.vsh", "orignalimage_v1_0.frag", m_iVertexShaderId, m_iFragmentShaderId);

            IQUtil.IQAssert(bResult);

            return bResult;
        }

        public override void RenderFrame(int[] src, IQFrameBuffer[] dst)
        {
            int program = m_Program.getProgramID();

            GlHelper.setProgram(program);

            Gl.uniform1i(Gl.getUniformLocation(program, "u_image"), 0);

            int yFlip = Gl.getUniformLocation(program, "u_yflip");

            Gl.uniform1f(yFlip, 1);

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

        //@Override
        public override bool initializeVariables()
        {
            return false;
        }
    }
}
