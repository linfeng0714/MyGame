using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public abstract class SubWindow
{
    public virtual void OnUpdate() { }
    public abstract void OnGUI(Rect rect);
    public abstract string GetTitle();
}
