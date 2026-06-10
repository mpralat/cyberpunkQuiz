using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ZXing;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    private const int QUESTIONS_TO_DRAW = 8;

    private static bool initialized = false;

    // Start Panel
    public GameObject StartPanel;
    public Button StartButton;

    // Main Panel
    public GameObject MainPanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextAsset questionsJson;
    public CanvasGroup questionPanelGroup;
    private string MainPanelBackground = "background1";

	// Gender Question Panel
	public GameObject GenderQuestionPanel;
    public Button mButton;
    public Button fButton;
    public Button nButton;

    // Result Panel
    public GameObject resultPanel;
    public Image resultImage;
    public TextMeshProUGUI resultText;
    public bool ShowDescription = false;
    public Image DescriptionBackground;
    public Button ShowDescriptionButton;
    public TextMeshProUGUI SpiritDescription;
    public Sprite ShowCharacterSprite;
    public Sprite ShowDescriptionSprite;

    // Data
    private QuestionList loadedQuestions;
    private List<Question> questions;
    private int currentQuestionIndex = 0;

    private ScoreManager scoreManager;

    bool isProcessing = false;
    Color selectedColor      = new Color(0.77f, 0.61f, 0.28f, 1f);
    Color defaultButtonColor = new Color(1f, 1f, 1f, 1f);
    Color defaultTextColor   = new Color(0.2f, 0.15f, 0.1f, 1f);
    float deselectedAlpha    = 0.7f;

    void Start()
    {
        if (!initialized)
        {
            initialized = true;
            loadedQuestions = JsonUtility.FromJson<QuestionList>(questionsJson.text);

            scoreManager = new ScoreManager();
            scoreManager.LoadClasses();
            DescriptionBackground.enabled = ShowDescription;
            
            ShowDescriptionButton.onClick.RemoveAllListeners();
            ShowDescriptionButton.onClick.AddListener(() => ToggleDescription());

            StartButton.onClick.RemoveAllListeners();
            StartButton.onClick.AddListener(() => StartGame());
        }

        ChooseQuestions();
        currentQuestionIndex = 0;
        scoreManager.ResetPoints();
        DescriptionBackground.enabled = false;

        MainPanel.SetActive(true);
		GenderQuestionPanel.SetActive(false);
        resultPanel.SetActive(false);
        resultImage.enabled = true;
        SpiritDescription.enabled = false;
        ShowDescription = false;

        StartPanel.SetActive(true);
    }

    void StartGame()
    {
        StartPanel.SetActive(false);
        ShowQuestion();
    }

    void ChooseQuestions()
    {
        questions = loadedQuestions.questions
            .OrderBy(_ => UnityEngine.Random.value)
            .Take(QUESTIONS_TO_DRAW)
            .ToList();
    }
    
    void ShowQuestion()
    {
        ToggleQuestionBackground();
        if (currentQuestionIndex >= questions.Count)
        {
            OnNormalQuestionsFinished();
            return;
        }

        ResetButtons();
        isProcessing = false;

        Question q = questions[currentQuestionIndex];
        questionText.text = q.text;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.answers[idx].text;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(idx));
        }
    }

    void OnAnswerSelected(int index)
    {
        if (isProcessing) return;
        isProcessing = true;

        HighlightSelected(index);

        var answer = questions[currentQuestionIndex].answers[index];
        scoreManager.AddPoints(answer.characterClass);

        Invoke(nameof(NextQuestion), 0.4f);
    }

    void NextQuestion()
    {
        ResetButtons();
        currentQuestionIndex++;
        ShowQuestion();
        ClearHoverState();
        isProcessing = false;
    }
    
    void OnNormalQuestionsFinished()
    {
        List<string> tied = scoreManager.GetTiedClasses();

		Debug.Log(tied);
        if (tied.Count == 1)
        {
            scoreManager.CalculateCharacterClass();
	        ShowGenderQuestion();
        }
        else
        {
            ShowTiebreakerQuestion(tied);
        }
    }

    void ShowTiebreakerQuestion(List<string> tiedClasses)
    {
        ToggleQuestionBackground();
        ResetButtons();
        isProcessing = false;

        TiebreakerQuestion tbq = FindTiebreakerQuestion(tiedClasses);
        questionText.text = tbq.text;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < tbq.answers.Length)
            {
                string classForThisButton = tbq.answers[i].characterClass;
                var btn = answerButtons[i];
                btn.gameObject.SetActive(true);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = tbq.answers[i].text;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnTiebreakerSelected(classForThisButton));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnTiebreakerSelected(string characterClass)
    {
        if (isProcessing) return;
        isProcessing = true;

        foreach (var btn in answerButtons)
            btn.gameObject.SetActive(true);

        scoreManager.ForceClass(characterClass);
        scoreManager.CalculateCharacterClass();

        Invoke(nameof(ShowGenderQuestion), 0.4f);
    }

   TiebreakerQuestion FindTiebreakerQuestion(List<string> tiedClasses)
    {
        Debug.Log("FindTiebreakerQuestion");

        Debug.Log(string.Join(", ", tiedClasses));

        if (loadedQuestions.tiebreakerQuestions == null) return null;

        if (tiedClasses.Count == 2)
    {
        string classA = tiedClasses[0];
        string classB = tiedClasses[1];

        foreach (var tbq in loadedQuestions.tiebreakerQuestions)
        {
            bool hasA = tbq.answers.Any(a => a.characterClass == classA);
            bool hasB = tbq.answers.Any(a => a.characterClass == classB);
            if (hasA && hasB) return tbq;
        }
        return null;
    }

    if (tiedClasses.Count > 2)
    {
        var shuffled = tiedClasses.OrderBy(_ => Random.value).ToList();
        for (int i = 0; i < shuffled.Count - 1; i++)
        {
            var pair = new List<string> { shuffled[i], shuffled[i + 1] };
            var result = FindTiebreakerQuestion(pair);
            if (result != null) return result;
        }
        return null;
    }

    return null;
}
	void ShowGenderQuestion()
	{
    	MainPanel.SetActive(false);
    	GenderQuestionPanel.SetActive(true);
  
    	mButton.onClick.RemoveAllListeners();
    	mButton.onClick.AddListener(() => OnGenderSelected(ScoreManager.CharacterGender.Male));
    	fButton.onClick.RemoveAllListeners();
    	fButton.onClick.AddListener(() => OnGenderSelected(ScoreManager.CharacterGender.Female));
    	nButton.onClick.RemoveAllListeners();
    	nButton.onClick.AddListener(() => OnGenderSelected(ScoreManager.CharacterGender.NonBinary));

	}

	void OnGenderSelected(ScoreManager.CharacterGender gender)
	{
    	scoreManager.CurrentCharacterGender = gender;
        Invoke(nameof(ShowResult), 0.4f);
	}
    
    void ShowResult()
    {
        Debug.Log("ShowResult");
        MainPanel.SetActive(false);
    	GenderQuestionPanel.SetActive(false);
		
        CharacterClass resultCharacterClass = scoreManager.CurrentCharacterClass;
        resultPanel.SetActive(true);
        resultText.text = $"Twoim wynikiem jest {resultCharacterClass.Name}!";
        SpiritDescription.text = resultCharacterClass.Description;

		string fileName = scoreManager.GetFileName(resultCharacterClass.Class);
        Debug.Log(fileName == null ? "NIE ZNALEZIONO" : "OK");
        resultImage.sprite = Resources.Load<Sprite>($"Character/{fileName}");
    }
    
    public void ResetGame()
    {
        foreach (var btn in answerButtons)
            btn.gameObject.SetActive(true);

        ShowDescription = false;
       	ShowDescriptionButton.GetComponent<Image>().sprite = ShowDescriptionSprite;
        
        Start();
    }

    public void ToggleDescription()
    {
        ShowDescription = !ShowDescription;
        resultImage.enabled = !ShowDescription;
        SpiritDescription.enabled = ShowDescription;
        DescriptionBackground.enabled = ShowDescription;

        ShowDescriptionButton.GetComponent<Image>().sprite = 
            ShowDescription ? ShowCharacterSprite : ShowDescriptionSprite;
    }

    public void ToggleQuestionBackground()
    {
        if (MainPanelBackground == "background1")
        {
            MainPanel.GetComponent<Image>().sprite = Resources.Load<Sprite>("background2");
            MainPanelBackground = "background2";
        }
        else
        {
            MainPanel.GetComponent<Image>().sprite = Resources.Load<Sprite>("background1");
            MainPanelBackground = "background1";
        }
    }
    
    void ResetButtons()
    {
        foreach (var btn in answerButtons)
        {
            btn.image.color = defaultButtonColor;
            btn.GetComponentInChildren<TextMeshProUGUI>().color = defaultTextColor;
            btn.transform.localScale = new Vector3(0.89f, 0.89f, 0.89f);
            btn.interactable = true;
        }
    }

    void HighlightSelected(int index)
    {
        var chosen = answerButtons[index];
        chosen.image.color = selectedColor;
        chosen.GetComponentInChildren<TextMeshProUGUI>().color = defaultButtonColor;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i == index) continue;
            var c = answerButtons[i].image.color;
            c.a = deselectedAlpha;
            answerButtons[i].image.color = c;
            answerButtons[i].interactable = false;
        }
    }

    void ClearHoverState()
    {
        EventSystem.current.SetSelectedGameObject(null);
        var pointerData = new PointerEventData(EventSystem.current);
        foreach (var btn in answerButtons)
            ExecuteEvents.Execute<IPointerExitHandler>(
                btn.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
    }
}