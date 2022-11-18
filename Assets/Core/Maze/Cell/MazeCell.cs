using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Maze.Cell
{
    public class MazeCell : MonoBehaviour
    {
        public List<GameObject> walls;

        private void Start()
        {
            walls = new List<GameObject>()
            {
                wallU,
                wallR,
                wallL,
                wallD
            };
        }

        public GameObject wallL;
        public GameObject wallR;
        public GameObject wallU;
        public GameObject wallD;
    }
}