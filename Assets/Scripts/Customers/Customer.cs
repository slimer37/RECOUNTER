using System.Collections;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [SerializeField] CustomerController controller;

    IEnumerator Start()
    {
        while (true)
        {
            controller.MoveTo(transform.position + (Vector3)Random.insideUnitCircle * 5);
            yield return new WaitForSeconds(1.0f);
        }
    }
}
