using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Util
{
    public class EffectSettingsParameters
    {
        public float m_fQPScale_L;
        public float m_fQPScale_A;
        public float m_fQPScale_B;
        public float m_fImageIQ_G;
        public float m_fImageIQ_C;
        public float m_fImageIQ_P;
        public int m_iIbrWidth;
        public int m_iIbrHeight;
        public int m_iNativeHeight;
        public int m_iNativeWidth;
        public String m_szVersionNumber;

        public EffectSettingsParameters()
        {
            m_fQPScale_L = 0.0f;

            m_fQPScale_A = 0.0f;

            m_fQPScale_B = 0.0f;

            m_fImageIQ_G = 0.0f;

            m_fImageIQ_C = 0.0f;

            m_fImageIQ_P = 0.0f;

            m_iIbrWidth = 0;

            m_iIbrHeight = 0;

            m_iNativeHeight = 0;

            m_iNativeWidth = 0;

            m_szVersionNumber = "";

        }
    }
}
