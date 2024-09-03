using System.Collections;
using UnityEngine;

public class MuzzleLightFeedback : FeedBack
{
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private float _turnOnTime = 0.08f;
    [SerializeField] private bool _defaultState = false;

    public override void CreateFeedBack()
    {
        StartCoroutine(ActiveCoroutine());
    }

    private IEnumerator ActiveCoroutine()
    {
        _muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(_turnOnTime);
        _muzzleFlash.SetActive(false);
    }

    public override void CompletePrevFeedBack()
    {
        StopAllCoroutines();
        _muzzleFlash.SetActive(_defaultState);
    }

}
