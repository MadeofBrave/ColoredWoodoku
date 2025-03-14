using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ColorSquare : Shape
{
    public ShapeStorage shapeStorage;
    private new float holdTime = 0f;
    private new float requiredHoldTime = 1f;
    private new bool isHolding = false;

    private new void OnEnable()
    {
        shapeColor = GameEvents.LastExplosionColor;
        GameEvents.TriggerOneByOneBlockExplosion += HandleBlockExplosion;
    }

    private void OnDisable()
    {
        GameEvents.TriggerOneByOneBlockExplosion -= HandleBlockExplosion;
        shapeColor = ShapeColor.None;
    }

    public void HandleBlockExplosion(Shape.ShapeColor lastExplosionColor)
    {
        if (lastExplosionColor == Shape.ShapeColor.None)
        {
            return;
        }
        gameObject.SetActive(true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(CheckHoldTime());
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    private System.Collections.IEnumerator CheckHoldTime()
    {
        while (true)
        {
            yield return null;
        }
    }

    private void ShowColorSelectionPanel()
    {
        GameEvents.ShowColorSelectionPanelMethod(this);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        StopAllCoroutines();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = eventData.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        GridSquare gridSquare = null;

        foreach (RaycastResult hitResult in results)
        {
            if (hitResult.gameObject.CompareTag("GridSquare"))
            {
                gridSquare = hitResult.gameObject.GetComponent<GridSquare>();
                break;
            }
        }

        if (gridSquare != null && gridSquare.isOccupied)
        {
            MoveShapetoStartPosition();
            return;
        }

        bool shapePlaced = CheckIfOneByOneBlockCanBePlaced();

        if (shapePlaced)
        {
            gameObject.SetActive(false);
            GameEvents.SetLastExplosionColorMethod(ShapeColor.None);
        }
        else
        {
            MoveShapetoStartPosition();
        }
    }
}