using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Util
{
    public static class GlHelper
    {
        static private int m_iCurrentProgramId = -1;
        static private int m_iCurrentFrameBufferId = -1;
        static private int m_iCurrentActiveTextureId = -1;
        static private int[] m_iCurrentTextureId = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        static private int m_iCurrentViewPortX = -1;
        static private int m_iCurrentViewPortY = -1;
        static private int m_iCurrentViewPortWidth = -1;
        static private int m_iCurrentViewPortHeight = -1;

        static public void resetValuesToDefault()
        {
            m_iCurrentProgramId = -1;
            m_iCurrentFrameBufferId = -1;
            m_iCurrentActiveTextureId = -1;
            m_iCurrentTextureId = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            m_iCurrentViewPortX = -1;
            m_iCurrentViewPortY = -1;
            m_iCurrentViewPortWidth = -1;
            m_iCurrentViewPortHeight = -1;

        }

        static public void setProgram(int iProgramId)
        {
            if (m_iCurrentProgramId != iProgramId)
            {
                m_iCurrentProgramId = iProgramId;
                Gl.useProgram(iProgramId);
            }
        }

        static public void bindFrameBuffer(int iFrameBufferId)
        {
            if (m_iCurrentFrameBufferId != iFrameBufferId)
            {
                m_iCurrentFrameBufferId = iFrameBufferId;
                Gl.bindFramebuffer(Gl.FRAMEBUFFER, iFrameBufferId);
            }
        }

        static public void bindTexture(int iActiveTexture, int iTextureId)
        {
            if (m_iCurrentActiveTextureId != iActiveTexture)
            {
                m_iCurrentActiveTextureId = iActiveTexture;

                if (m_iCurrentActiveTextureId == 0)
                {
                    Gl.activeTexture(Gl.TEXTURE0);
                }
                else if (m_iCurrentActiveTextureId == 1)
                {
                    Gl.activeTexture(Gl.TEXTURE1);
                }
                else if (m_iCurrentActiveTextureId == 2)
                {
                    Gl.activeTexture(Gl.TEXTURE2);
                }
                else
                {
                    IQUtil.IQAssert();
                }
            }

            if (m_iCurrentTextureId[iActiveTexture] != iTextureId)
            {
                m_iCurrentTextureId[iActiveTexture] = iTextureId;
                Gl.bindTexture(Gl.TEXTURE_2D, iTextureId);
            }
        }

        static public void setupViewPort(int x, int y, int width, int height)
        {
            if (m_iCurrentViewPortX != x || m_iCurrentViewPortY != y ||
                    m_iCurrentViewPortWidth != width || m_iCurrentViewPortHeight != height)
            {
                m_iCurrentViewPortX = x;
                m_iCurrentViewPortY = y;
                m_iCurrentViewPortWidth = width;
                m_iCurrentViewPortHeight = height;

                Gl.viewport(m_iCurrentViewPortX, m_iCurrentViewPortY, m_iCurrentViewPortWidth, m_iCurrentViewPortHeight);
            }
        }
    }
}
