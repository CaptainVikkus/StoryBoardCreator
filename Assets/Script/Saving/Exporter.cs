using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Storyboard;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using MXRClasses;

public class Exporter : MonoBehaviour
{
    [SerializeField] StoryboardManager storyboard;
    [SerializeField] TMPro.TMP_InputField module;

    //Export any relevant data to appropriate files
    public void Export()
    {
        //Save first
        Save(module.text);
        //Check for Errors in formatting
        if (StoryboardManager.Instance.HasFlags() == false)
        {
            //Excel for Localisations
            BuildExcel();
            //ScenarioTaskList for ScenarioManager
            BuildScenarioTaskLists();
            //Questions for QuestionsDialogueManager
            BuildQuestions();
        }
        else
        {
            Debug.Log("Did not export because errors were found in Storyboard");
        }
    }

    //Save Module to filename after sanitizing the string
    public void Save(string filename)
    {
        filename = Serializer.SanitizeString(filename);
        if (filename == "")
            filename = StoryboardManager.Instance.savename;

        //Export a binary file of the scene
        Serializer.SaveModule(StoryboardManager.Instance, filename);
#if UNITY_EDITOR
        //Export a sudo-JSON file for developer purposes
        Serializer.SaveJSON(StoryboardManager.Instance);
#endif
    }

    //Load a module from filename if it exists
    public void Load(string filename)
    {
        filename = Serializer.SanitizeString(filename);
        if (filename == "")
            filename = StoryboardManager.Instance.savename;

        StartCoroutine(StoryboardManager.Instance.ReloadInstance(
            Serializer.LoadModule(filename)));

        Serializer.LoadFromQSTN();
    }

    #region Localisation
    //Build and save and Excel Spreadsheet to pathName
    private void BuildExcel()
    {
        using (SpreadsheetDocument spreadsheetDocument =
            SpreadsheetDocument.Create(
                Serializer.GetLocalisationPath($"{module.text}Localisation")
                , SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = BuildWorkBookPart(spreadsheetDocument);
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

            int id = 1;
            //Create a sheet in workbook for each scenario
            foreach (var header in storyboard.scenarios)
            {
                string nameofWorksheet = header.scenario.name;
                //Build New WorkPartSheet
                var worksheetPart = BuildSheetFromScenario(workbookPart, header.scenario);
                //Add Sheet to Sheets using worksheetPart's ID, and Scenario's name
                Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = (uint)id++, Name = nameofWorksheet };
                sheets.Append(sheet);
            }
        }
    }
    //Build the Workbook and its styling
    private WorkbookPart BuildWorkBookPart(SpreadsheetDocument spreadsheetDocument)
    {
        // Add a WorkbookPart to the document.
        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        //Add stylesheet per spreadsheet
        //Start formatting
        WorkbookStylesPart stylesheet = spreadsheetDocument.WorkbookPart.AddNewPart<WorkbookStylesPart>();
        Stylesheet workbookstylesheet = new Stylesheet();

        // <Fonts>
        DocumentFormat.OpenXml.Spreadsheet.Font font0 = new DocumentFormat.OpenXml.Spreadsheet.Font();// Default font
        Fonts fonts = new Fonts();// <APPENDING Fonts>
        fonts.Append(font0);

        // <Fills>
        Fill fill0 = new Fill();// Default fill
        Fills fills = new Fills();// <APPENDING Fills>
        fills.Append(fill0);

        // <Borders>
        Border border0 = new Border();// Default border
        Borders borders = new Borders();// <APPENDING Borders>
        borders.Append(border0);

        // <CellFormats>
        CellFormat cellformat0 = new CellFormat()// Default style : Mandatory
        {
            FontId = 0,
            FillId = 0,
            BorderId = 0
        };
        CellFormat cellformat1 = new CellFormat(new Alignment()
        {
            WrapText = true
        });// Style with textwrap set

        // <APPENDING CellFormats>
        CellFormats cellformats = new CellFormats();
        cellformats.Append(cellformat0);
        cellformats.Append(cellformat1);

        // Append FONTS, FILLS , BORDERS & CellFormats to stylesheet <Preserve the ORDER>
        workbookstylesheet.Append(fonts);
        workbookstylesheet.Append(fills);
        workbookstylesheet.Append(borders);
        workbookstylesheet.Append(cellformats);

        // Finalize
        stylesheet.Stylesheet = workbookstylesheet;
        stylesheet.Stylesheet.Save();
        //End formatting
        return workbookPart;
    }
    //Build a new sheet for the scenario
    private WorksheetPart BuildSheetFromScenario(WorkbookPart workbookPart, ScenarioBoard scenario)
    {

        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        Worksheet worksheet = new Worksheet();
        SheetData data = new SheetData();
        Columns columns = new Columns();

        //Initialise Columns
        var column1 = new Column()
        { Max = 1, Min = 1, CustomWidth = true, Width = 256/3 };
        var column2 = new Column()
        { Max = 2, Min = 2, CustomWidth = true, Width = 256 };
        columns.Append(column1);
        columns.Append(column2);
        workbookPart.Workbook.Save();

        //Initialise First Row ("Key", "English")
        Row row = new Row() { };
        Cell keyHeaderCell = new Cell
        {
            //CellReference = "A1",
            CellValue = new CellValue("Key"),
            DataType = CellValues.String,
            StyleIndex = Convert.ToUInt32(1),
        };
        Cell languageHeaderCell = new Cell
        {
            //CellReference = "A1",
            CellValue = new CellValue("English"),
            DataType = CellValues.String,
            StyleIndex = Convert.ToUInt32(1),
        };

        row.AppendChild(keyHeaderCell);
        row.AppendChild(languageHeaderCell);
        data.AppendChild(row);

        //Populate SheetData with all of a scenario's entries
        var localisations = BuildScenarioLocalisations(scenario);
        PopulateSheetLocalisation(data, localisations);

        //Add data to worksheet and worksheet to worksheetpart
        worksheet.Append(columns);
        worksheet.AppendChild(data);
        worksheetPart.Worksheet = worksheet;
        return worksheetPart;
    }
    //Populates the cells of a sheet with the formatted localisation keys and values
    private void PopulateSheetLocalisation(SheetData data, SortedDictionary<string, string> localisation)
    {
        //Add each localisation entry to data
        foreach (var entry in localisation)
        {
            //declare new row
            var row = new Row();
            //Load key and value into new cells
            Cell keyCell = new Cell
            {
                CellValue = new CellValue(entry.Key),
                DataType = CellValues.String,
                StyleIndex = Convert.ToUInt32(1),
            };
            Cell valueCell = new Cell
            {
                CellValue = new CellValue(entry.Value),
                DataType = CellValues.String,
                StyleIndex = Convert.ToUInt32(1),
            };
            //add cells to row, then row to data
            row.AppendChild(keyCell);
            row.AppendChild(valueCell);
            data.AppendChild(row);
        }
    }
    //Builds a SortedDictionary from a scenario's tasks
    private SortedDictionary<string, string> BuildScenarioLocalisations(ScenarioBoard scenario)
    {
        var localisation = new SortedDictionary<string, string>();
        //step through each TaskList in a scenario
        foreach (var tasklist in scenario.subLists)
        {
            //Title for List
           AddSortedString(localisation, $"{scenario.name}.TaskBar.{tasklist.name}.Title",
                $"{tasklist.title}");
            //step through each task in that tasklist
            foreach (var task in tasklist.subTasks)
            {
                ///Example: Scenario1.Taskbar.TaskList.TestTask = Title
                //Add a localisation for the task to Taskbar
               AddSortedString(localisation, $"{scenario.name}.TaskBar.{tasklist.name}.{task.name}",
                    $"{task.Editor.task.title}");
                //Message
                if (task.Editor.task is Message m)
                {
                    ///Example: Scenario1.Messages.TestTask = Message
                    //Message
                   AddSortedString(localisation, $"{scenario.name}.Messages.{task.name}",
                        $"{m.message}");

                    ///Example: Scenario1.Messages.Titles.TestTask = Test Task
                    //Title
                   AddSortedString(localisation, $"{scenario.name}.Messages.Titles.{task.name}",
                        $"{m.title}");
                }
                //Question
                else if (task.Editor.task is Question q)
                {
                    string type = q.qType.ToString();
                    ///Example: Scenario1.QDManager.MultiChoicePanel.TestTask.Title = Test Task
                    //Title
                   AddSortedString(localisation, $"{scenario.name}.QDManager.{type}.{task.name}.Title",
                        $"{q.title}");

                    ///Example: Scenario1.QDManager.MultiChoicePanel.TestTask.Question = Question
                    //Question
                   AddSortedString(localisation, $"{scenario.name}.QDManager.{type}.{task.name}.Question",
                        $"{q.question}");

                    ///Example: Scenario1.QDManager.MultiChoicePanel.TestTask.Options.0 = Answers[0]
                    //Answers
                    for (int i = 0; i < q.answers.Count; i++)
                    {
                        AddSortedString(localisation, $"{scenario.name}.QDManager.{type}.{task.name}.Options.{i.ToString()}",
                            $"{q.answers[i]}");
                    }
                    ///Example: Scenario1.QDManager.MultiChoicePanel.TestTask.CorrectAnswer = CorrectMessage
                    //AnswerFeedback
                   AddSortedString(localisation, $"{scenario.name}.QDManager.{type}.{task.name}.CorrectAnswer",
                        $"{q.correctMessage}");
                   AddSortedString(localisation, $"{scenario.name}.QDManager.{type}.{task.name}.IncorrectAnswer",
                        $"{q.incorrectMessage}");
                   AddSortedString(localisation, $"{scenario.name}.QDManager.{type}.{task.name}.PartialAnswer",
                        $"{q.partialcorrectMessage}");
                }
                //Feedback
                if (task.Editor.task.scored)
                {
                    ///Example: Scenario1.Feedback.Tasklist.TestTask.Title = Test Task
                    //Title
                   AddSortedString(localisation, $"{scenario.name}.Feedback.{tasklist.name}.{task.name}.Title",
                        $"{task.Editor.task.title}");

                    ///Example: Scenario1.Feedback.Tasklist.TestTask.Correct = correctMessage
                    //Messages
                   AddSortedString(localisation, $"{scenario.name}.Feedback.{tasklist.name}.{task.name}.Correct",
                        $"{task.Editor.task.feedback.correctMessage}");
                   AddSortedString(localisation, $"{scenario.name}.Feedback.{tasklist.name}.{task.name}.Incorrect",
                        $"{task.Editor.task.feedback.incorrectMessage}");
                   AddSortedString(localisation, $"{scenario.name}.Feedback.{tasklist.name}.{task.name}.partial",
                        $"{task.Editor.task.feedback.partialcorrectMessage}");

                }
            }
        }
        return localisation;
    }

    private void AddSortedString(SortedDictionary<string, string> localisations, string key, string value)
    {
        try
        {
            localisations.Add(key, value);
        }
        catch /*(ArgumentException e)*/
        {
            Debug.LogWarning($"Tried to add an already existing key: {key}", this);
        }
    }

    #endregion

    #region ScenarioTaskList
    //Build a ScenarioTaskList.task file for each scenario in storyboard
    private void BuildScenarioTaskLists()
    {
        foreach(var header in StoryboardManager.Instance.scenarios)
        {
            //copy and covert data
            var scenarioList = new ScenarioList();
            scenarioList.taskName = header.name;
            scenarioList.tasks = new List<ScenarioTask>();
            foreach (var groupList in header.scenario.subLists)
            {
                scenarioList.tasks.Add(BuildTaskList(groupList));
            }
            //create a file for each scenario
            Serializer.SaveScenarioTaskList(scenarioList, module.text + header.name);
        }
    }

    //Convert TaskListHeaders to ScenarioLists
    private ScenarioList BuildTaskList(TaskListHeader taskList)
    {
        var scenarioList = new ScenarioList();
        scenarioList.taskName = taskList.title;
        scenarioList.tasks = new List<ScenarioTask>();
        scenarioList.listType = taskList.type;
        //fill out scenariotasks from tasklists subtasks
        foreach (var task in taskList.subTasks)
        {
            var scenarioAction = new ScenarioAction();
            scenarioAction.taskName = task.Editor.task.title;
            scenarioAction.isQuestion = task.Editor.task is Question;
            scenarioAction.isScoreable = task.Editor.task.scored;
            scenarioList.tasks.Add(scenarioAction);
        }
        return scenarioList;
    }

    #endregion

    #region Question
    //Save a list of questions from each scenario in storyboard
    private void BuildQuestions()
    {
        foreach (var header in StoryboardManager.Instance.scenarios)
        {
            //copy and covert data
            var questions = new List<QDImporter>();
            foreach (var groupList in header.scenario.subLists)
            {
                //load any questions from each tasklist
                foreach (var task in groupList.subTasks)
                {
                    if (task.Editor.task is Question q)
                    {
                        questions.Add(BuildQuestion(q));
                    }
                }
            }
            //create a file for each scenario
            Serializer.SaveQuestions(questions, module.text + header.name);
        }
    }

    //Convert the data from Question into QDContent
    private QDImporter BuildQuestion(Question q)
    {
        //Question id and type
        var question = new QDImporter();
        question.id = Serializer.SanitizeString(q.title);
        question.qdType = q.qType;
        for (int i = 0; i < q.answers.Count; i++)
        {
            //Answer Data
            var option = new QDImportAnswer();
            option.correctOption = q.correctAnswers[i];

            question.qdOptions.Add(option);
        }
        return question;
    }

    #endregion
}