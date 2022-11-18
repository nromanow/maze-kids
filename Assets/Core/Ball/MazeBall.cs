using System;
using System.Linq;
using Core.Maze;
using UniRx;
using UnityEngine;

namespace Core.Ball
{
    public class MazeBall : MonoBehaviour
    {
        [SerializeField] private MainMaze maze;
        
        private Vector3 _targetPosition;
        private Vector2Int _target;
        private Vector2Int _gridPosition = new Vector2Int(1, 1);
        private float _speed = 0.1f;
        private bool _isMoving = false;

        private void Start()
        {
            Observable
                .EveryFixedUpdate()
                .Where(x => _isMoving)
                .Subscribe(x => Move());
        }

        private void Move()
        {
            var position = transform.position;
            
            this.transform.position = Vector3.MoveTowards(
                position, 
                _targetPosition, 
                _speed);

            if (this.transform.position == _targetPosition)
            {
                _gridPosition += _target;
                _isMoving = false;
                
                if (!CheckIsFork())
                {
                    TryInitialContinueMove(_target);
                }
            }
        }

        private void TryInitialContinueMove(Vector2Int target)
        {
            _isMoving = false;
            InitializeMove(target);
        }

        private bool CheckIsFork()
        {
            var commonWalls = GetCountOfFork();
            var isFork = commonWalls >= 3;
            
            return isFork;
        }
        
        private Vector3 GetTargetPosition(Vector2Int direction)
        {
            var cell = GetTargetCellFromMaze(direction);
            
            return cell == null ? transform.position : cell.cellObject.transform.position;
        }

        private MainMaze.Cell GetTargetCellFromMaze(Vector2Int direction)
        {
            var offset = _gridPosition + direction;

            if ((offset.x < 1 || offset.y < 1) || (offset.x > 6 || offset.y > 4))
                return default;

            var targetCell = maze.GetCellByPosition(offset);

            return targetCell;
        }

        private int GetCountOfFork()
        {
            var commonWalls = 0;

            var myCell = GetTargetCellFromMaze(Vector2Int.zero);
            var upCell = GetTargetCellFromMaze(new Vector2Int(0, 1));
            var downCell = GetTargetCellFromMaze(new Vector2Int(0, -1));
            var leftCell = GetTargetCellFromMaze(new Vector2Int(-1, 0));
            var rightCell = GetTargetCellFromMaze(new Vector2Int(1, 0));

            if (upCell != null)
            {
                if (!myCell.mCell.wallU.activeSelf && !upCell.mCell.wallD.activeSelf)
                    commonWalls++;
            }
            
            if (downCell != null)
            {
                if (!myCell.mCell.wallD.activeSelf && !downCell.mCell.wallU.activeSelf)
                    commonWalls++;
            }
            
            if (leftCell != null)
            {
                if (!myCell.mCell.wallL.activeSelf && !leftCell.mCell.wallR.activeSelf)
                    commonWalls++;
            }
            
            if (rightCell != null)
            {
                if (!myCell.mCell.wallD.activeSelf && !rightCell.mCell.wallL.activeSelf)
                    commonWalls++;
            }

            return commonWalls;
        }
        
        private bool CheckCanMoveOnTargetDirection(Vector2Int direction)
        {
            var currentCell = maze.GetCellByPosition(_gridPosition).mCell;
            var targetCell = GetTargetCellFromMaze(direction);

            if (targetCell == null) return false;

            var targetMCell = targetCell.mCell;
            var canMove = false;
            
            if (direction.x == 0)
            {
                switch (direction.y)
                {
                    case > 0:
                        canMove = !currentCell.wallU.activeSelf && !targetMCell.wallD.activeSelf;
                        break;
                    case < 0:
                        canMove = !currentCell.wallD.activeSelf && !targetMCell.wallU.activeSelf;
                        break;
                }
            }
            else
            {
                switch (direction.x)
                {
                    case > 0:
                        canMove = !currentCell.wallR.activeSelf && !targetMCell.wallL.activeSelf;
                        break;
                    case < 0:
                        canMove = !currentCell.wallL.activeSelf && !targetMCell.wallR.activeSelf;
                        break;
                }
            }

            return canMove;
        }

        public void TryInitializeMove(Vector2 direction)
        {
            if (_isMoving) return;
            InitializeMove(direction);
        }

        private void InitializeMove(Vector2 direction)
        {
            _target = default;
            _targetPosition = default;
            
            var absXBiggerThanY = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);
            
            _target = absXBiggerThanY ? 
                new Vector2Int(Mathf.RoundToInt(direction.x), 0) : 
                new Vector2Int(0, Mathf.RoundToInt(direction.y));
            
            _targetPosition = GetTargetPosition(_target);

            if (CheckCanMoveOnTargetDirection(_target))
                _isMoving = true;
        }
    }
}