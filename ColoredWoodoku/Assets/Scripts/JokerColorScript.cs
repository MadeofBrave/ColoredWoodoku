using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class JokerSquare : Shape
{
    public ShapeStorage shapeStorage;
    public int shapeCost = 25;

    private ShapeColor[] colorsToCycle = { ShapeColor.Blue, ShapeColor.Green, ShapeColor.Yellow }; 
    private int currentColorIndex = 0;
    private Coroutine colorCycleCoroutine; 

    public override void Awake()
    {
        base.Awake();
        EnsureShape7();
        gameObject.SetActive(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        StartColorCycle();
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        StopColorCycle(); 
    }

    public void StartColorCycle()
    {
        if (colorCycleCoroutine != null)
            StopCoroutine(colorCycleCoroutine); 

        colorCycleCoroutine = StartCoroutine(CycleColors()); 
    }

    private void StopColorCycle()
    {
        if (colorCycleCoroutine != null)
            StopCoroutine(colorCycleCoroutine);
    }

    private IEnumerator CycleColors()
    {
        while (true)
        {
            shapeColor = colorsToCycle[currentColorIndex];
            SetColor(shapeColor);
            currentColorIndex = (currentColorIndex + 1) % colorsToCycle.Length;
            yield return new WaitForSeconds(0.3f);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        bool shapePlaced = CheckIfOneByOneBlockCanBePlaced();
        if (shapePlaced)
        {
            GameEvents.AddScoresMethod(-shapeCost);
            GameEvents.CheckIfShapeCanBePlacedMethod();
            ResetAndEnable();
        }
        else
        {
            MoveShapetoStartPosition();
        }
    }


    private void ResetAndEnable()
    {
        EnsureShape7();
        MoveShapetoStartPosition();
        StartColorCycle();
        gameObject.SetActive(true);
    }

    private void EnsureShape7()
    {
        if (shapeStorage != null)
        {
            CreateShape(shapeStorage.shapeData[6]);
        }
    }
}