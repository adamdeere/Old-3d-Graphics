using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Labs.Utility
{
    public class ShaderUtility
    {
        public int ShaderProgramID { get; private set; }
        public int VertexShaderID { get; private set; }
        public int FragmentShaderID { get; private set; }

        //Takes in two strings, these should be file paths for a vertex shader and a
        //fragment shader pair
        public ShaderUtility(string pVertexShaderFile, string pFragmentShaderFile)
        {
            StreamReader reader;
            //This creates a vertex shader and stores the ID
            VertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            //Next the f8ile is read
            reader = new StreamReader(pVertexShaderFile);
            //The contents are then sent to the shader ID as the source code
            GL.ShaderSource(VertexShaderID, reader.ReadToEnd());
            //The reader is then closed
            reader.Close();
            //Then the shader is compiled
            GL.CompileShader(VertexShaderID);

            int result;
            //This then checks if the shader compiled successfully, if not an exception
            //is thrown
            GL.GetShader(VertexShaderID, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                throw new Exception("Failed to compile vertex shader!" + GL.GetShaderInfoLog(VertexShaderID));
            }

            //The fragment shader is processed in the same way as above
            FragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            reader = new StreamReader(pFragmentShaderFile);
            GL.ShaderSource(FragmentShaderID, reader.ReadToEnd());
            reader.Close();
            GL.CompileShader(FragmentShaderID);

            GL.GetShader(FragmentShaderID, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                throw new Exception("Failed to compile fragment shader!" + GL.GetShaderInfoLog(FragmentShaderID));
            }

            //After this the shader program is created
            ShaderProgramID = GL.CreateProgram();
            //The vertex and fragment shaders are attached to the shader program
            GL.AttachShader(ShaderProgramID, VertexShaderID);
            GL.AttachShader(ShaderProgramID, FragmentShaderID);
            //Then the shader program is linked, meaning that the output of the vertex
            //shader is linked to the input of the fragment shader, which must match one
            //another
            GL.LinkProgram(ShaderProgramID);
        }

        public void Delete()
        {
            //This detaches the shader program from the shader and deletes them all
            GL.DetachShader(ShaderProgramID, VertexShaderID);
            GL.DetachShader(ShaderProgramID, FragmentShaderID);
            GL.DeleteShader(VertexShaderID);
            GL.DeleteShader(FragmentShaderID);
            GL.DeleteProgram(ShaderProgramID);
        }
    }
}
