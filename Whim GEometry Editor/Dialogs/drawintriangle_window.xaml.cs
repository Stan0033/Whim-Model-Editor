using MDLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    /// Interaction logic for drawintriangle_window.xaml
    /// </summary>
    public partial class drawintriangle_window : Window
    {
        private enum WorkMode
        {
            Select, Move,
            Rotate,
            Scale
        }
        public drawintriangle_window(w3Triangle triangle, w3Geoset geoset)
        {
            InitializeComponent();
            SelectedTriangle = triangle;
            InWhichGeoset = geoset;
        }
        private w3Triangle SelectedTriangle; // .Vertex1.X, .Vertex2.Y, .Vertex3.Z
        private w3Geoset InWhichGeoset;
        private int CurrentNumberOfAngles = 3;
        private List<Vector2> CurrentPoints;
        private WorkMode workMode = WorkMode.Select;
        private void Redraw(int corners)
        {
            int countCorners = corners < 3 ? 3 : corners;
            MyCanvas.Children.Clear();

            // Project 3D vertices of the triangle onto the canvas
            Point p1 = ProjectToCanvas(SelectedTriangle.Vertex1.Position);
            Point p2 = ProjectToCanvas(SelectedTriangle.Vertex2.Position);
            Point p3 = ProjectToCanvas(SelectedTriangle.Vertex3.Position);

            // Draw the triangle on the canvas
            DrawLine(p1, p2);
            DrawLine(p2, p3);
            DrawLine(p3, p1);

            // Generate and draw the shape with given corners inside the triangle
            var shapePoints = GeneratePolygonInsideTriangle(p1, p2, p3, countCorners);
            DrawPolygon(shapePoints);
        }
        private Point ProjectToCanvas(Coordinate position)
        {
            // Simple orthographic projection; adapt as needed
            return new Point(
                (position.X + 1) * 300, // Scale X to canvas width (600 / 2 = 300)
                (1 - position.Y) * 225  // Scale Y to canvas height (450 / 2 = 225)
            );
        }

        private void DrawLine(Point p1, Point p2)
        {
            var line = new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            MyCanvas.Children.Add(line);
        }
        private List<Point> GeneratePolygonInsideTriangle(Point p1, Point p2, Point p3, int corners)
        {
            var points = new List<Point>();
            for (int i = 0; i < corners; i++)
            {
                double t1 = 1 - (double)i / corners;
                double t2 = (double)i / corners;

                // Interpolate points along two triangle edges
                Point edge1 = new Point(p1.X * t1 + p2.X * t2, p1.Y * t1 + p2.Y * t2);
                Point edge2 = new Point(p1.X * t1 + p3.X * t2, p1.Y * t1 + p3.Y * t2);

                // Average the two edge points to position inside the triangle
                points.Add(new Point((edge1.X + edge2.X) / 2, (edge1.Y + edge2.Y) / 2));
            }
            return points;
        }

        private void DrawPolygon(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                // Draw lines between points
                DrawLine(points[i], points[(i + 1) % points.Count]);

                // Draw square points
                var rect = new Rectangle
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(rect, points[i].X - 2.5);
                Canvas.SetTop(rect, points[i].Y - 2.5);
                MyCanvas.Children.Add(rect);
            }
        }
        private void CreateDrawnShapeInTriangle()
        {
            // the canvas is height 450 width 600
        }

        private void setTriangle(object sender, RoutedEventArgs e)
        {
            Redraw(3);
        }

        private void setSquare(object sender, RoutedEventArgs e)
        {
            Redraw(4);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void setNgons(object sender, RoutedEventArgs e)
        {
            bool parsed = int.TryParse(Box.Text, out int count);
            if (parsed)
            {
               if (count < 5) { Redraw(5); }
               else { Redraw(count); }
            }
            else { Redraw(5); }
        }

        private void ok(object sender, RoutedEventArgs e)
        {

        }

        private void SetModeSelect(object sender, RoutedEventArgs e)
        {
            workMode = WorkMode.Select;
        }

        private void SetModeMove(object sender, RoutedEventArgs e)
        {
            workMode = WorkMode.Move;
        }

        private void SetModeRotate(object sender, RoutedEventArgs e)
        {
            workMode = WorkMode.Rotate;
        }

        private void SetModeScale(object sender, RoutedEventArgs e)
        {
            workMode = WorkMode.Scale;
        }
    }
}