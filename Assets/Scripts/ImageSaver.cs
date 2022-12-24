using UnityEngine;
using System.IO;

public static class ImageSaver
{
    public static void SaveImage(Texture2D image, string filePath)
    {
        // Encode the texture as a PNG image
        byte[] imageData = image.EncodeToPNG();

        // Write the image data to a file
        File.WriteAllBytes(filePath, imageData);
    }
}
