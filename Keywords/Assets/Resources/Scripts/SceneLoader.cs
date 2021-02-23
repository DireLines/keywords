using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [SerializeField]
    private Animator anim;
    [SerializeField]
    private TMP_Text loadingText;

    [SerializeField]
    private float transitionTime = 1f;

    private bool loading;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        loading = false;
        loadingText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public static void LoadSceneAsync(int buildIndex, LoadSceneMode mode = LoadSceneMode.Single)
    {
        instance.StartCoroutine(instance.LoadSceneCR(buildIndex, mode));
    }

    public static void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        LoadSceneAsync(SceneManager.GetSceneByName(sceneName).buildIndex, mode);
    }

    private IEnumerator LoadSceneCR(int buildIndex, LoadSceneMode mode)
    {
        loading = true;
        //StartCoroutine(LoadingTextCR());

        float speedMultiplier = 2 / (transitionTime);
        float waitTime = transitionTime / 2f;

        //print("SPEED:" + speedMultiplier);
        //print("WAIT TIME:" + waitTime);

        anim.SetFloat("speedMultiplier", speedMultiplier);
        anim.SetTrigger("start");
        yield return new WaitForSeconds(waitTime);

        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(buildIndex, mode);
        while (!asyncLoadLevel.isDone)
        {
            yield return null;
        }
        loading = false;
        anim.SetTrigger("stop");
        yield return new WaitForSeconds(waitTime);
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator LoadingTextCR()
    {
        loadingText.gameObject.SetActive(true);
        float t = 0f;
        Color endColor = loadingText.material.color;
        Color startColor = endColor;
        startColor.a = 0f;
        while (loading)
        {
            loadingText.material.SetColor("_Color", Color.Lerp(startColor, endColor, t));

            int elipsesCount = Mathf.FloorToInt(t * 3f) % 3 + 1;
            string text = "Loading";
            for (int i = 0; i < elipsesCount; i++)
            {
                text += ".";
            }
            loadingText.text = text;

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        loadingText.gameObject.SetActive(false);

    }
}
