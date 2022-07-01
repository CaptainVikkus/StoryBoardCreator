using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Storyboard
{
    [System.Serializable]
    //Controls UI Header for InteractionEditors and connects to TaskListHeader
    public class TaskHeader : MonoBehaviour
    {
        [Header("UI")]
        public GameObject taskContent;
        public TMPro.TextMeshProUGUI taskNumber;
        public TMPro.TMP_InputField taskTitle;
        public TMPro.TMP_Dropdown taskType;

        [Header("Prefab")]
        public GameObject questionPrefab;
        public GameObject messagePrefab;
        public GameObject interactionPrefab;

        [HideInInspector]
        public InteractionEditor Editor;
        public TaskListHeader taskList { private get; set; }
        public int taskIndex { get; private set; }
        public double id { get; private set; }

        public IEnumerator Start()
        {
            yield return new WaitWhile(() => StoryboardManager.Initializing);

            //check for editor content
            if (taskContent.transform.childCount > 0 && Editor == null)
            {
                Editor = taskContent.transform.GetChild(0).GetComponent<InteractionEditor>();
                Editor.UpdateEditorFromScene();
                //initialise title
                Editor.SetTitle(taskTitle.text);
            }
            //otherwise create a basic task
            else if (Editor == null)
            {
                Editor = CreateInteraction(InteractionType.Basic, taskContent);
                yield return new WaitWhile(() => Editor.task == null);
                //initialise title
                Editor.SetTitle(taskTitle.text);
            }
            //generate unique id
            id = Time.timeAsDouble;
        }

        #region UserInterface
        public void ToggleContent()
        {
            taskContent.SetActive(!taskContent.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(taskContent.GetComponent<RectTransform>());
        }

        //Sets the name for Hierarchy, editor and display
        public void SetNameInHierarchy(string text)
        {
            //Display
            if (taskTitle != null)
            {
                CheckError(text);
                taskTitle.text = text;
            }
            //Editor
            if (Editor != null)
            {
                Editor.SetTitle(text);
            }
            //Hierarchy
            gameObject.name = Serializer.SanitizeString(text);
        }

        //Deletes the task from its owning list, then itself and any flags
        public void Delete()
        {
            taskList.subTasks.Remove(this);
            taskList.UpdateTaskIndices();
            StoryboardManager.Instance.RemoveFlag(id);
            Destroy(this.gameObject);
        }

        //Update task to display index. i.e. 2.01 or 5.12
        public void UpdateIndex(int index)
        {
            taskIndex = index;
            taskNumber.text = (taskList.listIndex + 1).ToString() + "." +
                ((taskIndex < 10) ? "0" + (++index).ToString() :
                (++index).ToString());
        }

        //Move the tasks within the list
        public void MoveTask(int moveAmount)
        {
            taskList.MoveTaskIndex(taskIndex, taskIndex + moveAmount);
        }

        public void CheckError(string text)
        {
            var cb = taskTitle.colors;
            if (taskList.CheckTaskRepeats(text))
            { //Error = red tint
                cb.normalColor = new Color(255, 0, 0, .5f);
                StoryboardManager.Instance.AddFlag(id);
            }
            else
            { //No Error = clear white
                cb.normalColor = new Color(255, 255, 255, 0f);
                StoryboardManager.Instance.RemoveFlag(id);
            }
            taskTitle.colors = cb;
        }
        #endregion

        #region Interactions
        //Create Interaction based on InteractionType
        public InteractionEditor CreateInteraction(InteractionType type, GameObject parent)
        {
            InteractionEditor editor;
            switch (type)
            {
                //Special UI
                case InteractionType.Question:
                    editor = Instantiate(questionPrefab, parent.transform).GetComponent<QuestionEditor>();
                    editor.task = new Question();
                    return editor;
                case InteractionType.Message:
                    editor = Instantiate(messagePrefab, parent.transform).GetComponent<MessageEditor>();
                    editor.task = new Message();
                    return editor;
                //Basic Interaction UI
                case InteractionType.Basic:
                case InteractionType.Area:
                case InteractionType.Touch:
                case InteractionType.Look:
                default:
                    editor = Instantiate(interactionPrefab, parent.transform).GetComponent<InteractionEditor>();
                    editor.task = new Interaction();
                    editor.task.type = type;
                    return editor;
            }
        }

        //Copies existing data, creates a new interaction, then gives previous info to it
        public void SwitchInteraction(System.Int32 type)
        {
            if (Editor == null) return; // prevent false fires

            //Save old content
            var temp = Editor.task;
            //Destroy old content in taskContent
            if (taskContent.transform.childCount > 0)
                Destroy(taskContent.transform.GetChild(0).gameObject);

            //Create new content in taskContent
            Editor = CreateInteraction((InteractionType)type, taskContent);
            taskType.value = type;
            //copy old content to new editor
            Editor.task.Copy(temp);
            Editor.UpdateSceneFromEditor();
        }

        #endregion

        //Reloads the task and interaction from saved data
        public IEnumerator ReloadTask(Interaction task)
        {
            if (task == null) { Debug.LogError("Failed to load task"); yield break; }

            //Destroy old content in taskContent
            if (taskContent.transform.childCount > 0)
                Destroy(taskContent.transform.GetChild(0).gameObject);

            //Create interaction in taskHeader
            Editor = CreateInteraction(task.type, taskContent);
            yield return new WaitWhile(() => !Editor.Initialized);
            //Populate data
            Editor.task.Copy(task);
            taskType.value = (int)task.type;
            Editor.UpdateSceneFromEditor();
            SetNameInHierarchy(task.title);
        }
    }
}
