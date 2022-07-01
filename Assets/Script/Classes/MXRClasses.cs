using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MXRClasses
{
    #region ScenarioTaskList
    [System.Serializable]
    public class ScenarioTask
    {
        public string taskName = "Base Task";
        public bool expanded = true;
    }

    [System.Serializable]
    public class ScenarioAction : ScenarioTask
    {
        public bool isScoreable;
        public bool isQuestion;
        public ScenarioAction() { taskName = "Action Task"; }
    }

    [System.Serializable]
    public class ScenarioList : ScenarioTask
    {
        public ListTaskType listType;
        [SerializeReference] //The Magical Field
        public List<ScenarioTask> tasks = new List<ScenarioTask>();
        public ScenarioList() { taskName = "List Task"; }
    }

    #endregion

    #region QuestionDialogueManager
    public enum QuestionDialogueType
    {
        MultiChoicePanel,
        CharacterDialoguePanel,
        CharacterMonologuePanel,
        ImageSelectionPanel,
        SliderQuestionPanel,
        MultiChoiceImageRef,
        TextDragAndDropPanel,
        ImageDragAndDropPanel,
        SliderDynamicImageRef,
    }

    [System.Serializable]
    public class QDImporter
    {
        //TaskName
        public string id;

        //Localisation Strings
        public string title;
        public string question;
        public string correctAnswerResponse;
        public string incorrectAnswerResponse;
        public string partialAnswerResponse;

        public QuestionDialogueType qdType;
        public List<QDImportAnswer> qdOptions = new List<QDImportAnswer>();

    }

    [System.Serializable]
    public class QDImportAnswer
    {
        public string optionText = "";
        public string optionImageLocalised = "";
        public string responseText = "";
        public string responseAudio = "";
        public bool correctOption = false;
        public bool optionClosesWindow = false;
    }

    #endregion

    public enum ListTaskType
    {
        anyOrder,
        anyOne,
        sequential
    }

}
