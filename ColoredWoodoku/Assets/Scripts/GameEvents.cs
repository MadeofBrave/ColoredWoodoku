using System;

public static class GameEvents
{
    public static event Action<int> AddScores = delegate { };
    public static event Action<bool> GameOver = delegate { };
    public static event Action CheckIfShapeCanBePlaced = delegate { };
    public static event Action MoveShapetoStartPosition = delegate { };
    public static event Action RequestNewShape = delegate { };
    public static event Action SetShapeInactive = delegate { };
    public static event Action CheckIfOneByOneCanBePlaced;
    public static event Action<Shape.ShapeColor> TriggerOneByOneBlockExplosion;
    public static Shape.ShapeColor LastExplosionColor { get; private set; } = Shape.ShapeColor.None;
    public static event Action<int> UseHammer = delegate { };
    public static event Action<Shape> ShowColorSelectionPanel = delegate { };
    public static event Action<int> UseLineEraser = delegate { };
    public static event Action<Shape> ShowColorChangePanel;
    public static event Action<Shape> RotateShapeEvent = delegate { };
    public static event Action RequestNewShapes = delegate { };
    public static event Action<Shape> ShapeEnteredDropArea = delegate { };
    public static event Action<Shape> ShapeLeftDropArea = delegate { };
    public static event Action<Shape> ShapeStoredInDropArea = delegate { };

    public static void OnRequestNewShapes()
    {
        RequestNewShapes.Invoke();
    }
    public static void RotateShapeMethod(Shape shape)
    {
        RotateShapeEvent?.Invoke(shape);
    }
    public static void UseHammerMethod(int squareIndex)
    {
        UseHammer?.Invoke(squareIndex);
    }

    public static void SetLastExplosionColorMethod(Shape.ShapeColor color)
    {
        LastExplosionColor = color;
        
        // When explosion color changes, notify server to sync it if we're in a networked game
        // (This happens on next turn anyway, but this makes it more immediate)
        if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.IsServer)
        {
            // The server already knows the color, just make sure it's synced next update
        }
        else if (GameNetworkManager.Instance != null && !GameNetworkManager.Instance.IsServer)
        {
            // Client can notify server about color change, but server has final say on synced values
            // The server will sync back on next turn completion
        }
    }

    public static void CheckIfOneByOneCanBePlacedMethod()
    {
        CheckIfOneByOneCanBePlaced?.Invoke();
    }

    public static void TriggerOneByOneBlockExplosionMethod(Shape.ShapeColor color)
    {
        if (color == Shape.ShapeColor.None)
        {
            return;
        }
        ShapeStorage.Instance.EnableColorSquare();
    }

    public static void AddScoresMethod(int score)
    {
        AddScores(score);
    }

    public static void GameOverMethod(bool newBestScore)
    {
        GameOver(newBestScore);
    }

    public static void CheckIfShapeCanBePlacedMethod()
    {
        CheckIfShapeCanBePlaced();
    }

    public static void MoveShapetoStartPositionMethod()
    {
        MoveShapetoStartPosition();
    }

    public static void RequestNewShapeMethod()
    {
        RequestNewShape();
    }

    public static void SetShapeInactiveMethod()
    {
        SetShapeInactive();
    }

    public static void ShowColorSelectionPanelMethod(Shape shape)
    {
        ShowColorSelectionPanel?.Invoke(shape);
    }

    public static void UseLineEraserMethod(int squareIndex)
    {
        UseLineEraser?.Invoke(squareIndex);
    }

    public static void ShowColorChangePanelMethod(Shape shape)
    {
        if (ShowColorChangePanel != null)
        {
            ShowColorChangePanel(shape);
        }
    }

    public static void OnShapeEnteredDropArea(Shape shape)
    {
        ShapeEnteredDropArea?.Invoke(shape);
    }

    public static void OnShapeLeftDropArea(Shape shape)
    {
        ShapeLeftDropArea?.Invoke(shape);
    }

    public static void OnShapeStoredInDropArea(Shape shape)
    {
        ShapeStoredInDropArea?.Invoke(shape);
    }
}