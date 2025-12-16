using System.Collections.Generic;
using GameFrame.Runtime;

namespace GameFrame.Editor
{
    public struct ArrayExSimilarData
    {
        public int Count;
        public int State;
    }

    public class ArrayExSimilar
    {
        private ArrayEx<int> list;

        // private int beforeIndex = 0;
        // private List<int> similarIndex = new List<int>(128);
        private List<ArrayExSimilarData> returnData = new List<ArrayExSimilarData>();
        public string Name;

        public ArrayExSimilar(int max, string name)
        {
            list = new ArrayEx<int>(max);
            Name = name;
        }

        public void Add(int index, int value, int headFrame)
        {
            list[index] = value;
            CalculateData(headFrame);
        }

        //TODO: wiat opt...
        private List<ArrayExSimilarData> CalculateData(int headFrame)
        {
            returnData.Clear();
            int beforeState = list[headFrame];
            int count = list.Count;
            int n = 0;
            for (int i = 0; i < list.Count; i++)
            {
                n++;
                int index = (headFrame + i) % count;
                var curState = list[index];
                if (curState == 0)
                {
                    if (i != 0)
                    {
                        var curData = new ArrayExSimilarData();
                        curData.Count = n;
                        curData.State = beforeState;
                        returnData.Add(curData);
                    }

                    break;
                }

                if (curState != beforeState || i == list.Count - 1)
                {
                    var curData = new ArrayExSimilarData();
                    curData.Count = n;
                    curData.State = beforeState;
                    beforeState = curState;
                    n = 0;
                    returnData.Add(curData);
                }
            }

            return returnData;
        }

        public List<ArrayExSimilarData> GetData()
        {
            return returnData;
        }
    }
}