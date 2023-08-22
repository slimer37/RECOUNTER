using TMPro;
using UnityEngine;

namespace Recounter.Store
{
    public class WelcomeBoard : MonoBehaviour
    {
        void Awake()
        {
            TryGetComponent<TMP_Text>(out var tmp);

            tmp.text = string.Format(tmp.text, GameManager.StoreData.name);
        }
    }
}
