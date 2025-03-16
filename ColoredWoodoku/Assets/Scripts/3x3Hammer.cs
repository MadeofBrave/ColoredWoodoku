using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Area3x3Hammer : Shape
{
    public Shapedata area3x3ShapeData;
    public int clearCost = 50;
    protected RectTransform _rectTransform;
    protected CanvasGroup _canvasGroup;

    public override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        RequestNewShape(area3x3ShapeData);
    }

    private void Update()
    {
        UpdateInteractability();
    }

    private void UpdateInteractability()
    {
        if (Scores.Instance != null)
        {
            bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(clearCost);
            _canvasGroup.alpha = hasEnoughPoints ? 1f : 0.3f;
            _canvasGroup.blocksRaycasts = hasEnoughPoints;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!Scores.Instance.HasEnoughPoints(clearCost))
            return;

        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
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
        _canvasGroup.blocksRaycasts = true;
        UpdateInteractability();

        if (!Scores.Instance.HasEnoughPoints(clearCost))
        {
            MoveShapetoStartPosition();
            return;
        }

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        ClearArea(worldPoint);
        Scores.Instance.SpendPoints(clearCost);
        MoveShapetoStartPosition();
    }

    private void ClearArea(Vector2 dropPosition)
    {
        Collider2D[] hitSquares = Physics2D.OverlapBoxAll(dropPosition, new Vector2(1, 1), 0f);
        HashSet<GridSquare> clearedSquares = new HashSet<GridSquare>();

        foreach (Collider2D hit in hitSquares)
        {
            GridSquare gridSquare = hit.GetComponent<GridSquare>();
            if (gridSquare != null && !clearedSquares.Contains(gridSquare))
            {
                gridSquare.ClearOccupied();
                clearedSquares.Add(gridSquare);
            }
        }

        Debug.Log("3x3 Alan temizlendi!");
    }
}