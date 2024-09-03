using UnityEngine;

public abstract class FeedBack : MonoBehaviour
{
    public abstract void CreateFeedBack();
    public abstract void CompletePrevFeedBack();

    protected virtual void OnDestroy()
    {
        CompletePrevFeedBack();
    }

    protected virtual void OnDisable()
    {
        CompletePrevFeedBack();
    }


}
