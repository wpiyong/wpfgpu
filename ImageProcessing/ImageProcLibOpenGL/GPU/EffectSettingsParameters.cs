using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcLibOpenGL.GPU
{
    public class EffectSettingsParameters
    {
        public float m_ABFTheta;

        public String m_szEffect;

        public EffectSettingsParameters()
        {
            m_ABFTheta = 0;
            m_szEffect = "";
        }
    }
}
