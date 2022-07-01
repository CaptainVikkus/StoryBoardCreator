using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Storyboard
{
    [System.Serializable]
    public class InteractionEditor : MonoBehaviour
    {
        //DATA
        [Header("Data")]
        [SerializeReference] public Interaction task;

        //UI
        [Header("UI - Interaction")]
        [SerializeField] protected TMP_InputField description;
        [SerializeField] protected TMP_InputField trigger;
        [SerializeField] protected Toggle scored;
        [SerializeField] protected TMP_InputField optionalDescription;
        [Header("UI - Feedback")]
        [SerializeField] protected TMP_InputField correctFeedback;
        [SerializeField] protected TMP_InputField inCorrectFeedback;
        [SerializeField] protected TMP_InputField partialFeedback;

        public bool Initialized { get; protected set; } = false;

        protected virtual void Awake()
        {
            if (task == null)
            {
                task = new Interaction();
                UpdateEditorFromScene();
            }
            else
            {
                UpdateSceneFromEditor();
            }
            Initialized = true;
        }
        
        #region Interaction
        public void SetTitle(string title) { task.title = title; }
        public void SetDescription(string desc) { task.description = desc; }
        public void SetTrigger(string trigger) { task.trigger = trigger; }
        public void SetScored(bool scored) { task.scored = scored; }
        public void SetOptionals(string option) { task.optionalDetails = option; }
        #endregion

        #region Feedback
        public void SetCorrectFeedback(string message)
        {
            task.feedback.correctMessage = message;
        }
        public void SetInCorrectFeedback(string message)
        {
            task.feedback.incorrectMessage = message;
        }
        public void SetPartialFeedback(string message)
        {
            task.feedback.partialcorrectMessage = message;
        }
        #endregion

        #region SceneUtilities
        //Imports data from scene to editor
        public virtual void UpdateEditorFromScene()
        {
            task.description = description.text;
            task.trigger = trigger.text;
            task.scored = scored.isOn;
            task.optionalDetails = optionalDescription.text;
            task.feedback.correctMessage = correctFeedback.text;
            task.feedback.incorrectMessage = inCorrectFeedback.text;
            task.feedback.partialcorrectMessage = partialFeedback.text;
        }

        //Imports data from editor to fields in scene
        public virtual void UpdateSceneFromEditor()
        {
            description.text = task.description;
            trigger.text = task.trigger;
            scored.isOn = task.scored;
            optionalDescription.text = task.optionalDetails;
            correctFeedback.text = task.feedback.correctMessage;
            inCorrectFeedback.text = task.feedback.incorrectMessage;
            partialFeedback.text = task.feedback.partialcorrectMessage;
        }
        #endregion
    }
}
