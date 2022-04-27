using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI_Controller : MonoBehaviour
{
    private CanvasGroup cg;
    private bool visible = true;
    private float defaultAlphaLevel = 0.75f;

    [Header("UI Variables")]
    [Range(0.001f, 0.01f)][SerializeField] private float alphaIncreaseValue = 0.01f;
    [SerializeField] private string prevScene;
    [SerializeField] private string nextScene;

    [Header("Character Switching")]
    [SerializeField] private bool enableCharacterSwap;
    [SerializeField] private GameObject char1, char2;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();

        if (prevScene == "" || nextScene == "")
        {
            Destroy(GameObject.Find("PriorScene"));
            Destroy(GameObject.Find("NextScene"));
        }
        
        if (!enableCharacterSwap)
        {
            Destroy(GameObject.Find("Action 2"));
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            ChangeChar();
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            HideUI();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            ReloadScene();
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if (prevScene != "") PreviousScene();
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (nextScene != "") NextScene();
        }
        else if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }

    private void LateUpdate()
    {
        if (visible && cg.alpha < defaultAlphaLevel)
        {
            cg.alpha += alphaIncreaseValue;
            cg.interactable = true;
        }
        else if (!visible && cg.alpha > 0)
        {
            cg.alpha -= alphaIncreaseValue;
            cg.interactable = false;
        }
    }

    public void HideUI() => visible = !visible;

    public void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void PreviousScene() => SceneManager.LoadScene(prevScene);

    public void NextScene() => SceneManager.LoadScene(nextScene);

    public void ChangeChar()
    {
        if (enableCharacterSwap)
        {
            if (char1.activeSelf)
            {
                char1.SetActive(false);
                char2.SetActive(true);
            }
            else
            {
                char2.SetActive(false);
                char1.SetActive(true);
            }
        }
    }
}
