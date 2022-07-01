using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerEditor : MonoBehaviour
{
    public QuestionEditor question { set; private get; }
    public int index;
    public TMPro.TMP_InputField text;
    public Toggle toggle;

    public void AnswerText(string text)
    {
        question.EditAnswerText(text, index);
        this.text.text = text;
    }
    public void AnswerCorrect(bool correct)
    {
        question.EditAnswerCorrect(correct, index);
        toggle.isOn = correct;
    }
}
