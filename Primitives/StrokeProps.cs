using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace SimpleVectorGraphicsEditor.Primitives
{
    public partial class StrokeProps : Form
    {
        private int _lastColorIndex;
        private Stroke _stroke;

        public Stroke Stroke { get { return (_stroke); } set { _stroke = value; } }

        public StrokeProps(Stroke stroke)
        {
            InitializeComponent();
            // -------------------------------------------------------------------
            cbPattern.Items.Clear();
            cbPattern.Items.AddRange(DrawUtils.GetPenPatternNames()); // получение всех имён доступных типов линий
            cbPattern.SelectedIndex = 1;
            // -------------------------------------------------------------------
            cbWidth.Items.Clear();
            for (var i = 1; i < 61; i++) cbWidth.Items.Add(i.ToString("0"));
            // -------------------------------------------------------------------
            cbColor.Items.Clear();
            cbColor.Items.AddRange(DrawUtils.GetAllColorNames()); // получение всех имён доступных цветов
            cbColor.Items.Add("Выбор цвета..."); // добавление пункта выбора цвета
            cbColor.Text = DrawUtils.GetColorNameFromIndex(_lastColorIndex);
            // -------------------------------------------------------------------
            _stroke = (Stroke)stroke.Clone();
            // -------------------------------
            var index = DrawUtils.ColorToIndex(_stroke.Color);
            if (index < 0)
            {
                DrawUtils.AddCustomColor(_stroke.Color);
                cbColor.Items.Insert(cbColor.Items.Count - 1, "Мой цвет");
                index = cbColor.Items.Count - 2;
            }
            if (index >= 0) cbColor.SelectedIndex = index;
            // -------------------------------
            tbTrasparent.Value = 255 - _stroke.Alpha;
            lbTrasparent.Text = String.Format("{0} %", (int)(tbTrasparent.Value / 255.0 * 100.0));
            // -------------------------------
            cbWidth.SelectedIndex = (int)_stroke.Width - 1;
            // -------------------------------
            if (_stroke.DashStyle == DashStyle.Custom)
                cbPattern.SelectedIndex = 0;
            else
                cbPattern.SelectedIndex = (int)_stroke.DashStyle + 1;
            // -------------------------------------------------------------------
            cbLineJoin.Items.Clear();
            // получение всех имён доступных типов соединений линий
            cbLineJoin.Items.AddRange(DrawUtils.GetLineJoinNames());
            cbLineJoin.SelectedIndex = (int)_stroke.LineJoin;
        }

        private void cbPattern_DrawItem(object sender, DrawItemEventArgs e)
        {
            var cb = (ComboBox)sender;
            var g = e.Graphics;
            // Draw the background of the item.
            e.DrawBackground();
            var rect = new Rectangle(e.Bounds.X, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
            try
            {
                rect.Inflate(-4, 0);
                if (e.Index == 0)
                    ShowItemText(e, cb, g, rect);
                else
                {
                    using (var p = new Pen(e.ForeColor))
                    {
                        p.Width = 2;
                        p.DashStyle = (DashStyle)(e.Index - 1);
                        g.DrawLine(p, new Point(rect.Left, rect.Top + rect.Height / 2),
                                      new Point(rect.Right, rect.Top + rect.Height / 2));
                    }
                }
            }
            catch { }
            // Draw the focus rectangle if the mouse hovers over an item.
            e.DrawFocusRectangle();

        }

        private static void ShowItemText(DrawItemEventArgs e, ComboBox cb, Graphics g, Rectangle largerect)
        {
            using (var textColor = new SolidBrush(e.ForeColor))
            {
                string showing = cb.Items[e.Index].ToString();
                g.DrawString(showing, cb.Font, textColor, largerect);
            }
        }

        private void cbWidth_DrawItem(object sender, DrawItemEventArgs e)
        {
            var cb = (ComboBox)sender;
            var g = e.Graphics;
            // Draw the background of the item.
            e.DrawBackground();
            var rect = new Rectangle(e.Bounds.X, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
            try
            {
                rect.Inflate(-4, 0);
                using (var p = new Pen(e.ForeColor))
                {
                    p.Width = e.Index + 1;
                    g.DrawLine(p, new Point(rect.Left, rect.Top + rect.Height / 2),
                                  new Point(rect.Right, rect.Top + rect.Height / 2));
                    if (e.Index >= 9)
                    {
                        using (var textColor = new SolidBrush(e.BackColor))
                        {
                            rect.Offset(0, 2);
                            string showing = String.Format("{0} точек", cb.Items[e.Index]);
                            g.DrawString(showing, cb.Font, textColor, rect);
                        }
                    }
                }
            }
            catch { }
            // Draw the focus rectangle if the mouse hovers over an item.
            e.DrawFocusRectangle();
        }

        private void cbWidth_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            var cb = (ComboBox)sender;
            if (e.Index < cb.Height - 8)
                e.ItemHeight = cb.Height;
            else
                e.ItemHeight = e.Index + 8;
        }

        private void cbColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            var cb = (ComboBox)sender;
            var g = e.Graphics;
            var brushColor = DrawUtils.ColorFromIndex(e.Index);
            // Draw the background of the item.
            e.DrawBackground();
            var largerect = new Rectangle(e.Bounds.X, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
            var colorrect = new Rectangle(4, e.Bounds.Top + 2, e.Bounds.Height - 2, e.Bounds.Height - 5);
            if (DrawUtils.IsNamedColorIndex(e.Index)) // отрисовка рамки цвета пунктов основеых цветов
            {
                using (var brush = new SolidBrush(brushColor))
                    g.FillRectangle(brush, colorrect);
                g.DrawRectangle(Pens.Black, colorrect);
            }
            var textRect = new RectangleF(e.Bounds.X + colorrect.Width + 5, e.Bounds.Y + 1,
                                                 e.Bounds.Width, e.Bounds.Height);
            using (var textColor = new SolidBrush(e.ForeColor))
            {
                if (DrawUtils.IsNamedColorIndex(e.Index))
                {// отрисовка пунктов основных цветов
                    g.DrawString(DrawUtils.GetColorNameFromIndex(e.Index), cb.Font, textColor, textRect);
                }
                else
                    if (DrawUtils.IsCustomColorIndex(e.Index)) // отрисовка пунктов дополнительных цветов
                    {
                        using (var brush = new SolidBrush(brushColor))
                            g.FillRectangle(brush, largerect);
                        using (var pen = new Pen(cb.BackColor))
                            g.DrawRectangle(pen, largerect);
                    }
                    else // отрисовка последнего пункта: Выбор цвета...
                        g.DrawString(cb.Items[e.Index].ToString(), cb.Font, textColor, largerect);
            }
            // Draw the focus rectangle if the mouse hovers over an item.
            e.DrawFocusRectangle();
        }

        private void cbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cbox = (ComboBox)sender;
            if (cbox.SelectedIndex == cbox.Items.Count - 1)
            {
                try
                {
                    dlgSelectColor.Color = DrawUtils.ColorFromIndex(_lastColorIndex);
                    var selIndex = _lastColorIndex;
                    if (dlgSelectColor.ShowDialog() == DialogResult.OK)
                    {
                        Color selColor = dlgSelectColor.Color;
                        _stroke.Color = selColor;
                        if (!DrawUtils.FindColor(selColor))
                        {
                            DrawUtils.AddCustomColor(selColor);
                            dlgSelectColor.CustomColors = DrawUtils.GetCustomColors();
                            cbColor.Items.Insert(cbColor.Items.Count - 1, "Мой цвет");
                            cbColor.SelectedIndex = cbColor.Items.Count - 2;
                        }
                        else
                            cbox.SelectedIndex = DrawUtils.ColorToIndex(selColor);
                    }
                    else
                        cbox.SelectedIndex = selIndex;
                }
                catch
                { }
            }
            else
            {
                _lastColorIndex = cbox.SelectedIndex;
                cbox.Refresh();
                pbPreview.Refresh();
            }

        }

        private void tbTrasparent_Scroll(object sender, EventArgs e)
        {
            lbTrasparent.Text = String.Format(CultureInfo.InvariantCulture, "{0}", 
                (int)(tbTrasparent.Value / 255.0 * 100.0)) + @" %";
            _stroke.Alpha = 255 - tbTrasparent.Value;
            pbPreview.Refresh();
        }

        private void pbPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            var pb = (PictureBox)sender;
            var rect = new RectangleF(0, 0, pb.Width, pb.Height);
            var ps = new PointF[3];
            ps[0] = new PointF(rect.Left + rect.Width / 2, rect.Top + rect.Height / 8);
            ps[1] = new PointF(rect.Left + rect.Width / 4, rect.Top + 7 * rect.Height / 8);
            ps[2] = new PointF(rect.Right - rect.Width / 8, rect.Bottom - rect.Height / 8);
            using (var pen = new Pen(Color.Black))
                g.DrawLines(_stroke.UpdatePen(pen), ps);
        }

        private void cbPattern_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            if (cb.SelectedIndex == 0) _stroke.DashStyle = DashStyle.Custom;
            else _stroke.DashStyle = (DashStyle)(cb.SelectedIndex - 1);
            pbPreview.Refresh();
        }

        private void cbWidth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            _stroke.Width = cb.SelectedIndex + 1;
            pbPreview.Refresh();
        }

        private void cbColor_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            _stroke.Color = DrawUtils.ColorFromIndex(cb.SelectedIndex);
            pbPreview.Refresh();
        }

        private void cbLineJoin_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            _stroke.LineJoin = (LineJoin)(cb.SelectedIndex);
            pbPreview.Refresh();
        }
    }
}
