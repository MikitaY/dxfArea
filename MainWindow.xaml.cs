using Microsoft.Win32;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using Line = netDxf.Entities.Line;

namespace dxfArea
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openDxfFile = new OpenFileDialog
            {
                Title = "Open dxf file",
                Filter = "Excel Workbook (*.dxf)|*.dxf",
                RestoreDirectory = true,
                CheckFileExists = true,
            };

            openDxfFile.ShowDialog();

            var doc = DxfDocument.Load(openDxfFile.FileName);

            var polylines2D = doc.Entities.Polylines2D;
            var lines = doc.Entities.Lines.ToList();
            var arcs = doc.Entities.Arcs.ToList();
            var sircle = doc.Entities.Circles.ToList();

            foreach (var pLine in polylines2D)
            {
                var eOPolyline = pLine.Explode();
                var eLines = eOPolyline
                    .Where(s => s.Type == EntityType.Line).Cast<Line>();
                var eArcs = eOPolyline
                    .Where(s => s.Type == EntityType.Arc).Cast<Arc>();

                lines.AddRange(eLines);
                arcs.AddRange(eArcs);
            }

            var lineFromArc = new List<Line>();

            foreach (var arc in arcs)
            {
                var poligonalVertexes = arc.PolygonalVertexes(3000);

                for (var i = 0; i < 2999; i++)
                {
                    var sVector2Line = new Vector2(arc.Center.X + poligonalVertexes[i].X, arc.Center.Y + poligonalVertexes[i].Y);
                    var fVector2Line = new Vector2(arc.Center.X + poligonalVertexes[i + 1].X, arc.Center.Y + poligonalVertexes[i + 1].Y);

                    lineFromArc.Add(new Line(sVector2Line, fVector2Line));
                }
            }

            lines.AddRange(lineFromArc);

            var points = new List<netDxf.Vector2>();

            lines = RoundingCoordinatesLine(lines);

            if (lines.Count != 0)
                points = GetSequencePoint(lines);
            else
                MessageBox.Show("No suitable objects were found to calculate the area. It is not possible to determine the area of the part");

            //arcs.ArcsToLine();

            if (lines.Count * 2 != points.Count)
            {
                MessageBox.Show("No matching lines or polylines were found.");
                return;
            }

            var area = Math.Abs(points.Take(points.Count - 1)
               .Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
               .Sum() / 2);

            this.ResultLable.Content = $"Area: {area:f2}";
        }

        private List<Vector2> GetSequencePoint(List<Line> lines)
        {
            var sLines = new List<Line>(lines);
            var sortPoints = new List<Vector2>();

            var startLine = sLines[0];
            sLines.RemoveAt(0);

            sortPoints.Add(new Vector2(startLine.StartPoint.X, startLine.StartPoint.Y));
            sortPoints.Add(new Vector2(startLine.EndPoint.X, startLine.EndPoint.Y));

            for (int i = 0; i < (lines.Count - 1); i++)
            {
                sortPoints.Last();

                var lastVector3 = new Vector3(sortPoints.Last().X, sortPoints.Last().Y, 0);

                var nextLine = sLines?.First(
                    (s) =>
                    {
                        return (s.StartPoint == lastVector3 || s.EndPoint == lastVector3);
                    });

                if (nextLine == null)
                {
                    MessageBox.Show("It is not possible to define a closed contour to calculate the area of the part.");
                    return sortPoints;
                }

                sLines?.Remove(nextLine);
                if (nextLine.EndPoint == lastVector3)
                    nextLine.Reverse();

                sortPoints.Add(new Vector2(nextLine.StartPoint.X, nextLine.StartPoint.Y));
                sortPoints.Add(new Vector2(nextLine.EndPoint.X, nextLine.EndPoint.Y));
            }

            return sortPoints;
        }

        private List<Line> RoundingCoordinatesLine(List<Line> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].StartPoint = RoundingVector3(lines[i].StartPoint);
                lines[i].EndPoint = RoundingVector3(lines[i].EndPoint);
            }

            return lines;
        }

        private List<Polyline2D> RoundingCoordinatesPolyline2D(List<Polyline2D> pLines)
        {
            for (int i = 0; i < pLines.Count; i++)
            {
                var vertexesList = pLines[i].Vertexes;

                for (var j = 0; j < vertexesList.Count; j++)
                {
                    vertexesList[j].Position = RoundingVector2(vertexesList[j].Position);
                }
            }

            return pLines;
        }

        private Vector3 RoundingVector3(Vector3 vector)
        {
            vector.X = Math.Round(vector.X, 5);
            vector.Y = Math.Round(vector.Y, 5);
            vector.Z = Math.Round(vector.Z, 5);

            return vector;
        }

        private Vector2 RoundingVector2(Vector2 vector)
        {
            vector.X = Math.Round(vector.X, 5);
            vector.Y = Math.Round(vector.Y, 5);

            return vector;
        }
    }
}
