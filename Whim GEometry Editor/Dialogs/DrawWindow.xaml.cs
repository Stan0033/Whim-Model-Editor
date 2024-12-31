using MDLLib;
using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for DrawWindow.xaml
    /// </summary>
    /// pu
    public enum DrawMode
    {
        None,Pencil,Line
    }
    public partial class DrawWindow : Window
    {
        private bool isDrawing;
        private Point startPoint;
        private List<Shape> shapes = new List<Shape>(); // To store drawn shapes for undo/redo
        private int currentShapeIndex = -1; // Index of the currently drawn shape
        private bool isPencilMode = true; // Current drawing mode
        private List<Shape> undoneShapes = new List<Shape>(); // To manage redo functionality
        w3Model Model;
        private Polyline currentPencilLine; // Current polyline for pencil drawing
        private Line currentLine; // Current line for line drawing

        private DrawMode currentDrawMode = DrawMode.None;
        public DrawWindow(w3Model model)
        {
            InitializeComponent();
            Model = model;
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the starting point
            startPoint = e.GetPosition(drawingCanvas);

            if (currentDrawMode == DrawMode.Pencil)
            {
                // Pencil mode: Start a new Polyline for the pencil drawing
                isDrawing = true;
                currentPencilLine = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                currentPencilLine.Points.Add(startPoint);
                drawingCanvas.Children.Add(currentPencilLine);
                shapes.Add(currentPencilLine);
                currentShapeIndex = shapes.Count - 1;
                undoneShapes.Clear(); // Clear redo stack
            }
            else if (currentDrawMode == DrawMode.Line)
            {
                // Line mode: Initialize a new Line object
                isDrawing = true;
                currentLine = new Line
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    X1 = startPoint.X,
                    Y1 = startPoint.Y
                };
                drawingCanvas.Children.Add(currentLine);
                shapes.Add(currentLine);
                currentShapeIndex = shapes.Count - 1;
                undoneShapes.Clear(); // Clear redo stack
            }
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;

            Point currentPoint = e.GetPosition(drawingCanvas);

            if (currentDrawMode == DrawMode.Pencil)
            {
                // Continue adding points to the Polyline for freehand drawing
                currentPencilLine?.Points.Add(currentPoint);
            }
            else if (currentDrawMode == DrawMode.Line && currentLine != null)
            {
                // Update the end point of the line as the mouse moves
                currentLine.X2 = currentPoint.X;
                currentLine.Y2 = currentPoint.Y;
            }
        }


        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            if (currentDrawMode == DrawMode.Line)
            {
                // Finalize the line when the mouse button is released
                Point endPoint = e.GetPosition(drawingCanvas);
                currentLine.X2 = endPoint.X;
                currentLine.Y2 = endPoint.Y;
                currentLine = null;
            }
        }

        private Ellipse CreatePencilDot(Point point)
        {
            var pencilDot = new Ellipse
            {
                Fill = Brushes.Black,
                Width = 5,
                Height = 5
            };
            Canvas.SetLeft(pencilDot, point.X - pencilDot.Width / 2);
            Canvas.SetTop(pencilDot, point.Y - pencilDot.Height / 2);
            return pencilDot;
        }

        private void PencilButton_Click(object sender, RoutedEventArgs e)
        {
            currentDrawMode = DrawMode.Pencil;
            pencilButton.Background = new SolidColorBrush(Colors.White);
            lineButton.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            currentDrawMode = DrawMode.Line;
            pencilButton.Background = new SolidColorBrush(Colors.LightGray);
            lineButton.Background = new SolidColorBrush(Colors.White);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentShapeIndex >= 0)
            {
                var shape = shapes[currentShapeIndex];
                drawingCanvas.Children.Remove(shape);
                undoneShapes.Add(shape);
                currentShapeIndex--;
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (undoneShapes.Count > 0)
            {
                var shape = undoneShapes[^1]; // Get the last undone shape
                drawingCanvas.Children.Add(shape);
                shapes.Add(shape);
                undoneShapes.RemoveAt(undoneShapes.Count - 1);
                currentShapeIndex++;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            w3Geoset geo = ConvertCanvasShapesToGeoset(drawingCanvas, 10);
            Calculator3D.CenterVertices(geo.Vertices, 0, 0, 0);
            geo.ID = IDCounter.Next();
            import_geoset ig = new import_geoset("Custom drawn shape", Model,  new List<w3Geoset>() { geo});
            ig.ShowDialog();
            if (ig.DialogResult == true)
            {
                DialogResult = true;
            }

        }
        public w3Geoset ConvertCanvasShapesToGeoset(Canvas canvas, double extrusionDepth)
        {
            // Create lists for vertices and triangles
            List<w3Vertex> allVertices = new List<w3Vertex>();
            List<w3Triangle> allTriangles = new List<w3Triangle>();

            // Iterate through all shapes in the Canvas
           // MessageBox.Show("children: " + canvas.Children.Count.ToString());
            foreach (var child in canvas.Children)
            {
                if (child is Polyline polyline)
                {
                    // Convert the Polyline to vertices and triangles
                    ConvertPolylineToGeoset(polyline, extrusionDepth, allVertices, allTriangles);
                }
                else if (child is Line line)
                {
                    // Convert the Line to vertices and triangles
                    ConvertLineToGeoset(line, extrusionDepth, allVertices, allTriangles);
                }
                else if (child is Ellipse ellipse)
                {
                    // Convert the Ellipse to vertices and triangles
                    ConvertEllipseToGeoset(ellipse, extrusionDepth, allVertices, allTriangles);
                }
                else if (child is Rectangle rectangle)
                {
                    // Convert the Rectangle to vertices and triangles
                    ConvertRectangleToGeoset(rectangle, extrusionDepth, allVertices, allTriangles);
                }
                // Add other shape types as needed...
            }

            // Create the final w3Geoset
            w3Geoset geoset = new w3Geoset();
            geoset.Vertices.AddRange(allVertices);
            geoset.Triangles.AddRange(allTriangles);

            return geoset;
        }
        private void ConvertPolylineToGeoset(Polyline polyline, double extrusionDepth, List<w3Vertex> vertices, List<w3Triangle> triangles)
        {
            // Extract vertices from the polyline (front face)
            List<w3Vertex> polylineVertices = polyline.Points
                .Select(p => new w3Vertex(IDCounter.Next(), (float)p.X, (float)p.Y, 0)) // Front face (Z = 0)
                .ToList();

            // Create extruded vertices for the back face
            List<w3Vertex> extrudedVertices = polylineVertices
                .Select(v => new w3Vertex(IDCounter.Next(), v.Position. X, v.Position .Y,(float) extrusionDepth)) // Back face (Z = extrusionDepth)
                .ToList();

            // Add front and back vertices to the final vertex list
            vertices.AddRange(polylineVertices);
            vertices.AddRange(extrudedVertices);

            // Add front and back triangles
            for (int i = 0; i < polylineVertices.Count - 2; i++)
            {
                triangles.Add(new w3Triangle(polylineVertices[i], polylineVertices[i + 1], polylineVertices[i + 2]));
                triangles.Add(new w3Triangle(extrudedVertices[i], extrudedVertices[i + 1], extrudedVertices[i + 2]));
            }

            // Add side faces between front and back vertices
            for (int i = 0; i < polylineVertices.Count - 1; i++)
            {
                var front1 = polylineVertices[i];
                var front2 = polylineVertices[i + 1];
                var back1 = extrudedVertices[i];
                var back2 = extrudedVertices[i + 1];

                // Two triangles per side face
                triangles.Add(new w3Triangle(front1, back1, front2));
                triangles.Add(new w3Triangle(back1, back2, front2));
            }
        }

        private void ConvertLineToGeoset(Line line, double extrusionDepth, List<w3Vertex> vertices, List<w3Triangle> triangles)
        {
            // Create front face vertices from the line's endpoints
            w3Vertex front1 = new w3Vertex(IDCounter.Next(), (float)line.X1, (float)line.Y1, 0);
            w3Vertex front2 = new w3Vertex(IDCounter.Next(), (float)line.X2, (float)line.Y2, 0);

            // Create back face vertices by extruding along the Z-axis
            w3Vertex back1 = new w3Vertex(IDCounter.Next(), (float)line.X1, (float)line.Y1, (float)extrusionDepth);
            w3Vertex back2 = new w3Vertex(IDCounter.Next(), (float)line.X2, (float)line.Y2, (float)extrusionDepth);

            // Add vertices to the final list
            vertices.AddRange(new[] { front1, front2, back1, back2 });

            // Create side faces by connecting front and back vertices
            triangles.Add(new w3Triangle(front1, back1, front2));
            triangles.Add(new w3Triangle(back1, back2, front2));
        }
        private void ConvertEllipseToGeoset(Ellipse ellipse, double extrusionDepth, List<w3Vertex> vertices, List<w3Triangle> triangles)
        {
            // Number of points to approximate the ellipse
            int numPoints = 32;
            double centerX = Canvas.GetLeft(ellipse) + ellipse.Width / 2;
            double centerY = Canvas.GetTop(ellipse) + ellipse.Height / 2;
            double radiusX = ellipse.Width / 2;
            double radiusY = ellipse.Height / 2;

            List<w3Vertex> frontVertices = new List<w3Vertex>();
            List<w3Vertex> backVertices = new List<w3Vertex>();

            for (int i = 0; i < numPoints; i++)
            {
                double angle = 2 * Math.PI * i / numPoints;
                double x = centerX + radiusX * Math.Cos(angle);
                double y = centerY + radiusY * Math.Sin(angle);

                // Front face vertex
                var frontVertex = new w3Vertex(IDCounter.Next(), (float)x, (float)y, 0);
                frontVertices.Add(frontVertex);

                // Back face vertex
                var backVertex = new w3Vertex(IDCounter.Next(), (float)x, (float)y, (float)extrusionDepth);
                backVertices.Add(backVertex);
            }

            vertices.AddRange(frontVertices);
            vertices.AddRange(backVertices);

            // Create triangles for front and back faces
            for (int i = 0; i < numPoints - 2; i++)
            {
                triangles.Add(new w3Triangle(frontVertices[0], frontVertices[i + 1], frontVertices[i + 2]));
                triangles.Add(new w3Triangle(backVertices[0], backVertices[i + 1], backVertices[i + 2]));
            }

            // Create side faces
            for (int i = 0; i < numPoints; i++)
            {
                int next = (i + 1) % numPoints;
                triangles.Add(new w3Triangle(frontVertices[i], backVertices[i], frontVertices[next]));
                triangles.Add(new w3Triangle(backVertices[i], backVertices[next], frontVertices[next]));
            }
        }
        private void ConvertRectangleToGeoset(Rectangle rectangle, double extrusionDepth, List<w3Vertex> vertices, List<w3Triangle> triangles)
        {
            // Get rectangle corners
            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);
            double right = left + rectangle.Width;
            double bottom = top + rectangle.Height;

            // Front face vertices (Z = 0)
            var frontTopLeft = new w3Vertex(IDCounter.Next(), (float)left, (float)top, 0);
            var frontTopRight = new w3Vertex(IDCounter.Next(), (float)right, (float)top, 0);
            var frontBottomLeft = new w3Vertex(IDCounter.Next(), (float)left, (float)bottom, 0);
            var frontBottomRight = new w3Vertex(IDCounter.Next(), (float)right, (float)bottom, 0);

            // Back face vertices (Z = extrusionDepth)
            var backTopLeft = new w3Vertex(IDCounter.Next(), (float)left, (float)top, (float)extrusionDepth);
            var backTopRight = new w3Vertex(IDCounter.Next(), (float)right, (float)top, (float)extrusionDepth);
            var backBottomLeft = new w3Vertex(IDCounter.Next(), (float)left, (float)bottom, (float)extrusionDepth);
            var backBottomRight = new w3Vertex(IDCounter.Next(), (float)right, (float)bottom, (float)extrusionDepth);

            vertices.AddRange(new[] { frontTopLeft, frontTopRight, frontBottomLeft, frontBottomRight });
            vertices.AddRange(new[] { backTopLeft, backTopRight, backBottomLeft, backBottomRight });

            // Front face triangles
            triangles.Add(new w3Triangle(frontTopLeft, frontTopRight, frontBottomLeft));
            triangles.Add(new w3Triangle(frontBottomLeft, frontTopRight, frontBottomRight));

            // Back face triangles
            triangles.Add(new w3Triangle(backTopLeft, backTopRight, backBottomLeft));
            triangles.Add(new w3Triangle(backBottomLeft, backTopRight, backBottomRight));

            // Side faces
            triangles.Add(new w3Triangle(frontTopLeft, backTopLeft, frontTopRight));
            triangles.Add(new w3Triangle(backTopLeft, backTopRight, frontTopRight));
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            shapes.Clear();
            currentShapeIndex = -1;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
