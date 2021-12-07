using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using Assets.Scripts;
using TMPro;
using UnityEngine;

public class FindColor : MonoBehaviour
{
    [SerializeField]
    bool hasCalculated;

    //[SerializeField]
    //MeshRenderer resultRenderer;
    public Transform userPlant;
    public Transform ground;
    private bool potDetected;
    HashSet<float> availablePositions = new HashSet<float>();

    [SerializeField]
    float colorGate = 0.1f;

    // [SerializeField]
    // bool useHueLight;

    [SerializeField]
    Color averageColor;

    // [SerializeField]
    // HueSettings hueSettings;

    // [SerializeField]
    // string hueLightName;

    private Color currentAverageColor;

    private Camera colourCamera;
    private RenderTexture renderTexture;
    // private HueLightHelper hueLightHelper;
    private WaitForSeconds shortPause = new WaitForSeconds(0.2f);
    private WaitForSeconds mediumPause = new WaitForSeconds(0.5f);

    private System.Random random = new System.Random();


    // Start is called before the first frame update
    void Start()
    {
        this.colourCamera = GetComponent<Camera>();
        this.renderTexture = this.colourCamera.targetTexture;
        // populate available positions for flowers
        float pos = -7f;
        while (pos < 7f)
        {
            availablePositions.Add(pos);
            pos += 1.5f;
        }
        // this.hueLightHelper = new HueLightHelper(hueSettings);
        // if (useHueLight)
        // {
        //     this.hueLightHelper.Connected = () => { hueLightHelper.ChangeLight(hueLightName, this.averageColor).ConfigureAwait(continueOnCapturedContext: false); };
        //     this.hueLightHelper.Connect().ConfigureAwait(false);
        // }
    }

    // TODO: Consider using a CommandBuffer rather than PreRender as camera setup may lag
    private void OnPreRender()
    {
        if (!this.hasCalculated)
        {
            StartCoroutine(FindAverageColor());

        }
        Debug.Log("Current color: " + this.averageColor);
        bool isCurrPot = IsPot(this.averageColor);
        bool isCurrBackground = IsBackground(this.averageColor);
        if (!potDetected && isCurrPot)
        {
            // Instantiate plant in random position
            // Choose at 1.5 intervals
            List<float> availableList = availablePositions.ToList();
            float xPos = availableList[random.Next(availableList.Count)];
            availablePositions.Remove(xPos);
            float yPos = Random.Range(-0.3f, 1f);
            Vector3 pos = new Vector3(xPos, yPos, 0.65f);
            Debug.Log("Instantiate at position: " + pos);
            Transform newPlant = Instantiate(userPlant, ground);
            newPlant.localPosition = pos;
            potDetected = true;
        }
        else if (potDetected && isCurrBackground)
        {
            potDetected = false;
        }

        //if (this.resultRenderer != null)
        //{
        //    this.resultRenderer.material.color = this.averageColor;
        //}
    }

    private bool IsPot(Color color)
    {
        // Samples of pots
        Color[] pots = new Color[] {
            new Color(0.778f, 0.572f, 0.328f),
            new Color(0.833f, 0.542f, 0.275f)
        };
        // Check if our current color is close to any of the pots
        for (int i = 0; i < pots.Length; i++)
        {
            Color pot = pots[i];
            float rDiff = Mathf.Abs(color.r - pot.r);
            float bDiff = Mathf.Abs(color.b - pot.b);
            float gDiff = Mathf.Abs(color.g - pot.g);
            if (rDiff <= 0.1 && bDiff <= 0.1 && gDiff <= 0.1)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBackground(Color color)
    {
        float red = 0.211f;
        float green = 0.224f;
        float blue = 0.231f;
        float rDiff = Mathf.Abs(color.r - red);
        float bDiff = Mathf.Abs(color.b - blue);
        float gDiff = Mathf.Abs(color.g - green);
        if (rDiff <= 0.05 && bDiff <= 0.05 && gDiff <= 0.05)
        {
            return true;
        }
        return false;
    }


    // private async void OnApplicationQuit()
    // {
    //     if (this.hueLightHelper.IsConnected)
    //     {
    //         await this.hueLightHelper.TurnOff().ConfigureAwait(false);
    //     }
    // }


    private IEnumerator FindAverageColor()
    {
        while (!this.hasCalculated)
        {
            this.hasCalculated = true;
            Texture2D tex2d = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, mipChain: false);

            RenderTexture.active = renderTexture;
            tex2d.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex2d.Apply();

            var detectorX = renderTexture.width;
            var detectorY = renderTexture.height;
            var colours = tex2d.GetPixels(0, 0, renderTexture.width, renderTexture.height);

            var averageColor = AverageWeightedColor(colours);
            if (HasAnAverageColour(averageColor))
            {
                this.averageColor = averageColor;
                if (!currentAverageColor.Compare(this.averageColor))
                {
                    this.currentAverageColor = this.averageColor;
                    // if (hueLightHelper.IsConnected)
                    // {
                    //     hueLightHelper.ChangeLight(hueLightName, this.averageColor).ConfigureAwait(continueOnCapturedContext: false);
                    // }
                }

                yield return this.shortPause;
                this.hasCalculated = false;
            }
            else
            {
                print("No average colour");
                this.hasCalculated = false;
                yield return this.mediumPause;
            }
        }
    }

    private static bool HasAnAverageColour(Color averageColor)
    {
        return averageColor.r + averageColor.g + averageColor.b > 0;
    }

    private Color AverageWeightedColor(Color[] colors)
    {
        var total = 0;
        var r = 0f; var g = 0f; var b = 0f;
        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].r + colors[i].g + colors[i].b > colorGate)
            {
                r += colors[i].r > colorGate ? colors[i].r : 0f;
                g += colors[i].g > colorGate ? colors[i].g : 0f;
                b += colors[i].b > colorGate ? colors[i].b : 0f;
                total++;
            }
        }
        return new Color(r / total, g / total, b / total, 1);
    }

}
