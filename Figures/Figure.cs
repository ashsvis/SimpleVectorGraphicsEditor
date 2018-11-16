using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleVectorGraphicsEditor.Markers;
using SimpleVectorGraphicsEditor.Primitives;

namespace SimpleVectorGraphicsEditor.Figures
{
    public interface IVertexSupport
    {
        IEnumerable<Marker> CreateVertexMarkers();
        void DrawVertexMarkers(Graphics graphics);
        void DrawCustomFigure(Graphics graphics, PointF[] points);
    }

    public interface IRotateSupport
    {
        void RotateAt(float angle, float cx, float cy);
    }

    public interface ISolidFigure
    {
        
    }

    /// <summary>
    /// Описание класса базовой фигуры 
    /// </summary>
    [Serializable]
    public abstract class Figure: ICloneable
    {
        // контейнер для хранения точек фигуры
        protected readonly List<PointF> Points = new List<PointF>();

        //
        protected readonly List<Marker> SizeMarkers = new List<Marker>();

        //
        protected readonly List<Marker> VertexMarkers = new List<Marker>();

        /// <summary>
        ///  Конструктор без параметра с настройкой по умолчанию
        /// </summary>
        protected Figure()
        {
            Stroke = new Stroke();
            Fill = new Fill();
        }

        /// <summary>
        ///  Конструктор без параметра с настройкой по умолчанию
        /// </summary>
// ReSharper disable UnusedParameter.Local
//        protected Figure(Point origin, Point offset) : this() { }
// ReSharper restore UnusedParameter.Local

        /// <summary>
        /// Базовая точка, обычно левый верхний угол прямоугольника,
        /// описывающего фигуру
        /// </summary>
//        public PointF Location { get; set; }

        /// <summary>
        /// Размер фигуры, ширина и высота прямоугольника,
        /// описывающего фигуру
        /// </summary>
//        public SizeF Size { get; set; }

        /// <summary>
        /// Карандаш для рисования контура фигуры
        /// </summary>
        public Stroke Stroke { get; set; }

        /// <summary>
        /// Кисть для заливки контура фигуры
        /// </summary>
        public Fill Fill { get; set; }


        /// <summary>
        /// Перемещение фигуры
        /// </summary>
        /// <param name="offset">смещение</param>
        public virtual void UpdateLocation(PointF offset)
        {
            // перемещение фигуры
            var pts = GetPoints();
            var oldrect = CalcFocusRect(PointF.Empty, null);
            var newrect = CalcFocusRect(offset, null);
            for (var i = 0; i < pts.Length; i++)
            {
                pts[i].X = newrect.Left + (pts[i].X - oldrect.Left) / oldrect.Width * newrect.Width;
                pts[i].Y = newrect.Top + (pts[i].Y - oldrect.Top) / oldrect.Height * newrect.Height;
            }
            SetPoints(pts);
            UpdateMarkers();
        }

        public void UpdateMarkers()
        {
            SizeMarkers.Clear();
            SizeMarkers.AddRange(CreateSizeMarkers());
            foreach (var marker in SizeMarkers)
                marker.UpdateLocation();
            var figure = this as IVertexSupport;
            if (figure == null) return;
            VertexMarkers.Clear();
            VertexMarkers.AddRange(figure.CreateVertexMarkers());
            foreach (var marker in VertexMarkers)
                marker.UpdateLocation();
        }

        /// <summary>
        /// Проверка попадания точки на отрезок между двумя другими точками
        /// </summary>
        /// <param name="p">текстируемая точка</param>
        /// <param name="p1">первая точка отрезка</param>
        /// <param name="p2">вторая точка отрезка</param>
        /// <returns></returns>
        public bool PointInRange(PointF p, PointF p1, PointF p2)
        {
            using (var gp = new GraphicsPath())
            {
                gp.AddLine(p1, p2);
                using (var pen = new Pen(Color.Black, Stroke.Width * 5f))
                    return gp.IsOutlineVisible(p, pen);
            }
        }

        /// <summary>
        /// Изменение внутреннего массива точек фигуры при работе с маркерами
        /// </summary>
        /// <param name="offset">смещение</param>
        /// <param name="marker">индекс маркера</param>
        public virtual void UpdateSize(PointF offset, Marker marker)
        {
            PointF[] pts;
            if (marker is ISizeMarker)
            {
                // перемещение границ
                pts = GetPoints();
                var oldrect = CalcFocusRect(PointF.Empty, marker);
                var newrect = CalcFocusRect(offset, marker);
                for (var i = 0; i < pts.Length; i++)
                {
                    pts[i].X = newrect.Left + (pts[i].X - oldrect.Left) / oldrect.Width * newrect.Width;
                    pts[i].Y = newrect.Top + (pts[i].Y - oldrect.Top) / oldrect.Height * newrect.Height;
                }
                SetPoints(pts);
                UpdateMarkers();
            }
            else
                if (marker is VertexLocationMarker)
                {
                    // перемещение узлов
                    pts = GetPoints();
                    var index = marker.Index;
                    if ((index >= 0) && (index < pts.Length))
                    {
                        pts[index].X += offset.X;
                        pts[index].Y += offset.Y;
                        SetPoints(pts);
                        UpdateMarkers();
                    }
                }
        }

        /// <summary>
        /// Метод рисования фигуры по точкам базового списка
        /// </summary>
        /// <param name="graphics">"холст" - объект для рисования</param>
        public abstract void Draw(Graphics graphics);

        protected abstract IEnumerable<Marker> CreateSizeMarkers();

        public abstract void DrawSizeMarkers(Graphics graphics);

        /// <summary>
        /// Метод проверяет принадлежность точки фигуре
        /// </summary>
        /// <param name="point">проверяемая точка</param>
        /// <returns>True - точка принадлежит фигуре</returns>
        public abstract bool PointInFigure(PointF point);

        protected RectangleF BoundsRect;

        /// <summary>
        /// Свойство возвращает реальный прямоугольник, охватывающий точки фигуры
        /// </summary>
        public virtual RectangleF Bounds
        {
            get
            {
                // если фигура очень узкая по горизонтали
                if (Math.Abs(BoundsRect.Width - 0) < Single.Epsilon)
                {
                    BoundsRect.X -= 2;
                    BoundsRect.Width += 4;
                }
                // если фигура очень узкая по вертикали
                if (Math.Abs(BoundsRect.Height - 0) < Single.Epsilon)
                {
                    BoundsRect.Y -= 2;
                    BoundsRect.Height += 4;
                }
                return BoundsRect;
            }
        }

        /// <summary>
        /// Поиск номера маркера в разных режимах редактора
        /// </summary>
        /// <param name="pt">точка "нажатия" мышки</param>
        /// <param name="nodeChanging"></param>
        /// <returns>индекс маркера</returns>
        public Marker MarkerSelected(PointF pt, bool nodeChanging)
        {
            if (nodeChanging)
            {
                // в режиме изменения узлов
                foreach (var marker in VertexMarkers)
                {
                    marker.UpdateLocation();
                    if (marker.IsInsidePoint(Point.Ceiling(pt)))
                        return marker;
                }
            }
            else
            {
                // в режиме изменения размеров или перемещения
                foreach (var marker in SizeMarkers)
                {
                    marker.UpdateLocation();
                    if (marker.IsInsidePoint(Point.Ceiling(pt)))
                        return marker;
                }
            }
            return null;
        }

        /// <summary>
        /// Метод возвращает массив точек фигуры
        /// </summary>
        /// <returns>Массив, копия списка точек фигуры</returns>
        public PointF[] GetPoints()
        {   
            // возвращает массив точек линии
            var ps = new PointF[Points.Count];
            Points.CopyTo(ps);
            return ps;
        }
        
        /// <summary>
        /// Метод восстанавливает массив точек из внешнего массива
        /// </summary>
        /// <param name="ps">внешний массив точек</param>
        public void SetPoints(IEnumerable<PointF> ps)
        {
            Points.Clear();
            Points.AddRange(ps);
        }

        /// <summary>
        /// Смещение фигуры
        /// </summary>
        /// <param name="p">величина смещения</param>
        public virtual void Offset(PointF p)
        {
            var pts = GetPoints();
            for (var i = 0; i < pts.Length; i++)
            {
                pts[i].X += p.X;
                pts[i].Y += p.Y;
            }
            SetPoints(pts);
        }

        /// <summary>
        /// Отражение сверху-вниз
        /// </summary>
        public void FlipVertical()
        {
            var rect = Bounds;
            var cx = rect.X + rect.Width * 0.5F;
            var pts = GetPoints();
            for (var i = 0; i < pts.Length; i++)
            {
                if (pts[i].X < cx)
                    pts[i].X += (cx - pts[i].X) * 2F;
                else
                    if (pts[i].X > cx)
                        pts[i].X -= (pts[i].X - cx) * 2F;
            }
            SetPoints(pts);
        }

        /// <summary>
        /// Отражение слева-направо
        /// </summary>
        public void FlipHorizontal()
        {
            var rect = Bounds;
            var cy = rect.Y + rect.Height * 0.5F;
            var pts = GetPoints();
            for (var i = 0; i < pts.Length; i++)
            {
                if (pts[i].Y < cy)
                    pts[i].Y += (cy - pts[i].Y) * 2F;
                else
                    if (pts[i].Y > cy)
                        pts[i].Y -= (pts[i].Y - cy) * 2F;
            }
            SetPoints(pts);
        }

        /// <summary>
        /// Поворот на угол
        /// </summary>
        /// <param name="angle">угол поворота</param>
        public void Rotate(float angle)
        {
            var figure = this as IRotateSupport;
            if (figure != null)
            {
                var rect = Bounds;
                var cx = rect.X + rect.Width * 0.5F;
                var cy = rect.Y + rect.Height * 0.5F;
                figure.RotateAt(angle, cx, cy);
            }
        }

        /// <summary>
        /// Метод для рисования контуров перетаскиваемых фигур
        /// </summary>
        /// <param name="graphics">канва для рисования</param>
        /// <param name="offset">смещение фигуры относительно её текущего положения</param>
        /// <param name="marker">индекс маркера</param>
        public virtual void DrawFocusFigure(Graphics graphics, PointF offset, Marker marker)
        {
            if (marker == null)
            {
                using (var gp = new GraphicsPath())
                {
                    AddFigureToGraphicsPath(gp);
                    // получаем графический путь
                    var ps = gp.PathPoints;
                    // для всех точек пути
                    for (var i = 0; i < ps.Length; i++)
                    {
                        // делаем смещение
                        ps[i].X += offset.X;
                        ps[i].Y += offset.Y;
                    }
                    var figure = this as IVertexSupport;
                    if (figure != null)
                        figure.DrawCustomFigure(graphics, ps);
                }
            }
            else
                if (marker is VertexLocationMarker)
                {
                    // тянут мышкой за маркер, изменяющий положение узла
                    using (var gp = new GraphicsPath())
                    {
                        AddFigureToGraphicsPath(gp);
                        var ps = gp.PathPoints;
                        var i = marker.Index;
                        if ((i >= 0) && (i < ps.Length))
                        {
                            ps[i].X += offset.X;
                            ps[i].Y += offset.Y;
                            var figure = this as IVertexSupport;
                            if (figure != null)
                                figure.DrawCustomFigure(graphics, ps);
                        }
                    }
                }
                else if (marker is ISizeMarker)
                {
                    // тянут за размерный маркер
                    var ps = GetPoints();
                    var oldrect = CalcFocusRect(PointF.Empty, marker);
                    var newrect = CalcFocusRect(offset, marker);
                    for (var i = 0; i < ps.Length; i++)
                    {
                        ps[i].X = newrect.Left + (ps[i].X - oldrect.Left) / oldrect.Width * newrect.Width;
                        ps[i].Y = newrect.Top + (ps[i].Y - oldrect.Top) / oldrect.Height * newrect.Height;
                    }
                    var figure = this as IVertexSupport;
                    if (figure != null)
                        figure.DrawCustomFigure(graphics, ps);
                }
        }

        protected abstract void AddFigureToGraphicsPath(GraphicsPath gp);

        /// <summary>
        /// Расчёт нового размерного прямоугольника, с учётом новой точки смещения и индекса маркера
        /// </summary>
        /// <param name="offset">смещение относительно точки нажатия</param>
        /// <param name="marker">индекс маркера</param>
        /// <returns>Новый расчётный прямоугольник</returns>
        protected RectangleF CalcFocusRect(PointF offset, Marker marker)
        {
            var rect = Bounds;
            var dx = offset.X;
            var dy = offset.Y;
            var dw = dx;
            var dh = dy;
            if (marker == null)
            {
                // перемещение фигуры
                rect.X += dx;
                rect.Y += dy;
            }
            else if (marker is TopLeftSizeMarker)
            {
                // влево-вверх
                if ((rect.Height - dh > 0) && (rect.Width - dw > 0))
                {
                    rect.Width -= dw;
                    rect.Height -= dh;
                    rect.X += dx;
                    rect.Y += dy;
                }
            }
            else if (marker is TopMiddleSizeMarker)
            {
                // вверх
                if (rect.Height - dh > 0)
                {
                    rect.Height -= dh;
                    rect.Y += dy;
                }
            }
            else if (marker is TopRightSizeMarker)
            {
                // вправо-вверх
                if ((rect.Height - dh > 0) && (rect.Width + dw > 0))
                {
                    rect.Width += dw;
                    rect.Height -= dh;
                    rect.Y += dy;
                }
            }
            else if (marker is MiddleRightSizeMarker)
            {
                // вправо
                if (rect.Width + dw > 0)
                {
                    rect.Width += dw;
                }
            }
            else if (marker is BottomRightSizeMarker)
            {
                // вправо-вниз
                if ((rect.Width + dw > 0) && (rect.Height + dh > 0))
                {
                    rect.Width += dw;
                    rect.Height += dh;
                }
            }
            else if (marker is BottomMiddleSizeMarker)
            {
                // вниз
                if (rect.Height + dh > 0)
                {
                    rect.Height += dh;
                }
            }
            else if (marker is BottomLeftSizeMarker)
            {
                // влево-вниз
                if ((rect.Height + dh > 0) && (rect.Width - dw > 0))
                {
                    rect.Width -= dw;
                    rect.Height += dh;
                    rect.X += dx;
                }
            }
            else if (marker is MiddleLeftSizeMarker)
            {
                // влево
                if (rect.Width - dw > 0)
                {
                    rect.Width -= dw;
                    rect.X += dx;
                }
            }
            return rect;
        }

        public object Clone()
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }
    }
}
