using System;
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
        private bool _isMoving;

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
            }
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
        
        public void InitializeMove(Vector2 direction)
        {
            if (_isMoving) return;
            
            _target = default;
            _targetPosition = default;
            
            var absXBiggerThanY = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);
            
            _target = absXBiggerThanY ? 
                new Vector2Int( (int)direction.x / 2, 0) : 
                new Vector2Int(0, (int)direction.y / 2);

            _targetPosition = GetTargetPosition(_target);

            if (CheckCanMoveOnTargetDirection(_target))
                _isMoving = true;
        }
    }
}