using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class HammerSquare : Shape, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected RectTransform _rectTransform;
    protected CanvasGroup _canvasGroup;
    protected static int hammerCost = 10;
    protected float lowPointsAlpha = 0.3f;
    protected float normalAlpha = 1f;
    protected bool isDragging = false;
    protected GridSquare currentHoveredSquare;
    public Shapedata hammerShapeData;
    private Vector3 _startPosition;

    public override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _startPosition = _rectTransform.localPosition;
        UpdateHammerVisibility();
    }

    private new void OnEnable()
    {
        RequestNewShape(hammerShapeData);
        UpdateHammerVisibility();
    }

    private void Update()
    {
        if (!isDragging)
        {
            UpdateHammerVisibility();
        }
    }

    private void UpdateHammerVisibility()
    {
        if (Scores.Instance != null)
        {
            bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(hammerCost);
            _canvasGroup.alpha = hasEnoughPoints ? normalAlpha : lowPointsAlpha;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GridSquare"))
        {
            currentHoveredSquare = collision.GetComponent<GridSquare>();
            if (currentHoveredSquare.isOccupied)
            {
                currentHoveredSquare.Selected = true;
            }
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("GridSquare"))
        {
            currentHoveredSquare = collision.GetComponent<GridSquare>();
            if (currentHoveredSquare.isOccupied)
            {
                currentHoveredSquare.Selected = true;
            }
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GridSquare"))
        {
            var square = collision.GetComponent<GridSquare>();
            if (square == currentHoveredSquare)
            {
                currentHoveredSquare.Selected = false;
                currentHoveredSquare = null;
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!Scores.Instance.HasEnoughPoints(hammerCost))
        {
            return;
        }

        isDragging = true;
        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;
    }

    public override void OnDrag(PointerEventData eventData)
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

    public override void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        _canvasGroup.blocksRaycasts = true;
        UpdateHammerVisibility();

        if (!Scores.Instance.HasEnoughPoints(hammerCost))
        {
            Debug.Log("HammerSquare: Yeterli puan yok, en az " + hammerCost + " puan gerekli.");
            MoveShapetoStartPosition();
            return;
        }

        var squareList = new List<GridSquare>();
        foreach (var square in FindObjectsOfType<GridSquare>())
        {
            if (square.Selected && square.isOccupied)
            {
                squareList.Add(square);
            }
        }

        if (squareList.Count > 0)
        {
            foreach (var square in squareList)
            {
                Scores.Instance.SpendPoints(hammerCost);
                square.ClearSquareWithHammer();
                square.Selected = false;
            }
            
            MoveShapetoStartPosition();
        }
        else
        {
            MoveShapetoStartPosition();
        }
    }

}
