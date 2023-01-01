using Silk.NET.OpenGL;

namespace RPlay
{

    public class FrameBuffer : IDisposable
    {
        public static GL GL { private set; get; }
        public static void Register(GL gl) => GL = gl;

        private uint _frameBuffer;
        private uint _textureColorBuffer;

        //8192
        public unsafe FrameBuffer(int width, int height)
        {
            _frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBuffer);

                _textureColorBuffer = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _textureColorBuffer);
                
                GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R16i, (uint)width, (uint)height, 0, PixelFormat.RedInteger, 
                    PixelType.Short, null);
                
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Nearest);
                
                GL.BindTexture(TextureTarget.Texture2D, 0);
                
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, 
                    TextureTarget.Texture2D, _textureColorBuffer, 0);
                
                if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
                    Console.WriteLine("ERROR::FRAMEBUFFER:: Framebuffer is not complete!");
                
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void ChangePixel(int x, int y, ushort type)
        {
            Bind();
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, 1, 1, PixelFormat.RedInteger, PixelType.Short, type);
        }
        
        public void SetupFrameBuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBuffer);
        }

        public void EndFrameBuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, _textureColorBuffer);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_frameBuffer);
            GL.DeleteTexture(_textureColorBuffer);
        }
    }
}