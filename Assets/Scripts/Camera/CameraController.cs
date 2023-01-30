using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float smoothSpeed;

  //  [SerializeField] private float minX, maxX, minY, maxY;


    // Start is called before the first frame update
   private void Start()
    {
     //   target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
   private void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

       transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, transform.position.z), smoothSpeed * Time.deltaTime);

      //  transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX),
           //                              Mathf.Clamp(transform.position.y, minY, maxY),
           //                                          transform.position.z);
    }
}
