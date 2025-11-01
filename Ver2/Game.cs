using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Ver2.Shapes;

namespace WindowEngine
{
    public class Game : IDisposable
    {
        private GameWindow _window;
        private Shader _shader;

        // VAOs, VBOs, and EBOs for multiple objects
        private int _cubeVao, _cubeVbo, _cubeEbo;
        private int _stationaryCubeVao, _stationaryCubeVbo, _stationaryCubeEbo;
        private int _planeVao, _planeVbo, _planeEbo;

        // Texture
        private Texture _planeTexture;

        //Shapes
        private Cube cube;
        private Plane plane;

        // Camera
        private Camera _camera;

        // Mouse control variables
        private float _lastX = 400.0f;
        private float _lastY = 300.0f;
        private bool _firstMouse = true;

        // Rotation variables
        private float _rotationAngle = 0.0f;
        private bool _isRotating = false;
        private float _rotationSpeed = 1.0f;

        // Light properties
        private Vector3 _lightPosition = new Vector3(1.2f, 1.0f, 2.0f);
        private Vector3 _lightColor = new Vector3(1.0f, 1.0f, 1.0f);
        private Vector3 _objectColor = new Vector3(0.8f, 0.3f, 0.2f);
        private Vector3 _stationaryCubeColor = new Vector3(0.2f, 0.5f, 0.8f);
        private Vector3 _planeColor = new Vector3(0.3f, 0.7f, 0.3f);

        // Model matrices
        private Matrix4 _rotatingCubeModel;
        private Matrix4 _stationaryCubeModel;
        private Matrix4 _planeModel;
        private Matrix4 _view;
        private Matrix4 _projection;

        public Game()
        {
            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "OpenGL 3.3 Textured Plane",
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
                NumberOfSamples = 4
            };

            var gameSettings = new GameWindowSettings()
            {
                UpdateFrequency = 60.0
            };

            _window = new GameWindow(gameSettings, nativeSettings);

            _window.Load += OnLoad;
            _window.RenderFrame += OnRenderFrame;
            _window.UpdateFrame += OnUpdateFrame;
            _window.Resize += OnResize;
            _window.MouseMove += OnMouseMove;
            _window.MouseWheel += OnMouseWheel;
        }

        private void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // Create shader
            _shader = new Shader("../../../Shaders/Shader.vert", "../../../Shaders/Shader.frag");

            // Load texture - replace with your PNG file path
            _planeTexture = new Texture("../../../Textures/floor.png");

            // Initialize camera
            _camera = new Camera(
                new Vector3(0.0f, 2.0f, 5.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 1.0f, 0.0f)
            );

            // Create shapes
            cube = new Cube();
            plane = new Plane();

            // Load rotating cube
            SetupCubeGeometry(ref _cubeVao, ref _cubeVbo, ref _cubeEbo, cube);

            // Load stationary cube
            SetupCubeGeometry(ref _stationaryCubeVao, ref _stationaryCubeVbo, ref _stationaryCubeEbo, cube);

            // Load plane with texture coordinates
            SetupPlaneGeometry();

            // Set up matrices
            _rotatingCubeModel = Matrix4.CreateTranslation(0.0f, 1.0f, 0.0f);
            _stationaryCubeModel = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            _planeModel = Matrix4.CreateScale(5.0f) * Matrix4.CreateTranslation(0.0f, -2.0f, 0.0f);

            _view = _camera.GetViewMatrix();
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 800.0f / 600.0f, 0.1f, 100.0f);

            _window.CursorState = CursorState.Grabbed;
        }

        private void SetupCubeGeometry(ref int vao, ref int vbo, ref int ebo, Cube cubeObj)
        {
            float[] vertices = cubeObj.getVert();
            uint[] indices = cubeObj.getIndices();

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Normal attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void SetupPlaneGeometry()
        {
            float[] planeVertices = plane.getVert();
            uint[] planeIndices = plane.getIndices();

            _planeVao = GL.GenVertexArray();
            _planeVbo = GL.GenBuffer();
            _planeEbo = GL.GenBuffer();

            GL.BindVertexArray(_planeVao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _planeVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, planeVertices.Length * sizeof(float), planeVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _planeEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, planeIndices.Length * sizeof(uint), planeIndices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Normal attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Texture coordinate attribute
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void OnUpdateFrame(FrameEventArgs e)
        {
            if (_window.IsKeyDown(Keys.Escape))
                _window.Close();

            float deltaTime = (float)e.Time;

            // Camera movement
            if (_window.IsKeyDown(Keys.W))
                _camera.ProcessKeyboard(Keys.W, deltaTime);
            if (_window.IsKeyDown(Keys.S))
                _camera.ProcessKeyboard(Keys.S, deltaTime);
            if (_window.IsKeyDown(Keys.A))
                _camera.ProcessKeyboard(Keys.A, deltaTime);
            if (_window.IsKeyDown(Keys.D))
                _camera.ProcessKeyboard(Keys.D, deltaTime);

            // Toggle rotation with R key
            if (_window.IsKeyPressed(Keys.R))
            {
                _isRotating = !_isRotating;
            }

            // Manual object rotation
            if (_window.IsKeyDown(Keys.Left))
                _rotationAngle -= 1.0f * deltaTime;
            if (_window.IsKeyDown(Keys.Right))
                _rotationAngle += 1.0f * deltaTime;

            // Automatic rotation
            if (_isRotating)
            {
                _rotationAngle += _rotationSpeed * deltaTime;
            }

            // Update rotating cube model matrix
            _rotatingCubeModel = Matrix4.CreateRotationY(_rotationAngle) * Matrix4.CreateTranslation(0.0f, 1.0f, 0.0f);

            // Light movement
            if (_window.IsKeyDown(Keys.Up))
                _lightPosition.Y += 1.0f * deltaTime;
            if (_window.IsKeyDown(Keys.Down))
                _lightPosition.Y -= 1.0f * deltaTime;
            if (_window.IsKeyDown(Keys.Comma))
                _lightPosition.X -= 1.0f * deltaTime;
            if (_window.IsKeyDown(Keys.Period))
                _lightPosition.X += 1.0f * deltaTime;

            // Change object color with number keys
            if (_window.IsKeyPressed(Keys.D1))
                _objectColor = new Vector3(0.8f, 0.3f, 0.2f);
            if (_window.IsKeyPressed(Keys.D2))
                _objectColor = new Vector3(0.2f, 0.6f, 0.8f);
            if (_window.IsKeyPressed(Keys.D3))
                _objectColor = new Vector3(0.3f, 0.8f, 0.3f);
            if (_window.IsKeyPressed(Keys.D4))
                _objectColor = new Vector3(0.9f, 0.9f, 0.3f);

            _view = _camera.GetViewMatrix();
        }

        private void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            // Set matrices
            _shader.SetMatrix4("view", ref _view);
            _shader.SetMatrix4("projection", ref _projection);

            // Set light properties
            _shader.SetVector3("lightPos", _lightPosition);
            _shader.SetVector3("lightColor", _lightColor);
            _shader.SetVector3("viewPos", _camera.Position);

            // Render rotating cube (no texture)
            _shader.SetBool("useTexture", false);
            _shader.SetMatrix4("model", ref _rotatingCubeModel);
            _shader.SetVector3("objectColor", _objectColor);
            GL.BindVertexArray(_cubeVao);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            // Render stationary cube (no texture)
            _shader.SetBool("useTexture", false);
            _shader.SetMatrix4("model", ref _stationaryCubeModel);
            _shader.SetVector3("objectColor", _stationaryCubeColor);
            GL.BindVertexArray(_stationaryCubeVao);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            // Render plane (with texture)
            _shader.SetBool("useTexture", true);
            _shader.SetInt("texture1", 0); // Use texture unit 0
            _planeTexture.Use(TextureUnit.Texture0);
            _shader.SetMatrix4("model", ref _planeModel);
            _shader.SetVector3("objectColor", _planeColor); // Fallback color if texture fails
            GL.BindVertexArray(_planeVao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            _window.SwapBuffers();
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), (float)e.Width / e.Height, 0.1f, 100.0f);
        }

        private void OnMouseMove(MouseMoveEventArgs e)
        {
            if (_firstMouse)
            {
                _lastX = e.X;
                _lastY = e.Y;
                _firstMouse = false;
            }

            float xOffset = e.X - _lastX;
            float yOffset = _lastY - e.Y;
            _lastX = e.X;
            _lastY = e.Y;

            _camera.ProcessMouseMovement(xOffset, yOffset);
        }

        private void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.ProcessMouseScroll(e.OffsetY);
            _camera.MovementSpeed += e.OffsetY * 0.5f;
            if (_camera.MovementSpeed < 0.5f)
                _camera.MovementSpeed = 0.5f;
            if (_camera.MovementSpeed > 10.0f)
                _camera.MovementSpeed = 10.0f;
        }

        public void Run() => _window.Run();

        public void Dispose()
        {
            GL.DeleteBuffer(_cubeVbo);
            GL.DeleteBuffer(_cubeEbo);
            GL.DeleteVertexArray(_cubeVao);

            GL.DeleteBuffer(_stationaryCubeVbo);
            GL.DeleteBuffer(_stationaryCubeEbo);
            GL.DeleteVertexArray(_stationaryCubeVao);

            GL.DeleteBuffer(_planeVbo);
            GL.DeleteBuffer(_planeEbo);
            GL.DeleteVertexArray(_planeVao);

            _shader?.Dispose();
            _planeTexture?.Dispose();
            _window.Dispose();
        }
    }
}