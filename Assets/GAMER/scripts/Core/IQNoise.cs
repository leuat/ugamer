using UnityEngine;
using System.Collections;
using LemonSpawn;

namespace LemonSpawn
{


    /* Translated from Shader code */
    public class IQNoise
    {

        float PI = Mathf.PI;




        static public float clamp(float a, float b, float c)
        {
            return Mathf.Clamp(a, b, c);
        }

        static public float pow(float a, float b)
        {
            return Mathf.Pow(a, b);
        }

        static public float frac(float a)
        {
            return a - Mathf.Floor(a);
        }

        /*  static public double frac(double a)
          {
              return a - System.Math.Floor(a);
          }
          */
        static public Vector3 frac(Vector3 a)
        {
            return new Vector3(a.x - Mathf.Floor(a.x), a.y - Mathf.Floor(a.y), a.z - Mathf.Floor(a.z));
        }

        static public Vector3 normalize(Vector3 a)
        {
            return a.normalized;
        }

        static public Vector3 cross(Vector3 a, Vector3 b)
        {
            return Vector3.Cross(a, b);
        }

        static public float length(Vector3 a)
        {
            return a.magnitude;
        }

        static public float floor(float a)
        {
            return Mathf.Floor(a);
        }

        static public Vector3 floor(Vector3 a)
        {
            return new Vector3(Mathf.Floor(a.x), Mathf.Floor(a.y), Mathf.Floor(a.z));
        }
        static public float sin(float a)
        {
            return Mathf.Sin(a);
        }

        static public float cos(float a)
        {
            return Mathf.Cos(a);
        }

        static public float abs(float a)
        {
            return Mathf.Abs(a);
        }



        static float iqhashStatic(float n)
        {
            return (float)frac(Mathf.Sin(n) * 753.5453123f);
        }


        static float lerp(float a, float b, float w)
        {
            //return Mathf.Lerp(a,b,c);
            return a + w * (b - a);

        }



        public static float noise(Vector3 x)
        {
            // The noise function returns a value in the range -1.0f -> 1.0f
            Vector3 p = floor(x);
            Vector3 f = frac(x);

            f.x = f.x * f.x * (3.0f - 2.0f * f.x);
            f.y = f.y * f.y * (3.0f - 2.0f * f.y);
            f.z = f.z * f.z * (3.0f - 2.0f * f.z);


            float n = (p.x + p.y * 157.0f + 113.0f * p.z);

            return lerp(lerp(lerp(iqhashStatic(n + 0.0f), iqhashStatic(n + 1.0f), f.x),
                lerp(iqhashStatic(n + 157.0f), iqhashStatic(n + 158.0f), f.x), f.y),
                lerp(lerp(iqhashStatic(n + 113.0f), iqhashStatic(n + 114.0f), f.x),
                    lerp(iqhashStatic(n + 270.0f), iqhashStatic(n + 271.0f), f.x), f.y), f.z);


        }


        static public float getStandardPerlin(Vector3 pos, float scale, float power, float sub, int N)
        {
            float n = 0;
            float A = 0;
            float ms = scale;
            Vector3 shift = new Vector3(0.123f, 2.314f, 0.6243f);

            for (int i = 1; i <= N; i++)
            {
                float f = pow(2, i) * 1.0293f;
                float amp = (2 * pow(i, power));
                n += noise(pos * f * ms + shift * f) / amp;
                A += 1 / amp;
            }

            float v = clamp(n - sub * A, 0, 1);
            return v;

        }

        public static float octave_noise_3d(float octaves, float persistence, float scale, float x, float y, float z)
        {
            float total = 0;
            float frequency = scale;
            float amplitude = 1;

            // We have to keep track of the largest possible amplitude,
            // because each octave adds more, and we need a value in [-1, 1].
            float maxAmplitude = 0;

            Vector3 n = Vector3.zero;

            for (int i = 0; i < octaves; i++)
            {
                n.Set(x* frequency, y* frequency, z* frequency);
                total += noise(n) * amplitude;

                frequency *= 2;
                maxAmplitude += amplitude;
                amplitude *= persistence;
            }

            return total / maxAmplitude;
        }

        float getMultiFractal(Vector3 p, float frequency, int octaves, float lacunarity, float offs, float gain, float initialO)
        {

            float value = 0.0f;
            float weight = 1.0f;

            Vector3 vt = p * frequency;
            for (float octave = 0; octave < octaves; octave++)
            {
                float signal = initialO + noise(vt);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);

                // Make the ridges.
                signal = abs(signal);
                signal = offs - signal;


                signal *= signal;

                signal *= weight;
                weight = signal * gain;
                weight = clamp(weight, 0, 1);

                value += (signal * 1);
                vt = vt * lacunarity;
                frequency *= lacunarity;
            }
            return value;
        }






    }
}