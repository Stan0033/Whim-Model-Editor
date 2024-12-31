using MDLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    public static class WhimGeosetImporter
    {
        public static List<w3Geoset> Work(string contents)
        {
            List<w3Geoset> geosets = new List<w3Geoset>();
            int currentIndex = 0;

            // Helper to extract the next 4-character string (representing 4 bytes) safely
            string GetNextFourChars()
            {
                if (currentIndex + 4 > contents.Length)
                {
                    throw new Exception("Attempted to read beyond the end of the contents string.");
                }

                string result = contents.Substring(currentIndex, 4);
                currentIndex += 4;
                return result;
            }

            while (currentIndex < contents.Length)
            {
                // Create a new w3Geoset object
                w3Geoset geoset = new w3Geoset();

                // Read vertex and triangle counts for the geoset, with boundary checks
                try
                {
                    int vertexCount = (int)BinaryConverter.BinaryToFloat(GetNextFourChars());
                    int triangleCount = (int)BinaryConverter.BinaryToFloat(GetNextFourChars());

                    // Read vertices
                    for (int i = 0; i < vertexCount; i++)
                    {
                        float x = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float y = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float z = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float xn = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float yn = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float zn = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float u = BinaryConverter.BinaryToFloat(GetNextFourChars());
                        float v = BinaryConverter.BinaryToFloat(GetNextFourChars());

                        // Create a new vertex and add it to the geoset
                        w3Vertex vertex = new w3Vertex
                        {
                            Position = new Coordinate(x, y, z),
                            Normal = new Coordinate(xn, yn, zn),
                            Texture_Position = new Coordinate2D(u, v)
                        };
                        geoset.Vertices.Add(vertex);
                    }

                    // Read triangles
                    for (int i = 0; i < triangleCount; i++)
                    {
                        int vertex1Index = (int)BinaryConverter.BinaryToFloat(GetNextFourChars());
                        int vertex2Index = (int)BinaryConverter.BinaryToFloat(GetNextFourChars());
                        int vertex3Index = (int)BinaryConverter.BinaryToFloat(GetNextFourChars());

                        // Create a new triangle and add it to the geoset
                        w3Triangle triangle = new w3Triangle
                        {
                            Vertex1 = geoset.Vertices[vertex1Index],
                            Vertex2 = geoset.Vertices[vertex2Index],
                            Vertex3 = geoset.Vertices[vertex3Index]
                        };
                        geoset.Triangles.Add(triangle);
                    }

                    // Add the assembled geoset to the list
                    geosets.Add(geoset);
                }
                catch (Exception ex)
                {
                    // Handle any errors gracefully
                    Console.WriteLine($"Error while processing geoset: {ex.Message}");
                    break;  // Exit the loop if there's an issue with the data
                }
            }

            return geosets;
        }

    }
}
