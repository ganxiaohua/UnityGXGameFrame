using System.Collections.Generic;
using GameFrame.Runtime;

namespace GameFrame.Editor
{
    public class EntityNode
    {
        public EntityNode PreNode;
        public List<EntityNode> NextNodes = new();
        public int Floor;
        public int Grid;
        public IEntity Entity;
        public World World;
        public EffEntity EffEntity;
        public bool IsEffEntity => EffEntity.IsValid;
        public bool HasTarget => IsEffEntity ? World != null && World.IsAlive(EffEntity) : Entity != null;
        public string Name => IsEffEntity ? (HasTarget ? World.GetName(EffEntity) : null) : Entity?.Name;
        public string TypeName => IsEffEntity ? nameof(Runtime.EffEntity) : Entity?.GetType().Name;

        public GeneralGrophNode GraphNode;
    }

    public class EntityInfos
    {
        public EntityNode RootNode;
        public Dictionary<int, int> FloorGrid;
        public bool Find = false;

        enum EntityType
        {
            All,
            UI,
            Ecs,
        }

        public EntityInfos()
        {
            FloorGrid = new();
        }

        public void GetRootEntity(IEntity ientity = null)
        {
            FloorGrid.Clear();
            RootNode = new EntityNode();
            IEntity root = null;
            if (ientity == null)
            {
                root = GXGameFrame.Instance.RootEntity;
            }
            else
            {
                root = ientity;
            }

            RootNode.Entity = root;
            RootNode.Floor = 0;
            RootNode.Grid = 0;
            FloorGrid.Add(0, 0);
            StructureNode(RootNode, RootNode.Floor);
        }


        public void StructureNode(EntityNode entityNode, int floor)
        {
            if (!FloorGrid.ContainsKey(floor))
            {
                FloorGrid[floor] = 0;
            }

            if (entityNode.Entity is Entity entity)
            {
                foreach (IEntity childEntity in entity.Children)
                {
                    CreateNode(entityNode, childEntity, floor + 1, FloorGrid[floor]++);
                }

                foreach (IEntity EntityComponent in entity.Components.Values)
                {
                    CreateNode(entityNode, EntityComponent, floor + 1, FloorGrid[floor]++);
                }
            }

            if (entityNode.Entity is World world)
            {
                foreach (var childEntity in world.Children)
                {
                    entityNode.NextNodes.Add(new EntityNode
                    {
                        PreNode = entityNode, World = world, EffEntity = childEntity,
                        Floor = floor + 1, Grid = FloorGrid[floor]++
                    });
                }
            }
        }

        public void CreateNode(EntityNode parentNode, IEntity ientity, int floor, int grid)
        {
            EntityNode entity = new EntityNode();
            parentNode.NextNodes.Add(entity);
            entity.PreNode = parentNode;
            entity.Floor = floor;
            entity.Grid = grid;
            entity.Entity = ientity;
            StructureNode(entity, entity.Floor);
        }
    }
}
