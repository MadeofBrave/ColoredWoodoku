using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Shape;

public class NewBehaviourScript : MonoBehaviour
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
    private List<GameObject> _GridSquares = new List<GameObject>();
    private LineIndicator _LineIndicator;
    private Shape shape;
    private ColorSquare _colorSquare;
    public int[,] line_data = new int[9, 9];
    public bool SquareOccupied { get; private set; } = false; 
    public Shape.ShapeColor SquareColor { get; private set; } = Shape.ShapeColor.None; 

    public void SetSquareOccupied(bool occupied, Shape.ShapeColor color)
    {
        SquareOccupied = occupied;
        SquareColor = color;
        Debug.Log($"Kare Güncellendi -> Doluluk: {SquareOccupied}, Renk: {SquareColor}");
    }

    public void ClearSquare()
    {
        SquareOccupied = false;
        SquareColor = Shape.ShapeColor.None;
        Debug.Log($"Kare Temizlendi -> Doluluk: {SquareOccupied}, Renk: {SquareColor}");
    }

    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;

    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
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
        if (currentSelectedShape == null) return;

        if (currentSelectedShape.TotalSquareNumber != squareIndexes.Count)
        {
            GameEvents.MoveShapetoStartPositionMethod();
            return;
        }

        bool canPlaceShape = squareIndexes.All(index => _GridSquares[index].GetComponent<GridSquare>().CanWeUseTheSquare());

        if (!canPlaceShape)
        {
            GameEvents.MoveShapetoStartPositionMethod();
            return;
        }

        foreach (var squareIndex in squareIndexes)
        {
            _GridSquares[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard(currentSelectedShape.shapeColor);
        }

        bool anyShapeLeft = shapeStorage.ShapeList.Any(shape => shape.IsonStartPosition() && shape.IsAnyOfShapeSquareActive());

        if (!anyShapeLeft)
        {
            GameEvents.RequestNewShapeMethod();
        }
        else
        {
            GameEvents.SetShapeInactiveMethod();
        }
        CheckIfAnyLineIsCompleted();
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

    private void CheckIfAnyLineIsCompleted()
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

    private Shape.ShapeColor GetExplosionColorFromCompletedLines(List<int[]> completedLines)
    {
        Shape.ShapeColor finalColor = Shape.ShapeColor.None;
        Dictionary<Shape.ShapeColor, int> colorCount = new Dictionary<Shape.ShapeColor, int>();

        foreach (var line in completedLines)
        {
            foreach (var index in line)
            {
                if (_GridSquares[index] == null) continue;

                GridSquare square = _GridSquares[index].GetComponent<GridSquare>();
                if (square != null && square.isOccupied)
                {
                    finalColor = square.squareColor; 
                }
            }
        }
        return finalColor;
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

        Shape.ShapeColor explosionColor = Shape.ShapeColor.None;

        foreach (var line in data)
        {
            string lineKey = string.Join(",", line);
            if (!uniqueLines.Contains(lineKey))
            {
                uniqueLines.Add(lineKey);

                if (CheckLineColors(line))
                {
                    ClearLine(line);
                    linesCompleted++;
                }
            }
        }

        if (linesCompleted > 0)
        {
            explosionColor = GetExplosionColorFromCompletedLines(data);
            if (explosionColor != Shape.ShapeColor.None) 
            {
                GameEvents.AddScoresMethod(linesCompleted * 10);
                GameEvents.SetLastExplosionColorMethod(explosionColor);
                GameEvents.TriggerOneByOneBlockExplosionMethod(explosionColor);
            }
        }


        return linesCompleted;
    }


    public GridSquare GetGridSquare(int index)
    {
        if (index >= 0 && index < _GridSquares.Count)
        {
            return _GridSquares[index].GetComponent<GridSquare>();
        }
        return null; 
    }


    private bool CheckIfLineIsCompleted(int[] line)
    {
        foreach (int index in line)
        {
            GridSquare square = _GridSquares[index].GetComponent<GridSquare>();
            if (square == null || !square.isOccupied)
            {
                return false;
            }
        }
        return true;
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

            if (!square.isOccupied || square.squareColor != firstColor)
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
        }
    }

    private void CheckIfPlayerLost()
    {
        var validShapes = 0;

        foreach (var shape in shapeStorage.ShapeList)
        {
            bool isShapeActive = shape.IsAnyOfShapeSquareActive();
            bool canBePlaced = CheckIfShapeCanBePlacedOnGrid(shape);

            Debug.Log($"Şekil: {shape.name}, Aktif mi: {isShapeActive}, Yerleşebilir mi: {canBePlaced}");

            if (canBePlaced && isShapeActive)
            {
                shape.ActivateShape();
                validShapes++;
            }
        }

        Debug.Log("Geçerli şekil sayısı: " + validShapes);

        if (validShapes == 0)
        {
            Debug.Log("OYUNCU KAYBETTİ!");
            GameEvents.GameOverMethod(false);
        }
    }


    private bool CheckIfShapeCanBePlacedOnGrid(Shape currentShape)
    {
        var currentShapeData = currentShape.CurrentShapeData;
        var shapeColumns = currentShapeData.columns;
        var shapeRows = currentShapeData.rows;

        List<int> originalShapeFilledUpSquares = new List<int>();
        var squareIndex = 0;

        for (var rowIndex = 0; rowIndex < shapeRows; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < shapeColumns; columnIndex++)
            {
                if (currentShapeData.board[rowIndex].column[columnIndex])
                {
                    originalShapeFilledUpSquares.Add(squareIndex);
                }
                squareIndex++;
            }
        }

        var squareList = GetAllSquaresCombination(shapeColumns, shapeRows);
        bool canBePlaced = false;

        foreach (var number in squareList)
        {
            Debug.Log($"Şekil {currentShape.name} şu kareleri kontrol ediyor: {string.Join(", ", number)}");

            bool shapeCanBePlacedOnTheBoard = true;
            foreach (var squareIndexToCheck in originalShapeFilledUpSquares)
            {
                if (squareIndexToCheck >= number.Length)
                {
                    Debug.LogError($"HATA: Index out of bounds: {squareIndexToCheck}");
                    shapeCanBePlacedOnTheBoard = false;
                    break;
                }

                var comp = _GridSquares[number[squareIndexToCheck]].GetComponent<GridSquare>();

                Debug.Log($"Kare index: {number[squareIndexToCheck]}, Doluluk: {comp.SquareOccupied}, Kare Rengi: {comp.squareColor}, Şekil Rengi: {currentShape.shapeColor}");

                if (comp.SquareOccupied || (comp.squareColor != Shape.ShapeColor.None && comp.squareColor != currentShape.shapeColor))
                {
                    Debug.Log($"Şekil {currentShape.name} şu kareye yerleşemez: {number[squareIndexToCheck]}");
                    shapeCanBePlacedOnTheBoard = false;
                    break;
                }
            }

            if (shapeCanBePlacedOnTheBoard)
            {
                Debug.Log($"Şekil {currentShape.name} için yer bulundu!");
                canBePlaced = true;
                break;
            }
        }
        Debug.Log($"[Grid] {currentShape.name} için yerleşim kontrol ediliyor: {string.Join(", ", originalShapeFilledUpSquares)}");

        Debug.Log($"CheckIfShapeCanBePlacedOnGrid Sonucu: {canBePlaced}");
        return canBePlaced;
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
  


}
