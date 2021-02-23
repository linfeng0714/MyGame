using System;
using System.Collections;
using System.Collections.Generic;

namespace GStore
{
    /// <summary>
    /// 加入HashSet管理的Stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashStack<T> : IEnumerable, IEnumerable<T>
    {
        private HashSet<T> m_HashSet = new HashSet<T>();
        private Stack<T> m_Stack = new Stack<T>();

        public int Count
        {
            get
            {
                return m_Stack.Count;
            }
        }

        public void CopyTo(T[] array, int index)
        {
            m_Stack.CopyTo(array, index);
        }

        public bool Contains(T t)
        {
            return m_HashSet.Contains(t);
        }

        public T Pop()
        {
            T t = m_Stack.Pop();
            m_HashSet.Remove(t);
            return t;
        }

        public T Peek()
        {
            T t = m_Stack.Peek();
            return t;
        }

        public void Push(T t)
        {
            m_HashSet.Add(t);
            m_Stack.Push(t);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_Stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Stack.GetEnumerator();
        }
    }
}
