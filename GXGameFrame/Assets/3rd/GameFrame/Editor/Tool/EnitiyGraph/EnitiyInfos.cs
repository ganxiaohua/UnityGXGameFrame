using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFrame.Editor
{
    public class EnitiyNode
    {
        public EnitiyNode PreNode;
        public List<EnitiyNode> NextNodes = new();
        public int Floor;
        public int Grid;
        public IEntity entity;
        public GeneralGrophNode GraphNode;
    }

    public class EnitiyInfos
    {
        public EnitiyNode RootNode;
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
            RootNode = new EnitiyNode();
            IEntity root = null;
            if (ientity == null)
            {
                root = GXGameFrame.Instance.MainScene;
            }
            else
            {
                root = ientity;
                // List<IEntity> entityList = EnitityHouse.Instance.GetEntity(ientity.GetType());
                // foreach (var item in entityList)
                // {
                //     if (item == ientity)
                //     {
                //         root = ientity
                //     }
                // }
            }

            RootNode.entity = root;
            RootNode.Floor = 0;
            RootNode.Grid = 0;
            FloorGrid.Add(0, 0);
            StructureNode(RootNode, RootNode.Floor);
        }


        public void StructureNode(EnitiyNode entityNode, int floor)
        {
            if (!FloorGrid.ContainsKey(floor))
            {
                FloorGrid[floor] = 0;
            }

            if (entityNode.entity is Entity entity)
            {
                foreach (IEntity childEntity in entity.Children.Values)
                {
                    CreateNode(entityNode, childEntity, floor + 1, FloorGrid[floor]++);
                }

                foreach (IEntity EntityComponent in entity.Components.Values)
                {
                    CreateNode(entityNode, EntityComponent, floor + 1, FloorGrid[floor]++);
                }
            }
        }

        public void CreateNode(EnitiyNode parentNode, IEntity entity, int floor, int grid)
        {
            EnitiyNode enitiy = new EnitiyNode();
            parentNode.NextNodes.Add(enitiy);
            enitiy.PreNode = parentNode;
            enitiy.Floor = floor;
            enitiy.Grid = grid;
            enitiy.entity = entity;
            StructureNode(enitiy, enitiy.Floor);
        }
    }
}