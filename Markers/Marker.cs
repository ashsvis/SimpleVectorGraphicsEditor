using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleVectorGraphicsEditor.Figures;

namespace SimpleVectorGraphicsEditor.Markers
{
    /// <summary>
    /// Признак размерного маркера
    /// </summary>
    public interface ISizeMarker { }

    [Serializable]
    public abstract class Marker
    {
        protected static int DefSize = 2;

        public Figure TargetFigure;

        public abstract Cursor Cursor { get; }

        public abstract int Index { get; }

        public bool IsInsidePoint(Point p)
        {
            if (p.X < Location.X - DefSize || p.X > Location.X + DefSize)
                return false;
            if (p.Y < Location.Y - DefSize || p.Y > Location.Y + DefSize)
                return false;
            return true;
        }

        public abstract void UpdateLocation();

        public virtual PointF Location { get; set; }

        public virtual void Draw(Graphics gr)
        {
            gr.DrawRectangle(Pens.Black, Location.X - DefSize,
                Location.Y - DefSize, DefSize * 2, DefSize * 2);
            gr.FillRectangle(Brushes.Violet, Location.X - DefSize,
                Location.Y - DefSize, DefSize * 2, DefSize * 2);
        }
    }
}
