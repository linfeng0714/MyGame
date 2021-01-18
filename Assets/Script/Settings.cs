using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    static Settings instance;
    [Header("Game Object References")]
    public Transform player;
    private void Awake()
    {
        if (instance != null || instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }
    // test git
    public static Vector3 GetPositionAroundPlayer(float radius)
    {
        Vector3 pos = instance.player.position;
        float angle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
        float s = Mathf.Sin(angle);
        float c = Mathf.Cos(angle);
        return new Vector3(c * radius, 1.1f, s * radius) + pos;
    }

    public static void PlayerDied()
    {
        if (instance.player == null)
            return;

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
