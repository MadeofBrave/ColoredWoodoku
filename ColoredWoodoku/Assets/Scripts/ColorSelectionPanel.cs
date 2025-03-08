using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorSelectionPanel : MonoBehaviour
{
    public GameObject panelObject;
    public Button blueButton;
    public Button greenButton;
    public Button yellowButton;
    public Text costText;
    
    private Shape currentShape;
    
    private void Awake()
    {
        panelObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        GameEvents.ShowColorSelectionPanel += ShowPanel;
    }
    
    private void OnDisable()
    {
        GameEvents.ShowColorSelectionPanel -= ShowPanel;
    }
    
    private void Start()
    {
        blueButton.onClick.AddListener(() => TryChangeColor(Shape.ShapeColor.Blue));
        greenButton.onClick.AddListener(() => TryChangeColor(Shape.ShapeColor.Green));
        yellowButton.onClick.AddListener(() => TryChangeColor(Shape.ShapeColor.Yellow));
    }
    
    private void ShowPanel(Shape shape)
    {
        currentShape = shape;
        UpdateCostText();
        panelObject.SetActive(true);
    }
    
    private void UpdateCostText()
    {
        costText.text = "Mavi: " + Shape.colorCosts[Shape.ShapeColor.Blue] + "\n" +
                       "Yeşil: " + Shape.colorCosts[Shape.ShapeColor.Green] + "\n" +
                       "Sarı: " + Shape.colorCosts[Shape.ShapeColor.Yellow];
    }
    
    private void TryChangeColor(Shape.ShapeColor newColor)
    {
        if (currentShape != null)
        {
            if (currentShape.TryChangeColor(newColor))
            {
                panelObject.SetActive(false);
            }
            else
            {
                // Yetersiz puan uyarısı gösterilebilir
            }
        }
    }
    
    public void ClosePanel()
    {
        panelObject.SetActive(false);
    }
} 