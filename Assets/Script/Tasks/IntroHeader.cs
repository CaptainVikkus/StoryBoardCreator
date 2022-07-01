using Storyboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroHeader : TaskListHeader
{
    public override void MoveTaskIndex(int currentIndex, int newIndex)
    {
        //Can't move above first message: Intro Text
        if (newIndex < 1) return;
        //business as usual
        base.MoveTaskIndex(currentIndex, newIndex);
    }

    public override IEnumerator ReloadTasks(List<Interaction> interactions)
    {
        if (interactions == null || interactions.Count <= 0) { Debug.LogError("Failed to Load Special TaskList"); yield break; }

        subTasks = new List<TaskHeader>();

        //remove existing tasks except custom: Intro Text
        while (subTaskListObject.transform.childCount > 1)
        {
            DestroyImmediate(subTaskListObject.transform.GetChild(1).gameObject);
        }
        //populate scene from subtasks
        for (int i = 0; i < interactions.Count; i++)
        {
            if (i == 0)//Intro Text
            {
                var header = subTaskListObject.transform.GetChild(i).gameObject.GetComponent<TaskHeader>();
                header.Editor = header.taskContent.transform.GetChild(0).GetComponent<InteractionEditor>();
                header.Editor.task.Copy(interactions[i]);
                header.Editor.SetTitle(header.taskTitle.text);
                header.Editor.UpdateSceneFromEditor();
                header.taskList = this;
                subTasks.Add(header);
            }
            else //UserGenerated
            {
                var temp = Instantiate(taskPrefab, subTaskListObject.transform).GetComponent<TaskHeader>();
                temp.taskList = this;
                yield return temp.ReloadTask(interactions[i]);
                subTasks.Add(temp);
            }
        }
    }
}
