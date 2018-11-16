using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SimpleVectorGraphicsEditor
{
    public partial class FillProps : Form
    {
        int _lastColorIndex;

        private Fill _fill;

        public Fill Fill { get { return (_fill); } set { _fill = value; } }

        public FillProps(Fill fill)
        {
            InitializeComponent();
            // -------------------------------------------------------------------
            cbColor.Items.Clear();
            cbColor.Items.AddRange(DrawUtils.GetAllColorNames()); // получение всех имён доступных цветов
            cbColor.Items.Add("Выбор цвета..."); // добавление пункта выбора цвета
            cbColor.Text = DrawUtils.GetColorNameFromIndex(_lastColorIndex);
            // -------------------------------------------------------------------
            _fill = (Fill)fill.Clone();
            // -------------------------------
            var index = DrawUtils.ColorToIndex(fill.Color);
            if (index < 0)
            {
                DrawUtils.AddCustomColor(fill.Color);
                cbColor.Items.Insert(cbColor.Items.Count - 1, "Мой цвет");
                index = cbColor.Items.Count - 2;
            }
            if (index >= 0) cbColor.SelectedIndex = index;
            // -------------------------------
            tbTrasparent.Value = 255 - fill.Alpha;
            lbTrasparent.Text = String.Format(CultureInfo.InvariantCulture, "{0}",
                                                (int)(tbTrasparent.Value / 255.0 * 100.0)) + @" %";
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
                {
                    g.FillRectangle(brush, colorrect);
                }
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
                        {
                            g.FillRectangle(brush, largerect);
                        }
                        using (var pen = new Pen(cb.BackColor))
                        {
                            g.DrawRectangle(pen, largerect);
                        }
                    }
                    else // отрисовка последнего пункта: Выбор цвета...
                        g.DrawString(cb.Items[e.Index].ToString(), cb.Font, textColor, largerect);
            }
            // Draw the focus rectangle if the mouse hovers over an item.
            e.DrawFocusRectangle();
        }

        private void tbTrasparent_Scroll(object sender, EventArgs e)
        {
            lbTrasparent.Text = String.Format("{0} %", (int)(tbTrasparent.Value / 255.0 * 100.0));
            _fill.Alpha = 255 - tbTrasparent.Value;
            pbPreview.Refresh();
        }

        private void pbPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var pb = (PictureBox)sender;
            var rect = new RectangleF(0, 0, pb.Width, pb.Height);
            using (var brush = new SolidBrush(Color.White))
            g.FillRectangle(_fill.UpdateBrush(brush), rect);
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
                        var selColor = dlgSelectColor.Color;
                        _fill.Color = selColor;
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
                {
                }
            }
            else
            {
                _lastColorIndex = cbox.SelectedIndex;
                cbox.Refresh();
                pbPreview.Refresh();
            }
        }

        private void cbColor_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            _fill.Color = DrawUtils.ColorFromIndex(cb.SelectedIndex);
            pbPreview.Refresh();
        }
    }
}
