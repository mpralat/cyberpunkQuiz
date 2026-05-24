using UnityEngine;

[System.Serializable]
public class Question
{
    public string text;
    public Answer[] answers;
}

[System.Serializable]
public class Answer
{
    public string text;
    public string characterClass;
}

[System.Serializable]
public class QuestionList
{
    public Question[] questions;
    public TiebreakerQuestion[] tiebreakerQuestions;
}

[System.Serializable]
public class TiebreakerQuestion
{
    public string text;
    public TiebreakerAnswer[] answers;
}

[System.Serializable]
public class TiebreakerAnswer
{
    public string text;
    public string characterClass;
}