using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ColorSquare : Shape
{
    public ShapeStorage shapeStorage;
    private float holdTime = 0f;
    private float requiredHoldTime = 1f;
    private bool isHolding = false;

    private void OnEnable()
    {
        RequestNewShape(ShapeStorage.Instance.shapeData[6]);
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
        isHolding = true;
        holdTime = 0f;
        StartCoroutine(CheckHoldTime());
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTime = 0f;
        StopAllCoroutines();
    }

    private System.Collections.IEnumerator CheckHoldTime()
    {
        while (isHolding)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= requiredHoldTime)
            {
                ShowColorSelectionPanel();
                isHolding = false;
                break;
            }
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
        isHolding = false;
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