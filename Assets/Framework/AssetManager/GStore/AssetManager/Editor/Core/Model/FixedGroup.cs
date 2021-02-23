using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

[Serializable]
public class FixedGroup
{
    [SerializeField]
    public bool separately = false;
    [SerializeField]
    public string folder = null;

    private string[] m_Filter = new string[] { ".meta", ".cs", ".DS_Store" };
    private List<string> m_Entries;
    public List<string> Entries
    {
        get
        {
            if (Directory.Exists(folder) == false)
            {
                m_Entries.Clear();
                return m_Entries;
            }
            if (m_Entries != null)
            {
                return m_Entries;
            }

            m_Entries = new List<string>();
            EditorTools.GetFiles(folder, m_Filter, m_Entries);
            return m_Entries;
        }
    }
}
