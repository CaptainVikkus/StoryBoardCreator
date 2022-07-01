using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Storyboard
{
    [System.Serializable]
    public class ScenarioBoard : MonoBehaviour
    {
        [SerializeField] public List<TaskListHeader> subLists { get; private set; }
        [SerializeField] GameObject taskListPrefab;
        [SerializeField] GameObject introPrefab;
        [SerializeField] GameObject endingPrefab;
        [SerializeField] GameObject feedbackPrefab;
        [SerializeField] GameObject taskListAdder;


        //Find intial tasks: intro, ending, feedback
        private IEnumerator Start()
        {
            yield return new WaitWhile(() => StoryboardManager.Initializing);

            if (subLists == null || subLists.Count == 0)
            {
                subLists = new List<TaskListHeader>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    //Find any TaskLists existing in the scene
                    var header = transform.GetChild(i).gameObject.GetComponent<TaskListHeader>();
                    if (header != null)
                    {
                        header.scenarioBoard = this;
                        subLists.Add(header);
                    }
                }
            }

            UpdateListIndices();
        }

        //Adds a new task list to the scene and sublist in the correct hierarchy/order
        public void AddTaskList()
        {
            var list = Instantiate(taskListPrefab, transform);
            //set display order to just above ending
            list.transform.SetSiblingIndex(transform.childCount - 4);
            //add list to sublists just above feedback
            var header = list.GetComponent<TaskListHeader>();
            header.scenarioBoard = this;
            header.SetNameInHierarchy($"TaskList {subLists.Count - 2}");
            subLists.Insert(subLists.Count - 2, header);
            //move the adder to proper position
            taskListAdder.transform.SetSiblingIndex(transform.childCount - 3);

            //Update List Info in scene
            UpdateListIndices();
        }

        public void MoveListIndex(int currentIndex, int newIndex)
        {
            //Exit if move would displace Intro, Ending or Feedback
            if (newIndex < 1 || newIndex >= subLists.Count - 2)
            {
                //Debug.LogWarning("Attempted to move group outside of editable" +
                //    "bounds");
                return;
            }
            //Back End
            var switcher = subLists[currentIndex];
            subLists[currentIndex] = subLists[newIndex];
            subLists[newIndex] = switcher;
            //Front End
            subLists[currentIndex].UpdateIndex(currentIndex);
            subLists[newIndex].UpdateIndex(newIndex);
            subLists[newIndex].transform.SetSiblingIndex(newIndex);
        }
        public void UpdateListIndices()
        {
            for (int i = 0; i < subLists.Count; i++)
            {
                subLists[i].UpdateIndex(i);
            }
        }

        //returns true if a list in subLists shares a name with listname
        public bool CheckListRepeats(string listName)
        {
            foreach (var list in subLists)
            {
                //check sanitized listName vs sanitized list Title, ignoring capitalisation
                if (Serializer.SanitizeString(list.title).Equals(
                    Serializer.SanitizeString(listName), System.StringComparison.CurrentCultureIgnoreCase))
                //returns if repeat found
                { return true; }
            }

            //no repeats found
            return false;
        }

        //Save the list of tasklists in the scenario
        public List<InteractionList> SaveBoard()
        {
            var listTasks = new List<InteractionList>();
            foreach (var listTask in subLists)
            {
                listTasks.Add(listTask.SaveTasks());
            }
            return listTasks;
        }

        //Reload the scenario from a saved list of tasklists
        public IEnumerator ReloadBoard(List<InteractionList> taskLists)
        {
            if (taskLists == null || taskLists.Count <= 0) { Debug.LogError("Error Loading Scenario Tasks"); yield break; }
            //move task adder out of the way
            taskListAdder.transform.SetAsFirstSibling();
            //wipe existing tasks in scene
            while (transform.childCount > 1)
            {
                DestroyImmediate(transform.GetChild(1).gameObject);
            }
            if (subLists == null || subLists.Count > 0)
                subLists = new List<TaskListHeader>();
            //rebuild tasks from sublist
            for (int i = 0; i < taskLists.Count; i++)
            {
                GameObject prefab;
                //special instances intro, ending, feedback
                if (i == 0)                          //Intro
                { prefab = introPrefab; }

                else if (i == (taskLists.Count - 2)) //Ending
                { prefab = endingPrefab; }

                else if (i == (taskLists.Count - 1)) //Feedback
                { prefab = feedbackPrefab; }

                else                                 //regular tasks
                { prefab = taskListPrefab; }

                //Create and reload TaskList
                var tasklist = Instantiate(prefab, transform).GetComponent<TaskListHeader>();
                tasklist.scenarioBoard = this;
                tasklist.SetNameInHierarchy(taskLists[i].title);
                tasklist.SetListType(taskLists[i].orderType);
                yield return tasklist.ReloadTasks(taskLists[i].interactions);
                subLists.Add(tasklist);
            }
            //replace task adder
            UpdateListIndices();
            taskListAdder.transform.SetSiblingIndex(transform.childCount - 3);
        }
    }
}