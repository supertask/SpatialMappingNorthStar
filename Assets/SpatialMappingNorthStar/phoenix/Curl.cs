using UnityEngine;

public static class Curl
{
    private static float snoise(Vector3 v)
    {
        return SimplexNoise.Noise.Generate(v.x, v.y, v.z);
    }

    public static Vector3 snoiseVec3(Vector3 x)
    {
        float s = snoise(x);
        float s1 = snoise(new Vector3(x.y - 19.1f, x.z + 33.4f, x.x + 47.2f));
        float s2 = snoise(new Vector3(x.z + 74.2f, x.x - 124.5f, x.y + 99.4f));
        return new Vector3(s, s1, s2);
    }

    //Simplex Curl Noise
    public static Vector3 Noise(Vector3 p)
    {
        const float e = 0.001f;
        Vector3 dx = new Vector3(e, 0.0f, 0.0f);
        Vector3 dy = new Vector3(0.0f, e, 0.0f);
        Vector3 dz = new Vector3(0.0f, 0.0f, e);

        Vector3 p_x0 = snoiseVec3(p - dx);
        Vector3 p_x1 = snoiseVec3(p + dx);
        Vector3 p_y0 = snoiseVec3(p - dy);
        Vector3 p_y1 = snoiseVec3(p + dy);
        Vector3 p_z0 = snoiseVec3(p - dz);
        Vector3 p_z1 = snoiseVec3(p + dz);

        float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
        float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
        float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

        const float divisor = 1.0f / (2.0f * e);
        return Vector3.Normalize(new Vector3(x, y, z) * divisor);
    }
}