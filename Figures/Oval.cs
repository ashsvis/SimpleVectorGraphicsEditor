using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using SimpleVectorGraphicsEditor.Markers;

namespace SimpleVectorGraphicsEditor.Figures
{
    [Serializable]
    public class Oval : Figure, ISolidFigure
    {
        protected RectangleF Basicrect;

        protected Oval()
        {
            Basicrect = new RectangleF();
        }

        public Oval(Point origin, Point offset)
        {
            Basicrect = new Rectangle(origin, new Size(offset.X, offset.Y));
        }

        public override void Draw(Graphics graphics)
        {
            // заливаем фон кистью
            using (var brush = new SolidBrush(Color.White))
                graphics.FillEllipse(Fill.UpdateBrush(brush), Basicrect);
            // рисуем контур карандашом
            using (var pen = new Pen(Color.Black))
                graphics.DrawEllipse(Stroke.UpdatePen(pen), Rectangle.Ceiling(Basicrect));
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

        public override void DrawSizeMarkers(Graphics graphics)
        {
            foreach (var marker in SizeMarkers)
            {
                marker.UpdateLocation();
                marker.Draw(graphics);
            }
        }

        public override bool PointInFigure(PointF point)
        {
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(Basicrect);
                return (gp.IsVisible(point));
            }
        }

        protected override void AddFigureToGraphicsPath(GraphicsPath gp)
        {
            gp.AddEllipse(Basicrect);
        }

        public override void DrawFocusFigure(Graphics graphics, PointF offset, Marker marker)
        {
            var newrect = CalcFocusRect(offset, marker is ISizeMarker ? marker : null);
            DrawCustomFigure(graphics, newrect);
        }

        protected static void DrawCustomFigure(Graphics graphics, RectangleF rect)
        {
            // определяем "карандаш" тонкий чёрный пунктирный
            using (var pen = new Pen(Color.Black, 1f))
            {
                pen.DashStyle = DashStyle.Dash;
                graphics.DrawEllipse(pen, Rectangle.Ceiling(rect)); // рисование контура
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                using (var gp = new GraphicsPath())
                {
                    gp.AddRectangle(Basicrect);
                    BoundsRect = gp.GetBounds();
                    return base.Bounds;
                }
            }
        }

        public override void UpdateLocation(PointF offset)
        {
            // перемещение фигуры
            Basicrect = CalcFocusRect(offset, null);
            UpdateMarkers();
        }

        public override void UpdateSize(PointF offset, Marker marker)
        {
            if (!(marker is ISizeMarker)) return;
            // перемещение границ
            Basicrect = CalcFocusRect(offset, marker);
            UpdateMarkers();
        }

        public override void Offset(PointF p)
        {
            Basicrect.Offset(p);
            UpdateMarkers();
        }
    }
}
