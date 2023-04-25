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
        public DialogueNode GraphNode;
    }
    public class EnitiyInfos
    {
        public EnitiyNode RootNode;
        public Dictionary<int, int> FloorGrid;
        enum EnitiyType
        {
            All,
            UI,
            Ecs,
        }

        public void GetAllEnitiy()
        {
            FloorGrid = new();
            var root = GXGameFrame.Instance.MainScene;
            RootNode = new EnitiyNode();
            RootNode.entity = root;
            RootNode.Floor = 0;
            RootNode.Grid = 0;
            FloorGrid.Add(0,0);
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
                // int grid = entityNode.Grid - (entity.Children.Values.Count+entity.Components.Values.Count) / 2;
                foreach (IEntity childEntity in entity.Children.Values)
                {
                    CreateNode(entityNode, childEntity, floor+1, FloorGrid[floor]++);
                    // grid++;
                    // if (++grid == entityNode.Grid)
                    // {
                    //     ++grid;
                    // }
                }

                foreach (IEntity EntityComponent in entity.Components.Values)
                {
                    CreateNode(entityNode, EntityComponent, floor+1, FloorGrid[floor]++);
                    // grid++;
                    // if (++grid == entityNode.Grid)
                    // {
                    //     ++grid;
                    // }
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