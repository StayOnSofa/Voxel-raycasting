using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RPlay
{

    public class Shader
    {
        public readonly uint Program;
        private readonly Dictionary<string, int> _uniformLocations;

        public static GL GL { private set; get; }

        public static void Register(GL gl)
        {
            GL = gl;
        }

        public Shader(string vertex, string fragment)
        {
            uint vertexShader = GL.CreateShader(ShaderType.VertexShader);
            uint fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        
            GL.ShaderSource(vertexShader, ShaderFile.GetShader(vertex));
            GL.ShaderSource(fragmentShader, ShaderFile.GetShader(fragment));
     
            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);
    
            Program = GL.CreateProgram();
            GL.AttachShader(Program, vertexShader);
            GL.AttachShader(Program, fragmentShader);
      
            GL.LinkProgram(Program);
            GL.DetachShader(Program, vertexShader);
            GL.DetachShader(Program, fragmentShader);
     
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            GL.GetProgram( Program, GLEnum.LinkStatus, out var status );
            if ( status == 0 ) {
                Console.WriteLine( $"Error linking shader {GL.GetProgramInfoLog(Program)}" );
            }
            
            _uniformLocations = new Dictionary<string, int>();

            GL.GetProgram(Program, GLEnum.ActiveUniforms, out var numberOfUniforms);
            
            Console.WriteLine(numberOfUniforms);
            
            for (uint i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Program, i, out _, out _);
                var location = GL.GetUniformLocation(Program, key);
                
                _uniformLocations.Add(key, location);
            }
        }
        
        public void Use()
        {
            GL.UseProgram(Program);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Program, attribName);
        }
        
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Program);
            GL.Uniform1(_uniformLocations[name], data);
        }
        
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Program);
            GL.Uniform1(_uniformLocations[name], data);
        }
        
        public void SetFloat(string name, double data)
        {
            SetFloat(name, (float) data);
        }
        
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Program);
            GL.Uniform3(_uniformLocations[name], data);
        }
        
        public void SetVector3i(string name, int x, int y, int z)
        {
            GL.UseProgram(Program);
            GL.Uniform3(_uniformLocations[name], x, y, z);
        }

        public void SetVector3i(string name, Vector3D<int> data)
        {
            SetVector3i(name, data.X, data.Y, data.Z);
        }
        
        public void SetVector3(string name, Vector2 data)
        {
            SetVector3(name, new Vector3(data.X, data.Y, 0));
        }

        public void SetUniform(string name, int value)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            GL.Uniform1(location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            GL.Uniform1(location, value);
        }
    }
}