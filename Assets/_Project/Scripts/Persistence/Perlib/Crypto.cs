using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

namespace SardonicMe.Perlib
{
	public static class Crypto
	{
		#region Public Functions
		public static string EncryptString(string text, string pwd)
		{
			byte[] originalBytes = Encoding.UTF8.GetBytes(text);
			byte[] encryptedBytes = null;
			byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);

			// Hash the password with SHA256
			passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

			// Generating salt bytes
			byte[] saltBytes = new byte[4];
			RNGCryptoServiceProvider.Create().GetBytes(saltBytes);

			// Appending salt bytes to original bytes
			byte[] bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
			for (int i = 0; i < saltBytes.Length; i++)
				bytesToBeEncrypted[i] = saltBytes[i];

			for (int i = 0; i < originalBytes.Length; i++)
				bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];

			encryptedBytes = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

			return Convert.ToBase64String(encryptedBytes);
		}

		public static string DecryptString(string decryptedText, string pwd)
		{
			byte[] bytesToBeDecrypted = Convert.FromBase64String(decryptedText);
			byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);

			// Hash the password with SHA256
			passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

			byte[] decryptedBytes = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

			// Getting the size of salt
			int _saltSize = 4;

			// Removing salt bytes, retrieving original bytes
			byte[] originalBytes = new byte[decryptedBytes.Length - _saltSize];
			for (int i = _saltSize; i < decryptedBytes.Length; i++)
				originalBytes[i - _saltSize] = decryptedBytes[i];

			return Encoding.UTF8.GetString(originalBytes);
		}
		#endregion

		#region Private Functions
		static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
		{
			byte[] encryptedBytes = null;
			byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

			using (MemoryStream ms = new MemoryStream())
			{
				using (RijndaelManaged AES = new RijndaelManaged())
				{
					AES.KeySize = 256;
					AES.BlockSize = 128;

					var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
					AES.Key = key.GetBytes(AES.KeySize / 8);
					AES.IV = key.GetBytes(AES.BlockSize / 8);

					AES.Mode = CipherMode.CBC;

					using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
						cs.Close();
					}
					encryptedBytes = ms.ToArray();
				}
			}

			return encryptedBytes;
		}

		static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
		{
			byte[] decryptedBytes = null;
			byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

			using (MemoryStream ms = new MemoryStream())
			{
				using (RijndaelManaged AES = new RijndaelManaged())
				{
					AES.KeySize = 256;
					AES.BlockSize = 128;

					var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
					AES.Key = key.GetBytes(AES.KeySize / 8);
					AES.IV = key.GetBytes(AES.BlockSize / 8);

					AES.Mode = CipherMode.CBC;

					using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
						cs.Close();
					}
					decryptedBytes = ms.ToArray();
				}
			}

			return decryptedBytes;
		}
		#endregion
	}
}
