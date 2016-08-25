using massive_moose.services.models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace massive_moose.web.owin.Models
{
    public class WallsViewModel
    {
        public IList<Wall> Walls { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Label")]
        public string NewWallLabel { get; set; }
    }
}