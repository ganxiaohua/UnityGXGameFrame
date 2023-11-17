using System;
using System.Collections.Generic;
using GameFrame;
namespace GameFrame
{
    public static class ArenaEnitiySystem
    {
        [SystemBind]
        public class ArenaEnitiyStartSystem : StartSystem<ArenaEnitiy, Type>
        {
            protected override void Start(ArenaEnitiy self, Type arena)
            {
                self.Arena = (Arena) ReferencePool.Acquire(arena);
                self.Arena.Init(self);
                self.DollForJackdollDic = new();
                self.JackdollDic = new();
                self.Dolls = new();
            }
        }

        [SystemBind]
        public class ArenaEnitiyUpdateSystem : UpdateSystem<ArenaEnitiy>
        {
            protected override void Update(ArenaEnitiy self, float elapseSeconds, float realElapseSeconds)
            {
                self.Arena.Update(elapseSeconds);
            }
        }

        [SystemBind]
        public class ArenaEnitiyClearSystem : ClearSystem<ArenaEnitiy>
        {
            protected override void Clear(ArenaEnitiy self)
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
        public static Jackdoll AddJackdoll<T>(this ArenaEnitiy self) where T : Jackdoll
        {
            Type type = typeof(T);
            var jackdollComponent = self.AddChild<JackdollEnitiy, Type, Arena>(type, self.Arena);
            self.JackdollDic.Add(type, jackdollComponent);
            return jackdollComponent.Jcakdoll;
        }

        /// <summary>
        /// 刪除一個人偶師
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void RemoveJackdoll<T>(this ArenaEnitiy self) where T : Jackdoll
        {
            Type type = typeof(T);
            if (!self.JackdollDic.TryGetValue(type, out JackdollEnitiy jackdoll))
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
        public static void ChangeJackdoll<T>(this ArenaEnitiy self, IDoll doll)where T : Jackdoll
        {
            Type jackdollType = typeof(T);
            if (self.DollForJackdollDic.TryGetValue(doll, out JackdollEnitiy jackdoll))
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
        public static T AddDoll<T>(this ArenaEnitiy self) where T : class, IDoll, new()
        {
            T doll = ReferencePool.Acquire<T>();
            self.Dolls.Add(doll);
            return doll;
        }

        /// <summary>
        /// 从舞台上删除一个人偶
        /// </summary>
        public static void RemoveDoll(this ArenaEnitiy self,IDoll doll)
        {
            if (!self.Dolls.Contains(doll))
            {
                return;
            }
            
            self.Dolls.Remove(doll);
            if (self.DollForJackdollDic.TryGetValue(doll, out JackdollEnitiy jackdoll))
            {
                self.DollForJackdollDic.Remove(doll);
                jackdoll.DollLeave(doll);
            }
            
        }
    }
}