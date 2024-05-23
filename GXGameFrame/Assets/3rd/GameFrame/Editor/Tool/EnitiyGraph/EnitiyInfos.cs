using System.Collections.Generic;

namespace GameFrame.Editor
{
    public class EntityNode
    {
        public EntityNode PreNode;
        public List<EntityNode> NextNodes = new();
        public int Floor;
        public int Grid;
        public IEntity entity;
        public GeneralGrophNode GraphNode;
    }

    public class EnitiyInfos
    {
        public EntityNode RootNode;
        public Dictionary<int, int> FloorGrid;
        public bool Find = false;

        enum EnitiyType
        {
            All,
            UI,
            Ecs,
        }

        public EnitiyInfos()
        {
            FloorGrid = new();
        }

        public void GetRootEnitiy(IEntity ientity = null)
        {
            FloorGrid.Clear();
            RootNode = new EntityNode();
            IEntity root = null;
            if (ientity == null)
            {
                root = GXGameFrame.Instance.MainScene;
            }
            else
            {
                root = ientity;
            }

            RootNode.entity = root;
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
            
            if (entityNode.entity is Entity entity)
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
        }

        public void CreateNode(EntityNode parentNode, IEntity ientity, int floor, int grid)
        {
            EntityNode entity = new EntityNode();
            parentNode.NextNodes.Add(entity);
            entity.PreNode = parentNode;
            entity.Floor = floor;
            entity.Grid = grid;
            entity.entity = ientity;
            StructureNode(entity, entity.Floor);
        }
    }
}