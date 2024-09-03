using System.Collections.Generic;
using UnityEngine;

public class FeedbackPlayer : MonoBehaviour
{
    private List<FeedBack> _feedbackToPlay = null;

    private void Awake()
    {
        _feedbackToPlay = new List<FeedBack>();
        GetComponents<FeedBack>(_feedbackToPlay);
    }

    public void PlayFeedback()
    {
        FinishFeedBack();
        foreach (FeedBack f in _feedbackToPlay)
        {
            f.CreateFeedBack();
        }
    }

    public void FinishFeedBack()
    {
        foreach (FeedBack f in _feedbackToPlay)
        {
            f.CompletePrevFeedBack();
        }
    }

}
