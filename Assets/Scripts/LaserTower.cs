using UnityEngine;

public class LaserTower : MonoBehaviour
{
    //[Header("LaserTower Settings")]
    //public float energy = 10;

    [Header("LaserTower Settings")]
    public bool isStable = true;

    void Start()
    {
        
    }

    public void Simulate(float h)
    {
        // Ajuste principal de los estados y acciones de la torre láser (base)

        if (!isStable) return; // Si la torre láser fue destruida (no es estable) entonces no hace nada
    }
}
