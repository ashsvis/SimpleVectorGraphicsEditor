using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using SimpleVectorGraphicsEditor.Markers;

namespace SimpleVectorGraphicsEditor.Figures
{
    [Serializable]
    public class Polygon : Figure, ISolidFigure, IVertexSupport, IRotateSupport
    {
        public Polygon(Point origin, Point offset)
        {
            var rect = new Rectangle(origin, new Size(offset.X, offset.Y));
            var points = new List<PointF>
                {
                    new PointF(rect.Left, rect.Top),
                    new PointF(rect.Left + rect.Width, rect.Top),
                    new PointF(rect.Left + rect.Width, rect.Top + rect.Height),
                    new PointF(rect.Left, rect.Top + rect.Height)
                };
            Points.AddRange(points);
        }

        protected override void AddFigureToGraphicsPath(GraphicsPath gp)
        {
            gp.AddPolygon(GetPoints());
        }

        public override void Draw(Graphics graphics)
        {
            var points = Points.ToArray();
            // заливаем фон кистью
            using (var brush = new SolidBrush(Color.White))
                graphics.FillPolygon(Fill.UpdateBrush(brush), points);
            // рисуем контур карандашом
            using (var pen = new Pen(Color.Black))
                graphics.DrawPolygon(Stroke.UpdatePen(pen), points);
        }

        public void DrawCustomFigure(Graphics graphics, PointF[] points)
        {
            // определяем "карандаш" тонкий чёрный пунктирный
            using (var pen = new Pen(Color.Black, 1f))
            {
                pen.DashStyle = DashStyle.Dash;
                graphics.DrawPolygon(pen, points); // рисование контура
            }
        }

        protected override IEnumerable<Marker> CreateSizeMarkers()
        {
            var markers = new List<Marker>
                {
                    new TopLeftSizeMarker {TargetFigure = this},
                    new TopMiddleSizeMarker {TargetFigure = this},
                    new TopRightSizeMarker {TargetFigure = this},
                    new MiddleRightSizeMarker {TargetFigure = this},
                    new BottomRightSizeMarker {TargetFigure = this},
                    new BottomMiddleSizeMarker {TargetFigure = this},
                    new BottomLeftSizeMarker {TargetFigure = this},
                    new MiddleLeftSizeMarker {TargetFigure = this}
                };
            return markers;
        }

        public IEnumerable<Marker> CreateVertexMarkers()
        {
            var markers = new List<Marker>();
            for (var i = 0; i < GetPoints().Length; i++)
                markers.Add(new VertexLocationMarker(i) { TargetFigure = this });
            return markers;
        }

        public override bool PointInFigure(PointF point)
        {
            using (var gp = new GraphicsPath())
            {
                gp.AddPolygon(GetPoints());
                return (gp.IsVisible(point));
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                using (var gp = new GraphicsPath())
                {
                    gp.AddPolygon(GetPoints());
                    BoundsRect = gp.GetBounds();
                    return base.Bounds;
                }
            }
        }

        /// <summary>
        /// Поворот вокруг точки
        /// </summary>
        /// <param name="angle">угол поворота</param>
        /// <param name="cx">центр X</param>
        /// <param name="cy">центр Y</param>
        public void RotateAt(float angle, float cx, float cy)
        {
            using (var gp = new GraphicsPath())
            {
                gp.AddPolygon(GetPoints());
                using (var m = new Matrix())
                {
                    m.RotateAt(angle, new PointF(cx, cy));
                    gp.Transform(m);
                }
                var ps = gp.PathPoints;
                SetPoints(ps);
            }
        }

        public override void DrawSizeMarkers(Graphics graphics)
        {
            foreach (var marker in SizeMarkers)
            {
                marker.UpdateLocation();
                marker.Draw(graphics);
            }
        }

        public void DrawVertexMarkers(Graphics graphics)
        {
            foreach (var marker in VertexMarkers)
            {
                marker.UpdateLocation();
                marker.Draw(graphics);
            }
        }

    }
}
