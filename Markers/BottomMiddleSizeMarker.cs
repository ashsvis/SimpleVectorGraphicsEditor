﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleVectorGraphicsEditor.Markers
{
    [Serializable]
    public class BottomMiddleSizeMarker : Marker, ISizeMarker
    {
        public override int Index { get { return 6; } }

        public override Cursor Cursor { get { return Cursors.SizeNS; } }

        public override void UpdateLocation()
        {
            if (TargetFigure == null) return;
            var bounds = TargetFigure.Bounds;
            Location = new Point((int)Math.Round(bounds.Left + bounds.Width/2),
                                 (int)Math.Round(bounds.Bottom) + DefSize / 2);
        }
    }
}