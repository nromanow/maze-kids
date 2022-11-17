using Core.Ball;
using UniRx;
using UnityEngine;

namespace Core.Touch
{
    public class TouchRecognizer : MonoBehaviour
    {
        [SerializeField] private MazeBall ball;
        
        private Vector2 _firstTouchPosition;
        private Vector2 _secondTouchPosition;
        
        private void Start()
        {
            Observable
                .EveryUpdate()
                .Where(x => Input.touchCount > 0)
                .Where(x => Input.GetTouch(0).phase == TouchPhase.Began)
                .Subscribe(x => OnTouchBegin(Input.GetTouch(0).position));

            Observable
                .EveryUpdate()
                .Where(x => Input.touchCount > 0)
                .Where(x => Input.GetTouch(0).phase is TouchPhase.Moved)
                .Subscribe(x => OnTouchMoveOrHold(Input.GetTouch(0).position));
        }

        private void OnTouchMoveOrHold(Vector2 position)
        {
            _secondTouchPosition = position;

            var direction = _secondTouchPosition - _firstTouchPosition;
            var clampDirection = Vector2.ClampMagnitude(direction, 2f);

            if (Mathf.Abs(clampDirection.x) >= 2f || Mathf.Abs(clampDirection.y) >= 2f)
            {
                OnSwipeRecognized(clampDirection);
                _firstTouchPosition = _secondTouchPosition = Vector2.zero;
            }
        }

        private void OnSwipeRecognized(Vector2 clampDirection)
        {
            ball.InitializeMove(clampDirection);
        }

        private void OnTouchBegin(Vector2 position)
        {
            _firstTouchPosition = _secondTouchPosition = position;
        }
    }
}