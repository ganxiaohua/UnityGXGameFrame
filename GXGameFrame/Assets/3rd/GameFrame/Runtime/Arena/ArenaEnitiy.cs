using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class ArenaEnitiy : Entity, IStartSystem<Type>, IUpdateSystem, IClearSystem
    {
        private Arena Arena;
        private Dictionary<Type, JackdollEnitiy> JackdollDic;
        private Dictionary<IDoll, JackdollEnitiy> DollForJackdollDic;
        private List<IDoll> Dolls;

        public void Start(Type arenatype)
        {
            Arena = (Arena) ReferencePool.Acquire(arenatype);
            Arena.Init(this);
            DollForJackdollDic = new();
            JackdollDic = new();
            Dolls = new();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            Arena.Update(elapseSeconds);
        }

        public override void Clear()
        {
            base.Clear();
            ReferencePool.Release(Arena);
            JackdollDic.Clear();
            DollForJackdollDic.Clear();
            foreach (var doll in Dolls)
            {
                ReferencePool.Release(doll);
            }
        }


        /// <summary>
        /// 添加一個人偶師
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Jackdoll AddJackdoll<T>() where T : Jackdoll
        {
            Type type = typeof(T);
            var jackdollComponent = AddChild<JackdollEnitiy, Type, Arena>(type, Arena);
            JackdollDic.Add(type, jackdollComponent);
            return jackdollComponent.Jcakdoll;
        }

        /// <summary>
        /// 刪除一個人偶師
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public void RemoveJackdoll<T>() where T : Jackdoll
        {
            Type type = typeof(T);
            if (!JackdollDic.TryGetValue(type, out JackdollEnitiy jackdoll))
            {
                return;
            }

            RemoveChild(jackdoll);
            JackdollDic.Remove(type);
        }

        /// <summary>
        /// 玩偶转入到其他的人偶师的操作下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="jackdollType">人偶师类型</param>
        /// <param name="doll">玩偶本身</param>
        public void ChangeJackdoll<T>(IDoll doll) where T : Jackdoll
        {
            Type jackdollType = typeof(T);
            if (DollForJackdollDic.TryGetValue(doll, out JackdollEnitiy jackdoll))
            {
                DollForJackdollDic.Remove(doll);
                jackdoll.DollLeave(doll);
            }

            if (!JackdollDic.TryGetValue(jackdollType, out jackdoll))
            {
                Debugger.LogError($"不存在这个{jackdollType.GetType()}的玩偶师");
                return;
            }

            jackdoll.DollJoin(doll);
            DollForJackdollDic.Add(doll, jackdoll);
        }

        /// <summary>
        /// 加入一个人偶到舞台上
        /// </summary>
        public T AddDoll<T>() where T : class, IDoll, new()
        {
            T doll = ReferencePool.Acquire<T>();
            Dolls.Add(doll);
            return doll;
        }

        /// <summary>
        /// 从舞台上删除一个人偶
        /// </summary>
        public void RemoveDoll(IDoll doll)
        {
            if (!Dolls.Contains(doll))
            {
                return;
            }

            Dolls.Remove(doll);
            if (DollForJackdollDic.TryGetValue(doll, out JackdollEnitiy jackdoll))
            {
                DollForJackdollDic.Remove(doll);
                jackdoll.DollLeave(doll);
            }
        }
    }
}