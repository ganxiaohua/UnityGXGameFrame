using System.Collections.Generic;

namespace GameFrame
{
    public partial class FsmController
    {
        private Dictionary<string, object> blackboard = new Dictionary<string, object>(8);

        public void SetData(string key,object data)
        {
          var has =  blackboard.ContainsKey(key);
          Assert.IsTrue(!has,$"{key}已经加入黑板");
          blackboard[key] = data;
        }

        public object GetData(string key)
        {
           bool has = blackboard.TryGetValue(key, out var data);
           Assert.IsTrue(has,$"{key}没有加入黑板");
           return data;
        }

        public void RemoveData(string key)
        {
            blackboard.Remove(key);
        }

        public void ClearBlcakboard()
        {
            blackboard.Clear();
        }
    }
}