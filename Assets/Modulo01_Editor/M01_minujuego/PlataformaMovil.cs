using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{


    [SerializeField] private float velocidad;
    [SerializeField] private float distancia;
    private Vector3 posicionInicial;
    private float direccion = 1f;
    [SerializeField] bool movEje;


    public void Mover()
    {
        transform.position += Vector3.right * velocidad * Time.deltaTime * direccion;
        float recorrido;
        if (movEje == true)
        {
            recorrido = transform.position.x - posicionInicial.x;
        }
        else
        {
            recorrido = transform.position.y - posicionInicial.y;
        }

        if (recorrido >= distancia)
        {
            direccion = -1f;
        }
        else if (recorrido <= 0f)
        {
            direccion = 1f;
        }

    }
    void Start()
    {
        posicionInicial = transform.localPosition;
    }


    void Update()
    {
        Mover();
    }
}
