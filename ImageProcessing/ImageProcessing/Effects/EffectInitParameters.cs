using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessing.Util;

namespace ImageProcessing.Effects
{
    public class EffectInitParameters
    {
        public int iScreenWidth;
        public int iScreenHeight;
        public string szShaderVersion;
        public FrameBufferType frameBufferType;
        public EffectInitParameters()
        {
            resetValues();
        }

        public void resetValues()
        {
            iScreenWidth = 0;
            iScreenHeight = 0;
            szShaderVersion = "";
            frameBufferType = FrameBufferType.FrameBufferType_None;
        }
    }
}
