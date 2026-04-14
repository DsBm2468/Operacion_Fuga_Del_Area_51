using Unity.VisualScripting;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    [Header("Soldier Settings")]
    public float health = 100f; // Porcentaje de vida inicial del alien
    public float speed = 2.5f;
    public float visionRange = 5f;
    public float damage = 13f;
    public float currentDamage;
    public int munition = 15; // Munición estándar del soldado, sin embargo, al inicio, la cantidad de munición será al azar (sin salirse de este rango)

    [Header("Soldier States")]
    public bool isAlive = true; // Estado actual
    public static bool HelpSignal = false; // Señal de auxilio si esta en peligro, inicialmente no lo está
    public static Vector3 positionHelp;

    private Vector3 destination; // Punto exacto del mapa hacia donde se dirigue el soldado
    private float h; // h es el tiempo que dura cada paso de la simulación, esto viene de SimulationManager.secondsPerIteration =1f;

    void Start()
    {
        // Inicialmente, la cantidad de munición del soldado será distribuida al azar para que así la batalla sea más equilibrada
        munition = Random.Range(5, 16);

        destination = transform.position; // Su posición inicial es estática
    }

    public void Simulate(float h)
    {
        // Ajuste principal de los estados y acciones del soldado

        if (!isAlive) return; // Si el soldado no está vivo entonces no hace nada

        this.h = h; // Luego se registra cuanto tiempo pasó

        CheckState(); // Instantáneamente revisarán su estado actual

        Move();
    }

    void CheckState()
    {
        // Estado actual del soldado

        if (health <= 0) // Si el estado de salud del soldado es menor a 0...
        {
            isAlive = false; // El soldado fue eliminado
            Debug.Log("<color=red> SOLDADO ELIMINADO</color>");
            Destroy(gameObject);
        }
        else if (health < 30) // Si el estado de salud del soldado es crítico (es menor a 30)...
        {
            TacticalRetreat(); // Realice retirada a la base para curarse
        }
        else
        {
            Alert();
        }
    }

    void Alert()
    {
        // El soldado está en modo alerta patrullando la zona

        if (Vector3.Distance(transform.position, destination) < 0.5f) // Si el soldado está cerca de su destino actual (patrullar cierta área)...
            // Se pregunta si esta a menos 0.5 (este es el margen de error) para saber si ya llegó a ese punto
        {
            // Entonces se indica un nuevo punto de destino en el mapa de forma aleatoria, sin salir del área central (Sin tocar la nave ni el área 51)
            float xRandom = Random.Range(-10f, 10f);
            float yRandom = Random.Range(-5f, 5f);

            destination = new Vector3(xRandom, yRandom, 0);
            Debug.Log("<color=green> Área patrullada libre de aliens. Reposicionándose... </color>");
        }

        // Durante el patrullaje, el soldado puede encontrar actividad sostechosa

        Collider2D alienHit = Physics2D.OverlapCircle(transform.position, visionRange, LayerMask.GetMask("Aliens")); // Funciona como radar (patrullaje) para detectar si hay algún alien en el área
        
        if (alienHit != null) // Si en la zona se detectó algo...
        {
            Debug.Log("<color=yellow> Extraterrestre detectado, Modo ataque activado. Analizando situación...</color>");
            CheckWeapons(alienHit.gameObject);
        }
        else // Si no, entonces no hay nada alrededor
        {
            Debug.Log("La zona está despejada");

            if (HelpSignal) // Si la señal de auxilio está activada...
            {
                destination = positionHelp; // Se dirigue a la posición del llamado
                Debug.Log("<color=yellow> Se detectó una señal de auxilio. Moviéndose a zona de combate.</color>");
            }
        }
    }

    void CheckWeapons(GameObject alienHit)
    {
        // Luego de detectar a un alien en la zona, el soldado revisará si tiene armamento disponible

        if (munition > 4) // Si está en un estado crítico (Menos de 4 balas)...
        {
            Debug.Log("<color=yellow> Munición suficiente. Verificar cantidad de aliens...</color>");
            NumberOfAliens(alienHit);
        }
        else
        {
            Debug.Log("<color=yellow> Munición insuficiente. Inicia retirada táctica para recargar</color>");
            TacticalRetreat();
        }
    }

    void NumberOfAliens(GameObject alienHit)
    {
        // Al ver que tiene municiones, el soldado identifica la situación en la que está

        Collider2D[] allAliensClose = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Aliens")); // Funciona como radar para identificar cuantos aliens hay cerca

        int aliensDetected = allAliensClose.Length;
        Debug.Log("Situación actual: " + aliensDetected + " aliens detectados en el área");

        if (aliensDetected > 1) // Si se detectan más de un alien...
        {
            HelpSignal = true; // La señaal es activada
            positionHelp = transform.position; // Registra la posición actual del soldado en peligro (si mismo)
            int closeSoldiers = CallBackup(); // Entonces el soldado llama refuerzos

            if (closeSoldiers > 1) // Revisa cuantos aliados tiene el soldado cerca, si hay aunque sea uno...
            {
                GroupCombat(alienHit);
            }
            else // Si no hay ningún aliado cerca entonces inicia retirada
            {
                TacticalRetreat();
            }
        }
        else
        {
            SingleCombat(alienHit);
        }

    }

    int CallBackup()
    {
        // Al notar que hay más de un alien, el soldado llama refuerzos cercanos, en estas cuentas tambien se detecta a si mismo
        Collider2D[] founderSoldiers = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Soldiers"));

        return founderSoldiers.Length;
    }

    public void TakeDamageSoldier(float quantifyDamage)
    {
        // Cuando el soldado es atacado, va reduciendo la cantidad de vida

        health -= quantifyDamage; // quantifyDamage es quien recibe el número de vida que el alien le quita
        Debug.Log("<color=orange> Salud del soldado: </color>" + health);
    }

    void SingleCombat(GameObject alienHit)
    {
        // luego de decidir que va a atacar por su cuenta, se dirigue al alien detectado
        destination = alienHit.transform.position;

        // Se crea una variable temporal para acceder a la información de salud actual del alien, de esta manera se define hasta que momento se atacará
        Alien scriptAlien = alienHit.GetComponent<Alien>();

        if (scriptAlien != null && scriptAlien.health > 0)
        {
            if (munition > 4)
            {
                scriptAlien.TakeDamage(damage); // Se le otorga los puntos de daño que el soldado hace
                munition--; // Gastando munición en el proceso
                Debug.Log("Combate en proceso... Munición restante: " + munition);
            }
            else // Si en pleno combate se queda con pocas balas (estado crítico)...
            {
                Debug.Log("<color=yellow> Se le acabaron las balas en pleno combate. Inicia Retirada táctica </color>");
                TacticalRetreat();
            }
        }
    }

    void GroupCombat(GameObject alienHit)
    {
        // luego de decidir que va a atacar por su cuenta, se dirigue al alien detectado
        destination = alienHit.transform.position;

        // Se crea una variable temporal para acceder a la información de salud actual del alien, de esta manera se define hasta que momento se atacará
        Alien scriptAlien = alienHit.GetComponent<Alien>();

        if (scriptAlien != null && scriptAlien.health > 0)
        {
            if (munition > 4)
            {
                // El daño provocado depende de la cantidad de aliados
                int totalSoldiersCombating = CallBackup();
                currentDamage = damage * totalSoldiersCombating;

                scriptAlien.TakeDamage(currentDamage); // Se le otorga los puntos de daño que el soldado hace
                munition--; // Gastando munición en el proceso
                Debug.Log("Combate en proceso... " + totalSoldiersCombating + " soldados están atacando." + "Munición restante: " + munition);
            }
            else // Si en pleno combate se queda con pocas balas (estado crítico)...
            {
                Debug.Log("Se le acabaron las balas en pleno combate. Inicia Retirada táctica");
                TacticalRetreat();
            }
        } else // Si el alien está muerto...
        {
            HelpSignal = false; // Se apaga la señal
        }
    }

    void TacticalRetreat()
    {
        // En momentos críticos el soldado se retira del combate

        GameObject[] towers = GameObject.FindGameObjectsWithTag("LaserTower"); // Primero se buscan las torres

        if (towers.Length > 0) // Se verifica que la torre sea estable, si lo es...
        {
            destination = towers[0].transform.position; // Se dirigue a la primera torre encontrada

            if (Vector3.Distance(transform.position, destination) < 0.5f) // Si el soldado está cerca de su destino actual (torre láser (base))...
            // Se pregunta si esta a menos 0.5 (este es el margen de error) para saber si ya llegó a ese punto
            {
                Debug.Log("Soldado esperando reabastecimiento en la torre.");
            }
        }
    }

    private void Move()
    {
        // Da el movimiento básico del soldado

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
        );
    }

    private void OnDrawGizmosSelected()
    {
        // Vista del soldado representada en scene

        // Área despejada
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Obstáculo (Amenaza) detectada
        Gizmos.color = Color.darkMagenta;
        Gizmos.DrawWireSphere(destination, 0.2f);

        // Indicación de dirección en la que va
        Gizmos.color = Color.orange;
        Gizmos.DrawLine(transform.position, destination);
    }
}
