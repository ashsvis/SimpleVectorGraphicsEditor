using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

namespace SimpleVectorGraphicsEditor
{
    public class NamedColor
    {
        public NamedColor(Color color, string colorName)
        {
            Color = color;
            Name = colorName;
        }
        
        public Color Color { get; set; }
        
        public string Name { get; set; }
    }

    public static class DrawUtils
    {
        static private readonly List<NamedColor> NamedColors = new List<NamedColor>
            {   new NamedColor(Color.Black, "Чёрный"),
            new NamedColor(Color.White, "Белый"),
            new NamedColor(Color.Red, "Красный"),
            new NamedColor(Color.Lime, "Ярко-зелёный"),
            new NamedColor(Color.Blue, "Синий"),
            new NamedColor(Color.Yellow, "Жёлтый"),
            new NamedColor(Color.Magenta, "Лиловый"),
            new NamedColor(Color.Cyan, "Бирюзовый"),
            new NamedColor(Color.Brown, "Коричневый"),
            new NamedColor(Color.Green, "Зелёный"),
            new NamedColor(Color.Navy, "Тёмно-синий"),
            new NamedColor(Color.Olive, "Коричнево-зелёный"),
            new NamedColor(Color.DarkMagenta, "Фиолетовый"),
            new NamedColor(Color.DarkCyan, "Тёмно-бирюзовый"),
            new NamedColor(Color.WhiteSmoke, "Серый 10%"),
            new NamedColor(Color.Gainsboro, "Серый 25%"),
            new NamedColor(Color.LightGray, "Серый 40%"),
            new NamedColor(Color.Silver, "Серый 50%"),
            new NamedColor(Color.DarkGray, "Серый 60%"),
            new NamedColor(Color.Gray, "Серый 75%"),
            new NamedColor(Color.DimGray, "Серый 90%")
        };
        
        static private readonly List<Color> CustomColors = new List<Color>();
        
        static readonly HatchStyle[] HatchStyleArray = (HatchStyle[])Enum.GetValues(typeof(HatchStyle));
        
        static readonly int HatchStyleCount = HatchStyleArray.Length - 3;
        
        static readonly LinearGradientMode[] LinearGradientModeArray =
            (LinearGradientMode[])Enum.GetValues(typeof(LinearGradientMode));
        
        static readonly int LinearGradientModeCount = LinearGradientModeArray.Length;
        
        static readonly DashStyle[] DashStyleArray = (DashStyle[])Enum.GetValues(typeof(DashStyle));
        
        static readonly int DashStyleCount = DashStyleArray.Length - 1;
        
        public static object[] GetPenPatternNames()
        {
            var dashNameArray = Enum.GetNames(typeof(DashStyle));
            var n = 1 + DashStyleCount;
            var names = new object[n];
            names[0] = "Нет";
            var i = 1;
            for (var j = 0; j < DashStyleCount; j++) { names[i] = dashNameArray[j]; i++; }
            return names;
        }

        public static object[] GetLineJoinNames()
        {
            var names = new object[3];
            names[0] = "Угловое";
            names[1] = "Скошенное";
            names[2] = "Круговое";
            return names;
        }

        public static object[] GetLineCapNames()
        {
            var names = new object[4];
            names[0] = "Плоское";
            names[1] = "Квадратное";
            names[2] = "Круглое";
            names[3] = "Треугольное";
            return names;
        }

        public static bool FindColor(Color color)
        {
            var found = NamedColors.Any(t => t.Color == color);
            if (!found)
                if (CustomColors.Any(t => t == color))
                    found = true;
            return (found);
        }

        public static bool IsNamedColorIndex(int index)
        {
            return ((index >= 0) && (index < NamedColors.Count));
        }

        public static bool IsCustomColorIndex(int index)
        {
            return ((index >= NamedColors.Count) && (index < NamedColors.Count + CustomColors.Count));
        }

        public static Color ColorFromIndex(int index)
        {
            var color = new Color();
            if ((index >= 0) && (index < NamedColors.Count)) color = NamedColors[index].Color;
            else
            {
                var moreIndex = index - NamedColors.Count;
                if ((moreIndex >= 0) && (moreIndex < CustomColors.Count)) color = CustomColors[moreIndex];
            }
            return color;
        }

        public static int ColorToIndex(Color color)
        {
            var index = -1;
            for (var i = 0; i < NamedColors.Count; i++)
            {
                if (NamedColors[i].Color != color) continue;
                index = i; break;
            }
            if (index < 0)
            {
                for (var i = 0; i < CustomColors.Count; i++)
                {
                    if (CustomColors[i] != color) continue;
                    index = i + NamedColors.Count; break;
                }
            }
            return index;
        }

        public static int[] GetCustomColors()
        {
            var count = CustomColors.Count;
            var argbColors = new int[count];
            for (var i = 0; i < count; i++) argbColors[i] = CustomColors[i].ToArgb();
            return argbColors;
        }

        public static void AddCustomColor(Color color)
        {
            CustomColors.Add(color);
        }

        public static object[] GetAllColorNames()
        {
            var n = NamedColors.Count + CustomColors.Count;
            var names = new object[n];
            var nc = 0;
            foreach (var t in NamedColors)
                names[nc++] = t.Name;
            for (var i = 0; i < CustomColors.Count; i++)
                names[nc++] = String.Format(CultureInfo.InvariantCulture, "Цвет {0}", i);
           return names;
        }

        public static string GetColorNameFromIndex(int index)
        {
            var colorName = "";
            if (IsNamedColorIndex(index)) colorName = NamedColors[index].Name;
            else 
                if (IsCustomColorIndex(index)) colorName =
                    String.Format(CultureInfo.InvariantCulture, "Цвет {0}", 
                                  index - NamedColors.Count);
            return colorName;
        }

        public static string[] GetAllPatternNames()
        {
            var hatchNameArray = Enum.GetNames(typeof(HatchStyle));
            var linearGradientNameArray = Enum.GetNames(typeof(LinearGradientMode));

            var n = 2 + LinearGradientModeCount + HatchStyleCount;
            var names = new string[n];
            names[0] = "Прозрачный";
            names[1] = "Сплошной";
            var i = 2;
            for (var j = 0; j < LinearGradientModeCount; j++) { names[i] = linearGradientNameArray[j]; i++; }
            for (var j = 0; j < HatchStyleCount; j++) { names[i] = hatchNameArray[j]; i++; }
            return names;
        }

        public static bool IsNonePatternIndex(int index)
        {
            return (index == 0);
        }

        public static bool IsSolidPatternIndex(int index)
        {
            return (index == 1);
        }

        public static bool IsLinearGradientPatternIndex(int index)
        {
            checked
            {
                var idx = index - 2;
                return ((idx >= 0) && (idx < LinearGradientModeCount));
            }
        }

        public static bool IsHatchPatternIndex(int index)
        {
            checked
            {
                var idx = index - 2 - LinearGradientModeCount;
                return ((idx >= 0) && (idx < HatchStyleCount));
            }
        }

        public static HatchStyle HatchStyleFromIndex(int index)
        {
            checked
            {
                return (HatchStyle)(index - 2 - LinearGradientModeCount);
            }
        }

        public static LinearGradientMode LinearGradientModeFromIndex(int index)
        {
            checked
            {
                return (LinearGradientMode)(index - 2);
            }
        }
    }
}
