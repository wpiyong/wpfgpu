using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ImageProcLibOpenGL.GPU
{
    public static class ShaderHelper
    {
        private static string TAG = "ShaderHelper";
	
        public static int compileVertexShader(string szShaderCode)
        {
            IQUtil.IQLog(TAG, "Compiling Vertex Shader" + "\n", false);

            return compileShader(szShaderCode, Gl.VERTEX_SHADER);
        }

        public static int compileFragmentShader(string szShaderCode)
        {
            IQUtil.IQLog(TAG, "Compiling Fragment Shader" + "\n", false);

            return compileShader(szShaderCode, ShaderType.FragmentShader);
        }

        private static int compileShader(string szShaderCode, ShaderType iShaderType)
        {
            int[] iCompileStatus = new int[1];

            int iShaderObjectID = Gl.createShader(iShaderType);

            if (iShaderObjectID == 0)
            {
                string szMessage = string.Format("{0} Could not create new shader", TAG);

                IQUtil.IQLog(TAG, szMessage, true);

                IQUtil.IQAssert();
            }

            Gl.shaderSource(iShaderObjectID, szShaderCode);

            Gl.compileShader(iShaderObjectID);

            //Gl.GetShaderiv(iShaderObjectID, Gl.COMPILE_STATUS, iCompileStatus, 0);
            Gl.getShaderiv(iShaderObjectID, ShaderParameter.CompileStatus, iCompileStatus);

            IQUtil.IQLog(TAG, "Results of compiling source: " + iCompileStatus[0] + " -->" + Gl.getShaderInfoLog(iShaderObjectID) + "\n", false);

            if (iCompileStatus[0] == 0)
            {

                Gl.deleteShader(iShaderObjectID);

                string szMessage = string.Format("{0} Compilation of shader failed", TAG);

                IQUtil.IQLog(TAG, szMessage, false);

                IQUtil.IQAssert();
            }
            return iShaderObjectID;
        }

        public static int linkProgram(int iVertexShaderID, int iFragmentShaderID)
        {
            int[] iLinkStatus = new int[1];

            int iProgramObjectID = Gl.createProgram();

            IQUtil.IQLog(TAG, "Linking Vertex and Fragment Shaders" + "\n", false);

            if (iProgramObjectID == 0)
            {

                string szMessage = string.Format("{0} Could not create new program", TAG);

                IQUtil.IQLog(TAG, szMessage, true);

                IQUtil.IQAssert();

            }

            Gl.attachShader(iProgramObjectID, iVertexShaderID);

            IQUtil.checkGlError("glAttachShader");

            Gl.attachShader(iProgramObjectID, iFragmentShaderID);

            IQUtil.checkGlError("glAttachShader");

            Gl.linkProgram(iProgramObjectID);

            //Gl.GetProgramiv(iProgramObjectID, Gl.LINK_STATUS, iLinkStatus, 0);
            Gl.getProgramiv(iProgramObjectID, GetProgramParameterName.LinkStatus, iLinkStatus);

            IQUtil.IQLog(TAG, "Results of linking program: " + iLinkStatus[0] + " --> " + Gl.getProgramInfoLog(iProgramObjectID), false);

            if (iLinkStatus[0] == 0)
            {
                Gl.deleteProgram(iProgramObjectID);

                string szMessage = string.Format("{0} Linking of program failed", TAG);

                IQUtil.IQLog(TAG, szMessage, true);

                IQUtil.IQAssert();
            }

            return iProgramObjectID;
        }

        public static bool validateProgram(int iProgramObjectID)
        {
            int[] iValidateStatus = new int[1];

            IQUtil.IQLog(TAG, "Validating Program" + "\n", false);

            Gl.validateProgram(iProgramObjectID);

            //Gl.getProgramiv(iProgramObjectID, Gl.VALIDATE_STATUS, iValidateStatus, 0);
            Gl.getProgramiv(iProgramObjectID, GetProgramParameterName.ValidateStatus, iValidateStatus);

            IQUtil.IQLog(TAG, "Results of validating program: " + iValidateStatus[0] + "-->" + Gl.getProgramInfoLog(iProgramObjectID), false);

            IQUtil.IQAssert(iValidateStatus[0] != 0);

            return iValidateStatus[0] != 0;
        }

        public static int buildProgram(string szVertexShaderSource, string szFragmentShaderSource)
        {
            int iProgramID = 0;
            int iVertexShaderID = 0;
            int iFragmentShaderID = 0;

            iVertexShaderID = compileVertexShader(szVertexShaderSource);

            if (iVertexShaderID != 0)
            {
                iFragmentShaderID = compileFragmentShader(szFragmentShaderSource);

                if (iFragmentShaderID != 0)
                {
                    iProgramID = linkProgram(iVertexShaderID, iFragmentShaderID);

                    if (iProgramID != 0)
                    {
                        bool iResult = validateProgram(iProgramID);

                        if (iResult == false)
                        {
                            iProgramID = 0;

                            IQUtil.IQAssert();
                        }
                    }
                    else
                    {
                        IQUtil.IQAssert();
                    }
                }
                else
                {
                    IQUtil.IQAssert();
                }

            }

            return iProgramID;
        }
    }
}
