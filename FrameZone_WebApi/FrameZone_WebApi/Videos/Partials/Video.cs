using FrameZone_WebApi.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameZone_WebApi.Models
{   
    partial class Channel
    {
        [ForeignKey("UserId")]
        public UserProfile UserProfile { get; set; }
    }
}
