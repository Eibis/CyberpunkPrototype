// Author: Matteo Bevan
using UnityEngine;
using System.Collections;
using ComplexMath;

public class CyberspaceFractal : MonoBehaviour 
{
    Texture2D texture;
    int textureSize = 512;

	void Start () 
    {
        texture = new Texture2D(512, 512, TextureFormat.ARGB32, false);

        GetComponent<Renderer>().material.mainTexture = texture;

        StartCoroutine("fractalAnimation");
	}

    IEnumerator fractalAnimation()
    {
        int max = 14;
        int i = 1;

        while (true)
        {
            if (i > max)
                i = 1;

            yield return StartCoroutine(MBrot(i));

            i++;
            
    //        yield return new WaitForSeconds(1.1f);
        }
    }

    public IEnumerator MBrot(int pow)
    {
        float epsilon = 0.005f; // The step size across the X and Y axis
        float x;
        float y;

        int min = -2;
        int max = 2;

        int maxIterations = 20; // increasing this will give you a more detailed fractal

        Complex Z;
        Complex C;
        int iterations;
        for (x = min; x <= max; x += epsilon)
        {
            for (y = min; y <= max; y += epsilon)
            {
                iterations = 0;
                C = new Complex(x, y);
                Z = new Complex(0, 0);

                while (Complex.Abs(Z) < max && iterations < maxIterations)
                {
                    for (int i = 0; i < pow; i++)
                        Z = Z * Z;
                    
                    Z += C;
                    iterations++;
                }

                Random.seed = iterations + 0;
                float r = Random.value;

                Random.seed = iterations + 1;
                float g = Random.value;

                Random.seed = iterations + 2;
                float b = Random.value;
                
                Color color = new Color(r, g, b, 1f);

                texture.SetPixel((int)((x + Mathf.Abs(min)) * textureSize / (max + Mathf.Abs(min))),
                                  (int)((y + Mathf.Abs(min)) * textureSize / (max + Mathf.Abs(min))), 
                                  color); // depending on the number of iterations, color a pixel.
            }

        }
       
        yield return null;
       
        texture.Apply();
    }
}
