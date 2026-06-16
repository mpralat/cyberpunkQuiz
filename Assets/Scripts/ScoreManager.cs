using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ScoreManager
{
    private Dictionary<string, int> scores = new Dictionary<string, int>
    {
        { "nomada",     0 }, 
        { "fixer",      0 },
        { "rocker",     0 },
        { "korpo",      0 },
        { "solo",       0 },
        { "ripperdoc",  0 },
        { "netrunner",  0 },
        { "reporter",   0 },
    };

    public enum CharacterGender
    {
        Male,
        Female,
        NonBinary
    }

    private CharacterClass[] allClasses;

    #nullable enable
    private CharacterClass? _currentCharacterClass;
    #nullable disable
    #nullable enable
    private CharacterGender? _currentCharacterGender;
    #nullable disable

    public CharacterClass CurrentCharacterClass
    {
        get
        {
            if (_currentCharacterClass == null)
                throw new System.InvalidOperationException("CurrentCharacterClass is not set.");
            return _currentCharacterClass;
        }
        set { _currentCharacterClass = value; }
    }
    
    public CharacterGender CurrentCharacterGender
    {
        get
        {
            if (_currentCharacterGender == null)
                throw new System.InvalidOperationException("CurrentCharacterGender is not set.");
            return _currentCharacterGender.Value;
        }
        set { _currentCharacterGender = value; }
    }

    public void LoadClasses()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("characterClasses");
        CharacterClassList wrapper = JsonUtility.FromJson<CharacterClassList>(jsonText.text);
        allClasses = wrapper.characterClasses;
    }

    public void ResetPoints()
    {
        var keys = new List<string>(scores.Keys);
        foreach (var key in keys)
            scores[key] = 0;
        _currentCharacterClass = null;
    }

    public void AddPoints(string className, int amount = 1)
    {
        if (scores.ContainsKey(className))
            scores[className] += amount;
        else
            Debug.LogWarning($"Unknown class: {className}");
    }

    public List<string> GetTiedClasses()
    {
        if (scores.Count == 0) return new List<string>();
        int max = scores.Values.Max();
        return scores.Where(kv => kv.Value == max).Select(kv => kv.Key).ToList();
    }

    public void ForceClass(string className)
    {
        if (!scores.ContainsKey(className))
        {
            Debug.LogWarning($"ForceClass: unknown class '{className}'");
            return;
        }
        int max = scores.Values.Max();
        scores[className] = max + 100;
    }

    public void CalculateCharacterClass()
    {
        int maxScore = scores.Values.Max();
        var tiedKeys = scores.Where(kv => kv.Value == maxScore).Select(kv => kv.Key).ToList();

        string chosenClass = tiedKeys.OrderBy(_ => UnityEngine.Random.value).First();

        Debug.Log($"CalculateCharacterClass — chosen: {chosenClass}");
        foreach (var kv in scores)
            Debug.Log($"  {kv.Key}: {kv.Value}");

        CurrentCharacterClass = allClasses.First(c => c.Class == chosenClass);
        Debug.Log($"CharacterClass selected: {CurrentCharacterClass.Name}");
    }

    public string GetFileName(string className)
    {
        string suffix = _currentCharacterGender switch
        {
            CharacterGender.Female => "f",
            CharacterGender.Male => "m",
            CharacterGender.NonBinary => "n",
            _ => "m"
        };
        return $"{className}_{suffix}";
    }
}