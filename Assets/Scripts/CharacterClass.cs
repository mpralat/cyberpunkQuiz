using UnityEngine;
 
[System.Serializable]
public class CharacterClass
{
    public string Name;
    public string Description;
    public string Class;
}
 
[System.Serializable]
public class CharacterClassList
{
    public CharacterClass[] characterClasses;
}