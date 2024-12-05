using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BA.BoiseAutoMetric.DataModels
{
    public class CommentCheckModel
    {
        public string blog { get; set; }
        public string user_ip { get; set; }
        public string user_agent { get; set; }
        public string referrer { get; set; }
        public string comment_type { get; set; }
        public string comment_author { get; set; }
        public string comment_author_email { get; set; }
        public string comment_content { get; set; }
        public bool is_test { get; set; }
    }
}
