using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _301428777_GuertaLigo__Lab_2
{

    //book stored in dynamoDB
    public class BookItem
    {
        public string Email { get; set; }        
        public string Isbn { get; set; }          
        public string Title { get; set; }        
        public string Author { get; set; }       
        public int BookmarkPage { get; set; }    
        public string BookmarkTime { get; set; }  
        public string S3Key { get; set; }
    }
}
