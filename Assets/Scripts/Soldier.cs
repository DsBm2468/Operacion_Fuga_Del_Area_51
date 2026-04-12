using UnityEngine;

public class Soldier : MonoBehaviour
{
    //[Header("Soldier Settings")]
    //public float energy = 10;

    [Header("Soldier Settings")]
    public bool isAlive = true;

    void Start()
    {
        
    }

    public void Simulate(float h)
    {
        // Ajuste principal de los estados y acciones del soldado

        if (!isAlive) return; // Si el soldado no está vivo entonces no hace nada
    }
}
