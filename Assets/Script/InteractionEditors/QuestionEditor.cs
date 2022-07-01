using MXRClasses;
using Storyboard;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionEditor : InteractionEditor
{
    [Header("UI - Question")]
    [SerializeField] TMP_InputField question;
    [SerializeField] TMP_Dropdown questionType;
    [SerializeField] TMP_InputField correctAnswerMessage;
    [SerializeField] TMP_InputField inCorrectAnswerMessage;
    [SerializeField] TMP_InputField partialAnswerMessage;
    [SerializeField] Transform answersTransform;
    [SerializeField] GameObject answerPrefab;

    protected override void Awake()
    {
        if (task == null)
        {
            task = new Question();
            UpdateEditorFromScene();
        }
        else
        {
            UpdateSceneFromEditor();
        }
        Initialized = true;
    }

    #region Question
    public void SetQuestion(string question) { (task as Question).question = question; }
    public void SetQuestionType(int type) { (task as Question).qType = (QuestionDialogueType)type; }
    public void SetCorrectMessage(string message) { (task as Question).correctMessage = message; }
    public void SetInCorrectMessage(string message) { (task as Question).incorrectMessage = message; }
    public void SetPartialCorrectMessage(string message) { (task as Question).partialcorrectMessage = message; }
    public void SetAnswers(List<string> answers, List<bool> correct)
    {
        (task as Question).answers = answers;
        (task as Question).correctAnswers = correct;
    }
    #endregion

    #region Answers
    //Create a default answer and update the answer with it's place in question.answers
    public void AddQuestion()
    {
        if (task is Question q)
        {
            var answer = CreateAnswer();
            q.answers.Add("");
            q.correctAnswers.Add(true);
            //set index to the last in q.answers
            answer.index = q.answers.Count - 1;
        }
    }

    //Creates the answer field from prefab, passing a reference to the question
    public AnswerEditor CreateAnswer()
    {
        if (task is Question q)
        {
            //create answer in scene
            var answer = Instantiate(answerPrefab, answersTransform).GetComponent<AnswerEditor>();
            answer.question = this;
            return answer;
        }
        return null;
    }

    //Remove the answer at the last index from both scene and question
    public void RemoveQuestion()
    {
        if (task is Question q)
        {
            //Get Last Question
            if (answersTransform.childCount == 0) return;
            var answer = answersTransform.GetChild(answersTransform.childCount - 1)
            .gameObject.GetComponent<AnswerEditor>();
            if (answer == null) return; //no answer found

            //Remove from scene and answer list
            q.answers.RemoveAt(answer.index);
            q.correctAnswers.RemoveAt(answer.index);
            Destroy(answer.gameObject);
        }
    }

    public void ClearQuestions()
    {
        while(answersTransform.childCount > 0)
        {
            DestroyImmediate(answersTransform.GetChild(0).gameObject);
        }
    }

    public void EditAnswerText(string text, int index)
    {
        if (task is Question q)
        {
            q.answers[index] = text;
        }
    }
    public void EditAnswerCorrect(bool correct, int index)
    {
        if (task is Question q)
        {
            q.correctAnswers[index] = correct;
        }
    }

    #endregion
    //Import data from the scene, first base, then question specific
    public override void UpdateEditorFromScene()
    {
        base.UpdateEditorFromScene();

        if (task is Question q)
        {
            q.question = question.text;
            q.qType = (QuestionDialogueType)questionType.value;
            //Answer List
            q.correctMessage = correctAnswerMessage.text;
            q.incorrectMessage = inCorrectAnswerMessage.text;
            q.partialcorrectMessage = partialAnswerMessage.text;
        }
    }

    //Import editor data to the scene, first base, then question specific
    public override void UpdateSceneFromEditor()
    {
        base.UpdateSceneFromEditor();

        if (task is Question q)
        {
            question.text = q.question;
            questionType.value = (int)q.qType;

            var qp = GetComponentInChildren<QuestionEditor>();
            //Answer List
            if (qp != null && q.answers.Count == q.correctAnswers.Count)
            {
                qp.ClearQuestions();
                for (int i = 0; i < q.answers.Count; i++)
                {
                    var ans = qp.CreateAnswer();
                    ans.index = i;
                    ans.AnswerCorrect(q.correctAnswers[i]);
                    ans.AnswerText(q.answers[i]);
                }
            }

            correctAnswerMessage.text = q.correctMessage;
            inCorrectAnswerMessage.text = q.incorrectMessage;
            partialAnswerMessage.text = q.partialcorrectMessage;
        }
    }
}
