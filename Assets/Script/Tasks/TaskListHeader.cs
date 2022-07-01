using MXRClasses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Storyboard
{
    [System.Serializable]
    //Controls UI Header for TaskLists and creates Tasks
    public class TaskListHeader : MonoBehaviour
    {
        public GameObject subTaskListObject;
        public GameObject taskPrefab;
        public TMPro.TextMeshProUGUI GroupNumber;
        public TMPro.TMP_InputField GroupName;
        public TMPro.TMP_Dropdown GroupType;
        
        [HideInInspector] 
        public ScenarioBoard scenarioBoard { private get; set; }
        public int listIndex { get; private set; }
        public string title;
        public ListTaskType type = ListTaskType.sequential;
        public List<TaskHeader> subTasks { get; protected set; }
        public double id { get; protected set; }

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => StoryboardManager.Initializing);

            //Load scene content if subtasks didn't get initialized
            if (subTasks == null || subTasks.Count == 0)
            {
                subTasks = new List<TaskHeader>();
                //Check children of subtask object for Tasks
                for (int i = 0; i < subTaskListObject.transform.childCount; i++)
                {
                    var header = subTaskListObject.transform.GetChild(i).gameObject.GetComponent<TaskHeader>();
                    if (header != null)
                    {
                        header.taskList = this;
                        subTasks.Add(header);
                    }
                    else
                    { Debug.Log("TaskHeader not found"); }
                }
                //Make sure they have proper indexing
                UpdateTaskIndices();
            }
            //generate unique id
            id = Time.timeAsDouble;
        }

        #region Tasks
        public void ToggleSubTasks()
        {
            subTaskListObject.SetActive(
                !subTaskListObject.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(subTaskListObject.GetComponent<RectTransform>());
        }

        //Creates a task, adding it to subtask's and subtasktransform's end
        public void CreateSubTask()
        {
            var task = Instantiate(taskPrefab, subTaskListObject.transform).GetComponent<TaskHeader>();
            task.taskList = this;
            task.SetNameInHierarchy($"Task Title {subTasks.Count + 1}");
            subTasks.Add(task);
            task.UpdateIndex(subTasks.Count - 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(subTaskListObject.GetComponent<RectTransform>());
        }

        //Update the indices of all tasks inside of subTasks
        public void UpdateTaskIndices()
        {
            if (subTasks == null) return;
            for (int i = 0; i < subTasks.Count; i++)
            {
                subTasks[i].UpdateIndex(i);
            }
        }

        //Swaps tasks in currentIndex and newIndex subtasks and subtaskstransform
        public virtual void MoveTaskIndex(int currentIndex, int newIndex)
        {
            //Exit with warning if move is illegal
            if (newIndex >= subTasks.Count ||
                newIndex < 0)
            {
                //Debug.LogWarning("Attempted to move task out of bounds");
                return;
            }

            //Back End
            var switcher = subTasks[currentIndex];
            subTasks[currentIndex] = subTasks[newIndex];
            subTasks[newIndex] = switcher;
            //Front End
            subTasks[currentIndex].UpdateIndex(currentIndex);
            subTasks[newIndex].UpdateIndex(newIndex);
            subTasks[newIndex].transform.SetSiblingIndex(newIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(subTaskListObject.GetComponent<RectTransform>());
        }

        //returns true if a task in subTasks shares a name with taskName
        public bool CheckTaskRepeats(string taskName)
        {
            foreach (var task in subTasks)
            {
                //check sanitized taskName vs sanitized task Title, ignoring capitalisation
                if (Serializer.SanitizeString(task.Editor.task.title)
                    .Equals(Serializer.SanitizeString(taskName), System.StringComparison.CurrentCultureIgnoreCase))
                //returns if repeat found
                { return true; } 
            }

            //no repeats found
            return false;
        }
        #endregion

        #region TaskList
        public void SetNameInHierarchy(string text)
        {
            if (GroupName != null)
            {
                CheckError(text);
                GroupName.text = text;
            }
            
            title = text;
            gameObject.name = Serializer.SanitizeString(text);
        }

        public void SetListType(int type)
        {
            this.type = (ListTaskType)type;
            if (GroupType != null)
                GroupType.value = type;
        }

        //Delete the group from storyboard, then any flags, then itself
        public void DeleteGroup()
        {
            scenarioBoard.subLists.Remove(this);
            scenarioBoard.UpdateListIndices();
            StoryboardManager.Instance.RemoveFlag(id);
            Destroy(this.gameObject);
        }

        //Update Group to display index. i.e. T1 or T5
        public void UpdateIndex(int index)
        {
            listIndex = index;
            GroupNumber.text = "T" + (++index).ToString();
            UpdateTaskIndices();
        }

        public void MoveList(int amount)
        {
            scenarioBoard.MoveListIndex(listIndex, listIndex + amount);
        }

        public void CheckError(string text)
        {
            var cb = GroupName.colors;
            if (scenarioBoard.CheckListRepeats(text))
            { //Error = red tint
                cb.normalColor = new Color(255, 0, 0, .5f);
                //Add flag to storyboard
                StoryboardManager.Instance.AddFlag(id);
            }
            else
            { //No Error = clear white
                cb.normalColor = new Color(255, 255, 255, 0f);
                //Remove flag if exists in storyboard
                StoryboardManager.Instance.RemoveFlag(id);
            }
            GroupName.colors = cb;
        }
        #endregion

        //Save the Interactions in TaskHeaders' Editors to InteractionList
        public InteractionList SaveTasks()
        {
            var interactionList = new InteractionList();
            interactionList.orderType = (int)type;
            interactionList.title = title;
            interactionList.interactions = new List<Interaction>();
            foreach (var task in subTasks)
            {
                interactionList.interactions.Add(task.Editor.task);
            }
            return interactionList;
        }

        //Reloads TaskListHeader from saved data and reloads the lists' tasks
        public virtual IEnumerator ReloadTasks(List<Interaction> interactions)
        {
            if (interactions == null) { Debug.LogError("Failed to Load TaskList"); yield break; }
            subTasks = new List<TaskHeader>();

            //remove existing tasks
            while (subTaskListObject.transform.childCount > 0)
            {
                DestroyImmediate(subTaskListObject.transform.GetChild(0).gameObject);
            }
            //populate scene from subtasks
            foreach(var task in interactions)
            {
                var temp = Instantiate(taskPrefab, subTaskListObject.transform).GetComponent<TaskHeader>();
                temp.taskList = this;
                yield return temp.ReloadTask(task);
                subTasks.Add(temp);
            }
        }
    }
}
