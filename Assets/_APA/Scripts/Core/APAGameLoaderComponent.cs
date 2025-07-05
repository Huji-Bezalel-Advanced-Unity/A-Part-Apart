using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _APA.Scripts
{
    public class APAGameLoaderComponent : MonoBehaviour
    {
        private void Start()
        {
            // var manager = new APAManager();
            // manager.LoadManagers(() =>
            // {
            //     int mainMenuIndex = 1;
            //     SceneManager.LoadScene(mainMenuIndex);
            // });
            var manager = APAManager.Instance;
            if (manager.EventManager != null)
            {
                int mainMenuIndex = 1;
                SceneManager.LoadScene(mainMenuIndex);
            }

        }
    }
}
