using System.Collections.Generic;

namespace GameFrame
{
    public class World : Entity, IInitializeSystem, IUpdateSystem
    {
        private Dictionary<Matcher, Group> groups = new();
        private List<Group>[] groupsList;
        public float DeltaTime { get; private set; }
        public float Multiple { get; private set; }

        public virtual void Initialize()
        {
            SetMultiple(1);
            groupsList = new List<Group>[GXComponents.ComponentTypes.Length];
        }

        protected virtual void SetMultiple(float mul)
        {
            Multiple = mul;
        }


        public ECSEntity AddChild()
        {
            ECSEntity ecsEntity = AddChild<ECSEntity>();
            ecsEntity.SetContext(this);
            return ecsEntity;
        }

        public void RemoveChild(ECSEntity ecsEntity)
        {
            base.RemoveChild(ecsEntity);
        }

        public void Reactive(List<int> indexs, ECSEntity ecsEntity, ushort changeType)
        {
            int count = indexs.Count;
            for (int i = 0; i < count; i++)
            {
                Reactive(indexs[i], ecsEntity, changeType);
            }
        }

        public Group GetGroup(Matcher matcher)
        {
            if (groups.TryGetValue(matcher, out Group grop)) return grop;
            grop = Group.CreateGroup(matcher);
            foreach (var item in Children)
            {
                grop.HandleEntitySilently((ECSEntity) item, EcsChangeEventState.AddType);
            }

            groups.Add(matcher, grop);
            foreach (var cid in matcher.Indices)
            {
                groupsList[cid] ??= new List<Group>(128);
                groupsList[cid].Add(grop);
            }

            return grop;
        }

        public void Reactive(int comid, ECSEntity entity, ushort changeType)
        {
            var groupList = groupsList[comid];
            if (groupList != null)
            {
                int count = groupList.Count;
                for (int i = 0; i < count; i++)
                {
                    groupList[i].HandleEntity(entity, changeType);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var group in groups)
            {
                Matcher.RemoveMatcher(group.Key);
                Group.RemoveGroup(group.Value);
            }

            groupsList = null;
            groups.Clear();
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            DeltaTime = elapseSeconds * Multiple;
        }
    }
}