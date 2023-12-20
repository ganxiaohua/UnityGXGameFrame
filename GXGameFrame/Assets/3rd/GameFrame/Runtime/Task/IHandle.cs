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
    public interface IHandle
    {

        TaskState TaskState { set;get; }
        bool IsDone { get;}
        Action AsyncStateMoveNext { get; set; }
    }
}