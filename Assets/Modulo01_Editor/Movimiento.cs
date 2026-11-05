using UnityEngine;

public class Movimiento : MonoBehaviour
{

    [SerializeField] private float velocidad = 2f;


    void Update()
    {
        transform.position += Vector3.right * velocidad * Time.deltaTime;
    }
}
