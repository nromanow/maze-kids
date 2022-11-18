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
                .Where(x => Input.GetTouch(0).phase is TouchPhase.Moved or TouchPhase.Stationary)
                .Subscribe(x => OnTouchMoveOrHold(Input.GetTouch(0).position));
        }

        private void OnTouchMoveOrHold(Vector2 position)
        {
            _secondTouchPosition = position;
            
            var distance = Vector2.Distance(_firstTouchPosition, _secondTouchPosition);

            if (distance >= 50f)
            {
                var direction = _secondTouchPosition - _firstTouchPosition;
                var clampDirection = Vector2.ClampMagnitude(direction, 1f);
                
                OnSwipeRecognized(clampDirection);
                _firstTouchPosition = _secondTouchPosition = Vector2.zero;
            }
        }

        private void OnSwipeRecognized(Vector2 clampDirection)
        {
            ball.TryInitializeMove(clampDirection);
        }

        private void OnTouchBegin(Vector2 position)
        {
            _firstTouchPosition = _secondTouchPosition = position;
        }
    }
}