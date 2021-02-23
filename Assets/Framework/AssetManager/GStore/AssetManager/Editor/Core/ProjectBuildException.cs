using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectBuildException : Exception
{
    public ProjectBuildException(string msg) : base(msg) { }
    public ProjectBuildException(string format, params object[] args) : this(string.Format(format, args))
    {
    }
}
