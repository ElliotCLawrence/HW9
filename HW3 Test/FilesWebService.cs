using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;


namespace CS422
{
    class FilesWebService : WebService
    {
        private readonly FileSys422 r_sys;

        public FilesWebService(FileSys422 fs)
        {
            r_sys = fs;
        }

        public override string ServiceURI
        {
            get
            {
                return "/files";
            }
        }

        public override void Handler(WebRequest req)
        {
            if (!req.URI.StartsWith(this.ServiceURI))
            {
                throw new InvalidOperationException();
            }

            string[] pieces = req.URI.Substring(ServiceURI.Length).Split('/');  //split up the path by '/' tokens
            Dir422 dir = r_sys.GetRoot(); //grab the root of the filesystem
            for (int i = 0; i < pieces.Length -1; i++) //go through the parts of the path
            {
                dir = dir.getDir(pieces[i]);
                if (dir == null) //if you encounter a directory that doesn't exist, tell the user that the target they requested is not found and return
                {
                    req.WriteNotFoundResponse("File not found.\n");
                    return;
                }
            }

            //one piece to process left
            //check if dir is in the last piece we have 
            File422 file = dir.GetFile(pieces[pieces.Length - 1]); //grab the last file of the path
            if (file != null)
            {
                RespondWithFile(file, req);
            }
            else
            {
                dir = dir.getDir(pieces[pieces.Length - 1]); //if it wasn't a file, grab it as a dir
                if (dir != null)
                {
                    RespondWithList(dir, req);
                }
                else //if it's null, tell the user it was not found
                {
                    req.WriteNotFoundResponse("Not found\n");
                }
            }
        }

        String BuildDirHTML(Dir422 directory) 
        {

            var html = new StringBuilder("<html>");
            html.AppendLine("<h1>Folders</h1>"); //label the beginning of folders
            foreach (Dir422 dir in directory.GetDirs())
            {
                html.AppendLine(
                    String.Format("<a href=\"{0}\">{1}</a>", ServiceURI + GetHREFFromDir422(dir), dir.Name) //FIX THIS, first one should be full path
                    );
                html.AppendLine("</br>");
            }

            html.AppendLine("<h1>Files</h1>"); //label the beginning of files

            foreach (File422 file in directory.GetFiles())
            {
                html.AppendLine(
                    String.Format("<a href=\"{0}\">{1}</a>", ServiceURI + GetHREFFromFile422(file), file.Name) //FIX THIS, first one should be full path
                );
                html.AppendLine("</br>");
            }

            //TODO: build appropriate link based on what file/folder we're in
            //should this filename contain spaces, '#', or something like that encode them so they're correct

            html.AppendLine("</html>");
            return html.ToString();
        }

        private void RespondWithList(Dir422 dir, WebRequest req)
        {
            req.WriteHTMLResponse(BuildDirHTML(dir).ToString());
        }

        private void RespondWithFile(File422 file, WebRequest req) //return a file
        {
            var html = new StringBuilder("<html>"); //start a string builder and start filling with HTML
            byte[] buffer = new byte[1024]; //create a buffer for reading from a file


            FileStream fs = (FileStream) file.OpenReadOnly(); //open this file
            html.AppendLine(); //add a line after the open tag
            
            while (fs.Read(buffer,0,1024) > 0) //read in the file and add it to HTML
            {
                html.Append(Encoding.Default.GetString(buffer));
            }
            html.AppendLine("</html>");
            req.WriteHTMLResponse(html.ToString()); //write a page as a file
        }

        string GetHREFFromFile422(File422 file) //get filepath from file
        {
            string path = file.Name; //create a filepath to return

            Dir422 temp = file.Parent; //create a temporary directory as this files parent

            while (temp != null) //while not past the root
            {
                path = temp.Name + "/" + path; //prepend the parents name to the filepath with a '/'
                temp = temp.Parent; //move up the parent path
            }

            path = "/" + path; //start the path with a '/'
            return path;
        }

        string GetHREFFromDir422(Dir422 dir) //get filepath from directory
        {
            string path = ""; //path string

            while (dir != null) //while not past the root
            {
                path = dir.Name + "/" + path; //prepend the parents name to the filepath with a '/'
                dir = dir.Parent; //move up the parent path
            }

            path = "/" + path; //start the path with a '/'
            return HttpUtility.HtmlEncode(path);


        }
    }
}

