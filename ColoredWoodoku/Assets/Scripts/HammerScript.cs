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
    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        GridSquare targetSquare = FindObjectsOfType<GridSquare>().FirstOrDefault(gs => gs.isOccupied);

        if (targetSquare != null)
        {
            GameEvents.UseHammerMethod(targetSquare.SquareIndex);
            gameObject.SetActive(false);
            Debug.Log("HammerSquare: Ýlk bulunan dolu kare temizlendi.");
        }
        else
        {
            Debug.Log("HammerSquare: Geçerli bir kare bulunamadý, baþlangýç pozisyonuna dönüyor.");
            MoveShapetoStartPosition();
        }
    }

}
