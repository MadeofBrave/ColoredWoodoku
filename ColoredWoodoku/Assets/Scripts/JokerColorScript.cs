using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class JokerSquare : Shape
{
    public ShapeStorage shapeStorage;
    public int shapeCost = 25;
    public Shapedata jokerShapeData;

    private ShapeColor[] colorsToCycle = { ShapeColor.Blue, ShapeColor.Green, ShapeColor.Yellow }; 
    private int currentColorIndex = 0;
    private Coroutine colorCycleCoroutine; 

    public override void Awake()
    {
        base.Awake();
        
        // Image bileşenlerinin Raycast Target özelliğini aç
        var mainImage = GetComponent<UnityEngine.UI.Image>();
        if (mainImage != null)
        {
            mainImage.raycastTarget = true;
        }

        var childImages = GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var image in childImages)
        {
            image.raycastTarget = true;
        }
        
        if (jokerShapeData != null)
        {
            CreateShape(jokerShapeData);
        }
        else
        {
            EnsureShape7();
        }
        
        gameObject.SetActive(true);
    }

    public override void RequestNewShape(Shapedata shapeData)
    {
        if (jokerShapeData != null)
        {
            CreateShape(jokerShapeData);
        }
        else if (shapeStorage != null)
        {
            CreateShape(shapeStorage.shapeData[6]);
        }
        else
        {
            base.RequestNewShape(shapeData);
        }
        transform.localPosition = _startPosition;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Image bileşenlerinin Raycast Target özelliğini aç
        var mainImage = GetComponent<UnityEngine.UI.Image>();
        if (mainImage != null)
        {
            mainImage.raycastTarget = true;
        }

        var childImages = GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var image in childImages)
        {
            image.raycastTarget = true;
        }

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
        if (jokerShapeData != null)
        {
            CreateShape(jokerShapeData);
        }
        else
        {
            EnsureShape7();
        }
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

    public override void DeactivateShape()
    {
        if (isInDropArea)
        {
            return;
        }

        if (_shapeactive)
        {
            foreach (var square in _currentShape)
            {
                if (square != null)
                {
                    square.SetActive(false);
                }
            }
            _shapeactive = false;
            
            // Şekli yeniden oluştur
            if (jokerShapeData != null)
            {
                CreateShape(jokerShapeData);
            }
            else
            {
                EnsureShape7();
            }
            MoveShapetoStartPosition();
            StartColorCycle();
            gameObject.SetActive(true);
            _shapeactive = true;
        }
    }

    public override void ActivateShape()
    {
        if (!_shapeactive)
        {
            foreach (var square in _currentShape)
            {
                if (square != null)
                {
                    square.SetActive(true);
                }
            }
            _shapeactive = true;
            StartColorCycle();
        }
    }
}