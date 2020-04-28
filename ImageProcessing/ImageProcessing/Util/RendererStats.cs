using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Util
{
    public class RendererStats
    {
        public float m_fAverageFPS;
        public float m_fInstantFPS;
        public long m_iMovieWidth;
        public long m_iMovieHeight;
        public long m_iDisplayWidth;
        public long m_iDisplayHeight;
        public long m_iIbrWidth;
        public long m_iIbrHeight;
        public long m_iNativeWidth;
        public long m_iNativeHeight;
        public float m_fVersion;
        public long m_iTotalFrameCount;

        public RendererStats()
        {
            m_fAverageFPS = 0;

            m_fInstantFPS = 0;

            m_iMovieWidth = 0;

            m_iMovieHeight = 0;

            m_iDisplayWidth = 0;

            m_iDisplayHeight = 0;

            m_iTotalFrameCount = 0;

            m_iIbrWidth = 0;

            m_iIbrHeight = 0;

            m_iNativeWidth = 0;

            m_iNativeHeight = 0;

            m_fVersion = 0.0f;

        }
    }
}
