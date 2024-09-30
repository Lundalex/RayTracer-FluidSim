using UnityEngine;

public static class TextureUnpacker
{
    /// <summary>
    /// Unpacks a Texture2D into a 2D array of Vector3.
    /// </summary>
    /// <param name="sourceTexture">The Texture2D to unpack.</param>
    /// <returns>A 2D array of Vector3 containing the unpacked data.</returns>3
    public static Vector3[,] UnpackTextureTo2DArray(Texture2D sourceTexture)
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        Vector3[,] normalsBuffer = new Vector3[width, height];

        // Get all pixel colors from the texture
        Color[] pixels = sourceTexture.GetPixels();

        // Loop through each pixel and extract the float3 values
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color pixel = pixels[index];

                // Extract RGB values as a Vector3
                Vector3 normal = new Vector3(pixel.r, pixel.g, pixel.b);

                // Store in the 2D array
                normalsBuffer[x, y] = normal;
            }
        }

        return normalsBuffer;
    }

    /// <summary>
    /// Unpacks a RenderTexture into a 2D array of Vector3.
    /// </summary>
    /// <param name="sourceTexture">The RenderTexture to unpack.</param>
    /// <returns>A 2D array of Vector3 containing the unpacked data.</returns>
    public static Vector3[,] UnpackTextureTo2DArray(RenderTexture sourceTexture)
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        // Create a temporary Texture2D to read pixel data
        Texture2D tempTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);

        // Copy data from RenderTexture to Texture2D
        RenderTexture.active = sourceTexture;
        tempTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;

        // Use the existing method to unpack the Texture2D
        return UnpackTextureTo2DArray(tempTexture);
    }

    /// <summary> Unpacks a Texture2D into a 1D array. </summary>
    /// <param name="sourceTexture">The Texture2D to unpack.</param>
    /// <returns>A 1D array of Vector3 containing the unpacked data.</returns>
    public static T[] UnpackTextureTo1DArray<T>(Texture2D sourceTexture) where T : struct
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        Color[] pixels = sourceTexture.GetPixels();

        if (typeof(T) == typeof(float))
        {
            float[] resultBuffer = new float[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                resultBuffer[i] = pixel.r;
            }
            return resultBuffer as T[];
        }
        else if (typeof(T) == typeof(Vector2))
        {
            Vector2[] resultBuffer = new Vector2[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                resultBuffer[i] = new Vector2(pixel.r, pixel.g);
            }
            return resultBuffer as T[];
        }
        else if (typeof(T) == typeof(Vector3))
        {
            Vector3[] resultBuffer = new Vector3[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                resultBuffer[i] = new Vector3(pixel.r, pixel.g, pixel.b);
            }
            return resultBuffer as T[];
        }
        else if (typeof(T) == typeof(Vector4))
        {
            Vector4[] resultBuffer = new Vector4[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                resultBuffer[i] = new Vector4(pixel.r, pixel.g, pixel.b, pixel.a);
            }
            return resultBuffer as T[];
        }
        else
        {
            throw new System.ArgumentException("Unsupported type");
        }
    }


    /// <summary> Unpacks a Rendertexture into a 1D array. </summary>
    /// <param name="sourceTexture">The Texture2D to unpack.</param>
    /// <returns>A 1D array of Vector3 containing the unpacked data.</returns>
    public static T[] UnpackTextureTo1DArray<T>(RenderTexture sourceTexture) where T : struct
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        // Create a temporary Texture2D to read pixel data
        Texture2D tempTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);

        // Copy data from RenderTexture to Texture2D
        RenderTexture.active = sourceTexture;
        tempTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;

        // Use the existing method to unpack the Texture2D
        return UnpackTextureTo1DArray<T>(tempTexture);
    }
}
