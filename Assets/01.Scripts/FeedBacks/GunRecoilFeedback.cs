using DG.Tweening;
using UnityEngine;

public class GunRecoilFeedback : FeedBack
{
    [SerializeField] private Transform _targetTrm;
    [SerializeField] private float _recoilPower = 0.2f;

    public override void CreateFeedBack()
    {
        float current = _targetTrm.localPosition.y;

        Sequence seq = DOTween.Sequence();
        seq.Append(_targetTrm.DOLocalMoveY(current - _recoilPower, 0.1f));
        seq.Append(_targetTrm.DOLocalMoveY(current, 0.1f));
    }

    public override void CompletePrevFeedBack()
    {
        _targetTrm.DOComplete();
    }

}
