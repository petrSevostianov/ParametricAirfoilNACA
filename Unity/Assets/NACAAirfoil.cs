
using System;
using UnityEditor;
using UnityEngine;

public class NACAAirfoil : MonoBehaviour
{
    public int numPoints;
    [Range(0,1)]
    public float T = 0.1f;
    [Range(0, 1)]
    public float M = 0.05f;
    [Range(0, 1)]
    public float P = 0.4f;

    void Start()
    {
        
    }

    //https://en.wikipedia.org/wiki/NACA_airfoil


    public float Power = 1;

    public string AirfoilFrontSolidworks;
    public string AirfoilBackSolidworks;


    private static float k1 = 0.2969f*5;
    private static float k2 = -0.126f*5;
    private static float k3 = -0.3537f*5;
    private static float k4 = 0.2843f*5;
    private static float k5 = -0.1015f*5;

    private static class SymbolicSolidworks {
        static string Scale => "\"C@S\"";
        //static string T => "\"T\"";
        static string TScled => "\"T@S\"";
        static string P => $"\"P@S\"/{Scale}";

        static string MScled => "\"M@S\"";

        static string at => "sqrt(t*t)";
        static string tt => "t*t";

        public static string BaseScaled => $"{TScled} * t * ({k1} + {at} * ({k2} + {tt} * ({k3} + {tt} * ({k4} + {k5} * {tt}))))";

        public static string FrontCurveScaled = $"({MScled} / ({P} * {P}) * (2 * {P} * t*t - t*t*t*t))";
        public static string BackCurveScaled = $"{MScled} / ((1 - {P}) * (1 - {P})) * (1 - 2 * {P} + 2 * {P} * t*t - t*t*t*t)";
        

        public static string FrontY => $"{BaseScaled} + {FrontCurveScaled}";
        public static string BackY => $"{BaseScaled} + {BackCurveScaled}";



        /*
         var result = AirfoilBase(t);

        float pp = P * P;
        float omp = 1 - P;
        float m = M / (omp * omp);

        result.y += m * (1 - 2 * P + 2 * P * t*t - t*t*t*t);
         */

    }







    private Vector3[] Drawer(int numPoints, float from, float to, Func<float, Vector3> func) {
        var result = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++) {
            float n = (float)i / (numPoints - 1);
            float t = from + n * (to - from);
            result[i] = func(t);
        }
        return result;
    }

    public Func<float, Vector3> AirfoilBase => (t) => {
        float x = t * t;
        float at = Mathf.Sqrt(t * t);
        float tt = t * t;
        var y = T * t * (k1 + at * (k2 + tt * (k3 + tt * (k4 + k5 * tt))));
        return new Vector3(0, y, x);
    };

    public Func<float, Vector3> AirfoilFront => (t) => {
        var result = AirfoilBase(t);

        float mFront = M / (P * P);

        result.y += mFront * (2 * P * t*t - t*t*t*t);
        return result;
    };

    public Func<float, Vector3> AirfoilBack => (t) => {
        var result = AirfoilBase(t);

        float mBack = M / ((1 - P) * (1 - P));

        result.y += mBack * (1 - 2 * P + 2 * P * t*t - t*t*t*t);
        return result;
    };



    /*

    public Vector3[] AirfoilFront(int numPoints, float th, float m, float p, float sign = 1) {
        var result = new Vector3[numPoints];

        //float ic = 1 / c;
        float pp = p * p;
        float m1 = m / pp;
        float m2 = m / ((1 - p) * (1 - p));

        for (int i = 0; i < numPoints; i++) {

            float n = (float)i / (numPoints - 1) ;
            float t = (2 * n  - 1)* Mathf.Sqrt(p);
            
            float x = t * t;

            float sx = t;
            float xx = x * x;
            float xxx = xx * x;
            float xxxx = xx * xx;
            //float xic = x * ic;
            float at = Mathf.Sqrt(t * t);
            var y = 5 * th * t * (k1 + k2 * at + k3 * at * t * t + k4 * at * t * t * t * t + k5 * at * t * t * t * t * t * t);

            float yc = m1 * (2 * p * x - xx);
            if (x < p) {
                yc = m1 * (2 * p * x - xx);
            } else {
                yc = m2 * (1 - 2 * p + 2 * p * x - xx);
            }


            result[i] = new Vector3(0, y + yc, x);
        }
        return result;
    }*/





    public Vector3[] AirfoilReference(int numPoints, float th, float m, float p, float sign = 1) {
        var result = new Vector3[numPoints];
        float pp = p * p;
        float m1 = m / pp;
        float m2 = m / ((1-p) * (1-p));
        for (int i = 0; i < numPoints; i++) {
            float t = (float)i / (numPoints - 1);
            float x = t*t;

            float sx = t;
            float xx = x * x;
            float xxx = xx * x;
            float xxxx = xx * xx;
            var y = sign * 5 * th * (0.2969f * t - 0.126f * t*t - 0.3537f * t*t*t*t + 0.2843f * t*t*t*t*t*t - 0.1015f * t*t*t*t*t*t*t*t);

            float yc = 0;
            if (x < p) {
                yc = m1 * (2 * p * x - xx);
            } else {
                yc = m2 * (1 - 2 * p + 2 * p * x - xx);
            }


            result[i] = new Vector3(0, y+yc, x);
        }
        return result;
    }

    private void OnDrawGizmos() {
        Handles.color = Color.gray;
        Handles.DrawPolyLine(AirfoilReference(numPoints, T, M, P, 1));
        Handles.DrawPolyLine(AirfoilReference(numPoints, T, M, P, -1));
        Handles.color = Color.white;

        Handles.DrawLine(
            new Vector3(0, 0.5f * T + M, P),
            new Vector3(0, -0.5f * T + M,  P)
            );


        Handles.DrawPolyLine(Drawer(numPoints,-Mathf.Sqrt(P), Mathf.Sqrt(P), AirfoilFront));

        Handles.DrawPolyLine(Drawer(numPoints, Mathf.Sqrt(P), 1, AirfoilBack));
        Handles.DrawPolyLine(Drawer(numPoints, -Mathf.Sqrt(P), -1, AirfoilBack));
        /*foreach (var p in Airfoil(numPoints, t, m, p)) {
            Handles.DotCap(0, p, Quaternion.identity, 0.001f);
        }

        Handles.DrawPolyLine(Airfoil(numPoints, t, m, p, -1));
        Handles.color = Color.red;
        Handles.DrawPolyLine(AirfoilFront(numPoints, t, m, p));*/

        AirfoilFrontSolidworks = SymbolicSolidworks.FrontY;
        AirfoilBackSolidworks = SymbolicSolidworks.BackY;
    }

}
