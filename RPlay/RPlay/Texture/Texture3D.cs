using Silk.NET.OpenGL;

namespace RPlay
{

    public class Texture3D : IDisposable
    {
        public static string Path => Application.Path + "/Voxels/";
        
        public static GL GL { private set; get; }
        public static void Register(GL gl) => GL = gl;

        private uint _handle;
        
        public Texture3D(int width, int height, int depth, ReadOnlySpan<ushort> dataSpan)
        {
            _handle = GL.GenTexture();
            Bind();
            
            GL.TexImage3D(TextureTarget.Texture3D, 0, InternalFormat.R16i, (uint)width, (uint)height, (uint)depth, 0, PixelFormat.RedInteger, 
                PixelType.Short, dataSpan);
            
            SetParameters();
        }

        public unsafe Texture3D(int width, int height, int depth)
        {
            _handle = GL.GenTexture();
            Bind();
            
            GL.TexImage3D(TextureTarget.Texture3D, 0, InternalFormat.R16i, (uint)width, (uint)height, (uint)depth, 0, PixelFormat.RedInteger, 
                PixelType.Short, null);
            
            SetParameters();
        }

        private void SetParameters()
        {
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
        }
        
        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture3D, _handle);
        }

        public uint GetHandle() => _handle;

        public void ChangePixel(int x, int y, int z, ushort type)
        {
            Bind();
            GL.TexSubImage3D(TextureTarget.Texture3D, 0, x, y, z, 1, 1, 1, PixelFormat.RedInteger, PixelType.Short, type);
        }

        public void Dispose()
        {
            GL.DeleteTexture(_handle);
        }
    }
}