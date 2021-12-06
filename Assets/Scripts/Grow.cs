using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grow : MonoBehaviour
{
    public Sprite[] buddedSprites;
    public Sprite[] blossomedSprites;
    bool gotSunlight = false;
    bool gotRain = false;
    bool budded = false;
    bool blossomed = false;
    SpriteRenderer sr;
    AudioSource audioSource;
    public ParticleSystem sparkles;
    

    // Start is called before the first frame update
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {  
        if (GameObject.Find("Day")) {
            gotSunlight = true;
        }
        // if rained, then gotRain = true
        if (GameObject.Find("Rain")) {
            gotRain = true;
        }
        if (!budded && ((gotSunlight && !gotRain) || (!gotSunlight && gotRain))) {
            // make plant bud, but not bloom completely
            Sprite buddedSprite = buddedSprites[Random.Range(0, buddedSprites.Length)];
            sr.sprite = buddedSprite;
            audioSource.Play();
            sparkles.Play();
        }
        if (!blossomed && gotSunlight && gotRain) {
            // bloom flower completely
            Sprite blossomedSprite = blossomedSprites[Random.Range(0, blossomedSprites.Length)];
            sr.sprite = blossomedSprite;
            audioSource.Play();
            sparkles.Play();
        }
        budded = gotSunlight || gotRain;
        blossomed = gotSunlight && gotRain;
    }

    /*Sprite getRandomBuddedSprite() {

    }

    Sprite getRandomBlossomedSpirte() {

    }*/
}
