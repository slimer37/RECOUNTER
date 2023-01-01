using System.Collections.Generic;
using UnityEngine;

public class Box : Interactable
{
    [Header("Items")]
    [SerializeField] int _capacity;

    [Header("Level Indicator")]
    [SerializeField] Transform _levelIndicator;
    [SerializeField] float _lowestY;
    [SerializeField] float _highestY;
    [SerializeField] float _noiseAmplitude;
    [SerializeField] Vector3 _randomRotation;
    [SerializeField] int _maxRotations;

    List<GameObject> _contents;

    void Awake()
    {
        _levelIndicator.gameObject.SetActive(false);
        _contents = new List<GameObject>();
    }

    protected override bool CanInteract(Employee e) =>
        _contents.Count < _capacity && e.RightHand.IsFull;

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = Icon.Hand,
                text = $"Store {e.RightHand.HeldObject.name}\n{_contents.Count}/{_capacity}"
            }
            : BlankHud;
    }

    protected override void OnInteract(Employee e)
    {
        e.ItemHotbar.TryRemoveActiveItem(out var item);

        var obj = item.gameObject;

        _contents.Add(obj);
        obj.SetActive(false);

        _levelIndicator.gameObject.SetActive(_contents.Count > 0);

        var filledAmount = (float)_contents.Count / _capacity;
        _levelIndicator.localPosition = Vector3.up * Mathf.Lerp(_lowestY, _highestY, filledAmount);
        _levelIndicator.localEulerAngles = _randomRotation * Random.Range(0, _maxRotations);
    }
}
