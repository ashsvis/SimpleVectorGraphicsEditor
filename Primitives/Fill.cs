using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleVectorGraphicsEditor
{
    /// <summary>
    /// Описание класса хранения свойств для закрашивания поверхности фигуры
    /// </summary>
    [Serializable]
    public class Fill : ICloneable
    {
       /// <summary>
        /// Конструктор без параметров, с цветом по умолчанию
        /// </summary>
        public Fill()
        {
            Color = Color.White;
            Alpha = 255;
        }

        /// <summary>
        /// Цвет заливки
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Яркость:
        /// 0 - полностью прозрачный,
        /// 255 - полноцветный
        /// </summary>
        public int Alpha { get; set; }

        /// <summary>
        /// Свойство возвращает "кисть", настроенный по текущим свойствам fill
        /// </summary>
        /// <returns>Настроенная кисть для заполнения контура фигуры</returns>
        public Brush UpdateBrush(Brush brush)
        {
            if (brush == null)
                throw new ArgumentNullException();
            var solidBrush = brush as SolidBrush;
            if (solidBrush != null)
                solidBrush.Color = Color.FromArgb(Alpha, Color);
            return brush; 
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
