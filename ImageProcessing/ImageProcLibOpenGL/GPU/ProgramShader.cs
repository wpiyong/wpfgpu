using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ImageProcLibOpenGL.GPU
{
    public class ProgramShader
    {
        private static string TAG = "ProgramShader";
	
	    private int m_iProgramID;
        private int m_iVertexShaderResourceID;
        private int m_iFragmentShaderResourceID;
        private string m_szVertexShaderName;
        private string m_szFragmentShaderName;
        private int m_a_position;
        private int m_a_texCoord;
        private int m_VertexBufferObject;
        private int m_TexCoordBufferObject;

        private static float[] fPositionBufferVertex = {
            -1.0f,  -1.0f,
             1.0f,  -1.0f,
            -1.0f,   1.0f,
            -1.0f,   1.0f,
             1.0f,  -1.0f,
             1.0f,   1.0f };

        private static float[] fTexCoordBufferVertex = {
            0.0f, 0.0f,
            1.0f, 0.0f,
            0.0f, 1.0f,
            0.0f, 1.0f,
            1.0f, 0.0f,
            1.0f, 1.0f };

        public ProgramShader(string szVertexShaderName, string szFragmentShaderName, int iVertexShaderResourceID, int ifragmentShaderResourceID)
        {
            m_iProgramID = -1;

            m_iVertexShaderResourceID = iVertexShaderResourceID;

            m_iFragmentShaderResourceID = ifragmentShaderResourceID;

            m_szVertexShaderName = szVertexShaderName;

            m_szFragmentShaderName = szFragmentShaderName;

            m_VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, fPositionBufferVertex.Length * sizeof(float), fPositionBufferVertex, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            IQUtil.checkGlError("ArrayBuffer");

            m_TexCoordBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_TexCoordBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, fTexCoordBufferVertex.Length * sizeof(float), fTexCoordBufferVertex, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            IQUtil.checkGlError("ArrayBuffer");
        }

        public bool initProgram()
        {
            bool bResult = false;
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine("dll location: {0}", assemblyFolder);

            Console.WriteLine("{0}, {1}",TAG, "Compiling  " + m_szVertexShaderName + " / " + m_szFragmentShaderName);

            //string szVertexShaderSource = File.ReadAllText(@".\Shaders\" + m_szVertexShaderName);
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            string name = string.Format("{0}.{1}.{2}", "ImageProcLibOpenGL", "Shaders", m_szVertexShaderName);
            string szVertexShaderSource = ReadResource(name);

            IQUtil.IQAssert(szVertexShaderSource != null);

            //string szFragmentShaderSource = File.ReadAllText(@".\Shaders\" + m_szFragmentShaderName);
            name = string.Format("{0}.{1}.{2}", "ImageProcLibOpenGL", "Shaders", m_szFragmentShaderName);
            string szFragmentShaderSource = ReadResource(name);

            IQUtil.IQAssert(szFragmentShaderSource != null);

            m_iProgramID = ShaderHelper.buildProgram(szVertexShaderSource, szFragmentShaderSource);

            IQUtil.IQAssert(m_iProgramID != -1);

            bResult = (m_iProgramID != 0);

            if (bResult == false)
            {
                string szMessage = string.Format("{0} error compiling the {1} / {2} shaders", TAG, m_szVertexShaderName, m_szFragmentShaderName);

                Console.WriteLine("{0}, {1}", TAG, szMessage);

                IQUtil.IQAssert();
            }

            return bResult;

        }

        public string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public int getProgramID()
        {
            return m_iProgramID;
        }

        public void setupPositionTexcoord()
        {
            m_a_position = GL.GetAttribLocation(m_iProgramID, "a_position");

            m_a_texCoord = GL.GetAttribLocation(m_iProgramID, "a_texCoord");

            GL.EnableVertexAttribArray(m_a_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_VertexBufferObject);
            GL.VertexAttribPointer(m_a_position, 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.EnableVertexAttribArray(m_a_texCoord);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_TexCoordBufferObject);
            GL.VertexAttribPointer(m_a_texCoord, 2, VertexAttribPointerType.Float, false, 0, 0); 

        }
    }
}
