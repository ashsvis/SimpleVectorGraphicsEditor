using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleVectorGraphicsEditor.Markers
{
    [Serializable]
    public class VertexLocationMarker : Marker
    {
        private readonly int _index;

        public VertexLocationMarker(int index)
        {
            _index = index;
        }

        public override int Index { get { return _index; } }

        public override Cursor Cursor { get { return Cursors.SizeAll; } }

        public override void UpdateLocation()
        {
            if (TargetFigure == null) return;
            var points = TargetFigure.GetPoints();
            if (_index < 0 || _index >= points.Length) return;
            Location = new Point((int)points[_index].X, (int)points[_index].Y);
        }

        public override void Draw(Graphics gr)
        {
            gr.DrawEllipse(Pens.Black, Location.X - DefSize,
                           Location.Y - DefSize, DefSize * 2, DefSize * 2);
            gr.FillEllipse(Brushes.DarkOrange, Location.X - DefSize,
                           Location.Y - DefSize, DefSize * 2, DefSize * 2);
        }
    }
}