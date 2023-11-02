using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform Player;
    public Transform Camera;
    public Vector3 Target;

    private Vector3 P2CDir;
    private Vector3 P2DDir;
    private Vector3 Nowdir;
    private float P2CDirLength;
    void Start()
    {
        Target = new Vector3(0, 0, 8);
        P2DDir = Target - Player.position;
        P2CDir = Camera.position - Player.position;
        Nowdir = P2CDir;
        P2CDirLength = P2CDir.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        Nowdir = Vector3.RotateTowards(Nowdir, P2DDir.normalized, Mathf.Deg2Rad * 10*Time.deltaTime, 0);
        Camera.position = Nowdir.normalized * P2CDirLength + Player.position;
    }
}
