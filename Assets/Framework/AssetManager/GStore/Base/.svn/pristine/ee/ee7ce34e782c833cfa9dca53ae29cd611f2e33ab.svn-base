#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace GStore
{
    /// <summary>
    /// 编辑器下的
    /// </summary>
    public static class EditorCoroutineRunner
    {
        private class EditorCoroutine : IEnumerator
        {
            private Stack<IEnumerator> m_ExecutionStack;

            public EditorCoroutine(IEnumerator iterator)
            {
                this.m_ExecutionStack = new Stack<IEnumerator>();
                this.m_ExecutionStack.Push(iterator);
            }

            public bool MoveNext()
            {
                if (m_ExecutionStack.Count == 0)
                {
                    return false;
                }

                IEnumerator i = this.m_ExecutionStack.Peek();

                if (i.MoveNext())
                {
                    object result = i.Current;
                    if (result != null && result is IEnumerator)
                    {
                        this.m_ExecutionStack.Push(result as IEnumerator);
                    }
                    return true;
                }
                else
                {
                    if (this.m_ExecutionStack.Count > 1)
                    {
                        this.m_ExecutionStack.Pop();
                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                throw new System.NotSupportedException("This Operation Is Not Supported.");
            }

            public void End()
            {
                m_ExecutionStack.Clear();
            }

            public object Current
            {
                get { return this.m_ExecutionStack.Peek().Current; }
            }

            public bool Find(IEnumerator iterator)
            {
                return this.m_ExecutionStack.Contains(iterator);
            }
        }

        private static List<EditorCoroutine> m_EditorCoroutineList;
        private static List<IEnumerator> m_Buffer;

        public static IEnumerator StartEditorCoroutine(IEnumerator iterator)
        {
            if (m_EditorCoroutineList == null)
            {
                m_EditorCoroutineList = new List<EditorCoroutine>();
            }
            if (m_Buffer == null)
            {
                m_Buffer = new List<IEnumerator>();
            }
            if (m_EditorCoroutineList.Count == 0)
            {
                EditorApplication.update += Update;
            }

            // add iterator to buffer first
            m_Buffer.Add(iterator);

            return iterator;
        }

        public static void StopEditorCoroutine(IEnumerator iterator)
        {
            EditorCoroutine ec = Find(iterator);
            if (ec != null)
            {
                ec.End();
            }
            else if (m_Buffer != null && m_Buffer.Count != 0 && m_Buffer.Contains(iterator))
            {
                m_Buffer.Remove(iterator);
            }
        }

        private static EditorCoroutine Find(IEnumerator iterator)
        {
            for (int i = 0; i < m_EditorCoroutineList.Count; i++)
            {
                EditorCoroutine editorCoroutine = m_EditorCoroutineList[i];
                if (editorCoroutine.Find(iterator))
                {
                    return editorCoroutine;
                }
            }
            return null;
        }

        private static void Update()
        {
            // EditorCoroutine execution may append new iterators to buffer
            // Therefore we should run EditorCoroutine first
            m_EditorCoroutineList.RemoveAll
            (
                coroutine => { return coroutine.MoveNext() == false; }
            );

            // If we have iterators in buffer
            if (m_Buffer.Count > 0)
            {
                for (int i = 0; i < m_Buffer.Count; i++)
                {
                    IEnumerator iterator = m_Buffer[i];
                    // If this iterators not exists
                    if (Find(iterator) == null)
                    {
                        // Added this as new EditorCoroutine
                        m_EditorCoroutineList.Add(new EditorCoroutine(iterator));
                    }
                }

                // Clear buffer
                m_Buffer.Clear();
            }

            // If we have no running EditorCoroutine
            // Stop calling update anymore
            if (m_EditorCoroutineList.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }
    }
}
#endif