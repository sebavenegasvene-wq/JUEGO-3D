using UnityEngine;

public class Rotador : MonoBehaviour
{
    void Update()
    {
        // Rota el elemento una cantidad diferente en cada dirección y en cada intervalo de tiempo
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}