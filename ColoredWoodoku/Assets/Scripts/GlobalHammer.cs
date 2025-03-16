using UnityEngine;
using UnityEngine.EventSystems;

public class ClearBoardHammer : Shape
{
    public int clearBoardCost = 20;
    protected RectTransform _rectTransform;
    protected CanvasGroup _canvasGroup;

    public override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        UpdateInteractability();
    }

    private void UpdateInteractability()
    {
        if (Scores.Instance != null)
        {
            bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(clearBoardCost);
            _canvasGroup.alpha = hasEnoughPoints ? 1f : 0.3f;
            _canvasGroup.blocksRaycasts = hasEnoughPoints;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!Scores.Instance.HasEnoughPoints(clearBoardCost))
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

        if (!Scores.Instance.HasEnoughPoints(clearBoardCost))
        {
            MoveShapetoStartPosition();
            return;
        }

        ClearEntireBoard();
        Scores.Instance.SpendPoints(clearBoardCost);
        MoveShapetoStartPosition();
    }

    private void ClearEntireBoard()
    {
        GridSquare[] gridSquares = FindObjectsOfType<GridSquare>();
        foreach (GridSquare square in gridSquares)
        {
            square.ClearOccupied();
        }

        Debug.Log("Tahta temizlendi!");
    }
}
