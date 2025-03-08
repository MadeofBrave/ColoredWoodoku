using System;
using UnityEngine;

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
    public static event Action<Shape> ShowColorSelectionPanel;

    public static void UseHammerMethod(int squareIndex)
    {
        UseHammer?.Invoke(squareIndex);
    }

    public static void SetLastExplosionColorMethod(Shape.ShapeColor color)
    {
        LastExplosionColor = color;
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
}