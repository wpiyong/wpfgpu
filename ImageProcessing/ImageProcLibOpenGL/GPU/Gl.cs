using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ImageProcLibOpenGL.GPU
{
    public static class Gl
    {
        public static TextureUnit TEXTURE0 = TextureUnit.Texture0;// GL_TEXTURE0;
        public static TextureUnit TEXTURE1 = TextureUnit.Texture1;
        public static TextureUnit TEXTURE2 = TextureUnit.Texture2;
        public static TextureTarget TEXTURE_2D = TextureTarget.Texture2D;// GL_TEXTURE_2D;
        public static TextureParameterName TEXTURE_WRAP_S = TextureParameterName.TextureWrapS;
        public static TextureParameterName TEXTURE_WRAP_T = TextureParameterName.TextureWrapT;
        public static TextureWrapMode CLAMP_TO_EDGE = TextureWrapMode.ClampToEdge;
        public static TextureParameterName TEXTURE_MAG_FILTER = TextureParameterName.TextureMagFilter;
        public static TextureParameterName TEXTURE_MIN_FILTER = TextureParameterName.TextureMinFilter;
        public static TextureMinFilter LINEAR = TextureMinFilter.Linear; // GL_LINEAR;
        public static BeginMode TRIANGLES = BeginMode.Triangles; // GL_TRIANGLES;
        public static TextureMinFilter NEAREST = TextureMinFilter.Nearest; // GL_NEAREST;
        public static PixelFormat RGBA = PixelFormat.Rgba; // GL_RGBA;
        public static PixelFormat BGRA = PixelFormat.Bgra; // GL_RGBA;
        public static PixelType UNSIGNED_BYTE = PixelType.UnsignedByte; // GL_UNSIGNED_BYTE;
        public static FramebufferAttachment COLOR_ATTACHMENT0 = FramebufferAttachment.ColorAttachment0; // GL_COLOR_ATTACHMENT0;
        public static FramebufferTarget FRAMEBUFFER = FramebufferTarget.Framebuffer; // GL_FRAMEBUFFER;
        //public static int EXTENSIONS = GL_EXTENSIONS;
        public static ClearBufferMask COLOR_BUFFER_BIT = ClearBufferMask.ColorBufferBit; // GL_COLOR_BUFFER_BIT;
        public static ClearBufferMask DEPTH_BUFFER_BIT = ClearBufferMask.DepthBufferBit; // GL_COLOR_BUFFER_BIT;
        public static ShaderType VERTEX_SHADER = ShaderType.VertexShader;// GL_VERTEX_SHADER;
        public static ShaderType FRAGMENT_SHADER = ShaderType.FragmentShader;// GL_FRAGMENT_SHADER;
        public static ShaderParameter COMPILE_STATUS = ShaderParameter.CompileStatus;// GL_COMPILE_STATUS;
        public static GetProgramParameterName VALIDATE_STATUS = GetProgramParameterName.ValidateStatus; // GL_VALIDATE_STATUS;
        public static GetProgramParameterName LINK_STATUS = GetProgramParameterName.LinkStatus;// GL_LINK_STATUS;
        //	public static int RGB16F					=  GL_RGB16F				;
        public static PixelFormat RGB = PixelFormat.Rgb; // GL_RGB;
        //	public static int HALF_FLOAT				=  GL_HALF_FLOAT			;
        public static FramebufferErrorCode FRAMEBUFFER_COMPLETE = FramebufferErrorCode.FramebufferComplete; // GL_FRAMEBUFFER_COMPLETE;
        public static PixelType FLOAT = PixelType.Float;// GL_FLOAT;
        public static PixelType LOW_FLOAT = PixelType.HalfFloat; // GL_LOW_FLOAT;
        public static TextureMinFilter LINEAR_MIPMAP_LINEAR = TextureMinFilter.LinearMipmapLinear;// GL_LINEAR_MIPMAP_LINEAR;
        public static GenerateMipmapTarget MIPMAPTEXTURE_2D = GenerateMipmapTarget.Texture2D;

        public static void useProgram(int program)
        {
            GL.UseProgram(program);

            IQUtil.checkGlError("glUseProgram");
        }

        public static void uniform1f(int location, float x)
        {
            //GL.Uniform1f(location, x);
            GL.Uniform1(location, x);

            IQUtil.checkGlError("glUniform1f");
        }

        public static int getUniformLocation(int program, String name)
        {
            int iret = GL.GetUniformLocation(program, name);

            IQUtil.checkGlError("glGetUniformLocation");

            return iret;
        }

        public static void activeTexture(TextureUnit texture)
        {
            GL.ActiveTexture(texture);

            IQUtil.checkGlError("glActiveTexture");
        }

        public static void bindTexture(TextureTarget target, int texture)
        {
            GL.BindTexture(target, texture);

            IQUtil.checkGlError("glBindTexture");
        }

        public static void texParameteri(TextureTarget target, TextureParameterName pname, int param)
        {
            GL.TexParameter(target, pname, param);

            IQUtil.checkGlError("glTexParameteri");
        }

        public static void uniform1i(int location, int x)
        {
            GL.Uniform1(location, x);

            IQUtil.checkGlError("glUniform1i");
        }

        public static void uniform4f(int location, double x, double y, double z, double w)
        {
            GL.Uniform4(location, (float)x, (float)y, (float)z, (float)w);

            IQUtil.checkGlError("glUniform4f");
        }

        public static void drawArrays(BeginMode mode, int first, int count)
        {
            GL.DrawArrays(mode, first, count);

            IQUtil.checkGlError("glDrawArrays");
        }

        public static int createFramebuffer()
        {
            int[] iFrameBuffer = new int[1];

            GL.GenFramebuffers(1, iFrameBuffer);

            IQUtil.checkGlError("glGenFramebuffers");

            return iFrameBuffer[0];
        }

        public static void deleteFrameBuffer(int bufferID)
        {
            if(bufferID != 0)
            {
                GL.DeleteFramebuffers(1, ref bufferID);
                IQUtil.checkGlError("glDeleteBuffers");
            }
        }

        public static void framebufferTexture2D(FramebufferTarget iTarget, FramebufferAttachment iAttachment, TextureTarget iTextarget, int iTexture, int iLevel)
        {
            GL.FramebufferTexture2D(iTarget, iAttachment, iTextarget, iTexture, iLevel);

            IQUtil.checkGlError("glFramebufferTexture2D");
        }

        public static void bindFramebuffer(FramebufferTarget iTarget, int iFrameBuffer)
        {
            GL.BindFramebuffer(iTarget, iFrameBuffer);

            IQUtil.checkGlError("glBindFramebuffer");
        }

        public static void texImage2D(TextureTarget iTarget, int iLevel, PixelInternalFormat iInternalformat, int iWidth, int iHeight, int iBorder, PixelFormat iFormat, PixelType iType, IntPtr iPixels)
        {
            GL.TexImage2D(iTarget, iLevel, iInternalformat, iWidth, iHeight, iBorder, iFormat, iType, iPixels);

            IQUtil.checkGlError("glTexImage2D");
        }

        public static int createTexture()
        {
            int[] iRenderTex = new int[1];

            GL.GenTextures(1, iRenderTex);

            IQUtil.checkGlError("glGenTextures");

            return iRenderTex[0];
        }

        public static void deleteTexture(int texID)
        {
            if(texID != 0)
            {
                GL.DeleteTextures(1, ref texID);
                IQUtil.checkGlError("glDeleteTextures");
            }
        }

        public static String glGetString(StringName name)
        {
            String szRet = GL.GetString(name);

            IQUtil.checkGlError("glGetString");

            return szRet;
        }

        public static void viewport(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);

            IQUtil.checkGlError("glViewport");
        }

        public static void clearColor(float red, float green, float blue, float alpha)
        {
            GL.ClearColor(red, green, blue, alpha);

            IQUtil.checkGlError("glClearColor");
        }

        public static void clear(ClearBufferMask mask)
        {
            GL.Clear(mask);

            IQUtil.checkGlError("glClear");
        }

        public static int createShader(ShaderType type)
        {
            int iret = GL.CreateShader(type);

            IQUtil.checkGlError("glCreateShader");

            return iret;
        }

        public static void shaderSource(int shader, string str )
        {
            GL.ShaderSource(shader, str);

            IQUtil.checkGlError("glShaderSource");
        }

        public static void deleteShader(int shader)
        {
            GL.DeleteShader(shader);

            IQUtil.checkGlError("glDeleteShader");
        }

        public static String getShaderInfoLog(int shader)
        {
            String szRet = GL.GetShaderInfoLog(shader);

            IQUtil.checkGlError("glGetShaderInfoLog");

            return szRet;
        }

        public static int createProgram()
        {
            int iret = GL.CreateProgram();

            IQUtil.checkGlError("glCreateProgram");

            return iret;
        }

        public static void attachShader(int program, int shader)
        {
            GL.AttachShader(program, shader);

            IQUtil.checkGlError("glAttachShader");
        }

        public static void linkProgram(int program)
        {
            GL.LinkProgram(program);

            IQUtil.checkGlError("glLinkProgram");
        }

        public static void deleteProgram(int program)
        {
            GL.DeleteProgram(program);

            IQUtil.checkGlError("glDeleteProgram");
        }

        public static void validateProgram(int program)
        {
            GL.ValidateProgram(program);

            IQUtil.checkGlError("glValidateProgram");
        }

        public static String getProgramInfoLog(int program)
        {
            String szRet = GL.GetProgramInfoLog(program);

            IQUtil.checkGlError("getProgramInfoLog");

            return szRet;
        }

        public static void getProgramiv(int program, GetProgramParameterName pname, int[] param)
        {
            GL.GetProgram(program, pname, param);

            IQUtil.checkGlError("glGetProgramiv");
        }

        public static void compileShader(int shader)
        {
            GL.CompileShader(shader);

            IQUtil.checkGlError("glCompileShader");
        }

        public static void getShaderiv(int shader, ShaderParameter pname, int[] param)
        {
            GL.GetShader(shader, pname, param);

            IQUtil.checkGlError("glGetShaderiv");
        }

        public static void readPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
        {
            GL.ReadPixels(x, y, width, height, format, type, pixels);

            IQUtil.checkGlError("glReadPixels");
        }

        public static FramebufferErrorCode checkFramebufferStatus(FramebufferTarget target)
        {

            FramebufferErrorCode iret = GL.CheckFramebufferStatus(target);

            IQUtil.checkGlError("glCheckFramebufferStatus");

            return iret;

        }

        public static void getShaderPrecisionFormat(ShaderType shadertype, ShaderPrecision precisiontype, int[] range, out int precision)
        {
            GL.GetShaderPrecisionFormat(shadertype, precisiontype, range, out precision);
        }

        public static void generateMipmap(GenerateMipmapTarget arg0)
        {
            GL.GenerateMipmap(arg0);

            IQUtil.checkGlError("glGenerateMipmap");
        }
    }
}
