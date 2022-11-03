using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SceneLoading
{
    internal class LoadingScreen : MonoBehaviour
    {
        [SerializeField] Slider loadingBar;
        [SerializeField] TMP_Text label;
        [SerializeField] CanvasGroup group;
        [SerializeField] float fadeTime;

        void Awake()
        {
            loadingBar.minValue = 0;
            loadingBar.maxValue = 1;
            group.alpha = 0;
        }

        public IEnumerator WaitToShow() => Show(true);

        public void Activate(AsyncOperation[] ops) => StartCoroutine(DisplayProgress(ops));

        IEnumerator DisplayProgress(AsyncOperation[] operations)
        {
            var total = operations.Length * 0.9f;
            
            while (operations.Any(op => !op.isDone))
            {
                var progress = operations.Sum(op => op.progress) / total;
                SetProgress(progress);
                yield return null;
            }

            yield return Show(false);
        }

        void SetProgress(float progress)
        {
            loadingBar.value = progress;
            label.text = progress.ToString("P0");
        }

        IEnumerator Show(bool show)
        {
            group.blocksRaycasts = show;
            SetProgress(show ? 0 : 1);
            yield return Fade(show ? 1 : 0);
        }

        IEnumerator Fade(float to)
        {
            var start = group.alpha;
            
            float t = 0;
            while (t < 1)
            {
                group.alpha = Mathf.Lerp(start, to, t);
                t += Time.deltaTime / fadeTime;
                yield return null;
            }

            group.alpha = to;
        }
    }
}