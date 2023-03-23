using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.Core;

using System.Collections;



//[RequireComponent(typeof(ROSClockSubscriber))]
public class ROS2_Camera : MonoBehaviour
{

    ROSConnection ros;
    public string imageTopic = "/camera_robot/front";
     
    //public string CompressedImageTopic = "/camera_rect/image_rect_compressed";
 
    public Camera camera_robot;
 
    public bool compressed = false;
 
    public float pubMsgFrequency = 30f;
 
    private float timeElapsed;
    private RenderTexture renderTexture;
    private RenderTexture lastTexture;
 
    private Texture2D mainCameraTexture;
    private Rect frame;
 
 
    private int frame_width;
    private int frame_height;
    private const int isBigEndian = 0;
    private uint image_step = 4;
    TimeMsg lastTime;
 
    //private ROSClockSubscriber clock;
 
    private ImageMsg img_msg;
    private CameraInfoMsg infoCamera;
 
    private HeaderMsg header;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(imageTopic);


        if (!camera_robot)
        {
            camera_robot = Camera.main;

        }
 
        if (camera_robot)
        {
            renderTexture = new RenderTexture(camera_robot.pixelWidth, camera_robot.pixelHeight, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            renderTexture.Create();
 
            frame_width = renderTexture.width;
            frame_height = renderTexture.height;
 
            frame = new Rect(0, 0, frame_width, frame_height);
 
            mainCameraTexture = new Texture2D(frame_width, frame_height, TextureFormat.RGBA32, false);
            
            header = new HeaderMsg();
 
            img_msg = new ImageMsg();
 
            img_msg.width = (uint) frame_width;
            img_msg.height = (uint) frame_height;
            img_msg.step = image_step * (uint) frame_width;
            img_msg.encoding = "rgba8";
        }

    }

    private void Update()
    {
        if (camera_robot)
        {
            timeElapsed += Time.deltaTime;
 
            if (timeElapsed > (1 / pubMsgFrequency))
            {
                //header.stamp = clock._time;
                header.stamp.sec =( int)(System.DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds;
                header.stamp.nanosec = (uint)(System.DateTime.UtcNow - new DateTime(1970,1,1)).Milliseconds*1000*1000;
                //infoCamera.header = header;
 
                img_msg.header = header;
                img_msg.data = get_frame_raw();
           
                ros.Publish(imageTopic, img_msg);
                //ros.Send(camInfoTopic, infoCamera);
                //Debug.Log("Send Img");
                timeElapsed = 0;
            }
        }
        else
        {
            Debug.Log("No camera found.");
        }
 
    }
 
    /// <summary>
    // https://gist.github.com/mminer/816ff2b8a9599a9dd342e553d189e03f
    /// Vertically flips a render texture in-place.
    /// </summary>
    /// <param name="target">Render texture to flip.</param>
    /*public var VerticallyFlipRenderTexture(RenderTexture target)
    {
        var temp = RenderTexture.GetTemporary(target.descriptor);
        Graphics.Blit(target, temp, new Vector2(1, -1), new Vector2(0, 1));
        Graphics.Blit(temp, target);
        //RenderTexture.ReleaseTemporary(temp);
        return temp;
    }*/

    private byte[] get_frame_raw()
    {    
          
        camera_robot.targetTexture = renderTexture;
        lastTexture = RenderTexture.active;
       
        RenderTexture.active = renderTexture;
        camera_robot.Render();
 
        mainCameraTexture.ReadPixels(frame, 0, 0);
        mainCameraTexture.Apply();
 
        camera_robot.targetTexture = lastTexture;
 
        camera_robot.targetTexture = null;
 
        return mainCameraTexture.GetRawTextureData();;
    }
}