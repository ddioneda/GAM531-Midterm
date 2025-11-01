using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.IO;

namespace WindowEngine
{
    public class Shader : IDisposable
    {
        private readonly int _handle;

        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            string vertexSource = LoadShaderFromFile(vertexShaderPath);
            string fragmentSource = LoadShaderFromFile(fragmentShaderPath);

            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            _handle = GL.CreateProgram();
            GL.AttachShader(_handle, vertexShader);
            GL.AttachShader(_handle, fragmentShader);
            GL.LinkProgram(_handle);
            CheckProgram(_handle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private string LoadShaderFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Shader file not found: {filePath}");

            return File.ReadAllText(filePath);
        }

        public void Use()
        {
            GL.UseProgram(_handle);
        }

        public void SetMatrix4(string name, ref Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(_handle, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(string name, Vector3 value)
        {
            int location = GL.GetUniformLocation(_handle, name);
            GL.Uniform3(location, value);
        }

        public void SetFloat(string name, float value)
        {
            int location = GL.GetUniformLocation(_handle, name);
            GL.Uniform1(location, value);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                throw new Exception($"Shader compilation error: {GL.GetShaderInfoLog(shader)}");
            return shader;
        }

        private void CheckProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
                throw new Exception($"Program linking error: {GL.GetProgramInfoLog(program)}");
        }

        public void SetBool(string name, bool value)
        {
            int location = GL.GetUniformLocation(_handle, name);
            GL.Uniform1(location, value ? 1 : 0);
        }

        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(_handle, name);
            GL.Uniform1(location, value);
        }

        public void Dispose()
        {
            GL.DeleteProgram(_handle);
        }
    }
}