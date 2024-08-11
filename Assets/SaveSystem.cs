using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string progressSave = Application.persistentDataPath + "/currentProgress.json";
    private static string unlockedLevels = Application.persistentDataPath + "/unlockedLevels.json";

    public static void SaveProgress(GameProgress progress)
    {
        string json = JsonUtility.ToJson(progress);
        File.WriteAllText(progressSave, json);
    }

    public static GameProgress LoadProgress()
    {
        if (File.Exists(progressSave))
        {
            string json = File.ReadAllText(progressSave);
            return JsonUtility.FromJson<GameProgress>(json);
        }
        else
        {
            // Return a new GameProgress instance with default values if no save file exists
            return new GameProgress();
        }
    }

    public static void UnlockLevel(int currentLevel)
    {
        UnlockedLevels unlocked;

        if (File.Exists(unlockedLevels))
        {
            string json = File.ReadAllText(unlockedLevels);
            unlocked = JsonUtility.FromJson<UnlockedLevels>(json);
        }
        else
        {
            unlocked = new UnlockedLevels { unlockedLevelIndices = new List<int>() };
        }

        if (!unlocked.unlockedLevelIndices.Contains(currentLevel))
        {
            unlocked.unlockedLevelIndices.Add(currentLevel);
            string json = JsonUtility.ToJson(unlocked);
            File.WriteAllText(unlockedLevels, json);
        }
    }

    public static bool IsUnlocked(int levelIndex)
    {
        if (File.Exists(unlockedLevels))
        {
            string json = File.ReadAllText(unlockedLevels);
            UnlockedLevels unlocked = JsonUtility.FromJson<UnlockedLevels>(json);
            return unlocked.unlockedLevelIndices.Contains(levelIndex);
        }
        return false;
    }
}

[System.Serializable]
public class UnlockedLevels
{
    public List<int> unlockedLevelIndices;
}
