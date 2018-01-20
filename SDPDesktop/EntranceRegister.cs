namespace SDPDesktop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EntranceRegister
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int UserId { get; set; }
    }
}
