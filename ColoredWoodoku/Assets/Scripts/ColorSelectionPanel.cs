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
        blueButton.onClick.AddListener(ChangeToBlue);
        greenButton.onClick.AddListener(ChangeToGreen);
        yellowButton.onClick.AddListener(ChangeToYellow);
    }
    
    private void ShowPanel(Shape shape)
    {
        currentShape = shape;
        UpdateCostText();
        
        if (panelObject == null)
        {
            return;
        }

        panelObject.SetActive(true);
    }
    
    private void UpdateCostText()
    {
        costText.text = "Blue: " + Shape.colorCosts[Shape.ShapeColor.Blue] + " " +
                       "Green: " + Shape.colorCosts[Shape.ShapeColor.Green] + " " +
                       "Yellow: " + Shape.colorCosts[Shape.ShapeColor.Yellow];
    }
    
    public void ChangeToBlue()
    {
        TryChangeColor(Shape.ShapeColor.Blue);
    }

    public void ChangeToGreen()
    {
        TryChangeColor(Shape.ShapeColor.Green);
    }

    public void ChangeToYellow()
    {
        TryChangeColor(Shape.ShapeColor.Yellow);
    }
    
    private void TryChangeColor(Shape.ShapeColor newColor)
    {
        if (currentShape != null)
        {
            if (currentShape.TryChangeColor(newColor))
            {
                panelObject.SetActive(false);
            }
        }
    }
    
    public void ClosePanel()
    {
        panelObject.SetActive(false);
    }
} 