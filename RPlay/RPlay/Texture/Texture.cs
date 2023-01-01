using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RPlay
{

    public class Texture : IDisposable
    {
        private static string Path => Application.Path + "Textures/";
        
        public static GL GL { private set; get; }

        public static void Register(GL gl)
        {
            GL = gl;
        }

        private uint _handle;
        private bool _linear;

     public unsafe Texture(string path, bool linear = true)
     {
            _linear = linear;
            _handle = GL.GenTexture();
            Bind();
            
            using (var img = Image.Load<Rgba32>(Path + path))
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
                
                img.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        fixed (void* data = accessor.GetRowSpan(y))
                        {
                            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint) accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                        }
                    }
                });
            }

            SetParameters();
        }

        public unsafe Texture(Span<byte> data, uint width, uint height)
        {
            _handle = GL.GenTexture();
            Bind();
            
            fixed (void* d = &data[0])
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                SetParameters();
            }
        }

        private void SetParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.MirrorClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.MirrorClampToEdge);

            if (_linear)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) GLEnum.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) GLEnum.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Nearest);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public uint GetHandle() => _handle;
        
        public void Dispose()
        {
            GL.DeleteTexture(_handle);
        }
    }
}