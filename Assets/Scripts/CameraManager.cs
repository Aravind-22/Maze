using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    private float yPos = 5f, zPos = -2f;

    void Awake()
    {
        instance = this;
    }
    
    public void SetCamPos(int level)
    {
        this.transform.position = new Vector3(0f , yPos + (level - 1) , zPos - (level - 1) * 0.3f);
    }
}
