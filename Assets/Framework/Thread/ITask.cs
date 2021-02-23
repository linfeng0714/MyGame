using System;

public interface ITaskResult
{
    int TaskId { get; }
    object Data { get; }
    bool IsCompleted { get; }
    bool Wait();
    bool Wait(int millisecondsTimeout);
    bool Wait(TimeSpan timeout);
}

public interface ITaskResult<T> : ITaskResult
{
    new T Data { get; }
}

public interface ITask
{
    int Id { get; }

    object Data { get; }

    ITaskResult Result { get; }

    void Execute();

    /// <summary>
    /// 异常回调函数（此函数在子线程中被调用,故别在此函数中调用unity api）
    /// </summary>
    /// <param name="ex"></param>
    void OnException(Exception ex);
}

public interface ITask<T> : ITask
{
    new T Data { get; }

    new ITaskResult<T> Result { get; }
}

