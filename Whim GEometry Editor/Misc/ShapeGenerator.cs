using MDLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{

   

    public static class ShapeGenerator
    {
        public static w3Geoset CreateGrid(int columns, int rows, float cellSize)
        {
            // Ensure valid input
            if (rows <= 0 || columns <= 0) return new w3Geoset();

            // Create a new geoset
            w3Geoset builtGeoset = new w3Geoset();

            // The normal for each vertex on the plane (pointing along the Z-axis)
            float normalX = 0;
            float normalY = 0;
            float normalZ = 1;

            // Create vertices and triangles
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // Calculate base position for this quad
                    float x = col * cellSize;
                    float y = row * cellSize;
                    float z = 0;  // Assuming the plane is on the X-Y axis, z = 0

                    // Create 4 vertices for the quad
                    w3Vertex v1 = new w3Vertex(); v1.Position.SetTo(x, y, z); v1.Normal.SetTo(normalX, normalY, normalZ);
                    w3Vertex v2 = new w3Vertex(); v2.Position.SetTo(x + cellSize, y, z); v2.Normal.SetTo(normalX, normalY, normalZ);
                    w3Vertex v3 = new w3Vertex(); v3.Position.SetTo(x, y + cellSize, z); v3.Normal.SetTo(normalX, normalY, normalZ);
                    w3Vertex v4 = new w3Vertex(); v4.Position.SetTo(x + cellSize, y + cellSize, z); v4.Normal.SetTo(normalX, normalY, normalZ);

                    // Add vertices to the geoset
                    builtGeoset.Vertices.Add(v1);
                    builtGeoset.Vertices.Add(v2);
                    builtGeoset.Vertices.Add(v3);
                    builtGeoset.Vertices.Add(v4);

                    // Create two triangles for the quad
                    // Triangle 1: v1, v2, v3
                    w3Triangle tr1 = new w3Triangle();
                    tr1.Vertex1 = v1;
                    tr1.Vertex2 = v2;
                    tr1.Vertex3 = v3;
                    builtGeoset.Triangles.Add(tr1);

                    // Triangle 2: v3, v2, v4
                    w3Triangle tr2 = new w3Triangle();
                    tr2.Vertex1 = v3;
                    tr2.Vertex2 = v2;
                    tr2.Vertex3 = v4;
                    builtGeoset.Triangles.Add(tr2);
                }
            }

            return builtGeoset;
        }


        public static w3Geoset GenerateCube(int cuts)
        {
            if (cuts < 1 || cuts > 20)
                throw new ArgumentOutOfRangeException(nameof(cuts), "Cuts must be between 1 and 20.");

            w3Geoset geoset = new w3Geoset();
            List<w3Vertex> vertices = new List<w3Vertex>();
            List<w3Triangle> triangles = new List<w3Triangle>();

            // Define the vertices for the cube's corners
            Coordinate[] cubeCorners = new Coordinate[]
            {
        new Coordinate { X = -1, Y = -1, Z = -1 },
        new Coordinate { X =  1, Y = -1, Z = -1 },
        new Coordinate { X =  1, Y =  1, Z = -1 },
        new Coordinate { X = -1, Y =  1, Z = -1 },
        new Coordinate { X = -1, Y = -1, Z =  1 },
        new Coordinate { X =  1, Y = -1, Z =  1 },
        new Coordinate { X =  1, Y =  1, Z =  1 },
        new Coordinate { X = -1, Y =  1, Z =  1 }
            };

            // Create vertices for the cube
            foreach (var corner in cubeCorners)
            {
                // Add vertex with normal pointing outward and texture coordinates
                vertices.Add(new w3Vertex
                {
                    Position = corner,
                    Normal = new Coordinate { X = corner.X, Y = corner.Y, Z = corner.Z }, // Normal
                    Texture_Position = new Coordinate2D { U = (corner.X + 1) / 2, V = (corner.Y + 1) / 2 } // Simple texture mapping
                });
            }

            // Define the triangles using the vertices
            int[,] cubeTriangles = new int[,]
            {
        // Front face
        { 0, 1, 2 },
        { 0, 2, 3 },
        
        // Back face
        { 4, 6, 5 },
        { 4, 7, 6 },
        
        // Left face
        { 0, 4, 5 },
        { 0, 5, 1 },
        
        // Right face
        { 1, 5, 6 },
        { 1, 6, 2 },
        
        // Top face
        { 3, 2, 6 },
        { 3, 6, 7 },
        
        // Bottom face
        { 0, 3, 7 },
        { 0, 7, 4 }
            };

            // Create triangles from the defined indices
            for (int i = 0; i < cubeTriangles.GetLength(0); i++)
            {
                triangles.Add(new w3Triangle
                {
                    Vertex1 = vertices[cubeTriangles[i, 0]],
                    Vertex2 = vertices[cubeTriangles[i, 1]],
                    Vertex3 = vertices[cubeTriangles[i, 2]]
                });
            }

            geoset.Vertices = vertices;
            geoset.Triangles = triangles;
            return geoset;
        }

        // Get vertex position based on the face index and grid coordinates
        private static Coordinate GetCubeVertexPosition(int face, int i, int j, float step)
        {
            float x = -1.0f + i * step;
            float y = -1.0f + j * step;

            switch (face)
            {
                case 0: return new Coordinate(x, y, 1);     // Front face
                case 1: return new Coordinate(x, y, -1);    // Back face
                case 2: return new Coordinate(1, x, y);     // Right face
                case 3: return new Coordinate(-1, x, y);    // Left face
                case 4: return new Coordinate(x, 1, y);     // Top face
                case 5: return new Coordinate(x, -1, y);    // Bottom face
                default: throw new ArgumentOutOfRangeException(nameof(face));
            }
        }

        // Get normal vector for a face (constant per face)
        private static Coordinate GetCubeNormal(int face)
        {
            switch (face)
            {
                case 0: return new Coordinate(0, 0, 1);    // Front face
                case 1: return new Coordinate(0, 0, -1);   // Back face
                case 2: return new Coordinate(1, 0, 0);    // Right face
                case 3: return new Coordinate(-1, 0, 0);   // Left face
                case 4: return new Coordinate(0, 1, 0);    // Top face
                case 5: return new Coordinate(0, -1, 0);   // Bottom face
                default: throw new ArgumentOutOfRangeException(nameof(face));
            }
        }

        // Get texture coordinates based on grid position
        private static Coordinate2D GetCubeTexCoord(int i, int j, int cuts)
        {
            float u = (float)i / cuts;
            float v = (float)j / cuts;
            return new Coordinate2D(u, v);
        }

        public static w3Geoset GenerateCylinder(int sections)
        {
            if (sections < 3 || sections > 50)
                throw new ArgumentOutOfRangeException(nameof(sections), "Sections must be between 3 and 50.");

            w3Geoset geoset = new w3Geoset();
            List<w3Vertex> vertices = new List<w3Vertex>();
            List<w3Triangle> triangles = new List<w3Triangle>();

            float radius = 1.0f;
            float height = 2.0f;
            float angleStep = 2 * MathF.PI / sections;

            // Create vertices for the top and bottom circles
            for (int i = 0; i < sections; i++)
            {
                float angle = i * angleStep;
                float x = radius * MathF.Cos(angle);
                float y = radius * MathF.Sin(angle);

                // Bottom vertices
                vertices.Add(new w3Vertex
                {
                    Position = new Coordinate { X = x, Y = y, Z = 0 },
                    Normal = new Coordinate { X = 0, Y = 0, Z = -1 },
                    Texture_Position = new Coordinate2D { U = (x + 1) / 2, V = (y + 1) / 2 }
                });

                // Top vertices
                vertices.Add(new w3Vertex
                {
                    Position = new Coordinate { X = x, Y = y, Z = height },
                    Normal = new Coordinate { X = 0, Y = 0, Z = 1 },
                    Texture_Position = new Coordinate2D { U = (x + 1) / 2, V = (y + 1) / 2 }
                });
            }

            // Top center vertex
            w3Vertex topCenter = new w3Vertex
            {
                Position = new Coordinate { X = 0, Y = 0, Z = height },
                Normal = new Coordinate { X = 0, Y = 0, Z = 1 },
                Texture_Position = new Coordinate2D { U = 0.5f, V = 0.5f }
            };
            vertices.Add(topCenter);

            // Bottom center vertex
            w3Vertex bottomCenter = new w3Vertex
            {
                Position = new Coordinate { X = 0, Y = 0, Z = 0 },
                Normal = new Coordinate { X = 0, Y = 0, Z = -1 },
                Texture_Position = new Coordinate2D { U = 0.5f, V = 0.5f }
            };
            vertices.Add(bottomCenter);

            // Create triangles for the top and bottom faces
            for (int i = 0; i < sections; i++)
            {
                int next = (i + 1) % sections;

                // Bottom face
                triangles.Add(new w3Triangle
                {
                    Vertex1 = bottomCenter,
                    Vertex2 = vertices[2 * i],
                    Vertex3 = vertices[2 * next]
                });

                // Top face
                triangles.Add(new w3Triangle
                {
                    Vertex1 = vertices[2 * i + 1],
                    Vertex2 = topCenter,
                    Vertex3 = vertices[2 * next + 1]
                });
            }

            // Create triangles for the sides
            for (int i = 0; i < sections; i++)
            {
                int next = (i + 1) % sections;

                triangles.Add(new w3Triangle
                {
                    Vertex1 = vertices[2 * i],
                    Vertex2 = vertices[2 * next],
                    Vertex3 = vertices[2 * i + 1]
                });

                triangles.Add(new w3Triangle
                {
                    Vertex1 = vertices[2 * next],
                    Vertex2 = vertices[2 * next + 1],
                    Vertex3 = vertices[2 * i + 1]
                });
            }

            geoset.Vertices = vertices;
            geoset.Triangles = triangles;
            return geoset;
        }


        public static w3Geoset GenerateSphere(int sections, int slices)
        {
            if (sections < 3 || sections > 50)
                throw new ArgumentOutOfRangeException(nameof(sections), "Sections must be between 3 and 50.");
            if (slices < 3 || slices > 50)
                throw new ArgumentOutOfRangeException(nameof(slices), "Slices must be between 3 and 50.");

            w3Geoset geoset = new w3Geoset();
            List<w3Vertex> vertices = new List<w3Vertex>();
            List<w3Triangle> triangles = new List<w3Triangle>();

            float radius = 1.0f;
            float angleStepSections = 2 * MathF.PI / sections; // Angle step for longitude
            float angleStepSlices = MathF.PI / slices; // Angle step for latitude

            // Create vertices for the sphere
            for (int i = 0; i <= slices; i++)
            {
                float theta = i * angleStepSlices; // Angle from top to bottom
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int j = 0; j <= sections; j++)
                {
                    float phi = j * angleStepSections; // Angle around the sphere
                    float x = radius * sinTheta * MathF.Cos(phi);
                    float y = radius * sinTheta * MathF.Sin(phi);
                    float z = radius * cosTheta;

                    // Add vertex with normal and texture coordinates
                    vertices.Add(new w3Vertex
                    {
                        Position = new Coordinate { X = x, Y = y, Z = z },
                        Normal = new Coordinate { X = x / radius, Y = y / radius, Z = z / radius }, // Normal points outward
                        Texture_Position = new Coordinate2D { U = (float)j / sections, V = (float)i / slices } // Texture coordinates
                    });
                }
            }

            // Create triangles
            for (int i = 0; i < slices; i++)
            {
                for (int j = 0; j < sections; j++)
                {
                    int first = (i * (sections + 1)) + j;
                    int second = first + sections + 1;

                    triangles.Add(new w3Triangle
                    {
                        Vertex1 = vertices[first],
                        Vertex2 = vertices[second],
                        Vertex3 = vertices[first + 1]
                    });

                    triangles.Add(new w3Triangle
                    {
                        Vertex1 = vertices[second],
                        Vertex2 = vertices[second + 1],
                        Vertex3 = vertices[first + 1]
                    });
                }
            }

            geoset.Vertices = vertices;
            geoset.Triangles = triangles;
            return geoset;
        }


        public static w3Geoset GenerateCone(int sections)
        {
            if (sections < 3 || sections > 50)
                throw new ArgumentOutOfRangeException(nameof(sections), "Sections must be between 3 and 50.");

            w3Geoset geoset = new w3Geoset();
            List<w3Vertex> vertices = new List<w3Vertex>();
            List<w3Triangle> triangles = new List<w3Triangle>();

            float radius = 1.0f;
            float height = 2.0f;
            float angleStep = 2 * MathF.PI / sections;

            // Create the base center vertex
            w3Vertex baseCenter = new w3Vertex
            {
                Position = new Coordinate { X = 0, Y = 0, Z = 0 },
                Normal = new Coordinate { X = 0, Y = 0, Z = -1 },
                Texture_Position = new Coordinate2D { U = 0.5f, V = 0.5f }
            };
            vertices.Add(baseCenter);

            // Create vertices for the base
            for (int i = 0; i < sections; i++)
            {
                float angle = i * angleStep;
                float x = radius * MathF.Cos(angle);
                float y = radius * MathF.Sin(angle);
                vertices.Add(new w3Vertex
                {
                    Position = new Coordinate { X = x, Y = y, Z = 0 },
                    Normal = new Coordinate { X = 0, Y = 0, Z = -1 }, // Normal for the base
                    Texture_Position = new Coordinate2D { U = (x + 1) / 2, V = (y + 1) / 2 }
                });
            }

            // Create the tip vertex
            w3Vertex tip = new w3Vertex
            {
                Position = new Coordinate { X = 0, Y = 0, Z = height },
                Normal = new Coordinate { X = 0, Y = 0, Z = 1 },
                Texture_Position = new Coordinate2D { U = 0.5f, V = 1 }
            };
            vertices.Add(tip);

            // Create triangles for the base
            for (int i = 1; i <= sections; i++)
            {
                triangles.Add(new w3Triangle
                {
                    Vertex1 = baseCenter,
                    Vertex2 = vertices[i],
                    Vertex3 = vertices[i % sections + 1]
                });
            }

            // Create triangles for the sides
            for (int i = 1; i <= sections; i++)
            {
                triangles.Add(new w3Triangle
                {
                    Vertex1 = vertices[i],
                    Vertex2 = tip,
                    Vertex3 = vertices[i % sections + 1]
                });
            }

            geoset.Vertices = vertices;
            geoset.Triangles = triangles;
            return geoset;
        }

        // Normalize method for Coordinate
        public static Coordinate Normalize(this Coordinate vector)
        {
            float length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            if (length == 0) return new Coordinate { X = 0, Y = 0, Z = 0 }; // Avoid division by zero
            return new Coordinate { X = vector.X / length, Y = vector.Y / length, Z = vector.Z / length };
        }

    }
}
