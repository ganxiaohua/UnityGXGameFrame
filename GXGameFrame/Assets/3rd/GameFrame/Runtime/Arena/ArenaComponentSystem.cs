using System;
using System.Collections.Generic;
using GameFrame;
namespace GameFrame
{
    public static class ArenaComponentSystem
    {
        [SystemBind]
        public class ArenaComponentStartSystem : StartSystem<ArenaComponent, Type>
        {
            protected override void Start(ArenaComponent self, Type arena)
            {
                self.Arena = (Arena) ReferencePool.Acquire(arena);
                self.Arena.Init(self);
                self.DollForJackdollDic = new();
                self.JackdollDic = new();
                self.Dolls = new();
            }
        }

        [SystemBind]
        public class ArenaComponentUpdateSystem : UpdateSystem<ArenaComponent>
        {
            protected override void Update(ArenaComponent self, float elapseSeconds, float realElapseSeconds)
            {
                self.Arena.Update(elapseSeconds);
            }
        }

        [SystemBind]
        public class ArenaComponentClearSystem : ClearSystem<ArenaComponent>
        {
            protected override void Clear(ArenaComponent self)
            {
                ReferencePool.Release(self.Arena);
                self.JackdollDic.Clear();
                self.DollForJackdollDic.Clear();
                foreach (var doll in self.Dolls)
                {
                    ReferencePool.Release(doll);
                }
            }
        }

        /// <summary>
        /// 添加一個人偶師
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Jackdoll AddJackdoll<T>(this ArenaComponent self) where T : Jackdoll
        {
            Type type = typeof(T);
            var jackdollComponent = self.AddChild<JackdollComponent, Type, Arena>(type, self.Arena);
            self.JackdollDic.Add(type, jackdollComponent);
            return jackdollComponent.Jcakdoll;
        }

        /// <summary>
        /// 刪除一個人偶師
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void RemoveJackdoll<T>(this ArenaComponent self) where T : Jackdoll
        {
            Type type = typeof(T);
            if (!self.JackdollDic.TryGetValue(type, out JackdollComponent jackdoll))
            {
                return;
            }

            self.RemoveChild(jackdoll);
            self.JackdollDic.Remove(type);
        }

        /// <summary>
        /// 玩偶转入到其他的人偶师的操作下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="jackdollType">人偶师类型</param>
        /// <param name="doll">玩偶本身</param>
        public static void ChangeJackdoll<T>(this ArenaComponent self, IDoll doll)where T : Jackdoll
        {
            Type jackdollType = typeof(T);
            if (self.DollForJackdollDic.TryGetValue(doll, out JackdollComponent jackdoll))
            {
                self.DollForJackdollDic.Remove(doll);
                jackdoll.DollLeave(doll);
            }

            if (!self.JackdollDic.TryGetValue(jackdollType, out jackdoll))
            {
                Debugger.LogError($"不存在这个{jackdollType.GetType()}的玩偶师");
                return;
            }

            jackdoll.DollJoin(doll);
            self.DollForJackdollDic.Add(doll, jackdoll);
        }

        /// <summary>
        /// 加入一个人偶到舞台上
        /// </summary>
        public static T AddDoll<T>(this ArenaComponent self) where T : class, IDoll, new()
        {
            T doll = ReferencePool.Acquire<T>();
            self.Dolls.Add(doll);
            return doll;
        }

        /// <summary>
        /// 从舞台上删除一个人偶
        /// </summary>
        public static void RemoveDoll(this ArenaComponent self,IDoll doll)
        {
            if (!self.Dolls.Contains(doll))
            {
                return;
            }
            
            self.Dolls.Remove(doll);
            if (self.DollForJackdollDic.TryGetValue(doll, out JackdollComponent jackdoll))
            {
                self.DollForJackdollDic.Remove(doll);
                jackdoll.DollLeave(doll);
            }
            
        }
    }
}