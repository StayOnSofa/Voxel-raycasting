using Silk.NET.OpenGL;

namespace RPlay.Primitives
{

    public static class ScreenBuffer
    {
        private static uint _vao;
        public static GL GL { private set; get; }

        public static unsafe void Register(GL gl)
        {
            GL = gl;

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            uint vertices = GL.GenBuffer();

            float p = 1f;

            float[] verticesArray =
            {
                p, p, 1.0f, // 0
                p, -p, 1.0f, // 1
                -p, p, 1.0f, // 3
                p, -p, 1.0f, // 1
                -p, -p, 1.0f, // 2
                -p, p, 1.0f, // 3
            };

            GL.BindBuffer(GLEnum.ArrayBuffer, vertices);
            GL.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>) verticesArray.AsSpan(), GLEnum.StaticDraw);
            GL.VertexAttribPointer(0, 3, GLEnum.Float, false, 0, null);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        public static void DrawScreenBuffer()
        {
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}