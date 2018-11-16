using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleVectorGraphicsEditor.Primitives
{
    /// <summary>
    /// Описание класса хранения свойств для рисования контура фигуры
    /// </summary>
    [Serializable]
    public class Stroke: ICloneable
    {
        /// <summary>
        /// Конструктор без параметров, со свойствами по умолчанию
        /// </summary>
        public Stroke()
        {
            Color = Color.Black;
            Width = 1f;
            Alpha = 255;
        }

        /// <summary>
        /// Цвет линии фигуры
        /// </summary>
        public Color Color { get; set; }
        
        /// <summary>
        /// Ширина линии фигуры
        /// </summary>
        public float Width { get; set; }
        
        /// <summary>
        /// Стиль линии фигуры
        /// </summary>
        public DashStyle DashStyle { get; set; }
        
        /// <summary>
        /// Яркость:
        /// 0 - полностью прозрачный,
        /// 255 - полноцветный
        /// </summary>
        public int Alpha { get; set; }
        
        /// <summary>
        /// Стиль начала линии
        /// </summary>
        public LineCap StartCap { get; set; }
        
        /// <summary>
        /// Стиль конца линии
        /// </summary>
        public LineCap EndCap { get; set; }
        
        /// <summary>
        /// Стиль соединения двух отрезков линии
        /// </summary>
        public LineJoin LineJoin { get; set; }

        /// <summary>
        /// Свойство возвращает "карандаш", настроенный по текущим свойствам stroke
        /// </summary>
        public Pen UpdatePen(Pen pen)
        {
            if (pen == null)
                throw new ArgumentNullException();
            pen.Color = Color.FromArgb(Alpha, Color);
            pen.Width = Width;
            pen.DashStyle = DashStyle;
            pen.StartCap = StartCap;
            pen.EndCap = EndCap;
            pen.LineJoin = LineJoin;
            return pen;
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
