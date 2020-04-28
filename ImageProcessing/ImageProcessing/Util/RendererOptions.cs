using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Util
{
    public enum SaveAsImageType
    {
        SaveAsImageType_None,
        SaveAsImageType_PNG,
        SaveAsImageType_JPG,
        SaveAsImageType_BMP,
        SaveAsImageType_TextureImage
    }

    public class RendererOptions
    {
        public bool m_bIsFullScreen;
        public bool m_bIsPlayOriginalFile;
        public bool m_bSaveSingleFrameToDisk;
        public string m_szSaveAsFileNamePrefix;
        public string m_szSaveAsFileNameDirectory;
        public string m_szSaveAsFileExtension;
        public SaveAsImageType m_SaveAsImageType;
        public bool m_bDisplayOrignalFile;
        public bool m_bEnableImageIQEffect;
        public bool m_bEnableIBREffect;
        public bool m_bEnableLancozxEffect;
        public bool m_bEnableLancozxHQEffect;
        public bool m_bEnableHybridEffect;
        public bool m_bEnableHybridHQEffect;
        public bool m_bResetValuesToDefault;

        public RendererOptions()
        {
            m_bDisplayOrignalFile = false;

            m_bEnableImageIQEffect = false;

            m_bEnableIBREffect = false;

            m_bEnableLancozxEffect = false;

            m_bEnableHybridEffect = false;

            m_bEnableLancozxHQEffect = false;

            m_bEnableHybridHQEffect = false;

            m_bIsFullScreen = false;

            m_bIsPlayOriginalFile = false;

            m_bSaveSingleFrameToDisk = false;

            m_SaveAsImageType = SaveAsImageType.SaveAsImageType_None;

            m_szSaveAsFileNamePrefix = "";

            m_szSaveAsFileNameDirectory = "";

            m_szSaveAsFileExtension = "";

            m_bResetValuesToDefault = false; ;

        }
    }
}
