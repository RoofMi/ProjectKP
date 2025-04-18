using Unity.VisualScripting;
using UnityEngine;

public class UI_MoveIn : MonoBehaviour
{
    [Header("Animation Settings")]
    public float Duration = 0.5f;
    public float Speed = 500.0f;

    public enum Direction { Left, Right, Up, Down }
    public Direction MoveFrom = Direction.Left;

    private RectTransform _rectTransform;
    private Vector2 _originalPosition;
    private Vector2 _startPosition;
    private float _elapsedTime = 0.0f;
    private bool bIsAnimating = false;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalPosition = _rectTransform.anchoredPosition;

        Vector2 offset = GetOffsetByDirection();
        _startPosition = _originalPosition + offset;

        float distance = Vector2.Distance(_startPosition, _originalPosition);
        Duration = distance / Speed;
    }

    void OnEnable()
    {
        _rectTransform.anchoredPosition = _startPosition;
        
        _elapsedTime = 0.0f;
        bIsAnimating = true;
    }

    void Update()
    {
        if (bIsAnimating)
        {
            _elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(_elapsedTime / Duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            _rectTransform.anchoredPosition = Vector2.Lerp(_startPosition, _originalPosition, t);

            if (t >= 1.0f)
            {
                bIsAnimating = false;

                _rectTransform.anchoredPosition = _originalPosition;
            } 
        }
    }

    private Vector2 GetOffsetByDirection()
    {
        Vector2 offset = Vector2.zero;
        Vector2 size = _rectTransform.rect.size;

        switch (MoveFrom)
        {
            case Direction.Left:
                offset = new Vector2(-size.x, 0f);
                break;
            case Direction.Right:
                offset = new Vector2(size.x, 0f);
                break;
            case Direction.Up:
                offset = new Vector2(0f, size.y);
                break;
            case Direction.Down:
                offset = new Vector2(0f, -size.y);
                break;
        }

        return offset;
    }
}
