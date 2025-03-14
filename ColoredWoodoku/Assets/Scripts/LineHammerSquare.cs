using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class LineHammerSquare : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool isHorizontal = true;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector3 _startPosition;
    private bool isDragging = false;
    private static int hammerCost = 10;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _startPosition = _rectTransform.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Scores.Instance.HasEnoughPoints(hammerCost)) return;
        
        isDragging = true;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        _rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            _rectTransform.localPosition = _startPosition;
            return;
        }

        isDragging = false;
        _canvasGroup.blocksRaycasts = true;

        if (!Scores.Instance.HasEnoughPoints(hammerCost))
        {
            _rectTransform.localPosition = _startPosition;
            return;
        }

        // Hedef kareyi bul
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(eventData.position), Vector2.zero);
        
        if (hit.collider != null && hit.collider.CompareTag("GridSquare"))
        {
            var targetSquare = hit.collider.GetComponent<GridSquare>();
            if (targetSquare != null)
            {
                if (ClearLine(targetSquare.SquareIndex))
                {
                    Scores.Instance.SpendPoints(hammerCost);
                }
            }
        }

        _rectTransform.localPosition = _startPosition;
    }

    private bool ClearLine(int squareIndex)
    {
        var allSquares = FindObjectsOfType<GridSquare>();
        bool clearedAny = false;

        if (isHorizontal)
        {
            // Yatay satırı temizle
            int row = squareIndex / 9;
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                if (index >= 0 && index < allSquares.Length && allSquares[index].isOccupied)
                {
                    allSquares[index].ClearSquareWithHammer();
                    clearedAny = true;
                }
            }
        }
        else
        {
            // Dikey sütunu temizle
            int col = squareIndex % 9;
            for (int row = 0; row < 9; row++)
            {
                int index = row * 9 + col;
                if (index >= 0 && index < allSquares.Length && allSquares[index].isOccupied)
                {
                    allSquares[index].ClearSquareWithHammer();
                    clearedAny = true;
                }
            }
        }

        return clearedAny;
    }
} 