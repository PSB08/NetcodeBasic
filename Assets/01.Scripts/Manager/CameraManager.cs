using UnityEngine;

public class CameraManager
{
    private static CameraManager _instance;
    public static CameraManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CameraManager();
            }
            return _instance;
        }
    }

    private Camera _mainCam;

    public Camera MainCam
    {
        get
        {
            if (_mainCam == null)
            {
                _mainCam = Camera.main;
                Debug.Log(_mainCam);
            }
            return _mainCam;
        }
    }

    private CameraManager()
    {

    }

}
