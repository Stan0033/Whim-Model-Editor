﻿using MDLLib;
using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    class eObject
    {
        public List<string> Vertices = new();
        public List<string> Normals = new();
        public List<string> TVertices = new();
        public List<string> Faces = new();
    }
    static class Parser_OBJ
    {
      
        internal static void Save(w3Model WhichModel, string TargetPath)
        {
            // the decimal sign must always be .
            //----------------------------
            CultureInfo ci = new("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            StringBuilder sb = new StringBuilder();
            //----------------------------
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            List<string> vertices = new() { "" };
            List<string> normals = new() { "" };
            List<string> tvertices = new() { "" };
            List<string> faces = new() { "" };
            List<w3Vertex> FullVertices = new() { new w3Vertex() };
            sb.AppendLine($"# Model '{WhichModel.Name}' generated by {AppInfo.AppTitle} v{AppInfo.Version} on {DateTime.Now}");
            foreach (w3Geoset g in WhichModel.Geosets)
            {
                foreach (w3Vertex v in g.Vertices)
                {
                    vertices.Add($"v {v.Position.X:f6} {v.Position.Y:f6} {v.Position.Z:f6}");
                    normals.Add($"vn {v.Normal.X:f6} {v.Normal.Y:f6} {v.Normal.Z:f6}");
                    tvertices.Add($"vt {v.Texture_Position.U:f6} {v.Texture_Position.V:f6}");
                }
            }
            foreach (string s in vertices) sb.AppendLine(s);
            foreach (string s in normals) sb.AppendLine(s);
            foreach (string s in tvertices) sb.AppendLine(s);
            foreach (string s in faces) sb.AppendLine(s);
            foreach (w3Geoset g in WhichModel.Geosets)
            { FullVertices.AddRange(g.Vertices); }
            foreach (w3Geoset g in WhichModel.Geosets)
            {
                foreach (w3Triangle t in g.Triangles)
                {
                    int vertex1 = FullVertices.FindIndex(x => x.Id == t.Index1);
                    int vertex2 = FullVertices.FindIndex(x => x.Id == t.Index2);
                    int vertex3 = FullVertices.FindIndex(x => x.Id == t.Index3);
                    sb.AppendLine($"f {vertex1}/{vertex1}/{vertex1} {vertex2}/{vertex2}/{vertex2} {vertex3}/{vertex3}/{vertex3}");
                }
            }
            File.WriteAllText(TargetPath, sb.ToString());
        }

        public static eObject ParseRaw(string filePath)
        {
            eObject currentObject = new eObject();
            foreach (var line in File.ReadLines(filePath))
            {
                if (line.StartsWith("v ")) // Vertex
                {
                    currentObject?.Vertices.Add(line.Substring(2));
                }
                else if (line.StartsWith("vn ")) // Normal
                {
                    currentObject?.Normals.Add(line.Substring(3));
                }
                else if (line.StartsWith("f ")) // Face
                {
                    currentObject?.Faces.Add(line.Substring(2));
                }
                else if (line.StartsWith("vt ")) // texture
                {
                    currentObject?.TVertices.Add(line.Substring(3));
                }
            }
            return currentObject;
        }
        internal static w3Geoset Parse(string file)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            eObject obj = ParseRaw(file);
            //-------------------------------------------------
            w3Geoset geo = new w3Geoset();
            foreach (string data in obj.Vertices)
            {
                Coordinate v = new Coordinate();
                string[] parts = data.Split(' ').Select(x => x.Trim()).ToArray();
                v.X = Converters.SafeFloat(parts[0]);
                v.Y = Converters.SafeFloat(parts[1]);
                v.Z = Converters.SafeFloat(parts[2]);
                geo.Vertices.Add(new w3Vertex() { Position = v });
            }
            for (int i = 0; i < obj.Normals.Count; i++)
            {
                Coordinate v = new Coordinate();
                string[] parts = obj.Normals[i].Split(' ').Select(x => x.Trim()).ToArray();
                v.X = Converters.SafeFloat(parts[0]);
                v.Y = Converters.SafeFloat(parts[1]);
                v.Z = Converters.SafeFloat(parts[2]);
                if (AppHelper.ListContainsIndex(geo.Vertices, i))
                {
                    geo.Vertices[i].Normal = v;
                }
            }
            for (int i = 0; i < obj.TVertices.Count; i++)
            {
                Coordinate2D v = new Coordinate2D();
                string[] parts = obj.TVertices[i].Split(' ').Select(x => x.Trim()).ToArray();
                v.U = Converters.SafeFloat(parts[0]);
                v.V = Converters.SafeFloat(parts[1]);
                if (AppHelper.ListContainsIndex(geo.Vertices, i))
                {
                    geo.Vertices[i].Texture_Position = v;
                }
            }
            obj.Faces = OBJFaceConverter.ConvertFacesToTriangles(obj.Faces);
            foreach (string data in obj.Faces)
            {
                string[] parts = data.Split(' ').Select(x => x.Trim()).ToArray();
                if (parts.Length == 3)
                {
                    w3Triangle triangle = new w3Triangle();
                    triangle.Index1 = Converters.SafeInt(parts[0].Split("/")[0],-1) - 1;
                    triangle.Index2 = Converters.SafeInt(parts[1].Split("/")[0], -1) - 1;
                    triangle.Index3 = Converters.SafeInt(parts[2].Split("/")[0], -1) - 1;
                    geo.Triangles.Add(triangle);
                }
                if (parts.Length == 4)
                {
                    w3Triangle triangle = new w3Triangle();
                    triangle.Index1 = Converters.SafeInt(parts[0].Split("/")[0], -1) - 1;
                    triangle.Index2 = Converters.SafeInt(parts[1].Split("/")[0],-1) - 1;
                    triangle.Index3 = Converters.SafeInt(parts[2].Split("/")[0], -1) - 1;
                    geo.Triangles.Add(triangle);
                    w3Triangle triangle2 = new w3Triangle();
                    triangle.Index1 = Converters.SafeInt(parts[0].Split("/")[0], -1) - 1;
                    triangle.Index2 = Converters.SafeInt(parts[2].Split("/")[0],-1) - 1;
                    triangle.Index3 = Converters.SafeInt(parts[3].Split("/")[0], -1) - 1;
                    geo.Triangles.Add(triangle2);
                }
                if (parts.Length > 4)
                {
                    List<int> indexes = new List<int>();
                    foreach (string ngon in parts) { indexes.Add(int.Parse(ngon.Split('/')[0]) - 1); }
                    while (indexes.Count > 2)
                    {
                        w3Triangle triangle = new w3Triangle();
                        triangle.Index1 = indexes[0];
                        triangle.Index2 = indexes[1];
                        triangle.Index3 = indexes[2];
                        indexes.RemoveAt(1);
                    }
                }
            }
            foreach (w3Vertex v in geo.Vertices)
            {
                v.Id = IDCounter.Next();
            }
            foreach (w3Triangle tr in geo.Triangles)
            {
                tr.Vertex1 = geo.Vertices[tr.Index1];
                tr.Vertex2 = geo.Vertices[tr.Index2];
                tr.Vertex3 = geo.Vertices[tr.Index3];
            }
            geo.TransferMatrixGroups();
            return geo;
        }
        internal static List<int> ExtractVertexPositions(string line)
        {
            //f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3 ...
            List<int> vertexIndices = new List<int>();
            // Split the line by space to get individual tokens
            List<string> tokens = line.Split(' ').ToList();
            tokens.RemoveAll(x => x.Length == 0);
            foreach (string token in tokens)
            {
                vertexIndices.Add(int.Parse(token.Split('/')[0]));
            }
            return vertexIndices;
        }

    }
    public static class OBJFaceConverter
    {
        public static List<string> ConvertFacesToTriangles(List<string> faces)
        {
            List<string> triangleFaces = new List<string>();
            foreach (string face in faces)
            {
                // Split the face string to get individual vertex information
                string[] vertices = face.Split(' ');
                // If the face is not in the correct format, skip it
                if (vertices.Length < 3)
                {
                    continue;
                }
                // Process the face as an n-gon (n-sided polygon)
                for (int i = 1; i < vertices.Length - 1; i++)
                {
                    // Create a triangle from the first vertex, the current vertex, and the next vertex
                    string triangle = $"{vertices[0]} {vertices[i]} {vertices[i + 1]}";
                    triangleFaces.Add(triangle);
                }
            }
            return triangleFaces;
        }
    }
}