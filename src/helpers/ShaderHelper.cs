using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenTK_Sprite_Animation
{
    /// <summary>
    /// Helper class for shader compilation and program linking
    /// </summary>
    public static class ShaderHelper
    {
        public static int CreateShaderProgram()
        {
            string vertexShader = @"
            #version 330 core
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aTexCoord;
            out vec2 vTexCoord;
            uniform mat4 projection;
            uniform mat4 model;
            void main() {
                gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
                vTexCoord = vec2(aTexCoord.x, 1.0 - aTexCoord.y);
            }";

            string fragmentShader = @"
            #version 330 core
            in vec2 vTexCoord;
            out vec4 color;
            uniform sampler2D uTexture;
            uniform vec2 uOffset;
            uniform vec2 uSize;
            void main() {
                vec2 uv = uOffset + vTexCoord * uSize;
                color = texture(uTexture, uv);
            }";

            int vs = CompileShader(ShaderType.VertexShader, vertexShader);
            int fs = CompileShader(ShaderType.FragmentShader, fragmentShader);
            int program = LinkProgram(vs, fs);

            // Cleanup shader objects after linking
            GL.DetachShader(program, vs);
            GL.DetachShader(program, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            return program;
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception($"{type} shader compilation failed:\n{log}");
            }

            return shader;
        }

        private static int LinkProgram(int vertexShader, int fragmentShader)
        {
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetProgramInfoLog(program);
                throw new Exception($"Program linking failed:\n{log}");
            }

            return program;
        }
    }
}