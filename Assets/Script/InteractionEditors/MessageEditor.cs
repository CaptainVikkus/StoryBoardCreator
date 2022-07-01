using Storyboard;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageEditor : InteractionEditor
{
    [Header("UI - Message")]
    [SerializeField] protected TMP_InputField message;

    protected override void Awake()
    {
        if (task == null)
        {
            task = new Message();
            UpdateEditorFromScene();
        }
        else
        {
            UpdateSceneFromEditor();
        }
        Initialized = true;
    }

    #region Message
    public void SetMessage(string message) { (task as Message).message = message; }
    #endregion
    
    public override void UpdateSceneFromEditor()
    {
        base.UpdateSceneFromEditor();

        if (task is Message m)
        {
            message.text = m.message;
        }
    }

    public override void UpdateEditorFromScene()
    {
        base.UpdateEditorFromScene();

        if (task is Message m)
        {
            m.message = message.text;
        }
    }
}
