using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class HammerSquare : Shape, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    public Shapedata hammerShapeData;

    public override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _startPosition = _rectTransform.localPosition;
    }
    private new void OnEnable()
    {
        RequestNewShape(hammerShapeData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos);
        _rectTransform.localPosition = pos;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        var currentSelectedShape = ShapeStorage.Instance.GetCurrentSelectedShape();

        if (currentSelectedShape == null)
        {
            Debug.Log("HammerSquare: Þu an seçili bir þekil yok, iþlem yapýlamaz.");
            MoveShapetoStartPosition();
            return;
        }

        List<int> occupiedIndexes = new List<int>();

        foreach (var gridSquare in FindObjectsOfType<GridSquare>())
        {
            if (gridSquare.isOccupied)
            {
                occupiedIndexes.Add(gridSquare.SquareIndex);
            }
        }

        bool shapeCanBePlaced = true;

        foreach (var squareIndex in occupiedIndexes)
        {
            var gridSquare = FindObjectsOfType<GridSquare>().FirstOrDefault(gs => gs.SquareIndex == squareIndex);
            if (gridSquare != null && gridSquare.isOccupied)
            {
                gridSquare.ClearSquareWithHammer(); 
            }
            else
            {
                shapeCanBePlaced = false;
            }
        }

        if (shapeCanBePlaced)
        {
            Debug.Log("HammerSquare: Þeklin alaný temizlendi!");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("HammerSquare: Þekil yerleþtirilemez, baþlangýç pozisyonuna dönüyor.");
            MoveShapetoStartPosition();
        }
    }

}
