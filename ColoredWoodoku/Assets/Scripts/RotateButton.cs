using UnityEngine;
using UnityEngine.UI;

public class RotateButton : MonoBehaviour
{
    public Button rotateButton;
    private Shape selectedShape;

    private void Start()
    {
        rotateButton.onClick.AddListener(OnRotateButtonClick);
    }

    public void SetSelectedShape(Shape shape)
    {
        selectedShape = shape;
    }

    private void OnRotateButtonClick()
    {
        if (selectedShape != null)
        {
            selectedShape.transform.Rotate(0, 0, 90); 
        }
    }
}