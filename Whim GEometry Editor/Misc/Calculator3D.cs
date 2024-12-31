
using MDLLib;
using System.Drawing;
using System.Numerics;
using System.Windows;
using Whim_GEometry_Editor.Misc;

namespace MDLLibs.Classes.Misc
{
    public class FacingAngle
    {
        private float yaw, pitch, roll = 0;
        public float Yaw { get => yaw; set { if (value < 360 || value > 360) { yaw = 0; } else { yaw = value; } } }
        public float Pitch { get => pitch; set { if (value < 360 || value > 360) { pitch = 0; } else { pitch = value; } } }
        public float Roll { get => roll; set { if (value < 360 || value > 360) { roll = 0; } else { roll = value; } } }

    }
    class DistanceSetter
    {
       
        public static void SetDistance(w3Vertex vertex1, w3Vertex vertex2, float distance)
        {
            // Calculate the current distance between the vertices
            float currentDistance = CalculateDistance(vertex1.Position, vertex2.Position);
            // Calculate the scaling factor to adjust the distance
            float scale = distance / currentDistance;
            // Adjust the position of vertex2 to match the desired distance from vertex1
            vertex2.Position.X = vertex1.Position.X + scale * (vertex2.Position.X - vertex1.Position.X);
            vertex2.Position.Y = vertex1.Position.Y + scale * (vertex2.Position.Y - vertex1.Position.Y);
            vertex2.Position.Z = vertex1.Position.Z + scale * (vertex2.Position.Z - vertex1.Position.Z);
        }

        internal static float GetDistance(w3Vertex w3Vertex1, w3Vertex w3Vertex2)
        {
            float dx = w3Vertex2.Position.X - w3Vertex1.Position.X;
            float dy = w3Vertex2.Position.Y - w3Vertex1.Position.Y;
            float dz = w3Vertex2.Position.Z - w3Vertex1.Position.Z;

            // Calculate the Euclidean distance
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        private static float CalculateDistance(Coordinate coord1, Coordinate coord2)
        {
            float dx = coord2.X - coord1.X;
            float dy = coord2.Y - coord1.Y;
            float dz = coord2.Z - coord1.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
    internal class GeoData
    {
        public List<float[]> Vertices { get; set; }
        public List<int[]> Triangles { get; set; }
    }
    internal static class ShapeHelper
    {
        public static GeoData GenerateCube(int sections)
        {
            // Calculate the number of vertices needed per face
            int verticesPerFace = (sections + 1) * (sections + 1);
            // Calculate the step size for each section
            float stepSize = 1f / sections;
            List<float[]> vertices = new List<float[]>();
            List<int[]> triangles = new List<int[]>();
            // Generate vertices and triangles for each face
            for (int face = 0; face < 6; face++)
            {
                // Define the indices of the vertices for the current face
                int offset = face * verticesPerFace;
                int xIndex = offset;
                int yIndex = xIndex + sections + 1;
                // Generate vertices and triangles for the current face
                for (int y = 0; y < sections; y++)
                {
                    for (int x = 0; x < sections; x++)
                    {
                        // Define the indices of the current square's vertices
                        int topLeft = x + y * (sections + 1) + offset;
                        int topRight = topLeft + 1;
                        int bottomLeft = topLeft + (sections + 1);
                        int bottomRight = bottomLeft + 1;
                        // Add triangles for the current square
                        triangles.Add(new int[] { topLeft, bottomLeft, topRight });
                        triangles.Add(new int[] { topRight, bottomLeft, bottomRight });
                        // Generate vertices for the current square
                        vertices.Add(new float[] { x * stepSize - 0.5f, y * stepSize - 0.5f, GetFaceZCoordinate(face, x, y, sections, stepSize) });
                        vertices.Add(new float[] { (x + 1) * stepSize - 0.5f, y * stepSize - 0.5f, GetFaceZCoordinate(face, x + 1, y, sections, stepSize) });
                        vertices.Add(new float[] { x * stepSize - 0.5f, (y + 1) * stepSize - 0.5f, GetFaceZCoordinate(face, x, y + 1, sections, stepSize) });
                        vertices.Add(new float[] { (x + 1) * stepSize - 0.5f, (y + 1) * stepSize - 0.5f, GetFaceZCoordinate(face, x + 1, y + 1, sections, stepSize) });
                    }
                }
            }
            return new GeoData { Vertices = vertices, Triangles = triangles };
        }
        public static GeoData GenerateCone(int sections)
        {
            if (sections < 3)
            {
                throw new ArgumentException("Number of sections must be 3 or more.");
            }
            List<float[]> vertices = new List<float[]>();
            List<int[]> triangles = new List<int[]>();
            // Generate the vertices for the base of the cone
            vertices.Add(new float[] { 0f, 0f, 0f }); // Center vertex of the base
            float angleStep = 2 * MathF.PI / sections;
            for (int i = 0; i < sections; i++)
            {
                float angle = i * angleStep;
                float x = MathF.Cos(angle) * 0.5f;
                float y = MathF.Sin(angle) * 0.5f;
                vertices.Add(new float[] { x, y, 0f });
            }
            // Apex vertex
            vertices.Add(new float[] { 0f, 0f, 1f });
            int apexIndex = vertices.Count - 1;
            // Generate triangles for the base of the cone
            for (int i = 1; i < sections; i++)
            {
                int nextIndex = i + 1;
                triangles.Add(new int[] { 0, i, nextIndex });
            }
            // Connect the last section back to the first
            triangles.Add(new int[] { 0, sections, 1 });
            // Generate triangles for the sides of the cone
            for (int i = 1; i < sections; i++)
            {
                int nextIndex = i + 1;
                triangles.Add(new int[] { i, nextIndex, apexIndex });
            }
            // Connect the last section back to the first
            triangles.Add(new int[] { sections, 1, apexIndex });
            return new GeoData { Vertices = vertices, Triangles = triangles };
        }
        private static float GetFaceZCoordinate(int face, int x, int y, int sections, float stepSize)
        {
            switch (face)
            {
                case 0: // Front face
                    return 0.5f;
                case 1: // Back face
                    return -0.5f;
                case 2: // Top face
                    return y * stepSize - 0.5f;
                case 3: // Bottom face
                    return -y * stepSize + 0.5f;
                case 4: // Left face
                    return x * stepSize - 0.5f;
                case 5: // Right face
                    return -x * stepSize + 0.5f;
                default:
                    return 0.0f;
            }
        }
     }
    
    internal static class Calculator3D
    {
        public static void CutEdge(w3Edge edge, w3Geoset geoset, int createdVertices)
        {
            // Get positions of the two vertices of the edge
            Coordinate vertex1Pos = edge.Vertex1.Position;
            Coordinate vertex2Pos = edge.Vertex2.Position;

            // Calculate the step for equally distanced vertices
            double stepX = (vertex2Pos.X - vertex1Pos.X) / (createdVertices );
            double stepY = (vertex2Pos.Y - vertex1Pos.Y) / (createdVertices  );
            double stepZ = (vertex2Pos.Z - vertex1Pos.Z) / (createdVertices );

            // Create new vertices along the edge
            for (int i = 1; i < createdVertices; i++) // Start from 1 to skip the first vertex
            {
                w3Vertex newVertex = new w3Vertex();
                newVertex.Position = new Coordinate(
                    vertex1Pos.X + stepX * i,
                    vertex1Pos.Y + stepY * i,
                    vertex1Pos.Z + stepZ * i
                );
                newVertex.AttachedTo = edge.Vertex1.AttachedTo.ToList();

                // Add the new vertex to the geoset
                if (geoset.Vertices.Any(x=>x.Position.Compare(newVertex.Position) == false))
                {
                    geoset.Vertices.Add(newVertex);
                }
                
            }
        }

        public static FacingAngle GetTriangleFacingDirection(w3Triangle triangle)
        {
            // Get the positions of the triangle vertices
            Coordinate one = triangle.Vertex1.Position;
            Coordinate two = triangle.Vertex2.Position;
            Coordinate three = triangle.Vertex3.Position;

            // Get the normals of the triangle vertices
            Coordinate normal1 = triangle.Vertex1.Normal;
            Coordinate normal2 = triangle.Vertex2.Normal;
            Coordinate normal3 = triangle.Vertex3.Normal;

            // Calculate the average normal vector of the triangle
            float avgNormalX = (normal1.X + normal2.X + normal3.X) / 3;
            float avgNormalY = (normal1.Y + normal2.Y + normal3.Y) / 3;
            float avgNormalZ = (normal1.Z + normal2.Z + normal3.Z) / 3;

            // Normalize the average normal vector
            float length = (float)Math.Sqrt(avgNormalX * avgNormalX + avgNormalY * avgNormalY + avgNormalZ * avgNormalZ);
            avgNormalX /= length;
            avgNormalY /= length;
            avgNormalZ /= length;

            // Calculate yaw, pitch, and roll based on the average normal
            // Yaw is the rotation around the Y-axis, Pitch is the rotation around the X-axis, and Roll is the rotation around the Z-axis
            float yaw = (float)Math.Atan2(avgNormalX, avgNormalZ) * (180 / (float)Math.PI);
            float pitch = (float)Math.Asin(-avgNormalY) * (180 / (float)Math.PI);
            float roll = 0; // Roll is usually not calculated from normals alone, assuming no roll for simplicity.

            // Return the calculated angles as a FacingAngle object
            return new FacingAngle
            {
                Yaw = yaw,
                Pitch = pitch,
                Roll = roll
            };
        }

        public static void CenterVertices(List<w3Vertex> vertices, float x, float y, float z)
        {
            if (vertices.Count == 0) return;
            if (vertices.Count == 1)
            {
                vertices[0].Position.SetTo(x, y, z);
                return;
            }
            // Target centroid where we want to move the vertices
            Coordinate centerOnTarget = new Coordinate(x, y, z);

            // Current centroid of all vertices
            Coordinate currentCentroid = GetCentroid(vertices);

            // Calculate the offset needed to move the centroid to the target position
            float offsetX = centerOnTarget.X - currentCentroid.X;
            float offsetY = centerOnTarget.Y - currentCentroid.Y;
            float offsetZ = centerOnTarget.Z - currentCentroid.Z;

            // Apply the offset to each vertex
            foreach (w3Vertex v in vertices)
            {
                v.Position.X += offsetX;
                v.Position.Y += offsetY;
                v.Position.Z += offsetZ;
            }
        }


        public static void CenterVertices(List<w3Vertex> vertices, Coordinate coord)
        {
            CenterVertices(vertices, coord.X, coord.Y, coord.Z);

        }
        public static void RotateHorizontallyAroundCenter(ref float eyeX, ref float eyeY, ref float eyeZ, int angleChange)
        {
            // Step 1: Calculate the distance from the center in the XZ plane
            float distanceXZ = (float)Math.Sqrt(eyeX * eyeX + eyeZ * eyeZ); // Horizontal distance in the XZ plane

            // Step 2: Calculate the current angle in the XZ plane (rotation around Y-axis)
            float azimuth = (float)Math.Atan2(eyeZ, eyeX); // Current angle of eye's position in the XZ plane

            // Step 3: Adjust the azimuth by the given angleChange (convert degrees to radians)
            float angleChangeRadians = angleChange * (float)Math.PI / 180f;
            azimuth += angleChangeRadians; // Apply horizontal rotation

            // Step 4: Recalculate the new eyeX and eyeZ while keeping the distance constant
            eyeX = distanceXZ * (float)Math.Cos(azimuth); // Update X position
            eyeZ = distanceXZ * (float)Math.Sin(azimuth); // Update Z position

            // The eyeY stays the same because we're not moving vertically
            // No zooming in or out; distance remains the same.
        }









        public static Coordinate CalculateCentroidFromVertices(List<w3Vertex> vertices)
        {
            if (vertices == null || vertices.Count ==0)
                return new Coordinate(); ;

            float totalX = 0f;
            float totalY = 0f;
            float totalZ = 0f;

            foreach (var vertex in vertices)
            {
                totalX += vertex.Position.X;
                totalY += vertex.Position.Y;
                totalZ += vertex.Position.Z;
            }

            int vertexCount = vertices.Count;

            return new Coordinate
            {
                X = totalX / vertexCount,
                Y = totalY / vertexCount,
                Z = totalZ / vertexCount
            };
        }
       
            public static Extent GetExtentFromVertexList(List<w3Vertex> vertices)
            {
                if (vertices == null || vertices.Count == 0)
                    return new Extent(); // Return an empty extent if there are no vertices.

                // Initialize the extent with the first vertex's position
                Extent extent = new Extent
                {
                    Minimum_X = vertices[0].Position.X,
                    Maximum_X = vertices[0].Position.X,
                    Minimum_Y = vertices[0].Position.Y,
                    Maximum_Y = vertices[0].Position.Y,
                    Minimum_Z = vertices[0].Position.Z,
                    Maximum_Z = vertices[0].Position.Z
                };

                // Iterate through all vertices to calculate the min and max extents
                foreach (var vertex in vertices)
                {
                    if (vertex.Position.X < extent.Minimum_X)
                        extent.Minimum_X = vertex.Position.X;
                    if (vertex.Position.X > extent.Maximum_X)
                        extent.Maximum_X = vertex.Position.X;

                    if (vertex.Position.Y < extent.Minimum_Y)
                        extent.Minimum_Y = vertex.Position.Y;
                    if (vertex.Position.Y > extent.Maximum_Y)
                        extent.Maximum_Y = vertex.Position.Y;

                    if (vertex.Position.Z < extent.Minimum_Z)
                        extent.Minimum_Z = vertex.Position.Z;
                    if (vertex.Position.Z > extent.Maximum_Z)
                        extent.Maximum_Z = vertex.Position.Z;
                }

                return extent;
            }

         
        public static Coordinate GetExtent(List<w3Vertex> vertices)
        {
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            foreach (var vertex in vertices)
            {
                minX = Math.Min(minX, vertex.Position.X);
                minY = Math.Min(minY, vertex.Position.Y);
                minZ = Math.Min(minZ, vertex.Position.Z);
                maxX = Math.Max(maxX, vertex.Position.X);
                maxY = Math.Max(maxY, vertex.Position.Y);
                maxZ = Math.Max(maxZ, vertex.Position.Z);
            }
            return new Coordinate
            {
                X = maxX - minX,
                Y = maxY - minY,
                Z = maxZ - minZ
            };
        }
        public static bool IsDistanceLessThan(Coordinate p1, Coordinate p2, float threshold)
        {
            float distanceSquared = DistanceSquared(p1, p2);
            float thresholdSquared = threshold * threshold;
            return distanceSquared < thresholdSquared;
        }
        public static void ScaleToFitIn(Extent limit, List<w3Vertex> vertices)
        {
            float scaleX = (limit.Maximum_X - limit.Minimum_X) / GetExtent(vertices).X;
            float scaleY = (limit.Maximum_Y - limit.Minimum_Y) / GetExtent(vertices).Y;
            float scaleZ = (limit.Maximum_X - limit.Minimum_Z) / GetExtent(vertices).Z;
            foreach (var vertex in vertices)
            {
                vertex.Position.X *= scaleX;
                vertex.Position.Y *= scaleY;
                vertex.Position.Z *= scaleZ;
            }
        }
        public static bool HasZeroArea(Coordinate p1, Coordinate p2, Coordinate p3)
        {
            // Calculate squared distances between vertices
            float distSquared12 = DistanceSquared(p1, p2);
            float distSquared23 = DistanceSquared(p2, p3);
            float distSquared31 = DistanceSquared(p3, p1);
            // Compare squared distances to avoid floating-point precision issues
            float epsilonSquared = 0.15f * 0.15f;
            // Check if any pair of vertices are too close
            if (distSquared12 < epsilonSquared || distSquared23 < epsilonSquared || distSquared31 < epsilonSquared)
            {
                return true;
            }
            return false;
        }
        private static float DistanceSquared(Coordinate p1, Coordinate p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            float dz = p1.Z - p2.Z;
            return dx * dx + dy * dy + dz * dz;
        }
        public static w3Geoset CreateGeosetFromBitmap(Bitmap bitmap)
        {
            w3Geoset geoset = new w3Geoset();
            geoset.Triangles = new List<w3Triangle>();
            geoset.Vertices = new List<w3Vertex>();
            // Create vertices
            w3Vertex vertex1 = new w3Vertex();
            vertex1.Id = IDCounter.Next();
            vertex1.Position = new Coordinate() { X = 0, Y = 0, Z = 0 };
            vertex1.Texture_Position = new Coordinate2D() { U = 0, V = 0 };
            geoset.Vertices.Add(vertex1);
            w3Vertex vertex2 = new w3Vertex();
            vertex2.Position = new Coordinate() { X = bitmap.Width, Y = 0, Z = 0 };
            vertex2.Texture_Position = new Coordinate2D() { U = 1, V = 0 };
            geoset.Vertices.Add(vertex2);
            vertex2.Id = IDCounter.Next();
            w3Vertex vertex3 = new w3Vertex();
            vertex3.Position = new Coordinate() { X = bitmap.Width / 2, Y = bitmap.Height, Z = 0 };
            vertex3.Texture_Position = new Coordinate2D() { U = 0.5f, V = 1 };
            geoset.Vertices.Add(vertex3);
            vertex3.Id = IDCounter.Next();
            w3Vertex vertex4 = new w3Vertex();
            vertex4.Position = new Coordinate() { X = 0, Y = bitmap.Height, Z = 0 };
            vertex4.Texture_Position = new Coordinate2D() { U = 0, V = 1 };
            geoset.Vertices.Add(vertex4);
            vertex4.Id = IDCounter.Next();
            // Create triangles
            w3Triangle triangle1 = new w3Triangle();
            triangle1.Index1 = vertex1.Id;
            triangle1.Index2 =vertex2.Id;
            triangle1.Index3 = vertex3.Id;
            geoset.Triangles.Add(triangle1);
            w3Triangle triangle2 = new w3Triangle();
            triangle2.Index1 =vertex3.Id;
            triangle2.Index2 =vertex2.Id;
            triangle2.Index3 = vertex1.Id;
            geoset.Triangles.Add(triangle2);
            return geoset;
        }
        internal static Coordinate[] GetCubeCoordinatesOfExtent(Extent extent)
        {
            float minX = extent.Minimum_X;
            float minY = extent.Minimum_Y;
            float minZ = extent.Minimum_Z;
            float maxX = extent.Maximum_X;
            float maxY = extent.Maximum_Y;
            float maxZ = extent.Maximum_Z;
            Coordinate[] coordinates =
            [
                new Coordinate { X = minX, Y = minY, Z = minZ },
                new Coordinate { X = maxX, Y = minY, Z = minZ },
                new Coordinate { X = maxX, Y = maxY, Z = minZ },
                new Coordinate { X = minX, Y = maxY, Z = minZ },
                new Coordinate { X = minX, Y = minY, Z = maxZ },
                new Coordinate { X = maxX, Y = minY, Z = maxZ },
                new Coordinate { X = maxX, Y = maxY, Z = maxZ },
                new Coordinate { X = minX, Y = maxY, Z = maxZ },
            ];
            return coordinates;
        }
        internal static Coordinate CurrentCentroid = new Coordinate(0, 0, 0);
        internal static Coordinate GetCentroid(List<w3Vertex> vertices)
        {
            if (vertices.Count == 0) { return new Coordinate(); }
            if (vertices.Count == 1) { return vertices[0].Position; }
            List<Coordinate> coordinates = new List<Coordinate>();
            foreach (w3Vertex v in vertices) { coordinates.Add(v.Position); }
            return GetCentroid(coordinates);
        }
        internal static Coordinate GetCentroid(List<Coordinate> vertices)
        {
            float totalVolume = 0;
            float centerX = 0;
            float centerY = 0;
            float centerZ = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                int next = (i + 1) % vertices.Count;
                float crossProductX = (vertices[i].Y - vertices[next].Y) * (vertices[i].Z + vertices[next].Z);
                float crossProductY = (vertices[i].Z - vertices[next].Z) * (vertices[i].X + vertices[next].X);
                float crossProductZ = (vertices[i].X - vertices[next].X) * (vertices[i].Y + vertices[next].Y);
                totalVolume += (crossProductX + crossProductY + crossProductZ);
                centerX += (vertices[i].X + vertices[next].X) * (crossProductX + crossProductY + crossProductZ);
                centerY += (vertices[i].Y + vertices[next].Y) * (crossProductX + crossProductY + crossProductZ);
                centerZ += (vertices[i].Z + vertices[next].Z) * (crossProductX + crossProductY + crossProductZ);
            }
            totalVolume /= 6;
            centerX /= (24 * totalVolume);
            centerY /= (24 * totalVolume);
            centerZ /= (24 * totalVolume);
            return new Coordinate(centerX, centerY, centerZ);
        }
        internal static Coordinate GetMiddleCoordinate(Coordinate coord1, Coordinate coord2)
        {
            float midX = (coord1.X + coord2.X) / 2;
            float midY = (coord1.Y + coord2.Y) / 2;
            float midZ = (coord1.Z + coord2.Z) / 2;
            return new Coordinate(midX, midY, midZ);
        }
        internal static bool VerticesFormFlatSurface(List<Coordinate> vertices)
        {
            if (vertices == null || vertices.Count < 3)
                return false;
            // Check if all vertices have the same x, y, or z coordinate
            bool isFlatX = AreEqual(vertices, v => v.X);
            bool isFlatY = AreEqual(vertices, v => v.Y);
            bool isFlatZ = AreEqual(vertices, v => v.Z);
            return isFlatX || isFlatY || isFlatZ;
        }
        private static bool AreEqual(List<Coordinate> vertices, Func<Coordinate, double> selector)
        {
            double referenceValue = selector(vertices[0]);
            foreach (var vertex in vertices)
            {
                if (selector(vertex) != referenceValue)
                    return false;
            }
            return true;
        }
        internal static bool IsFlatSurface(List<w3Vertex> vertices)
        {
            if (vertices.Count <= 1) {return false;}
            // Check if all coordinates have the same value in any axis (x, y, or z)
            return AllSameValue(vertices.Select(x=>x.Position).ToList(), "X") ||
                   AllSameValue(vertices.Select(x => x.Position).ToList(), "Y") ||
                   AllSameValue(vertices.Select(x => x.Position).ToList(), "Z");
        }
        internal static class FlatCkecker
        {
            internal static bool IsFlatSurface(List<w3Triangle> triangles)
            {
                if (triangles == null || triangles.Count == 0)
                    return false;

                // Compute the normal of the first triangle
                var firstTriangle = triangles[0];
                var normal = ComputeNormal(firstTriangle);

                // Check if all triangles are in the same plane
                foreach (var triangle in triangles)
                {
                    var currentNormal = ComputeNormal(triangle);
                    if (!AreNormalsParallel(normal, currentNormal))
                        return false;
                }

                // Check if all triangles are connected
                if (!AreTrianglesConnected(triangles))
                    return false;

                return true;
            }

            // Helper method to compute the normal of a triangle
            private static Coordinate ComputeNormal(w3Triangle triangle)
            {
                var v1 = triangle.Vertex1.Position;
                var v2 = triangle.Vertex2.Position;
                var v3 = triangle.Vertex3.Position;

                var edge1 = Subtract(v2, v1);
                var edge2 = Subtract(v3, v1);

                return Normalize(CrossProduct(edge1, edge2));
            }

            // Helper method to determine if two normals are parallel
            private static bool AreNormalsParallel(Coordinate normal1, Coordinate normal2)
            {
                var cross = CrossProduct(normal1, normal2);
                return Length(cross) < 1e-6; // Use a tolerance for floating-point precision
            }

            // Helper method to check if triangles are connected
            private static bool AreTrianglesConnected(List<w3Triangle> triangles)
            {
                var visited = new HashSet<w3Triangle>();
                var toVisit = new Queue<w3Triangle>();

                toVisit.Enqueue(triangles[0]);

                while (toVisit.Count > 0)
                {
                    var current = toVisit.Dequeue();
                    if (!visited.Add(current))
                        continue;

                    foreach (var neighbor in GetAdjacentTriangles(current, triangles))
                    {
                        if (!visited.Contains(neighbor))
                            toVisit.Enqueue(neighbor);
                    }
                }

                return visited.Count == triangles.Count; // All triangles must be visited
            }

            // Helper method to find triangles adjacent to a given triangle
            private static List<w3Triangle> GetAdjacentTriangles(w3Triangle triangle, List<w3Triangle> triangles)
            {
                var neighbors = new List<w3Triangle>();

                foreach (var other in triangles)
                {
                    if (other == triangle) continue;

                    if (ShareEdge(triangle, other))
                        neighbors.Add(other);
                }

                return neighbors;
            }

            // Helper method to determine if two triangles share an edge
            private static bool ShareEdge(w3Triangle t1, w3Triangle t2)
            {
                var edges1 = new HashSet<(Coordinate, Coordinate)>
    {
        (t1.Vertex1.Position, t1.Vertex2.Position),
        (t1.Vertex2.Position, t1.Vertex3.Position),
        (t1.Vertex3.Position, t1.Vertex1.Position)
    };

                var edges2 = new HashSet<(Coordinate, Coordinate)>
    {
        (t2.Vertex1.Position, t2.Vertex2.Position),
        (t2.Vertex2.Position, t2.Vertex3.Position),
        (t2.Vertex3.Position, t2.Vertex1.Position)
    };

                foreach (var edge in edges1)
                {
                    var reversedEdge = (edge.Item2, edge.Item1);
                    if (edges2.Contains(edge) || edges2.Contains(reversedEdge))
                        return true;
                }

                return false;
            }

            // Coordinate math utility methods
            private static Coordinate Subtract(Coordinate a, Coordinate b)
            {
                return new Coordinate { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
            }

            private static Coordinate CrossProduct(Coordinate a, Coordinate b)
            {
                return new Coordinate
                {
                    X = a.Y * b.Z - a.Z * b.Y,
                    Y = a.Z * b.X - a.X * b.Z,
                    Z = a.X * b.Y - a.Y * b.X
                };
            }

            private static Coordinate Normalize(Coordinate a)
            {
                var length = Length(a);
                return new Coordinate { X = a.X / length, Y = a.Y / length, Z = a.Z / length };
            }

            private static float Length(Coordinate a)
            {
                return (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
            }

        }

        public static bool IsUninterruptedSurface(List<w3Triangle> triangles)
        {
            var edgeCount = new Dictionary<Tuple<w3Vertex, w3Vertex>, int>();

            // Count edges for each triangle
            foreach (var triangle in triangles)
            {
                foreach (var edge in triangle.GetEdges())
                {
                    // Ensure edges are in a consistent order based on vertex references
                    var orderedEdge = edge.Item1.GetHashCode() < edge.Item2.GetHashCode()
                        ? Tuple.Create(edge.Item1, edge.Item2)
                        : Tuple.Create(edge.Item2, edge.Item1);

                    if (!edgeCount.ContainsKey(orderedEdge))
                    {
                        edgeCount[orderedEdge] = 0;
                    }
                    edgeCount[orderedEdge]++;
                }
            }

            // Check that each edge is shared exactly twice
            return edgeCount.Values.All(count => count == 2);
        }

        private static bool AllSameValue(List<Coordinate> coordinates, string axis)
        {
            double value = 0;
            foreach (var coord in coordinates)
            {
                double coordValue = axis switch
                {
                    "X" => coord.X,
                    "Y" => coord.Y,
                    "Z" => coord.Z,
                    _ => throw new ArgumentException("Invalid axis"),
                };
                if (value == 0)
                    value = coordValue;
                else if (coordValue != value)
                    return false;
            }
            return true;
        }
        internal static double DistanceBetween(w3Vertex one, w3Vertex two)
        {
            return DistanceBetween(one.Position.X, one.Position.Y, one.Position.Z, two.Position.X, two.Position.Y, two.Position.Z);
        }
            internal static double DistanceBetween(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            // Calculate the differences between coordinates
            double deltaX = x2 - x1;
            double deltaY = y2 - y1;
            double deltaZ = z2 - z1;
            // Calculate the square of the differences
            double deltaXSquare = deltaX * deltaX;
            double deltaYSquare = deltaY * deltaY;
            double deltaZSquare = deltaZ * deltaZ;
            // Calculate the sum of squares
            double sumOfSquares = deltaXSquare + deltaYSquare + deltaZSquare;
            // Calculate the square root of the sum
            double distance = Math.Sqrt(sumOfSquares);
            return distance;
        }
        internal static float[] RotateVertex(float vertexPositionX, float vertexPositionY, float vertexPositionZ,
                                       float bonePositionX, float bonePositionY, float bonePositionZ,
                                       float rotationXAmountDegrees, float rotationYAmountDegrees, float rotationZAmountDegrees)
        {
            // Convert rotation amounts to radians
            float rotationXAmountRadians = DegreesToRadians(rotationXAmountDegrees);
            float rotationYAmountRadians = DegreesToRadians(rotationYAmountDegrees);
            float rotationZAmountRadians = DegreesToRadians(rotationZAmountDegrees);
            // Translate the vertex so that the bone position is at the origin
            float translatedVertexX = vertexPositionX - bonePositionX;
            float translatedVertexY = vertexPositionY - bonePositionY;
            float translatedVertexZ = vertexPositionZ - bonePositionZ;
            // Apply rotation around X-axis
            float tempY = translatedVertexY;
            float tempZ = translatedVertexZ;
            translatedVertexY = (float)(tempY * Math.Cos(rotationXAmountRadians) - tempZ * Math.Sin(rotationXAmountRadians));
            translatedVertexZ = (float)(tempY * Math.Sin(rotationXAmountRadians) + tempZ * Math.Cos(rotationXAmountRadians));
            // Apply rotation around Y-axis
            double tempX = translatedVertexX;
            translatedVertexX = (float)(tempX * Math.Cos(rotationYAmountRadians) + translatedVertexZ * Math.Sin(rotationYAmountRadians));
            translatedVertexZ = (float)(-tempX * Math.Sin(rotationYAmountRadians) + translatedVertexZ * Math.Cos(rotationYAmountRadians));
            // Apply rotation around Z-axis
            tempX = translatedVertexX;
            tempY = translatedVertexY;
            translatedVertexX = (float)(tempX * Math.Cos(rotationZAmountRadians) - tempY * Math.Sin(rotationZAmountRadians));
            translatedVertexY = (float)(tempX * Math.Sin(rotationZAmountRadians) + tempY * Math.Cos(rotationZAmountRadians));
            // Translate the vertex back to its original position
            translatedVertexX += bonePositionX;
            translatedVertexY += bonePositionY;
            translatedVertexZ += bonePositionZ;
            return  [ translatedVertexX, translatedVertexY, translatedVertexZ ];
        }
        private static float DegreesToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180.0);
        }
        internal static Coordinate CalculateCentroid(Coordinate coord1, Coordinate coord2, Coordinate coord3)
        {
            float centroidX = (float)((coord1.X + coord2.X + coord3.X) / 3.0);
            float centroidY = (float)((coord1.Y + coord2.Y + coord3.Y) / 3.0);
            float centroidZ = (float)((coord1.Z + coord2.Z + coord3.Z) / 3.0);
            return new Coordinate(centroidX, centroidY, centroidZ);
        }
        internal static Coordinate2D CalculateCentroid2D(Coordinate2D coord1, Coordinate2D coord2, Coordinate2D coord3)
        {
            float centroidX = (coord1.U + coord2.U + coord3.U) / 3.0f;
            float centroidY = (coord1.V + coord2.V + coord3.V) / 3.0f;
            return new Coordinate2D(centroidX, centroidY);
        }
        internal static bool AreAllCoordinatesConnected(List<Coordinate> coordinates)
        {
            if (coordinates.Count <= 1) { return true; }
            // Build adjacency list
            Dictionary<Coordinate, List<Coordinate>> adjacencyList = BuildAdjacencyList(coordinates);
            // Initialize all vertices as not visited
            Dictionary<Coordinate, bool> visited = new Dictionary<Coordinate, bool>();
            foreach (var coordinate in coordinates)
            {
                visited[coordinate] = false;
            }
            // Start BFS from the first coordinate
            Queue<Coordinate> queue = new Queue<Coordinate>();
            queue.Enqueue(coordinates[0]);
            visited[coordinates[0]] = true;
            // Perform BFS traversal
            while (queue.Count > 0)
            {
                Coordinate current = queue.Dequeue();
                // Visit all adjacent coordinates
                foreach (var neighbor in adjacencyList[current])
                {
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            // Check if all coordinates are visited
            foreach (var coordinate in coordinates)
            {
                if (!visited[coordinate])
                    return false; // There's a gap, not all coordinates are connected
            }
            return true; // All coordinates are connected
        }
        private static Dictionary<Coordinate, List<Coordinate>> BuildAdjacencyList(List<Coordinate> coordinates)
        {
            Dictionary<Coordinate, List<Coordinate>> adjacencyList = new Dictionary<Coordinate, List<Coordinate>>();
            // Build adjacency list
            for (int i = 0; i < coordinates.Count; i++)
            {
                adjacencyList[coordinates[i]] = new List<Coordinate>();
                for (int j = i + 1; j < coordinates.Count; j++)
                {
                    if (AreCoordinatesConnected(coordinates[i], coordinates[j]))
                    {
                        adjacencyList[coordinates[i]].Add(coordinates[j]);
                        adjacencyList[coordinates[j]].Add(coordinates[i]);
                    }
                }
            }
            return adjacencyList;
        }
        private static bool AreCoordinatesConnected(Coordinate coord1, Coordinate coord2)
        {
            // Replace this with your logic to determine if coord1 and coord2 can be connected
            // For example, you might check if the distance between them is within a certain threshold
            return true; // For demonstration purposes, always return true
        }
        internal static bool HasExactlyTwoIslands(List<int> vertexIDs, List<w3Triangle> triangles)
        {
            // Build adjacency list
            Dictionary<int, List<int>> adjacencyList = BuildAdjacencyList(vertexIDs, triangles);
            // Perform graph traversal
            int islandCount = 0;
            HashSet<int> visited = new HashSet<int>();
            foreach (int vertexID in vertexIDs)
            {
                if (!visited.Contains(vertexID))
                {
                    TraverseGraph(adjacencyList, visited, vertexID);
                    islandCount++;
                }
            }
            return islandCount == 2;
        }
        private static void TraverseGraph(Dictionary<int, List<int>> adjacencyList, HashSet<int> visited, int vertexID)
        {
            visited.Add(vertexID);
            foreach (int neighborID in adjacencyList[vertexID])
            {
                if (!visited.Contains(neighborID))
                {
                    TraverseGraph(adjacencyList, visited, neighborID);
                }
            }
        }
        private static Dictionary<int, List<int>> BuildAdjacencyList(List<int> vertexIDs, List<w3Triangle> triangles)
        {
            Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
            // Initialize adjacency list
            foreach (int vertexID in vertexIDs)
            {
                adjacencyList[vertexID] = new List<int>();
            }
            // Build adjacency list from triangles
            foreach (var triangle in triangles)
            {
                AddEdge(adjacencyList, triangle.Index1, triangle.Index2);
                AddEdge(adjacencyList, triangle.Index2, triangle.Index3);
                AddEdge(adjacencyList, triangle.Index3, triangle.Index1);
            }
            return adjacencyList;
        }
        private static void AddEdge(Dictionary<int, List<int>> adjacencyList, int vertexID1, int vertexID2)
        {
            if (!adjacencyList[vertexID1].Contains(vertexID2))
            {
                adjacencyList[vertexID1].Add(vertexID2);
            }
            if (!adjacencyList[vertexID2].Contains(vertexID1))
            {
                adjacencyList[vertexID2].Add(vertexID1);
            }
        }
        public static void ScaleTriangle(Coordinate t1a, Coordinate t1b, Coordinate t1c,
                                     Coordinate t2a, Coordinate t2b, Coordinate t2c)
        {
            float GetDistance(Coordinate p1, Coordinate p2)
            {
                return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2));
            }
            // Calculate the side lengths of the first triangle
            float side1A = GetDistance(t1a, t1b);
            float side1B = GetDistance(t1b, t1c);
            float side1C = GetDistance(t1c, t1a);
            // Calculate the side lengths of the second triangle
            float side2A = GetDistance(t2a, t2b);
            float side2B = GetDistance(t2b, t2c);
            float side2C = GetDistance(t2c, t2a);
            // Calculate the scaling factor
            float scalingFactorA = side1A / side2A;
            float scalingFactorB = side1B / side2B;
            float scalingFactorC = side1C / side2C;
            // Average the scaling factors to ensure uniform scaling
            float scalingFactor = (scalingFactorA + scalingFactorB + scalingFactorC) / 3.0f;
            // Assume t2a is the reference point for scaling
            t2b.ScaleFrom(t2a, scalingFactor);
            t2c.ScaleFrom(t2a, scalingFactor);
        }
        private static double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
      internal  static void MoveTriangleInFacingDirection(double[] triangleCoordinates, double distance)
        {
            // Calculate the normal vector of the triangle's surface
            double[] normalVector = CalculateNormalVector(triangleCoordinates);
            // Move all points of the triangle in the direction of the normal vector
            for (int i = 0; i < triangleCoordinates.Length; i++)
            {
                triangleCoordinates[i] += normalVector[i % 3] * distance;
            }
        }
       internal static void MoveTriangleInOppositeFacingDirection(double[] triangleCoordinates, double distance)
        {
            // Calculate the normal vector of the triangle's surface
            double[] normalVector = CalculateNormalVector(triangleCoordinates);
            // Move all points of the triangle in the opposite direction of the normal vector
            for (int i = 0; i < triangleCoordinates.Length; i++)
            {
                triangleCoordinates[i] -= normalVector[i % 3] * distance;
            }
        }
      private  static double[] CalculateNormalVector(double[] triangleCoordinates)
        {
            // Calculate vectors for two sides of the triangle
            double[] vector1 = {
            triangleCoordinates[3] - triangleCoordinates[0],
            triangleCoordinates[4] - triangleCoordinates[1],
            triangleCoordinates[5] - triangleCoordinates[2]
        };
            double[] vector2 = {
            triangleCoordinates[6] - triangleCoordinates[0],
            triangleCoordinates[7] - triangleCoordinates[1],
            triangleCoordinates[8] - triangleCoordinates[2]
        };
            // Calculate the cross product of the two vectors to get the normal vector
            double[] normalVector = {
            vector1[1] * vector2[2] - vector1[2] * vector2[1],
            vector1[2] * vector2[0] - vector1[0] * vector2[2],
            vector1[0] * vector2[1] - vector1[1] * vector2[0]
        };
            return normalVector;
        }
        internal static Extent CalculateCollectiveExtent(List<Extent> extents)
        {
            if (extents == null || extents.Count == 0) { return new Extent(); }
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            foreach (var extent in extents)
            {
                minX = Math.Min(minX, extent.Minimum_X);
                minY = Math.Min(minY, extent.Minimum_Y);
                minZ = Math.Min(minZ, extent.Minimum_Z);
                maxX = Math.Max(maxX, extent.Maximum_X);
                maxY = Math.Max(maxY, extent.Maximum_Y);
                maxZ = Math.Max(maxZ, extent.Maximum_Z);
            }
            return new Extent
            {
                Minimum_X = minX,
                Minimum_Y = minY,
                Minimum_Z = minZ,
                Maximum_X = maxX,
                Maximum_Y = maxY,
                Maximum_Z = maxZ
            };
        }
        internal static Coordinate CoordinateDifference(Coordinate vPos, Coordinate bonePos)
        {
            return new Coordinate()
            {
                X = Math.Abs( vPos.X - bonePos.X),
                Y = Math.Abs( vPos.Y - bonePos.Y),
                Z = Math.Abs( vPos.Z - bonePos.Z)
            };
        }
        internal static Coordinate RotateAroundBone(Coordinate vertex, Coordinate bone, float angleX, float angleY, float angleZ)
        {
            // Translate vertex position to bone-centered coordinates
            float translatedX = vertex.X - bone.X;
            float translatedY = vertex.Y - bone.Y;
            float translatedZ = vertex.Z - bone.Z;
            // Convert angles from degrees to radians
            float radX = angleX * (float)Math.PI / 180f;
            float radY = angleY * (float)Math.PI / 180f;
            float radZ = angleZ * (float)Math.PI / 180f;
            // Rotation around X-axis
            float cosX = (float)Math.Cos(radX);
            float sinX = (float)Math.Sin(radX);
            float y1 = cosX * translatedY - sinX * translatedZ;
            float z1 = sinX * translatedY + cosX * translatedZ;
            // Rotation around Y-axis
            float cosY = (float)Math.Cos(radY);
            float sinY = (float)Math.Sin(radY);
            float x2 = cosY * translatedX + sinY * z1;
            float z2 = -sinY * translatedX + cosY * z1;
            // Rotation around Z-axis
            float cosZ = (float)Math.Cos(radZ);
            float sinZ = (float)Math.Sin(radZ);
            float x3 = cosZ * x2 - sinZ * y1;
            float y3 = sinZ * x2 + cosZ * y1;
            // Translate back to the original coordinate system
            float finalX = x3 + bone.X;
            float finalY = y3 + bone.Y;
            float finalZ = z2 + bone.Z;
            return new Coordinate { X = finalX, Y = finalY, Z = finalZ };
        }
        internal static Extent CalculateCollectiveExtentFromCoordinates(List<Coordinate> coordinates)
        {
            Extent extent = new Extent();
            foreach (var coord in coordinates)
            {
                if (coord.X < extent.Minimum_X) extent.Minimum_X = coord.X;
                if (coord.Y < extent.Minimum_Y) extent.Minimum_Y = coord.Y;
                if (coord.Z < extent.Minimum_Z) extent.Minimum_Z = coord.Z;
                if (coord.X > extent.Maximum_X) extent.Maximum_X = coord.X;
                if (coord.Y > extent.Maximum_Y) extent.Maximum_Y = coord.Y;
                if (coord.Z > extent.Maximum_Z) extent.Maximum_Z = coord.Z;
            }
            return extent;
        }
        public static void PositionVerticesAt(List<w3Vertex> vertices, Coordinate newCentroid)
        {
            // Extract current coordinates of vertices
            List<Coordinate> coords = vertices.Select(x => x.Position).ToList();
            // Calculate current centroid
            Coordinate currentCentroid = Calculator3D.GetCentroid(coords);
            // Calculate the translation vector from current centroid to new centroid
            float translationX = newCentroid.X - currentCentroid.X;
            float translationY = newCentroid.Y - currentCentroid.Y;
            float translationZ = newCentroid.Z - currentCentroid.Z;
            // Translate each vertex's position by the translation vector
            foreach (var vertex in vertices)
            {
                vertex.Position = new Coordinate(
                    vertex.Position.X + translationX,
                    vertex.Position.Y + translationY,
                    vertex.Position.Z + translationZ
                );
            }
        }
        public static void PositionVerticesAtX(List<w3Vertex> vertices, float X)
        {
            // Extract current coordinates of vertices
            List<Coordinate> coords = vertices.Select(x => x.Position).ToList();
            // Calculate current centroid
            Coordinate currentCentroid = Calculator3D.GetCentroid(coords);
            Coordinate newCentroid = new Coordinate(X, currentCentroid.Y, currentCentroid.Z);
            // Calculate the translation vector from current centroid to new centroid
            float translationX = newCentroid.X - currentCentroid.X;
            float translationY = newCentroid.Y - currentCentroid.Y;
            float translationZ = newCentroid.Z - currentCentroid.Z;
            // Translate each vertex's position by the translation vector
            foreach (var vertex in vertices)
            {
                vertex.Position = new Coordinate(
                    vertex.Position.X + translationX,
                    vertex.Position.Y + translationY,
                    vertex.Position.Z + translationZ
                );
            }
        }
        public static void PositionVerticesAtY(List<w3Vertex> vertices, float Y)
        {
            // Extract current coordinates of vertices
            List<Coordinate> coords = vertices.Select(x => x.Position).ToList();
            // Calculate current centroid
            Coordinate currentCentroid = Calculator3D.GetCentroid(coords);
            Coordinate newCentroid = new Coordinate(currentCentroid.X, Y, currentCentroid.Z);
            // Calculate the translation vector from current centroid to new centroid
            float translationX = newCentroid.X - currentCentroid.X;
            float translationY = newCentroid.Y - currentCentroid.Y;
            float translationZ = newCentroid.Z - currentCentroid.Z;
            // Translate each vertex's position by the translation vector
            foreach (var vertex in vertices)
            {
                vertex.Position = new Coordinate(
                    vertex.Position.X + translationX,
                    vertex.Position.Y + translationY,
                    vertex.Position.Z + translationZ
                );
            }
        }
        public static void PositionVerticesAtZ(List<w3Vertex> vertices, float Z)
        {
            // Extract current coordinates of vertices
            List<Coordinate> coords = vertices.Select(x => x.Position).ToList();
            // Calculate current centroid
            Coordinate currentCentroid = Calculator3D.GetCentroid(coords);
            Coordinate newCentroid = new Coordinate(currentCentroid.X, currentCentroid. Y, Z);
            // Calculate the translation vector from current centroid to new centroid
            float translationX = newCentroid.X - currentCentroid.X;
            float translationY = newCentroid.Y - currentCentroid.Y;
            float translationZ = newCentroid.Z - currentCentroid.Z;
            // Translate each vertex's position by the translation vector
            foreach (var vertex in vertices)
            {
                vertex.Position = new Coordinate(
                    vertex.Position.X + translationX,
                    vertex.Position.Y + translationY,
                    vertex.Position.Z + translationZ
                );
            }
        }
        internal static void makeEquilateral(w3Triangle triangle)
        {
            w3Vertex v1 = FindVertex(triangle.Index1);
            w3Vertex v2 = FindVertex(triangle.Index2);
            w3Vertex v3 = FindVertex(triangle.Index3);
            // Calculate the centroid of the triangle
            Coordinate centroid = new Coordinate
            {
                X = (v1.Position.X + v2.Position.X + v3.Position.X) / 3.0f,
                Y = (v1.Position.Y + v2.Position.Y + v3.Position.Y) / 3.0f,
                Z = (v1.Position.Z + v2.Position.Z + v3.Position.Z) / 3.0f
            };
            // Calculate the side length of the equilateral triangle
            float sideLength = GetDistance(v1.Position, v2.Position);
            // Calculate new positions for vertices
            v1.Position = new Coordinate
            {
                X = centroid.X + (float)(sideLength / Math.Sqrt(3)),
                Y = centroid.Y,
                Z = centroid.Z
            };
            v2.Position = new Coordinate
            {
                X = centroid.X - (float)(sideLength / (2 * Math.Sqrt(3))),
                Y = centroid.Y + (float)(sideLength / 2),
                Z = centroid.Z
            };
            v3.Position = new Coordinate
            {
                X = centroid.X - (float)(sideLength / (2 * Math.Sqrt(3))),
                Y = centroid.Y - (float)(sideLength / 2),
                Z = centroid.Z
            };
            // Update the triangle vertices
            UpdateVertexPosition(triangle.Index1, v1.Position);
            UpdateVertexPosition(triangle.Index2, v2.Position);
            UpdateVertexPosition(triangle.Index3, v3.Position);
        }
        private static float GetDistance(Coordinate p1, Coordinate p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
        }
        // Mockup methods for demonstration
        private static List<int> GetSelectedTriangles() => new List<int>();
        private static List<w3Triangle> GetSelectedTrianglesFromIDs(List<int> ids) => new List<w3Triangle>();
        private static w3Vertex FindVertex(int id) => new w3Vertex { Position = new Coordinate() };
        private static void UpdateVertexPosition(int id, Coordinate position) { }
        internal static bool AllVerticesUseTheSameBone(List<w3Vertex> vertices)
        {
           if (vertices.Count <= 1) { return true; }
            List<int> first = vertices[0].AttachedTo;
            for (int i = 1; i< vertices.Count; i++)
            {
                if (AreEquivalent(first, vertices[i].AttachedTo) == false) { return false; }
            }
            return false;
        }
        public static bool AreEquivalent(List<int> list1, List<int> list2)
        {
            // Check if both lists are not null and have the same count
            if (list1 == null || list2 == null || list1.Count != list2.Count)
                return false;
            // Sort both lists
            list1.Sort();
            list2.Sort();
            // Compare sorted lists element by element
            return list1.SequenceEqual(list2);
        }

        public static w3Vertex GetMiddleAsNewVertex(w3Vertex v1, w3Vertex v2, w3Vertex v3)
        {
            return new w3Vertex(IDCounter.Next(),
                (v1.Position.X + v2.Position.X + v3.Position.X) / 3,
                (v1.Position.Y + v2.Position.Y + v3.Position.Y) / 3,
                (v1.Position.Z + v2.Position.Z + v3.Position.Z) / 3, v1.AttachedTo.ToList()
            )
            ;
        }

        internal static w3Vertex GetMiddleAsNewVertex(w3Vertex vertex1, w3Vertex vertex2)
        {
            // Calculate the midpoint coordinates
            float midX = (vertex1.Position.X + vertex2.Position.X) / 2;
            float midY = (vertex1.Position.Y + vertex2.Position.Y) / 2;

            // If w3Vertex has a 3D position (Z coordinate), include it
            float midZ = (vertex1.Position.Z + vertex2.Position.Z) / 2;

            // Create a new Coordinate object for the midpoint
            Coordinate newVertexCoordinate = new Coordinate
            {
                X = midX,
                Y = midY,
                Z = midZ // Include Z if applicable
            };

            // Return a new w3Vertex with the midpoint coordinate
            return new w3Vertex { Position = newVertexCoordinate };
        }

        internal static void SetGeometryChangeBasedOnCoordinates(List<w3Vertex> vertices, Extent ex, float x, float y, float z)
        {
            // Step 1: Calculate the current centroid of the selected vertices
            Coordinate oldCentroid = CalculateCentroidFromVertices(vertices);

            // Step 2: Shift the extent by the given amounts (optional, depending on use case)
            Extent newExtent = ex.Clone();
            newExtent.Maximum_X += x;
            newExtent.Minimum_X += x;
            newExtent.Maximum_Y += y;
            newExtent.Minimum_Y += y;
            newExtent.Maximum_Z += z;
            newExtent.Minimum_Z += z;

            // Step 3: Calculate the new centroid after the shift
            Coordinate newCentroid = new Coordinate
            {
                X = oldCentroid.X + x,
                Y = oldCentroid.Y + y,
                Z = oldCentroid.Z + z
            };

            // Step 4: Compute the delta between the old and new centroids
            float deltaX = newCentroid.X - oldCentroid.X;
            float deltaY = newCentroid.Y - oldCentroid.Y;
            float deltaZ = newCentroid.Z - oldCentroid.Z;

            // Step 5: Apply the delta to each vertex's position to move them consistently
            foreach (w3Vertex v in vertices)
            {
                v.Position.X += deltaX;
                v.Normal.X += deltaX;
                v.Position.Y += deltaY;
                v.Normal.Y += deltaY;
                v.Position.Z += deltaZ;
                v.Normal.Z += deltaZ;
            }
        }

        internal static void ScaleVerticesBasedOnReference(List<w3Vertex> vertices, float percentX, float percentY, float percentZ)
        {
            // Convert percentages to normalized scale factors (e.g., 150% -> 1.5)
            float scaleX = percentX / 100;
            float scaleY = percentY / 100;
            float scaleZ = percentZ / 100;

            // Step 1: Calculate the centroid of the selected vertices
            Coordinate centroid = CalculateCentroidFromVertices(vertices);

            // Step 2: Apply scaling to each vertex relative to the centroid
            foreach (w3Vertex v in vertices)
            {
                // Calculate the distance from the vertex to the centroid
                float deltaX = v.Position.X - centroid.X;
                float deltaY = v.Position.Y - centroid.Y;
                float deltaZ = v.Position.Z - centroid.Z;

                // Scale the distance from the centroid by the scale factor for each axis
                v.Position.X = centroid.X + deltaX * scaleX;
                v.Position.Y = centroid.Y + deltaY * scaleY;
                v.Position.Z = centroid.Z + deltaZ * scaleZ;
            }
        }

        internal static void RotateVerticesBasedOnRotation(List<w3Vertex> vertices, float rotateX, float rotateY, float rotateZ)
        {
            // Step 1: Calculate the centroid of the selected vertices
            Coordinate centroid = CalculateCentroidFromVertices(vertices);

            // Step 2: Convert the Euler angles from degrees to radians
            float radX = (float)(rotateX * Math.PI / 180);
            float radY = (float)(rotateY * Math.PI / 180);
            float radZ = (float)(rotateZ * Math.PI / 180);

            // Step 3: Apply rotation to each vertex relative to the centroid
            foreach (w3Vertex v in vertices)
            {
                // Translate vertex to origin (centroid becomes the new origin)
                float deltaX = v.Position.X - centroid.X;
                float deltaY = v.Position.Y - centroid.Y;
                float deltaZ = v.Position.Z - centroid.Z;

                // Rotation around Z-axis
                float tempX_Z = deltaX * (float)Math.Cos(radZ) - deltaY * (float)Math.Sin(radZ);
                float tempY_Z = deltaX * (float)Math.Sin(radZ) + deltaY * (float)Math.Cos(radZ);
                float tempZ_Z = deltaZ; // No change in Z-axis

                // Rotation around Y-axis (after Z)
                float tempX_Y = tempX_Z * (float)Math.Cos(radY) + tempZ_Z * (float)Math.Sin(radY);
                float tempY_Y = tempY_Z; // No change in Y-axis
                float tempZ_Y = -tempX_Z * (float)Math.Sin(radY) + tempZ_Z * (float)Math.Cos(radY);

                // Rotation around X-axis (after Y and Z)
                float finalX = tempX_Y;
                float finalY = tempY_Y * (float)Math.Cos(radX) - tempZ_Y * (float)Math.Sin(radX);
                float finalZ = tempY_Y * (float)Math.Sin(radX) + tempZ_Y * (float)Math.Cos(radX);

                // Translate the vertex back from origin (centroid)
                v.Position.X = centroid.X + finalX;
                v.Position.Y = centroid.Y + finalY;
                v.Position.Z = centroid.Z + finalZ;
            }
        }

        internal static void RotateNormalsBasedOnRotation(List<w3Vertex> vertices, float rotateX, float rotateY, float rotateZ)
        {
            // Step 1: Convert the Euler angles from degrees to radians
            float radX = (float)(rotateX * Math.PI / 180);
            float radY = (float)(rotateY * Math.PI / 180);
            float radZ = (float)(rotateZ * Math.PI / 180);

            // Step 2: Apply rotation to each vertex's normal
            foreach (w3Vertex v in vertices)
            {
                // Extract the normal vector components
                float nx = v.Normal.X;
                float ny = v.Normal.Y;
                float nz = v.Normal.Z;

                // Rotation around Z-axis
                float tempX_Z = nx * (float)Math.Cos(radZ) - ny * (float)Math.Sin(radZ);
                float tempY_Z = nx * (float)Math.Sin(radZ) + ny * (float)Math.Cos(radZ);
                float tempZ_Z = nz; // No change in Z-axis

                // Rotation around Y-axis (after Z)
                float tempX_Y = tempX_Z * (float)Math.Cos(radY) + tempZ_Z * (float)Math.Sin(radY);
                float tempY_Y = tempY_Z; // No change in Y-axis
                float tempZ_Y = -tempX_Z * (float)Math.Sin(radY) + tempZ_Z * (float)Math.Cos(radY);

                // Rotation around X-axis (after Y and Z)
                float finalX = tempX_Y;
                float finalY = tempY_Y * (float)Math.Cos(radX) - tempZ_Y * (float)Math.Sin(radX);
                float finalZ = tempY_Y * (float)Math.Sin(radX) + tempZ_Y * (float)Math.Cos(radX);

                // Set the rotated normal back to the vertex
                v.Normal.X = finalX;
                v.Normal.Y = finalY;
                v.Normal.Z = finalZ;
            }
        }

        internal static Coordinate GetCentroidFromExtent(Extent selectionExtent)
        {
            Coordinate c = new Coordinate();
            c.X = (selectionExtent.Maximum_X + selectionExtent.Minimum_X) / 2;
            c.Y = (selectionExtent.Maximum_Y + selectionExtent.Minimum_Y) / 2;
            c.Z = (selectionExtent.Maximum_Z + selectionExtent.Minimum_Z) / 2;

            return c;
        }

        internal static void SetDistanceBetweenPoints(w3Vertex vertex1, w3Vertex vertex2, float modifyAmount, bool Increase)
        {
            // Calculate the vector between the two vertices
            float dx = vertex2.Position.X - vertex1.Position.X;
            float dy = vertex2.Position.Y - vertex1.Position.Y;
            float dz = vertex2.Position.Z - vertex1.Position.Z;

            // Calculate the current distance
            float currentDistance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

            // If the current distance is 0, we cannot decrease it
            if (currentDistance == 0 && !Increase)
            {
                return; // Do nothing if they are at the same point and we want to decrease distance
            }

            // Determine the new distance
            float newDistance;
            if (Increase)
            {
                newDistance = currentDistance + modifyAmount;
            }
            else
            {
                newDistance = MathF.Max(0, currentDistance - modifyAmount); // Prevent negative distance
            }

            // If the new distance is 0, we don't want to move vertex2 to the same position as vertex1
            if (newDistance == 0)
            {
                return; // Do nothing if we would end up at the same point
            }

            // Calculate the scaling factor to adjust the distance
            float scaleFactor = newDistance / currentDistance;

            // Update the positions of the vertices
            vertex2.Position.X = vertex1.Position.X + dx * scaleFactor;
            vertex2.Position.Y = vertex1.Position.Y + dy * scaleFactor;
            vertex2.Position.Z = vertex1.Position.Z + dz * scaleFactor;
        }

        public static List<w3Vertex> GetOuterVertices(List<w3Vertex> vertices)
        {
            if (vertices.Count <= 3)
            {
                // If there are 3 or fewer vertices, they are all outer vertices
                return new List<w3Vertex>(vertices);
            }

            // Sort the vertices by X coordinate, then by Y coordinate
            var sortedVertices = vertices.OrderBy(v => v.Position.X).ThenBy(v => v.Position.Y).ToList();

            // Build the lower hull
            var lowerHull = new List<w3Vertex>();
            foreach (var vertex in sortedVertices)
            {
                while (lowerHull.Count >= 2 &&
                       Cross(lowerHull[lowerHull.Count - 2], lowerHull[lowerHull.Count - 1], vertex) <= 0)
                {
                    lowerHull.RemoveAt(lowerHull.Count - 1);
                }
                lowerHull.Add(vertex);
            }

            // Build the upper hull
            var upperHull = new List<w3Vertex>();
            for (int i = sortedVertices.Count - 1; i >= 0; i--)
            {
                var vertex = sortedVertices[i];
                while (upperHull.Count >= 2 &&
                       Cross(upperHull[upperHull.Count - 2], upperHull[upperHull.Count - 1], vertex) <= 0)
                {
                    upperHull.RemoveAt(upperHull.Count - 1);
                }
                upperHull.Add(vertex);
            }

            // Remove the last vertex of each half because it's repeated at the beginning of the other half
            upperHull.RemoveAt(upperHull.Count - 1);
            lowerHull.AddRange(upperHull);

            return lowerHull;
        }

        // Helper function to calculate the cross product of vectors
        private static float Cross(w3Vertex o, w3Vertex a, w3Vertex b)
        {
            return (a.Position.X - o.Position.X) * (b.Position. Y - o.Position.Y) - (a.Position.Y - o.Position.Y) * (b.Position.X - o.Position.X);
        }

        internal static void MoveInFacingDirection(w3Triangle triangle, FacingAngle angle, float amount)
        {
            // Convert yaw and pitch from degrees to radians for calculations
            float yawRad = angle.Yaw * (float)Math.PI / 180f;
            float pitchRad = angle.Pitch * (float)Math.PI / 180f;

            // Calculate the direction vector from yaw and pitch
            float directionX = (float)(Math.Cos(pitchRad) * Math.Sin(yawRad));
            float directionY = (float)Math.Sin(pitchRad);
            float directionZ = (float)(Math.Cos(pitchRad) * Math.Cos(yawRad));

            // Scale the direction vector by the movement amount
            float moveX = directionX * amount;
            float moveY = directionY * amount;
            float moveZ = directionZ * amount;

            // Move each vertex of the triangle in the direction vector
            triangle.Vertex1.Position.X += moveX;
            triangle.Vertex1.Position.Y += moveY;
            triangle.Vertex1.Position.Z += moveZ;

            triangle.Vertex2.Position.X += moveX;
            triangle.Vertex2.Position.Y += moveY;
            triangle.Vertex2.Position.Z += moveZ;

            triangle.Vertex3.Position.X += moveX;
            triangle.Vertex3.Position.Y += moveY;
            triangle.Vertex3.Position.Z += moveZ;
        }

        internal static void IncrementCentroidOfTriangle(w3Vertex vertex1, w3Vertex vertex2, w3Vertex vertex3, Whim_Model_Editor.AxisMode axisMode, bool increment, float modifyAmount)
        {
            // Calculate the current centroid of the triangle
            Coordinate centroid = GetCentroid(vertex1, vertex2, vertex3);

            // Determine the modification direction based on axis mode
            float change = increment ? modifyAmount : -modifyAmount;

            // Modify the centroid based on the specified axis mode
            switch (axisMode)
            {
                case Whim_Model_Editor.AxisMode.X:
                    centroid.X += change;
                    break;
                case Whim_Model_Editor.AxisMode.Y:
                    centroid.Y += change;
                    break;
                case Whim_Model_Editor.AxisMode.Z:
                    centroid.Z += change;
                    break;
                
            }

            // Calculate the difference from the original centroid to the new centroid
            float deltaX = centroid.X - GetCentroid(vertex1, vertex2, vertex3).X;
            float deltaY = centroid.Y - GetCentroid(vertex1, vertex2, vertex3).Y;
            float deltaZ = centroid.Z - GetCentroid(vertex1, vertex2, vertex3).Z;

            // Move each vertex towards the new centroid
            vertex1.Position.X += deltaX;
            vertex1.Position.Y += deltaY;
            vertex1.Position.Z += deltaZ;

            vertex2.Position.X += deltaX;
            vertex2.Position.Y += deltaY;
            vertex2.Position.Z += deltaZ;

            vertex3.Position.X += deltaX;
            vertex3.Position.Y += deltaY;
            vertex3.Position.Z += deltaZ;
        }


        private static Coordinate GetCentroid(w3Vertex vertex1, w3Vertex vertex2, w3Vertex vertex3)
        {
            // Calculate the average of the X, Y, and Z coordinates of the vertices
            float centroidX = (vertex1.Position.X + vertex2.Position.X + vertex3.Position.X) / 3.0f;
            float centroidY = (vertex1.Position.Y + vertex2.Position.Y + vertex3.Position.Y) / 3.0f;
            float centroidZ = (vertex1.Position.Z + vertex2.Position.Z + vertex3.Position.Z) / 3.0f;

            // Return the new Coordinate representing the centroid
            return new Coordinate(centroidX, centroidY, centroidZ);
        }

        internal static void AlignEdgeTo(w3Edge w3Edge1, w3Edge w3Edge2, Whim_Model_Editor.AxisMode x)
        {
            Coordinate centroid1 = GetCentroid(new List<w3Vertex>() { w3Edge1.Vertex1, w3Edge1.Vertex2});
            Coordinate centroid2 = GetCentroid(new List<w3Vertex>() { w3Edge2.Vertex1, w3Edge1.Vertex2});
            SetNewCentroid(w3Edge2, centroid1, x);

           
        }
        internal static void SetNewCentroid(w3Edge edge, Coordinate newCentroid, Whim_Model_Editor.AxisMode axisMode)
        {
            // Calculate the current centroid of the edge (average of the two vertices)
            float currentCentroidX = (edge.Vertex1.Position.X + edge.Vertex2.Position.X) / 2.0f;
            float currentCentroidY = (edge.Vertex1.Position.Y + edge.Vertex2.Position.Y) / 2.0f;
            float currentCentroidZ = (edge.Vertex1.Position.Z + edge.Vertex2.Position.Z) / 2.0f;

            // Calculate the offset based on the selected axis
            float offsetX = 0, offsetY = 0, offsetZ = 0;

            switch (axisMode)
            {
                case Whim_Model_Editor.AxisMode.X:
                    offsetX = newCentroid.X - currentCentroidX;
                    break;
                case Whim_Model_Editor.AxisMode.Y:
                    offsetY = newCentroid.Y - currentCentroidY;
                    break;
                case Whim_Model_Editor.AxisMode.Z:
                    offsetZ = newCentroid.Z - currentCentroidZ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axisMode), "Invalid axis mode.");
            }

            // Move both vertices by the calculated offset along the specified axis
            edge.Vertex1.Position.X += offsetX;
            edge.Vertex1.Position.Y += offsetY;
            edge.Vertex1.Position.Z += offsetZ;

            edge.Vertex2.Position.X += offsetX;
            edge.Vertex2.Position.Y += offsetY;
            edge.Vertex2.Position.Z += offsetZ;
        }


        internal static void SetNewCentroid(w3Triangle triangle, Coordinate newCentroid, Whim_Model_Editor.AxisMode axisMode)
        {
            // Calculate the current centroid of the triangle
            Coordinate currentCentroid = GetCentroid(triangle.Vertex1, triangle.Vertex2, triangle.Vertex3);

            // Calculate the offset based on the selected axis
            float offsetX = 0, offsetY = 0, offsetZ = 0;

            switch (axisMode)
            {
                case Whim_Model_Editor.AxisMode.X:
                    offsetX = newCentroid.X - currentCentroid.X;
                    break;
                case Whim_Model_Editor.AxisMode.Y:
                    offsetY = newCentroid.Y - currentCentroid.Y;
                    break;
                case Whim_Model_Editor.AxisMode.Z:
                    offsetZ = newCentroid.Z - currentCentroid.Z;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axisMode), "Invalid axis mode.");
            }

            // Move all three vertices by the calculated offset along the specified axis
            triangle.Vertex1.Position.X += offsetX;
            triangle.Vertex1.Position.Y += offsetY;
            triangle.Vertex1.Position.Z += offsetZ;

            triangle.Vertex2.Position.X += offsetX;
            triangle.Vertex2.Position.Y += offsetY;
            triangle.Vertex2.Position.Z += offsetZ;

            triangle.Vertex3.Position.X += offsetX;
            triangle.Vertex3.Position.Y += offsetY;
            triangle.Vertex3.Position.Z += offsetZ;
        }


        internal static void AlignTriangleTo(w3Triangle w3Triangle1, w3Triangle w3Triangle2, Whim_Model_Editor.AxisMode x)
        {
            Coordinate centroid1 = GetCentroid(new List<w3Vertex>() { w3Triangle1.Vertex1, w3Triangle1.Vertex2, w3Triangle1.Vertex3 });
            Coordinate centroid2 = GetCentroid(new List<w3Vertex>() { w3Triangle2.Vertex1, w3Triangle2.Vertex2, w3Triangle2.Vertex3 });
            
            SetNewCentroid(w3Triangle2, centroid1, x);
        }

        public static void ShiftVerticesBy(List<w3Vertex> vertices, float offset, Whim_Model_Editor.AxisMode axisMode)
        {
            foreach (var vertex in vertices)
            {
                switch (axisMode)
                {
                    case Whim_Model_Editor.AxisMode.X:
                        vertex.Position.X += offset;
                        break;
                    case Whim_Model_Editor.AxisMode.Y:
                        vertex.Position.Y += offset;
                        break;
                    case Whim_Model_Editor.AxisMode.Z:
                        vertex.Position.Z += offset;
                        break;
                     
                }
            }
        }

        internal static void CreateInbetweenTriangleOfInsetTriangle(w3Triangle innerTriangle, w3Triangle outerTriangle, w3Geoset CurrentGeoset)
        {
            List<w3Triangle> generatedTriangles = new List<w3Triangle>();

            // First quadrilateral split into two triangles
            generatedTriangles.Add(new w3Triangle(innerTriangle.Vertex1, outerTriangle.Vertex1, outerTriangle.Vertex2));
            generatedTriangles.Add(new w3Triangle(innerTriangle.Vertex1, outerTriangle.Vertex2, innerTriangle.Vertex2));

            // Second quadrilateral split into two triangles
            generatedTriangles.Add(new w3Triangle(innerTriangle.Vertex2, outerTriangle.Vertex2, outerTriangle.Vertex3));
            generatedTriangles.Add(new w3Triangle(innerTriangle.Vertex2, outerTriangle.Vertex3, innerTriangle.Vertex3));

            // Third quadrilateral split into two triangles
            generatedTriangles.Add(new w3Triangle(innerTriangle.Vertex3, outerTriangle.Vertex3, outerTriangle.Vertex1));
            generatedTriangles.Add(new w3Triangle(innerTriangle.Vertex3, outerTriangle.Vertex1, innerTriangle.Vertex1));

            // Add the generated triangles to the current geoset
            CurrentGeoset.Triangles.AddRange(generatedTriangles);
        }

        internal static void InsetTriangle(w3Triangle triangle, bool increase)
        {
            // Get the centroid of the triangle
            Coordinate centroid = GetCentroid(new List<w3Vertex>() { triangle.Vertex1, triangle.Vertex2, triangle.Vertex3 });

            // Scaling factor: Increase (1.1) or Decrease (0.9)
            float scale = increase ? 1.1f : 0.9f;

            // Scale each vertex relative to the centroid, respecting the inset constraint
            triangle.Vertex1.Position = ScaleVertex(triangle.Vertex1.Position, centroid, scale, triangle.InsetConstaint.one.Position);
            triangle.Vertex2.Position = ScaleVertex(triangle.Vertex2.Position, centroid, scale, triangle.InsetConstaint.two.Position);
            triangle.Vertex3.Position = ScaleVertex(triangle.Vertex3.Position, centroid, scale, triangle.InsetConstaint.tree.Position);
        }

        private static Coordinate ScaleVertex(Coordinate vertexPos, Coordinate centroid, float scale, Coordinate constraintPos)
        {
            // Calculate the direction from the centroid to the vertex position
            var direction = new Coordinate(
                vertexPos.X - centroid.X,
                vertexPos.Y - centroid.Y,
                vertexPos.Z - centroid.Z
            );

            // Scale the direction vector
            direction.X *= scale;
            direction.Y *= scale;
            direction.Z *= scale;

            // Compute the new position of the vertex
            var newVertexPos = new Coordinate(
                centroid.X + direction.X,
                centroid.Y + direction.Y,
                centroid.Z + direction.Z
            );

            // Make sure the new vertex position does not exceed the constraint
            newVertexPos.X = Clamp(newVertexPos.X, centroid.X, constraintPos.X);
            newVertexPos.Y = Clamp(newVertexPos.Y, centroid.Y, constraintPos.Y);
            newVertexPos.Z = Clamp(newVertexPos.Z, centroid.Z, constraintPos.Z);

            return newVertexPos;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        internal static bool TrianglesAreQuad(w3Triangle w3Triangle1, w3Triangle w3Triangle2)
        {
            // Collect vertices from the first triangle
            var vertices1 = new HashSet<w3Vertex> { w3Triangle1.Vertex1, w3Triangle1.Vertex2, w3Triangle1.Vertex3 };
            // Collect vertices from the second triangle
            var vertices2 = new HashSet<w3Vertex> { w3Triangle2.Vertex1, w3Triangle2.Vertex2, w3Triangle2.Vertex3 };

            // Find the intersection of the two sets
            vertices1.IntersectWith(vertices2);

            // If the triangles share exactly two vertices, they form a quad
            return vertices1.Count == 2;
        }

        internal static void FitUV(w3Triangle w3Triangle1, w3Triangle w3Triangle2)
        {
            // Step 1: Find the shared vertices between the two triangles
            var sharedVertices = new HashSet<w3Vertex> { w3Triangle1.Vertex1, w3Triangle1.Vertex2, w3Triangle1.Vertex3 };
            sharedVertices.IntersectWith(new HashSet<w3Vertex> { w3Triangle2.Vertex1, w3Triangle2.Vertex2, w3Triangle2.Vertex3 });

            // Step 2: Assign UV coordinates to the four vertices
            // The quad consists of four vertices, two from each triangle, and two of them are shared
            var vertices = new List<w3Vertex> { w3Triangle1.Vertex1, w3Triangle1.Vertex2, w3Triangle1.Vertex3, w3Triangle2.Vertex1, w3Triangle2.Vertex2, w3Triangle2.Vertex3 };

            // Remove shared vertices from the list to identify the unique vertices
            var uniqueVertices = vertices.Where(v => !sharedVertices.Contains(v)).ToList();

            // Now we have 4 vertices for the quad, and we can assign UVs to them
            if (uniqueVertices.Count == 2)
            {
                // Step 3: Assign UV coordinates to the vertices of the quad
                uniqueVertices[0].Texture_Position = new Coordinate2D { U = 1, V = 0 };  // First unique vertex gets (1, 0)
                uniqueVertices[1].Texture_Position = new Coordinate2D { U = 1, V = 1 };  // Second unique vertex gets (1, 1)

                // Step 4: Assign UV coordinates to the shared vertices
                foreach (var sharedVertex in sharedVertices)
                {
                    if (sharedVertex == w3Triangle1.Vertex1 || sharedVertex == w3Triangle2.Vertex1)
                    {
                        sharedVertex.Texture_Position = new Coordinate2D { U = 0, V = 0 };  // Shared vertex 1 gets (0, 0)
                    }
                    else
                    {
                        sharedVertex.Texture_Position = new Coordinate2D { U = 0, V = 1 };  // Shared vertex 2 gets (0, 1)
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("The triangles do not form a valid quad.");
            }
        }

        internal static void PasteFacingAngle(w3Triangle copiedFacingTriangle, w3Triangle pastedOnTriangle)
        {
            // Step 1: Calculate the normal of the copied triangle
            Coordinate edge1 = new Coordinate
            {
                X = copiedFacingTriangle.Vertex2.Position.X - copiedFacingTriangle.Vertex1.Position.X,
                Y = copiedFacingTriangle.Vertex2.Position.Y - copiedFacingTriangle.Vertex1.Position.Y,
                Z = copiedFacingTriangle.Vertex2.Position.Z - copiedFacingTriangle.Vertex1.Position.Z
            };

            Coordinate edge2 = new Coordinate
            {
                X = copiedFacingTriangle.Vertex3.Position.X - copiedFacingTriangle.Vertex1.Position.X,
                Y = copiedFacingTriangle.Vertex3.Position.Y - copiedFacingTriangle.Vertex1.Position.Y,
                Z = copiedFacingTriangle.Vertex3.Position.Z - copiedFacingTriangle.Vertex1.Position.Z
            };

            // Calculate the cross product of edge1 and edge2 to get the normal
            Coordinate normal = new Coordinate
            {
                X = edge1.Y * edge2.Z - edge1.Z * edge2.Y,
                Y = edge1.Z * edge2.X - edge1.X * edge2.Z,
                Z = edge1.X * edge2.Y - edge1.Y * edge2.X
            };

            // Normalize the normal vector
            float length = (float)Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
            if (length > 0)
            {
                normal.X /= length;
                normal.Y /= length;
                normal.Z /= length;
            }

            // Step 2: Apply the normal to each vertex of the pasted triangle
            pastedOnTriangle.Vertex1.Normal = normal;
            pastedOnTriangle.Vertex2.Normal = normal;
            pastedOnTriangle.Vertex3.Normal = normal;
        }

        internal static void AimTriangleAtCoordinate(w3Triangle triangle, Coordinate target)
        {
            // Step 1: Calculate the centroid of the triangle
            Coordinate centroid = new Coordinate
            {
                X = (triangle.Vertex1.Position.X + triangle.Vertex2.Position.X + triangle.Vertex3.Position.X) / 3,
                Y = (triangle.Vertex1.Position.Y + triangle.Vertex2.Position.Y + triangle.Vertex3.Position.Y) / 3,
                Z = (triangle.Vertex1.Position.Z + triangle.Vertex2.Position.Z + triangle.Vertex3.Position.Z) / 3
            };

            // Step 2: Compute the direction vector from the centroid to the target coordinate
            Coordinate direction = new Coordinate
            {
                X = target.X - centroid.X,
                Y = target.Y - centroid.Y,
                Z = target.Z - centroid.Z
            };

            // Step 3: Normalize the direction vector
            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);
            if (length > 0)
            {
                direction.X /= length;
                direction.Y /= length;
                direction.Z /= length;
            }

            // Step 4: Set the triangle's normal to the direction vector
            triangle.Vertex1.Normal = direction;
            triangle.Vertex2.Normal = direction;
            triangle.Vertex3.Normal = direction;

            // Optional: Adjust the triangle's vertices so the face is oriented towards the target
            // If you want the triangle to rotate and face the target, you can implement this step,
            // but it involves more complex geometry transformations like matrix rotation.
        }

        internal static void ScaleRelativeToCoordinate(List<w3Vertex> vertices, Coordinate custom, bool x, bool y, bool z, int percentage)
        {
            float scaleFactor = percentage / 100f;

            foreach (var vertex in vertices)
            {
                // Get current position
                var pos = vertex.Position;

                // Scale each axis conditionally
                if (x)
                    pos.X = custom.X + (pos.X - custom.X) * scaleFactor;
                if (y)
                    pos.Y = custom.Y + (pos.Y - custom.Y) * scaleFactor;
                if (z)
                    pos.Z = custom.Z + (pos.Z - custom.Z) * scaleFactor;
            }
        }

        internal static void ScaleRelativeToCoordinate(List<w3Vertex> vertices, Coordinate custom, bool x, bool y, bool z, float result1, float result2, float result3)
        {
            float scaleFactor1 = result1 / 100f;
            float scaleFactor2 = result2 / 100f;
            float scaleFactor3 = result3 / 100f;

            foreach (var vertex in vertices)
            {
                // Get current position
                var pos = vertex.Position;

                // Scale each axis conditionally
                if (x)
                    pos.X = custom.X + (pos.X - custom.X) * scaleFactor1;
                if (y)
                    pos.Y = custom.Y + (pos.Y - custom.Y) * scaleFactor2;
                if (z)
                    pos.Z = custom.Z + (pos.Z - custom.Z) * scaleFactor3;
            }
        }

        internal static bool TrianglesHaveSameArea(w3Triangle triangle1, w3Triangle triangle2)
        {
            float GetTriangleArea(w3Triangle triangle)
            {
                var u = new Coordinate
                {
                    X = triangle.Vertex2.Position.X - triangle.Vertex1.Position.X,
                    Y = triangle.Vertex2.Position.Y - triangle.Vertex1.Position.Y,
                    Z = triangle.Vertex2.Position.Z - triangle.Vertex1.Position.Z
                };

                var v = new Coordinate
                {
                    X = triangle.Vertex3.Position.X - triangle.Vertex1.Position.X,
                    Y = triangle.Vertex3.Position.Y - triangle.Vertex1.Position.Y,
                    Z = triangle.Vertex3.Position.Z - triangle.Vertex1.Position.Z
                };

                // Cross product
                var crossX = u.Y * v.Z - u.Z * v.Y;
                var crossY = u.Z * v.X - u.X * v.Z;
                var crossZ = u.X * v.Y - u.Y * v.X;

                // Magnitude of the cross product
                var magnitude = Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ);

                return (float)(magnitude / 2);
            }

            return Math.Abs(GetTriangleArea(triangle1) - GetTriangleArea(triangle2)) < 1e-6; // Allow small floating-point error
        }

        internal static bool TrianglesHaveSimilarFacingAngle(w3Triangle triangle1, w3Triangle triangle2)
        {
            bool HaveSimilarFacing(Coordinate p1, Coordinate p2, Coordinate p3, Coordinate q1, Coordinate q2, Coordinate q3)
            {
                // Vectors for triangle 1
                var edge1A = new Coordinate
                {
                    X = p2.X - p1.X,
                    Y = p2.Y - p1.Y,
                    Z = p2.Z - p1.Z
                };
                var edge2A = new Coordinate
                {
                    X = p3.X - p1.X,
                    Y = p3.Y - p1.Y,
                    Z = p3.Z - p1.Z
                };

                // Vectors for triangle 2
                var edge1B = new Coordinate
                {
                    X = q2.X - q1.X,
                    Y = q2.Y - q1.Y,
                    Z = q2.Z - q1.Z
                };
                var edge2B = new Coordinate
                {
                    X = q3.X - q1.X,
                    Y = q3.Y - q1.Y,
                    Z = q3.Z - q1.Z
                };

                // Dot products
                var dot1 = edge1A.X * edge1B.X + edge1A.Y * edge1B.Y + edge1A.Z * edge1B.Z;
                var dot2 = edge2A.X * edge2B.X + edge2A.Y * edge2B.Y + edge2A.Z * edge2B.Z;

                // Same facing condition
                return dot1 >= 0 && dot2 >= 0;
            }

            var t1 = triangle1;
            var t2 = triangle2;

            // Check both orientations
            return HaveSimilarFacing(t1.Vertex1.Position, t1.Vertex2.Position, t1.Vertex3.Position,
                                     t2.Vertex1.Position, t2.Vertex2.Position, t2.Vertex3.Position) ||
                   HaveSimilarFacing(t1.Vertex1.Position, t1.Vertex2.Position, t1.Vertex3.Position,
                                     t2.Vertex1.Position, t2.Vertex3.Position, t2.Vertex2.Position);
        }

        internal static void RescaleSequenceKeyframes(int originalFrom, int originalTo, int newFrom, int newTo, List<w3Keyframe> keyframes)
        {
            // Validate input ranges
            if (keyframes == null || keyframes.Count == 0)
                throw new ArgumentException("Keyframes list is null or empty.");
            if (originalFrom == originalTo)
                throw new ArgumentException("Original range (originalFrom to originalTo) must not be zero-length.");
            if (keyframes.Any(kf => kf == null))
                throw new ArgumentException("Keyframes list contains a null keyframe.");

            // Handle each keyframe
            foreach (var keyframe in keyframes)
            {
                // Clamp track value to original range to avoid extrapolation
                int clampedTrack = Math.Clamp(keyframe.Track, originalFrom, originalTo);

                // Rescale the track value to the new range
                keyframe.Track = newFrom + (clampedTrack - originalFrom) * (newTo - newFrom) / (originalTo - originalFrom);
            }
        }

        internal static bool SequencesAreBackToBack(List<w3Sequence> sequences)
        {
            if (sequences == null || sequences.Count < 2)
                return false; // Not enough sequences to compare

            for (int i = 1; i < sequences.Count; i++)
            {
                if (sequences[i - 1].To + 1 != sequences[i].From)
                    return false; // Sequences are not back-to-back
            }

            return true; // All sequences are back-to-back
        }
        private static void RescaleKeyframesIntoOneSequence(List<w3Transformation> transformations, w3Sequence targetSequence, List<w3Sequence> whichSequences)
        {
            if (transformations == null || targetSequence == null || whichSequences == null || whichSequences.Count == 0)
                throw new ArgumentException("Invalid input: one or more arguments are null or empty.");

            // Check if the target sequence is valid
            if (targetSequence.From == targetSequence.To)
                throw new ArgumentException("Target sequence is invalid (start cannot equal end).");

            foreach (w3Transformation transformation in transformations)
            {
                foreach (w3Keyframe keyframe in transformation.Keyframes)
                {
                    // Rescale the keyframe's Track based on the target sequence range
                    int originalTrack = keyframe.Track;

                    // Calculate the new track value by scaling the original position to the target range
                    keyframe.Track = targetSequence.From + (originalTrack - whichSequences.First().From) * (targetSequence.To - targetSequence.From) / (whichSequences.First().To - whichSequences.First().From);
                }
            }
        }

        public static void MergeSequences(List<w3Sequence> selectedSequences, List<w3Sequence> fullList, string givenName, List<w3Transformation> transformations)
        {
            if (selectedSequences == null || fullList == null || selectedSequences.Count == 0)
                throw new ArgumentException("Selected or full list is null or empty.");

            if (SequencesAreBackToBack(selectedSequences))
            {
                // Create a new merged sequence
                w3Sequence newSequence = new w3Sequence
                {
                    From = selectedSequences.First().From,
                    To = selectedSequences.Last().To,
                    Name = givenName,
                    Looping = false
                };

                // Remove selected sequences from the full list
                fullList.RemoveAll(x => selectedSequences.Contains(x));

                // Add the merged sequence to the full list
                fullList.Add(newSequence);
            }
            else
            {
                int fullIntevnal = selectedSequences.Last().To - selectedSequences.First().From;
                int largestTo = fullList.Max(sequence => sequence.To);
                if (largestTo+1 + fullIntevnal > 999999)
                {
                    MessageBox.Show("The selected sequences are not back to back, and the new interval of the new sequence's To would exceed 999,999. Not merged. ", "Precaution");
                    return;
                }
                else
                {
                    w3Sequence sequence = new w3Sequence(givenName, largestTo+1, largestTo+1+fullIntevnal , 0, 0, false);
                    RescaleKeyframesIntoOneSequence(transformations, sequence, selectedSequences);
                }
            }
        }

        internal static bool CanBeExtended(List<w3Triangle> triangles)
        {
            // Group triangles into islands based on shared vertices
            var islands = new List<List<w3Triangle>>();
            var visited = new HashSet<w3Triangle>();

            void ExploreIsland(w3Triangle triangle, List<w3Triangle> island)
            {
                if (!visited.Add(triangle)) return;
                island.Add(triangle);

                foreach (var neighbor in triangles)
                {
                    if (!visited.Contains(neighbor) && SharesVertex(triangle, neighbor))
                    {
                        ExploreIsland(neighbor, island);
                    }
                }
            }

            foreach (var triangle in triangles)
            {
                if (!visited.Contains(triangle))
                {
                    var island = new List<w3Triangle>();
                    ExploreIsland(triangle, island);
                    islands.Add(island);
                }
            }

            // For each island, ensure no duplicate triangle sharing occurs (no conflicting connections)
            foreach (var island in islands)
            {
                var vertices = new HashSet<w3Vertex>();
                foreach (var triangle in island)
                {
                    // Check if vertices are reused within the same island
                    if (!vertices.Add(triangle.Vertex1) ||
                        !vertices.Add(triangle.Vertex2) ||
                        !vertices.Add(triangle.Vertex3))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool SharesVertex(w3Triangle t1, w3Triangle t2)
        {
            return t1.Vertex1 == t2.Vertex1 || t1.Vertex1 == t2.Vertex2 || t1.Vertex1 == t2.Vertex3 ||
                   t1.Vertex2 == t2.Vertex1 || t1.Vertex2 == t2.Vertex2 || t1.Vertex2 == t2.Vertex3 ||
                   t1.Vertex3 == t2.Vertex1 || t1.Vertex3 == t2.Vertex2 || t1.Vertex3 == t2.Vertex3;
        }

        internal static List<w3Vertex> GetVerticesOfEdges(w3Edge one, w3Edge two)
        {
            w3Vertex joint = null;
            List<w3Vertex> vertices = new List<w3Vertex>();

            // Identify the joint vertex
            if (one.Vertex1 == two.Vertex1) { joint = one.Vertex1; }
            else if (one.Vertex1 == two.Vertex2) { joint = one.Vertex1; }
            else if (one.Vertex2 == two.Vertex1) { joint = one.Vertex2; }
            else if (one.Vertex2 == two.Vertex2) { joint = one.Vertex2; }

            if (joint != null)
            {
                // Add all unique vertices, joint at the end
                vertices.AddRange(new[] { one.Vertex1, one.Vertex2, two.Vertex1, two.Vertex2 }
                                  .Where(v => v != joint).Distinct());
                vertices.Add(joint);
            }

            return vertices;
        }

        internal static bool GeosetContainsTriangleWithTheseVertices(w3Geoset targetGeoset, List<w3Vertex> vertices)
        {
            return targetGeoset.Triangles.Any(triangle =>
       vertices.Contains(triangle.Vertex1) &&
       vertices.Contains(triangle.Vertex2) &&
       vertices.Contains(triangle.Vertex3));
        }

        internal static List<w3Triangle> CreateTwoTriangles(List<w3Vertex> list)
        {
            if (list.Count != 4)
                throw new ArgumentException("The list must contain exactly 4 vertices.");

            // Ensure vertices are not collinear by checking the area of the quadrilateral formed
            if (IsCollinear(list[0], list[1], list[2]) || IsCollinear(list[0], list[2], list[3]) ||
                IsCollinear(list[1], list[2], list[3]) || IsCollinear(list[0], list[1], list[3]))
            {
                throw new ArgumentException("The vertices cannot be collinear.");
            }

            w3Triangle first = new w3Triangle(list[0], list[1], list[2]);
            w3Triangle second = new w3Triangle(list[0], list[2], list[3]);

            // Optionally, ensure the triangles do not overlap by checking the orientation or geometry

            return new List<w3Triangle> { first, second };
        }

        // Helper method to check if three points are collinear (using cross product for 2D)
        private static bool IsCollinear(w3Vertex v1, w3Vertex v2, w3Vertex v3)
        {
            float crossProduct = (v2.Position.X - v1.Position.X) * (v3.Position.Y - v1.Position.Y) - (v2.Position.Y - v1.Position.Y) * (v3.Position.X - v1.Position.X);
            return Math.Abs(crossProduct) < float.Epsilon;  // Consider points collinear if the cross product is close to 0
        }

        internal static void ArrangeVertices(List<w3Vertex> vertices, CustomAngle angle, float DistanceFromCentroid)
        {
            // Get the centroid of the vertices
            Coordinate centroid = GetCentroid(vertices);

            foreach (var vertex in vertices)
            {
                // Step 1: Calculate the vector from the vertex to the centroid
                float deltaX = vertex.Position.X - centroid.X;
                float deltaY = vertex.Position.Y - centroid.Y;
                float deltaZ = vertex.Position.Z - centroid.Z;

                // Step 2: Normalize the vector
                float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
                if (length != 0)
                {
                    deltaX /= length;
                    deltaY /= length;
                    deltaZ /= length;
                }

                // Step 3: Scale the vector to match the desired distance from the centroid
                deltaX *= DistanceFromCentroid;
                deltaY *= DistanceFromCentroid;
                deltaZ *= DistanceFromCentroid;

                // Step 4: Apply rotation to the vector using the angles provided (around X, Y, Z axes)
                var rotatedVector = RotateVector(new Coordinate(deltaX, deltaY, deltaZ), angle);

                // Step 5: Update the vertex position
                vertex.Position.X = centroid.X + rotatedVector.X;
                vertex.Position.Y = centroid.Y + rotatedVector.Y;
                vertex.Position.Z = centroid.Z + rotatedVector.Z;
            }
        }

        // Rotate the vector based on the angle in degrees
        private static Coordinate RotateVector(Coordinate vector, CustomAngle angle)
        {
            // Convert angles to radians
            float radX = angle.Angle_X * (float)Math.PI / 180f;
            float radY = angle.Angle_Y * (float)Math.PI / 180f;
            float radZ = angle.Angle_Z * (float)Math.PI / 180f;

            // Rotation matrices for each axis (X, Y, Z)
            float cosX = (float)Math.Cos(radX);
            float sinX = (float)Math.Sin(radX);
            float cosY = (float)Math.Cos(radY);
            float sinY = (float)Math.Sin(radY);
            float cosZ = (float)Math.Cos(radZ);
            float sinZ = (float)Math.Sin(radZ);

            // Rotate around X axis
            float y1 = cosX * vector.Y - sinX * vector.Z;
            float z1 = sinX * vector.Y + cosX * vector.Z;

            // Rotate around Y axis
            float x2 = cosY * vector.X + sinY * z1;
            float z2 = -sinY * vector.X + cosY * z1;

            // Rotate around Z axis
            float x3 = cosZ * x2 - sinZ * y1;
            float y3 = sinZ * x2 + cosZ * y1;

            return new Coordinate(x3, y3, z2);
        }

        internal static float GetTriangleArea(w3Triangle triangle)
        {
            // Get the vertices
            var v1 = triangle.Vertex1.Position;
            var v2 = triangle.Vertex2.Position;
            var v3 = triangle.Vertex3.Position;

            // Calculate the vectors for two sides of the triangle
            var ab = new Vector3(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
            var ac = new Vector3(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);

            // Compute the cross product of AB and AC
            var crossProduct = Vector3.Cross(ab, ac);

            // The area is half the magnitude of the cross product
            return 0.5f * crossProduct.Length();
        }

        internal static w3Geoset CreateVerticalPlane(Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            // Define vertices based on the bitmap's dimensions
            w3Vertex one = new w3Vertex(0, 0, 0); // Bottom-left corner
            w3Vertex two = new w3Vertex(0, height, 0); // Top-left corner
            w3Vertex three = new w3Vertex(width, 0, 0); // Bottom-right corner
            w3Vertex four = new w3Vertex(width, height, 0); // Top-right corner

            // Create the geoset and add vertices
            w3Geoset geo = new w3Geoset();
            geo.Vertices.AddRange(new[] { one, two, three, four });

            // Define triangles for the plane
            w3Triangle triangle1 = new w3Triangle
            {
                Vertex1 = one, // Bottom-left vertex
                Vertex2 = two, // Top-left vertex
                Vertex3 = three // Bottom-right vertex
            };

            w3Triangle triangle2 = new w3Triangle
            {
                Vertex1 = three, // Bottom-right vertex
                Vertex2 = two, // Top-left vertex
                Vertex3 = four // Top-right vertex
            };

            // Add triangles to the geoset
            geo.Triangles.Add(triangle1);
            geo.Triangles.Add(triangle2);

            return geo;
        }

        internal static bool TrianglesAreConnected(w3Triangle t1, w3Triangle t2)
        {
            // Compare vertices of t1 with vertices of t2
            return VerticesAreConnected(t1.Vertex1, t2) ||
                   VerticesAreConnected(t1.Vertex2, t2) ||
                   VerticesAreConnected(t1.Vertex3, t2);
        }

        private static bool VerticesAreConnected(w3Vertex vertex, w3Triangle triangle)
        {
            // Check if the vertex matches any vertex in the triangle
            return vertex == triangle.Vertex1 ||
                   vertex == triangle.Vertex2 ||
                   vertex == triangle.Vertex3;
        }

        internal static bool EdgesAreConnected(w3Edge edge1, w3Edge edge2)
        {
            return edge1.Vertex1 == edge2.Vertex1 || edge1.Vertex1 == edge2.Vertex2 ||
                   edge1.Vertex2 == edge2.Vertex1 || edge1.Vertex2 == edge2.Vertex2;
        }

        internal static void ExpandEdges(List<w3Edge> selectedEdges, bool expand, float modifyAmount)
        {
            // Iterate through each edge
            foreach (var edge in selectedEdges)
            {
                // Calculate the midpoint of the edge
                var midpoint = new w3Vertex
                {
                    Position = new Coordinate
                    {
                        X = (edge.Vertex1.Position.X + edge.Vertex2.Position.X) / 2,
                        Y = (edge.Vertex1.Position.Y + edge.Vertex2.Position.Y) / 2,
                        Z = (edge.Vertex1.Position.Z + edge.Vertex2.Position.Z) / 2
                    }
                };

                // Calculate direction vectors for each vertex relative to the midpoint
                var direction1 = new Coordinate
                {
                    X = edge.Vertex1.Position.X - midpoint.Position.X,
                    Y = edge.Vertex1.Position.Y - midpoint.Position.Y,
                    Z = edge.Vertex1.Position.Z - midpoint.Position.Z
                };

                var direction2 = new Coordinate
                {
                    X = edge.Vertex2.Position.X - midpoint.Position.X,
                    Y = edge.Vertex2.Position.Y - midpoint.Position.Y,
                    Z = edge.Vertex2.Position.Z - midpoint.Position.Z
                };

                // Normalize direction vectors (to ensure consistent scaling)
                float length1 = (float)Math.Sqrt(direction1.X * direction1.X + direction1.Y * direction1.Y + direction1.Z * direction1.Z);
                float length2 = (float)Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z);

                if (length1 > 0)
                {
                    direction1.X /= length1;
                    direction1.Y /= length1;
                    direction1.Z /= length1;
                }

                if (length2 > 0)
                {
                    direction2.X /= length2;
                    direction2.Y /= length2;
                    direction2.Z /= length2;
                }

                // Adjust the positions of the vertices
                float modifier = expand ? modifyAmount : -modifyAmount;

                // Ensure we don't shrink below 0 distance
                if (!expand)
                {
                    float distance1 = length1 + modifier;
                    float distance2 = length2 + modifier;

                    if (distance1 < 0) modifier = -length1;
                    if (distance2 < 0) modifier = -length2;
                }

                edge.Vertex1.Position.X += direction1.X * modifier;
                edge.Vertex1.Position.Y += direction1.Y * modifier;
                edge.Vertex1.Position.Z += direction1.Z * modifier;

                edge.Vertex2.Position.X += direction2.X * modifier;
                edge.Vertex2.Position.Y += direction2.Y * modifier;
                edge.Vertex2.Position.Z += direction2.Z * modifier;
            }
        }


        internal static void ExpandTriangles(List<w3Triangle> selectedTriangles, bool expand, float modifyAmount)
        {
            // Iterate through each triangle
            foreach (var triangle in selectedTriangles)
            {
                // Calculate the centroid (geometric center) of the triangle
                var centroid = new Coordinate
                {
                    X = (triangle.Vertex1.Position.X + triangle.Vertex2.Position.X + triangle.Vertex3.Position.X) / 3,
                    Y = (triangle.Vertex1.Position.Y + triangle.Vertex2.Position.Y + triangle.Vertex3.Position.Y) / 3,
                    Z = (triangle.Vertex1.Position.Z + triangle.Vertex2.Position.Z + triangle.Vertex3.Position.Z) / 3
                };

                // Adjust each vertex of the triangle
                AdjustVertexPosition(triangle.Vertex1, centroid, expand, modifyAmount);
                AdjustVertexPosition(triangle.Vertex2, centroid, expand, modifyAmount);
                AdjustVertexPosition(triangle.Vertex3, centroid, expand, modifyAmount);
            }
        }

        private static void AdjustVertexPosition(w3Vertex vertex, Coordinate centroid, bool expand, float modifyAmount)
        {
            // Calculate the direction vector from the centroid to the vertex
            var direction = new Coordinate
            {
                X = vertex.Position.X - centroid.X,
                Y = vertex.Position.Y - centroid.Y,
                Z = vertex.Position.Z - centroid.Z
            };

            // Calculate the length of the direction vector
            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

            // Normalize the direction vector
            if (length > 0)
            {
                direction.X /= length;
                direction.Y /= length;
                direction.Z /= length;
            }

            // Determine the movement amount
            float modifier = expand ? modifyAmount : -modifyAmount;

            // Ensure we don't shrink below 0 distance
            if (!expand && length + modifier < 0)
            {
                modifier = -length;
            }

            // Adjust the vertex position
            vertex.Position.X += direction.X * modifier;
            vertex.Position.Y += direction.Y * modifier;
            vertex.Position.Z += direction.Z * modifier;
        }

        internal static Coordinate RotateVertexAroundBone(Coordinate vertexPosition, float[] instruction, Coordinate pivotPoint)
        {
            Coordinate coord = new Coordinate();
            if (instruction.Length != 3)
                throw new ArgumentException("Instruction must contain exactly three elements (x, y, z rotation angles)."+ " got " + instruction.Length.ToString() + ": " + string.Join(", ", instruction));

            // Translate vertex to pivot point
            float translatedX = vertexPosition.X - pivotPoint.X;
            float translatedY = vertexPosition.Y - pivotPoint.Y;
            float translatedZ = vertexPosition.Z - pivotPoint.Z;

            // Extract rotation angles
            float angleX = instruction[0]; // Rotation around X-axis
            float angleY = instruction[1]; // Rotation around Y-axis
            float angleZ = instruction[2]; // Rotation around Z-axis

            // Apply rotation around X-axis
            float cosX = (float)Math.Cos(angleX);
            float sinX = (float)Math.Sin(angleX);
            float y1 = cosX * translatedY - sinX * translatedZ;
            float z1 = sinX * translatedY + cosX * translatedZ;

            // Apply rotation around Y-axis
            float cosY = (float)Math.Cos(angleY);
            float sinY = (float)Math.Sin(angleY);
            float x2 = cosY * translatedX + sinY * z1;
            float z2 = -sinY * translatedX + cosY * z1;

            // Apply rotation around Z-axis
            float cosZ = (float)Math.Cos(angleZ);
            float sinZ = (float)Math.Sin(angleZ);
            float x3 = cosZ * x2 - sinZ * y1;
            float y3 = sinZ * x2 + cosZ * y1;

            // Translate vertex back to its original position
            coord.X = x3 + pivotPoint.X;
            coord.Y = y3 + pivotPoint.Y;
            coord.Z = z2 + pivotPoint.Z;
            return coord;
        }

        internal static void SimplifyTriangles(w3Geoset geo, List<w3Triangle> triangles)
        {
            if (triangles == null || triangles.Count < 2)
                return;

            // Step 1: Extract all unique vertices from the given triangles
            List<w3Vertex> allVertices = new List<w3Vertex>();
            foreach (var triangle in triangles)
            {
                if (!allVertices.Contains(triangle.Vertex1)) allVertices.Add(triangle.Vertex1);
                if (!allVertices.Contains(triangle.Vertex2)) allVertices.Add(triangle.Vertex2);
                if (!allVertices.Contains(triangle.Vertex3)) allVertices.Add(triangle.Vertex3);
            }

            // Step 2: Compute the convex hull of these vertices
            List<w3Vertex> outerVertices = ComputeConvexHull(allVertices);

            // Step 3: Remove the original triangles from the geoset
            foreach (var triangle in triangles)
            {
                geo.Triangles.Remove(triangle);
            }

            // Step 4: Triangulate the convex hull to form new triangles
            List<w3Triangle> newTriangles = TriangulateConvexHull(outerVertices);

            // Step 5: Add the new triangles to the geoset
            geo.Triangles.AddRange(newTriangles);
        }

        // Helper function to compute the convex hull of a set of vertices
        private static List<w3Vertex> ComputeConvexHull(List<w3Vertex> vertices)
        {
            // TODO: Implement a convex hull algorithm, e.g., QuickHull, Gift Wrapping, or Graham's Scan.
            throw new NotImplementedException();
        }

        // Helper function to triangulate the convex hull vertices
        private static List<w3Triangle> TriangulateConvexHull(List<w3Vertex> outerVertices)
        {
            var triangles = new List<w3Triangle>();

            // Assuming the outerVertices form a convex shape, triangulate them
            // Simplified for planar cases; for 3D cases, use advanced triangulation
            for (int i = 1; i < outerVertices.Count - 1; i++)
            {
                triangles.Add(new w3Triangle
                {
                    Vertex1 = outerVertices[0],
                    Vertex2 = outerVertices[i],
                    Vertex3 = outerVertices[i + 1]
                });
            }

            return triangles;
        }

        internal static void MirrorGeometryInGeoset(w3Geoset GeosetOwner, List<w3Triangle> SelectedTriangles)
        {
            if (GeosetOwner == null) { return; }
            if (SelectedTriangles == null) { return; }
            if (SelectedTriangles.Count == 0) { return; }
            if (SelectedTriangles.Count == GeosetOwner.Triangles.Count) { return; }

            // Create temporary geoset that will store the mirrored geometry
            w3Geoset TemporaryGeoset = new w3Geoset();

            // Create a copy of the unselected geometry
            foreach (w3Triangle triangle in GeosetOwner.Triangles)
            {
                if (!SelectedTriangles.Contains(triangle))
                {
                    w3Vertex one = triangle.Vertex1.Clone();
                    w3Vertex two = triangle.Vertex2.Clone();
                    w3Vertex three = triangle.Vertex3.Clone();

                    w3Triangle clonedTriangle = new w3Triangle
                    {
                        Vertex1 = one,
                        Vertex2 = two,
                        Vertex3 = three
                    };

                    TemporaryGeoset.Triangles.Add(clonedTriangle);
                    TemporaryGeoset.Vertices.Add(one);
                    TemporaryGeoset.Vertices.Add(two);
                    TemporaryGeoset.Vertices.Add(three);
                }
            }

            // Determine the mirror plane (e.g., XZ-plane, so Y is negated)
            // Adjust as needed for other planes (e.g., YZ or XY).
            Func<w3Vertex, w3Vertex> mirrorFunction = vertex =>
            {
                w3Vertex v = new w3Vertex();
                v.Position.Y = -vertex.Position.Y;
                v.Position.Z = vertex.Position.Z;
                return v;
            };

            // Mirror the positions of all vertices in the temporary geoset
            for (int i = 0; i < TemporaryGeoset.Vertices.Count; i++)
            {
                TemporaryGeoset.Vertices[i] = mirrorFunction(TemporaryGeoset.Vertices[i]);
            }

            // Merge vertices that have the same position (within a tolerance)
            MergeDuplicateVertices(TemporaryGeoset);

            // Add the mirrored geometry to the original geoset
            GeosetOwner.Vertices.AddRange(TemporaryGeoset.Vertices);
            GeosetOwner.Triangles.AddRange(TemporaryGeoset.Triangles);
        }

        private static void MergeDuplicateVertices(w3Geoset geoset)
        {
            const float Tolerance = 0.0001f; // Adjust as needed
            Dictionary<w3Vertex, w3Vertex> uniqueVertices = new Dictionary<w3Vertex, w3Vertex>(new VertexEqualityComparer());

            foreach (var triangle in geoset.Triangles)
            {
                triangle.Vertex1 = GetOrAddUniqueVertex(triangle.Vertex1, uniqueVertices, Tolerance);
                triangle.Vertex2 = GetOrAddUniqueVertex(triangle.Vertex2, uniqueVertices, Tolerance);
                triangle.Vertex3 = GetOrAddUniqueVertex(triangle.Vertex3, uniqueVertices, Tolerance);
            }

            geoset.Vertices = uniqueVertices.Values.ToList();
        }

        private static w3Vertex GetOrAddUniqueVertex(w3Vertex vertex, Dictionary<w3Vertex, w3Vertex> uniqueVertices, float tolerance)
        {
            foreach (var kvp in uniqueVertices)
            {
                if (Math.Abs(kvp.Key.Position.X - vertex.Position.X) < tolerance &&
                    Math.Abs(kvp.Key.Position.Y - vertex.Position.Y) < tolerance &&
                    Math.Abs(kvp.Key.Position.Z - vertex.Position.Z) < tolerance)
                {
                    return kvp.Value;
                }
            }

            uniqueVertices[vertex] = vertex;
            return vertex;
        }

        private class VertexEqualityComparer : IEqualityComparer<w3Vertex>
        {
            public bool Equals(w3Vertex v1, w3Vertex v2)
            {
                return Math.Abs(v1.Position.X - v2.Position.X) < 0.0001f &&
                       Math.Abs(v1.Position.Y - v2.Position.Y) < 0.0001f &&
                       Math.Abs(v1.Position.Z - v2.Position.Z) < 0.0001f;
            }

            public int GetHashCode(w3Vertex vertex)
            {
                return HashCode.Combine(vertex.Position .X, vertex.Position.Y, vertex.Position.Z);
            }
        }

    }
    public static class TriangleScaler
    {
        // Function to calculate the area of a triangle given its vertices
        private static float CalculateTriangleArea(w3Vertex v1, w3Vertex v2, w3Vertex v3)
        {
            float ax = v2.Position.X - v1.Position.X;
            float ay = v2.Position.Y - v1.Position.Y;
            float az = v2.Position.Z - v1.Position.Z;
            float bx = v3.Position.X - v1.Position.X;
            float by = v3.Position.Y - v1.Position.Y;
            float bz = v3.Position.Z - v1.Position.Z;
            float cx = ay * bz - az * by;
            float cy = az * bx - ax * bz;
            float cz = ax * by - ay * bx;
            return (float)Math.Sqrt(cx * cx + cy * cy + cz * cz) / 2;
        }
        // Function to scale the second triangle to be equal in size to the first triangle
        public static void ScaleSecondTriangle(w3Vertex v1a, w3Vertex v2a, w3Vertex v3a, w3Vertex v1b, w3Vertex v2b, w3Vertex v3b)
        {
            float areaA = CalculateTriangleArea(v1a, v2a, v3a);
            float areaB = CalculateTriangleArea(v1b, v2b, v3b);
            if (areaB == 0 || areaA == 0)
            {
                throw new ArgumentException("One of the triangles has zero area.");
            }
            float scaleFactor = (float)Math.Sqrt(areaA / areaB);
            ScaleVertex(v1b, scaleFactor);
            ScaleVertex(v2b, scaleFactor);
            ScaleVertex(v3b, scaleFactor);
        }
        // Function to scale a vertex by a given factor
        private static void ScaleVertex(w3Vertex vertex, float scaleFactor)
        {
            vertex.Position.X *= scaleFactor;
            vertex.Position.Y *= scaleFactor;
            vertex.Position.Z *= scaleFactor;
        }
        public static void ScaleFirstTriangle(w3Vertex v1a, w3Vertex v2a, w3Vertex v3a, w3Vertex v1b, w3Vertex v2b, w3Vertex v3b)
        {
            float areaA = CalculateTriangleArea(v1a, v2a, v3a);
            float areaB = CalculateTriangleArea(v1b, v2b, v3b);
            if (areaB == 0 || areaA == 0)
            {
                throw new ArgumentException("One of the triangles has zero area.");
            }
            float scaleFactor = (float)Math.Sqrt(areaB / areaA);
            ScaleVertex(v1a, scaleFactor);
            ScaleVertex(v2a, scaleFactor);
            ScaleVertex(v3a, scaleFactor);
        }
    }
    public static class OverlapCalculator
    {
        internal static bool AreTrianglesOverlapping(w3Triangle triangle1, w3Triangle triangle2)
        {
            // Check if any vertex of triangle1 is inside triangle2
            if (IsPointInTriangle(triangle1.Vertex1.Position, triangle2) ||
                IsPointInTriangle(triangle1.Vertex2.Position, triangle2) ||
                IsPointInTriangle(triangle1.Vertex3.Position, triangle2))
            {
                return true; // Overlap if any vertex of triangle1 is inside triangle2
            }

            // Check if any vertex of triangle2 is inside triangle1
            if (IsPointInTriangle(triangle2.Vertex1.Position, triangle1) ||
                IsPointInTriangle(triangle2.Vertex2.Position, triangle1) ||
                IsPointInTriangle(triangle2.Vertex3.Position, triangle1))
            {
                return true; // Overlap if any vertex of triangle2 is inside triangle1
            }

            return false; // No overlap if no vertices are inside each other
        }

        internal static bool IsPointInTriangle(Coordinate p, w3Triangle triangle)
        {
            var a = triangle.Vertex1.Position;
            var b = triangle.Vertex2.Position;
            var c = triangle.Vertex3.Position;

            // Calculate vectors
            var v0 = Subtract(c, a);
            var v1 = Subtract(b, a);
            var v2 = Subtract(p, a);

            // Calculate dot products
            float dot00 = DotProduct(v0, v0);
            float dot01 = DotProduct(v0, v1);
            float dot02 = DotProduct(v0, v2);
            float dot11 = DotProduct(v1, v1);
            float dot12 = DotProduct(v1, v2);

            // Calculate the barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if the point is inside the triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        internal static Vector3 Subtract(Coordinate a, Coordinate b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        internal static float DotProduct(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

    }
}
