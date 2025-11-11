using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenTK_Sprite_Animation
{
    public class SpriteAnimationGame : GameWindow
    {
        private Character _character;
        private int _shaderProgram;
        private int _vao, _vbo;
        private Dictionary<AnimationState, int> _textures;
        private Dictionary<AnimationState, float> _textureWidths;
        private int _backgroundTex;
        private int _backgroundWidth;
        private int _backgroundHeight;

        // Camera
        private float _cameraX = 0f;
        private const float CameraSmoothness = 5f;
        private const float SceneWidth = 1600f;

        private KeyboardState _prevKeyboard;
        private AudioManager _audioManager;
        public SpriteAnimationGame()
            : base(
                new GameWindowSettings(),
                new NativeWindowSettings { Size = (800, 600), Title = "Advanced Sprite Animation - Fighting Game" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.4f, 1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shaderProgram = ShaderHelper.CreateShaderProgram();
            // Initialize audio
            _audioManager = new AudioManager();
            // Load all animation textures
            _textures = new Dictionary<AnimationState, int>();
            _textureWidths = new Dictionary<AnimationState, float>();

            // Load all animation states
            LoadAnimationTexture(AnimationState.Idle);
            LoadAnimationTexture(AnimationState.Walking);
            LoadAnimationTexture(AnimationState.Running);
            LoadAnimationTexture(AnimationState.Crouch);
            LoadAnimationTexture(AnimationState.Jump);
            LoadAnimationTexture(AnimationState.Punch1);
            LoadAnimationTexture(AnimationState.Punch2);
            LoadAnimationTexture(AnimationState.DefenseAttack);
            LoadAnimationTexture(AnimationState.FireKick);
            LoadAnimationTexture(AnimationState.ExplosiveStrike);
            // Load sound effects (adjust paths to your actual audio files)
            _audioManager.LoadSoundEffect("punch", Path.Combine("Assets", "Audio", "punch.wav"));
            _audioManager.LoadSoundEffect("kick", Path.Combine("Assets", "Audio", "punch.wav"));
            _audioManager.LoadSoundEffect("jump", Path.Combine("Assets", "Audio", "jump.wav"));
            //_audioManager.LoadSoundEffect("land", Path.Combine("Assets", "Audio", "land.wav"));
            

            // Create quad geometry (86x86 sprite size)
            float w = 86f, h = 86f;
            float[] vertices =
            {
                -w/2, -h/2, 0f, 0f,
                 w/2, -h/2, 1f, 0f,
                 w/2,  h/2, 1f, 1f,
                -w/2,  h/2, 0f, 1f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            // Texture coordinate attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            // Load background (scenario)
            _backgroundTex = TextureLoader.LoadTexture(System.IO.Path.Combine("Assets", "Sprites", "Background.png"));

            // Get background dimensions
            using var bgImg = SixLabors.ImageSharp.Image.Load<Rgba32>(System.IO.Path.Combine("Assets", "Sprites", "Background.png"));
            _backgroundWidth = bgImg.Width;
            _backgroundHeight = bgImg.Height;

            // Setup shader uniforms
            GL.UseProgram(_shaderProgram);

            int texLoc = GL.GetUniformLocation(_shaderProgram, "uTexture");
            GL.Uniform1(texLoc, 0);

            _character = new Character(_textures, _textureWidths, 400, 150, _audioManager);
        }

        private void LoadAnimationTexture(AnimationState state)
        {
            string filename = SpriteSheetHelper.GetTextureFileName(state);
            string path = System.IO.Path.Combine("Assets", "Sprites", filename);

            _textures[state] = TextureLoader.LoadTexture(path);

            // Get texture width for UV calculations
            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
            _textureWidths[state] = img.Width;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Create a snapshot copy of current keyboard state
            var currentKeys = KeyboardState.GetSnapshot();

            // Initialize previous keyboard state on first frame
            if (_prevKeyboard == null)
            {
                _prevKeyboard = currentKeys;
                return; // Skip first frame
            }

            // Movement inputs
            bool leftDown = currentKeys.IsKeyDown(Keys.Left) || currentKeys.IsKeyDown(Keys.A);
            bool rightDown = currentKeys.IsKeyDown(Keys.Right) || currentKeys.IsKeyDown(Keys.D);
            bool prevLeftDown = _prevKeyboard.IsKeyDown(Keys.Left) || _prevKeyboard.IsKeyDown(Keys.A);
            bool prevRightDown = _prevKeyboard.IsKeyDown(Keys.Right) || _prevKeyboard.IsKeyDown(Keys.D);

            // Attack inputs
            bool attackDown = currentKeys.IsKeyDown(Keys.J) || currentKeys.IsKeyDown(Keys.Z);
            bool prevAttackDown = _prevKeyboard.IsKeyDown(Keys.J) || _prevKeyboard.IsKeyDown(Keys.Z);

            bool defenseAttackDown = currentKeys.IsKeyDown(Keys.L) || currentKeys.IsKeyDown(Keys.C);
            bool prevDefenseAttackDown = _prevKeyboard.IsKeyDown(Keys.L) || _prevKeyboard.IsKeyDown(Keys.C);

            bool fireKickDown = currentKeys.IsKeyDown(Keys.U) || currentKeys.IsKeyDown(Keys.Q);
            bool prevFireKickDown = _prevKeyboard.IsKeyDown(Keys.U) || _prevKeyboard.IsKeyDown(Keys.Q);

            bool explosiveStrikeDown = currentKeys.IsKeyDown(Keys.I) || currentKeys.IsKeyDown(Keys.E);
            bool prevExplosiveStrikeDown = _prevKeyboard.IsKeyDown(Keys.I) || _prevKeyboard.IsKeyDown(Keys.E);

            // DEBUG: Show actual values
            System.Console.WriteLine($"Attack: current={attackDown}, prev={prevAttackDown}, pressed={(attackDown && !prevAttackDown)}");

            var input = new InputState
            {
                // Movement
                Left = leftDown,
                Right = rightDown,
                Jump = currentKeys.IsKeyDown(Keys.Space) || currentKeys.IsKeyDown(Keys.W),
                Sprint = currentKeys.IsKeyDown(Keys.LeftShift) || currentKeys.IsKeyDown(Keys.RightShift),
                Crouch = currentKeys.IsKeyDown(Keys.S) || currentKeys.IsKeyDown(Keys.Down),
                Defend = currentKeys.IsKeyDown(Keys.K) || currentKeys.IsKeyDown(Keys.X),

                // Basic attack
                Attack = attackDown,

                // Special attacks
                DefenseAttack = defenseAttackDown,
                FireKick = fireKickDown,
                ExplosiveStrike = explosiveStrikeDown,

                // Edge-triggered (pressed this frame)
                LeftPressed = leftDown && !prevLeftDown,
                RightPressed = rightDown && !prevRightDown,
                AttackPressed = attackDown && !prevAttackDown,
                DefenseAttackPressed = defenseAttackDown && !prevDefenseAttackDown,
                FireKickPressed = fireKickDown && !prevFireKickDown,
                ExplosiveStrikePressed = explosiveStrikeDown && !prevExplosiveStrikeDown
            };

            if (_character != null)
                _character.Update((float)e.Time, input);

            // Camera Follow
            float targetCameraX = _character.GetPositionX() - 400f;
            _cameraX += (targetCameraX - _cameraX) * (CameraSmoothness * (float)e.Time);

            // Clamp camera to scene edges
            _cameraX = System.Math.Clamp(_cameraX, 0f, SceneWidth - 800f);

            // Save current as previous for next frame
            _prevKeyboard = currentKeys;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);

            // Update projection matrix with current camera position
            int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(_cameraX, _cameraX + 800, 0, 600, -1, 1);
            GL.UniformMatrix4(projLoc, false, ref ortho);

            // Draw background
            GL.BindTexture(TextureTarget.Texture2D, _backgroundTex);

            // Calculate proper scaling to maintain aspect ratio
            float targetHeight = 600f; // Match window height
            float aspectRatio = (float)_backgroundWidth / _backgroundHeight;
            float scaledWidth = targetHeight * aspectRatio;

            // Calculate how many tiles we need to cover the visible area
            float visibleStartX = _cameraX;
            float visibleEndX = _cameraX + 800f;

            // Start drawing from before the visible area
            float tileStartX = (float)System.Math.Floor(visibleStartX / scaledWidth) * scaledWidth;
            int tilesNeeded = (int)System.Math.Ceiling((visibleEndX - tileStartX) / scaledWidth) + 1;

            for (int i = 0; i < tilesNeeded; i++)
            {
                float tileX = tileStartX + (i * scaledWidth);
                float tileCenterX = tileX + (scaledWidth / 2f);
                float tileCenterY = targetHeight / 2f; // Center vertically

                Matrix4 bgModel =
                    Matrix4.CreateScale(scaledWidth / 86f, targetHeight / 86f, 1f) *
                    Matrix4.CreateTranslation(tileCenterX, tileCenterY, 0f);

                int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
                GL.UniformMatrix4(modelLoc, false, ref bgModel);

                // Set UV to show full background image
                int offsetLoc = GL.GetUniformLocation(_shaderProgram, "uOffset");
                int sizeLoc = GL.GetUniformLocation(_shaderProgram, "uSize");
                GL.Uniform2(offsetLoc, 0f, 0f);
                GL.Uniform2(sizeLoc, 1f, 1f);

                GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            }

            // Draw character
            _character.Render(_shaderProgram);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteProgram(_shaderProgram);

            foreach (var texture in _textures.Values)
            {
                GL.DeleteTexture(texture);
            }

            GL.DeleteTexture(_backgroundTex);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            base.OnUnload();
        }
    }
}