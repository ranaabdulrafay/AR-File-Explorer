using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace AR.Explorer
{
    public class GetDirectoryTest : MonoBehaviour
    {
        [System.Serializable]
        public struct FileSystemEntry
        {
            public readonly string Path;
            public readonly string Name;
            public readonly string Extension;
            public readonly FileAttributes Attributes;

            public bool IsDirectory { get { return (Attributes & FileAttributes.Directory) == FileAttributes.Directory; } }

            public FileSystemEntry(string path, string name, bool isDirectory)
            {
                Path = path;
                Name = name;
                Extension = isDirectory ? null : System.IO.Path.GetExtension(name);
                Attributes = isDirectory ? FileAttributes.Directory : FileAttributes.Normal;
            }

            public FileSystemEntry(FileSystemInfo fileInfo)
            {
                Path = fileInfo.FullName;
                Name = fileInfo.Name;
                Extension = fileInfo.Extension;
                Attributes = fileInfo.Attributes;
            }
        }

        public FileSystemEntry[] entries;
        public Transform Content;
        public GameObject ItemPrefab;
        public List<Item> ItemsInstances = new List<Item>();
        public List<Item> ItemsPool = new List<Item>();

        public Sprite FolderIcon;
        public Sprite FileIcon;
        public Sprite DriveIcon;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public FileSystemEntry[] GetDirectories(string path)
        {
            try
            {
                //DirectoryInfo directoryInfo = new DirectoryInfo(path);
                //directoryInfo.Parent.get
                FileSystemInfo[] items = new DirectoryInfo(path).GetFileSystemInfos();
                FileSystemEntry[] result = new FileSystemEntry[items.Length];
                int index = 0;

                for (int i = 0; i < items.Length; i++)
                {
                    try
                    {
                        if ((items[i].Extension == ".txt" || items[i].Extension == ".json")
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
#if PLATFORM_STANDALONE || UNITY_EDITOR
            string path = System.Environment.CurrentDirectory;
#else 
            string path = Application.persistentDataPath;
#endif
            LoadPath(path);
        }
        public void LoadPath(string path)
        {
            if (Directory.Exists(path))
            {
                ClearItemInstances();

                entries = GetDirectories(path);
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

                foreach (var entry in entries)
                {
                    InstantiateItem(entry);
                }
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }
        public void InstantiateItem(FileSystemEntry entry, bool isFirstSibling = false, bool isDrive = false)
        {
            Item Itm;
            if (ItemsPool.Count == 0)
            {
                GameObject obj = Instantiate(ItemPrefab, Content);
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
            if (entry.IsDirectory)
                Itm.OnClickBtn
                    .onClick.AddListener(() => LoadPath(_path));
            else
                Itm.OnClickBtn
                .onClick.AddListener(() => LoadFilePath(_path));
            Itm.Name
                .text = entry.IsDirectory ? entry.Name
                : string.Compare(entry.Name, entry.Extension) != 0 ? entry.Name
                : entry.Name + entry.Extension;
            Itm.Icon
                .sprite = isDrive ? DriveIcon : entry.IsDirectory ? FolderIcon : FileIcon;
        }
        public void LoadFilePath(string path)
        {
            if (File.Exists(path))
                Debug.Log("its a txt or json File");
        }
        void ClearItemInstances()
        {
            for (int i = 0; i < ItemsInstances.Count; i++)
            {
                ItemsInstances[i].gameObject.SetActive(false);
                ItemsPool.Add(ItemsInstances[i]);
                ItemsInstances[i].OnClickBtn.onClick.RemoveAllListeners();
            }
            ItemsInstances.Clear();
        }
    }
}

