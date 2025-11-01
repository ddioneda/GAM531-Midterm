using OpenTK.Mathematics;

namespace Ver2.Shapes
{
    public class Plane
    {
        private float[] vertices;
        private uint[] indices;

        public Plane()
        {
            // Define plane vertices with positions, normals, and texture coordinates
            vertices = new float[] {
                // Positions          // Normals        // Texture Coords
                -0.5f, 0.25f, -0.5f,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,  // bottom-left
                 0.5f, 0.25f, -0.5f,  0.0f, 1.0f, 0.0f,  1.0f, 0.0f,  // bottom-right
                 0.5f, 0.25f,  0.5f,  0.0f, 1.0f, 0.0f,  1.0f, 1.0f,  // top-right
                -0.5f, 0.25f,  0.5f,  0.0f, 1.0f, 0.0f,  0.0f, 1.0f,  // top-left
            };

            indices = new uint[] {
                0, 1, 2,
                2, 3, 0
            };
        }

        public float[] getVert()
        {
            return vertices;
        }

        public uint[] getIndices()
        {
            return indices;
        }
    }
}