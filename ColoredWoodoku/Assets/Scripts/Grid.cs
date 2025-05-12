using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Shape;
using static GridStateManager;
using Unity.Netcode;

public class Grid : MonoBehaviour
{
    public ShapeStorage shapeStorage;
    public int columns = 9;
    public int rows = 9;
    public float squaresGap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startPosition = new Vector2(0.0f, 0.0f);
    public float squareScale = 0.5f;
    public float everySquareOffset = 0f;
    private Vector2 _offset = new Vector2(0.0f, 0.0f);
    public List<GameObject> _GridSquares = new List<GameObject>();
    private LineIndicator _LineIndicator;
    public int[,] line_data = new int[9, 9];
    public bool SquareOccupied { get; private set; } = false;
    public Shape.ShapeColor SquareColor { get; private set; } = Shape.ShapeColor.None;
    public static Grid Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
       
    }

    public void SetSquareOccupied(bool occupied, Shape.ShapeColor color)
    {
        SquareOccupied = occupied;
        SquareColor = color;
    }

    public void ClearSquare()
    {
        SquareOccupied = false;
        SquareColor = Shape.ShapeColor.None;
    }

    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
        GameEvents.UseHammer += HandleHammerUsage;
        GameEvents.RequestNewShape += OnRequestNewShape;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
        GameEvents.UseHammer -= HandleHammerUsage;
        GameEvents.RequestNewShape -= OnRequestNewShape;
    }

    private void HandleHammerUsage(int squareIndex)
    {
        _GridSquares[squareIndex].GetComponent<GridSquare>().ClearSquareWithHammer();
    }

    void Start()
    {
        _LineIndicator = GetComponent<LineIndicator>();
        CreateGrid();
    }

    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
    }

    private void SpawnGridSquares()
    {
        int squareIndex = 0;
        for (var row = 0; row < rows; ++row)
        {
            for (var column = 0; column < columns; ++column)
            {
                _GridSquares.Add(Instantiate(gridSquare) as GameObject);
                var gridSquareComponent = _GridSquares[_GridSquares.Count - 1].GetComponent<GridSquare>();
                gridSquareComponent.SquareIndex = squareIndex;
                _GridSquares[_GridSquares.Count - 1].transform.SetParent(this.transform);
                _GridSquares[_GridSquares.Count - 1].transform.localScale = new Vector3(squareScale, squareScale, squareScale);
                gridSquareComponent.SetImage(_LineIndicator.GetGridSquareIndex(squareIndex) % 2 == 0);
                squareIndex++;
            }
        }
    }

    private void SetGridSquaresPositions()
    {
        int columnNumber = 0;
        int rowNumber = 0;
        Vector2 squareGapNumber = new Vector2(0.0f, 0.0f);
        var squareRect = _GridSquares[0].GetComponent<RectTransform>();
        _offset.x = squareRect.rect.width * squareRect.transform.localScale.x + everySquareOffset;
        _offset.y = squareRect.rect.height * squareRect.transform.localScale.y + everySquareOffset;

        foreach (GameObject square in _GridSquares)
        {
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
            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + posXOffset, startPosition.y - posYOffset);
            square.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x + posXOffset, startPosition.y - posYOffset, 0.0f);
            columnNumber++;
        }
    }
    private void CheckIfShapeCanBePlaced()
    {
        var squareIndexes = new List<int>();
        foreach (var square in _GridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if (gridSquare.Selected && !gridSquare.SquareOccupied)
            {
                squareIndexes.Add(gridSquare.SquareIndex);
                gridSquare.Selected = false;
            }
        }
        var currentSelectedShape = shapeStorage.GetCurrentSelectedShape();
        if (currentSelectedShape == null)
        {
            return;
        }

        string shapeName = currentSelectedShape.gameObject.name;
        ShapeColor shapeColor = currentSelectedShape.shapeColor;
        

        if (currentSelectedShape.TotalSquareNumber != squareIndexes.Count)
        {
            currentSelectedShape.MoveShapetoStartPosition();
            return;
        }

        bool canPlaceShape = squareIndexes.All(index => _GridSquares[index].GetComponent<GridSquare>().CanWeUseTheSquare());

        if (!canPlaceShape)
        {
            currentSelectedShape.MoveShapetoStartPosition();
            return;
        }

        PlaceShapeOnGrid(currentSelectedShape, squareIndexes, shapeColor);

        bool anyShapeLeft = shapeStorage.ShapeList.Any(shape => 
            shape.gameObject.activeSelf && 
            !shape.isInDropArea &&
            shape.IsonStartPosition() && 
            shape.IsAnyOfShapeSquareActive());

        if (!anyShapeLeft)
        {
            SendGridStateToServer();
            if (GameNetworkManager.Instance != null)
            {
                GameNetworkManager.Instance.LocalPlayerFinishedPlacingShapes();
            }
            else
            {
                GameEvents.RequestNewShapeMethod();
            }
        }
        else
        {
            GameEvents.SetShapeInactiveMethod();
        }

        CheckIfAnyLineIsCompleted();
        
        StartCoroutine(CheckPlayerLostAfterDelay());
    }
    
    private void PlaceShapeOnGrid(Shape shape, List<int> squareIndexes, ShapeColor color)
    {
        bool isJoker = shape is JokerSquare;
        
        foreach (var squareIndex in squareIndexes)
        {
            _GridSquares[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard(color, isJoker);
        }
        
        shape._shapeactive = false;
    }

    private int[] GetVerticalLine(int colIndex)
    {
        int[] verticalLine = new int[9];
        for (int row = 0; row < 9; row++)
        {
            verticalLine[row] = row * 9 + colIndex;
        }
        return verticalLine;
    }

    private int[] GetSquareLine(int squareIndex)
    {
        int[] squareLine = new int[9];
        int rowOffset = (squareIndex / 3) * 3;
        int colOffset = (squareIndex % 3) * 3;

        int index = 0;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                squareLine[index] = (rowOffset + row) * 9 + (colOffset + col);
                index++;
            }
        }
        return squareLine;
    }

    public void CheckIfAnyLineIsCompleted()
    {
        List<int[]> lines = new List<int[]>();

        for (int row = 0; row < 9; row++)
        {
            int[] horizontalLine = GetHorizontalLine(row);
            lines.Add(horizontalLine);
        }

        for (int col = 0; col < 9; col++)
        {
            int[] verticalLine = GetVerticalLine(col);
            lines.Add(verticalLine);
        }

        for (int square = 0; square < 9; square++)
        {
            int[] squareLine = GetSquareLine(square);
            lines.Add(squareLine);
        }

        int completedLines = CheckIfSquaresAreCompleted(lines);
    }

    private int[] GetHorizontalLine(int rowIndex)
    {
        int[] line = new int[9];
        for (int i = 0; i < 9; i++)
        {
            line[i] = rowIndex * 9 + i;
        }
        return line;
    }
    private int CheckIfSquaresAreCompleted(List<int[]> data)
    {
        int linesCompleted = 0;
        HashSet<string> uniqueLines = new HashSet<string>();

        foreach (var line in data)
        {
            string lineKey = string.Join(",", line);
            if (!uniqueLines.Contains(lineKey))
            {
                uniqueLines.Add(lineKey);

                if (CheckLineColors(line))
                {
                    Shape.ShapeColor explosionColor = Shape.ShapeColor.None;

                    foreach (var index in line)
                    {
                        var gridSquare = _GridSquares[index].GetComponent<GridSquare>();
                        if (gridSquare.isOccupied && gridSquare.squareColor != Shape.ShapeColor.None)
                        {
                            explosionColor = gridSquare.squareColor;
                            break;
                        }
                    }

                    if (explosionColor != Shape.ShapeColor.None)
                    {
                        GameEvents.SetLastExplosionColorMethod(explosionColor);
                    }

                    ClearLine(line);
                    linesCompleted++;
                }
            }
        }

        if (linesCompleted > 0)
        {
            GameEvents.AddScoresMethod(10);
            GameEvents.TriggerOneByOneBlockExplosionMethod(GameEvents.LastExplosionColor);
            
            SendGridStateToServer();
            if (GameNetworkManager.Instance != null)
            {
                GameNetworkManager.Instance.LocalPlayerFinishedPlacingShapes();
            }
        }

        return linesCompleted;
    }

    private bool CheckLineColors(int[] line)
    {
        if (line.Length == 0) return false;

        var firstSquare = _GridSquares[line[0]].GetComponent<GridSquare>();
        if (!firstSquare.isOccupied) return false;

        var firstColor = firstSquare.squareColor;

        for (int i = 1; i < line.Length; i++)
        {
            var square = _GridSquares[line[i]].GetComponent<GridSquare>();

            if (!square.isOccupied || (square.squareColor != firstColor && square.squareColor != Shape.ShapeColor.Joker))
            {
                return false;
            }
        }

        return true;
    }


    private void ClearLine(int[] line)
    {
        foreach (var index in line)
        {
            var gridSquare = _GridSquares[index].GetComponent<GridSquare>();
            gridSquare.ClearOccupied();
            gridSquare.StopColorCycle();
        }
    }

    private void CheckIfPlayerLost()
    {
        var validShapes = 0;
        var totalActiveShapes = 0;

        foreach (var shape in shapeStorage.ShapeList)
        {
            if (shape.isInDropArea || !shape.gameObject.activeSelf || !shape.IsAnyOfShapeSquareActive())
            {
                continue;
            }

            totalActiveShapes++;
            
            if (CheckIfShapeCanBePlacedOnGrid(shape))
            {
                validShapes++;
            }
        }

        if (totalActiveShapes > 0 && validShapes == 0)
        {
            GameEvents.GameOverMethod(false);
        }
    }

    private bool CheckIfShapeCanBePlacedOnGrid(Shape currentShape)
    {
        var currentShapeData = currentShape.CurrentShapeData;
        if (currentShapeData == null)
        {
            return false;
        }
            
        var shapeColumns = currentShapeData.columns;
        var shapeRows = currentShapeData.rows;

        for (int row = 0; row <= 9 - shapeRows; row++)
        {
            for (int col = 0; col <= 9 - shapeColumns; col++)
            {
                bool canPlaceHere = true;

                for (int shapeRow = 0; shapeRow < shapeRows && canPlaceHere; shapeRow++)
                {
                    for (int shapeCol = 0; shapeCol < shapeColumns && canPlaceHere; shapeCol++)
                    {
                        if (currentShapeData.board[shapeRow].column[shapeCol])
                        {
                            int gridIndex = (row + shapeRow) * 9 + (col + shapeCol);
                            
                            if (gridIndex < 0 || gridIndex >= _GridSquares.Count)
                            {
                                canPlaceHere = false;
                                continue;
                            }

                            var gridSquare = _GridSquares[gridIndex].GetComponent<GridSquare>();
                            if (gridSquare.SquareOccupied)
                            {
                                canPlaceHere = false;
                            }
                        }
                    }
                }

                if (canPlaceHere)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private List<int[]> GetAllSquaresCombination(int columns, int rows)
    {
        var squareList = new List<int[]>();
        var lastColumnIndex = 0;
        var lastRowIndex = 0;
        int safeIndex = 0;

        while (lastRowIndex + (rows - 1) < 9)
        {
            var rowData = new List<int>();

            for (var row = lastRowIndex; row < lastRowIndex + rows; row++)
            {
                for (var column = lastColumnIndex; column < lastColumnIndex + columns; column++)
                {
                    rowData.Add(_LineIndicator.line_data[row, column]);
                }
            }

            squareList.Add(rowData.ToArray());
            lastColumnIndex++;

            if (lastColumnIndex + (columns - 1) >= 9)
            {
                lastRowIndex++;
                lastColumnIndex = 0;
            }

            safeIndex++;
            if (safeIndex > 100)
            {
                break;
            }
        }
        return squareList;
    }

    public GameObject GetGridSquare(int index)
    {
        if (index >= 0 && index < _GridSquares.Count)
        {
            return _GridSquares[index];
        }
        return null;
    }

    private void OnRequestNewShape()
    {
        StartCoroutine(CheckPlayerLostAfterDelay());
    }

    private IEnumerator CheckPlayerLostAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        CheckIfPlayerLost();
    }

    private void SendGridStateToServer()
    {
        List<GridSquareState> gridState = new List<GridSquareState>();
        foreach (var square in _GridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();
            if (gridSquare.isOccupied)
            {
                gridState.Add(new GridSquareState
                {
                    index = gridSquare.SquareIndex,
                    isOccupied = true,
                    colorIndex = (int)gridSquare.squareColor
                });
            }
        }
        GridStateManager.Instance.SendGridStateToServerRpc(
            gridState.Select(s => s.index).ToArray(),
            gridState.Select(s => s.isOccupied).ToArray(),
            gridState.Select(s => s.colorIndex).ToArray()
        );
    }
}