using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcLibOpenGL.GPU
{
    public enum FrameBufferType
    {
        FrameBufferType_None,
        FrameBufferType_UnsignedByte,
        FrameBufferType_Float,
    };

    public class FrameBufferProp
    {
        public int width;
        public int height;
        public FrameBufferType frameBufferType;
        public IQFrameBuffer m_IQFrameBuffer;

        public FrameBufferProp()
        {
            width = 0;
            height = 0;
            frameBufferType = FrameBufferType.FrameBufferType_None;
            m_IQFrameBuffer = null;
        }
    }
}
