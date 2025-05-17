using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpponentGridVisualizer : MonoBehaviour
{
    public Transform gridContainer;
    public GameObject gridSquarePrefab;
    private List<GameObject> visualGridSquares = new List<GameObject>();
    
    public int columns = 9;
    public int rows = 9;
    public float squaresGap = 0.1f;
    public Vector2 startPosition = new Vector2(-25.0f, 0.0f);
    public float squareScale = 0.5f;
    public float everySquareOffset = 0f;
    
    private Vector2 _offset = new Vector2(-25.0f, 0.0f);

    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;
    public Sprite jokerSprite;


    private void Start()
    {
        CreateVisualGrid();
    }

    public void CreateVisualGrid()
    {
        ClearVisualGrid(); 
        
        for (int i = 0; i < rows * columns; i++)
        {
            GameObject squareObject = Instantiate(gridSquarePrefab, gridContainer);
            squareObject.name = $"GridSquare_{i}";
            squareObject.SetActive(true);
            
            GridSquare gridSquare = squareObject.GetComponent<GridSquare>();
            if (gridSquare == null)
            {
                gridSquare = squareObject.AddComponent<GridSquare>();
            }
            gridSquare.SquareIndex = i;
            
            visualGridSquares.Add(squareObject);
            
            Image squareImage = squareObject.GetComponent<Image>();
            if (squareImage != null)
            {
                squareImage.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
                squareImage.raycastTarget = false;
            }
            
            if (gridSquare != null)
            {
                gridSquare.DisableInteraction();
            }
        }
        
        PositionGridSquares();
    }

    private void CreateGridSquarePrefab()
    {
        if (gridSquarePrefab != null)
        {
            return;
        }
        
        gridSquarePrefab = new GameObject("GridSquarePrefab");
        
        RectTransform rectTransform = gridSquarePrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(28, 28);
        
        Image squareImage = gridSquarePrefab.AddComponent<Image>();
        squareImage.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        
        GridSquare gridSquare = gridSquarePrefab.AddComponent<GridSquare>();
        
        ValidateAndLoadSprites();
        
        GameObject normalImageObj = new GameObject("NormalImage");
        normalImageObj.transform.SetParent(gridSquarePrefab.transform, false);
        Image normalImage = normalImageObj.AddComponent<Image>();
        normalImage.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        gridSquare.normalImage = normalImage;
        
        GameObject hoverImageObj = new GameObject("HoverImage");
        hoverImageObj.transform.SetParent(gridSquarePrefab.transform, false);
        Image hoverImage = hoverImageObj.AddComponent<Image>();
        hoverImage.color = new Color(1f, 1f, 1f, 0.3f);
        gridSquare.hooverImage = hoverImage;
        hoverImageObj.SetActive(false);
        
        GameObject activeImageObj = new GameObject("ActiveImage");
        activeImageObj.transform.SetParent(gridSquarePrefab.transform, false);
        Image activeImage = activeImageObj.AddComponent<Image>();
        activeImage.color = new Color(1f, 1f, 0f, 0.3f);
        gridSquare.activeImage = activeImage;
        activeImageObj.SetActive(false);
        
        gridSquare.colorSprites = new Sprite[4];
        
        gridSquare.colorSprites[0] = blueSprite;
        gridSquare.colorSprites[1] = greenSprite;
        gridSquare.colorSprites[2] = yellowSprite;
        gridSquare.colorSprites[3] = jokerSprite;
        
        gridSquare.normalImages = new List<Sprite>() { blueSprite ?? CreatePlaceholderSprite(Color.blue), greenSprite ?? CreatePlaceholderSprite(Color.green) };
        
        gridSquarePrefab.SetActive(false);
    }

    private void ValidateAndLoadSprites()
    {
        if (blueSprite == null)
        {
            blueSprite = CreatePlaceholderSprite(Color.blue);
        }
        
        if (greenSprite == null)
        {
            greenSprite = CreatePlaceholderSprite(Color.green);
        }
        
        if (yellowSprite == null)
        {
            yellowSprite = CreatePlaceholderSprite(Color.yellow);
        }
        
        if (jokerSprite == null)
        {
            jokerSprite = CreatePlaceholderSprite(new Color(1f, 0f, 1f));
        }
    }

    private Sprite CreatePlaceholderSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    private void PositionGridSquares()
    {
        int columnNumber = 0;
        int rowNumber = 0;
        Vector2 squareGapNumber = new Vector2(0.0f, 0.0f);
        
        if (visualGridSquares.Count == 0) return;
        
        var squareRect = visualGridSquares[0].GetComponent<RectTransform>();
        _offset.x = squareRect.rect.width * squareScale + everySquareOffset;
        _offset.y = squareRect.rect.height * squareScale + everySquareOffset;

        for (int i = 0; i < visualGridSquares.Count; i++)
        {
            GameObject square = visualGridSquares[i];
            
            if (columnNumber + 1 > columns)
            {
                squareGapNumber.x = 0;
                columnNumber = 0;
                rowNumber++;
            }
            
            var posXOffset = _offset.x * columnNumber + (squareGapNumber.x * squaresGap);
            var posYOffset = _offset.y * rowNumber + (squareGapNumber.y * squaresGap);

            if (columnNumber > 0 && columnNumber % 3 == 0)
            {
                squareGapNumber.x++;
                posXOffset += squaresGap;
            }
            
            if (rowNumber > 0 && rowNumber % 3 == 0)
            {
                squareGapNumber.y++;
                posYOffset += squaresGap;
            }
            
            square.GetComponent<RectTransform>().localScale = new Vector3(squareScale, squareScale, squareScale);
            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + posXOffset, startPosition.y - posYOffset);
            
            columnNumber++;
        }
    }

    public void ClearVisualGrid()
    {
        foreach (var square in visualGridSquares)
        {
            Destroy(square);
        }
        visualGridSquares.Clear();
    }

    public void ResetVisualGridToWhite()
    {
        foreach (var square in visualGridSquares)
        {
            if (square == null) continue;
            
            GridSquare gridSquare = square.GetComponent<GridSquare>();
            if (gridSquare != null)
            {
                gridSquare.ClearOccupied();
                
                if (gridSquare.normalImage != null)
                {
                    gridSquare.normalImage.sprite = null;
                }
            }
        }
    }

    public void UpdateVisualGrid(List<GridStateManager.GridSquareState> gridState)
    {
        if (visualGridSquares.Count != rows * columns)
        {
            CreateVisualGrid();
        }
        
        foreach (var square in visualGridSquares)
        {
            if (square == null) continue;
            
            GridSquare gridSquare = square.GetComponent<GridSquare>();
            if (gridSquare != null)
            {
                gridSquare.ClearOccupied();
            }
            else
            {
                gridSquare = square.AddComponent<GridSquare>();
                
                Image squareImage = square.GetComponent<Image>();
                if (squareImage != null)
                {
                    GameObject normalImageObj = new GameObject("NormalImage");
                    normalImageObj.transform.SetParent(square.transform, false);
                    Image normalImage = normalImageObj.AddComponent<Image>();
                    normalImage.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
                    gridSquare.normalImage = normalImage;
                    
                    gridSquare.colorSprites = new Sprite[4];
                    gridSquare.colorSprites[0] = blueSprite;
                    gridSquare.colorSprites[1] = greenSprite;
                    gridSquare.colorSprites[2] = yellowSprite;
                    gridSquare.colorSprites[3] = jokerSprite;
                    
                    gridSquare.normalImages = new List<Sprite>() { blueSprite, greenSprite };
                }
            }
        }
        
        int doluKareSayisi = 0;
        List<int> boyananKareler = new List<int>();
        
        foreach (var state in gridState)
        {
            if (state.isOccupied && state.colorIndex >= 0 && state.index >= 0 && state.index < 81)
            {
                doluKareSayisi++;
                
                if (state.index < visualGridSquares.Count)
                {
                    GameObject square = visualGridSquares[state.index];
                    if (square == null) continue;
                    
                    GridSquare gridSquare = square.GetComponent<GridSquare>();
                    
                    if (gridSquare != null)
                    {
                        try
                        {
                            Shape.ShapeColor color = (Shape.ShapeColor)state.colorIndex;
                            
                            if (gridSquare.colorSprites == null || gridSquare.colorSprites.Length <= state.colorIndex)
                            {
                                gridSquare.colorSprites = new Sprite[4];
                                
                                gridSquare.colorSprites[0] = blueSprite;
                                gridSquare.colorSprites[1] = greenSprite;
                                gridSquare.colorSprites[2] = yellowSprite;
                                gridSquare.colorSprites[3] = jokerSprite;
                            }
                            
                            gridSquare.PlaceShapeOnBoard(color);
                            
                            boyananKareler.Add(state.index);
                        }
                        catch (System.Exception ex)
                        {
                        }
                    }
                }
            }
        }
    }
}
