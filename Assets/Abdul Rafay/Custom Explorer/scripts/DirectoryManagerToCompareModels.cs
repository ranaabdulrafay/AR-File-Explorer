using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AR.Explorer
{
    public class DirectoryManagerToCompareModels : MonoBehaviour
    {
        #region Singleton
        private static DirectoryManagerToCompareModels instance;
        public static DirectoryManagerToCompareModels Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<DirectoryManagerToCompareModels>();
                }
                return instance;
            }
        }
        #endregion

        //========= Common =============
        string currentDirectoryPath;
        public string CurrentDirectoryPath
        {
            get
            {
                return currentDirectoryPath;
            }
        }

        //========= Directorys =============
        FileSystemEntry[] Directoryentries;
        public Transform ContentDirectory;
        public GameObject ItemPrefab;
        #region ItemsPool
        public List<Item> ItemsInstances = new List<Item>();
        public List<Item> ItemsPool = new List<Item>();
        void ClearItemInstances()
        {
            for (int i = 0; i < ItemsInstances.Count; i++)
            {
                ItemsInstances[i].gameObject.SetActive(false);
                ItemsPool.Add(ItemsInstances[i]);

                ItemsInstances[i].Reset();
            }
            ItemsInstances.Clear();
        }
        #endregion

        public Sprite FolderIcon;
        public Sprite FileIcon; // dont need here
        public Sprite DriveIcon;


        ////========= models List Related =============
        public List<FileSystemEntry> ModelEntries = new List<FileSystemEntry>();
        public List<FileSystemEntry> LoadedModelEntries = new List<FileSystemEntry>();

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
        #region View Directory
        public FileSystemEntry[] GetDirectories(string path)
        {
            try
            {
                FileSystemInfo[] items = new DirectoryInfo(path).GetFileSystemInfos();
                FileSystemEntry[] result = new FileSystemEntry[items.Length];
                int index = 0;

                for (int i = 0; i < items.Length; i++)
                {
                    try
                    {
                        if ((items[i].Extension == ".fbx" || items[i].Extension == ".obj")
                            || ((items[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory &&
                            !((items[i].Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)))
                        {
                            result[index] = new FileSystemEntry(items[i]);
                            index++;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                if (result.Length != index)
                    System.Array.Resize(ref result, index);
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        public void LoadAppPersDatPth()
        {
            string path = System.Environment.CurrentDirectory;

            LoadPath(path);
        }
        public void LoadPath(string path)
        {
            if (Directory.Exists(path))
            {
                currentDirectoryPath = path;

                ClearItemInstances();

                Directoryentries = GetDirectories(path);
                DirectoryInfo directoryInfo = Directory.GetParent(path);
                if (directoryInfo != null)
                {
                    FileSystemEntry PreviousEntry = new FileSystemEntry(directoryInfo.FullName, "Up", true);
                    InstantiateItem(PreviousEntry, true);
                }
                else
                {
                    directoryInfo = new DirectoryInfo(path);

                    //DriveInfo directoryInfo.FullName
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    for (int i = 0; i < allDrives.Length; i++)
                    {
                        if (allDrives[i].RootDirectory.FullName != path && allDrives[i].IsReady)
                        {
                            FileSystemEntry otherDrive = new FileSystemEntry(allDrives[i].RootDirectory.FullName, allDrives[i].RootDirectory.FullName, true);
                            InstantiateItem(otherDrive, true, true);
                        }
                    }
                }
                LoadedModelEntries.Clear();
                ModelEntries.Clear();
                foreach (var entry in Directoryentries)
                {
                    InstantiateItem(entry);
                    if (!entry.IsDirectory)
                        ModelEntries.Add(entry);
                }
            }
            else
            {
                currentDirectoryPath = null;

                throw new DirectoryNotFoundException();
            }
        }
        public void InstantiateItem(FileSystemEntry entry, bool isFirstSibling = false, bool isDrive = false)
        {
            Item Itm;
            if (ItemsPool.Count == 0)
            {
                GameObject obj = Instantiate(ItemPrefab, ContentDirectory);
                Itm = obj.GetComponent<Item>();
                ItemsInstances.Add(Itm);
            }
            else
            {
                Itm = ItemsPool[0];
                ItemsInstances.Add(Itm);
                ItemsPool.Remove(Itm);
            }
            if (isFirstSibling)
            {
                Itm.transform.SetAsFirstSibling();
            }
            Itm.gameObject.SetActive(true);

            string _path = entry.Path;
            Itm.Path = _path;

            if (entry.IsDirectory)
                Itm.OnClickBtn.onClick.AddListener(() => LoadPath(_path));

            Itm.Name
                .text = entry.IsDirectory ? entry.Name
                : string.Compare(entry.Name, entry.Extension) != 0 ? entry.Name
                : entry.Name + entry.Extension;
            Itm.Icon
                .sprite = isDrive ? DriveIcon : entry.IsDirectory ? FolderIcon : FileIcon;
        }
        #endregion

        #region Models
        public void ModelEntryLoaded(FileSystemEntry Modelentry)
        {
            if (!LoadedModelEntries.Contains(Modelentry))
                LoadedModelEntries.Add(Modelentry);
            if (ModelEntries.Contains(Modelentry))
                ModelEntries.Remove(Modelentry);
        }
        public void ModelEntryUnLoaded(FileSystemEntry Modelentry)
        {
            if (LoadedModelEntries.Contains(Modelentry))
                LoadedModelEntries.Remove(Modelentry);
            if (!ModelEntries.Contains(Modelentry))
                ModelEntries.Add(Modelentry);

        }
        #endregion
    }
}
