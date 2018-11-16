using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleVectorGraphicsEditor
{
    [Serializable]
    public class StackMemory
    {
        readonly int _stackDepth; // глубина стека

        readonly List<byte[]> _list = new List<byte[]>();
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="depth">глубина стека</param>
        public StackMemory(int depth)
        {
            _stackDepth = depth;
            if (depth < 1) _stackDepth = 1;
            _list.Clear();
        }

        /// <summary>
        /// Помещаем данные в стек
        /// </summary>
        /// <param name="stream"></param>
        public void Push(MemoryStream stream)
        {
            if (_list.Count > _stackDepth) _list.RemoveAt(0);
            _list.Add(stream.ToArray());
        }

        /// <summary>
        /// Очищаем стек
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Количество сохранённых версий в стеке
        /// </summary>
        public int Count
        {
            get { return (_list.Count); }
        }

        /// <summary>
        /// Извлечение данных из стека
        /// </summary>
        /// <param name="stream"></param>
        public void Pop(MemoryStream stream)
        {
            if (_list.Count <= 0) return;
            var buff = _list[_list.Count - 1];
            stream.Write(buff, 0, buff.Length);
            _list.RemoveAt(_list.Count - 1);
        }
    }
}
