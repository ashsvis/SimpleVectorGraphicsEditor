using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SimpleVectorGraphicsEditor.Figures;
using SimpleVectorGraphicsEditor.Primitives;

namespace SimpleVectorGraphicsEditor
{
    public partial class FormMain : Form
    {
        private readonly Picture _editor;
        private Figure _focusedfigure;

        private const string Title = @"Простой векторный графический редактор (демо)";

        public FormMain()
        {
            InitializeComponent();
            // создаём хранилище созданных фигур, которое также и рисует их
            _editor = new Picture(pbCanvas)
            {
                BackgoundContextMenu = cmsBkgPopup,
                FigureContextMenu = cmsFigPopup
            };
            _editor.FigureSelected += EditorFigureSelected;
            _editor.EditorFarConnerUpdated += EditorFarConnerUpdated;
        }

        void EditorFarConnerUpdated(object sender, EventArgs e)
        {
            UpdateCanvasSize();
        }

        private void cmsFigPopup_Opening(object sender, CancelEventArgs e)
        {
            tsmiNodeSeparator.Visible = miBeginChangeNodes.Visible = _editor.CanStartNodeChanging;
            miEndChangeNodes.Visible = _editor.CanStopVertexChanging;
            miAddFigureNode.Visible = _editor.CanVertexAdding;
            miDeleteFigureNode.Visible = _editor.CanVertexDeleting;
            tsmiTransformsSeparator.Visible = tsmiTransforms.Visible = _editor.CanFigureRotate;
            miStrokeSeparator.Visible = miStroke.Visible = miFill.Visible = !_editor.CanGroupFigureOperation;
            miFill.Visible = _editor.CanFilling;
        }

        private void miBeginChangeNodes_Click(object sender, EventArgs e)
        {
            _editor.VertexChanging = true;
        }

        private void miEndChangeNodes_Click(object sender, EventArgs e)
        {
            _editor.VertexChanging = false;
        }

        private void miAddFigureNode_Click(object sender, EventArgs e)
        {
            _editor.AddNode();
        }

        private void miDeleteFigureNode_Click(object sender, EventArgs e)
        {
            _editor.RemoveNode();
        }

        private void miBringToFront_Click(object sender, EventArgs e)
        {
            _editor.BringToFront();
        }

        private void miSendToBack_Click(object sender, EventArgs e)
        {
            _editor.SendToBack();
        }

        private void miTurnLeft90_Click(object sender, EventArgs e)
        {
            _editor.TurnLeftAt90();
        }

        private void miTurnRight90_Click(object sender, EventArgs e)
        {
            _editor.TurnRightAt90();
        }

        private void miFlipVertical_Click(object sender, EventArgs e)
        {
            _editor.FlipVertical();
        }

        private void miFlipHorizontal_Click(object sender, EventArgs e)
        {
            _editor.FlipHorizontal();
        }

        private void cmsBkgPopup_Opening(object sender, CancelEventArgs e)
        {
            miPasteFromBuffer.Enabled = _editor.CanPasteFromClipboard;
        }

        private void miPasteFromBuffer_Click(object sender, EventArgs e)
        {
            _editor.PasteFromClipboardAndSelected();
        }

        private void miCutPopup_Click(object sender, EventArgs e)
        {
            _editor.CutSelectedToClipboard();
        }

        private void miCopyPopup_Click(object sender, EventArgs e)
        {
            _editor.CopySelectedToClipboard();
        }

        /// <summary>
        /// Обработчик смены выбора фигур
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditorFigureSelected(object sender, FigureSelectedEventArgs e)
        {
            var figure = _focusedfigure = e.FigureSelected;
            if (figure == null) return;
            _editor.DefaultStroke = (Stroke)figure.Stroke.Clone();
            if (figure is ISolidFigure)
                _editor.DefaultFill = (Fill)figure.Fill.Clone();
        }

        private void miStroke_Click(object sender, EventArgs e)
        {
            if (_focusedfigure == null) return;
            var frm = new StrokeProps(_focusedfigure.Stroke);
            if (frm.ShowDialog(this) != DialogResult.OK) return;
            _editor.UpdateStrokeForFocused(frm.Stroke);
            _editor.DefaultStroke = (Stroke)frm.Stroke.Clone();
        }

        private void miFill_Click(object sender, EventArgs e)
        {
            if (_focusedfigure == null) return;
            var frm = new FillProps(_focusedfigure.Fill);
            if (frm.ShowDialog(this) != DialogResult.OK) return;
            _editor.UpdateFillForFocused(frm.Fill);
            _editor.DefaultFill = (Fill)frm.Fill.Clone();
        }

        private void tsmEditMenu_DropDownOpening(object sender, EventArgs e)
        {
            tsmSelectAll.Enabled = _editor.CanSelectFigures;
            tsmCut.Enabled = tsmCopy.Enabled = _editor.CanOneFigureOperation ||
                                               _editor.CanGroupFigureOperation;
            tsmPaste.Enabled = _editor.CanPasteFromClipboard;
            tsmUndo.Enabled = _editor.CanUndoChanges;
            tsmRedo.Enabled = _editor.CanRedoChanges;
        }

        private void tsmSelectAll_Click(object sender, EventArgs e)
        {
            _editor.SelectAllFigures();
        }

        private void timerFormUpdate_Tick(object sender, EventArgs e)
        {
            tsbCut.Enabled = tsbCopy.Enabled = _editor.CanOneFigureOperation ||
                                               _editor.CanGroupFigureOperation;
            tsbPaste.Enabled = _editor.CanPasteFromClipboard;
            tsbUndo.Enabled = _editor.CanUndoChanges;
            tsbRedo.Enabled = _editor.CanRedoChanges;
            tsmSave.Enabled = tsbSave.Enabled = _editor.FileChanged;
            if (tsbArrow.Checked || _editor.EditorMode != EditorMode.Selection) return;
            foreach (ToolStripButton btn in tsFigures.Items) btn.Checked = false;
            tsbArrow.Checked = true;
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            UpdateCanvasSize();
        }

        private void UpdateCanvasSize()
        {
            var editrect = _editor.ClientRectangle;
            pbCanvas.Size = Size.Ceiling(editrect.Size);
            var rect = panelForScroll.ClientRectangle;
            if (pbCanvas.Width < rect.Width) pbCanvas.Width = rect.Width;
            if (pbCanvas.Height < rect.Height) pbCanvas.Height = rect.Height;
        }

        private void tsmUndo_Click(object sender, EventArgs e)
        {
            _editor.UndoChanges();
        }

        private void tsmRedo_Click(object sender, EventArgs e)
        {
            _editor.RedoChanges();
        }

        private void tsbSelectMode_Click(object sender, EventArgs e)
        {
            foreach (ToolStripButton btn in tsFigures.Items) btn.Checked = false;
            ((ToolStripButton)sender).Checked = true;
            if (tsbPolyline.Checked)
            {
                _editor.EditorMode = EditorMode.AddLine;                
            }
            else if (tsbPolygon.Checked)
            {
                _editor.EditorMode = EditorMode.AddPolygon;
            }
            else if (tsbRect.Checked)
            {
                _editor.EditorMode = EditorMode.AddRectangle;
            }
            else if (tsbSquare.Checked)
            {
                _editor.EditorMode = EditorMode.AddSquare;
            }
            else if (tsbEllipse.Checked)
            {
                _editor.EditorMode = EditorMode.AddEllipse;
            }
            else if (tsbCircle.Checked)
            {
                _editor.EditorMode = EditorMode.AddCircle;
            }
            else
            {
                _editor.EditorMode = EditorMode.Selection;
            }
        }

        private void tsmExit_Click(object sender, EventArgs e)
        {
            if (_editor.FileChanged &&
                MessageBox.Show(@"Есть не сохранённые данные! Закрыть программу?",
                                @"Редактор фигур",
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button3) == DialogResult.Yes)
                Close();
        }

        private void tsmSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_editor.FileName))
            {
                if (saveFiguresFileDialog.ShowDialog(this) != DialogResult.OK) return;
                _editor.SaveToFile(saveFiguresFileDialog.FileName);
                Text = Title + @" - " + _editor.FileName;
            }
            else
                _editor.SaveToFile(_editor.FileName);
        }

        private void tsmOpen_Click(object sender, EventArgs e)
        {
            if (_editor.FileChanged &&
                (!_editor.FileChanged || MessageBox.Show(@"Есть не сохранённые данные! Открыть новый файл?",
                                                         @"Редактор фигур",
                                                         MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                         MessageBoxDefaultButton.Button3) != DialogResult.Yes)) return;
            if (openFiguresFileDialog.ShowDialog(this) != DialogResult.OK) return;
            _editor.LoadFromFile(openFiguresFileDialog.FileName);
            Text = Title + @" - " + _editor.FileName;
        }

        private void tsmCreate_Click(object sender, EventArgs e)
        {
            if (_editor.FileChanged &&
                (!_editor.FileChanged || MessageBox.Show(@"Есть не сохранённые данные! Открыть новый файл?",
                                                         @"Редактор фигур",
                                                         MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                         MessageBoxDefaultButton.Button3) != DialogResult.Yes)) return;
            _editor.New();
            Text = Title;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Text = Title;
        }
    }
}
