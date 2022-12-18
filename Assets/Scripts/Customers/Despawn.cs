using System.Collections;
using UnityEngine;

public class Despawn : MonoBehaviour
{
    [SerializeField] float time;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
