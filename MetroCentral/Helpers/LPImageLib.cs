using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace MetroCentral
{
    class LPImageLib
    {
        public static byte[] GetPhotoThumbnail(string filePath)
        {
            FileStream stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            byte[] photo = reader.ReadBytes((int)stream.Length);
            reader.Close();
            stream.Close();

            byte[] smallerphoto = CreateThumbnail(photo, 55);

            return smallerphoto;
        }

        public static byte[] CreateThumbnail(byte[] PassedImage, int LargestSide)  
        {  
            byte[] ReturnedThumbnail;  
              
            using (MemoryStream StartMemoryStream = new MemoryStream(),  
                                NewMemoryStream = new MemoryStream())  
            {  
                // write the string to the stream  
                StartMemoryStream.Write(PassedImage, 0, PassedImage.Length);  
 
                // create the start Bitmap from the MemoryStream that contains the image  
                Bitmap startBitmap = new Bitmap(StartMemoryStream);  
 
                // set thumbnail height and width proportional to the original image.  
                int newHeight;  
                int newWidth;  
                double HW_ratio;  
                if (startBitmap.Height > startBitmap.Width)  
                {  
                    newHeight = LargestSide;  
                    HW_ratio = (double)((double)LargestSide / (double)startBitmap.Height);  
                    newWidth = (int)(HW_ratio * (double)startBitmap.Width);  
                }  
                else 
                {  
                    newWidth = LargestSide;  
                    HW_ratio = (double)((double)LargestSide / (double)startBitmap.Width);  
                    newHeight = (int)(HW_ratio * (double)startBitmap.Height);  
                }  
 
                // create a new Bitmap with dimensions for the thumbnail.  
                Bitmap newBitmap = new Bitmap(newWidth, newHeight);  
 
                // Copy the image from the START Bitmap into the NEW Bitmap.  
                // This will create a thumnail size of the same image.  
                newBitmap = ResizeImage(startBitmap, newWidth, newHeight);  
                  
                // Save this image to the specified stream in the specified format.  
                newBitmap.Save(NewMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);  
 
                // Fill the byte[] for the thumbnail from the new MemoryStream.  
                ReturnedThumbnail = NewMemoryStream.ToArray();  
            }  
 
            // return the resized image as a string of bytes.  
            return ReturnedThumbnail;  
        }  
 
        // Resize a Bitmap  
        private static Bitmap ResizeImage(Bitmap image, int width, int height)  
        {  
            Bitmap resizedImage = new Bitmap(width, height);  
            using (Graphics gfx = Graphics.FromImage(resizedImage))  
            {  
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),   
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);  
            }
            return resizedImage;
        } 

        public static void ReturnImageThumbnail(string OriginalFile, string NewFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
            System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(OriginalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (OnlyResizeIfWider)
            {
                if (FullsizeImage.Width <= NewWidth)
                {
                    NewWidth = FullsizeImage.Width;
                }
            }

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            // Save resized picture
            NewImage.Save(NewFile);
        }

        public static void ResizeImage(string OriginalFile, string NewFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
            System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(OriginalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (OnlyResizeIfWider)
            {
                if (FullsizeImage.Width <= NewWidth)
                {
                    NewWidth = FullsizeImage.Width;
                }
            }

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            // Save resized picture
            NewImage.Save(NewFile);
        }

    }
}
