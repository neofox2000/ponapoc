using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace SardonicMe.Perlib
{
    /// <summary>
    /// File-based, generic, and encrypted alternative to PlayerPrefs
    /// </summary>
    public class Perlib
    {
        #region Public Vars
        public const string Version = "1.0.3";

        /// <summary>
        /// Represents the current path and name of the Perlib.
        /// On Web platforms this merely represents a name, not a path.
        /// </summary>
        public FileInfo FileInfo { get; set; }

        /// <summary>
        /// Password used for file encryption. Encryption will not be used if empty or null.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Whether a Perlib exists at the location pointed to by FileInfo.
        /// </summary>
        public bool Exists
        {
            get
            {
#if UNITY_WEBPLAYER || UNITY_WEBGL
                return PlayerPrefs.HasKey(FileInfo.FullName);
#else
				return FileInfo.Exists;
#endif
            }
        }
        #endregion

        #region Private Vars
        /// <summary>
        /// Internal library of keys and values.
        /// </summary>
        SerializableDictionary<string, string> library;

        /// <summary>
        /// Serializer used for saving the library to file.
        /// </summary>
        XmlSerializer fileSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
        #endregion

        #region Public Functions
        /// <summary>
        /// Creates new Perlib.
        /// </summary>
        /// <param name="fileName">The name under which this Perlib will be created. Perlib will set the path to Application.persistentDataPath.</param>
        /// <param name="password">Password used for encryption and decryption.</param>
        public Perlib(string fileName, string password = null) : this(MakeDefaultFileInfo(fileName), password)
        {
        }

        /// <summary>
        /// Creates new Perlib.
        /// </summary>
        /// <param name="fileInfo">The FileInfo which Perlib will use to create its file. Use this to specify custom paths.</param>
        /// <param name="password">Password used for encryption and decryption.</param>
        public Perlib(FileInfo fileInfo, string password = null)
        {
            FileInfo = fileInfo;
            Password = password;
        }

        /// <summary>
        /// Deletes the Perlib if it exists.
        /// </summary>
        public void Delete()
        {
#if UNITY_WEBPLAYER || UNITY_WEBGL
            PlayerPrefs.DeleteKey(FileInfo.FullName);
#else
			FileInfo.Delete();
#endif
        }

        /// <summary>
        /// Opens the Perlib. If no file was found under the specified name, a new one will be created by the Perlib.
        /// Call this before any operations.
        /// </summary>
        public void Open()
        {
            if (Exists)
            {
                string toRead = null;
#if UNITY_WEBPLAYER || UNITY_WEBGL
                toRead = PlayerPrefs.GetString(FileInfo.FullName);
#else
				using (StreamReader fileReader = new StreamReader(FileInfo.FullName, Encoding.Unicode))
					toRead = fileReader.ReadToEnd();
#endif
                if (!string.IsNullOrEmpty(Password))
                    toRead = Crypto.DecryptString(toRead, Password);

                using (StringReader sr = new StringReader(toRead))
                    library = fileSerializer.Deserialize(sr) as SerializableDictionary<string, string>;
            }
            else
            {
                library = new SerializableDictionary<string, string>();
                Save();
            }
        }

        /// <summary>
        /// Saves all changes made to the Perlib.
        /// </summary>
        public void Save()
        {
            using (StringWriter textWriter = new StringWriter())
            {
                fileSerializer.Serialize(textWriter, library);

                string toWrite = textWriter.ToString();
                if (!string.IsNullOrEmpty(Password))
                    toWrite = Crypto.EncryptString(toWrite, Password);

#if UNITY_WEBPLAYER || UNITY_WEBGL
                PlayerPrefs.SetString(FileInfo.FullName, toWrite);
#else
                Directory.CreateDirectory(FileInfo.DirectoryName);
				using (StreamWriter sw = new StreamWriter(FileInfo.FullName, false, Encoding.Unicode))
					sw.Write(toWrite);
#endif
            }
        }

        /// <summary>
        /// Whether the Perlib contains a certain Key-Value par.
        /// </summary>
        public bool HasKey(string key)
        {
            return library.ContainsKey(key);
        }

        /// <summary>
        /// Will set a value.
        /// </summary>
        /// <param name="password">Password used to encrypt the value.</param>
        public void SetValue<T>(string key, T value, string password = null)
        {
            using (StringWriter textWriter = new StringWriter())
            {
                XmlSerializer valueSerializer = new XmlSerializer(typeof(T));
                valueSerializer.Serialize(textWriter, value);
                string toWrite = textWriter.ToString();

                if (!string.IsNullOrEmpty(password))
                    toWrite = Crypto.EncryptString(toWrite, password);

                library[key] = toWrite;
            }
        }

        /// <summary>
        /// Will return a value.
        /// </summary>
        /// <typeparam name="T">Type of the value you want</typeparam>
        /// <param name="defaultValue">If the Key is not found, this default value will be returned.</param>
        /// <param name="password">Password used to decrypt the value.</param>
        /// <returns>Value represented by the Key</returns>
        public T GetValue<T>(string key, T defaultValue = default(T), string password = null)
        {
            if (HasKey(key))
            {
                string toRead = library[key];

                if (!string.IsNullOrEmpty(password))
                    toRead = Crypto.DecryptString(toRead, password);

                using (StringReader sr = new StringReader(toRead))
                {
                    XmlSerializer valueSerializer = new XmlSerializer(typeof(T));
                    return (T)valueSerializer.Deserialize(sr);
                }
            }
            else
                return defaultValue;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Turns an incoming name into a usable FileInfo
        /// </summary>
        static FileInfo MakeDefaultFileInfo(string name)
        {
            return new FileInfo(Application.persistentDataPath + Path.DirectorySeparatorChar + name);
        }
        #endregion
    }
}