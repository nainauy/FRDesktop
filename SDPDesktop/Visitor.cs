namespace SDPDesktop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Visitor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string CardId { get; set; }

        public int? Gender_Id { get; set; }

        public virtual Gender Gender { get; set; }
    }
}
