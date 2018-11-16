using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleVectorGraphicsEditor.Markers
{
    [Serializable]
    public class TopLeftSizeMarker : Marker, ISizeMarker
    {
        public override int Index { get { return 1; } }

        public override Cursor Cursor { get { return Cursors.SizeNWSE; } }

        public override void UpdateLocation()
        {
            if (TargetFigure == null) return;
            var bounds = TargetFigure.Bounds;
            Location = new Point((int)Math.Round(bounds.Left) - DefSize / 2,
                                 (int)Math.Round(bounds.Top) - DefSize / 2);
        }
    }
}