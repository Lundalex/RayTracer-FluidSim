using UnityEngine;

public static class TextureUnpacker
{
    /// <summary>
    /// Unpacks a Texture2D into a 2D array of Vector3.
    /// </summary>
    /// <param name="sourceTexture">The Texture2D to unpack.</param>
    /// <returns>A 2D array of Vector3 containing the unpacked data.</returns>
    public static Vector3[,] UnpackTexture(Texture2D sourceTexture)
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
    public static Vector3[,] UnpackTexture(RenderTexture sourceTexture)
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
        return UnpackTexture(tempTexture);
    }

    /// <summary>
    /// Unpacks a Texture2D into a 1D array of Vector3.
    /// </summary>
    /// <param name="sourceTexture">The Texture2D to unpack.</param>
    /// <returns>A 1D array of Vector3 containing the unpacked data.</returns>
    public static Vector3[] UnpackTextureTo1DArray(Texture2D sourceTexture)
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        // Initialize a 1D array to store the unpacked Vector3 values
        Vector3[] normalsBuffer = new Vector3[width * height];

        // Get all pixel colors from the texture
        Color[] pixels = sourceTexture.GetPixels();

        // Loop through each pixel and extract the float3 values
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            // Extract RGB values as a Vector3
            Vector3 normal = new Vector3(pixel.r, pixel.g, pixel.b);

            // Store in the 1D array
            normalsBuffer[i] = normal;
        }

        return normalsBuffer;
    }

    /// <summary>
    /// Unpacks a RenderTexture into a 1D array of Vector3.
    /// </summary>
    /// <param name="sourceTexture">The RenderTexture to unpack.</param>
    /// <returns>A 1D array of Vector3 containing the unpacked data.</returns>
    public static Vector3[] UnpackTextureTo1DArray(RenderTexture sourceTexture)
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
        return UnpackTextureTo1DArray(tempTexture);
    }
}
