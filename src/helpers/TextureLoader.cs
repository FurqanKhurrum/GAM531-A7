using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using ImageSharp = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTK_Sprite_Animation
{
    /// <summary>
    /// Helper class for loading textures from image files
    /// </summary>
    public static class TextureLoader
    {
        public static int LoadTexture(string path)
        {
            if (!File.Exists(path))
                
                throw new FileNotFoundException($"Texture not found: {path}", path);

            using var img = ImageSharp.Load<Rgba32>(path);
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Copy pixel data to byte array and upload to GPU
            var pixels = new byte[4 * img.Width * img.Height];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                img.Width,
                img.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels);

            // For pixel art / sprite sheets, use Nearest filtering (no blur)
            // and don't generate mipmaps to keep pixels crisp
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Clamp to edge to avoid wrap artifacts
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texture;
        }

        /// <summary>
        /// Loads a texture with optional width/height output
        /// </summary>
        public static int LoadTexture(string path, out int width, out int height)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}", path);

            using var img = ImageSharp.Load<Rgba32>(path);
            width = img.Width;
            height = img.Height;

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            var pixels = new byte[4 * img.Width * img.Height];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                img.Width,
                img.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texture;
        }
    }
}