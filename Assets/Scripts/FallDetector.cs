using UnityEngine;
using UnityEngine.SceneManagement;
public class FallDetector : MonoBehaviour
{
    public float fallThreshold = -10.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < fallThreshold)
        {
            Restart();
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
