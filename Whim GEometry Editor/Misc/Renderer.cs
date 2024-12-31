using MDLLib;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whim_Model_Editor;
using SharpGL.SceneGraph.Assets;
using MDLLibs.Classes.Misc;
using Whim_GEometry_Editor.Dialogs;
using System.Windows;
using MdxLib.Animator;

namespace Whim_GEometry_Editor.Misc
{
    public static class Renderer
    {
        public static w3Geoset CreateCubeFromExtent(Extent extent)
        {
            w3Geoset geo = new w3Geoset();

            // Get the min and max coordinates from the extent
            float minX = extent.Minimum_X;
            float minY = extent.Minimum_Y;
            float minZ = extent.Minimum_Z;
            float maxX = extent.Maximum_X;
            float maxY = extent.Maximum_Y;
            float maxZ = extent.Maximum_Z;

            // Define the vertices of the cube
            w3Vertex v0 = new w3Vertex(minX, minY, minZ); // Bottom-left-back
            w3Vertex v1 = new w3Vertex(maxX, minY, minZ); // Bottom-right-back
            w3Vertex v2 = new w3Vertex(maxX, maxY, minZ); // Top-right-back
            w3Vertex v3 = new w3Vertex(minX, maxY, minZ); // Top-left-back
            w3Vertex v4 = new w3Vertex(minX, minY, maxZ); // Bottom-left-front
            w3Vertex v5 = new w3Vertex(maxX, minY, maxZ); // Bottom-right-front
            w3Vertex v6 = new w3Vertex(maxX, maxY, maxZ); // Top-right-front
            w3Vertex v7 = new w3Vertex(minX, maxY, maxZ); // Top-left-front

            // Add vertices to the geoset
            geo.Vertices.Add(v0);
            geo.Vertices.Add(v1);
            geo.Vertices.Add(v2);
            geo.Vertices.Add(v3);
            geo.Vertices.Add(v4);
            geo.Vertices.Add(v5);
            geo.Vertices.Add(v6);
            geo.Vertices.Add(v7);

            // Define the triangles of the cube using variable names
            geo.Triangles.Add(new w3Triangle(v0, v1, v2)); // Back face
            geo.Triangles.Add(new w3Triangle(v0, v2, v3));
            geo.Triangles.Add(new w3Triangle(v4, v5, v6)); // Front face
            geo.Triangles.Add(new w3Triangle(v4, v6, v7));
            geo.Triangles.Add(new w3Triangle(v0, v1, v5)); // Bottom face
            geo.Triangles.Add(new w3Triangle(v0, v5, v4));
            geo.Triangles.Add(new w3Triangle(v2, v3, v7)); // Top face
            geo.Triangles.Add(new w3Triangle(v2, v7, v6));
            geo.Triangles.Add(new w3Triangle(v1, v2, v6)); // Right face
            geo.Triangles.Add(new w3Triangle(v1, v6, v5));
            geo.Triangles.Add(new w3Triangle(v0, v3, v7)); // Left face
            geo.Triangles.Add(new w3Triangle(v0, v7, v4));

            // Finalize the geometry
            geo.RecalculateEdges();
            geo.ID = IDCounter.Next();
            geo.Material_ID = 0;

            return geo;
        }


        public static void RenderGrid(OpenGL gl, float gridSize, float lineSpacing)
        {
            if (gridSize == 0 || lineSpacing == 0) { return; }
            // Calculate half the size for easier symmetric drawing
            float halfGridSize = gridSize / 2f;

            // Set line color (grayish)
            gl.Color(AppSettings.GridColor[0], AppSettings.GridColor[1], AppSettings.GridColor[2]);  // RGB gray color

            gl.Begin(OpenGL.GL_LINES);

            // Draw lines along the X axis (horizontal)
            for (float y = -halfGridSize; y <= halfGridSize; y += lineSpacing)
            {
                // Horizontal lines parallel to the X axis (constant Y, varying X)
                gl.Vertex(-halfGridSize, y, 0);
                gl.Vertex(halfGridSize, y, 0);
            }

            // Draw lines along the Y axis (vertical)
            for (float x = -halfGridSize; x <= halfGridSize; x += lineSpacing)
            {
                // Vertical lines parallel to the Y axis (constant X, varying Y)
                gl.Vertex(x, -halfGridSize, 0);
                gl.Vertex(x, halfGridSize, 0);
            }

            gl.End();
        }
        public static void RenderYZGrid(OpenGL gl, float gridSize, float lineSpacing)
        {
            if (gridSize == 0 || lineSpacing == 0) { return; }

            float halfGridSize = gridSize / 2f;

            // Set line color (grayish)
            gl.Color(AppSettings.GridColor[0], AppSettings.GridColor[1], AppSettings.GridColor[2]);

            gl.Begin(OpenGL.GL_LINES);

            // Draw lines along the Y axis (horizontal in the YZ plane)
            for (float z = -halfGridSize; z <= halfGridSize; z += lineSpacing)
            {
                // Horizontal lines parallel to the Y axis (constant Z, varying Y)
                gl.Vertex(0, -halfGridSize, z);
                gl.Vertex(0, halfGridSize, z);
            }

            // Draw lines along the Z axis (vertical in the YZ plane)
            for (float y = -halfGridSize; y <= halfGridSize; y += lineSpacing)
            {
                // Vertical lines parallel to the Z axis (constant Y, varying Z)
                gl.Vertex(0, y, -halfGridSize);
                gl.Vertex(0, y, halfGridSize);
            }

            gl.End();
        }

        public static void RenderXZGrid(OpenGL gl, float gridSize, float lineSpacing)
        {
            if (gridSize == 0 || lineSpacing == 0) { return; }

            float halfGridSize = gridSize / 2f;

            // Set line color (grayish)
            gl.Color(AppSettings.GridColor[0], AppSettings.GridColor[1], AppSettings.GridColor[2]);

            gl.Begin(OpenGL.GL_LINES);

            // Draw lines along the X axis (horizontal in the XZ plane)
            for (float z = -halfGridSize; z <= halfGridSize; z += lineSpacing)
            {
                // Horizontal lines parallel to the X axis (constant Z, varying X)
                gl.Vertex(-halfGridSize, 0, z);
                gl.Vertex(halfGridSize, 0, z);
            }

            // Draw lines along the Z axis (vertical in the XZ plane)
            for (float x = -halfGridSize; x <= halfGridSize; x += lineSpacing)
            {
                // Vertical lines parallel to the Z axis (constant X, varying Z)
                gl.Vertex(x, 0, -halfGridSize);
                gl.Vertex(x, 0, halfGridSize);
            }

            gl.End();
        }


        public static void RenderExtents(OpenGL gl, MDLLib.w3Model currentModel)
        {


            // Set the color to blue (R, G, B)
            gl.Color(0.0f, 0.0f, 1.0f); // Blue color

            // Begin drawing lines
            gl.Begin(OpenGL.GL_LINES);

            foreach (w3Line line in currentModel.GeosetLines)
            {


                // Specify the start point of the line
                gl.Vertex(line.From.X, line.From.Y, line.From.Z);

                // Specify the end point of the line
                gl.Vertex(line.To.X, line.To.Y, line.To.Z);
            }

            // End drawing
            gl.End();

        }
        public static void RenderCollisionShapes(OpenGL gl, w3Model currentModel)
        {
            if (currentModel.CollisionShapeLines.Count > 0)
            {
                // Set the color to blue (R, G, B)
                gl.Color(0.0f, 0.0f, 1.0f); // Blue color

                // Begin drawing lines
                gl.Begin(OpenGL.GL_LINES);

                foreach (w3Line line in currentModel.CollisionShapeLines)
                {
                    // Specify the start point of the line
                    gl.Vertex(line.From.X, line.From.Y, line.From.Z);

                    // Specify the end point of the line
                    gl.Vertex(line.To.X, line.To.Y, line.To.Z);
                }

                // End drawing
                gl.End();
            }
        }

        public static void RenderEdges(OpenGL gl, w3Model currentModel, List<w3Geoset> ModifiedGeosets = null)
        {
            List<w3Geoset> Geosets = ModifiedGeosets != null ? ModifiedGeosets : currentModel.Geosets;
            foreach (w3Geoset geo in Geosets)
            {
                if (!geo.isVisible) continue;

             
                    gl.Begin(OpenGL.GL_LINES); // Start drawing lines once for all edges

                    foreach (w3Edge edge in geo.Edges)
                    {
                        if (edge.isSelected)
                        {
                            gl.Color(AppSettings.EdgeColorSelected[0], AppSettings.EdgeColorSelected[1], AppSettings.EdgeColorSelected[2]);

                        }
                        else
                        {
                            gl.Color(AppSettings.EdgeColor[0], AppSettings.EdgeColor[1], AppSettings.EdgeColor[2]);

                        }

                        // Get the vertices by their IDs
                        w3Vertex v1 = edge.Vertex1;
                        w3Vertex v2 = edge.Vertex2;

                        // Draw the edge
                        gl.Vertex(v1.Position.X, v1.Position.Y, v1.Position.Z);
                        gl.Vertex(v2.Position.X, v2.Position.Y, v2.Position.Z);
                    }

                    gl.End(); // End drawing lines after all edges have been drawn
                
            }
        }

        public static void RenderAxis(OpenGL gl, int gridsize)
        {
            float axisLength = gridsize; // Set the length of the axis lines

            gl.Begin(OpenGL.GL_LINES);

            // X-Axis (Red)
            gl.Color(1.0f, 0.0f, 0.0f);  // Red color
            gl.Vertex(0, 0, 0);          // Start at the origin
            gl.Vertex(axisLength, 0, 0);  // End at (axisLength, 0, 0)

            // Y-Axis (Green)
            gl.Color(0.0f, 1.0f, 0.0f);  // Green color
            gl.Vertex(0, 0, 0);          // Start at the origin
            gl.Vertex(0, axisLength, 0);  // End at (0, axisLength, 0)

            // Z-Axis (Blue)
            gl.Color(0.0f, 0.0f, 1.0f);  // Blue color
            gl.Vertex(0, 0, 0);          // Start at the origin
            gl.Vertex(0, 0, axisLength);  // End at (0, 0, axisLength)

            gl.End();
        }

        public static uint LoadTextureFromBitmap(OpenGL gl, Bitmap bitmap)
        {
            if (bitmap == null) { throw new Exception("Provided bitmap for handling texture was null"); }
            uint[] textureId = new uint[1]; // Create an array to hold the texture ID
            gl.GenTextures(1, textureId); // Generate a texture ID
            
            // Set texture parameters
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

            // Convert the Bitmap to a byte array
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                               ImageLockMode.ReadOnly,
                                               System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Upload the texture data
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, bitmap.Width, bitmap.Height, 0,
                          OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, data.Scan0);

            bitmap.UnlockBits(data); // Unlock the bitmap

            return textureId[0]; // Return the generated texture ID
        }
        public static void RenderTriangles(OpenGL gl, w3Model model, bool textured, List<w3Geoset> ModifiedGeosets = null)
        {
            List<w3Geoset> Geosets = ModifiedGeosets != null ? ModifiedGeosets : model.Geosets;
            if (textured) // use textures
            {
               
                // Loop through each geoset in the model
                foreach (w3Geoset geo in Geosets)
                {
                   
                    if (!geo.isVisible) continue; // Skip invisible geosets

                    // Bind the texture for the current geoset
                   if (textured) { gl.Enable(OpenGL.GL_TEXTURE_2D); }
                   if (geo.UsedTexture != null) { geo.UsedTexture.Bind(gl); }
                    

                    // Begin drawing triangles (filled)
                    gl.Begin(OpenGL.GL_TRIANGLES);

                    foreach (w3Triangle triangle in geo.Triangles)
                    {
                        // Get the vertices by their IDs
                        w3Vertex v1 = triangle.Vertex1;
                        w3Vertex v2 = triangle.Vertex2;
                        w3Vertex v3 = triangle.Vertex3;

                        // Set color based on selection
                        if (triangle.isSelected)
                        {
                            gl.Color(AppSettings.Color_VertexSelected[0], AppSettings.Color_VertexSelected[1], AppSettings.Color_VertexSelected[2]); // red
                        }
                        else
                        {
                            gl.Color(1.0f, 1.0f, 1.0f); // white
                        }

                        // Set texture coordinates and vertex positions
                        if (textured)
                        {
                            gl.TexCoord(v1.Texture_Position.U, v1.Texture_Position.V);
                        }
                        gl.Vertex(v1.Position.X, v1.Position.Y, v1.Position.Z);

                        if (textured)
                        {
                            gl.TexCoord(v2.Texture_Position.U, v2.Texture_Position.V);
                        }
                        gl.Vertex(v2.Position.X, v2.Position.Y, v2.Position.Z);

                        if (textured)
                        {
                            gl.TexCoord(v3.Texture_Position.U, v3.Texture_Position.V);
                        }
                        gl.Vertex(v3.Position.X, v3.Position.Y, v3.Position.Z);
                    }

                    // End drawing triangles
                    gl.End();
                }
              if (textured) { gl.Disable(OpenGL.GL_TEXTURE_2D); }
             
            }
            else // use white color
            {
                // Loop through each geoset in the model
                foreach (w3Geoset geo in Geosets)
                {
                    if (!geo.isVisible) continue; // Skip invisible geosets

                    // Begin drawing triangles (filled)
                    gl.Begin(OpenGL.GL_TRIANGLES);

                    foreach (w3Triangle triangle in geo.Triangles)
                    {
                        // Get the vertices by their IDs
                        w3Vertex v1 = triangle.Vertex1;
                        w3Vertex v2 = triangle.Vertex2;
                        w3Vertex v3 = triangle.Vertex3;

                        // Set color based on selection
                        if (triangle.isSelected)
                        {
                            gl.Color(1.0f, 0f, 0f); // red
                        }
                        else
                        {
                            gl.Color(1.0f, 1.0f, 1.0f); // white
                        }

                        // Set vertex positions
                        gl.Vertex(v1.Position.X, v1.Position.Y, v1.Position.Z);
                        gl.Vertex(v2.Position.X, v2.Position.Y, v2.Position.Z);
                        gl.Vertex(v3.Position.X, v3.Position.Y, v3.Position.Z);
                    }

                    // End drawing triangles
                    gl.End();
                }
            }
        }



        public static void RenderNormals(OpenGL gl, w3Model model)
        {
            float normalLength = 0.5f; // Adjust length of the normal visualization

            foreach (w3Geoset geo in model.Geosets)
            {
                foreach (w3Vertex vertex in geo.Vertices)
                {
                    Coordinate pos = vertex.Position;
                    Coordinate normal = vertex.Normal;

                    // Set color to green
                    gl.Color(AppSettings.NormalsColor[0], AppSettings.NormalsColor[1], AppSettings.NormalsColor[2]); // Green

                    // Begin drawing a line
                    gl.Begin(OpenGL.GL_LINES);

                    // Start point of the line is the vertex position
                    gl.Vertex(pos.X, pos.Y, pos.Z);

                    // End point of the line is the vertex position plus the scaled normal vector
                    gl.Vertex(
                        pos.X + normal.X * normalLength,
                        pos.Y + normal.Y * normalLength,
                        pos.Z + normal.Z * normalLength
                    );

                    gl.End();
                }
            }
        }

        public static void RenderRigging(OpenGL gl, w3Model model)
        {
            foreach (w3Geoset geo in model.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    Coordinate From = v.Position;
                    foreach (int boneId in v.AttachedTo)
                    {
                        if (model.Nodes.Any(x => x.objectId == boneId))
                        {
                            w3Node node = model.Nodes.First(x => x.objectId == boneId);
                            if (node.Data is Bone)
                            {
                                Coordinate To = node.PivotPoint;
                                DrawLineBetweenVertexAndBone(gl, From, To);
                            }
                        }
                    }
                }
            }
        }
        private static void DrawLineBetweenVertexAndBone(OpenGL gl, Coordinate from, Coordinate to)
        {
            // Set color to orange
            gl.Color(AppSettings.RiggingColor[0], AppSettings.RiggingColor[1], AppSettings.RiggingColor[2]); // RGB for orange

            // Begin drawing lines
            gl.Begin(OpenGL.GL_LINES);

            // Specify the start point of the line (vertex position)
            gl.Vertex(from.X, from.Y, from.Z);

            // Specify the end point of the line (bone pivot point)
            gl.Vertex(to.X, to.Y, to.Z);

            // End drawing lines
            gl.End();
        }
        private static void DrawLineBetweenNodes(OpenGL gl, Coordinate from, Coordinate to)
        {
      
            gl.Color(AppSettings.SkeletonColor[0], AppSettings.SkeletonColor[0], AppSettings.SkeletonColor[2]); // RGB for purple


            // Begin drawing lines
            gl.Begin(OpenGL.GL_LINES);

            // Specify the start point of the line (vertex position)
            gl.Vertex(from.X, from.Y, from.Z);

            // Specify the end point of the line (bone pivot point)
            gl.Vertex(to.X, to.Y, to.Z);

            // End drawing lines
            gl.End();
        }
        public static void RenderSkeleton(OpenGL gl, w3Model model)
        {
            foreach (w3Node node in model.Nodes)
            {
                if (node.parentId >= 0 && model.Nodes.Any(x=>x.objectId == node.parentId))
                {
                    Coordinate From = node.PivotPoint;
                    Coordinate To = model.Nodes.First(x => x.objectId == node.parentId).PivotPoint;
                     DrawLineBetweenNodes(gl, From, To);
                     
                }
            }
        }




        internal static void RenderNodes(OpenGL gl, w3Model currentModel)
        {
            float rectWidth = 0.5f * AppSettings.PointSize;  // Default width of the rectangle or triangle
            float rectHeight = 0.025f * AppSettings.PointSize; // Adjusted height of the triangle to avoid being too tall

            // Camera position (eye)
            float[] cameraPos = { CameraControl.eyeX, CameraControl.eyeY, CameraControl.eyeZ };

            // Up vector (camera up direction)
            float[] baseUpVector = { CameraControl.UpX, CameraControl.UpY, CameraControl.UpZ };

            // Enable polygon offset to avoid Z-fighting
            gl.Enable(OpenGL.GL_POLYGON_OFFSET_FILL);
            gl.PolygonOffset(-1.0f, -1.0f);

            // Fetch point size from settings
            float pointSize = AppSettings.PointSize;

            foreach (w3Node node in currentModel.Nodes)
            {
                Coordinate pos = node.PivotPoint;

                // Set color based on selection state
                if (node.isSelected)
                {
                    gl.Color(AppSettings.NodeColorSelected[0], AppSettings.NodeColorSelected[1], AppSettings.NodeColorSelected[2]); // Color for selected nodes
                }
                else
                {
                    gl.Color(AppSettings.NodeColor[0], AppSettings.NodeColor[1], AppSettings.NodeColor[2]); // Color for unselected nodes
                }

                // Determine if we draw squares or triangles based on settings
                if (AppSettings.PointType == PointType.Square)
                {
                    // Get the four corners of the rectangle (adjusted for camera view)
                    float[][] rectangleVertices = CalculateBillboardedSquare(pos, rectWidth, cameraPos, (float[])baseUpVector.Clone());

                    // Draw the rectangle
                    gl.Begin(OpenGL.GL_QUADS);

                    gl.Vertex(rectangleVertices[0][0], rectangleVertices[0][1], rectangleVertices[0][2]); // Bottom-left
                    gl.Vertex(rectangleVertices[1][0], rectangleVertices[1][1], rectangleVertices[1][2]); // Bottom-right
                    gl.Vertex(rectangleVertices[2][0], rectangleVertices[2][1], rectangleVertices[2][2]); // Top-right
                    gl.Vertex(rectangleVertices[3][0], rectangleVertices[3][1], rectangleVertices[3][2]); // Top-left

                    gl.End();
                }
                else if (AppSettings.PointType == PointType.Triangle)
                {
                    // Get the three vertices of the triangle (adjusted for camera view)
                    float[][] triangleVertices = CalculateEquilateralTriangle(pos, rectWidth, cameraPos, (float[])baseUpVector.Clone());

                    // Draw the triangle
                    gl.Begin(OpenGL.GL_TRIANGLES);

                    gl.Vertex(triangleVertices[0][0], triangleVertices[0][1], triangleVertices[0][2]); // Bottom-left
                    gl.Vertex(triangleVertices[1][0], triangleVertices[1][1], triangleVertices[1][2]); // Bottom-right
                    gl.Vertex(triangleVertices[2][0], triangleVertices[2][1], triangleVertices[2][2]); // Top

                    gl.End();
                }
            }

            // Disable polygon offset after rendering
            gl.Disable(OpenGL.GL_POLYGON_OFFSET_FILL);
        }


        internal static void RenderVertices(OpenGL gl, w3Model currentModel,
          List<w3Geoset> ModifiedGeosets = null, bool Animating = false, bool AnimatorMode = false)
        {
            float rectWidth = 0.5f * AppSettings.PointSize  ;   // Default width of the rectangle or triangle
            float rectHeight = 0.25f * AppSettings.PointSize  ;  // Adjusted height of the triangle to avoid being too tall

            // Camera position (eye)
            float[] cameraPos = { CameraControl.eyeX, CameraControl.eyeY, CameraControl.eyeZ };

            // Up vector (camera up direction)
            float[] baseUpVector = { CameraControl.UpX, CameraControl.UpY, CameraControl.UpZ };

            // Enable polygon offset for filled polygons (depth bias to push them forward)
            gl.Enable(OpenGL.GL_POLYGON_OFFSET_FILL);
            gl.PolygonOffset(-1.0f, -1.0f);  // Apply a slight depth offset

            // Fetch point size from settings
            float pointSize = AppSettings.PointSize;

          List<w3Geoset> Geosets = AnimatorMode ? ModifiedGeosets : currentModel.Geosets;
            // Render based on point type (square or triangle)
            foreach (w3Geoset geo in Geosets)
            {
                if (!geo.isVisible) continue;

                foreach (w3Vertex vertex in geo.Vertices)
                {
                    Coordinate VertexPosition = vertex.Position;
                    

                    // Set color based on selection state
                    if (vertex.isSelected && vertex.isRigged)
                    {
                        gl.Color(AppSettings.Color_VertexRiggedSelected[0], AppSettings.Color_VertexRiggedSelected[1], AppSettings.Color_VertexRiggedSelected[2]);
                    }
                    else if (vertex.isSelected && !vertex.isRigged)
                    {
                        gl.Color(AppSettings.Color_VertexSelected[0], AppSettings.Color_VertexSelected[1], AppSettings.Color_VertexSelected[2]);
                    }
                    else if (!vertex.isSelected && !vertex.isRigged)
                    {
                        gl.Color(AppSettings.Color_Vertex[0], AppSettings.Color_Vertex[1], AppSettings.Color_Vertex[2]);
                    }
                    else if (!vertex.isSelected && !vertex.isRigged)
                    {
                        gl.Color(AppSettings.Color_VertexRigged[0], AppSettings.Color_VertexRigged[1], AppSettings.Color_VertexRigged[2]);
                    }

                    // Check the type of point rendering from settings
                    if (AppSettings.PointType == PointType.Square)
                    {
                        // Get the four corners of the rectangle (adjusted for camera view)
                        float[][] rectangleVertices = CalculateBillboardedSquare(VertexPosition, rectWidth,  cameraPos, (float[])baseUpVector.Clone());

                        // Begin drawing the rectangle (QUADS)
                        gl.Begin(OpenGL.GL_QUADS);

                        gl.Vertex(rectangleVertices[0][0], rectangleVertices[0][1], rectangleVertices[0][2]); // Bottom-left
                        gl.Vertex(rectangleVertices[1][0], rectangleVertices[1][1], rectangleVertices[1][2]); // Bottom-right
                        gl.Vertex(rectangleVertices[2][0], rectangleVertices[2][1], rectangleVertices[2][2]); // Top-right
                        gl.Vertex(rectangleVertices[3][0], rectangleVertices[3][1], rectangleVertices[3][2]); // Top-left

                        gl.End();
                    }
                    else if (AppSettings.PointType == PointType.Triangle)
                    {
                        // Get the three vertices of the triangle (adjusted for camera view)
                        float[][] triangleVertices = CalculateEquilateralTriangle(VertexPosition, rectWidth, cameraPos, (float[])baseUpVector.Clone());

                        // Begin drawing the triangle (TRIANGLES)
                        gl.Begin(OpenGL.GL_TRIANGLES);

                        gl.Vertex(triangleVertices[0][0], triangleVertices[0][1], triangleVertices[0][2]); // Bottom-left
                        gl.Vertex(triangleVertices[1][0], triangleVertices[1][1], triangleVertices[1][2]); // Bottom-right
                        gl.Vertex(triangleVertices[2][0], triangleVertices[2][1], triangleVertices[2][2]); // Top

                        gl.End();
                    }
                }
            }

            // Disable polygon offset after rendering
            gl.Disable(OpenGL.GL_POLYGON_OFFSET_FILL);
        }
        internal static float[][] CalculateEquilateralTriangle(Coordinate pos, float edgeLength, float[] cameraPos, float[] upVector)
        {
            // Calculate the forward vector from the camera to the position
            float[] forwardVector = { pos.X - cameraPos[0], pos.Y - cameraPos[1], pos.Z - cameraPos[2] };
            NormalizeVector(forwardVector);

            // Calculate the right vector as a cross product of the forward vector and up vector
            float[] rightVector = CrossProduct(forwardVector, upVector);
            NormalizeVector(rightVector);

            // Normalize the up vector to ensure consistency
            NormalizeVector(upVector);

            // Calculate the base of the triangle using the right vector
            float[][] triangleVertices = new float[3][];

            // Bottom-left vertex
            triangleVertices[0] = new float[]
            {
        pos.X - rightVector[0] * (edgeLength / 2),
        pos.Y - rightVector[1] * (edgeLength / 2),
        pos.Z - rightVector[2] * (edgeLength / 2)
            };

            // Bottom-right vertex
            triangleVertices[1] = new float[]
            {
        pos.X + rightVector[0] * (edgeLength / 2),
        pos.Y + rightVector[1] * (edgeLength / 2),
        pos.Z + rightVector[2] * (edgeLength / 2)
            };

            // Top vertex: at 60 degrees relative to the base to form an equilateral triangle
            float height = (float)(Math.Sqrt(3) / 2.0f * edgeLength);

            triangleVertices[2] = new float[]
            {
        pos.X + upVector[0] * height - forwardVector[0] * height * 0.1f, // Adding slight forward bias to avoid "stretch"
        pos.Y + upVector[1] * height - forwardVector[1] * height * 0.1f,
        pos.Z + upVector[2] * height - forwardVector[2] * height * 0.1f
            };

            return triangleVertices;
        }


















        internal static void NormalizeVector(float[] vector)
        {
            float length = (float)Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);

            // Prevent division by zero
            if (length == 0.0f)
                return;

            vector[0] /= length;
            vector[1] /= length;
            vector[2] /= length;
        }

        internal static float[] CrossProduct(float[] vectorA, float[] vectorB)
        {
            return new float[]
            {
        vectorA[1] * vectorB[2] - vectorA[2] * vectorB[1],
        vectorA[2] * vectorB[0] - vectorA[0] * vectorB[2],
        vectorA[0] * vectorB[1] - vectorA[1] * vectorB[0]
            };
        }

        internal static float[][] CalculateBillboardedSquare(
    Coordinate pos, float squareSize, float[] cameraPos, float[] upVector)
        {
            // Get the vector from the camera to the vertex position
            float[] lookVector = new float[3];
            lookVector[0] = pos.X - cameraPos[0];
            lookVector[1] = pos.Y - cameraPos[1];
            lookVector[2] = pos.Z - cameraPos[2];

            // Normalize the look vector
            float length = (float)Math.Sqrt(lookVector[0] * lookVector[0] + lookVector[1] * lookVector[1] + lookVector[2] * lookVector[2]);
            lookVector[0] /= length;
            lookVector[1] /= length;
            lookVector[2] /= length;

            // Calculate the right vector (cross product of look vector and up vector)
            float[] rightVector = new float[3];
            rightVector[0] = upVector[1] * lookVector[2] - upVector[2] * lookVector[1];
            rightVector[1] = upVector[2] * lookVector[0] - upVector[0] * lookVector[2];
            rightVector[2] = upVector[0] * lookVector[1] - upVector[1] * lookVector[0];

            // Normalize and scale the right vector by half the square size
            length = (float)Math.Sqrt(rightVector[0] * rightVector[0] + rightVector[1] * rightVector[1] + rightVector[2] * rightVector[2]);
            rightVector[0] = (rightVector[0] / length) * (squareSize / 2);
            rightVector[1] = (rightVector[1] / length) * (squareSize / 2);
            rightVector[2] = (rightVector[2] / length) * (squareSize / 2);

            // Normalize and scale the up vector by half the square size
            length = (float)Math.Sqrt(upVector[0] * upVector[0] + upVector[1] * upVector[1] + upVector[2] * upVector[2]);
            upVector[0] = (upVector[0] / length) * (squareSize / 2);
            upVector[1] = (upVector[1] / length) * (squareSize / 2);
            upVector[2] = (upVector[2] / length) * (squareSize / 2);

            // Calculate the positions of the four corners of the square
            float[][] squareVertices = new float[4][];

            squareVertices[0] = new float[] { pos.X - rightVector[0] - upVector[0], pos.Y - rightVector[1] - upVector[1], pos.Z - rightVector[2] - upVector[2] }; // Bottom-left
            squareVertices[1] = new float[] { pos.X + rightVector[0] - upVector[0], pos.Y + rightVector[1] - upVector[1], pos.Z + rightVector[2] - upVector[2] }; // Bottom-right
            squareVertices[2] = new float[] { pos.X + rightVector[0] + upVector[0], pos.Y + rightVector[1] + upVector[1], pos.Z + rightVector[2] + upVector[2] }; // Top-right
            squareVertices[3] = new float[] { pos.X - rightVector[0] + upVector[0], pos.Y - rightVector[1] + upVector[1], pos.Z - rightVector[2] + upVector[2] }; // Top-left

            return squareVertices;
        }

        internal static void ApplySSAA(OpenGL gl)
        {
            return;
            
        }

        internal static void ApplySMAA(OpenGL gl)
        {
            return;
        }

        internal static void ApplyFXAA(OpenGL gl)
        {
            return;
        }

        internal static void HandleLighting(OpenGL gl)
        {
            if (AppSettings.EnableLighting)
            {
                // Enable lighting and set up light 0
                gl.Enable(OpenGL.GL_LIGHTING);
                gl.Enable(OpenGL.GL_LIGHT0);

                // Set the ambient, diffuse, and specular components of the light
                gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, AppSettings.AmbientColor);
                gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, AppSettings.DiffuseColor);
                gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, AppSettings.SpecularColor);
                gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, AppSettings.LightPostion);

                // Set material properties for the object being drawn
                gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, AppSettings.Shininess);
            }
            else
            {
                // Disable lighting
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_LIGHT0);
            }
        }


        internal static void HandeShading(OpenGL gl)
        {
            if (DisplayOptions.Shading)
            {
                // Enable smooth shading or flat shading based on preference
                gl.ShadeModel(OpenGL.GL_SMOOTH);  // Or use GL_FLAT for flat shading
            }
            else
            {
                // Default back to flat shading or disable shading if preferred
                gl.ShadeModel(OpenGL.GL_FLAT);
            }
        }


        internal static void HandleCulling(OpenGL gl)
        {
            if (AppSettings.BackfaceCullingEnabled)
            {
                if (AppSettings.BackfaceCullingClockwise)
                {
                    // Enable backface culling
                    gl.Enable(OpenGL.GL_CULL_FACE);

                    // Set the front face winding order to clockwise (CW)
                    gl.FrontFace(OpenGL.GL_CW);

                    // Cull back faces
                    gl.CullFace(OpenGL.GL_BACK);
                }
                else
                {
                    // Enable backface culling
                    gl.Enable(OpenGL.GL_CULL_FACE);

                    // Set the front face winding order to counterclockwise (CCW)
                    gl.FrontFace(OpenGL.GL_CCW);

                    // Cull back faces
                    gl.CullFace(OpenGL.GL_BACK);
                }
            }
        }

        internal static void HandleAntiAliasing(OpenGL gl)
        {
            //unfinished
            switch (AppSettings.AA)
            {
                case AntialiasingTechnique.None:
                    // Disable multisampling and any antialiasing techniques
                    gl.Disable(OpenGL.GL_MULTISAMPLE);
                    break;

                case AntialiasingTechnique.MSAA_2x:
                    // Enable 2x Multisample Anti-Aliasing (MSAA)
                    gl.Enable(OpenGL.GL_MULTISAMPLE);
                    // Ensure your OpenGL context is set up for 2x MSAA
                    break;

                case AntialiasingTechnique.MSAA_4x:
                    // Enable 4x MSAA
                    gl.Enable(OpenGL.GL_MULTISAMPLE);
                    // Ensure your OpenGL context is set up for 4x MSAA
                    break;

                case AntialiasingTechnique.MSAA_8x:
                    // Enable 8x MSAA
                    gl.Enable(OpenGL.GL_MULTISAMPLE);
                    // Ensure your OpenGL context is set up for 8x MSAA
                    break;

                case AntialiasingTechnique.FXAA:
                    // FXAA needs to be implemented as a post-processing shader
                    Renderer.ApplyFXAA(gl); // Placeholder function where you'd set up and apply FXAA shader
                    break;

                case AntialiasingTechnique.SMAA:
                    // Similar to FXAA, SMAA would need to be implemented as a shader
                    Renderer.ApplySMAA(gl); // Placeholder function for SMAA shader setup
                    break;

                case AntialiasingTechnique.SSAA:
                    // Supersampling (SSAA) involves rendering at a higher resolution
                    Renderer.ApplySSAA(gl); // Placeholder function for SSAA setup
                    break;

                case AntialiasingTechnique.Blending:
                    // Enable alpha blending to smooth edges
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                    break;

                default:
                    gl.Disable(OpenGL.GL_MULTISAMPLE); // Default: No AA
                    break;
            }
        }
    }
}
