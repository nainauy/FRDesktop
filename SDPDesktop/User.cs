namespace SDPDesktop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public int PersonalNumber { get; set; }

        public byte[] Avatar { get; set; }

        public int? Gender_Id { get; set; }

        public int? Sector_Id { get; set; }

        public virtual Gender Gender { get; set; }

        public virtual Sector Sector { get; set; }
    }
}
