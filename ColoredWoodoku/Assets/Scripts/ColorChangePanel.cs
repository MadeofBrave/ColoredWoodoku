using UnityEngine;
using UnityEngine.UI;
using System;

public class ColorChangePanel : MonoBehaviour
{
    private const int COLOR_CHANGE_COST = 30;
    private Shape currentShape;
    private CanvasGroup canvasGroup;

    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Text costText;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HidePanel();

        if (blueButton != null) blueButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Blue));
        if (greenButton != null) greenButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Green));
        if (yellowButton != null) yellowButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Yellow));

        if (costText != null)
        {
            costText.text = COLOR_CHANGE_COST.ToString();
        }
    }

    public void ShowPanel(Shape shape, Vector2 position)
    {
        currentShape = shape;
        transform.position = position;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        UpdateButtonsInteractability();
    }

    public void HidePanel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        currentShape = null;
    }

    private void UpdateButtonsInteractability()
    {
        bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(COLOR_CHANGE_COST);
        blueButton.interactable = hasEnoughPoints;
        greenButton.interactable = hasEnoughPoints;
        yellowButton.interactable = hasEnoughPoints;
    }

    private void ChangeColor(Shape.ShapeColor newColor)
    {
        if (currentShape == null || !Scores.Instance.HasEnoughPoints(COLOR_CHANGE_COST)) return;

        if (currentShape.TryChangeColor(newColor))
        {
            HidePanel();
        }
    }
} 