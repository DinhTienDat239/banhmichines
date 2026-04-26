using DAT.Core.DesignPatterns;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public void LoadScene(string sn)
    {
        if (string.IsNullOrWhiteSpace(sn))
        {
            return;
        }

        StartCoroutine(LoadSceneIE(sn));
    }

    private IEnumerator LoadSceneIE(string sn)
    {
        Time.timeScale = 1f;

        Scene current = SceneManager.GetActiveScene();
        if (current.IsValid() && current.isLoaded)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(current);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }

        yield return Resources.UnloadUnusedAssets();

        // Single mode đảm bảo scene cũ được clean nếu còn sót gì đó.
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sn, LoadSceneMode.Single);
        if (loadOp != null)
        {
            while (!loadOp.isDone)
            {
                yield return null;
            }
        }
    }
}
