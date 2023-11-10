using System;
namespace GameFrame
{
   public enum TaskState
    {
        None,
        Ing,
        Fail,
        Succ,
    } 
    public interface ITask
    {

        TaskState TaskState { set;get; }
        bool IsDone { get;}
        Action AsyncStateMoveNext { get; set; }
    }
}