using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{
    class FilesWebService : WebService
    {
        private readonly FileSys422 r_sys;

        public FilesWebService(FileSys422 fs)
        {
            r_sys = fs;

        }

        public override void Handler(WebRequest req)
        {
            if (!req.URI.StartsWith(this.ServiceURI))
            {
                throw new InvalidOperationException();
            }

            string[] pieces = req.URI.Substring(ServiceURI.Length).Split('/'); 
            Dir422 dir = r_sys.GetRoot();
            for (int i = 0; i < pieces.Length -1; i++)
            {
                dir = dir.getDir(pieces[i]);
                if (dir == null)
                {
                    req.WriteNotFoundResponse("File not found.\n");
                    return;
                }
            }

            //one piece to process left
            //check if dir is in the last piece we have 
            File422 file = dir.GetFile(pieces[pieces.Length - 1]);
            if (file != null)
            {
                //RespondWithFile(file);
            }
            else
            {
                dir = dir.getDir(pieces[pieces.Length - 1]);
                if (dir != null)
                {
                    RespondWithList(dir, req);
                }
                else
                {
                    req.WriteNotFoundResponse("Not found\n");
                }
            }
        }

        private void RespondWithList(Dir422 dir, WebRequest req)
        {
            var html = new System.Text.StringBuilder("<html>");

            foreach (File422 file in dir.GetFiles())
            {
                html.Append(
                    "<a href= \"{0}\">{1}</a>", //finish this
                );

                // Get HREF for File422 object:
                // Last part: File422Obj.Name
                // Recurse through parent directories until hitting root
                // for each one, append directory name to FRONT of string



                //TODO: build appropriate link based on what file/folder we're in
                //should this filename contain spaces, '#', or something like that
            }

            html.AppendLine("</html>");
            req.WriteHTMLResponse(html.ToString());
        }




        public override string ServiceURI
        {
            get
            {
                return "/files";
            }
        }
    }
}

