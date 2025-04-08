using APIBase.Helpers;
using APIBase.Services;
using APIBase.Utils.Encryption;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace APIBase.Utils
{
	public class IOHelper
	{
		#region ReadWriteFiles IO

		#region Create and Read file with Fixed Encryption Key encription
		public static void CreateOrReplaceAppDataFileUsingEncryptionKey(string FolderName, string FileName, string data, EncMaster EncMaster)
		{
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!Directory.Exists(Path.Combine(directory, FolderName)))
			{
				Directory.CreateDirectory(Path.Combine(directory, FolderName));
			}
			var encData = EncryptProvider.AESEncrypt(data, EncMaster.AppSettings.Encryptkey);
			using (StreamWriter outputFile = new StreamWriter(Path.Combine(directory, FolderName, FileName)))
			{
				outputFile.Write(encData);
			}
		}

		public static string ReadAppDataFileUsingEncryptionKey(string FolderName, string FileName, EncMaster EncMaster)
		{
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!File.Exists(Path.Combine(directory, FolderName, FileName)))
			{
				return null;
			}

			try
			{
				using (var sr = new StreamReader(Path.Combine(directory, FolderName, FileName)))
				{
					var encData = sr.ReadToEnd();
					var decData = EncryptProvider.AESDecryptNet6(encData, EncMaster.AppSettings.Encryptkey);
					return decData;
				}
			}
			catch (/*FileNotFoundException*/Exception)
			{
				return null;
			}
		}
		#endregion Create and Read file with Fixed Encryption Key encription

		#region Create and Read file with RSA PEM encription
		public static void CreateOrReplaceAppDataFileRSAPEM(string FolderName, string FileName, string data, EncMaster EncMaster)
		{
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!Directory.Exists(Path.Combine(directory, FolderName)))
			{
				Directory.CreateDirectory(Path.Combine(directory, FolderName));
			}
			var encData = EncryptProvider.RSAEncryptWithPem(EncMaster.RsaKeyPemAPI.PublicKey, data);
			using (StreamWriter outputFile = new StreamWriter(Path.Combine(directory, FolderName, FileName)))
			{
				outputFile.Write(encData);
			}
		}

		public static string ReadAppDataFileRSAPEM(string FolderName, string FileName, EncMaster EncMaster)
		{
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!File.Exists(Path.Combine(directory, FolderName, FileName)))
			{
				return null;
			}

			try
			{
				using (var sr = new StreamReader(Path.Combine(directory, FolderName, FileName)))
				{
					var encData = sr.ReadToEnd();
					var decData = EncryptProvider.RSADecryptWithPem(EncMaster.RsaKeyPemAPI.PrivateKey, encData);
					return decData;
				}
			}
			catch (/*FileNotFoundException*/Exception)
			{
				return null;
			}
		}
		#endregion Create and Read file with RSA PEM encription

		#region Create and Read file with AES encription
		public static void CreateOrReplaceAppDataFileAes(string FolderName, string FileName, string data, EncMaster EncMaster)
		{
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!Directory.Exists(Path.Combine(directory, FolderName)))
			{
				Directory.CreateDirectory(Path.Combine(directory, FolderName));
			}
			var encData = EncryptProvider.AESEncrypt(data, EncMaster.AesKeyPemLocal.Key, EncMaster.AesKeyPemLocal.IV);
			using (StreamWriter outputFile = new StreamWriter(Path.Combine(directory, FolderName, FileName)))
			{
				outputFile.Write(encData);
			}
		}

		public static string ReadAppDataFileAes(string FolderName, string FileName, EncMaster EncMaster)
		{
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			if (!File.Exists(Path.Combine(directory, FolderName, FileName)))
			{
				return null;
			}

			try
			{
				using (var sr = new StreamReader(Path.Combine(directory, FolderName, FileName)))
				{
					var encData = sr.ReadToEnd();
					var decData = EncryptProvider.AESDecrypt(encData, EncMaster.AesKeyPemLocal.Key, EncMaster.AesKeyPemLocal.IV);
					return decData;
				}
			}
			catch (/*FileNotFoundException*/Exception)
			{
				return null;
			}
		}
		#endregion Create and Read file with AES encription
		#endregion WriteFiles IO

		/// <summary>
		/// Resize the image to the specified width and height.
		/// </summary>
		/// <param na me="image">The image to resize.</param>
		/// <param name="width">The width to resize to.</param>
		/// <param name="height">The height to resize to.</param>
		/// <returns>The resized image.</returns>
		public static System.Drawing.Bitmap ResizeImage(string Base64Image, int width, int height)
		{
			var resultImage = new System.Drawing.Bitmap(width, height);
			try
			{
				byte[] bytes = Convert.FromBase64String(Base64Image);
				System.Drawing.Image image;
				using (MemoryStream ms = new MemoryStream(bytes))
				{
					image = System.Drawing.Image.FromStream(ms);
				}

				if (!(image.Width > 100 || image.Height > 100))
				{
					return new System.Drawing.Bitmap(image);
				}

				var destRect = new System.Drawing.Rectangle(0, 0, width, height);

				resultImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
				using (var graphics = System.Drawing.Graphics.FromImage(resultImage))
				{
					graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
					graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
					graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
					using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
					{
						wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
						graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
					}
				}

				return resultImage;
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				//resultImage.Dispose();
			}
		}

		//public static Bitmap Base64StringToBitmap(string base64String)
		//{
		//	Bitmap bmpReturn = null;
		//	//Convert Base64 string to byte[]
		//	byte[] byteBuffer = Convert.FromBase64String(base64String);
		//	MemoryStream memoryStream = new MemoryStream(byteBuffer);

		//	memoryStream.Position = 0;

		//	bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);

		//	memoryStream.Close();
		//	memoryStream = null;
		//	byteBuffer = null;

		//	return bmpReturn;
		//}

		//Base64 to Bitmap :

		//public static Bitmap Base64ToBitmap(String base64String)
		//{
		//	byte[] imageAsBytes = Base64.Decode(base64String, Base64Flags.Default);
		//	return BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
		//}
		//Bitmap to Base64 :

	
		public static System.Drawing.Bitmap Base64ToImage(String base64String)
		{
			//data:image/gif;base64,
			//this image is a single pixel (black)
			byte[] bytes = Convert.FromBase64String(base64String);

			Image image;
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				image = Image.FromStream(ms);
			}

			return new System.Drawing.Bitmap(image);
		}

		public static System.Drawing.Bitmap ResizeImage(Image imgToResize, Size destinationSize)
		{
			var originalWidth = imgToResize.Width;
			var originalHeight = imgToResize.Height;

			//how many units are there to make the original length
			var hRatio = (float)originalHeight / destinationSize.Height;
			var wRatio = (float)originalWidth / destinationSize.Width;

			//get the shorter side
			var ratio = Math.Min(hRatio, wRatio);

			var hScale = Convert.ToInt32(destinationSize.Height * ratio);
			var wScale = Convert.ToInt32(destinationSize.Width * ratio);

			//start cropping from the center
			var startX = (originalWidth - wScale) / 2;
			var startY = (originalHeight - hScale) / 2;

			//crop the image from the specified location and size
			var sourceRectangle = new Rectangle(startX, startY, wScale, hScale);

			//the future size of the image
			var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);

			//fill-in the whole bitmap
			var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

			//generate the new image
			using (var g = Graphics.FromImage(bitmap))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(imgToResize, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
			}

			return bitmap;

		}

	}
}
