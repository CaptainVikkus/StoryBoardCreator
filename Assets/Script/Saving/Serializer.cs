using MXRClasses;
using Newtonsoft.Json;
using Storyboard;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//Static class for Serializing Storyboard data and sanitizing/formatting strings for IO
public static class Serializer
{
    //Remove and chars that would interfere with IO
    public static string SanitizeString(string value)
    {
        char[] banned = { ' ', '.', ',', (char)39 /*apostrophe*/,
            ';', '?', '/', '{', '}'};
        string[] cleaner = value.Split(banned);
        value = string.Join("", cleaner);
        return value;
    }

    public static string GetSavePath(string filename)
    {
        return Path.Combine($"{Application.dataPath}/Save", $"{SanitizeString(filename)}.sav");
    }
    public static string GetLocalisationPath(string filename)
    {
        return Path.Combine($"{Application.dataPath}/Save", $"{SanitizeString(filename)}.xlsx");
    }
    public static string GetScenarioTaskListPath(string filename)
    {
        return Path.Combine($"{Application.dataPath}/Save/Scenarios", $"{SanitizeString(filename)}.task");
    }
    public static string GetQuestionPath(string filename)
    {
        return Path.Combine($"{Application.dataPath}/Save/Scenarios", $"{SanitizeString(filename)}.qstn");
    }

    //Check for the file at given fullpath
    public static bool SaveExists(string fullpath)
    {
        Debug.Log($"Checking for Savedata at: {Path.GetFullPath(fullpath)}");
        try
        {
            bool file = File.Exists(Path.GetFullPath(fullpath));
            return file;
        }
        catch
        {
            return false;
        }
    }

    //Save a module to binary at filename.sav
    public static void SaveModule(StoryboardManager module, string filename)
    {
        //Binary
        var bf = new BinaryFormatter();
        var data = module.SaveInstance();
        Directory.CreateDirectory($"{Application.dataPath}/Save");
        var file = File.Create(GetSavePath(filename));
        bf.Serialize(file, data);
        file.Close();

        Debug.Log($"Saved Module to {GetSavePath(filename)}");
    }
    //Try to load from the save path
    public static Module LoadModule(string filename)
    {
        try
        {
            var bf = new BinaryFormatter();
            var file = File.Open(GetSavePath(filename), FileMode.Open);
            var interaction = (Module)bf.Deserialize(file);
            file.Close();
            return interaction;
        }
        catch (SerializationException)
        { Debug.LogError("Save data failed to load"); }
        catch (FileNotFoundException)
        { Debug.Log("No file with that name exists"); }
        return null;
    }

    //Save a sudo-JSON file to /Save/save.json
    public static void SaveJSON(StoryboardManager module)
    {
        var data = module.SaveInstance();
        Directory.CreateDirectory($"{Application.dataPath}/Save");
        string output;
        //build string
        output = "{\n";
        //scenarios
        for (int i = 0; i < data.scenarios.Count; i++)
        {
            output += "Scenario " + i.ToString() + "\n\t{\n";
            //tasklists
            foreach (var list in data.scenarios[i])
            {
                output += "\t" + list.title + "\n";
                output += "\t" + ((ListTaskType)list.orderType).ToString();
                output += "\n\t\t{\n";
                //tasks
                foreach (var task in list.interactions)
                {
                    output += "\t\t\t" + JsonUtility.ToJson(task) + "\n";
                }
                output += "\t\t}\n";
            }
            output += "\t}\n";
        }
        output += "}";

        File.WriteAllText(Path.Combine($"{Application.dataPath}/Save", $"save.json"), output);
    }

    public static void SaveScenarioTaskList(ScenarioList scenario, string filename)
    {
        var bf = new BinaryFormatter();
        Directory.CreateDirectory($"{Application.dataPath}/Save/Scenarios");
        var file = File.Create(GetScenarioTaskListPath(filename));
        bf.Serialize(file, scenario);
        file.Close();
    }

    public static ScenarioList LoadScenarioTaskList(string filename)
    {
        try
        {
            var bf = new BinaryFormatter();
            var file = File.Open(GetScenarioTaskListPath(filename), FileMode.Open);
            var interaction = (ScenarioList)bf.Deserialize(file);
            file.Close();
            Debug.Log($"Loaded Scenario: {interaction}");
            return interaction;
        }
        catch (SerializationException)
        {
            Debug.LogError("scenario data failed to load");
        }
        return null;
    }

    public static void SaveQuestions(List<QDImporter> questions, string filename)
    {
        Directory.CreateDirectory($"{Application.dataPath}/Save/Scenarios");
        var js = new JsonSerializer();
        js.Formatting = Formatting.Indented;
        using (var sw = new StreamWriter(GetQuestionPath(filename)))
        using (var writer = new JsonTextWriter(sw))
        {
            js.Serialize(writer, questions);
        }
    }

    public static void LoadFromQSTN()
    {
        //Locate File
        var js = new JsonSerializer();
        js.Formatting = Formatting.Indented;
        string pathName = GetQuestionPath("Scenario1");
        if (!pathName.Contains(".qstn")) return;

        using (var sr = new StreamReader(pathName))
        using (var reader = new JsonTextReader(sr))
        {
            var questions = (List<QDImporter>)js.Deserialize(reader, typeof(List<QDImporter>));
            Debug.Log(questions.ToString());
        }
    }

}
