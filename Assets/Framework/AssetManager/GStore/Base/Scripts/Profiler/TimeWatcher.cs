using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 性能分析工具--函数计时器
/// </summary>
public class TimeWatcher
{
    private static readonly int s_MainThreadId = 0;

    private static Dictionary<int, StackInfo> s_StackInfoDict = new Dictionary<int, StackInfo>();

    private class StackInfo
    {
        public Stack<TimeWatcher> RuningStack = new Stack<TimeWatcher>();
        public List<TimeWatcher> RootStack = new List<TimeWatcher>();
    }

    public static TimeWatcher BeginTime()
    {
        return new TimeWatcher(string.Empty);
    }

    public static float StopTime(TimeWatcher watcher)
    {
        return (float)watcher.Stop();
    }

    private System.Diagnostics.Stopwatch sw;
    private string tag;
    private List<TimeWatcher> children;
    private TimeWatcher(string tag)
    {
        this.tag = tag;
        sw = new System.Diagnostics.Stopwatch();
        sw.Start();
    }
    private void AddChild(TimeWatcher child)
    {
        if (children == null)
        {
            children = new List<TimeWatcher>();
        }
        children.Add(child);
    }

    static TimeWatcher()
    {
        s_MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    public double Stop()
    {
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }

    [System.Diagnostics.Conditional("USING_TIME_WATCH")]
    public static void Clear()
    {
#if USING_TIME_WATCH
        s_StackInfoDict.Clear();
#endif
    }

    /// <summary>
    /// 开始单次的时间统计(单次栈式)
    /// </summary>
    /// <param name="_str"></param>
    [System.Diagnostics.Conditional("USING_TIME_WATCH")]
    public static void BeginStack(string tag)
    {
#if USING_TIME_WATCH
        int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        var timeWatcher = new TimeWatcher(tag);

        StackInfo stackInfo = null;

        lock (s_StackInfoDict)
        {
            if (s_StackInfoDict.TryGetValue(threadId, out stackInfo) == false)
            {
                stackInfo = new StackInfo();
                s_StackInfoDict.Add(threadId, stackInfo);
            }
        }

        if (stackInfo.RuningStack.Count > 0)
        {
            var parent = stackInfo.RuningStack.Peek();
            parent.AddChild(timeWatcher);
        }
        else
        {
            stackInfo.RootStack.Add(timeWatcher);
        }

        stackInfo.RuningStack.Push(timeWatcher);
#endif
    }

    /// <summary>
    /// 开始单次的时间统计(单次栈式)
    /// </summary>
    /// <param name="_str"></param>
    [System.Diagnostics.Conditional("USING_TIME_WATCH")]
    public static void EndStack()
    {
#if USING_TIME_WATCH
        int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        StackInfo stackInfo = null;
        if (s_StackInfoDict.TryGetValue(threadId, out stackInfo) == false)
        {
            Debug.LogError("BeginStack not match EndStack");
        }

        if (stackInfo.RuningStack.Count <= 0)
        {
            Debug.LogError("BeginStack not match EndStack");
            return;
        }
        var timeWatcher = stackInfo.RuningStack.Pop();
        timeWatcher.Stop();
#endif
    }

    private const string LINE = "--------------------Time Watcher---------------------";
    private const string FORMAT = "{0}{1}\tCalls = {2}\tTime = {3} ms";

    /// <summary>
    /// 输出栈信息
    /// </summary>
    /// <param name="collapse">折叠信息</param>
    /// <param name="printToLog">直接打印</param>
    /// <returns>栈信息</returns>
    public static string FlushStackInfo(bool collapse = true, bool printToLog = true)
    {
#if USING_TIME_WATCH
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var kvp in s_StackInfoDict)
        {
            var stackInfo = kvp.Value;

            while (stackInfo.RuningStack.Count != 0)
            {
                EndStack();
                Debug.LogError("BeginStack not match EndStack");
            }

            sb.AppendLine(LINE + "threadId=" + kvp.Key);

            ExtractStackInfo(stackInfo.RootStack, sb, collapse);

            sb.AppendLine();
        }
        Clear();

        string result = sb.ToString();
        if (printToLog)
        {
            Debug.Log(result);
        }
        return result;
#else
        return string.Empty;
#endif
    }

    /// <summary>
    /// 按照堆栈层次提取信息
    /// </summary>
    /// <param name="list">提取列表</param>
    /// <param name="outInfo">输出信息</param>
    /// <param name="collapse">折叠信息</param>
    /// <param name="depth">堆栈层数</param>
    private static void ExtractStackInfo(List<TimeWatcher> list, System.Text.StringBuilder outInfo, bool collapse = true, int depth = 0)
    {
        if (list == null || list.Count == 0)
        {
            return;
        }

        if (collapse)
        {
            Dictionary<string, List<TimeWatcher>> collapseDict = new Dictionary<string, List<TimeWatcher>>();
            Dictionary<string, List<TimeWatcher>> taggedChildrenDict = new Dictionary<string, List<TimeWatcher>>();
            foreach (var item in list)
            {
                List<TimeWatcher> watchers = null;
                if (collapseDict.TryGetValue(item.tag, out watchers) == false)
                {
                    watchers = new List<TimeWatcher>();
                    collapseDict.Add(item.tag, watchers);
                }
                watchers.Add(item);
                if (item.children != null)
                {
                    List<TimeWatcher> taggedChild = null;
                    if (taggedChildrenDict.TryGetValue(item.tag, out taggedChild) == false)
                    {
                        taggedChild = new List<TimeWatcher>();
                        taggedChildrenDict.Add(item.tag, taggedChild);
                    }
                    taggedChild.AddRange(item.children);
                }
            }

            string space = new string('\t', depth);
            foreach (var kvp in collapseDict)
            {
                double time = 0;
                foreach (var item in kvp.Value)
                {
                    time += item.sw.Elapsed.TotalMilliseconds;
                }
                string line = string.Format(FORMAT, space, kvp.Key, kvp.Value.Count, (float)time);
                outInfo.AppendLine(line);

                List<TimeWatcher> taggedChild = null;
                if (taggedChildrenDict.TryGetValue(kvp.Key, out taggedChild) == false)
                {
                    continue;
                }
                ExtractStackInfo(taggedChild, outInfo, collapse, depth + 1);
            }
        }
        else
        {
            string space = new string('\t', depth);
            foreach (var item in list)
            {
                string line = string.Format(FORMAT, space, item.tag, 1, (float)item.sw.Elapsed.TotalMilliseconds);
                outInfo.AppendLine(line);
                ExtractStackInfo(item.children, outInfo, collapse, depth + 1);
            }
        }
    }

#if UNITY_EDITOR
    [MenuItem("GStore/TimeWatcher/FlushStackInfo")]
    public static void FlushStackInfo()
    {
        FlushStackInfo(true);
    }
#endif
}
