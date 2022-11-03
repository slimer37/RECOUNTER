using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    void Awake() => text.text = Application.version;
}
