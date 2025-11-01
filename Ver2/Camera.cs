using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WindowEngine
{
    public class Camera
    {
        // Camera vectors
        public Vector3 Position { get; private set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }

        // Euler angles
        private float _yaw;
        private float _pitch;

        // Camera options
        public float MovementSpeed { get; set; }
        public float MouseSensitivity { get; set; }
        public float Zoom { get; private set; }

        public Camera(Vector3 position, Vector3 front, Vector3 up, float movementSpeed = 2.5f, float mouseSensitivity = 0.1f)
        {
            Position = position;
            Front = front;
            Up = up;
            MovementSpeed = movementSpeed;
            MouseSensitivity = mouseSensitivity;
            Zoom = 45.0f;
            _yaw = -90.0f;
            _pitch = 0.0f;

            UpdateCameraVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void ProcessKeyboard(Keys key, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;

            switch (key)
            {
                case Keys.W:
                    Position += Front * velocity;
                    break;
                case Keys.S:
                    Position -= Front * velocity;
                    break;
                case Keys.A:
                    Position -= Right * velocity;
                    break;
                case Keys.D:
                    Position += Right * velocity;
                    break;
            }
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= MouseSensitivity;
            yOffset *= MouseSensitivity;

            _yaw += xOffset;
            _pitch += yOffset;

            // Make sure that when pitch is out of bounds, screen doesn't get flipped
            if (constrainPitch)
            {
                if (_pitch > 89.0f)
                    _pitch = 89.0f;
                if (_pitch < -89.0f)
                    _pitch = -89.0f;
            }

            // Update Front, Right and Up vectors using the updated Euler angles
            UpdateCameraVectors();
        }

        public void ProcessMouseScroll(float yOffset)
        {
            Zoom -= yOffset;
            if (Zoom < 1.0f)
                Zoom = 1.0f;
            if (Zoom > 45.0f)
                Zoom = 45.0f;
        }

        private void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            Front = Vector3.Normalize(front);

            // Also re-calculate the Right and Up vector
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY)); // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}