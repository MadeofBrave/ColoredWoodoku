using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LineEraser : Shape
{
    private const int COST = 20;
    private Text costText;
    private Grid grid;
    private bool isDragging = false;
    protected RectTransform _rectTransform;
    protected CanvasGroup _canvasGroup;

    [SerializeField]
    private bool isHorizontal = true;

    public override void Awake()
    {
        base.Awake();
        grid = FindObjectOfType<Grid>();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        costText = GetComponentInChildren<Text>();
        
        if (costText != null)
        {
            costText.text = COST.ToString();
        }

        UpdateVisibility();
    }

    private void Update()
    {
        if (!isDragging)
        {
            UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        if (Scores.Instance != null)
        {
            bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(COST);
            _canvasGroup.alpha = hasEnoughPoints ? 1f : 0.3f;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!Scores.Instance.HasEnoughPoints(COST))
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
        if (!isDragging) return;
        isDragging = false;
        _canvasGroup.blocksRaycasts = true;
        UpdateVisibility();
        
        if (!Scores.Instance.HasEnoughPoints(COST))
        {
            MoveShapetoStartPosition();
            return;
        }

        var selectedSquares = GetSelectedSquares();
        if (selectedSquares.Count > 0)
        {
            Scores.Instance.SpendPoints(COST);
            ClearSelectedLine(selectedSquares);
        }

        MoveShapetoStartPosition();
    }

    private void MoveShapetoStartPosition()
    {
        transform.localPosition = _startPosition;
    }

    private List<GridSquare> GetSelectedSquares()
    {
        List<GridSquare> selectedSquares = new List<GridSquare>();
        foreach (var square in grid._GridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();
            if (gridSquare != null && gridSquare.Selected)
            {
                selectedSquares.Add(gridSquare);
                gridSquare.Selected = false;
            }
        }
        return selectedSquares;
    }

    private void ClearSelectedLine(List<GridSquare> selectedSquares)
    {
        if (selectedSquares.Count == 0) return;

        int firstIndex = selectedSquares[0].SquareIndex;
        int row = firstIndex / 9;
        int column = firstIndex % 9;

        if (isHorizontal)
        {
            ClearRow(row);
        }
        else
        {
            ClearColumn(column);
        }
    }

    private void ClearRow(int rowIndex)
    {
        if (grid == null || grid._GridSquares == null) return;

        for (int i = 0; i < 9; i++)
        {
            int index = rowIndex * 9 + i;
            if (index < grid._GridSquares.Count)
            {
                var gridSquare = grid._GridSquares[index].GetComponent<GridSquare>();
                if (gridSquare != null && gridSquare.isOccupied)
                {
                    gridSquare.ClearOccupied();
                }
            }
        }
    }

    private void ClearColumn(int columnIndex)
    {
        if (grid == null || grid._GridSquares == null) return;

        for (int i = 0; i < 9; i++)
        {
            int index = i * 9 + columnIndex;
            if (index < grid._GridSquares.Count)
            {
                var gridSquare = grid._GridSquares[index].GetComponent<GridSquare>();
                if (gridSquare != null && gridSquare.isOccupied)
                {
                    gridSquare.ClearOccupied();
                }
            }
        }
    }
} 