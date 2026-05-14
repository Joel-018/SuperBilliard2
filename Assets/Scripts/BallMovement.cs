using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource altavoz;
    public AudioClip sonidoAgujero;

    public float targetScale; // 1
    public float timeToReachTarget; // 2
    private float startScale;  // 3
    private float percentScaled; // 4
    private bool check = true; // 4

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        altavoz = GetComponent<AudioSource>();
        startScale = transform.localScale.x;
    }

    
    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Hole"))
        {
            if (otro.CompareTag("Wall"))
            {
                rb.isKinematic = true;
            }
          
            if (sonidoAgujero != null)
            {
                altavoz.PlayOneShot(sonidoAgujero);
            }

            while (check)
            {
                if (percentScaled < 1f) // 1
                {
                    percentScaled += Time.deltaTime / timeToReachTarget; // 2
                    float scale = Mathf.Lerp(startScale, targetScale, percentScaled); // 3
                    transform.localScale = new Vector3(scale, scale, scale); // 4
                }
                else
                {
                    check = false;
                }
            }
            

            Destroy(gameObject, targetScale);

        }
    }
}