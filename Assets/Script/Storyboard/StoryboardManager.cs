using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Storyboard;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

[System.Serializable]
public class StoryboardManager : MonoBehaviour
{
    [Header("UI Links")]
    public TMPro.TMP_InputField moduleName;
    public GameObject scenarioFolder;
    public GameObject scenarioBoard;
    public ScrollRect scrollBar;
    [Header("Prefabs")]
    public GameObject scenarioPrefab;
    public GameObject boardPrefab;
    [SerializeField] public List<ScenarioHeader> scenarios;
    private ScenarioBoard activeBoard;
    //Saving, Exporting
    public string savename { get; private set; }  = "module";
    private List<double> repeats;

    //Singleton
    public static StoryboardManager Instance { get; private set; }
    public static bool Initializing;

    private void Awake()
    {
        if (StoryboardManager.Instance != null)
            Destroy(this); //There can be only one
        else
        {
            //Init
            Initializing = true;
            scenarios = new List<ScenarioHeader>();
            Instance = this;    
            //Load Save
            if (Serializer.SaveExists(Serializer.GetSavePath(savename)))
            {
                Debug.Log("Began Loading " + Serializer.GetSavePath(savename));
                //replace existing scenariomanager
                var data = Serializer.LoadModule(savename);
                //load scenariomanager to scene
                StartCoroutine(ReloadInstance(data));
            }
            //Failed to Load or no save found
            if (scenarios.Count == 0)
            {
                //Find Existing In Editor
                foreach (var item in FindObjectsOfType<ScenarioHeader>())
                { scenarios.Add(item); }
            }
            //Show first scenario
            if (scenarios.Count > 0) { ShowScenario(scenarios[0].scenario); }

            Initializing = false;
        }
    }
    //Update the scrollbar to the shown board
    public void UpdateScrollBar()
    {
        scrollBar.content = activeBoard.GetComponent<RectTransform>();
    }

    public void UpdateModuleName(string name)
    {
        savename = Serializer.SanitizeString(name);
    }

    #region Scenarios
    //Create a header and board for a scenario, naming both according to their index
    public void CreateScenario()
    {
        if (scenarios == null)
            scenarios = new List<ScenarioHeader>();

        //Create ScenarioHeader
        var header = Instantiate(scenarioPrefab, scenarioFolder.transform).GetComponent<ScenarioHeader>();
        scenarios.Add(header);
        header.title.text = "Scenario " + scenarios.Count.ToString();
        header.name = "Scenario" + scenarios.Count.ToString();
        //Create ScenarioBoard
        var board = Instantiate(boardPrefab, scenarioBoard.transform).GetComponent<ScenarioBoard>();
        board.name = "Scenario" + scenarios.Count.ToString();
        header.scenario = board;
        //Select the new scenario
        header.gameObject.GetComponent<Button>().Select();
        ShowScenario(board);
    }

    //Hide any other scenario and show the given one
    public void ShowScenario(ScenarioBoard board)
    {
        foreach (var header in scenarios)
        { header.scenario.gameObject.SetActive(false); }

        board.gameObject.SetActive(true);
        activeBoard = board;
        UpdateScrollBar();
    }

    public void RemoveScenario(ScenarioHeader header)
    {
        scenarios.Remove(header);
        Destroy(header.scenario.gameObject);
        Destroy(header.gameObject);
        //Rename Scenarios
        for (int i = 0; i < scenarios.Count; i++)
        {
            string title = "Scenario " + (i + 1).ToString();

            scenarios[i].title.text = title;
            scenarios[i].name = title.Replace(" ", "");
            scenarios[i].scenario.name = title.Replace(" ", "");
        }
    }
    #endregion

    #region Flags
    public void AddFlag(double id)
    {
        //create a list in manager if it isn't instantiated yet
        if (repeats == null)
        { repeats = new List<double>(); }
        //check to prevent adding flag twice
        if (repeats.Contains(id))
        { return; }
        //add the flag
        repeats.Add(id);
    }

    public bool RemoveFlag(double id)
    {
        //can't remove from a null list
        if (repeats == null)
        { return false; }
        //use the list remove to return true or false
        else
        { return repeats.Remove(id); }
    }

    //storyboard has flags if repeats list exists and has >0 entries
    public bool HasFlags()
    { return repeats != null && repeats.Count > 0; }

    #endregion

    #region Saving
    private void OnApplicationQuit()
    {
        //Serializer.SaveModule(Instance, savename);
    }

    //Save the TaskLists (List<InteractionList>) of each scenario
    public Module SaveInstance()
    {
        var module = new Module();
        if (moduleName == null || moduleName.text == "")
        { module.title = "Module"; }
        else
        { module.title = moduleName.text; }
        module.scenarios = new List<List<InteractionList>>();
        foreach (var header in scenarios)
        {
            module.scenarios.Add(header.scenario.SaveBoard());
        }
        return module;
    }

    //Rebuild the scene from a saved module
    public IEnumerator ReloadInstance(Module interactionLists)
    {
        if (interactionLists == null || interactionLists.scenarios.Count <= 0) { Debug.LogError("Failed to Load Instance"); yield break; }

        //remove existing headers
        while (scenarioFolder.transform.childCount > 0)
        {
            DestroyImmediate(scenarioFolder.transform.GetChild(0).gameObject);
        }
        //remove existing boards, avoiding scrollbar at index 0
        while (scenarioBoard.transform.childCount > 1)
        {
            DestroyImmediate(scenarioBoard.transform.GetChild(1).gameObject);
        }
        scenarios = new List<ScenarioHeader>();

        //build from data
        moduleName.text = interactionLists.title;
        foreach (var scenario in interactionLists.scenarios)
        {
            //create scenario header and board, then select latest and reload it
            CreateScenario();
            yield return scenarios[scenarios.Count - 1].scenario.ReloadBoard(scenario);
        }
    }
    #endregion
}
