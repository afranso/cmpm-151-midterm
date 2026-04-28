using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//************** use UnityOSC namespace...
using UnityOSC;
//*************

public class MovePlayer : MonoBehaviour {

	public float speed;
	public Text countText;

	private Rigidbody rb;
	private int count;

  private bool onGround = true;
  private bool wasOnGround = false;

	//************* Need to setup this server dictionary...
	Dictionary<string, ServerLog> servers = new Dictionary<string, ServerLog> ();
	//*************

	// Use this for initialization
	void Start () 
	{
		Application.runInBackground = true; //allows unity to update when not in focus

		//************* Instantiate the OSC Handler...
	    OSCHandler.Instance.Init ();
        OSCHandler.Instance.SendMessageToClient("pd", "/unity/TriangleChannel", "ready");
		OSCHandler.Instance.SendMessageToClient("pd", "/unity/PulseWave1", "ready");
        //*************


        rb = GetComponent<Rigidbody> ();
		count = 0;
		setCountText (0);
	}
	

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space) && onGround)
    {
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        onGround = false;
    }
  }


	void FixedUpdate()
	{
		OSCHandler.Instance.SendMessageToClient ("pd", "/unity/TriangleChannel", Math.Abs(transform.position.x) * 100);

		float moveHorizontal = Input.GetAxis ("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");


		Vector3 movement = new Vector3 (moveHorizontal, 0, moveVertical);

		rb.AddForce (movement*speed);

		//************* Routine for receiving the OSC...
		OSCHandler.Instance.UpdateLogs();
		Dictionary<string, ServerLog> servers = new Dictionary<string, ServerLog>();
		servers = OSCHandler.Instance.Servers;

		foreach (KeyValuePair<string, ServerLog> item in servers) {
			// If we have received at least one packet,
			// show the last received from the log in the Debug console
			if (item.Value.log.Count > 0) {
				int lastPacketIndex = item.Value.packets.Count - 1;

				//get address and data packet
				countText.text = item.Value.packets [lastPacketIndex].Address.ToString ();
				countText.text += item.Value.packets [lastPacketIndex].Data [0].ToString ();

			}
		}
		//*************

    if (!wasOnGround && onGround)
    {
      OSCHandler.Instance.SendMessageToClient ("pd", "/unity/NoiseChannel", count);
    }

    wasOnGround = onGround;
	}
		

	void OnTriggerEnter(Collider other) 
    {
		if (other.gameObject.CompareTag ("Pick Up")) 
		{
			other.gameObject.SetActive (false);
			count = count + 1;
			setCountText (1);
		} else if (other.gameObject.CompareTag ("Pick Up 2")) 
		{
			other.gameObject.SetActive (false);
			count = count + 1;
			setCountText (2);
		}
	}

  void OnCollisionStay(Collision collision)
  {
    if (collision.gameObject.CompareTag("Ground"))
    {
      onGround = true;
    }
  }

  void OnCollisionExit(Collision collision)
  {
    if (collision.gameObject.CompareTag("Ground"))
    {
      onGround = false;
    }
  }

	void setCountText(int type)
	{
    countText.text = "Count: " + count.ToString();

    //************* Send the message to the client...	
    if (type == 1) OSCHandler.Instance.SendMessageToClient ("pd", "/unity/PulseWave1", count);
    if (type == 2) OSCHandler.Instance.SendMessageToClient ("pd", "/unity/PulseWave2", count);
    //*************
  }
		
}
