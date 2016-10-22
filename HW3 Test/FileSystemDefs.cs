using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CS422
{
    public abstract class Dir422
    {
        public abstract string Name { get; }

        public abstract IList<Dir422> GetDirs();

        public abstract List<File422> GetFiles();

        public abstract Dir422 Parent { get; }

        public abstract bool ContainsFile(string fileName, bool recursive);

        public abstract bool ContainsDir(string dirName, bool recursive);

        public abstract Dir422 getDir(string name);

        public abstract File422 GetFile(string name);

        public abstract File422 CreateFile(string name);

        public abstract Dir422 CreateDir(string name);



    }

    public abstract class File422
    {
        public string Name { get; }

        public Dir422 Parent { get; }

        public abstract Stream OpenReadOnly();

        public abstract Stream OpenReadWrite();
    }

    public abstract class FileSys422
    {
        public abstract Dir422 GetRoot();

        public virtual bool Contains(File422 file)
        {
            return Contains(file.Parent);
        }

        public virtual bool Contains(Dir422 dir)
        {
            if (dir == null) { return false; }
            if (dir == this.GetRoot()) { return true; }
            return Contains(dir.Parent);
        }
    }



    public class StandardFileSystem : FileSys422
    {
        static Dir422 root; //shared between all FS

        public override Dir422 GetRoot()
        {
            return root;
        }

        public static StandardFileSystem Create(string rootDir)
        {
            root = new StdFSDir(rootDir);
            return new StandardFileSystem();
        }
    }

    public class StdFSDir : Dir422
    {
        private string m_path;       
        

        public StdFSDir(string path)
        {
            m_path = path;
        }

        public override string Name
        {
            get
            {
                return Path.GetFileName(m_path);
            }
        }

        public override Dir422 Parent
        {
            get
            {
                
                return new StdFSDir(Directory.GetParent(m_path).FullName);
            }
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            if (dirName.Contains("/") || dirName.Contains("\\"))
            {
                return false;
            }


            foreach (string dir in Directory.GetDirectories(m_path))
            {
                if (Path.GetDirectoryName(dir) == dirName)
                {
                    return true;
                }
            }

            if (recursive == true) //if recursive not true, return false
            {
                foreach (Dir422 child in this.GetDirs()) //if it is true, search children
                {
                    if (child.ContainsDir(dirName, true))
                        return true;
                }
            }

            return false;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            foreach (string file in Directory.GetFiles(m_path))
            {
                if (Path.GetFileName(file) == fileName)
                    return true;
            }
            return false;
        }

        public override Dir422 CreateDir(string name)
        {
            string fullName = m_path+"/"+name;
            if (Directory.CreateDirectory(fullName) != null)
                return new StdFSDir(fullName);
            else
                return null;

        }

        public override File422 CreateFile(string name)
        {
            string fullName = m_path + "/" + name;
            if (File.Create(fullName) != null)
                return new StdFSFile(fullName);
            else
                return null;
        }

        public override Dir422 getDir(string name)
        {
            string fullName = m_path + "/" + name;
            foreach (string dir in Directory.GetDirectories(m_path))
            {
                if (dir == fullName)
                    return new StdFSDir(fullName);
            }

                return null;
        }

        public override IList<Dir422> GetDirs()
        {
            List<Dir422> dirs = new List<Dir422>();
            foreach (string dir in Directory.GetDirectories(m_path))
            {
                dirs.Add(new StdFSDir(dir));
            }
            return dirs;
        }

        public override File422 GetFile(string name)
        {
            string fullName = m_path + "/" + name;
            foreach (string file in Directory.GetFiles(m_path))
            {
                if (file == fullName)
                    return new StdFSFile(fullName);
            }

            return null;
        }

        public override List<File422> GetFiles()
        {
            List<File422> files = new List<File422>();
            foreach (string file in Directory.GetFiles(m_path))
            {
                files.Add(new StdFSFile(file));
            }
            return files;
        }
    }

    public class StdFSFile : File422
    {
        private string m_path;

        public StdFSFile(string path) { m_path = path; }

        public override Stream OpenReadOnly() //one line function return a stream with m_path in it
        {
            return File.Open(m_path, FileMode.Open, FileAccess.Read);
        }

        public override Stream OpenReadWrite() //one line function return a stream with m_path in it
                                               //if you fail to open this stream, return null, don't throw exception
        {
            return File.Open(m_path, FileMode.Open, FileAccess.ReadWrite);
        }
    }


    public class MemoryFileSystem : FileSys422
    {
        MemFSDir root; 

        public override Dir422 GetRoot()
        {
            return root;
        }

        public MemoryFileSystem()
        {
            root = new MemFSDir("/", null);
        }
    }

    public class MemFSDir : Dir422
    {
        private string dirName;
        private MemFSDir dirParent;
        private List<Dir422> directoryChildren;
        private List<File422> fileChildren;
        private string name;

        public MemFSDir(string name, MemFSDir parentD)
        {
            this.name = name;
            dirParent = parentD;
            directoryChildren = new List<Dir422>();
            fileChildren = new List<File422>();
        }

        public override string Name
        {
            get
            {
                return dirName;
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return dirParent;
            }
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            foreach (MemFSDir child in directoryChildren) //check all the files in this folder for a match
            {
                if (child.Name == dirName)
                    return true;
            }
            
            if (recursive == true) //if recursive not true, return false
            {
                foreach (MemFSDir child in directoryChildren) //if it is true, search children
                {
                    if (child.ContainsDir(dirName, true))
                        return true;
                }
            }

            return false;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            foreach (MemFSFile child in fileChildren) //check all the files in this folder for a match
            {
                if (child.Name == fileName)
                    return true;
            }

            if (recursive == true) //if recursive not true, return false
            {
                foreach (MemFSDir child in directoryChildren) //if it is true, search children
                {
                    if (child.ContainsFile(dirName, true))
                        return true;
                }
            }

            return false;
        }

        public override Dir422 CreateDir(string name)
        {
            directoryChildren.Add(new MemFSDir(name, this));
            return directoryChildren[directoryChildren.Count-1];
        }

        public override File422 CreateFile(string name)
        {
            
            fileChildren.Add(new MemFSFile(name, this));
            return fileChildren[fileChildren.Count - 1];
        }

        public override Dir422 getDir(string name)
        {
            for (int x = 0; x < directoryChildren.Count; x++)
            {
                if (directoryChildren[x].Name == name)
                    return directoryChildren[x];
            }

            return null;
        }

        public override IList<Dir422> GetDirs()
        {
            return directoryChildren;
        }

        public override File422 GetFile(string name)
        {
            for (int x = 0; x < fileChildren.Count; x++)
            {
                if (fileChildren[x].Name == name)
                    return fileChildren[x];
            }

            return null;
        }

        public override List<File422> GetFiles()
        {
            return fileChildren;
        }
    }

    public class MemFSFile : File422
    {
        private string fileName;
        MemFSDir parentDir;

        public MemFSFile(string name, MemFSDir parentDir)
        {
            this.fileName = name;
        }

        public string Name
        {
            get
            {
                return fileName;
            }
        }

        public MemFSDir Parent
        {
            get
            {
                return parentDir;
            }
        }



        public override Stream OpenReadOnly() 
        {
            throw new NotImplementedException();
        }

        public override Stream OpenReadWrite()
        {
            throw new NotImplementedException();
        }
    }
}

//overwrite observable stream class