using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

/// <summary>
///
/// </summary>
public class ROS2PublisherExample : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "pos_rot";

    // El Nombre del game object
    public GameObject cube;
 
    // Publica la posición y rotación del cubo cada N segundos
    public float publishMessageFrequency = 0.5f;

    // Permite determinar el tiempo transcurrido desde la publicación del último mensaje.
    private float timeElapsed;

    void Start()
    {
        // Iniciar la conexión ROS
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PosRotMsg>(topicName);
        cube = GameObject.Find("cube");
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            cube.transform.rotation = Random.rotation;

            PosRotMsg cubePos = new PosRotMsg(
                cube.transform.position.x,
                cube.transform.position.y,
                cube.transform.position.z,
                cube.transform.rotation.x,
                cube.transform.rotation.y,
                cube.transform.rotation.z,
                cube.transform.rotation.w
            );

            // Finalmente envía el mensaje a server_endpoint.py que se ejecuta en ROS
            ros.Publish(topicName, cubePos);

            timeElapsed = 0;
        }
    }
}

