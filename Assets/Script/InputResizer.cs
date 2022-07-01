using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputResizer : MonoBehaviour
{
    [SerializeField]TMP_InputField input;
    RectTransform inputRect;
    LayoutElement layout;
    public float min = 50;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        inputRect = input.GetComponent<RectTransform>();
        layout = GetComponent<LayoutElement>();

        yield return new WaitWhile(() => StoryboardManager.Initializing);
        Resize("default");
    }
    void OnEnable()
    {
        if (input == null) return;
        input.onValueChanged.AddListener(Resize);
    }
    void OnDisable()
    {
        if (input == null) return;
        input.onValueChanged.RemoveListener(Resize);
    }

    //Gets the preferred size of the inputField, calculated by a ContentSizeFitter
    public void Resize(string text)
    {
        if (inputRect == null) return;

        float prefHeight = LayoutUtility.GetPreferredHeight(inputRect);
        prefHeight += 10; //add padding
        layout.minHeight = Mathf.Max(min, prefHeight);
    }
}
