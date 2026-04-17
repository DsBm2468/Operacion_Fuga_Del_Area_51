using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Alien : MonoBehaviour
{
    [Header("Alien Settings")]
    public float health = 150f; // Porcentaje de vida inicial del alien
    public float speed = 4f;
    public float visionRange = 5f;
    public float damage = 10f;
    public float currentDamage;
    public static int escapedAliensCounter = 0; // Contador de aliens que escaparon con éxito, inicialmente será 0

    [Header("Alien visual")]
    public Sprite spriteNormal;
    public Sprite spriteAction;
    private SpriteRenderer sr;

    [Header("Alien States")]
    public bool isAlive = true; // Estado actual
    public static bool callAlien = false; // Llamado del alien si encuentra un obstáculo, inicialmente no lo está
    public static Vector3 positionCall;

    private Vector3 destination; // Punto exacto del mapa hacia donde se dirigue el alien
    private float h; // h es el tiempo que dura cada paso de la simulación, esto viene de SimulationManager.secondsPerIteration =1f;
    
    private GameObject spaceShip; // Llamado al objetivo de los aliens en la simulación, llegar a la nave
    private GameObject area51; // Llamado al lugar donde los aliens iniciarán la simulación, dentro del área 51

    private void Start()
    {
        // Interacciones iniciales del alien en la simulación

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = spriteNormal;

        spaceShip = GameObject.FindGameObjectWithTag("SpaceShip"); // Busca la nave en la escena por su nombre (etiqueta)
        area51 = GameObject.FindGameObjectWithTag("Area51"); // Busca el área 51 en la escena por su nombre (etiqueta)

        // Aparecerán los aliens de forma aleatoria en el área 51
        if (area51 != null) // Si hay área 51...
        {
            Bounds bounds = area51.GetComponent<Collider2D>().bounds; // Se dan los límites del área

            // Luego se le asigna un punto al azar dentro del área al alien
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);

            transform.position = new Vector3(randomX, randomY, 0); // Finalmente es ubicado el alien en ese punto
        }

        // El objetivo de los aliens en la simulación es llegar a la nave para escapar
        if (spaceShip != null) // Si hay nave...
        {
            destination = spaceShip.transform.position; // Entonces el alien va hacia allá
        }
        else // Si no, entonces se queda quieto en su puesto de inicio
        {
            destination = transform.position;
        }
    }

    public void Simulate(float h) 
    {
        // Ajuste principal de los estados y acciones del alien
        
        if (!isAlive) return; // Si el alien no está vivo entonces no hace nada
        
        this.h = h; // Luego se registra cuanto tiempo pasó

        GoToObjetive(); // Instantáneamente iniciará su objetivo

        Move();
        CheckState();

    }

    void GoToObjetive()
    {
        GameObject obstacle = ScanPerimeter(); // Llama al escaner para corroborar que la zona esté vacia

        if (obstacle != null) // Si hay algún obstáculo (amenaza)...
        {
            LaserTower tower = obstacle.GetComponent<LaserTower>();
            if (tower != null && !tower.isStable)
            {
                Debug.Log("Esta torre ya fue destruida.");
                obstacle = null; // Forzamos a que ignore la torre muerta
            }
        }

        if (obstacle != null) // Si hay algún obstáculo (amenaza)...
        {
            Debug.Log("<color=yellow> Obstáculo detectado, Modo ataque activado. Analizando situación...</color>");
            Alert(obstacle); // Entonces el alien se pondrá en modo alerta
            callAlien = true; // El llamado es activado
            positionCall = transform.position; // Registra la posición actual del alien (si mismo)
        }

        else if (callAlien) // Si no ve nada pero algún otro alien pidió refuerzos...
        {
            destination = positionCall; // Se dirigue a la posición del llamado
            Debug.Log("<color=yellow> Se detectó un llamado de su misma especie. Moviéndose a zona de combate.</color>");

            if (Vector3.Distance(transform.position, destination) < 0.5f) // Si el soldado está cerca de su destino actual (zona de donde vino el llamado)...
            // Se pregunta si esta a menos 0.5 (este es el margen de error) para saber si ya llegó a ese punto
            {
                callAlien = false;  // Se apaga la señal
            }
        }
        else // Si no, entonces continuará con su objetivo que es escapar en la nave
        {
            Scaping();
        }
    }

    GameObject ScanPerimeter()
    {
        Collider2D obstacleHit = Physics2D.OverlapCircle(transform.position, visionRange, LayerMask.GetMask("Obstacles")); // Funciona como radar para detectar si hay algún obstáculo en el área

        if (obstacleHit != null) // Si en la zona se detectó algo...
        {
            Debug.Log("<color=yellow> Amenaza detectada </color>");
            return obstacleHit.gameObject; // Entonces entra el objeto encontrado (en este punto aún no se determina que es exactamente)
        }   
        else // Si no, entonces no hay nada alrededor
        { 
            Debug.Log("La zona está despejada");
            return null; 
        } 
    }

    void Alert(GameObject obstacle) // Se pone agresivo en el caso de encontrar obstáculos o amenazas para su meta
    {
        int closeAliens = countCloseAliens();
        
        if (closeAliens > 1) // Revisa cuantos aliados tiene el alien cerca, si hay aunque sea uno...
        {
            GroupAttack(obstacle);
        }
        else
        {
            SingleAttack(obstacle);
        }

        //if (obstacle.CompareTag("LaserTower"))
        //{
        //    AttackTower(obstacle);
        //}
        //else if (obstacle.CompareTag("Soldier"))
        //{
        //    AttackSoldier(obstacle);
        //}
    }

    int countCloseAliens()
    {
        // Detectar los aliados cercanos, en estas cuentas tambien se detecta a si mismo
        Collider2D[] founderAllies = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Aliens"));

        return founderAllies.Length;
    }

    void GroupAttack(GameObject obstacle)
    {
        // El daño provocado depende de la cantidad de aliados
        int totalAliensAtacking = countCloseAliens();
        currentDamage = damage * totalAliensAtacking;

        Debug.Log(totalAliensAtacking + "aliens están atacando. Daño total provocado = " + currentDamage);

        // Se define cual es la amenaza
        if (obstacle.CompareTag("LaserTower"))
        {
            AttackTower(obstacle);
        }
        else if (obstacle.CompareTag("Soldier"))
        {
            AttackSoldier(obstacle);
        }
    }

    void SingleAttack(GameObject obstacle)
    {
        // Daño provocado por un solo alien
        currentDamage = damage;
        Debug.Log("Alien ataca individualmente. Daño realizado = " + currentDamage);

        // Se define cual es la amenaza
        if (obstacle.CompareTag("LaserTower"))
        {
            AttackTower(obstacle);
        }
        else if (obstacle.CompareTag("Soldier"))
        {
            AttackSoldier(obstacle);
        }
    }

    void AttackTower(GameObject obstacle)
    {
        // Se dirigue(n) a la torre para destruirla
        sr.sprite = spriteAction;
        destination = obstacle.transform.position;

        //Si el alien está lo suficientemente cerca de la distancia de combate...
        if (Vector3.Distance(transform.position, destination) < 1.5f)
        // Se pregunta si esta a menos 1.5 (este es el margen de error, se usa este valor ya que de esta forma lo detecta mejor, 0.5 es el centro del collider)
        // para saber si ya llegó a ese punto
        {
            // Entonces le quita resistencia a la torre

            // Se crea una variable temporal para acceder a la información de salud actual del soldado, de esta manera se define hasta que momento se atacará
            LaserTower scriptTower = obstacle.GetComponent<LaserTower>();

            if (scriptTower != null && scriptTower.isStable)
            {
                scriptTower.TakeDamageTower(currentDamage); // Se llama la función que detecta el estado de la torre
            }
        }
    }

    void AttackSoldier(GameObject obstacle)
    {
        // Se dirigue(n) al soldado para matarlo
        sr.sprite = spriteAction;
        destination = obstacle.transform.position;

        //Si el alien está lo suficientemente cerca de la distancia de combate...
        if (Vector3.Distance(transform.position, destination) < 1.5f)
        // Se pregunta si esta a menos 1.5 (este es el margen de error, se usa este valor ya que de esta forma lo detecta mejor, 0.5 es el centro del collider)
        // para saber si ya llegó a ese punto
        {
            // Entonces le quita vida al soldado

            // Se crea una variable temporal para acceder a la información de salud actual del soldado, de esta manera se define hasta que momento se atacará
            Soldier scriptSoldier = obstacle.GetComponent<Soldier>();
            
            if (scriptSoldier != null && scriptSoldier.isAlive)
            {
                scriptSoldier.TakeDamageSoldier(currentDamage); // Se llama la función que detecta el estado del soldado
            }
        }
    }

    public void TakeDamage(float quantify)
    {
        // Cuando el alien es atacado, va reduciendo la cantidad de vida

        health -= quantify; // quantify es quien recibe el número de vida que el soldado le quita
        Debug.Log("<color=orange> Salud del alien: </color>" + health);
    }

    void Scaping()
    {
        // Deben los aliens llegar a la nave para escapar, ese es su objetivo
        sr.sprite = spriteNormal;
        destination = spaceShip.transform.position; // Entonces se dirigirán a ella
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
        );
    }

    void CheckState()
    {
        // Estado actual del alien

        if (health <= 0) // Si el estado de salud del alien es menor a 0...
        {
            isAlive = false; // El alien fue eliminado
            Debug.Log("<color=red> ALIEN ELIMINADO</color>");
            Destroy(gameObject);
        }

        // Se utiliza el collider de la nave para saber si el alien ya está dentro de ella
        Collider2D shipCollider = spaceShip.GetComponent<Collider2D>();

        if (shipCollider.OverlapPoint(transform.position) && isAlive) // Si el alien llegó a la nave en buen estado...
        {
            isAlive = false; // Se desactiva sus estados para que permanezca estático en su puesto nuevo dentro de la nave, además esto hace que el contador solo cuente una vez
            escapedAliensCounter++; // Se incrementa el contador de fugados
            Debug.Log("<color=green> Fuga exitosa. Alien abordo de la nave!! Total de fugados: </color>" + escapedAliensCounter);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vista del alien representada en scene

        // Área despejada
        Gizmos.color = Color.green; 
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Obstáculo (Amenaza) detectada
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(destination, 0.2f);

        // Indicación de dirección en la que va
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }
}
