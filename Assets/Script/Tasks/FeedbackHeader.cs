using Storyboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackHeader : TaskListHeader
{
    public override IEnumerator ReloadTasks(List<Interaction> interactions)
    {
        if (interactions == null || interactions.Count <= 0) { Debug.LogError("Failed to Load Special TaskList"); yield break; }
        subTasks = new List<TaskHeader>();

        for (int i = 0; i < subTaskListObject.transform.childCount; i++)
        {
            var header = subTaskListObject.transform.GetChild(i).gameObject.GetComponent<TaskHeader>();
            if (header != null)
            {
                header.Editor = header.taskContent.transform.GetChild(0).GetComponent<InteractionEditor>();
                header.Editor.task.Copy(interactions[i]);
                header.Editor.SetTitle(header.taskTitle.text);
                header.Editor.UpdateSceneFromEditor();
                header.taskList = this;
                subTasks.Add(header);
            }
            else
            { Debug.Log("TaskHeader not found"); }
        }
        UpdateTaskIndices();
    }
}
